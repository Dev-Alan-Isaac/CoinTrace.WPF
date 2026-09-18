using System;
using System.Security.Cryptography;

namespace Smart_Tools.WPF.Security
{
    /// <summary>
    /// One-way hashing for anything that only ever needs to be verified,
    /// never displayed or recovered - passwords and secret-question
    /// answers. Uses PBKDF2-HMACSHA256 with a random salt per value, so
    /// two identical passwords never produce the same stored hash.
    /// </summary>
    internal static class PasswordHasher
    {
        private const int SaltSizeBytes = 16;
        private const int HashSizeBytes = 32;
        private const int Iterations = 100_000;

        public static void Hash(string input, out string saltBase64, out string hashBase64)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(input, salt, Iterations, HashAlgorithmName.SHA256, HashSizeBytes);

            saltBase64 = Convert.ToBase64String(salt);
            hashBase64 = Convert.ToBase64String(hash);
        }

        public static bool Verify(string input, string saltBase64, string hashBase64)
        {
            if (string.IsNullOrEmpty(saltBase64) || string.IsNullOrEmpty(hashBase64))
                return false;

            byte[] salt = Convert.FromBase64String(saltBase64);
            byte[] expectedHash = Convert.FromBase64String(hashBase64);
            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(input, salt, Iterations, HashAlgorithmName.SHA256, expectedHash.Length);

            // Constant-time comparison so a mistyped password can't be
            // distinguished from a correct one by how long the check takes.
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
