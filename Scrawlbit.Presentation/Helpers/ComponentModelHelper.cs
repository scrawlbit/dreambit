using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Scrawlbit.Presentation.Helpers
{
    public static class ComponentModelHelper
    {
        public static string DisplayName(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DisplayAttribute)field.GetCustomAttribute(typeof(DisplayAttribute));

            return attribute?.Name ?? value.ToString();
        }
    }
}