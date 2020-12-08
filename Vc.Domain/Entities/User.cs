using System;
using System.Collections.Generic;
using System.Text;

namespace Vc.Domain.Entities
{
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string PhotoUrl { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<RoomUser> RoomUsers { get; set; }
    }
}
