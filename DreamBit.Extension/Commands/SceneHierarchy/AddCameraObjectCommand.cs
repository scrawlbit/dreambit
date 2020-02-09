using DreamBit.Extension.Components;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface IAddCameraObjectCommand : IToolCommand { }
    internal sealed class AddCameraObjectCommand : ToolCommand, IAddCameraObjectCommand
    {
        protected override int Id => DreamBitPackage.Guids.AddCameraObjectCommand;

        public override void Execute()
        {
        }
    }
}