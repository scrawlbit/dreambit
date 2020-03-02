using System;
using DreamBit.Game.Drawing;

namespace DreamBit.Game.Elements.Components
{
    internal class EditableImageRenderer : ImageRenderer
    {
        internal EditableImageRenderer(IDrawBatch drawBatch) : base(drawBatch)
        {
        }

        public ComponentType Type => ComponentType.ImageRenderer;
        public Guid FileId { get; set; }
    }
}