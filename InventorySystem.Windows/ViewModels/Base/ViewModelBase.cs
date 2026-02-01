using System.Collections;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace InventorySystem.Windows.ViewModels.Base;

/// <summary>
/// ViewModel base que implementa INotifyPropertyChanged e INotifyDataErrorInfo.
/// Permite mostrar errores de validación (p. ej. de FluentValidation) en rojo debajo de cada campo.
/// Para limpiar errores al editar, usa partial void On&lt;Property&gt;Changed en el ViewModel
/// o llama a ClearErrors(nombrePropiedad) manualmente.
/// </summary>
public abstract class ViewModelBase : ObservableObject, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errorsByProperty = new();

    public bool HasErrors => _errorsByProperty.Values.Any(list => list.Count > 0);

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _errorsByProperty.Values.SelectMany(list => list).ToList();

        return _errorsByProperty.TryGetValue(propertyName, out var errors)
            ? errors
            : Enumerable.Empty<string>();
    }

    /// <summary>
    /// Establece los errores para una propiedad. Notifica a la UI para que muestre/oculte el mensaje.
    /// </summary>
    protected void SetErrors(string propertyName, IEnumerable<string> errors)
    {
        var list = errors.ToList();
        if (list.Count > 0)
            _errorsByProperty[propertyName] = list;
        else
            _errorsByProperty.Remove(propertyName);

        OnErrorsChanged(propertyName);
        OnPropertyChanged(nameof(HasErrors));
    }

    /// <summary>
    /// Limpia todos los errores de validación.
    /// </summary>
    protected void ClearErrors()
    {
        var keys = _errorsByProperty.Keys.ToList();
        _errorsByProperty.Clear();
        foreach (var key in keys)
            OnErrorsChanged(key);
        OnPropertyChanged(nameof(HasErrors));
    }

    /// <summary>
    /// Limpia los errores de una propiedad específica.
    /// </summary>
    protected void ClearErrors(string propertyName)
    {
        if (_errorsByProperty.Remove(propertyName))
        {
            OnErrorsChanged(propertyName);
            OnPropertyChanged(nameof(HasErrors));
        }
    }

    /// <summary>
    /// Aplica los errores de una ValidationException de FluentValidation al ViewModel.
    /// Los PropertyName del validator deben coincidir con los nombres de propiedades del ViewModel.
    /// </summary>
    protected void ApplyValidationErrors(FluentValidation.ValidationException ex)
    {
        ClearErrors();
        var byProperty = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToList());

        foreach (var (propertyName, messages) in byProperty)
            SetErrors(propertyName, messages);
    }

    protected virtual void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }
}
