using AutoMapper;
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
using VcGrpcService.Managers;
using Proto = VcGrpcService.Proto;

namespace VcGrpcService.AppServices
{
    public class ChatAppService : AbstractAppService
    {

        private readonly ILogger<ChatAppService> _logger;
        private readonly OngoingCallOfferManager _ongoingCallOfferManager;
        private readonly OnlineUserManager _onlineUserManager;
        private readonly IRoomRepository _roomRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ChatAppService(ILogger<ChatAppService> logger, OngoingCallOfferManager ongoingCallOfferManager, OnlineUserManager onlineUserManager, IRoomRepository roomRepository, IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper)
        {
            _logger = logger;
            _ongoingCallOfferManager = ongoingCallOfferManager;
            _onlineUserManager = onlineUserManager;
            _roomRepository = roomRepository;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public void AddOnlineUser(string userId, IServerStreamWriter<Proto.JoinResponse> responseStream)
        {
            _onlineUserManager.AddOnlineUser(userId, responseStream);
        }

        public void RemoveOnlineUser(string userId)
        {
            
            _onlineUserManager.RemoveOnlineUser(userId);
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
                    if (_onlineUserManager.OnlineUsers.TryGetValue(user.UserId, out IServerStreamWriter<Proto.JoinResponse> stream))
                    {
                        try
                        {
                            await stream.WriteAsync(createJoinResponseForMessage(senderId, message));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Stream write error removing user from online users" );
                            _onlineUserManager.OnlineUsers.TryRemove(user.UserId, out _);
                        }
                        
                    }
                }
            }
        }

        public async Task<Proto.GetMessagesResponse> GetMessagesByRoomIdOfUser(string roomId, long lastMessageTimestamp)
        {

            _logger.LogDebug("Getting messages for room {0} with lastmessage timestamp of {1} starting", roomId, lastMessageTimestamp);
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
            _logger.LogDebug("Getting messages for room {0} with lastmessage timestamp of {1} done", roomId, lastMessageTimestamp);

            return response;

        }

        public async Task<Proto.CallAnswerResponse> SendCallAnserAsync(Proto.CallOfferStatus status, string receiverId, string roomId, Proto.RtcSessionDescription rtcSessionDescription)
        {
            Proto.CallAnswerResponse response = null;

            if (_ongoingCallOfferManager.OnGoingCallOffer.TryGetValue(roomId, out CallInfo callInfo))
            {
                callInfo.RtcSessionDescription = rtcSessionDescription;
                callInfo.ReceiverId = receiverId;
                switch (status)
                {
                    case Proto.CallOfferStatus.CallOfferAccepted:
                        callInfo.Status = CallStatus.Accepted;
                        break;
                    default:
                        callInfo.Status = CallStatus.Rejected;
                        break;
                }

                response = new Proto.CallAnswerResponse();
            }


            return  await Task.FromResult(response);

        }

        public async Task<Proto.IceCandidateResponse> SendIceCandidate(string senderId, Proto.IceCandidateRequest request)
        {
            Room room = await _roomRepository.GetRoomAsync(request.RoomId);
            if (room.IsNull())
            {
                _logger.LogError("Room with id : {0} not found.", request.RoomId);
            }
            else
            {
                IEnumerable<RoomUser> otherRoomUsers = room.RoomUsers.Where(ru => ru.UserId != senderId);
                foreach (var user in otherRoomUsers)
                {
                    if (_onlineUserManager.OnlineUsers.TryGetValue(user.UserId, out IServerStreamWriter<Proto.JoinResponse> stream))
                    {
                        try
                        {
                            await stream.WriteAsync(createIceCandidateNotification(request));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "SendIceCandidate stream write error. Removing user from online users");
                            _onlineUserManager.OnlineUsers.TryRemove(user.UserId, out _);
                        }

                    }
                }

            }
            return new Proto.IceCandidateResponse();

        }

        public async Task<Proto.GetRoomResponse> GetRoomAsync(string senderId, Proto.GetRoomRequest request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Getting room with roomId : {0} or userId : {1}", request.RoomId, request.UserId);
            Room room = null;
            if (request.Type == Proto.GetRoomType.FromRoomId)
            {
                room = await _roomRepository.GetRoomAsync(request.RoomId);
            }
            else if(request.Type == Proto.GetRoomType.FromUserIdPrivate)
            {
                room = await _roomRepository.GetPrivateRoomAsync(senderId, request.UserId);
            }
            var response = new Proto.GetRoomResponse();
            if (room == null)
            {
                response.RoomStatus = Proto.RoomStatus.RoomNotExisting;
            }
            else
            {
                response.Room = await createRoomReplyAsync(senderId, room);
                response.RoomStatus = response.Room.Status;
            }
            return response;
        }

        /// <summary>
        /// Search user base on a keyword from Proto.SearchUserRequest
        /// </summary>
        /// <param name="request"> The Proto.SearchUserRequest that contains the keyword</param>
        /// <returns>A Proto.SearchUserResponse that contains the users.</returns>
        public async Task<Proto.SearchUserResponse> SearchUser(string currentUserId, Proto.SearchUserRequest request)
        {
            _logger.LogDebug("Searching for {0}", request.Keyword);

            var users = await _userRepository.GetUsersUsingKeywordAsync(request.Keyword);

            var filteredUsers = users.Where(u => u.Id != currentUserId).ToList();

            return createSearchUserResponse(filteredUsers);

        }

        private Proto.SearchUserResponse createSearchUserResponse(List<User> users)
        {
            var response = new Proto.SearchUserResponse();
            response.Users.AddRange(_mapper.Map<List<Proto.User>>(users));
            return response;
        }

        public async Task<Proto.PeerConnectionCloseResponse> SendPeerConnectionClose(string senderId, Proto.PeerConnectionCloseRequest request)
        {
            _logger.LogDebug("Broadcasting peer connection close from {0}", senderId);
            //get and validate sender early
            User sender = await _userRepository.GetUserAsync(senderId);
            if (sender.IsNull())
            {
                _logger.LogError("Sender with id : {0} not found. throwing exeption.", senderId);
                throw new UserNotFoundExeption("Sender not found");
            }

            Room room = await _roomRepository.GetRoomAsync(request.RoomId);

            if (room.IsNull())
            {
                _logger.LogError("Room with id : {0} not found.", request?.RoomId ?? string.Empty);
            }
            else
            {

                foreach (var user in room.RoomUsers.Where(ru => ru.UserId != senderId))
                {
                    if (_onlineUserManager.OnlineUsers.TryGetValue(user.UserId, out IServerStreamWriter<Proto.JoinResponse> stream))
                    {
                        try
                        {
                            await stream.WriteAsync(createJoinResponseForPeerConnectionClose(senderId, request));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Stream write error removing user from online users");
                            _onlineUserManager.OnlineUsers.TryRemove(user.UserId, out _);
                        }

                    }
                }
            }

            return new Proto.PeerConnectionCloseResponse();
        }

        private static Proto.JoinResponse createIceCandidateNotification(Proto.IceCandidateRequest request)
        {
            var iceCandidate = new Proto.IceCandidateNotification() { RoomId = request.RoomId, RtcIceCandidate = request.RtcIceCandidate };
            Proto.JoinResponse joinResponse = new Proto.JoinResponse() { Type = Proto.JoinResponseType.IceCandidate, IceCandidateNotification = iceCandidate };
            return joinResponse;
        }

        public async Task<Proto.CallOfferResponse> SendCallOfferAsync(string senderId, string roomId, Proto.RtcSessionDescription rtcSessionDescription, CancellationToken cancellationToken)
        {
            CallInfo callInfo = new CallInfo() { Status = CallStatus.Ongoing };
            _ongoingCallOfferManager.OnGoingCallOffer.TryRemove(roomId ?? "", out _ );

            _ongoingCallOfferManager.OnGoingCallOffer.TryAdd(roomId ?? "", callInfo);

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
                    if (_onlineUserManager.OnlineUsers.TryGetValue(user.UserId, out IServerStreamWriter<Proto.JoinResponse> stream))
                    {
                        try
                        {
                            
                            await stream.WriteAsync(createJoinResponseForVideoCall(senderId, roomId, rtcSessionDescription));
                            availableRoomUsers++;

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Stream write error removing user from online users");
                            _onlineUserManager.OnlineUsers.TryRemove(user.UserId, out _);
                        }

                    }
                }
                
            }
            if (availableRoomUsers > 0)
            {
                //hard 2 min timeout
                TimeSpan hardTimeout = TimeSpan.FromMinutes(2);
                DateTime starTime = DateTime.Now;



                CallInfo callInfo1 = callInfo;

                while (callInfo1.IsNotNull() && callInfo1.Status == CallStatus.Ongoing && !cancellationToken.IsCancellationRequested && (DateTime.Now - starTime) <= hardTimeout)
                {
                    await Task.Delay(1000);
                    _ongoingCallOfferManager.OnGoingCallOffer.TryGetValue(roomId, out callInfo1);
                }

                callInfo = callInfo1;
            }

            var status = Proto.CallOfferStatus.CallOfferRejected;
            if (callInfo != null)
            {
                switch (callInfo.Status)
                {
                    case CallStatus.Accepted:
                        status = Proto.CallOfferStatus.CallOfferAccepted;
                        break;
                    default:
                        status = Proto.CallOfferStatus.CallOfferRejected;
                        break;
                }
            }



            _ongoingCallOfferManager.OnGoingCallOffer.TryRemove(roomId, out _);

            return new Proto.CallOfferResponse() { Status = status, RtcSessionDescription = callInfo?.RtcSessionDescription, ReceiverId = callInfo?.ReceiverId };

        }

        private static Proto.JoinResponse createJoinResponseForVideoCall(string senderId, string roomId, Proto.RtcSessionDescription rtcSessionDescription)
        {
            return new Proto.JoinResponse() { Type = Proto.JoinResponseType.CallSignaling, CallOfferNotification = new Proto.CallOfferNotification() { SenderId = senderId, RtcSessionDescription = rtcSessionDescription, RoomId = roomId } };
        }

        public async Task<bool> IsUserInRoomAsync(string userId, string roomId)
        {
            //validate if user is in room
            return await _roomRepository.IsUserInRoomAsync(userId, roomId);
        }

        public async Task<Proto.GetRoomsResponse> GetUserRoomsAsync(string userId)
        {
            var rooms = await _roomRepository.GetUserRoomsAsync(userId);
            var roomList = new Proto.GetRoomsResponse();
            foreach (var room in rooms)
            {
                roomList.Rooms.Add(await createRoomReplyAsync(userId, room));
            }
            return roomList;
        }
        //test
        private async Task<Proto.Room> createRoomReplyAsync(string currentUserId, Room room)
        {
            

            string name = room.Name;
            string photoUrl = room.PhotoUrl;

            if (room.Type == RoomType.Private)
            {
                name = room.RoomUsers.FirstOrDefault(ru => ru.UserId != currentUserId)?.Nickname ?? string.Empty;
                photoUrl = room.RoomUsers.FirstOrDefault(ru => ru.UserId != currentUserId)?.PhotoUrl ?? string.Empty;
            }
            Message message = await _messageRepository.GetRoomLastMessageAsync(room.Id);
            string lastMessage = message?.MessageBody ?? string.Empty;
            long unixTimestamp = message?.DateSent.IsValid() ?? false ? ((DateTimeOffset)message.DateSent).ToUnixTimeSeconds() : 0;
            bool isOnline = room.RoomUsers.Select(ru => ru.UserId).Intersect(_onlineUserManager.OnlineUsers.Where(u => u.Key != currentUserId).Select(u => u.Key)).Any();
            var domStatus = room.RoomUsers.Where(ru => ru.UserId == currentUserId).Select(ru => ru.Status).FirstOrDefault();
            var protoStatus = Proto.RoomStatus.RoomNotExisting;
            switch (domStatus)
            {
                case RoomUserStatus.InvitePending:
                    protoStatus = Proto.RoomStatus.RoomInvitePending;
                    break;
                case RoomUserStatus.AcceptPending:
                    protoStatus = Proto.RoomStatus.RoomAcceptPending;
                    break;
                case RoomUserStatus.Accepted:
                    protoStatus = Proto.RoomStatus.RoomAccepted;
                    break;
                default:
                    break;
            }

            return new Proto.Room() { Id = room.Id, Name = name, Type = covertRoomType(room.Type),  LastMessage = lastMessage, LastMessageDatetime = unixTimestamp, IsOnline = isOnline, PhotoUrl= photoUrl, Status = protoStatus };
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

        private Proto.JoinResponse createJoinResponseForPeerConnectionClose(string senderId, Proto.PeerConnectionCloseRequest messageRequest)
        {
            Proto.PeerConnectionCloseNotification notification = new Proto.PeerConnectionCloseNotification() { RoomId = messageRequest.RoomId, };
            return new Proto.JoinResponse() { Type = Proto.JoinResponseType.PeerConnectionClose, PeerConnectionCloseNotification = notification };
        }

        private Message createMessage(Proto.MessageRequest messageRequest, Room room, User sender)
        {
            return new Message() { DateSent = DateTime.Now, MessageBody = messageRequest.MessageBody, RoomId = room.Id, SenderId = sender.Id, Room = room, Sender = sender };
        }


    }
}
