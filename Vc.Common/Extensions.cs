using System;
using System.Collections.Generic;
using System.Text;

namespace Vc.Common
{
    public static class Extensions
    {
        public static bool IsNull<T>(this T Object)
        {
            return Object == null;
        }

        public static bool IsNotNull<T>(this T Object)
        {
            return !Object.IsNull();
        }
    }
}
