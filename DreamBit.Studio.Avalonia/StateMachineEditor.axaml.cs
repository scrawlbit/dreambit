using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using DreamBit.Engine.Components;

namespace DreamBit.Studio.Avalonia
{
    /// <summary>
    /// Editor visual (nó-e-fio) da <see cref="AnimationStateMachine"/>: estados como nós
    /// arrastáveis e transições como setas. Arraste com o botão direito de um nó a outro para
    /// criar uma transição; parâmetro/condição das transições são editados no inspetor por texto.
    /// </summary>
    public partial class StateMachineEditor : Window
    {
        private readonly AnimationStateMachine _fsm = null!; // sempre criado via ctor com fsm; o ctor sem-arg é só para o carregador XAML
        private Canvas _canvas = null!;

        private const double NodeW = 150, NodeH = 54;

        private AnimStateDef? _selected;
        private AnimStateDef? _dragDef;
        private Point _dragOffset;
        private AnimStateDef? _wireFrom;

        public StateMachineEditor() { InitializeComponent(); }

        public StateMachineEditor(AnimationStateMachine fsm) : this()
        {
            _fsm = fsm;
            _canvas = this.FindControl<Canvas>("NodeCanvas")!;
            _canvas.PointerMoved += OnCanvasMoved;
            _canvas.PointerReleased += OnCanvasReleased;
            AutoLayout();
            Redraw();
        }

        private void AutoLayout()
        {
            if (_fsm.States.All(s => s.X == 0 && s.Y == 0))
            {
                int i = 0;
                foreach (var s in _fsm.States)
                {
                    s.X = 40 + (i % 4) * 190;
                    s.Y = 40 + (i / 4) * 110;
                    i++;
                }
            }
        }

        private void Redraw()
        {
            _canvas.Children.Clear();

            // Transições (setas) primeiro, por baixo dos nós.
            foreach (var t in _fsm.Transitions)
            {
                var a = _fsm.States.FirstOrDefault(s => s.Name == t.From);
                var b = _fsm.States.FirstOrDefault(s => s.Name == t.To);
                if (b == null) continue;
                var pa = a != null ? Center(a) : Center(b); // "*" (qualquer) sai do próprio destino
                var pb = Center(b);
                _canvas.Children.Add(new Line
                {
                    StartPoint = pa, EndPoint = pb,
                    Stroke = new SolidColorBrush(Color.Parse("#5A6270")), StrokeThickness = 2
                });
                var mid = new Point((pa.X + pb.X) / 2, (pa.Y + pb.Y) / 2);
                var label = new Border
                {
                    Background = new SolidColorBrush(Color.Parse("#2A2E38")),
                    CornerRadius = new CornerRadius(3), Padding = new Thickness(4, 1),
                    Child = new TextBlock { Text = $"{(string.IsNullOrEmpty(t.From) ? "*" : "")}{t.Parameter} {CondText(t.Condition)}", FontSize = 10, Foreground = Brushes.White },
                    Tag = t
                };
                label.DoubleTapped += (_, _) => { _fsm.RemoveTransition(t); Redraw(); };
                Canvas.SetLeft(label, mid.X - 20);
                Canvas.SetTop(label, mid.Y - 8);
                _canvas.Children.Add(label);
            }

            // Nós (estados).
            foreach (var s in _fsm.States)
            {
                bool isDefault = s.Name == _fsm.DefaultState;
                var node = new Border
                {
                    Width = NodeW, Height = NodeH,
                    CornerRadius = new CornerRadius(6),
                    Background = new SolidColorBrush(Color.Parse(ReferenceEquals(s, _selected) ? "#2E3A55" : "#242A36")),
                    BorderThickness = new Thickness(isDefault ? 2 : 1),
                    BorderBrush = new SolidColorBrush(Color.Parse(isDefault ? "#FFD27A" : "#3A4150")),
                    Padding = new Thickness(8),
                    Tag = s,
                    Child = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock { Text = s.Name, Foreground = Brushes.White, FontWeight = FontWeight.SemiBold },
                            new TextBlock { Text = "clipe: " + s.Clip, Foreground = new SolidColorBrush(Color.Parse("#8A93A6")), FontSize = 11 }
                        }
                    }
                };
                Canvas.SetLeft(node, s.X);
                Canvas.SetTop(node, s.Y);
                node.PointerPressed += OnNodePressed;
                _canvas.Children.Add(node);
            }
        }

        private Point Center(AnimStateDef s) => new(s.X + NodeW / 2, s.Y + NodeH / 2);
        private static string CondText(AnimCondition c) => c switch
        { AnimCondition.BoolTrue => "= true", AnimCondition.BoolFalse => "= false", _ => "(trigger)" };

        private void OnNodePressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not Border node || node.Tag is not AnimStateDef def)
                return;
            _selected = def;

            var pt = e.GetCurrentPoint(node);
            if (pt.Properties.IsRightButtonPressed)
            {
                _wireFrom = def; // início de uma transição
            }
            else
            {
                _dragDef = def;
                var onCanvas = e.GetPosition(_canvas);
                _dragOffset = new Point(onCanvas.X - def.X, onCanvas.Y - def.Y);
            }
            Redraw();
            e.Handled = true;
        }

        private void OnCanvasMoved(object? sender, PointerEventArgs e)
        {
            if (_dragDef == null) return;
            var p = e.GetPosition(_canvas);
            _dragDef.X = (float)(p.X - _dragOffset.X);
            _dragDef.Y = (float)(p.Y - _dragOffset.Y);
            Redraw();
        }

        private void OnCanvasReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_wireFrom != null)
            {
                var target = NodeAt(e.GetPosition(_canvas));
                if (target != null && target != _wireFrom)
                {
                    _fsm.AddTransition(_wireFrom.Name, target.Name, "param", AnimCondition.BoolTrue);
                    Redraw();
                }
                _wireFrom = null;
            }
            _dragDef = null;
        }

        private AnimStateDef? NodeAt(Point p)
            => _fsm.States.FirstOrDefault(s => p.X >= s.X && p.X <= s.X + NodeW && p.Y >= s.Y && p.Y <= s.Y + NodeH);

        private void OnAddState(object? sender, RoutedEventArgs e)
        {
            int n = _fsm.States.Count + 1;
            string name = "estado" + n;
            var def = _fsm.AddStateAt(name, name, 40 + (n % 4) * 190, 200);
            if (string.IsNullOrEmpty(_fsm.DefaultState))
                _fsm.DefaultState = name;
            _selected = def;
            Redraw();
        }

        private void OnSetDefault(object? sender, RoutedEventArgs e)
        {
            if (_selected != null) { _fsm.DefaultState = _selected.Name; Redraw(); }
        }

        private void OnDeleteState(object? sender, RoutedEventArgs e)
        {
            if (_selected != null) { _fsm.RemoveState(_selected); _selected = null; Redraw(); }
        }
    }
}
