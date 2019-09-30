using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace ScrawlBit.Presentation.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class LocalizableDescriptionAttribute : DescriptionAttribute
    {
        private readonly Type _resourcesType;
        private bool _isLocalized;

        public LocalizableDescriptionAttribute(Type resourcesType, string key)
            : base(key)
        {
            _resourcesType = resourcesType;
        }

        public override string Description
        {
            get
            {
                if (!_isLocalized)
                {
                    var resourceManager = GetResourceManager();
                    var culture = GetCulture();

                    _isLocalized = true;

                    if (resourceManager != null)
                        DescriptionValue = resourceManager.GetString(DescriptionValue, culture);
                }

                return DescriptionValue;
            }
        }

        private ResourceManager GetResourceManager()
        {
            var member = _resourcesType.InvokeMember(
                @"ResourceManager",
                BindingFlags.GetProperty | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic,
                null,
                null,
                new object[] { }
            );

            return member as ResourceManager;
        }
        private CultureInfo GetCulture()
        {
            var member = _resourcesType.InvokeMember(
                @"Culture",
                BindingFlags.GetProperty | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic,
                null,
                null,
                new object[] { }
            );

            return member as CultureInfo;
        }
    }
}