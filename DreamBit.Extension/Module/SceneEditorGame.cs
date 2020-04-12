﻿using DreamBit.Extension.Management;
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
    public class SceneEditorGame : GameModule
    {
        private readonly IEditor _editor;
        private readonly IPipeline _pipeline;
        private readonly IContentLoader _contentLoader;
        private readonly IContentDrawer _contentDrawer;
        private ContentManager _contentManager;

        public SceneEditorGame(IEditor editor, IPipeline pipeline, IContentLoader contentLoader, IContentDrawer contentDrawer)
        {
            _editor = editor;
            _pipeline = pipeline;
            _contentLoader = contentLoader;
            _contentDrawer = contentDrawer;

            _pipeline.Notify().On(p => p.Loaded).Changed(InitializeContentManager);
        }

        public override void Initialize()
        {
            _contentDrawer.SpriteBatch = SpriteBatch;
        }
        public override void Draw()
        {
            GraphicsDevice.Clear(Color.Transparent);

            if (_editor.OpenedScene == null)
                return;

            _contentDrawer.Begin();
            _editor.OpenedScene.Preview();
            _contentDrawer.End();
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