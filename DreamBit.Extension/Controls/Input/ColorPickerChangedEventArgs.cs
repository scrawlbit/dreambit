using DreamBit.General.State;
using Microsoft.Xna.Framework;

namespace DreamBit.Extension.Controls.Input
{
    public class ColorPickerChangedEventArgs : ValueChangedEventArgs<Color>
    {
        public ColorPickerChangedEventArgs(Color oldValue, Color newValue, string colorName)
        {
            OldValue = oldValue;
            NewValue = newValue;
            ColorName = colorName;
        }

        public string ColorName { get; set; }
    }
}
