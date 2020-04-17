using DreamBit.Game.Elements.Components;
using Microsoft.Xna.Framework;
using Scrawlbit.MonoGame.Helpers;
using Scrawlbit.Notification;
using System;
using System.Linq;

namespace DreamBit.Game.Elements
{
    public sealed partial class GameObject : NotificationObject
    {
        private GameObject _parent;
        private string _name;
        private bool _isVisible;
        private bool _isExpanded;
        private bool _isSelected;
        private bool _isMoving;

        public GameObject()
        {
            Id = Guid.NewGuid();
            Transform = new Transform();
            IsVisible = true;
            Children = new GameObjectCollection(this);
            Components = new GameComponentCollection(this);
        }

        public Guid Id { get; internal set; }
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }
        public Transform Transform { get; }
        public bool IsVisible
        {
            get => _isVisible;
            set => Set(ref _isVisible, value);
        }
        public GameObject Parent
        {
            get => _parent;
            set
            {
                if (value == _parent)
                    return;

                _parent?.Children.Remove(this);
                _parent = value;
                _parent?.Children.Add(this);

                Transform.BaseTransform = value?.Transform;
            }
        }
        public IGameObjectCollection Children { get; }
        public IGameComponentCollection Components { get; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => Set(ref _isExpanded, value);
        }
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }
        public bool IsMoving
        {
            get => _isMoving;
            set => Set(ref _isMoving, value);
        }

        public Rectangle Area()
        {
            Rectangle[] areas =
            {
                ImageArea(),
                TextArea()
            };

            var points = areas.SelectMany(a => new[]
            {
                a.LeftTop(),
                a.RightTop(),
                a.RightBottom(),
                a.LeftBottom()
            }).Select(p => p.ToVector2()).ToArray();

            var minX = points.Min(p => p.X);
            var minY = points.Min(p => p.Y);
            var maxX = points.Max(p => p.X);
            var maxY = points.Max(p => p.Y);

            var leftTop = VectorHelper.Transform(minX, minY, Transform.Matrix);
            var rightBottom = VectorHelper.Transform(maxX, maxY, Transform.Matrix);

            if (Transform.Rotation != 0)
            {
                points = new[] { leftTop, rightBottom };

                minX = points.Min(p => p.X);
                minY = points.Min(p => p.Y);
                maxX = points.Max(p => p.X);
                maxY = points.Max(p => p.Y);

                leftTop = new Vector2(minX, minY);
                rightBottom = new Vector2(maxX, maxY);
            }

            return new Rectangle(leftTop.ToPoint(), (rightBottom - leftTop).ToPoint());
        }
        public Rectangle TotalArea()
        {
            var area = Area();

            foreach (var child in Children)
                area = Rectangle.Union(area, child.TotalArea());

            return area;
        }

        internal void Initialize()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Initialize(this);
            }

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Initialize();
            }
        }
        internal void Update()
        {
            if (!IsVisible)
                return;

            for (var i = 0; i < Components.Count; i++)
            {
                GameComponent component = Components[i];

                component.Initialize(this);
                component.Update();
            }

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Update();
            }
        }
        internal void PostUpdate()
        {
            if (!IsVisible)
                return;

            for (var i = 0; i < Components.Count; i++)
            {
                GameComponent component = Components[i];

                if (component.Started)
                    component.PostUpdate();
            }

            for (int i = 0; i < Children.Count; i++)
                Children[i].PostUpdate();
        }
        internal void Draw()
        {
            if (!IsVisible)
                return;

            Transform.ValidateTransformations();

            for (var i = 0; i < Components.Count; i++)
            {
                GameComponent component = Components[i];

                if (component.Started)
                    component.Draw();
            }

            for (int i = 0; i < Children.Count; i++)
                Children[i].Draw();
        }
        internal void Preview()
        {
            if (!IsVisible)
                return;

            Transform.ValidateTransformations();

            for (var i = 0; i < Components.Count; i++)
            {
                GameComponent component = Components[i];

                component.Initialize(this);
                component.Draw();
            }

            for (int i = 0; i < Children.Count; i++)
                Children[i].Preview();
        }

        private Rectangle ImageArea()
        {
            ImageRenderer renderer = Components.Find<ImageRenderer>();

            if (renderer?.Image?.Texture == null)
                return Rectangle.Empty;

            return CalculateComponentArea(renderer.Origin, renderer.Image.Size);
        }
        private Rectangle TextArea()
        {
            TextRenderer renderer = Components.Find<TextRenderer>();

            if (renderer?.Text == null || renderer.Font?.SpriteFont == null)
                return Rectangle.Empty;

            return CalculateComponentArea(renderer.Origin, renderer.Size);
        }
        private static Rectangle CalculateComponentArea(Vector2 origin, Vector2 size)
        {
            return new Rectangle((-origin).ToPoint(), size.ToPoint());
        }
    }
}
