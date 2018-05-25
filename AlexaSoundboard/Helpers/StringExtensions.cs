using System.Globalization;
using System.Text;

namespace AlexaSoundboard.Helpers
{
    public static class StringExtensions
    {
        public static string AsFileName(this string str)
        {
            return str.ToLower().Replace(" ", "");
        }

        public static string RemoveDiacritics(this string str)
        {
            var normalizedString = str.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark && !char.IsPunctuation(c))
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
