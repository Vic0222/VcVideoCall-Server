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

namespace VcGrpcService.AppServices
{
    public class ChatAppService : AbstractAppService
    {
        private ConcurrentDictionary<string, IServerStreamWriter<Notification>> onlineUsers = new ConcurrentDictionary<string, IServerStreamWriter<Notification>>();
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

        public void AddOnlineUser(string userId, IServerStreamWriter<Notification> responseStream)
        {
            onlineUsers.TryAdd(userId, responseStream);
        }

        public void RemoveOnlineUser(string userId)
        {
            onlineUsers.TryRemove(userId, out IServerStreamWriter<Notification> stream);
        }

        public async Task BroadcastMessage(string senderId, MessageRequest messageRequest)
        {
            //get and validate sender early
            User sender = await _userRepository.GetUserAsync(senderId);
            if (sender.IsNull())
            {
                _logger.LogError("Sender with id : {0} not found. throwing exeption.", senderId);
                throw new UserNotFoundExeption("Sender not found");
            }

            Room room;

            if (messageRequest.Type == RoomTypeReply.Private && messageRequest.RoomId.IsNull())
            {
                //get and validate receiver 
                User receiver = await _userRepository.GetUserAsync(messageRequest?.Target);
                if (receiver.IsNull())
                {
                    _logger.LogError("Receiver with id : {0} not found. throwing exeption.", messageRequest?.Target);
                    throw new UserNotFoundExeption("Receiver not found");
                }

                room = createRoom(senderId, messageRequest?.Target, RoomType.Private);
                room.RoomUsers.Add(new RoomUser() { UserId = sender?.Id, Nickname = sender?.Username });
                room.RoomUsers.Add(new RoomUser() { UserId = receiver?.Id, Nickname = receiver?.Username });

                room.Id = await _roomRepository.AddRoomAsync(room);

            }
            else
            {
                room = await _roomRepository.GetRoomAsync(messageRequest.RoomId);
            }

            if (room.IsNull())
            {
                _logger.LogError("Room with id : {0} not found.", messageRequest?.Target);
            }
            else
            {
                await _messageRepository.AddMessageAsync(createMessage(messageRequest, room, sender));

                foreach (var user in room.RoomUsers)
                {
                    if (onlineUsers.TryGetValue(user.UserId, out IServerStreamWriter<Notification> stream))
                    {
                        await stream.WriteAsync(createNotification(senderId, messageRequest));
                    }
                }
            }
        }

        public async Task SendUserRoomsAsync(string userId, IServerStreamWriter<RoomReply> responseStream)
        {
            var rooms = await _roomRepository.GetUserRoomsAsync(userId);    
            foreach (var room in rooms)
            {
                await responseStream.WriteAsync(createRoomReply(room));
            }
        }

        private RoomReply createRoomReply(Room room)
        {
            return new RoomReply() { Id = room.Id, Name = room.Name, LastMessage = room.LastMessage, Type = covertRoomType(room.Type) };
        }

        private RoomTypeReply covertRoomType(RoomType roomType)
        {
            switch (roomType)
            {
                case RoomType.Private:
                    return RoomTypeReply.Private;
                case RoomType.Group:
                    return RoomTypeReply.Group;
                default:
                    return RoomTypeReply.Unknown;
            }
        }

        public Room createRoom(string senderId, string receiverId, RoomType roomType)
        {
            string name = string.Format("{0}-{1}", senderId, receiverId);
            return new Room() { Name = name, Type = roomType };
        }

        private Notification createNotification(string senderId, MessageRequest messageRequest)
        {
            return new Notification() { RoomId = messageRequest.Target, Sender = senderId, MessageBody = messageRequest.MessageBody };
        }

        private Message createMessage(MessageRequest messageRequest, Room room, User sender)
        {
            return new Message() { DateSent = DateTime.Now, MessageBody = messageRequest.MessageBody, Room = room, Sender = sender };
        }


    }
}
