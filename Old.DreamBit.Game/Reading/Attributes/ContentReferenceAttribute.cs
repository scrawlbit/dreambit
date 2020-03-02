using System;

namespace DreamBit.Game.Reading.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class ContentReferenceAttribute : Attribute
    {
        public ContentReferenceAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }
    }
}