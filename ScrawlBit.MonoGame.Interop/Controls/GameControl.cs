using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScrawlBit.MonoGame.Interop.Controls
{
    public class GameControl : ContentControl
    {
        public static readonly DependencyProperty ModuleProperty;
        private readonly DrawingSurface _surface;
        private bool _surfaceLoaded;
        private bool _shouldFireKeyEvents;

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
            _surface.Focusable = true;
            _surface.FocusVisualStyle = null;

            _surface.LoadContent += OnLoadContent;
            _surface.Draw += OnDraw;
            _surface.RenderSizeChanged += OnRenderSizeChanged;

            _surface.KeyDown += OnKeyDown;
            _surface.KeyUp += OnKeyUp;

            _surface.MouseEnter += OnMouseEnter;
            _surface.MouseMove += OnMouseMove;
            _surface.MouseLeave += OnMouseLeave;
            _surface.MouseDown += OnMouseDown;
            _surface.MouseUp += OnMouseUp;
            _surface.MouseWheel += OnMouseWheel;

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
        private void OnRenderSizeChanged(object sender, RenderSizeChangedEventArgs args)
        {
            Module?.OnRenderSizeChanged(args);
        }

        private void OnKeyDown(object sender, KeyEventArgs args)
        {
            if (_shouldFireKeyEvents)
                Module?.OnKeyDown(args);
        }
        private void OnKeyUp(object sender, KeyEventArgs args)
        {
            if (_shouldFireKeyEvents)
                Module?.OnKeyUp(args);
        }

        private void OnMouseEnter(object sender, MouseEventArgs args)
        {
            _surface.Focus();
            _shouldFireKeyEvents = true;

            Module?.OnMouseEnter(new GameMouseEventArgs(args, _surface));
        }
        private void OnMouseMove(object sender, MouseEventArgs args)
        {
            Module?.OnMouseMove(new GameMouseEventArgs(args, _surface));
        }
        private void OnMouseLeave(object sender, MouseEventArgs args)
        {
            _shouldFireKeyEvents = false;

            Module?.OnMouseLeave(new GameMouseEventArgs(args, _surface));
        }
        private void OnMouseDown(object sender, MouseButtonEventArgs args)
        {
            Module?.OnMouseDown(new GameMouseButtonEventArgs(args, _surface));
        }
        private void OnMouseUp(object sender, MouseButtonEventArgs args)
        {
            Module?.OnMouseUp(new GameMouseButtonEventArgs(args, _surface));
        }
        private void OnMouseWheel(object sender, MouseWheelEventArgs args)
        {
            Module?.OnMouseWheel(new GameMouseWheelEventArgs(args, _surface));
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