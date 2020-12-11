using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VcGrpcService.Managers
{
    public class OnlineUserManager
    {
        public OnlineUserManager(ILogger<OnlineUserManager> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Holds online users id. Should move to cache db like redis
        /// </summary>
        private ConcurrentDictionary<string, IServerStreamWriter<Proto.JoinResponse>> _onlineUsers = new ConcurrentDictionary<string, IServerStreamWriter<Proto.JoinResponse>>();
        private readonly ILogger<OnlineUserManager> _logger;

        public ConcurrentDictionary<string, IServerStreamWriter<Proto.JoinResponse>> OnlineUsers { get => _onlineUsers; }

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
    }
}
