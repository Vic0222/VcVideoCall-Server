using System;
using System.Collections.Generic;
using System.Text;

namespace Vc.Domain.Entities
{
    [Flags]
    public enum RoomType : byte
    {
        Unknown = 0,
        Private = 1,
        Group = 2
    }
}
