using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AuthService.Application.Interfaces;

namespace AuthService.Application.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int Iterations = 100_000;
        private const int KeySize = 32;

        public void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            salt = RandomNumberGenerator.GetBytes(16);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256);

            hash = pbkdf2.GetBytes(KeySize);
        }

        public bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                storedSalt,
                Iterations,
                HashAlgorithmName.SHA256);

            var computedHash = pbkdf2.GetBytes(KeySize);
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
    }
}