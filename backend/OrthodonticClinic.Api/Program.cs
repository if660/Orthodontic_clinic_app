using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrthodonticClinic.Api.Data;
using OrthodonticClinic.Api.Models;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
    SeedInitialData(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowVueApp");

app.UseAuthorization();

app.MapControllers();

app.Run();

static void SeedInitialData(AppDbContext dbContext)
{
    if (!dbContext.Doctors.Any(d => d.LicenseNumber == "ORTO-PL-1003"))
    {
        dbContext.Doctors.AddRange(
            new Doctor
            {
                FirstName = "Ewa",
                LastName = "Grabowska",
                Specialization = "Ortodoncja estetyczna",
                LicenseNumber = "ORTO-PL-1003",
                AvailableDays = "Tuesday,Wednesday,Thursday",
                AvailabilityStart = "09:00",
                AvailabilityEnd = "17:00",
                ProfileNote = "Skupia się na estetycznych rozwiązaniach z aparatami przezroczystymi."
            },
            new Doctor
            {
                FirstName = "Grzegorz",
                LastName = "Nowicki",
                Specialization = "Ortodoncja dorosłych",
                LicenseNumber = "ORTO-PL-1004",
                AvailableDays = "Monday,Wednesday,Friday",
                AvailabilityStart = "08:30",
                AvailabilityEnd = "16:00",
                ProfileNote = "Doświadczenie w leczeniu dorosłych pacjentów z kompleksowymi przypadkami."
            },
            new Doctor
            {
                FirstName = "Magdalena",
                LastName = "Kaczmarek",
                Specialization = "Ortodoncja dziecięca",
                LicenseNumber = "ORTO-PL-1005",
                AvailableDays = "Monday,Tuesday,Thursday,Friday",
                AvailabilityStart = "10:00",
                AvailabilityEnd = "18:00",
                ProfileNote = "Specjalistka w pracy z dziećmi i młodzieżą, nastawiona na komfort pacjenta."
            }
        );
    }

    if (!dbContext.Patients.Any(p => p.Email == "ola.jankowska@example.com"))
    {
        dbContext.Patients.AddRange(
            new Patient
            {
                FirstName = "Aleksandra",
                LastName = "Jankowska",
                Phone = "501234567",
                Email = "ola.jankowska@example.com",
                BirthDate = new DateTime(2012, 5, 28),
                GuardianFullName = "Katarzyna Jankowska",
                GuardianPhone = "501234568",
                GuardianEmail = "katarzyna.jankowska@example.com"
            },
            new Patient
            {
                FirstName = "Tomasz",
                LastName = "Malinowski",
                Phone = "502345678",
                Email = "tomasz.malinowski@example.com",
                BirthDate = new DateTime(1985, 11, 14),
                GuardianFullName = null,
                GuardianPhone = null,
                GuardianEmail = null
            },
            new Patient
            {
                FirstName = "Martyna",
                LastName = "Szymańska",
                Phone = "503456789",
                Email = "martyna.szymanska@example.com",
                BirthDate = new DateTime(1996, 7, 3),
                GuardianFullName = null,
                GuardianPhone = null,
                GuardianEmail = null
            },
            new Patient
            {
                FirstName = "Oskar",
                LastName = "Kubiak",
                Phone = "504567890",
                Email = "oskar.kubiak@example.com",
                BirthDate = new DateTime(2009, 12, 6),
                GuardianFullName = "Anna Kubiak",
                GuardianPhone = "504567891",
                GuardianEmail = "anna.kubiak@example.com"
            }
        );
    }

    dbContext.SaveChanges();

    if (!dbContext.Appointments.Any())
    {
        var doctorMarta = dbContext.Doctors.FirstOrDefault(d => d.LicenseNumber == "ORTO-PL-1001");
        var doctorAdam = dbContext.Doctors.FirstOrDefault(d => d.LicenseNumber == "ORTO-PL-1002");
        var doctorEwa = dbContext.Doctors.FirstOrDefault(d => d.LicenseNumber == "ORTO-PL-1003");
        var doctorGrzegorz = dbContext.Doctors.FirstOrDefault(d => d.LicenseNumber == "ORTO-PL-1004");
        var doctorMagdalena = dbContext.Doctors.FirstOrDefault(d => d.LicenseNumber == "ORTO-PL-1005");

        var patientAnna = dbContext.Patients.FirstOrDefault(p => p.Email == "anna.kowalska@example.com");
        var patientJan = dbContext.Patients.FirstOrDefault(p => p.Email == "jan.wisniewski@example.com");
        var patientOla = dbContext.Patients.FirstOrDefault(p => p.Email == "ola.jankowska@example.com");
        var patientTomasz = dbContext.Patients.FirstOrDefault(p => p.Email == "tomasz.malinowski@example.com");

        if (doctorMarta != null && patientAnna != null)
        {
            dbContext.Appointments.Add(new Appointment
            {
                PatientId = patientAnna.Id,
                DoctorId = doctorMarta.Id,
                AppointmentDate = new DateTime(2026, 6, 10, 14, 30, 0),
                Status = "Zaplanowana",
                VisitType = "Kontrola aparatu",
                Notes = "Pacjent zgłasza lekki ból przy jedzeniu."
            });
        }

        if (doctorAdam != null && patientJan != null)
        {
            dbContext.Appointments.Add(new Appointment
            {
                PatientId = patientJan.Id,
                DoctorId = doctorAdam.Id,
                AppointmentDate = new DateTime(2026, 6, 11, 9, 0, 0),
                Status = "Zaplanowana",
                VisitType = "Pierwsza konsultacja",
                Notes = "Pierwsza wizyta pacjenta."
            });
        }

        if (doctorEwa != null && patientOla != null)
        {
            dbContext.Appointments.Add(new Appointment
            {
                PatientId = patientOla.Id,
                DoctorId = doctorEwa.Id,
                AppointmentDate = new DateTime(2026, 6, 13, 10, 0, 0),
                Status = "Zaplanowana",
                VisitType = "Wizyta kontrolna",
                Notes = "Kontrola postępów leczenia."
            });
        }

        if (doctorMagdalena != null && patientTomasz != null)
        {
            dbContext.Appointments.Add(new Appointment
            {
                PatientId = patientTomasz.Id,
                DoctorId = doctorMagdalena.Id,
                AppointmentDate = new DateTime(2026, 6, 15, 11, 30, 0),
                Status = "Zaplanowana",
                VisitType = "Aparat stały",
                Notes = "Pacjent nowy, planowane założenie aparatu stałego."
            });
        }

        dbContext.SaveChanges();
    }
}
