using DreamBit.Game.Elements;
using Microsoft.Xna.Framework;
using Scrawlbit.Notification;
using Scrawlbit.Notification.Notificator;
using System;

namespace DreamBit.Extension.Module.Selection
{
    internal class SingleSelectionMode : ISelectionMode
    {
        private GameObject _gameObject;
        private readonly Notificator<GameObject> _notificator;

        public SingleSelectionMode(GameObject gameObject)
        {
            _gameObject = gameObject;
            _notificator = gameObject.Notify()
                .On(g => g.Name).Changed(NotifyNameChanged)
                .On(g => g.Transform.Rotation).Changed(NotifyAreaChanged)
                .On(g => g.Transform.Scale).Changed(NotifyAreaChanged);
        }

        public event Action NameChanged;
        public event Action AreaChanged;
        public string Name() => _gameObject.Name;
        public bool IsVisible() => _gameObject.IsVisible;
        public Vector2 Position() => _gameObject.Transform.Position;
        public float Rotation() => _gameObject.Transform.Rotation;
        public Vector2 Scale() => _gameObject.Transform.Scale;
        public Rectangle Area() => _gameObject.TotalArea();

        private void NotifyNameChanged()
        {
            NameChanged?.Invoke();
        }
        private void NotifyAreaChanged()
        {
            AreaChanged?.Invoke();
        }

        public void Dispose()
        {
            _notificator.Dispose();
        }
    }
}
