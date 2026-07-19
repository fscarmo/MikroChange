using System.Security.Cryptography;


namespace MikroChange.Infra.Security;


/// <summary>
/// Responsável exclusivamente por gerar e verificar hashes de senha (PBKDF2-HMAC-SHA256).
/// </summary>
public static class PasswordHasher
{
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;


    // Recomendação OWASP (2024) para PBKDF2-HMAC-SHA256.
    private const int Iterations = 600_000;


    public sealed record HashResult(string SaltBase64, string HashBase64, int Iterations);


    public static HashResult Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSizeBytes);

        return new HashResult(
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash),
            Iterations);
    }


    public static bool Verify(string password, string saltBase64, string hashBase64, int iterations)
    {
        var salt = Convert.FromBase64String(saltBase64);
        var expectedHash = Convert.FromBase64String(hashBase64);

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedHash.Length);

        // Comparação em tempo constante para evitar timing attacks.
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
