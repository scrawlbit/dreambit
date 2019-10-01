using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

namespace DreamBit.Extension.Components
{
    public abstract class ToolWindow : ToolWindowPane
    {
        protected ToolWindow() : base(null)
        {
        }

        protected void InitializeToolbar(int id)
        {
            ToolBar = new CommandID(new Guid(DreamBitPackage.Guids.CommandSet), id);
        }
    }
}