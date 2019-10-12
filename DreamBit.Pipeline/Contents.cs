using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Pipeline.Exceptions;
using DreamBit.Pipeline.Files;
using DreamBit.Pipeline.Helpers;
using DreamBit.Pipeline.Imports;
using DreamBit.Project;

namespace DreamBit.Pipeline
{
    public interface IContents
    {
        IReadOnlyList<IContentImport> Imports { get; }

        ITextureImport AddImport(IPipelineImage image);
        IFontImport AddImport(IPipelineFont font);
        ICopyImport AddCopy(IProjectFile file);

        void Remove(IProjectFile file);
        void Clear();
    }

    internal class Contents : IContents
    {
        private readonly IPipelineManager _manager;
        private readonly IContentImporter _contentImporter;

        internal Contents(IPipelineManager manager, IContentImporter contentImporter)
        {
            _manager = manager;
            _contentImporter = contentImporter;
        }

        public IReadOnlyList<IContentImport> Imports => _contentImporter.Imports;

        public ITextureImport AddImport(IPipelineImage image)
        {
            return Add(image, path => new TextureImport(path));
        }
        public IFontImport AddImport(IPipelineFont font)
        {
            return Add(font, path => new FontImport(path));
        }
        public ICopyImport AddCopy(IProjectFile file)
        {
            return Add(file, path => new CopyImport(path));
        }

        public void Remove(IProjectFile file)
        {
            string path = file.GetContentPath();

            if (!_contentImporter.IsPathIncluded(path))
                return;

            _contentImporter.Remove(path);
            _manager.NotifyChanges();
        }
        public void Clear()
        {
            _contentImporter.Clear();
            _manager.NotifyChanges();
        }

        private T Add<T>(IProjectFile file, Func<string, T> factory) where T : IContentImport
        {
            string path = file.GetContentPath();
            T import = (T)_contentImporter.GetByPath(path);

            if (import == null)
            {
                import = factory(path);
                _contentImporter.AddOrUpdate(import);
                _manager.NotifyChanges();
            }

            return import;
        }
    }
}