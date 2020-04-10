using Microsoft.Xna.Framework;
using System;

namespace DreamBit.Extension.Module.Selection
{
    interface ISelectionMode : IDisposable
    {
        event Action NameChanged;
        event Action AreaChanged;

        string Name();
        bool IsVisible();
        Vector2 Position();
        float Rotation();
        Vector2 Scale();
        Rectangle Area();
    }
}
