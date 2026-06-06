using OrthodonticClinic.Api.Models;

namespace OrthodonticClinic.Api.Services
{
    public static class DoctorAvailabilityService
    {
        private static readonly Dictionary<DayOfWeek, string> DayLabels = new()
        {
            [DayOfWeek.Monday] = "poniedziałek",
            [DayOfWeek.Tuesday] = "wtorek",
            [DayOfWeek.Wednesday] = "środę",
            [DayOfWeek.Thursday] = "czwartek",
            [DayOfWeek.Friday] = "piątek",
            [DayOfWeek.Saturday] = "sobotę",
            [DayOfWeek.Sunday] = "niedzielę",
        };

        public static bool IsAvailable(Doctor doctor, DateTime appointmentDate, out string? reason)
        {
            var availableDays = doctor.AvailableDays
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (availableDays.Length == 0)
            {
                reason = "Lekarz nie ma ustawionych dni dyspozycyjności.";
                return false;
            }

            var dayName = appointmentDate.DayOfWeek.ToString();
            if (!availableDays.Contains(dayName, StringComparer.OrdinalIgnoreCase))
            {
                var readableDays = string.Join(", ", availableDays);
                reason = $"Lekarz nie przyjmuje w {DayLabels[appointmentDate.DayOfWeek]}. Dostępne dni: {readableDays}.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(doctor.AvailabilityStart) &&
                !string.IsNullOrWhiteSpace(doctor.AvailabilityEnd))
            {
                if (!TimeSpan.TryParse(doctor.AvailabilityStart, out var start) ||
                    !TimeSpan.TryParse(doctor.AvailabilityEnd, out var end))
                {
                    reason = "Nie udało się odczytać godzin dyspozycyjności lekarza.";
                    return false;
                }

                var time = appointmentDate.TimeOfDay;
                if (time < start || time >= end)
                {
                    reason = $"Wybrana godzina jest poza godzinami pracy lekarza ({doctor.AvailabilityStart}–{doctor.AvailabilityEnd}).";
                    return false;
                }
            }

            reason = null;
            return true;
        }
    }
}
