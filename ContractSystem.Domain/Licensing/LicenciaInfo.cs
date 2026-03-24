namespace ContractSystem.Domain.Licensing;

/// <summary>
/// Entidad que almacena la información de licencia de la aplicación.
/// Una sola fila por base de datos.
/// </summary>
public class LicenciaInfo
{
    public int Id { get; set; }

    /// <summary>
    /// Clave de licencia proporcionada por el proveedor.
    /// </summary>
    public string Clave { get; set; } = string.Empty;

    /// <summary>
    /// Fecha en que se activó la licencia.
    /// </summary>
    public DateTime FechaActivacion { get; set; }

    /// <summary>
    /// Fecha de expiración de la licencia.
    /// </summary>
    public DateTime FechaExpiracion { get; set; }
}
