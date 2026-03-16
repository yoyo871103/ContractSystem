namespace ContractSystem.Domain.Nomencladores;

/// <summary>
/// Contacto asociado a un tercero (persona de contacto).
/// </summary>
public sealed class ContactoTercero
{
    public int Id { get; set; }
    public int TerceroId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;

    public Tercero? Tercero { get; set; }
}
