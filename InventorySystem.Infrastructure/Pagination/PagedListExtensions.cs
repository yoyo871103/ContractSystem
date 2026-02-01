using InventorySystem.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Pagination;

/// <summary>
/// Extensiones para crear <see cref="PagedList{T}"/> desde <see cref="IQueryable{T}"/> de forma eficiente:
/// se ejecuta una consulta de conteo y otra con Skip/Take para obtener solo la página solicitada.
/// </summary>
public static class PagedListExtensions
{
    /// <summary>
    /// Crea una lista paginada a partir de la consulta, ejecutando en base de datos solo el conteo total
    /// y los elementos de la página solicitada (Skip/Take).
    /// </summary>
    /// <typeparam name="T">Tipo de cada elemento.</typeparam>
    /// <param name="source">Consulta (p. ej. DbSet o IQueryable con filtros).</param>
    /// <param name="pageNumber">Número de página (1-based). Si es menor que 1, se usa 1.</param>
    /// <param name="pageSize">Tamaño de página (filas por página). Si es menor que 1, se usa 1.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista paginada con los elementos de la página y la información de paginación.</returns>
    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 1;

        var totalRows = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = totalRows == 0 ? 0 : (int)Math.Ceiling(totalRows / (double)pageSize);
        var selectedPageSize = items.Count;

        return new PagedList<T>(
            items,
            currentPage: pageNumber,
            totalPages,
            totalRowsPerPage: pageSize,
            totalRows: totalRows,
            selectedPageSize);
    }

    /// <summary>
    /// Crea una lista paginada a partir de la consulta usando los parámetros de la consulta filtrable.
    /// Aplique antes los filtros (SearchText) y orden (SortBy) sobre el IQueryable; esta extensión
    /// solo aplica paginación (PageNumber, RowsPerPage) y rellena SelectedPageSize en la respuesta.
    /// </summary>
    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> source,
        FilterableQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var pageNumber = query.PageNumber >= 1 ? query.PageNumber : 1;
        var rowsPerPage = query.RowsPerPage >= 1 ? query.RowsPerPage : 10;

        var paged = await source.ToPagedListAsync(pageNumber, rowsPerPage, cancellationToken);

        // Rellenar SelectedPageSize en la consulta para quien la use como DTO de entrada/salida
        query.SelectedPageSize = paged.SelectedPageSize;

        return paged;
    }
}
