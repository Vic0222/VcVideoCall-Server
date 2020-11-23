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
    public class ChatRoomService : ChatRoom.ChatRoomBase
    {
        private readonly ILogger<ChatRoomService> _logger;
        private ChatAppService _chatAppService;

        public ChatRoomService(ILogger<ChatRoomService> logger, ChatAppService chatAppService)
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
                await _chatAppService.BroadcastMessage(senderId, requestStream.Current);
            } while (await requestStream.MoveNext());

            _chatAppService.RemoveOnlineUser(senderId);
        }

        public override async Task GetRooms(Empty request, IServerStreamWriter<RoomReply> responseStream, ServerCallContext context)
        {
            string userId = context.GetHttpContext().User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _chatAppService.SendUserRoomsAsync(userId, responseStream);
        }
    }
}
