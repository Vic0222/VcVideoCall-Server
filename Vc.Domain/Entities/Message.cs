using System;
using System.Collections.Generic;
using System.Text;

namespace Vc.Domain.Entities
{
    public class Message
    {
        public string Id { get; set; }
        public string RoomId { get; set; }
        public string SenderId { get; set; }
        public string MessageBody { get; set; }
        public DateTime DateSent { get; set; }
        public User Sender { get; set; }
        public Room Room { get; set; }

    }
}
