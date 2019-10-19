namespace DreamBit.Pipeline.Imports
{
    public interface IContentImport
    {
        string Path { get; }
        BuildtAction BuildtAction { get; }
    }
    
    internal class ContentImport : IContentImport
    {
        protected ContentImport(string path, BuildtAction buildtAction)
        {
            Path = path;
            BuildtAction = buildtAction;
        }

        public string Path { get; internal set; }
        public BuildtAction BuildtAction { get; }
    }
}