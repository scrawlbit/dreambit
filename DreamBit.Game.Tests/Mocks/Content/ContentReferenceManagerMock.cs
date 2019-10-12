using System;
using System.Reflection;
using DreamBit.Game.Content;
using DreamBit.Game.Elements.Components;

namespace DreamBit.Game.Tests.Mocks.Content
{
    public class ContentReferenceManagerMock : IContentReferenceManager
    {
        public void Prepare(GameObjectComponent component, PropertyInfo property, Guid fileId)
        {
            throw new NotImplementedException();
        }
        public void Resolve(GameObjectComponent component)
        {
            throw new NotImplementedException();
        }
    }
}