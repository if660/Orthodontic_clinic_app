namespace OrthodonticClinic.Api.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Specialization { get; set; } = string.Empty;

        public List<Appointment> Appointments { get; set; } = new();
    }
}