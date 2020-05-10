using DreamBit.Extension.Management;
using DreamBit.Game.Content;
using DreamBit.Game.Drawing;
using DreamBit.Pipeline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Scrawlbit.Notification;
using ScrawlBit.MonoGame.Interop.Controls;

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

        public override void Initialize()
        {
            _drawer.SpriteBatch = SpriteBatch;
        }
        public override void Draw()
        {
            GraphicsDevice.Clear(Color.Transparent);

            if (_editor.OpenedScene == null)
                return;

            using (_drawer.Batch(transformMatrix: _editor.Camera.TransformMatrix))
            {
                _editor.OpenedScene.Preview(_drawer);

                Rectangle selecionArea = _editor.Selection.Area();

                if (!selecionArea.IsEmpty)
                    _drawer.DrawRectangle(selecionArea, Color.Yellow);
            }
        }
        public override void RenderSizeChanged(RenderSizeChangedEventArgs args)
        {
            _editor.Camera.Size = new Vector2(args.Width, args.Height);
        }

        private void InitializeContentManager()
        {
            if (!_pipeline.Loaded)
            {
                _contentManager.Dispose();
                return;
            }

            _contentManager = new ContentManager(ServiceProvider, _pipeline.BuiltContentFolder);
            _contentLoader.Manager = _contentManager;
        }
    }
}
