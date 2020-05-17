using DreamBit.Extension.Module.Handlers;
using System.Windows.Input;

namespace DreamBit.Extension.Module.Tools
{
    internal interface ISelectionTool : IEditorTool { }
    internal class SelectionTool : HandlerTool, ISelectionTool
    {
        public SelectionTool(IMoveHandler moveHandler, IRotateHandler rotateHandler, IScaleHandler scaleHandler, ISelectHandler selectHandler)
            : base(moveHandler, rotateHandler, scaleHandler, selectHandler)
        {
            selectHandler.DrawOrder = 1;
            moveHandler.DrawOrder = 2;
            rotateHandler.DrawOrder = 3;
            scaleHandler.DrawOrder = 4;
        }

        public override string Icon => "cursor";
        public override Key ShortcutKey => Key.V;
    }
}
