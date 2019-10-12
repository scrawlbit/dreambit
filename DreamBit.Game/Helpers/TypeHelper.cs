using System;
using System.Collections.Generic;
using System.Reflection;

namespace DreamBit.Game.Helpers
{
    internal static class TypeHelper
    {
        public static IEnumerable<PropertyInfo> GetInstanceProperties(this Type type)
        {
            foreach (var property in type.GetProperties())
            {
                if (property.IsStatic()) continue;
                if (property.HasParameters()) continue;

                yield return property;
            }
        }
    }
}