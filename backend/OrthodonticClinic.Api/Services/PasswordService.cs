using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace OrthodonticClinic.Api.Services
{
    public class PasswordService
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;
        private const string Lowercase = "abcdefghijkmnopqrstuvwxyz";
        private const string Uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const string Digits = "23456789";
        private const string Special = "!@#$%^&*?";

        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }

        public bool Verify(string password, string passwordHash)
        {
            var parts = passwordHash.Split('.');
            if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[1]);
            var expectedKey = Convert.FromBase64String(parts[2]);
            var actualKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedKey.Length);

            return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
        }

        public string GenerateTemporaryPassword(int length = 14)
        {
            if (length < 12) length = 12;

            var chars = new List<char>
            {
                GetRandomChar(Lowercase),
                GetRandomChar(Uppercase),
                GetRandomChar(Digits),
                GetRandomChar(Special)
            };

            var allChars = Lowercase + Uppercase + Digits + Special;
            while (chars.Count < length)
            {
                chars.Add(GetRandomChar(allChars));
            }

            return new string(chars.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
        }

        public bool IsStrongPassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password) &&
                   password.Length >= 8 &&
                   Regex.IsMatch(password, "[a-z]") &&
                   Regex.IsMatch(password, "[A-Z]") &&
                   Regex.IsMatch(password, "[0-9]") &&
                   Regex.IsMatch(password, "[^a-zA-Z0-9]");
        }

        private static char GetRandomChar(string chars)
        {
            return chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }
    }
}
