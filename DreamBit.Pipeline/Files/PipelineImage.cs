using DreamBit.Project;

namespace DreamBit.Pipeline.Files
{
    public interface IPipelineImage : IProjectFile
    {
    }

    public sealed class PipelineImage : ProjectFile, IPipelineImage
    {
        private readonly IPipeline _pipeline;

        internal PipelineImage(IPipeline pipeline)
        {
            _pipeline = pipeline;
        }

        protected override void OnAdded()
        {
            _pipeline.Contents.AddImport(this);
        }
        protected override void OnMoved(MovedEventArgs e)
        {
            _pipeline.Contents.Move(this, e.OldLocation);
        }
        protected override void OnRemoved()
        {
            _pipeline.Contents.Remove(this);
        }
    }
}