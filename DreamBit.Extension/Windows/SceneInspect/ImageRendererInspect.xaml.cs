﻿using DreamBit.Extension.Controls.Input;
using DreamBit.Game.Elements.Components;
using DreamBit.Game.Helpers;
using DreamBit.General.State;
using Microsoft.Xna.Framework;

namespace DreamBit.Extension.Windows.SceneInspect
{
    public partial class ImageRendererInspect
    {
        public ImageRendererInspect()
        {
            InitializeComponent();
        }

        private ImageRenderer ImageRenderer
        {
            get => (ImageRenderer)DataContext;
        }

        private void OnFlipHorizontallyChanged(CheckBox sender, ValueChangedEventArgs<bool?> e)
        {
            string description = $"{ImageRenderer.GameObject.Name}'s image renderer flipped horizontally";
            IStateCommand command = ImageRenderer.State().SetProperty(i => i.FlipHorizontally, e.AsBoolean(), description);

            ViewModel.State.Add(command);
        }
        private void OnFlipVerticallyChanged(CheckBox sender, ValueChangedEventArgs<bool?> e)
        {
            string description = $"{ImageRenderer.GameObject.Name}'s image renderer flipped vertically";
            IStateCommand command = ImageRenderer.State().SetProperty(i => i.FlipVertically, e.AsBoolean(), description);

            ViewModel.State.Add(command);
        }
        private void OnOriginChanged(FloatBox sender, ValueChangedEventArgs<float> e)
        {
            var change = e.AsVectorChange(ImageRenderer.Origin, sender == OriginX);
            string value = change.NewValue.Text();
            string description = $"{ImageRenderer.GameObject.Name}'s image renderer origin changed to {value}";
            IStateCommand command = ImageRenderer.State().SetProperty(i => i.Origin, change, description);

            ViewModel.State.Add(command);
        }
    }
}
