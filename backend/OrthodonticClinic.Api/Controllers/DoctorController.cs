using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrthodonticClinic.Api.Data;
using OrthodonticClinic.Api.Models;

namespace OrthodonticClinic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var doctors = await _context.Doctors.ToListAsync();

                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Błąd podczas pobierania lekarzy.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (doctor == null)
                {
                    return NotFound(new
                    {
                        message = "Lekarz nie istnieje."
                    });
                }

                return Ok(doctor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Błąd podczas pobierania lekarza.",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Doctor doctor)
        {
            try
            {
                _context.Doctors.Add(doctor);

                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = doctor.Id },
                    doctor
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Błąd podczas dodawania lekarza.",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Doctor updatedDoctor)
        {
            try
            {
                var doctor = await _context.Doctors.FindAsync(id);

                if (doctor == null)
                {
                    return NotFound(new
                    {
                        message = "Lekarz nie istnieje."
                    });
                }

                doctor.FirstName = updatedDoctor.FirstName;
                doctor.LastName = updatedDoctor.LastName;
                doctor.Specialization = updatedDoctor.Specialization;
                doctor.LicenseNumber = updatedDoctor.LicenseNumber;
                doctor.AvailableDays = updatedDoctor.AvailableDays;
                doctor.AvailabilityStart = updatedDoctor.AvailabilityStart;
                doctor.AvailabilityEnd = updatedDoctor.AvailabilityEnd;
                doctor.ProfileNote = updatedDoctor.ProfileNote;

                await _context.SaveChangesAsync();

                return Ok(doctor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Błąd podczas aktualizacji lekarza.",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var doctor = await _context.Doctors.FindAsync(id);

                if (doctor == null)
                {
                    return NotFound(new
                    {
                        message = "Lekarz nie istnieje."
                    });
                }

                _context.Doctors.Remove(doctor);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Lekarz został usunięty."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Błąd podczas usuwania lekarza.",
                    error = ex.Message
                });
            }
        }
    }
}