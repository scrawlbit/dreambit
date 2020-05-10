using DreamBit.Extension.Components;
using System.Runtime.InteropServices;

namespace DreamBit.Extension.Windows
{
    [Guid(DreamBitPackage.Guids.SceneHierarchyWindow)]
    public class SceneHierarchyWindow : ToolWindow
    {
        public SceneHierarchyWindow()
        {
            Caption = "Scene Hierarchy";
            Content = new SceneHierarchyView();
        }
    }
}
