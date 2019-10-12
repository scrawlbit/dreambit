using System;
using DreamBit.Game.Drawing;
using DreamBit.Game.Elements.Components;
using Scrawlbit.Injection;

namespace DreamBit.Game.Factory
{
    internal class EditableGameObjectComponentFactory : IGameObjectComponentFactory
    {
        private readonly IContainer _container;

        public EditableGameObjectComponentFactory(IContainer container)
        {
            _container = container;
        }

        public GameObjectComponent CreateImageRenderer()
        {
            return new EditableImageRenderer(_container.Resolve<IDrawBatch>());
        }
        public GameObjectComponent CreateTextRenderer()
        {
            return new EditableTextRenderer(_container.Resolve<IDrawBatch>());
        }
        public GameObjectComponent CreateCameraObject()
        {
            return new EditableCameraObject();
        }
        public GameObjectComponent CreateScriptBehavior(string fileId)
        {
            return new EditableScriptBehavior(Guid.Parse(fileId));
        }
    }
}