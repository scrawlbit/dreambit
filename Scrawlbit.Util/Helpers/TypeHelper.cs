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

        public static bool IsAssignableTo(this Type type, Type interfaceType)
        {
            return interfaceType.IsAssignableFrom(type);
        }
        public static bool IsAssignableTo<TInterface>(this Type type)
        {
            return type.IsAssignableTo(typeof(TInterface));
        }

        public static bool IsNullable(this Type type)
        {
            return IsNullable(type, out _);
        }
        public static bool IsNullable(this Type type, out Type underlyingType)
        {
            return (underlyingType = Nullable.GetUnderlyingType(type)) != null;
        }
    }
}