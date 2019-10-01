using System.Runtime.InteropServices;
using DreamBit.Extension.Components;

namespace DreamBit.Extension.Windows
{
    [Guid(DreamBitPackage.Guids.SceneHierarchyWindow)]
    public class SceneHierarchyWindow : ToolWindow
    {
        public SceneHierarchyWindow()
        {
            Caption = "Scene Hierarchy";
            Content = new SceneHierarchyView();

            InitializeToolbar(DreamBitPackage.Guids.SceneHierarchyToolbar);
        }
    }
}
