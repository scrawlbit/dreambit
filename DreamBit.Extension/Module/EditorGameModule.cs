using DreamBit.Extension.Management;
using DreamBit.Game.Content;
using DreamBit.Game.Drawing;
using DreamBit.Pipeline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Scrawlbit.MonoGame.Helpers;
using Scrawlbit.Notification;
using ScrawlBit.MonoGame.Interop.Controls;
using System.Windows.Input;

namespace DreamBit.Extension.Module
{
    public class EditorGameModule : GameModule
    {
        private readonly IEditor _editor;
        private readonly IPipeline _pipeline;
        private readonly IContentLoader _contentLoader;
        private readonly IContentDrawer _drawer;
        private ContentManager _contentManager;

        public EditorGameModule(IEditor editor, IPipeline pipeline, IContentLoader contentLoader, IContentDrawer contentDrawer)
        {
            _editor = editor;
            _pipeline = pipeline;
            _contentLoader = contentLoader;
            _drawer = contentDrawer;

            _pipeline.Notify().On(p => p.Loaded).Changed(InitializeContentManager);
        }

        protected override void Initialize()
        {
            _drawer.SpriteBatch = SpriteBatch;
        }
        protected override void Draw()
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (_editor.OpenedScene != null)
            {
                DrawScene();
                DrawTools();
            }
        }
        protected override void OnRenderSizeChanged(RenderSizeChangedEventArgs args)
        {
            _editor.Camera.Size = new Vector2(args.Width, args.Height);
        }

        protected override void OnKeyDown(KeyEventArgs args)
        {
            _editor.ToolBox.OnKeyDown(args);
        }
        protected override void OnKeyUp(KeyEventArgs args)
        {
            _editor.ToolBox.OnKeyUp(args);
        }

        protected override void OnMouseEnter(GameMouseEventArgs args)
        {
            _editor.ToolBox.OnMouseEnter(args);
        }
        protected override void OnMouseMove(GameMouseEventArgs args)
        {
            _editor.ToolBox.OnMouseMove(args);
        }
        protected override void OnMouseLeave(GameMouseEventArgs args)
        {
            _editor.ToolBox.OnMouseLeave(args);
        }
        protected override void OnMouseDown(GameMouseButtonEventArgs args)
        {
            _editor.ToolBox.OnMouseDown(args);
        }
        protected override void OnMouseUp(GameMouseButtonEventArgs args)
        {
            _editor.ToolBox.OnMouseUp(args);
        }
        protected override void OnMouseWheel(GameMouseWheelEventArgs args)
        {
            _editor.ToolBox.OnMouseWheel(args);
        }

        private void InitializeContentManager()
        {
            if (!_pipeline.Loaded)
            {
                _contentManager?.Dispose();
                return;
            }

            _contentManager = new ContentManager(ServiceProvider, _pipeline.BuiltContentFolder);
            _contentLoader.Manager = _contentManager;
        }
        private void DrawScene()
        {
            using (_drawer.Batch(transformMatrix: _editor.Camera.TransformMatrix))
                _editor.OpenedScene.Preview(_drawer);
        }
        private void DrawTools()
        {
            using (_drawer.Batch())
                _editor.ToolBox.Draw(_drawer);
        }
    }
}
