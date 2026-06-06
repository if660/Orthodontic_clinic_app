using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrthodonticClinic.Api.Data;
using OrthodonticClinic.Api.Services;

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
            var authorization = Request.Headers.Authorization.ToString();
            if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = "Brak tokena." });
            }

            var principal = _tokenService.ValidateToken(authorization["Bearer ".Length..].Trim());
            var email = principal?.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized(new { message = "Token jest nieprawidlowy." });
            }

            var user = await _context.AppUsers
                .Include(u => u.Patient)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return Unauthorized(new { message = "Uzytkownik nie istnieje." });
            }

            return Ok(ToResponse(user, authorization["Bearer ".Length..].Trim()));
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
                user.PatientId,
                patientName);
        }
    }

    public record LoginRequest(string Email, string Password);

    public record AuthResponse(
        string Token,
        string Role,
        string Email,
        int? PatientId,
        string? PatientName);
}
