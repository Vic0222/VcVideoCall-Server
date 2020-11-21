using System;
using System.Collections.Generic;
using System.Linq;
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

            do
            {
                _chatAppService.AddOnlineUser(requestStream.Current.Sender, responseStream);
                await _chatAppService.BroadcastMessage(requestStream.Current);
            } while (await requestStream.MoveNext());

            _chatAppService.RemoveOnlineUser(requestStream.Current.Sender);
        }

    }
}
