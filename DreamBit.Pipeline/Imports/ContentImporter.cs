using System.Collections.Generic;
using System.Linq;
using DreamBit.Pipeline.Exceptions;
using Scrawlbit.Helpers;
using Scrawlbit.Mapping;

namespace DreamBit.Pipeline.Imports
{
    internal interface IContentImporter
    {
        IReadOnlyList<ContentImport> Imports { get; }

        bool IsPathIncluded(string path);
        ContentImport GetByPath(string path);

        void AddOrUpdate(ContentImport import);
        void Move(string oldPath, string newPath);
        void Remove(string path);
        void Clear();
    }

    internal class ContentImporter : IContentImporter
    {
        private readonly IMappingService _mappingService;
        private readonly List<ContentImport> _imports;

        public ContentImporter(IMappingService mappingService)
        {
            _mappingService = mappingService;
            _imports = new List<ContentImport>();
        }

        public IReadOnlyList<ContentImport> Imports => _imports;

        public bool IsPathIncluded(string path)
        {
            return _imports.Any(c => c.Path == path);
        }
        public ContentImport GetByPath(string path)
        {
            return Imports.SingleOrDefault(i => i.Path == path);
        }

        public void AddOrUpdate(ContentImport import)
        {
            var added = GetByPath(import.Path);

            if (added != null)
                _mappingService.Map(import).To(added);
            else
                _imports.InsertOrdered(import, i => i.Path);
        }
        public void Move(string oldPath, string newPath)
        {
            var import = GetByPath(oldPath);
            var existent = GetByPath(newPath);

            import.Path = newPath;

            _imports.Remove(import);
            _imports.Remove(existent);
            _imports.InsertOrdered(import, i => i.Path);
        }
        public void Remove(string path)
        {
            var import = GetByPath(path);

            if (import == null)
                throw new ImportNotFoundException(path);

            _imports.Remove(import);
        }
        public void Clear()
        {
            _imports.Clear();
        }
    }
}