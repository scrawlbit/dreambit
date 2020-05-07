using DreamBit.Extension.Helpers;
using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using DreamBit.General.State;
using Scrawlbit.Presentation.Commands;
using Scrawlbit.Presentation.Dependency;
using Scrawlbit.Presentation.DragAndDrop;
using Scrawlbit.Presentation.Helpers;
using System;

namespace DreamBit.Extension.Controls.Input
{
    public partial class GameObjectSelector
    {
        public delegate void GameObjectSelectorEventHandler(GameObjectSelector sender, ValueChangedEventArgs<Guid> e);
        public static DependencyProperty<GameObjectSelector, Guid> IdProperty;
        private readonly IEditor _editor;

        static GameObjectSelector()
        {
            var registry = new DependencyRegistry<GameObjectSelector>();

            IdProperty = registry.Property(g => g.Id, g => g.OnIdChanged());
        }
        public GameObjectSelector()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            DreamBitPackage.Container.Inject(out _editor);

            var command = new DelegateCommand<DropEventArgs>(Execute, CanExecute);
            var behavior = new DroppableBehavior { DropCommand = command };

            this.AddBehavior(behavior);
        }

        public event GameObjectSelectorEventHandler Changed;
        public Guid Id
        {
            get => IdProperty.Get(this);
            set => IdProperty.Set(this, value);
        }

        private void OnIdChanged()
        {
            GameObject gameObject = _editor.OpenedScene.Objects.Find(Id);

            Input.Text = gameObject?.Name ?? "";
        }

        private bool CanExecute(DropEventArgs args)
        {
            return args.Data.Length == 1 && args.Data[0] is GameObject;
        }
        private void Execute(DropEventArgs args)
        {
            GameObject gameObject = (GameObject)args.Data[0];

            if (gameObject.Id != Id)
            {
                Guid oldId = Id;

                Id = gameObject.Id;
                Changed?.Invoke(this, (oldId, Id));
            }
        }
    }
}
