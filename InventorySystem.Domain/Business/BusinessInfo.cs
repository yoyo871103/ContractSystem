namespace InventorySystem.Domain.Business;

public class BusinessInfo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public byte[]? Logo { get; set; }
    public string Nit { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Eslogan { get; set; } = string.Empty;
    public string NombreDueno { get; set; } = string.Empty;
}
