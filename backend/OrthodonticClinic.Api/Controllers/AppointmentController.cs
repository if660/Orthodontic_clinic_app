using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrthodonticClinic.Api.Data;
using OrthodonticClinic.Api.Models;

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

        [HttpGet("List")]
        public async Task<IActionResult> List()
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
                        PatientName = a.Patient != null
                            ? a.Patient.FirstName + " " + a.Patient.LastName
                            : null,
                        a.DoctorId,
                        DoctorName = a.Doctor != null
                            ? a.Doctor.FirstName + " " + a.Doctor.LastName
                            : null,
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
                return StatusCode(500, new
                {
                    message = "Wystąpił błąd podczas pobierania listy wizyt.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("Data/{id}")]
        public async Task<IActionResult> Data(int id)
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
                        PatientName = a.Patient != null
                            ? a.Patient.FirstName + " " + a.Patient.LastName
                            : null,
                        PatientPhone = a.Patient != null ? a.Patient.Phone : null,
                        PatientEmail = a.Patient != null ? a.Patient.Email : null,
                        a.DoctorId,
                        DoctorName = a.Doctor != null
                            ? a.Doctor.FirstName + " " + a.Doctor.LastName
                            : null,
                        DoctorSpecialization = a.Doctor != null ? a.Doctor.Specialization : null,
                        a.AppointmentDate,
                        a.Status,
                        a.VisitType,
                        a.Notes
                    })
                    .FirstOrDefaultAsync();

                if (appointment == null)
                {
                    return NotFound(new
                    {
                        message = "Nie znaleziono wizyty o podanym ID."
                    });
                }

                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Wystąpił błąd podczas pobierania danych wizyty.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add(Appointment appointment)
        {
            try
            {
                if (appointment.PatientId <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Nie wybrano pacjenta."
                    });
                }

                if (appointment.DoctorId <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Nie wybrano lekarza."
                    });
                }

                if (appointment.AppointmentDate == default)
                {
                    return BadRequest(new
                    {
                        message = "Nie podano daty wizyty."
                    });
                }

                if (string.IsNullOrWhiteSpace(appointment.VisitType))
                {
                    return BadRequest(new
                    {
                        message = "Nie podano typu wizyty."
                    });
                }

                var patientExists = await _context.Patients
                    .AnyAsync(p => p.Id == appointment.PatientId);

                if (!patientExists)
                {
                    return NotFound(new
                    {
                        message = "Nie znaleziono pacjenta o podanym ID."
                    });
                }

                var doctorExists = await _context.Doctors
                    .AnyAsync(d => d.Id == appointment.DoctorId);

                if (!doctorExists)
                {
                    return NotFound(new
                    {
                        message = "Nie znaleziono lekarza o podanym ID."
                    });
                }

                if (string.IsNullOrWhiteSpace(appointment.Status))
                {
                    appointment.Status = "Zaplanowana";
                }

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(Data), new { id = appointment.Id }, appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Wystąpił błąd podczas dodawania wizyty.",
                    error = ex.Message
                });
            }
        }

        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, Appointment updatedAppointment)
        {
            try
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointment == null)
                {
                    return NotFound(new
                    {
                        message = "Nie znaleziono wizyty o podanym ID."
                    });
                }

                if (updatedAppointment.PatientId <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Nie wybrano pacjenta."
                    });
                }

                if (updatedAppointment.DoctorId <= 0)
                {
                    return BadRequest(new
                    {
                        message = "Nie wybrano lekarza."
                    });
                }

                var patientExists = await _context.Patients
                    .AnyAsync(p => p.Id == updatedAppointment.PatientId);

                if (!patientExists)
                {
                    return NotFound(new
                    {
                        message = "Nie znaleziono pacjenta o podanym ID."
                    });
                }

                var doctorExists = await _context.Doctors
                    .AnyAsync(d => d.Id == updatedAppointment.DoctorId);

                if (!doctorExists)
                {
                    return NotFound(new
                    {
                        message = "Nie znaleziono lekarza o podanym ID."
                    });
                }

                if (updatedAppointment.AppointmentDate == default)
                {
                    return BadRequest(new
                    {
                        message = "Nie podano daty wizyty."
                    });
                }

                if (string.IsNullOrWhiteSpace(updatedAppointment.VisitType))
                {
                    return BadRequest(new
                    {
                        message = "Nie podano typu wizyty."
                    });
                }

                appointment.PatientId = updatedAppointment.PatientId;
                appointment.DoctorId = updatedAppointment.DoctorId;
                appointment.AppointmentDate = updatedAppointment.AppointmentDate;
                appointment.Status = string.IsNullOrWhiteSpace(updatedAppointment.Status)
                    ? "Zaplanowana"
                    : updatedAppointment.Status;
                appointment.VisitType = updatedAppointment.VisitType;
                appointment.Notes = updatedAppointment.Notes;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Wizyta została zaktualizowana.",
                    appointment
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Wystąpił błąd podczas edycji wizyty.",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointment == null)
                {
                    return NotFound(new
                    {
                        message = "Nie znaleziono wizyty o podanym ID."
                    });
                }

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Wizyta została usunięta."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Wystąpił błąd podczas usuwania wizyty.",
                    error = ex.Message
                });
            }
        }
    }
}