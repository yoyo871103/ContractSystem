using System.Security.Cryptography;
using System.Text;
using ContractSystem.Application.Auth;

namespace ContractSystem.Infrastructure.Auth;

internal sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public (string Hash, string Salt) HashPassword(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            Iterations,
            Algorithm,
            HashSize);

        var salt = Convert.ToBase64String(saltBytes);
        var hash = Convert.ToBase64String(hashBytes);
        return (hash, salt);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = Convert.FromBase64String(hash);
        var computed = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            Iterations,
            Algorithm,
            HashSize);

        return CryptographicOperations.FixedTimeEquals(computed, hashBytes);
    }
}
