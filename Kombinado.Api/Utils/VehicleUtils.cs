using System.Text.RegularExpressions;

namespace Kombinado.Api.Utils
{
    public static class VehicleUtils
    {
        // Seats offered to passengers (the driver's seat is not counted)
        public const int MIN_TOTAL_SEATS = 1;
        public const int MAX_TOTAL_SEATS = 8;

        // Old format (ABC1234) or Mercosul format (ABC1D23)
        private const string PLATE_REGEX = @"^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$";

        public static bool IsValidTotalSeats(int? totalSeats)
        {
            return totalSeats is >= MIN_TOTAL_SEATS and <= MAX_TOTAL_SEATS;
        }

        // Plates are stored in uppercase without separators (e.g. "abc-1d23" -> "ABC1D23")
        public static string NormalizePlate(string? plate)
        {
            return Regex.Replace(plate ?? string.Empty, @"[\s-]", string.Empty).ToUpperInvariant();
        }

        public static bool IsValidPlate(string plate)
        {
            return Regex.IsMatch(plate, PLATE_REGEX);
        }
    }
}
