using System;
using System.Collections.Generic;
using System.Text;

namespace Vc.Common
{
    public static class Extensions
    {
        public static bool IsNull<T>(this T obj)
        {
            return obj == null;
        }

        public static bool IsNotNull<T>(this T obj)
        {
            return !obj.IsNull();
        }

        public static bool IsValid(this DateTime datetime)
        {
            return datetime.Year >= 1990;
        }
    }
}
