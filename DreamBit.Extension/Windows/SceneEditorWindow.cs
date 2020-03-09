using System.Runtime.InteropServices;
using DreamBit.Extension.Components;
using DreamBit.Extension.Helpers;
using DreamBit.Extension.Management;
using Scrawlbit.Notification;

namespace DreamBit.Extension.Windows
{
    [Guid(DreamBitPackage.Guids.SceneEditorWindow)]
    public class SceneEditorWindow : ToolWindow
    {
        private readonly IEditor _editor;

        public SceneEditorWindow()
        {
            var view = new SceneEditorView();

            Content = view;

            if (!view.IsInDesignMode())
            {
                DreamBitPackage.Container.Inject(out _editor);

                _editor.Notify().On(e => e.OpenedSceneFile.Name).Changed(UpdateCaption);
            }

            UpdateCaption();
        }

        private void UpdateCaption(string name = null)
        {
            Caption = name ?? "Scene Editor";
        }
    }
}