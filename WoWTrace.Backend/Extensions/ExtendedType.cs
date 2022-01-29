using System;
using System.Linq;

namespace WoWTrace.Backend.Extensions
{
    public static class ExtendedType
    {
        public static T GetCustomAttributes<T>(this Type type)
        {
            return (T)type.GetCustomAttributes(typeof(T), false).Single();
        }
    }
}
