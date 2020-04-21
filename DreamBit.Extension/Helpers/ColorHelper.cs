using System.Windows.Media;
using MonoGameColor = Microsoft.Xna.Framework.Color;

namespace DreamBit.Extension.Helpers
{
    public static class ColorHelper
    {
        public static bool AreEquals(this Color color, MonoGameColor xnaColor)
        {
            return color.R == xnaColor.R &&
                   color.G == xnaColor.G &&
                   color.B == xnaColor.B &&
                   color.A == xnaColor.A;
        }

        public static MonoGameColor ToMonoGameColor(this Color color)
        {
            return new MonoGameColor(color.R, color.G, color.B, color.A);
        }

        public static Color ToMediaColor(this MonoGameColor xnaColor)
        {
            return new Color
            {
                R = xnaColor.R,
                G = xnaColor.G,
                B = xnaColor.B,
                A = xnaColor.A
            };
        }
    }
}
