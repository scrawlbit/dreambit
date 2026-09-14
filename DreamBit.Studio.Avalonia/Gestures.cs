using System.Text;
using Avalonia.Input;

namespace DreamBit.Studio.Avalonia
{
    /// <summary>
    /// Formata um gesto de teclado (modificadores + tecla) numa string canônica como
    /// "Ctrl+Shift+G", usada para casar com os atalhos configuráveis das preferências.
    /// </summary>
    public static class Gestures
    {
        public static string Format(KeyModifiers modifiers, Key key)
        {
            if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift
                    or Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin or Key.None)
                return string.Empty; // só o modificador, ainda não é um gesto

            var sb = new StringBuilder();
            if (modifiers.HasFlag(KeyModifiers.Control)) sb.Append("Ctrl+");
            if (modifiers.HasFlag(KeyModifiers.Shift)) sb.Append("Shift+");
            if (modifiers.HasFlag(KeyModifiers.Alt)) sb.Append("Alt+");
            sb.Append(NormalizeKey(key));
            return sb.ToString();
        }

        // Normaliza teclas equivalentes (NumPad0 == D0) para o gesto padrão.
        private static string NormalizeKey(Key key) => key switch
        {
            Key.NumPad0 => "D0",
            _ => key.ToString()
        };
    }
}
