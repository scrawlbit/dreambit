using System;
using System.Reflection;
using DreamBit.Game.Elements.Components;

namespace DreamBit.Game.Content
{
    internal interface IContentReferenceManager
    {
        void Prepare(GameObjectComponent component, PropertyInfo property, Guid fileId);
        void Resolve(GameObjectComponent component);
    }
}