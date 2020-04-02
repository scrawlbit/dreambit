using Scrawlbit.Helpers;
using System;
using System.Linq.Expressions;
using System.Windows.Data;

namespace Scrawlbit.Presentation.Helpers
{
    public static class BindingHelper
    {
        public static Binding SourcePath<T, TPath>(T source, Expression<Func<T, TPath>> path)
        {
            return new Binding(path.GetExpressionText()) { Source = source };
        }
    }
}
