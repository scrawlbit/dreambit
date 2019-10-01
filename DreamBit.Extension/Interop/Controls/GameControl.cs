using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DreamBit.Extension.Interop.Controls
{
    public class GameControl : ContentControl
    {
        public static readonly DependencyProperty ModuleProperty;
        private readonly DrawingSurface _surface;
        private bool _surfaceLoaded;

        static GameControl()
        {
            ModuleProperty = DependencyProperty.Register(
                nameof(Module),
                typeof(GameModule),
                typeof(GameControl),
                new PropertyMetadata((s, e) => ((GameControl)s).OnModuleChanged())
            );
        }
        public GameControl()
        {
            if ((bool)GetValue(DesignerProperties.IsInDesignModeProperty))
                return;

            _surface = new DrawingSurface();
            _surface.AlwaysRefresh = true;
            _surface.LoadContent += OnLoadContent;
            _surface.Draw += OnDraw;

            Content = _surface;
        }

        public GameModule Module
        {
            get => (GameModule)GetValue(ModuleProperty);
            set => SetValue(ModuleProperty, value);
        }

        private void OnModuleChanged()
        {
            TryLoadModule();
        }

        private void OnLoadContent(object sender, GraphicsDeviceEventArgs e)
        {
            _surfaceLoaded = true;
            TryLoadModule();
        }
        private void OnDraw(object sender, DrawEventArgs e)
        {
            Module?.Draw();
        }

        private void TryLoadModule()
        {
            if (_surfaceLoaded && Module != null)
            {
                Module.Prepare(_surface.GraphicsDeviceService);
                Module.Initialize();
            }
        }
    }
}