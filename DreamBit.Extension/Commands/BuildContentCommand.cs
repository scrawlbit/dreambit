using System.Windows.Input;
using DreamBit.Extension.Components;

namespace DreamBit.Extension.Commands
{
    internal interface IBuildContentCommand : ICommand { }
    internal sealed class BuildContentCommand : ToolCommand, IBuildContentCommand
    {
        protected override int Id => DreamBitPackage.Guids.BuildContentCommand;

        public override void Execute(object parameter)
        {
        }
    }
}
