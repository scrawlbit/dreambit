using Scrawlbit.Presentation.Converters;

namespace DreamBit.Extension.Converters
{
    internal class VisibleIfNotNullConverter : ValueConverterGroup
    {
        public VisibleIfNotNullConverter()
        {
            Add(new IsNullConverter());
            Add(new InverseBooleanConverter());
            Add(new VisibilityConverter());
        }
    }
}
