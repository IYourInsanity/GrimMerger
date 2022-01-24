namespace GrimMerger.Helpers
{
    internal static class StringHelper
    {
        internal static string Forge(this string blank, params string[] args)
        {
            return string.Format(blank, args);
        }
    }
}
