using Scrawlbit.Presentation.Converters;

namespace DreamBit.Extension.Converters
{
    internal class VisibleIfNotNullConverter : ValueConverterGroupMarkup
    {
        public VisibleIfNotNullConverter()
        {
            Add(new IsNullConverter());
            Add(new InverseBooleanConverter());
            Add(new VisibilityConverter());
        }
    }
}
