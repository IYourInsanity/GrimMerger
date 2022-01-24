namespace GrimMerger.Extensions
{
    internal static class StringExtension
    {
        internal static string Forge(this string blank, params string[] args)
        {
            return string.Format(blank, args);
        }
    }
}
