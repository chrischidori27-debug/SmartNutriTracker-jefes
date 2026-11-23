using System.Security.Cryptography;

namespace SmartNutriTracker.Services;

// Provisional: cuando el Equipo 1 termine el login JWT, usar su servicio de contraseñas.
public static class PasswordHasher
{
    private const int Iteraciones = 100_000;

    public static string Hash(string password)
    {
        var sal = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, sal, Iteraciones, HashAlgorithmName.SHA256, 32);
        return $"PBKDF2${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }
}