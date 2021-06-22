namespace NonogramRow
{
    public static class Extensions
    {
        public static int? FirstIndexOfDifferent<T>(this T[] source, T value, int searchAfter)
        {
            for (var i = searchAfter; i < source.Length; i++)
                if (!(source[i]?.Equals(value) ?? true))
                    return i;
            return null;
        }
    }
}
