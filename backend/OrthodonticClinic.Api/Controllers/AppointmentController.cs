using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrthodonticClinic.Api.Data;
using OrthodonticClinic.Api.Models;
using OrthodonticClinic.Api.Services;

namespace OrthodonticClinic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Select(a => new
                    {
                        a.Id,
                        a.PatientId,
                        patientName = a.Patient != null ? a.Patient.FirstName + " " + a.Patient.LastName : "",
                        a.DoctorId,
                        doctorName = a.Doctor != null ? a.Doctor.FirstName + " " + a.Doctor.LastName : "",
                        a.AppointmentDate,
                        a.Status,
                        a.VisitType,
                        a.Notes
                    })
                    .ToListAsync();

                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Błąd podczas pobierania wizyt.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Where(a => a.Id == id)
                    .Select(a => new
                    {
                        a.Id,
                        a.PatientId,
                        patientName = a.Patient != null ? a.Patient.FirstName + " " + a.Patient.LastName : "",
                        a.DoctorId,
                        doctorName = a.Doctor != null ? a.Doctor.FirstName + " " + a.Doctor.LastName : "",
                        a.AppointmentDate,
                        a.Status,
                        a.VisitType,
                        a.Notes
                    })
                    .FirstOrDefaultAsync();

                if (appointment == null)
                    return NotFound(new { message = "Wizyta nie istnieje." });

                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Błąd podczas pobierania wizyty.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            try
            {
                var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);
                if (doctor == null)
                    return BadRequest(new { message = "Wybrany lekarz nie istnieje." });

                if (!DoctorAvailabilityService.IsAvailable(doctor, appointment.AppointmentDate, out var reason))
                    return BadRequest(new { message = reason });

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Błąd podczas dodawania wizyty.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Appointment updated)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                    return NotFound(new { message = "Wizyta nie istnieje." });

                var doctor = await _context.Doctors.FindAsync(updated.DoctorId);
                if (doctor == null)
                    return BadRequest(new { message = "Wybrany lekarz nie istnieje." });

                if (!DoctorAvailabilityService.IsAvailable(doctor, updated.AppointmentDate, out var reason))
                    return BadRequest(new { message = reason });

                appointment.PatientId = updated.PatientId;
                appointment.DoctorId = updated.DoctorId;
                appointment.AppointmentDate = updated.AppointmentDate;
                appointment.Status = updated.Status;
                appointment.VisitType = updated.VisitType;
                appointment.Notes = updated.Notes;

                await _context.SaveChangesAsync();
                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Błąd podczas aktualizacji wizyty.", error = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                    return NotFound(new { message = "Wizyta nie istnieje." });

                appointment.Status = dto.Status;
                await _context.SaveChangesAsync();
                return Ok(new { appointment.Id, appointment.Status });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Błąd podczas aktualizacji statusu.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                    return NotFound(new { message = "Wizyta nie istnieje." });

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Wizyta została usunięta." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Błąd podczas usuwania wizyty.", error = ex.Message });
            }
        }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
