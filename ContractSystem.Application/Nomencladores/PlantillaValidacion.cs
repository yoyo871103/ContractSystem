using ContractSystem.Domain.Nomencladores;

namespace ContractSystem.Application.Nomencladores;

/// <summary>
/// Reglas comunes de validación para crear/actualizar plantillas de documento.
/// </summary>
internal static class PlantillaValidacion
{
    /// <summary>
    /// Valida los datos y devuelve el TipoPersonalizado normalizado (null si el tipo no es "Otros").
    /// </summary>
    /// <param name="nombreArchivo">Nombre del archivo nuevo; null si no se cambia el archivo.</param>
    public static string? Validar(string nombre, TipoDocumentoPlantilla tipo, string? tipoPersonalizado, string? nombreArchivo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new InvalidOperationException("El nombre de la plantilla es obligatorio.");

        if (nombreArchivo is not null)
        {
            var extension = Path.GetExtension(nombreArchivo);
            if (string.IsNullOrEmpty(extension) || !PlantillaDocumento.ExtensionesPermitidas.Contains(extension))
                throw new InvalidOperationException(
                    $"El tipo de archivo '{extension}' no está permitido para plantillas. " +
                    $"Tipos permitidos: {string.Join(", ", PlantillaDocumento.ExtensionesPermitidas)}");
        }

        if (tipo != TipoDocumentoPlantilla.Otros)
            return null;

        var personalizado = tipoPersonalizado?.Trim();
        if (string.IsNullOrEmpty(personalizado))
            throw new InvalidOperationException("Debe indicar el nombre del tipo de documento cuando el tipo es 'Otros'.");
        if (personalizado.Length > 150)
            throw new InvalidOperationException("El nombre del tipo personalizado no puede superar 150 caracteres.");

        return personalizado;
    }
}
