using System.Globalization;

namespace Kombinado.Api.Utils
{
    public static class DateTimeUtils
    {
        private const string BRASILIA_TIME_ZONE_ID = "America/Sao_Paulo";
        
        private static readonly TimeZoneInfo BrasiliaTimeZone =
            TimeZoneInfo.TryFindSystemTimeZoneById(BRASILIA_TIME_ZONE_ID, out TimeZoneInfo? timeZone)
                ? timeZone
                : TimeZoneInfo.CreateCustomTimeZone(BRASILIA_TIME_ZONE_ID, TimeSpan.FromHours(-3), "Brasília", "Brasília");

        // Business rule: dates without an explicit offset are Brasília local time
        public static DateTime ToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(), // JSON with offset (e.g. "-03:00")
                _ => TimeZoneInfo.ConvertTimeToUtc(value, BrasiliaTimeZone) // JSON without offset
            };
        }

        // Formats a UTC date for API messages, e.g. "17/12 às 18:30"
        public static string ToBrasiliaDisplay(DateTime utc)
        {
            DateTime brasilia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), BrasiliaTimeZone);
            return brasilia.ToString("dd/MM 'às' HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
