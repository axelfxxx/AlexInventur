namespace InventurApp.Services
{
    public static class BarcodeService
    {
        public static string Normalize(string input)
            => new string((input ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).Trim();

        public static bool LooksLikeBarcode(string input)
        {
            var value = Normalize(input);
            return value.Length >= 6 && value.Length <= 32;
        }
    }
}
