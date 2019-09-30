using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Scrawlbit.Presentation.Data
{
    public class ContextMenuBinding : Binding
    {
        private const string PlacementTargetTag = "PlacementTarget.Tag";

        public ContextMenuBinding()
            : base(PlacementTargetTag)
        {
            Initialize();
        }
        public ContextMenuBinding(string path)
            : base(PlacementTargetTag + "." + path)
        {
            Initialize();
        }

        public new PropertyPath Path
        {
            get { return base.Path; }
            set
            {
                value.Path = PlacementTargetTag + "." + value.Path;
                base.Path = value;
            }
        }

        private void Initialize()
        {
            RelativeSource = new RelativeSource
            {
                Mode = RelativeSourceMode.FindAncestor,
                AncestorType = typeof(ContextMenu)
            };
        }
    }
}