using DreamBit.Extension.Components;
using DreamBit.Extension.Windows;

namespace DreamBit.Extension.Commands
{
    internal interface ISceneHierarchyWindowCommand : IToolCommand { }
    internal sealed class SceneHierarchyWindowCommand : ToolCommand, ISceneHierarchyWindowCommand
    {
        private readonly IPackageBridge _package;

        public SceneHierarchyWindowCommand(IPackageBridge package)
        {
            _package = package;
        }

        protected override int Id => DreamBitPackage.Guids.SceneHierarchyWindowCommand;

        public override void Execute(object parameter)
        {
            _package.ShowWindow<SceneHierarchyWindow>();
        }
    }
}
