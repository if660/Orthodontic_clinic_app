namespace OrthodonticClinic.Api.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Specialization { get; set; } = string.Empty;

        public string LicenseNumber { get; set; } = string.Empty;

        public string AvailableDays { get; set; } = string.Empty;

        public string? AvailabilityStart { get; set; }

        public string? AvailabilityEnd { get; set; }

        public string? ProfileNote { get; set; }

        public List<Appointment> Appointments { get; set; } = new();
    }
}