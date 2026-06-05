namespace OrthodonticClinic.Api.Models
{
    public class Patient
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string? Email { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? GuardianFullName { get; set; }

        public string? GuardianPhone { get; set; }

        public string? GuardianEmail { get; set; }

        public List<Appointment> Appointments { get; set; } = new();
    }
}