﻿using System;
using System.Globalization;
using System.Linq;

namespace Scrawlbit.Presentation.Converters
{
    public class OrConverter : ConverterMarkupExtension
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var result = values?.Any(v => Equals(v, true)) ?? false;
            return result;
        }
    }
}
