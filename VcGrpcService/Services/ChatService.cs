using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Vc.Common;
using VcGrpcService.AppServices;
using VcGrpcService.Proto;

namespace VcGrpcService.Services
{
    [Authorize]
    public class ChatService : Chat.ChatBase
    {
        private readonly ILogger<ChatService> _logger;
        private ChatAppService _chatAppService;

        public ChatService(ILogger<ChatService> logger, ChatAppService chatAppService)
        {
            _logger = logger;
            _chatAppService = chatAppService;

        }

        public override async Task Join(JoinRequest request, IServerStreamWriter<JoinResponse> responseStream, ServerCallContext context)
        {
            string senderId = context.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                
                _chatAppService.AddOnlineUser(senderId, responseStream);
                await responseStream.WriteAsync(new Proto.JoinResponse() { Type = JoinResponseType.Confirmation });

                while (!context.CancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(5000);
                }

                _chatAppService.RemoveOnlineUser(senderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Join room error. UserId : {0}", senderId);
                throw;
            }
            
        }


        public override async Task<GetRoomsResponse> GetRooms(GetRoomsRequest request, ServerCallContext context)
        {
            try
            {
                string userId = context.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
                return await _chatAppService.GetUserRoomsAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get Rooms error");
                throw;
            }
            
        }
        public async override Task<GetMessagesResponse> GetMessages(GetMessagesRequest request, ServerCallContext context)
        {
            try
            {
                
                string userId = context.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
                bool isInRoom = await _chatAppService.IsUserInRoomAsync(userId, request.RoomId);
                if (!isInRoom)
                {
                    throw new RpcException(new Status(StatusCode.PermissionDenied, "Room access denied"));
                }
                

                return await _chatAppService.GetMessagesByRoomIdOfUser(request.RoomId, request.LastMessageDatetime);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get Messages error");
                throw;
            }
        }

        public override async Task<MessageResponse> SendMessageRequest(MessageRequest request, ServerCallContext context)
        {
            string senderId = string.Empty;
            try
            {
                senderId = context.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _chatAppService.BroadcastMessage(senderId, request).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        _logger.LogError(t.Exception, "Broadcast error");
                    }
                });
                return new MessageResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send messages error RoomdId : {0}, Sender : {1}", request.RoomId, senderId);
                throw;
            }

        }

        public override async Task<CallOfferResponse> SendCallOffer(CallOfferRequest request, ServerCallContext context)
        {
            string senderId = context.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
               return  await _chatAppService.SendCallOfferAsync(senderId, request.RoomId, request.RtcSessionDescription, context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send offer video call error Sender : {1}", senderId);
                throw;
            }
        }

        public override async Task<CallAnswerResponse> SendCallAnswer(CallAnswerRequest request, ServerCallContext context)
        {
            string receiverId = context.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var response = await _chatAppService.SendCallAnserAsync(request.Status, receiverId, request.RoomId, request.RtcSessionDescription);
                if (response == null)
                {
                    throw new RpcException(new Status(StatusCode.Cancelled, "Call was cancelled"));
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send offer video call error Sender : {1}", receiverId);
                throw;
            }
        }

        public override async Task<IceCandidateResponse> SendIceCandidate(IceCandidateRequest request, ServerCallContext context)
        {
            string senderId = context.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                return await _chatAppService.SendIceCandidate(senderId, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send offer video call error Sender : {1}", senderId);
                throw;
            }
        }

        public override async Task<PeerConnectionCloseResponse> SendPeerConnectionClose(PeerConnectionCloseRequest request, ServerCallContext context)
        {
            string senderId = context.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                return await _chatAppService.SendPeerConnectionClose(senderId, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send offer video call error Sender : {0}", senderId);
                throw;
            }
        }

        public override async Task<SearchUserResponse> SearchUser(SearchUserRequest request, ServerCallContext context)
        {
            
            string senderId = context.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                return await _chatAppService.SearchUser(senderId, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User {0} searching  for {1}", senderId, request.Keyword);
                throw;
            }
        }

        public async override Task<GetRoomResponse> GetRoom(GetRoomRequest request, ServerCallContext context)
        {
            string senderId = context.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                return await _chatAppService.GetRoomAsync(senderId, request, context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User {0} getting room for {0}", senderId, request.UserId);
                throw;
            }
        }

        public override async Task<InviteUserResponse> SendInviteToUser(InviteUserRequest request, ServerCallContext context)
        {
            string senderId = context.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                return await _chatAppService.SendInviteToUserAsync(senderId, request, context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User {0} sending invite to user {1}", senderId, request.UserId);
                throw;
            }
        }
    }
}
