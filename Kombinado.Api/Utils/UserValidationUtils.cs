namespace Kombinado.Api.Utils
{
    // Shared rules for user data (signup and profile update). Each method returns the first error message or null.
    public static class UserValidationUtils
    {
        public const int MAX_NAME_LENGTH = 100;
        public const int MAX_COURSE_LENGTH = 100;
        public const int MAX_VEHICLE_MODEL_LENGTH = 50;
        public const int MAX_VEHICLE_COLOR_LENGTH = 30;

        // Expects normalized input (trimmed name/course, WhatsApp with digits only)
        public static string? ValidatePersonalData(string? name, string? whatsApp, string? course)
        {
            if (string.IsNullOrEmpty(name) || name.Length > MAX_NAME_LENGTH)
            {
                return $"O nome é obrigatório e deve ter no máximo {MAX_NAME_LENGTH} caracteres.";
            }

            if (!PhoneUtils.IsValidWhatsApp(whatsApp ?? string.Empty))
            {
                return "Informe um WhatsApp válido com DDD (10 ou 11 dígitos, sem o +55).";
            }

            if (string.IsNullOrEmpty(course) || course.Length > MAX_COURSE_LENGTH)
            {
                return $"O curso é obrigatório e deve ter no máximo {MAX_COURSE_LENGTH} caracteres.";
            }

            return null;
        }

        // Expects normalized input (trimmed model/color, plate from VehicleUtils.NormalizePlate)
        public static string? ValidateVehicleData(string? model, string? color, string? plate, int? totalSeats)
        {
            if (string.IsNullOrEmpty(model) ||
                string.IsNullOrEmpty(color) ||
                string.IsNullOrEmpty(plate))
            {
                return "Motoristas precisam informar o Modelo, Cor e Placa do veículo.";
            }

            if (model.Length > MAX_VEHICLE_MODEL_LENGTH)
            {
                return $"O modelo do veículo deve ter no máximo {MAX_VEHICLE_MODEL_LENGTH} caracteres.";
            }

            if (color.Length > MAX_VEHICLE_COLOR_LENGTH)
            {
                return $"A cor do veículo deve ter no máximo {MAX_VEHICLE_COLOR_LENGTH} caracteres.";
            }

            if (!VehicleUtils.IsValidPlate(plate))
            {
                return "Placa inválida. Use o formato ABC1234 ou ABC1D23.";
            }

            if (!VehicleUtils.IsValidTotalSeats(totalSeats))
            {
                return $"O número de vagas do veículo deve ser entre {VehicleUtils.MIN_TOTAL_SEATS} e {VehicleUtils.MAX_TOTAL_SEATS}.";
            }

            return null;
        }
    }
}
