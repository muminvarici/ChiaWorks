namespace ChiaWorks.FileChecker.Extensions
{
    public static class IntegerExtensions
    {
        public static int NonZero(this int value, int defaultValue)
        {
            return value > 0 ? value : defaultValue;
        }
    }
}