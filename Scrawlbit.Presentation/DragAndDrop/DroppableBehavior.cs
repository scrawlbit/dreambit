using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using ScrawlBit.Presentation.Dependency;

namespace ScrawlBit.Presentation.DragAndDrop
{
    public class DroppableBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty<DroppableBehavior, ICommand> DropCommandProperty;
        public static readonly DependencyProperty<DroppableBehavior, DroppableAdorner> AdornerProperty;
        private bool _canBeDropped;

        static DroppableBehavior()
        {
            var dependency = new DependencyRegistry<DroppableBehavior>();

            DropCommandProperty = dependency.Property(b => b.DropCommand);
            AdornerProperty = dependency.Property(b => b.Adorner);
        }

        public ICommand DropCommand
        {
            get { return DropCommandProperty.Get(this); }
            set { DropCommandProperty.Set(this, value); }
        }
        public DroppableAdorner Adorner
        {
            get { return AdornerProperty.Get(this); }
            set { AdornerProperty.Set(this, value); }
        }
        public bool CanDropFiles { get; set; }
        protected object[] Data { get; private set; }
        protected bool IsFiles { get; private set; }
        protected object Target => AssociatedObject.DataContext;
        protected DropType DropType { get; private set; }
        protected DropType LastDropType { get; private set; }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AllowDrop = true;
            AssociatedObject.DragEnter += OnDragEnter;
            AssociatedObject.DragOver += OnDragOver;
            AssociatedObject.DragLeave += OnDragLeave;
            AssociatedObject.Drop += OnDrop;
        }
        protected override void OnDetaching()
        {
            AssociatedObject.DragEnter -= OnDragEnter;
            AssociatedObject.DragOver -= OnDragOver;
            AssociatedObject.DragLeave -= OnDragLeave;
            AssociatedObject.Drop -= OnDrop;

            base.OnDetaching();
        }

        protected virtual void OnDragEnter(object sender, DragEventArgs e)
        {
            IsFiles = CanDropFiles && ((DataObject)e.Data).ContainsFileDropList();

            if (IsFiles)
                Data = ((DataObject)e.Data).GetFileDropList().Cast<object>().ToArray();
            else
                Data = e.Data.GetData(typeof(object[])) as object[];

            if (Data?.Length == 0)
                Data = null;

            _canBeDropped = Data != null;

            e.Handled = true;
        }
        protected virtual void OnDragOver(object sender, DragEventArgs e)
        {
            if (Data != null)
            {
                LastDropType = DropType;
                DropType = GetDropType(e);

                if (DropType != LastDropType)
                {
                    Adorner?.Update(DropType, Data, Target);

                    if (DropCommand != null && !Data.Contains(Target))
                    {
                        var args = GetDropEventArgs();

                        if (args.Data?.Length > 0)
                            _canBeDropped = DropCommand.CanExecute(args);
                    }
                }
            }

            if (!_canBeDropped)
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }
        protected virtual void OnDragLeave(object sender, DragEventArgs e)
        {
            _canBeDropped = false;
            Data = null;
            LastDropType = 0;
            DropType = 0;
            Adorner?.Remove();

            e.Handled = true;
        }
        protected virtual void OnDrop(object sender, DragEventArgs e)
        {
            if (_canBeDropped && !Data.Contains(Target) && DropCommand != null)
            {
                var args = GetDropEventArgs();

                if (args.Data?.Length > 0 && DropCommand.CanExecute(args))
                    DropCommand?.Execute(args);
            }

            Adorner?.Remove();
            e.Handled = true;
        }

        protected virtual DropType GetDropType(DragEventArgs e)
        {
            return DropType.Inside;
        }
        protected virtual DropEventArgs GetDropEventArgs()
        {
            return new DropEventArgs
            {
                Data = Data,
                IsFiles = IsFiles,
                Target = Target,
                DropType = DropType
            };
        }
    }
}