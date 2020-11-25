using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vc.Common;
using Vc.Domain.Entities;
using Vc.Domain.Exceptions;
using Vc.Domain.RepositoryInterfaces;
using Proto = VcGrpcService.Proto;

namespace VcGrpcService.AppServices
{
    public class ChatAppService : AbstractAppService
    {
        private ConcurrentDictionary<string, IServerStreamWriter<Proto.JoinResponse>> onlineUsers = new ConcurrentDictionary<string, IServerStreamWriter<Proto.JoinResponse>>();
        private readonly ILogger<ChatAppService> _logger;
        private readonly IRoomRepository _roomRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public ChatAppService(ILogger<ChatAppService> logger, IRoomRepository roomRepository, IMessageRepository messageRepository, IUserRepository userRepository)
        {
            _logger = logger;
            _roomRepository = roomRepository;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        public void AddOnlineUser(string userId, IServerStreamWriter<Proto.JoinResponse> responseStream)
        {
            _logger.LogDebug("Adding user to online users. userId : {0}", userId);
            //remove first if already existing to refresh
            onlineUsers.TryRemove(userId, out _);
            onlineUsers.TryAdd(userId, responseStream);
        }

        public void RemoveOnlineUser(string userId)
        {
            _logger.LogDebug("Removing user from online users. userId : {0}", userId);
            onlineUsers.TryRemove(userId, out IServerStreamWriter<Proto.JoinResponse> stream);
        }

        public async Task BroadcastMessage(string senderId, Proto.Message message)
        {
            _logger.LogDebug("Broadcasting message from {0}", senderId);
            //get and validate sender early
            User sender = await _userRepository.GetUserAsync(senderId);
            if (sender.IsNull())
            {
                _logger.LogError("Sender with id : {0} not found. throwing exeption.", senderId);
                throw new UserNotFoundExeption("Sender not found");
            }

            Room room;
            //check if private message
            if (message.Type == Proto.RoomType.Private && string.IsNullOrEmpty(message.RoomId))
            {
                //get and validate receiver 
                User receiver = await _userRepository.GetUserAsync(message?.Target);
                if (receiver.IsNull())
                {
                    _logger.LogError("Receiver with id : {0} not found. throwing exeption.", message?.Target);
                    throw new UserNotFoundExeption("Receiver not found");
                }

                //get private room
                room = await _roomRepository.GetPrivateRoomAsync(senderId, message?.Target);

                if (room == null)
                {
                    _logger.LogInformation("Creating private room for {0} and {1}", senderId, message?.Target);

                    room = createRoom(senderId, message?.Target, RoomType.Private);
                    room.RoomUsers.Add(new RoomUser() { UserId = sender?.Id, Nickname = sender?.Username });
                    room.RoomUsers.Add(new RoomUser() { UserId = receiver?.Id, Nickname = receiver?.Username });

                    room.Id = await _roomRepository.AddRoomAsync(room);
                }

            }
            else
            {
                room = await _roomRepository.GetRoomAsync(message.RoomId);
            }

            if (room.IsNull())
            {
                _logger.LogError("Room with id : {0} not found.", message?.Target);
            }
            else
            {
                await _messageRepository.AddMessageAsync(createMessage(message, room, sender));

                foreach (var user in room.RoomUsers)
                {
                    if (onlineUsers.TryGetValue(user.UserId, out IServerStreamWriter<Proto.JoinResponse> stream))
                    {
                        try
                        {
                            await stream.WriteAsync(createNotification(senderId, message));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Stream write error removing user from online users" );
                            onlineUsers.TryRemove(user.UserId, out _);
                        }
                        
                    }
                }
            }
        }

        public async Task<Proto.GetRoomsResponse> SendUserRoomsAsync(string userId)
        {
            var rooms = await _roomRepository.GetUserRoomsAsync(userId);
            var roomList = new Proto.GetRoomsResponse();
            foreach (var room in rooms)
            {
                roomList.Rooms.Add(createRoomReply(room));
            }
            return roomList;
        }

        private Proto.Room createRoomReply(Room room)
        {
            long unixTimestamp = room.LastMessageDatetime.IsValid() ? ((DateTimeOffset)room.LastMessageDatetime).ToUnixTimeSeconds() : 0;
            return new Proto.Room() { Id = room.Id, Name = room.Name, Type = covertRoomType(room.Type),  LastMessage = room.LastMessage ?? string.Empty, LastMessageDatetime = unixTimestamp };
        }

        private Proto.RoomType covertRoomType(RoomType roomType)
        {
            switch (roomType)
            {
                case RoomType.Private:
                    return Proto.RoomType.Private;
                case RoomType.Group:
                    return Proto.RoomType.Group;
                default:
                    return Proto.RoomType.Unknown;
            }
        }

        public Room createRoom(string senderId, string receiverId, RoomType roomType)
        {
            string name = string.Format("{0}-{1}", senderId, receiverId);
            return new Room() { Name = name, Type = roomType };
        }

        private Proto.JoinResponse createNotification(string senderId, Proto.Message messageRequest)
        {
            Proto.Notification notification = new Proto.Notification() { RoomId = messageRequest.Target, Sender = senderId, MessageBody = messageRequest.MessageBody };
            return new Proto.JoinResponse() { Notification = notification };
        }

        private Message createMessage(Proto.Message messageRequest, Room room, User sender)
        {
            return new Message() { DateSent = DateTime.Now, MessageBody = messageRequest.MessageBody, Room = room, Sender = sender };
        }


    }
}
