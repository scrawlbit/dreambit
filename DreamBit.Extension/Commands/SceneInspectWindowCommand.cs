using DreamBit.Extension.Components;
using DreamBit.Extension.Windows;

namespace DreamBit.Extension.Commands
{
    internal interface ISceneInspectWindowCommand : IToolCommand { }
    internal sealed class SceneInspectWindowCommand : ToolCommand, ISceneInspectWindowCommand
    {
        private readonly IPackageBridge _package;

        public SceneInspectWindowCommand(IPackageBridge package)
        {
            _package = package;
        }

        protected override int Id => DreamBitPackage.Guids.SceneInspectWindowCommand;

        public override void Execute(object parameter)
        {
            _package.ShowWindow<SceneInspectWindow>();
        }
    }
}
