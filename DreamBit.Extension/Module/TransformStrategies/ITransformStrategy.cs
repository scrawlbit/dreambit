using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;

namespace DreamBit.Extension.Module.TransformStrategies
{
    internal interface ITransformStrategy
    {
        bool IsMouseOverHandler(Vector2 position);

        void Draw(IContentDrawer drawer);
    }
}
