using System.Collections.Generic;
using System.Linq;
using DreamBit.Pipeline.Exceptions;
using DreamBit.Pipeline.Files;
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

        void Remove(string path);
        void Clear();
    }

    internal class Contents : IContents
    {
        private readonly IContentImporter _contentImporter;

        internal Contents(IContentImporter contentImporter)
        {
            _contentImporter = contentImporter;
        }

        public IReadOnlyList<IContentImport> Imports => _contentImporter.Imports;

        public ITextureImport AddImport(IPipelineImage image)
        {
            return Add(new TextureImport(image.Path));
        }
        public IFontImport AddImport(IPipelineFont font)
        {
            return Add(new FontImport(font.Path));
        }
        public ICopyImport AddCopy(IProjectFile file)
        {
            return Add(new CopyImport(file.Path));
        }

        public void Remove(string path)
        {
            _contentImporter.Remove(path);
        }
        public void Clear()
        {
            _contentImporter.Clear();
        }

        private T Add<T>(T import) where T : IContentImport
        {
            if (Imports.Any(i => i.Path == import.Path))
                throw new ImportAlreadyExistsException(import.Path);

            _contentImporter.AddOrUpdate(import);

            return import;
        }
    }
}