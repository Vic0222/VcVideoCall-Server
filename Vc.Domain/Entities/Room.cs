using System;
using System.Collections.Generic;
using System.Text;

namespace Vc.Domain.Entities
{
    public class Room
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public RoomType Type { get; set; }

    }
}
