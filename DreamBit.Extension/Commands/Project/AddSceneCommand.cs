using DreamBit.Extension.Components;
using DreamBit.Extension.Management;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.Project
{
    internal interface IAddSceneCommand : ICommand { }
    internal class AddSceneCommand : ToolCommand, IAddSceneCommand
    {
        private readonly IPackageBridge _package;
        private readonly IProjectManager _manager;

        public AddSceneCommand(IPackageBridge package, IProjectManager manager)
        {
            _package = package;
            _manager = manager;
        }


        protected override int Id => DreamBitPackage.Guids.AddSceneCommand;

        public override void Execute(object parameter)
        {
        }
        protected override bool CanShow(object parameter)
        {
            return _manager.IsSingleItemSelected();
        }
    }
}
