using DreamBit.Project;

namespace DreamBit.Pipeline.Files
{
    public interface IPipelineImage : IProjectFile
    {
    }

    public sealed class PipelineImage : ProjectFile, IPipelineImage
    {
        public const string ImageType = "Image";
        public const string ImageExtension = "png";

        public override string Type => ImageType;
        public override string Extension => ImageExtension;
    }
}