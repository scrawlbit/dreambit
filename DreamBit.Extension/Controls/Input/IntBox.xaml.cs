using DreamBit.General.State;
using Scrawlbit.Presentation.Dependency;

namespace DreamBit.Extension.Controls.Input
{
    public partial class IntBox
    {
        public delegate void IntBoxEventHandler(IntBox sender, ValueChangedEventArgs<int> e);
        public static readonly DependencyProperty<IntBox, int> ValueProperty;

        static IntBox()
        {
            var registry = new DependencyRegistry<IntBox>();

            ValueProperty = registry.Property(p => p.Value);
        }
        public IntBox()
        {
            InitializeComponent();
        }

        public event IntBoxEventHandler Changed;
        public int Value
        {
            get => ValueProperty.Get(this);
            set => ValueProperty.Set(this, value);
        }
        public int Increment
        {
            get => (int)Input.Increment;
            set => Input.Increment = (int)value;
        }

        private void OnInputChanged(FloatBox sender, ValueChangedEventArgs<float> e)
        {
            Changed?.Invoke(this, ((int)e.OldValue, (int)e.NewValue));
        }
    }
}
