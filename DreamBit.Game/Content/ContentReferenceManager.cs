using System;
using System.Collections.Generic;
using System.Reflection;
using DreamBit.Game.Elements.Components;

namespace DreamBit.Game.Content
{
    internal class ContentReferenceManager : IContentReferenceManager
    {
        private readonly IContentManager _contentManager;
        private readonly Dictionary<GameObjectComponent, List<(PropertyInfo Property, Guid FileId)>> _resolutions;

        public ContentReferenceManager(IContentManager contentManager)
        {
            _contentManager = contentManager;
            _resolutions = new Dictionary<GameObjectComponent, List<(PropertyInfo Property, Guid FileId)>>();
        }

        public void Prepare(GameObjectComponent component, PropertyInfo property, Guid fileId)
        {
            if (!_resolutions.ContainsKey(component))
                _resolutions[component] = new List<(PropertyInfo Property, Guid FileId)>();

            _resolutions[component].Add((property, fileId));
        }
        public void Resolve(GameObjectComponent component)
        {
            if (!_resolutions.ContainsKey(component))
                return;

            var resolution = _resolutions[component];

            foreach (var r in resolution)
                r.Property.SetValue(component, LoadContent(r.Property.PropertyType, r.FileId));
        }

        private object LoadContent(Type type, Guid fileId)
        {
            return _contentManager.Load(type, fileId);
        }
    }
}