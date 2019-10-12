using System.Collections.Generic;
using System.Linq;
using DreamBit.Pipeline.Exceptions;
using Scrawlbit.Helpers;
using Scrawlbit.Mapping;

namespace DreamBit.Pipeline.Imports
{
    internal interface IContentImporter
    {
        IReadOnlyList<IContentImport> Imports { get; }

        bool IsPathIncluded(string path);
        IContentImport GetByPath(string path);

        void AddOrUpdate(IContentImport import);
        void Remove(string path);
        void Clear();
    }

    internal class ContentImporter : IContentImporter
    {
        private readonly IMappingService _mappingService;
        private readonly List<IContentImport> _imports;

        public ContentImporter(IMappingService mappingService)
        {
            _mappingService = mappingService;
            _imports = new List<IContentImport>();
        }

        public IReadOnlyList<IContentImport> Imports => _imports;

        public bool IsPathIncluded(string path)
        {
            return _imports.Any(c => c.Path == path);
        }
        public IContentImport GetByPath(string path)
        {
            return Imports.SingleOrDefault(i => i.Path == path);
        }

        public void AddOrUpdate(IContentImport import)
        {
            var added = GetByPath(import.Path);

            if (added != null)
                _mappingService.Map(import).To(added);
            else
                _imports.InsertOrdered(import, i => (int)i.BuildtAction, i => i.Path);
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