using System.Windows.Input;

namespace DreamBit.Extension.Module.Tools
{
    internal class SelectionTool : EditorTool
    {
        public override string Icon => "cursor";
        public override Key ShortcutKey => Key.V;
    }
}
