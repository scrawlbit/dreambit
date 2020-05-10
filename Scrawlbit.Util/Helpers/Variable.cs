namespace Scrawlbit.Util.Helpers
{
    public static class Variable
    {
        public static bool Set<T>(ref T variable, T value)
        {
            if (!Equals(value, variable))
            {
                variable = value;
                return true;
            }

            return false;
        }
    }
}
