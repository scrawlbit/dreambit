using System;
using System.Linq;
using System.Reflection;

namespace ScrawlBit.Helpers
{
    public static class AttributeHelper
    {
        public static bool HasAttribute<T>(this ICustomAttributeProvider provider, bool inherit = true) where T : Attribute
        {
            return provider.GetCustomAttributes(typeof(T), inherit).Any();
        }
        public static T Attribute<T>(this ICustomAttributeProvider provider, bool inherit = true) where T : Attribute
        {
            return (T)provider.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        }
    }
}