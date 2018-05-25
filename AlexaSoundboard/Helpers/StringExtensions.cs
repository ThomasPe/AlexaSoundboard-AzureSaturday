namespace AlexaSoundboard.Helpers
{
    public static class StringExtensions
    {
        public static string AsFileName(this string str)
        {
            return str.ToLower().Replace(" ", "");
        }
    }
}
