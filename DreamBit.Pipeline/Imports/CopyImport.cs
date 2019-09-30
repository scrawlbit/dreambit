namespace DreamBit.Pipeline.Imports
{
    public interface ICopyImport : IContentImport
    {
    }

    internal class CopyImport : ContentImport, ICopyImport
    {
        internal CopyImport(string path) : base(path, BuildtAction.Copy)
        {
        }
    }
}