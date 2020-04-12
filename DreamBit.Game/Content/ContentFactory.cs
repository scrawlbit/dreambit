using DreamBit.Pipeline.Files;
using DreamBit.Project;
using System;
using System.Collections.Generic;

namespace DreamBit.Game.Content
{
    public interface IContentFactory
    {
        IContent Create(ProjectFile file);
        Image Create(PipelineImage file);
    }

    internal class ContentFactory : IContentFactory
    {
        private readonly IDictionary<Guid, IContent> _contents;

        public ContentFactory()
        {
            _contents = new Dictionary<Guid, IContent>();
        }

        public IContent Create(ProjectFile file)
        {
            switch (file)
            {
                case PipelineImage image: return Create(image);

                default: throw new ArgumentOutOfRangeException(nameof(file));
            }
        }
        public Image Create(PipelineImage file)
        {
            return GetOrCreate(file, () => new Image(file));
        }

        private T GetOrCreate<T>(ProjectFile file, Func<T> factory) where T : IContent
        {
            if (!_contents.ContainsKey(file.Id))
                _contents.Add(file.Id, factory());

            return (T)_contents[file.Id];
        }
    }
}
