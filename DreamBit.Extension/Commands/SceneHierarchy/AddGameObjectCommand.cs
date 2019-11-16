using DreamBit.Extension.Components;
using DreamBit.Extension.Models;

namespace DreamBit.Extension.Commands.SceneHierarchy
{
    internal interface IAddGameObjectCommand : IToolCommand { }
    internal sealed class AddGameObjectCommand : ToolCommand, IAddGameObjectCommand
    {
        private readonly IEditingScene _scene;

        public AddGameObjectCommand(IEditingScene scene)
        {
            _scene = scene;
        }

        protected override int Id => DreamBitPackage.Guids.AddGameObjectCommand;

        public override void Execute(object parameter)
        {
            var target = (parameter as ISceneObject)?.Children ?? _scene.Objects;

            target.Add("Game Object");
        }
    }
}
