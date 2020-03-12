using System.Reflection;

namespace Scrawlbit.Util.Helpers
{
    public static class MemberHelper
    {
        public static void SetProperty(this object obj, PropertyInfo property, object value)
        {
            property.SetValue(obj, value);
        }
    }
}
