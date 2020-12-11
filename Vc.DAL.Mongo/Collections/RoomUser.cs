using System;
using System.Collections.Generic;
using System.Text;

namespace Vc.DAL.Mongo.Collections
{
    public class RoomUser
    {
        public string UserId { get; set; }
        public string Nickname { get; set; }
        public string PhotoUrl { get; set; }
        public RoomUserStatus Status { get; set; }
    }

    public enum RoomUserStatus
    {
        InvitePending,
        AcceptPending,
        Accepted,
    }
}
