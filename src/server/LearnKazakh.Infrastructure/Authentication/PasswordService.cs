using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace LearnKazakh.Infrastructure.Authentication;

public interface IPasswordService
{
    (string Hash, string Salt) HashPassword(string password);
    bool VerifyPassword(string password, string hash, string salt);
}

public class PasswordService : IPasswordService
{
    private const int SaltSize = 16;
    private const int HashSize = 64;
    private const int Iterations = 100_000;

    public (string Hash, string Salt) HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        byte[] hash = KeyDerivation.Pbkdf2
        (
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA512,
            iterationCount: Iterations,
            numBytesRequested: HashSize
        );

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);

        byte[] computedHash = KeyDerivation.Pbkdf2
        (
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA512,
            iterationCount: Iterations,
            numBytesRequested: HashSize
        );

        return CryptographicOperations.FixedTimeEquals(computedHash, Convert.FromBase64String(hash));
    }
}