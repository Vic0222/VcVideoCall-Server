using System;
using System.Collections.Generic;
using System.Text;

namespace Vc.Domain.Entities
{
    [Flags]
    public enum RoomType : byte
    {
        Private = 1,
        Group = 2
    }
}
