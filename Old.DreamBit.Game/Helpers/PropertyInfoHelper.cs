using System.Linq;
using System.Reflection;

namespace DreamBit.Game.Helpers
{
    internal static class PropertyInfoHelper
    {
        public static bool IsStatic(this PropertyInfo property)
        {
            return property.GetMethod?.IsStatic ?? property.SetMethod.IsStatic;
        }
        public static bool HasParameters(this PropertyInfo property)
        {
            return property.GetIndexParameters().Any();
        }
        public static bool HasPublicGetter(this PropertyInfo property)
        {
            return property.GetMethod?.IsPublic == true;
        }
        public static bool HasPublicSetter(this PropertyInfo property)
        {
            return property.SetMethod?.IsPublic == true;
        }

        public static void CopyValue(this PropertyInfo property, object obj, object from)
        {
            var value = property.GetValue(from);
            property.SetValue(obj, value);
        }
    }
}