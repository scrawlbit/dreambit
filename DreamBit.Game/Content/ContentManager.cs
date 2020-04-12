using DreamBit.Pipeline.Files;
using DreamBit.Pipeline.Helpers;
using DreamBit.Project;
using System;
using System.Collections.Generic;
using MonoGameContentManager = Microsoft.Xna.Framework.Content.ContentManager;

namespace DreamBit.Game.Content
{
    internal class ContentManager : IContentManager, IContentLoader
    {
        private readonly IDictionary<Guid, IContent> _contents;

        public ContentManager()
        {
            _contents = new Dictionary<Guid, IContent>();
        }

        public MonoGameContentManager Manager { get; set; }

        IContent IContentManager.Load(IProjectFile file)
        {
            switch (file)
            {
                case IPipelineImage image: return ((IContentManager)this).Load(image);

                default: throw new ArgumentOutOfRangeException(nameof(file));
            }
        }
        Image IContentManager.Load(IPipelineImage file)
        {
            return GetOrCreate(file, () => new Image(file, this));
        }
        T IContentLoader.Load<T>(IProjectFile file)
        {
            return Manager.Load<T>(file.GetContentPath());
        }

        private T GetOrCreate<T>(IProjectFile file, Func<T> factory) where T : IContent
        {
            if (!_contents.ContainsKey(file.Id))
                _contents.Add(file.Id, factory());

            return (T)_contents[file.Id];
        }
    }
}
