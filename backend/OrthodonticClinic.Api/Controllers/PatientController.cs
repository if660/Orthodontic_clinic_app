using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrthodonticClinic.Api.Data;

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
    }
}