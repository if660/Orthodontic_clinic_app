using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrthodonticClinic.Api.Data;
using OrthodonticClinic.Api.Models;

namespace OrthodonticClinic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientDocumentController : ControllerBase
    {
        private readonly AppDbContext _context;

        private static readonly string[] AllowedContentTypes =
        [
            "application/pdf",
            "image/jpeg",
            "image/png",
            "image/gif",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ];

        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public PatientDocumentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            try
            {
                var documents = await _context.PatientDocuments
                    .Where(d => d.PatientId == patientId)
                    .OrderByDescending(d => d.UploadedAt)
                    .Select(d => new
                    {
                        d.Id,
                        d.PatientId,
                        d.FileName,
                        d.ContentType,
                        d.FileSize,
                        d.Description,
                        d.UploadedAt
                    })
                    .ToListAsync();

                return Ok(documents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Błąd podczas pobierania dokumentów.", error = ex.Message });
            }
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var document = await _context.PatientDocuments.FindAsync(id);

                if (document == null)
                    return NotFound(new { message = "Dokument nie istnieje." });

                return File(document.FileData, document.ContentType, document.FileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Błąd podczas pobierania pliku.", error = ex.Message });
            }
        }

        [HttpPost("upload")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> Upload(
            [FromForm] int patientId,
            [FromForm] IFormFile file,
            [FromForm] string? description)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Nie przesłano pliku." });

                if (file.Length > MaxFileSizeBytes)
                    return BadRequest(new { message = "Plik jest za duży. Maksymalny rozmiar to 10 MB." });

                if (!AllowedContentTypes.Contains(file.ContentType))
                    return BadRequest(new { message = "Niedozwolony typ pliku. Akceptowane: PDF, JPG, PNG, GIF, DOC, DOCX." });

                var patient = await _context.Patients.FindAsync(patientId);
                if (patient == null)
                    return NotFound(new { message = "Pacjent nie istnieje." });

                byte[] fileData;
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    fileData = ms.ToArray();
                }

                var document = new PatientDocument
                {
                    PatientId = patientId,
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    FileData = fileData,
                    FileSize = file.Length,
                    Description = description,
                    UploadedAt = DateTime.UtcNow
                };

                _context.PatientDocuments.Add(document);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(Download), new { id = document.Id }, new
                {
                    document.Id,
                    document.PatientId,
                    document.FileName,
                    document.ContentType,
                    document.FileSize,
                    document.Description,
                    document.UploadedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Błąd podczas przesyłania pliku.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var document = await _context.PatientDocuments.FindAsync(id);

                if (document == null)
                    return NotFound(new { message = "Dokument nie istnieje." });

                _context.PatientDocuments.Remove(document);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Dokument został usunięty." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Błąd podczas usuwania dokumentu.", error = ex.Message });
            }
        }
    }
}
