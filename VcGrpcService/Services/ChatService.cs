using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using VcGrpcService.AppServices;



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

        public override async Task Join(IAsyncStreamReader<MessageRequest> requestStream, IServerStreamWriter<Notification> responseStream, ServerCallContext context)
        {
            if (!await requestStream.MoveNext()) return;
            string senderId = context.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
            do
            {
                _chatAppService.AddOnlineUser(senderId, responseStream);
                await _chatAppService.BroadcastMessage(senderId, requestStream.Current).ContinueWith(t=> {
                    if (t.IsFaulted)
                    {
                        _logger.LogError(t.Exception, "Broadcast error");
                    }
                    
                });
            } while (await requestStream.MoveNext());

            _chatAppService.RemoveOnlineUser(senderId);
        }
        public override async Task<RoomListReply> GetRooms(RoomRequest request, ServerCallContext context)
        {
            string userId = context.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _chatAppService.SendUserRoomsAsync(userId);
        }
    }
}
