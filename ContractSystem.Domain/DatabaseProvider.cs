namespace ContractSystem.Domain;

/// <summary>
/// Proveedor de base de datos soportado por la aplicación.
/// La app Windows puede usar SqlServer o Sqlite.
/// La app MAUI móvil solo usa Sqlite (BD local).
/// </summary>
public enum DatabaseProvider
{
    SqlServer,
    Sqlite
}
