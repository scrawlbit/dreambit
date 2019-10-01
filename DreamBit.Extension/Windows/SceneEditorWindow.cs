using System.Runtime.InteropServices;
using DreamBit.Extension.Components;

namespace DreamBit.Extension.Windows
{
    [Guid(DreamBitPackage.Guids.SceneEditorWindow)]
    public class SceneEditorWindow : ToolWindow
    {
        public SceneEditorWindow()
        {
            Caption = "Scene Editor";
            Content = new SceneEditorView();
        }
    }
}