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

        public override async Task Join(IAsyncStreamReader<JoinRequest> requestStream, IServerStreamWriter<Proto.JoinResponse> responseStream, ServerCallContext context)
        {
            if (!await requestStream.MoveNext()) return;
            string senderId = context.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
            do
            {
                _chatAppService.AddOnlineUser(senderId, responseStream);
                if (requestStream.Current.Initial)
                {
                    await responseStream.WriteAsync(new Proto.JoinResponse() { Confirmation = true });
                }
                else
                {
                    await _chatAppService.BroadcastMessage(senderId, requestStream.Current?.MessageRequest).ContinueWith(t => {
                        if (t.IsFaulted)
                        {
                            _logger.LogError(t.Exception, "Broadcast error");
                        }
                    });
                }
                
            } while (await requestStream.MoveNext());

            _chatAppService.RemoveOnlineUser(senderId);
        }
        public override async Task<GetRoomsResponse> GetRooms(GetRoomsRequest request, ServerCallContext context)
        {
            try
            {
                string userId = context.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
                return await _chatAppService.SendUserRoomsAsync(userId);
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
                return await _chatAppService.GetMessagesByRoomIdOfUser(request.RoomId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get Messages error");
                throw;
            }
        }
    }
}
