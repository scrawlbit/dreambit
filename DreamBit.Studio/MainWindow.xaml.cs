using System.Windows;
using System.Windows.Input;
using DreamBit.Studio.Editor;
using DreamBit.Studio.Hosting;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace DreamBit.Studio
{
    public partial class MainWindow : Window
    {
        private readonly SceneEditorRenderer _scene = new();

        public MainWindow()
        {
            InitializeComponent();

            Surface.LoadContent += (_, device) => _scene.LoadContent(device);
            Surface.Draw += (_, e) =>
            {
                _scene.Draw(e.GraphicsDevice, e.DeltaSeconds, e.Width, e.Height);
                Dispatcher.InvokeAsync(UpdateStatus);
            };
        }

        private XnaVector2 MousePos(MouseEventArgs e)
        {
            var p = e.GetPosition(Surface);
            return new XnaVector2((float)p.X, (float)p.Y);
        }

        private void Surface_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Surface.Focus();
            Surface.CaptureMouse();
            _scene.OnMouseDown(MousePos(e));
        }

        private void Surface_MouseMove(object sender, MouseEventArgs e) => _scene.OnMouseMove(MousePos(e));

        private void Surface_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _scene.OnMouseUp();
            Surface.ReleaseMouseCapture();
        }

        private void UpdateStatus()
        {
            StatusText.Text = _scene.Selected
                ? "GameObject selecionado — arraste para mover."
                : "Clique no retângulo azul no canvas para selecioná-lo.";
        }
    }
}
