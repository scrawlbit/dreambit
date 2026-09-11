using System.Windows;
using System.Windows.Media;
using DreamBit.Engine.Rendering;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace DreamBit.Studio
{
    /// <summary>Alterna o tema claro/escuro do editor (chrome WPF + cores do canvas).</summary>
    internal static class ThemeManager
    {
        public static bool IsDark { get; private set; } = true;

        public static void Apply(bool dark, SceneRenderer renderer)
        {
            IsDark = dark;
            var r = Application.Current.Resources;

            if (dark)
            {
                Set(r, "WindowBg", "#1E2128");
                Set(r, "PanelBg", "#252932");
                Set(r, "FieldBg", "#2A2E38");
                Set(r, "TextFg", "#D6DBE6");
                Set(r, "MutedFg", "#8A93A6");
                Set(r, "BorderBg", "#11141A");
                Set(r, "AccentBg", "#2F3440");

                renderer.Background = new XnaColor(24, 26, 32);
                renderer.GridColor = new XnaColor(44, 48, 58);
                renderer.GridAxisColor = new XnaColor(90, 96, 110);
            }
            else
            {
                Set(r, "WindowBg", "#F3F4F6");
                Set(r, "PanelBg", "#FFFFFF");
                Set(r, "FieldBg", "#FFFFFF");
                Set(r, "TextFg", "#1E2128");
                Set(r, "MutedFg", "#6B7280");
                Set(r, "BorderBg", "#D1D5DB");
                Set(r, "AccentBg", "#E5E7EB");

                renderer.Background = new XnaColor(238, 240, 244);
                renderer.GridColor = new XnaColor(214, 218, 224);
                renderer.GridAxisColor = new XnaColor(150, 156, 166);
            }
        }

        public static void Toggle(SceneRenderer renderer) => Apply(!IsDark, renderer);

        private static void Set(ResourceDictionary resources, string key, string hex)
            => resources[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
    }
}
