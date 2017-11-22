namespace AspNetFileUpload.Helpers
{
    public static class StringHelpers
    {
        public static int ToInt(this string value, int defaultValue)
        {
            int i;
            return int.TryParse(value, out i) ? i : defaultValue;
        }

        public static string Default(this string value, string defaultValue)
        {
            return (string.IsNullOrEmpty(value)) ? defaultValue : value;
        }
    }
}