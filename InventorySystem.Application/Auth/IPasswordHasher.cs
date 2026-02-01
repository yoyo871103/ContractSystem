namespace InventorySystem.Application.Auth;

/// <summary>
/// Servicio para hashear y verificar contraseñas (PBKDF2 o similar).
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Genera un hash de la contraseña con un salt aleatorio.
    /// </summary>
    (string Hash, string Salt) HashPassword(string password);

    /// <summary>
    /// Verifica si la contraseña coincide con el hash y salt almacenados.
    /// </summary>
    bool VerifyPassword(string password, string hash, string salt);
}
