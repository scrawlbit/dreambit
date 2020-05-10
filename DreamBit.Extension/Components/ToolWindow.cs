using Microsoft.VisualStudio.Shell;

namespace DreamBit.Extension.Components
{
    public abstract class ToolWindow : ToolWindowPane
    {
        protected ToolWindow() : base(null)
        {
        }
    }
}