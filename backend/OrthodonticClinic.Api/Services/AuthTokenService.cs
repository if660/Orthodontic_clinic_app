using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OrthodonticClinic.Api.Models;

namespace OrthodonticClinic.Api.Services
{
    public class AuthTokenService
    {
        private readonly IConfiguration _configuration;

        public AuthTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateToken(AppUser user)
        {
            var key = GetSigningKey();
            var header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
            {
                alg = "HS256",
                typ = "JWT"
            }));

            var now = DateTimeOffset.UtcNow;
            var claims = new Dictionary<string, object?>
            {
                [ClaimTypes.NameIdentifier] = user.Id.ToString(),
                [ClaimTypes.Email] = user.Email,
                [ClaimTypes.Role] = user.Role,
                ["patientId"] = user.PatientId,
                ["mustChangePassword"] = user.MustChangePassword,
                ["exp"] = now.AddHours(8).ToUnixTimeSeconds(),
                ["iat"] = now.ToUnixTimeSeconds()
            };

            var payload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(claims));
            var signature = Sign($"{header}.{payload}", key);

            return $"{header}.{payload}.{signature}";
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                return null;
            }

            var expectedSignature = Sign($"{parts[0]}.{parts[1]}", GetSigningKey());
            if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedSignature),
                Encoding.UTF8.GetBytes(parts[2])))
            {
                return null;
            }

            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var payload = JsonDocument.Parse(payloadJson);
            if (!payload.RootElement.TryGetProperty("exp", out var expElement))
            {
                return null;
            }

            var exp = DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64());
            if (exp <= DateTimeOffset.UtcNow)
            {
                return null;
            }

            var claims = new List<Claim>();
            foreach (var property in payload.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Null)
                {
                    continue;
                }

                claims.Add(new Claim(property.Name, property.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, "Bearer");
            return new ClaimsPrincipal(identity);
        }

        private byte[] GetSigningKey()
        {
            var secret = _configuration["Auth:JwtSecret"];
            if (string.IsNullOrWhiteSpace(secret))
            {
                secret = "orthodontic-clinic-demo-secret-change-before-production";
            }

            return Encoding.UTF8.GetBytes(secret);
        }

        private static string Sign(string value, byte[] key)
        {
            using var hmac = new HMACSHA256(key);
            return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }

        private static string Base64UrlEncode(byte[] value)
        {
            return Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string value)
        {
            var padded = value.Replace('-', '+').Replace('_', '/');
            padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
            return Convert.FromBase64String(padded);
        }
    }
}
