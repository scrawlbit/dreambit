using System.Windows.Input;
using DreamBit.Extension.Components;
using DreamBit.Pipeline;
using DreamBit.Project;

namespace DreamBit.Extension.Commands
{
    internal interface IBuildContentCommand : ICommand
    {
        void Execute(bool clean = false);
    }
    internal sealed class BuildContentCommand : ToolCommand, IBuildContentCommand
    {
        private readonly IProject _project;
        private readonly IPipeline _pipeline;

        public BuildContentCommand(IProject project, IPipeline pipeline)
        {
            _project = project;
            _pipeline = pipeline;
        }

        protected override int Id => DreamBitPackage.Guids.BuildContentCommand;

        public override bool CanExecute(object parameter)
        {
            return _pipeline.Loaded;
        }
        public override void Execute(object parameter)
        {
            Execute(true);
        }
        public void Execute(bool clean)
        {
            _pipeline.Build($"{_project.Folder}\\bin\\Windows", clean);
        }
    }
}
