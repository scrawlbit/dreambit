using System;
using System.Linq;
using System.Reflection;

namespace Scrawlbit.Helpers
{
    public static class TypeHelper
    {
        public static bool HasInterface<T>(this Type type)
        {
            return type.GetInterfaces().Any(t => t == typeof(T));
        }

        public static PropertyInfo[] GetClassProperties(this Type type)
        {
            return type.GetProperties().Where(p => p.PropertyType.IsClassValueType()).ToArray();
        }

        public static bool IsClassValueType(this Type type)
        {
            return type != typeof(string) && (type.IsClass || type.IsAbstract || type.IsInterface);
        }

        public static bool IsAssignableTo<T>(this Type type)
        {
            return typeof(T).IsAssignableFrom(type);
        }
    }
}