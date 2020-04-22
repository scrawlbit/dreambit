using DreamBit.Extension.Controls.Input;
using DreamBit.Game.Content;
using DreamBit.Game.Elements.Components;
using DreamBit.Game.Helpers;
using DreamBit.General.State;

namespace DreamBit.Extension.Windows.SceneInspect
{
    public partial class TextRendererInspect
    {
        public TextRendererInspect()
        {
            InitializeComponent();
        }

        private TextRenderer TextRenderer
        {
            get => (TextRenderer)DataContext;
        }

        private void OnFontChanged(ContentSelector sender, ValueChangedEventArgs<IContent> e)
        {
            string description = $"{TextRenderer.GameObject.Name}'s text renderer file changed to \"{e.NewValue.File.Name}\"";
            IStateCommand command = TextRenderer.State().SetProperty(t => t.Font, e, description);

            ViewModel.State.Add(command);
        }
        private void OnTextChanged(TextBox sender, ValueChangedEventArgs<string> e)
        {
            string description = $"{TextRenderer.GameObject.Name}'s text renderer text changed to {e.NewValue}";
            IStateCommand command = TextRenderer.State().SetProperty(t => t.Text, e, description);

            ViewModel.State.Add(command);
        }
        private void OnColorChanged(ColorPicker sender, ColorPickerChangedEventArgs e)
        {
            string description = $"{TextRenderer.GameObject.Name}'s text renderer color changed to {e.ColorName}";
            IStateCommand command = TextRenderer.State().SetProperty(t => t.Color, e, description);

            ViewModel.State.Add(command);
        }
        private void OnFlipHorizontallyChanged(CheckBox sender, ValueChangedEventArgs<bool?> e)
        {
            string description = $"{TextRenderer.GameObject.Name}'s text renderer flipped horizontally";
            IStateCommand command = TextRenderer.State().SetProperty(t => t.FlipHorizontally, e.AsBoolean(), description);

            ViewModel.State.Add(command);
        }
        private void OnFlipVerticallyChanged(CheckBox sender, ValueChangedEventArgs<bool?> e)
        {
            string description = $"{TextRenderer.GameObject.Name}'s text renderer flipped vertically";
            IStateCommand command = TextRenderer.State().SetProperty(t => t.FlipVertically, e.AsBoolean(), description);

            ViewModel.State.Add(command);
        }
        private void OnOriginChanged(FloatBox sender, ValueChangedEventArgs<float> e)
        {
            var change = e.AsVectorChange(TextRenderer.Origin, sender == OriginX);
            string value = change.NewValue.Text();
            string description = $"{TextRenderer.GameObject.Name}'s text renderer origin changed to {value}";
            IStateCommand command = TextRenderer.State().SetProperty(t => t.Origin, change, description);

            ViewModel.State.Add(command);
        }
    }
}
