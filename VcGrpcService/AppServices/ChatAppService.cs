using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vc.Common;
using Vc.Domain.Entities;
using Vc.Domain.Exceptions;
using Vc.Domain.RepositoryInterfaces;
using VcGrpcService.CallHelpers;
using Proto = VcGrpcService.Proto;

namespace VcGrpcService.AppServices
{
    public class ChatAppService : AbstractAppService
    {
        private ConcurrentDictionary<string, IServerStreamWriter<Proto.JoinResponse>> _onlineUsers = new ConcurrentDictionary<string, IServerStreamWriter<Proto.JoinResponse>>();
        private ConcurrentDictionary<string, CallInfo> _onGoingCallOffer = new ConcurrentDictionary<string, CallInfo>();
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
            _onlineUsers.TryRemove(userId, out _);
            _onlineUsers.TryAdd(userId, responseStream);
        }

        public void RemoveOnlineUser(string userId)
        {
            _logger.LogDebug("Removing user from online users. userId : {0}", userId);
            _onlineUsers.TryRemove(userId, out IServerStreamWriter<Proto.JoinResponse> stream);
        }

        public async Task BroadcastMessage(string senderId, Proto.MessageRequest message)
        {
            _logger.LogDebug("Broadcasting message from {0}", senderId);
            //get and validate sender early
            User sender = await _userRepository.GetUserAsync(senderId);
            if (sender.IsNull())
            {
                _logger.LogError("Sender with id : {0} not found. throwing exeption.", senderId);
                throw new UserNotFoundExeption("Sender not found");
            }

            Room room = await _roomRepository.GetRoomAsync(message.RoomId);

            if (room.IsNull())
            {
                _logger.LogError("Room with id : {0} not found.", message?.RoomId);
            }
            else
            {
                await _messageRepository.AddMessageAsync(createMessage(message, room, sender));

                foreach (var user in room.RoomUsers)
                {
                    if (_onlineUsers.TryGetValue(user.UserId, out IServerStreamWriter<Proto.JoinResponse> stream))
                    {
                        try
                        {
                            await stream.WriteAsync(createJoinResponseForMessage(senderId, message));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Stream write error removing user from online users" );
                            _onlineUsers.TryRemove(user.UserId, out _);
                        }
                        
                    }
                }
            }
        }

        public async Task<Proto.GetMessagesResponse> GetMessagesByRoomIdOfUser(string roomId, long lastMessageTimestamp)
        {
            DateTime? lastMessageDatetime = null;
            if (lastMessageTimestamp > 0)
            {
                lastMessageDatetime = DateTimeOffset.FromUnixTimeSeconds(lastMessageTimestamp).DateTime;
            }

            List<Message> messages = await _messageRepository.GetMessagesByRoomIdOfUserAsync(roomId, lastMessageDatetime);

            var response = new Proto.GetMessagesResponse();
            if (messages.Count > 0)
            {
                long lastDateSentTimestamp = ((DateTimeOffset)messages.FirstOrDefault()?.DateSent).ToUnixTimeSeconds();
                response.LastMessageDatetime = lastDateSentTimestamp;
                foreach (var message in messages)
                {
                    long dateSentTimestamp = ((DateTimeOffset)message.DateSent).ToUnixTimeSeconds();
                    response.Messages.Add(new Proto.Message() { Id = message.Id, RoomId = message.RoomId, SenderId = message.SenderId, MessageBody = message.MessageBody, DateSent = dateSentTimestamp });
                }
            }
            
            return response;

        }

        public async Task<Proto.CallAnswerResponse> ReceiveCallAnserAsync(Proto.CallOfferStatus status, string receiverId, string roomId, Proto.RtcSessionDescription rtcSessionDescription)
        {
            _onGoingCallOffer.TryGetValue(roomId, out CallInfo callInfo);
            callInfo.RtcSessionDescription = rtcSessionDescription;
            callInfo.ReceiverId = receiverId;

            switch (status)
            {
                case Proto.CallOfferStatus.Accepted:
                    callInfo.Status = CallStatus.Accepted;
                    break;
                default:
                    callInfo.Status = CallStatus.Rejected;
                    break;
            }

            return new Proto.CallAnswerResponse() {  };
        }

        public async Task<Proto.CallOfferResponse> SendCallOfferAsync(string senderId, string roomId, Proto.RtcSessionDescription rtcSessionDescription, CancellationToken cancellationToken)
        {
            CallInfo callInfo = new CallInfo() { Status = CallStatus.Ongoing };
            _onGoingCallOffer.TryAdd(roomId ?? "", callInfo);

            int availableRoomUsers = 0;

            Room room = await _roomRepository.GetRoomAsync(roomId);
            if (room.IsNull())
            {
                _logger.LogError("Room with id : {0} not found.", roomId);
            }
            else
            {
                IEnumerable<RoomUser> otherRoomUsers = room.RoomUsers.Where(ru => ru.UserId != senderId);
                foreach (var user in otherRoomUsers)
                {
                    if (_onlineUsers.TryGetValue(user.UserId, out IServerStreamWriter<Proto.JoinResponse> stream))
                    {
                        try
                        {
                            
                            await stream.WriteAsync(createJoinResponseForVideoCall(senderId, rtcSessionDescription));
                            availableRoomUsers++;

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Stream write error removing user from online users");
                            _onlineUsers.TryRemove(user.UserId, out _);
                        }

                    }
                }
                
            }
            if (availableRoomUsers > 0)
            {
                //hard 2 min timeout
                TimeSpan hardTimeout = TimeSpan.FromMinutes(2);
                DateTime starTime = DateTime.Now;

                while (callInfo.Status == CallStatus.Ongoing && !cancellationToken.IsCancellationRequested && (DateTime.Now - starTime) <= hardTimeout)
                {
                    await Task.Delay(1000);
                }
            }

            var status = Proto.CallOfferStatus.Rejected;
            switch (callInfo.Status)
            {
                case CallStatus.Accepted:
                    status = Proto.CallOfferStatus.Accepted;
                    break;
                default:
                    status = Proto.CallOfferStatus.Rejected;
                    break;
            }
            return new Proto.CallOfferResponse() { Status = status, RtcSessionDescription = callInfo.RtcSessionDescription, ReceiverId = callInfo.ReceiverId };

        }

        private static Proto.JoinResponse createJoinResponseForVideoCall(string senderId, Proto.RtcSessionDescription rtcSessionDescription)
        {
            return new Proto.JoinResponse() { Type = Proto.JoinResponseType.CallSignaling, CallOfferNotification = new Proto.CallOfferNotification() { SenderId = senderId, RtcSessionDescription = rtcSessionDescription } };
        }

        public async Task<bool> IsUserInRoomAsync(string userId, string roomId)
        {
            //validate if user is in room
            return await _roomRepository.IsUserInRoomAsync(userId, roomId);
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

        private Proto.JoinResponse createJoinResponseForMessage(string senderId, Proto.MessageRequest messageRequest)
        {
            Proto.MessageNotification notification = new Proto.MessageNotification() { RoomId = messageRequest.RoomId, SenderId = senderId };
            return new Proto.JoinResponse() {Type = Proto.JoinResponseType.Notification, MessageNotification = notification };
        }

        private Message createMessage(Proto.MessageRequest messageRequest, Room room, User sender)
        {
            return new Message() { DateSent = DateTime.Now, MessageBody = messageRequest.MessageBody, RoomId = room.Id, SenderId = sender.Id, Room = room, Sender = sender };
        }


    }
}
