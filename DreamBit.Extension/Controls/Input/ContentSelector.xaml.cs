using DreamBit.Extension.Helpers;
using DreamBit.Game.Content;
using DreamBit.Project;
using DreamBit.Project.Helpers;
using Microsoft.VisualStudio.PlatformUI;
using Scrawlbit.Helpers;
using Scrawlbit.Presentation.Commands;
using Scrawlbit.Presentation.Dependency;
using Scrawlbit.Presentation.DragAndDrop;
using Scrawlbit.Presentation.Helpers;
using System;
using System.Windows.Input;

namespace DreamBit.Extension.Controls.Input
{
    public partial class ContentSelector
    {
        public static DependencyProperty<ContentSelector, IContent> ContentDataProperty;
        private readonly IProject _project;
        private readonly IContentManager _contentManager;

        static ContentSelector()
        {
            var registry = new DependencyRegistry<ContentSelector>();

            ContentDataProperty = registry.Property(c => c.ContentData);
        }
        public ContentSelector()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            DreamBitPackage.Container.Inject(out _project);
            DreamBitPackage.Container.Inject(out _contentManager);

            var command = new DelegateCommand<DropEventArgs>(Execute, CanExecute);
            var behavior = new DroppableBehavior { DropCommand = command };

            this.AddBehavior(behavior);
        }

        public IContent ContentData
        {
            get => ContentDataProperty.Get(this);
            set => ContentDataProperty.Set(this, value);
        }
        public Type ContentType { get; set; }
        public ICommand DropCommand { get; }

        private bool CanExecute(DropEventArgs args)
        {
            if (args.Data.Length != 1)
                return false;

            string path = args.Data[0] as string;

            if (path?.StartsWith(_project.Folder) != true)
                return false;

            ProjectFile file = _project.Files.GetByPath(path);

            if (!_contentManager.IsContent(file))
                return false;

            if (ContentType != null)
            {
                IContent content = _contentManager.Load(file);
                Type type = content.GetType();

                return type.IsAssignableTo(ContentType);
            }

            return true;
        }
        private void Execute(DropEventArgs args)
        {
            string path = (string)args.Data[0];
            ProjectFile file = _project.Files.GetByPath(path);
            IContent content = _contentManager.Load(file);

            ContentData = content;
        }
    }
}
