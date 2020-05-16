using DreamBit.Game.Drawing;
using Microsoft.Xna.Framework;

namespace DreamBit.Extension.Module.TransformHandlers
{
    internal interface ITransformHandler
    {
        bool IsMouseOver(Vector2 position);

        void Draw(IContentDrawer drawer);
    }
}
