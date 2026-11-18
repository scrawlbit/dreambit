using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using DreamBit.Engine.Components.Custom;
using DreamBit.Engine.Elements;
using DreamBit.Studio.ViewModels;
using Microsoft.Xna.Framework;

namespace DreamBit.Studio.Avalonia
{
    /// <summary>
    /// Inspector genérico dos componentes customizados (do jogo): monta, por reflexão, um
    /// editor para cada propriedade pública (bool, número, texto, enum, Vector2, Point, Color,
    /// caminho de asset e JSON para listas/objetos). Toda edição passa pelo histórico (undo).
    /// </summary>
    internal sealed class CustomComponentInspector
    {
        private readonly StackPanel _panel;
        private readonly EditorViewModel _editor;
        private readonly Func<string, string, System.Threading.Tasks.Task<string?>> _pickFile;

        public CustomComponentInspector(StackPanel panel, EditorViewModel editor,
            Func<string, string, System.Threading.Tasks.Task<string?>> pickFile)
        {
            _panel = panel;
            _editor = editor;
            _pickFile = pickFile;
        }

        public void Rebuild()
        {
            _panel.Children.Clear();
            var obj = _editor.Inspector.Target;
            if (obj == null)
                return;

            foreach (var component in obj.Components.ToList())
            {
                if (component is MissingComponent missing)
                    _panel.Children.Add(BuildMissing(missing));
                else if (ComponentRegistry.IsCustom(component))
                    _panel.Children.Add(BuildComponent(component));
            }
        }

        private Control BuildMissing(MissingComponent missing)
        {
            var stack = new StackPanel();
            stack.Children.Add(Header(missing.DisplayName, missing));
            stack.Children.Add(Field("Tipo não carregado: compile o jogo e confira as DLLs do projeto (Arquivo › Componentes do jogo). Os dados são preservados."));
            return new Border { Classes = { "component" }, Child = stack };
        }

        private Control BuildComponent(SceneComponent component)
        {
            var stack = new StackPanel();
            stack.Children.Add(Header(component.DisplayName, component));

            var enabled = new CheckBox { Content = "Ativo", IsChecked = component.Enabled };
            enabled.IsCheckedChanged += (_, _) => _editor.SetComponentEnabled(component, enabled.IsChecked == true);
            stack.Children.Add(enabled);

            foreach (var p in CustomComponentSerializer.GetProperties(component.GetType()))
            {
                var label = Field(Nicify(p.Name));
                if (p.Tooltip != null)
                    ToolTip.SetTip(label, p.Tooltip);
                stack.Children.Add(label);
                stack.Children.Add(EditorFor(component, p));
            }
            return new Border { Classes = { "component" }, Child = stack };
        }

        private Control Header(string title, SceneComponent component)
        {
            var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
            grid.Children.Add(new TextBlock { Classes = { "section" }, Text = title });
            var remove = new Button { Classes = { "remove" } };
            Grid.SetColumn(remove, 1);
            remove.Click += (_, _) => _editor.RemoveComponent(component);
            grid.Children.Add(remove);
            return grid;
        }

        private static TextBlock Field(string text) => new() { Classes = { "field" }, Text = text, TextWrapping = global::Avalonia.Media.TextWrapping.Wrap };

        private Control EditorFor(SceneComponent c, ComponentProperty p)
        {
            var type = Nullable.GetUnderlyingType(p.Type) ?? p.Type;
            var value = p.Get(c);

            if (type == typeof(bool))
            {
                var box = new CheckBox { IsChecked = value as bool? ?? false };
                box.IsCheckedChanged += (_, _) => _editor.SetCustomProperty(c, p, box.IsChecked == true);
                return box;
            }

            if (type.IsEnum)
            {
                var combo = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch, ItemsSource = Enum.GetNames(type), SelectedItem = value?.ToString() };
                combo.SelectionChanged += (_, _) =>
                {
                    if (combo.SelectedItem is string name)
                        _editor.SetCustomProperty(c, p, Enum.Parse(type, name));
                };
                return combo;
            }

            if (type == typeof(Vector2) || type == typeof(Point))
            {
                var v = type == typeof(Vector2) ? (Vector2)value! : ((Point)value!).ToVector2();
                var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,6,*") };
                var x = new TextBox { Text = Num(v.X) };
                var y = new TextBox { Text = Num(v.Y) };
                Commit(x, _ => Apply());
                Commit(y, _ => Apply());
                Grid.SetColumn(y, 2);
                grid.Children.Add(x);
                grid.Children.Add(y);
                void Apply()
                {
                    if (!TryNum(x.Text, out var fx) || !TryNum(y.Text, out var fy)) return;
                    _editor.SetCustomProperty(c, p, type == typeof(Vector2) ? new Vector2(fx, fy) : new Point((int)fx, (int)fy));
                }
                return grid;
            }

            if (type == typeof(Color))
            {
                return Commit(new TextBox { Text = CustomComponentSerializer.FormatColor((Color)value!), Watermark = "#RRGGBBAA" }, t =>
                {
                    if (CustomComponentSerializer.ParseColor(t.Text) is { } color)
                        _editor.SetCustomProperty(c, p, color);
                });
            }

            if (type == typeof(string) && p.AssetPath != null)
            {
                var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,4,Auto") };
                var text = Commit(new TextBox { Text = value as string ?? "" }, t => _editor.SetCustomProperty(c, p, t.Text ?? ""));
                var browse = new Button { Content = "…" };
                Grid.SetColumn(browse, 2);
                browse.Click += async (_, _) =>
                {
                    var path = await _pickFile(p.Name, p.AssetPath.Filter);
                    if (path == null) return;
                    var folder = _editor.Project.Project?.Folder;
                    if (folder != null)
                    {
                        var rel = System.IO.Path.GetRelativePath(folder, path);
                        if (!rel.StartsWith("..")) path = rel.Replace('\\', '/');
                    }
                    _editor.SetCustomProperty(c, p, path);
                };
                grid.Children.Add(text);
                grid.Children.Add(browse);
                return grid;
            }

            if (type == typeof(string))
                return Commit(new TextBox { Text = value as string ?? "" }, t => _editor.SetCustomProperty(c, p, t.Text ?? ""));

            if (IsNumber(type))
            {
                return Commit(new TextBox { Text = Convert.ToString(value, CultureInfo.InvariantCulture) }, t =>
                {
                    if (!double.TryParse(t.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)
                        && !double.TryParse(t.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out d))
                        return;
                    if (p.Range != null) d = Math.Clamp(d, p.Range.Min, p.Range.Max);
                    _editor.SetCustomProperty(c, p, Convert.ChangeType(d, type, CultureInfo.InvariantCulture));
                });
            }

            // Listas, arrays e objetos: edição como JSON.
            var json = JsonSerializer.Serialize(value, p.Type, CustomComponentSerializer.JsonOptions);
            return Commit(new TextBox { Text = json, AcceptsReturn = true, TextWrapping = global::Avalonia.Media.TextWrapping.Wrap, MinHeight = 48 }, t =>
            {
                try { _editor.SetCustomProperty(c, p, JsonSerializer.Deserialize(t.Text ?? "null", p.Type, CustomComponentSerializer.JsonOptions)); }
                catch (JsonException e) { DreamBit.Engine.Diagnostics.EngineLog.Warn($"{p.Name}: JSON inválido ({e.Message})"); }
            }, enterCommits: false);
        }

        /// <summary>Aplica a edição ao perder o foco (e no Enter, para campos de uma linha).</summary>
        private static TextBox Commit(TextBox box, Action<TextBox> apply, bool enterCommits = true)
        {
            box.LostFocus += (_, _) => apply(box);
            if (enterCommits)
                box.KeyDown += (_, e) => { if (e.Key == Key.Enter) { apply(box); e.Handled = true; } };
            return box;
        }

        private static bool IsNumber(Type t) =>
            t == typeof(int) || t == typeof(float) || t == typeof(double) || t == typeof(long) ||
            t == typeof(short) || t == typeof(byte) || t == typeof(uint) || t == typeof(decimal);

        private static string Num(float f) => f.ToString(CultureInfo.InvariantCulture);

        private static bool TryNum(string? s, out float f)
            => float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out f)
               || float.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out f);

        /// <summary>"MaxSpeed" → "Max Speed".</summary>
        private static string Nicify(string name)
        {
            var sb = new System.Text.StringBuilder(name.Length + 4);
            for (int i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
                    sb.Append(' ');
                sb.Append(name[i]);
            }
            return sb.ToString();
        }
    }
}
