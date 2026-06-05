using Microsoft.EntityFrameworkCore;
using OrthodonticClinic.Api.Models;

namespace OrthodonticClinic.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }

        public DbSet<Doctor> Doctors { get; set; }

        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<PatientDocument> PatientDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    Id = 1,
                    FirstName = "Anna",
                    LastName = "Kowalska",
                    Phone = "500600700",
                    Email = "anna.kowalska@example.com",
                    BirthDate = new DateTime(2001, 4, 12),
                    GuardianFullName = null,
                    GuardianPhone = null,
                    GuardianEmail = null
                },
                new Patient
                {
                    Id = 2,
                    FirstName = "Jan",
                    LastName = "Wiśniewski",
                    Phone = "600700800",
                    Email = "jan.wisniewski@example.com",
                    BirthDate = new DateTime(1998, 9, 25),
                    GuardianFullName = null,
                    GuardianPhone = null,
                    GuardianEmail = null
                }
            );

            modelBuilder.Entity<Doctor>().HasData(
                new Doctor
                {
                    Id = 1,
                    FirstName = "Marta",
                    LastName = "Nowak",
                    Specialization = "Ortodoncja dziecięca",
                    LicenseNumber = "ORTO-PL-1001",
                    AvailableDays = "Monday,Tuesday,Wednesday,Thursday",
                    AvailabilityStart = "08:00",
                    AvailabilityEnd = "15:00",
                    ProfileNote = "Specjalizuje się w leczeniu dzieci i młodzieży."
                },
                new Doctor
                {
                    Id = 2,
                    FirstName = "Adam",
                    LastName = "Zieliński",
                    Specialization = "Ortodoncja dorosłych",
                    LicenseNumber = "ORTO-PL-1002",
                    AvailableDays = "Monday,Wednesday,Friday",
                    AvailabilityStart = "10:00",
                    AvailabilityEnd = "18:00",
                    ProfileNote = "Prowadzi skomplikowane przypadki ortodontyczne dorosłych."
                }
            );

            modelBuilder.Entity<Appointment>().HasData(
                new Appointment
                {
                    Id = 1,
                    PatientId = 1,
                    DoctorId = 1,
                    AppointmentDate = new DateTime(2026, 6, 10, 14, 30, 0),
                    Status = "Zaplanowana",
                    VisitType = "Kontrola aparatu",
                    Notes = "Pacjent zgłasza lekki ból przy jedzeniu."
                },
                new Appointment
                {
                    Id = 2,
                    PatientId = 2,
                    DoctorId = 2,
                    AppointmentDate = new DateTime(2026, 6, 11, 9, 0, 0),
                    Status = "Zaplanowana",
                    VisitType = "Pierwsza konsultacja",
                    Notes = "Pierwsza wizyta pacjenta."
                }
            );
        }
    }
}