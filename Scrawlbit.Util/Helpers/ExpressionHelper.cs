using System.Linq.Expressions;

namespace Scrawlbit.Helpers
{
    public static class ExpressionHelper
    {
        public static string GetExpressionText(LambdaExpression expression)
        {
            var text = string.Empty;

            var member = expression.Body as MemberExpression;
            while (member != null)
            {
                if (text != string.Empty)
                    text = "." + text;

                text = member.Member.Name + text;

                member = member.Expression as MemberExpression;
            }


            return text;
        }
    }
}