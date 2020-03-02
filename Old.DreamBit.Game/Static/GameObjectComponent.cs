using DreamBit.Game.Components;
using DreamBit.Game.Helpers;

// ReSharper disable once CheckNamespace
namespace DreamBit.Game.Elements.Components
{
    partial class GameObjectComponent
    {
        internal static GameObjectComponent Copy(GameObjectComponent component)
        {
            var type = component.GetType();
            var properties = type.GetProperties();
            var copy = BaseGame.Container.Resolve(type);

            foreach (var property in properties)
            {
                if (property.HasPublicGetter() && property.HasPublicSetter())
                    property.CopyValue(copy, component);
            }

            return (GameObjectComponent)copy;
        }
    }
}