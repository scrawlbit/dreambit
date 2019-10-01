using System.Runtime.InteropServices;
using DreamBit.Extension.Components;

namespace DreamBit.Extension.Windows
{
    [Guid(DreamBitPackage.Guids.SceneInspectWindow)]
    public class SceneInspectWindow : ToolWindow
    {
        public SceneInspectWindow()
        {
            Caption = "Scene Inspect";
            Content = new SceneInspectView();
        }
    }
}