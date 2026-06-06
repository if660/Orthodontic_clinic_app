using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrthodonticClinic.Api.Data;
using OrthodonticClinic.Api.Models;
using OrthodonticClinic.Api.Services;
using System.Security.Claims;

namespace OrthodonticClinic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly AuthTokenService _tokenService;

        public AuthController(
            AppDbContext context,
            PasswordService passwordService,
            AuthTokenService tokenService)
        {
            _context = context;
            _passwordService = passwordService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _context.AppUsers
                .Include(u => u.Patient)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !_passwordService.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Nieprawidlowy email lub haslo." });
            }

            return Ok(ToResponse(user, _tokenService.CreateToken(user)));
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized(new { message = "Brak poprawnej sesji." });
            }

            return Ok(ToResponse(user, GetBearerToken() ?? _tokenService.CreateToken(user)));
        }

        [HttpGet("patient/{patientId:int}/account")]
        public async Task<IActionResult> GetPatientAccount(int patientId)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser?.Role != "Clinic")
            {
                return StatusCode(403, new { message = "Brak uprawnien kliniki." });
            }

            var account = await _context.AppUsers
                .Where(u => u.PatientId == patientId)
                .Select(u => new PatientAccountResponse(true, u.Email, u.MustChangePassword))
                .FirstOrDefaultAsync();

            return Ok(account ?? new PatientAccountResponse(false, null, false));
        }

        [HttpPost("patient/{patientId:int}/account")]
        public async Task<IActionResult> CreatePatientAccount(int patientId, CreatePatientAccountRequest request)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser?.Role != "Clinic")
            {
                return StatusCode(403, new { message = "Brak uprawnien kliniki." });
            }

            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound(new { message = "Pacjent nie istnieje." });
            }

            var email = request.Email.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "Podaj adres email pacjenta." });
            }

            var accountExists = await _context.AppUsers.AnyAsync(u => u.PatientId == patientId || u.Email == email);
            if (accountExists)
            {
                return BadRequest(new { message = "Konto pacjenta juz istnieje." });
            }

            var temporaryPassword = _passwordService.GenerateTemporaryPassword();
            var account = new AppUser
            {
                Email = email,
                PasswordHash = _passwordService.Hash(temporaryPassword),
                Role = "Patient",
                MustChangePassword = true,
                PatientId = patient.Id
            };

            _context.AppUsers.Add(account);
            await _context.SaveChangesAsync();

            return Ok(new CreatePatientAccountResponse(account.Email, temporaryPassword, account.MustChangePassword));
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized(new { message = "Brak poprawnej sesji." });
            }

            if (!_passwordService.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "Aktualne haslo jest nieprawidlowe." });
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new { message = "Nowe haslo i potwierdzenie musza byc takie same." });
            }

            if (!_passwordService.IsStrongPassword(request.NewPassword))
            {
                return BadRequest(new { message = "Haslo musi miec minimum 8 znakow, mala litere, duza litere, cyfre i znak specjalny." });
            }

            user.PasswordHash = _passwordService.Hash(request.NewPassword);
            user.MustChangePassword = false;
            await _context.SaveChangesAsync();

            return Ok(ToResponse(user, _tokenService.CreateToken(user)));
        }

        private string? GetBearerToken()
        {
            var authorization = Request.Headers.Authorization.ToString();
            if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return authorization["Bearer ".Length..].Trim();
        }

        private async Task<AppUser?> GetCurrentUserAsync()
        {
            var token = GetBearerToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var principal = _tokenService.ValidateToken(token);
            var email = principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            return await _context.AppUsers
                .Include(u => u.Patient)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        private static AuthResponse ToResponse(Models.AppUser user, string token)
        {
            var patientName = user.Patient != null
                ? $"{user.Patient.FirstName} {user.Patient.LastName}".Trim()
                : null;

            return new AuthResponse(
                token,
                user.Role,
                user.Email,
                user.MustChangePassword,
                user.PatientId,
                patientName);
        }
    }

    public record LoginRequest(string Email, string Password);

    public record AuthResponse(
        string Token,
        string Role,
        string Email,
        bool MustChangePassword,
        int? PatientId,
        string? PatientName);

    public record PatientAccountResponse(bool HasAccount, string? Email, bool MustChangePassword);

    public record CreatePatientAccountRequest(string Email);

    public record CreatePatientAccountResponse(string Email, string TemporaryPassword, bool MustChangePassword);

    public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
}
