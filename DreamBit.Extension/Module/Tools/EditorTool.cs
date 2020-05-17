using DreamBit.Extension.Module.Handlers;
using System.Windows.Input;

namespace DreamBit.Extension.Module.Tools
{
    public interface IEditorTool : IEditorHandler
    {
        string Icon { get; }
        Key ShortcutKey { get; }
        bool KeepShortcutPressed { get; }
    }

    internal abstract class EditorTool : EditorHandler, IEditorTool
    {
        public abstract string Icon { get; }
        public abstract Key ShortcutKey { get; }
        public virtual bool KeepShortcutPressed => false;
    }
}
