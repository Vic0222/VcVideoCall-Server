using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VcGrpcService.Proto;

namespace VcGrpcService.CallHelpers
{
    public class CallInfo
    {
        public RtcSessionDescription RtcSessionDescription { get; set; }
        public string ReceiverId { get; set; }
        public CallStatus Status { get; set; }
    }
    public enum CallStatus : byte { 
        Ongoing = 1,
        Accepted = 2,
        Rejected = 4
    }
}
