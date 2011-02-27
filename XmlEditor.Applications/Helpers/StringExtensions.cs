namespace XmlEditor.Applications.Helpers
{
    // string extension class
    public static class StringExtension
    {
        // string extension method ToUpperFirstLetter
        public static string ToUpperFirstLetter(this string source) {
            if (string.IsNullOrEmpty(source)) return string.Empty;
            var letters = source.ToCharArray();
            letters[0] = char.ToUpper(letters[0]);
            return new string(letters);
        }
    }
}