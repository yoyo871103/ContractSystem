namespace InventorySystem.Application.Common.Models;

/// <summary>
/// Lista paginada de elementos. Respuesta de consultas que usan <see cref="FilterableQuery"/>.
/// Contiene los datos de la página actual y la información de paginación.
/// </summary>
/// <typeparam name="T">Tipo de cada elemento de la lista.</typeparam>
public class PagedList<T>
{
    #region Public Constructors

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="PagedList{T}"/>.
    /// </summary>
    public PagedList()
    {
        Items = new List<T>();
    }

    /// <summary>
    /// Inicializa una nueva instancia con los elementos y datos de paginación.
    /// </summary>
    public PagedList(IList<T> items, int currentPage, int totalPages, int totalRowsPerPage, int totalRows, int selectedPageSize)
    {
        Items = items ?? new List<T>();
        CurrentPage = currentPage;
        TotalPages = totalPages;
        TotalRowsPerPage = totalRowsPerPage;
        TotalRows = totalRows;
        SelectedPageSize = selectedPageSize;
    }

    #endregion Public Constructors

    #region Public Properties

    /// <summary>
    /// Número de página actual.
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Total de páginas en la lista paginada.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Número total de filas por página (tamaño de página solicitado).
    /// </summary>
    public int TotalRowsPerPage { get; set; }

    /// <summary>
    /// Número total de filas en la lista paginada.
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// Tamaño real de la página seleccionada (cantidad de elementos en la página actual).
    /// </summary>
    public int SelectedPageSize { get; set; }

    /// <summary>
    /// Elementos de la lista paginada (página actual).
    /// </summary>
    public IList<T> Items { get; set; }

    #endregion Public Properties

    /// <summary>
    /// Crea una lista paginada vacía (página 1, sin elementos).
    /// </summary>
    public static PagedList<T> Empty(int rowsPerPage = 10)
    {
        return new PagedList<T>(
            new List<T>(),
            currentPage: 1,
            totalPages: 0,
            totalRowsPerPage: Math.Max(1, rowsPerPage),
            totalRows: 0,
            selectedPageSize: 0);
    }
}
