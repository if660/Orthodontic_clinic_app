using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrthodonticClinic.Api.Data;
using OrthodonticClinic.Api.Models;

namespace OrthodonticClinic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PatientController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("List")]
        public async Task<IActionResult> List()
        {
            try
            {
                var patients = await _context.Patients.ToListAsync();

                return Ok(patients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Wystąpił błąd podczas pobierania listy pacjentów.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("Data/{id}")]
        public async Task<IActionResult> Data(int id)
        {
            try
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (patient == null)
                {
                    return NotFound(new
                    {
                        message = "Nie znaleziono pacjenta o podanym ID."
                    });
                }

                return Ok(patient);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Wystąpił błąd podczas pobierania danych pacjenta.",
                    error = ex.Message
                });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var patients = await _context.Patients.ToListAsync();

                return Ok(patients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Błąd podczas pobierania pacjentów.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (patient == null)
                {
                    return NotFound(new
                    {
                        message = "Pacjent nie istnieje."
                    });
                }

                return Ok(patient);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Błąd podczas pobierania pacjenta.",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Patient patient)
        {
            try
            {
                _context.Patients.Add(patient);

                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = patient.Id },
                    patient
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Błąd podczas dodawania pacjenta.",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Patient updatedPatient)
        {
            try
            {
                var patient = await _context.Patients.FindAsync(id);

                if (patient == null)
                {
                    return NotFound(new
                    {
                        message = "Pacjent nie istnieje."
                    });
                }

                patient.FirstName = updatedPatient.FirstName;
                patient.LastName = updatedPatient.LastName;
                patient.Phone = updatedPatient.Phone;
                patient.Email = updatedPatient.Email;
                patient.BirthDate = updatedPatient.BirthDate;

                await _context.SaveChangesAsync();

                return Ok(patient);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Błąd podczas aktualizacji pacjenta.",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var patient = await _context.Patients.FindAsync(id);

                if (patient == null)
                {
                    return NotFound(new
                    {
                        message = "Pacjent nie istnieje."
                    });
                }

                _context.Patients.Remove(patient);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Pacjent został usunięty."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Błąd podczas usuwania pacjenta.",
                    error = ex.Message
                });
            }
        }
    }
}