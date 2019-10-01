using System.Windows.Input;
using DreamBit.Extension.Helpers;
using Microsoft.Xna.Framework;

namespace DreamBit.Extension.Windows
{
    public partial class SceneEditorView
    {
        public SceneEditorView()
        {
            InitializeComponent();
            if (this.IsInDesignMode())
                return;

            GameControl.Module = new Module.TestGame();
            GameControl.MouseMove += OnGameControlMouseMove;
        }

        private void OnGameControlMouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(GameControl);
            var module = (Module.TestGame)GameControl.Module;

            module.Position = new Vector2((float)position.X, (float)position.Y);
        }
    }
}
