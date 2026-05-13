namespace OrthodonticClinic.Api.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        public int PatientId { get; set; }

        public Patient? Patient { get; set; }

        public int DoctorId { get; set; }

        public Doctor? Doctor { get; set; }

        public DateTime AppointmentDate { get; set; }

        public string Status { get; set; } = "Zaplanowana";

        public string VisitType { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}