using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VcGrpcService.CallHelpers;

namespace VcGrpcService.Managers
{
    public class OngoingCallOfferManager
    {
        /// <summary>
        /// Should move to redis
        /// </summary>
        private ConcurrentDictionary<string, CallInfo> _onGoingCallOffer = new ConcurrentDictionary<string, CallInfo>();
        public ConcurrentDictionary<string, CallInfo> OnGoingCallOffer { get => _onGoingCallOffer; }

    }
}
