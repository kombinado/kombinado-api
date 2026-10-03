namespace Kombinado.Api.Utils
{
    public static class VehicleUtils
    {
        // Seats offered to passengers (the driver's seat is not counted)
        public const int MIN_TOTAL_SEATS = 1;
        public const int MAX_TOTAL_SEATS = 8;

        public static bool IsValidTotalSeats(int? totalSeats)
        {
            return totalSeats is >= MIN_TOTAL_SEATS and <= MAX_TOTAL_SEATS;
        }
    }
}
