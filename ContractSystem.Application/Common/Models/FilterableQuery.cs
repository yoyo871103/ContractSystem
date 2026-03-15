namespace ContractSystem.Application.Common.Models;

/// <summary>
/// Consulta base con filtrado, ordenación y paginación.
/// Las clases concretas de listados heredan de esta para recibir búsqueda, orden y página.
/// </summary>
public abstract class FilterableQuery
{
    /// <summary>
    /// Texto de búsqueda para filtrar el resultado.
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// Propiedad por la que ordenar el resultado (nombre de la columna o propiedad).
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Número de página a devolver (1-based).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Número máximo de registros por página.
    /// </summary>
    /// <remarks>Se usa para limitar la cantidad de registros devueltos por página.</remarks>
    public int RowsPerPage { get; set; } = 10;

    /// <summary>
    /// Número real de registros de la página seleccionada (se rellena en la respuesta).
    /// </summary>
    /// <remarks>
    /// Indica el número real de registros de la página seleccionada, que puede ser menor
    /// que el máximo especificado en <see cref="RowsPerPage"/> (p. ej. en la última página).
    /// </remarks>
    public int SelectedPageSize { get; set; }
}
