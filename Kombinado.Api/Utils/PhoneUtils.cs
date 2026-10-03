namespace Kombinado.Api.Utils
{
    public static class PhoneUtils
    {
        // WhatsApp numbers are stored as digits only: area code (DDD) + number, without +55
        public static string NormalizeWhatsApp(string? whatsApp)
        {
            return new string((whatsApp ?? string.Empty).Where(char.IsDigit).ToArray());
        }

        // Landline (10 digits) or mobile (11 digits) number with area code
        public static bool IsValidWhatsApp(string whatsApp)
        {
            return whatsApp.Length is 10 or 11;
        }
    }
}
