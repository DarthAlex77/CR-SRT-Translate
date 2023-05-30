namespace CR_SRT_Translate.Models
{
    public static class Extensions
    {
        public static bool IsNullOrWhiteSpace(this string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }
    }
}