using System.Text.RegularExpressions;

namespace Kombinado.Api.Utils
{
    public static class EmailUtils
    {
        private const string STUDENT_EMAIL_REGEX = @"^[\w-\.]+@estudante\.iftm\.edu\.br$";

        public static bool IsStudentEmail(string email)
        {
            return Regex.IsMatch(email, STUDENT_EMAIL_REGEX);
        }

        // Emails are stored and compared in a single canonical form (trimmed and lowercase)
        public static string Normalize(string? email)
        {
            return email?.Trim().ToLowerInvariant() ?? string.Empty;
        }
    }
}
