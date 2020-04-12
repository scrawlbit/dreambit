using DreamBit.Extension.Components;
using DreamBit.Pipeline;
using DreamBit.Project;

namespace DreamBit.Extension.Commands
{
    internal interface IBuildContentCommand : IToolCommand
    {
        bool CanExecute();

        void Execute(bool clean = false);
    }
    internal sealed class BuildContentCommand : ToolCommand, IBuildContentCommand
    {
        private readonly IPipeline _pipeline;

        public BuildContentCommand(IPipeline pipeline)
        {
            _pipeline = pipeline;
        }

        protected override int Id => DreamBitPackage.Guids.BuildContentCommand;

        public override bool CanExecute()
        {
            return _pipeline.Loaded;
        }

        public override void Execute()
        {
            Execute(true);
        }
        public void Execute(bool clean)
        {
            _pipeline.Build(_pipeline.BuiltContentFolder, clean);
        }
    }
}
