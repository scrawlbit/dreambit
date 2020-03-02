using System;
using DreamBit.Game.Drawing;

namespace DreamBit.Game.Elements.Components
{
    internal class EditableTextRenderer : TextRenderer
    {
        internal EditableTextRenderer(IDrawBatch drawBatch) : base(drawBatch)
        {
        }

        public ComponentType Type => ComponentType.TextRenderer;
        public Guid FileId { get; set; }
    }
}