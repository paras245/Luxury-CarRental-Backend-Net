using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CarRental_Backend_Net.Helpers
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            // Generate a 128-bit salt using a secure PRNG
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            // Return the salt and the hash combined
            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        public static bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            try
            {
                var parts = hashedPassword.Split('.', 2);
                if (parts.Length != 2) return false;

                var salt = Convert.FromBase64String(parts[0]);
                var storedHash = parts[1];

                string providedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: providedPassword,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

                return storedHash == providedHash;
            }
            catch
            {
                return false;
            }
        }
    }
}
