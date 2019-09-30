using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ScrawlBit.Helpers;

namespace ScrawlBit.Presentation.Helpers
{
    public static class KeyboardHelper
    {
        public static bool IsCtrlPressed()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        }
        public static bool IsShiftPressed()
        {
            return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        }
        public static bool IsAltPressed()
        {
            return Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
        }

        public static bool KeyIs(this KeyEventArgs args, params Key[] key)
        {
            var keys = new[] { args.SystemKey, args.Key };

            return keys.Any(k => k.EqualsAny(key));
        }
        public static bool KeyIsShift(this KeyEventArgs args)
        {
            return args.KeyIs(Key.LeftShift, Key.RightShift);
        }
        public static bool KeyIsAlt(this KeyEventArgs args)
        {
            return args.KeyIs(Key.LeftAlt, Key.RightAlt);
        }

        public static string GenerateToolTip(Key key, ModifierKeys modifierKeys = ModifierKeys.None)
        {
            if (key == Key.None)
                return null;

            var keys = new List<string>();

            if (modifierKeys != ModifierKeys.None)
            {
                var modifiers = new [] { ModifierKeys.Control, ModifierKeys.Windows, ModifierKeys.Alt, ModifierKeys.Shift };
                var usedModifiers = modifiers.Where(m => modifierKeys.HasFlag(m));

                keys.AddRange(usedModifiers.Select(m => m == ModifierKeys.Control ? "Ctrl" : m.ToString()));
            }

            keys.Add(key.ToString());

            return string.Join("+", keys);
        }
    }
}