using Microsoft.Extensions.DependencyInjection;

namespace InventorySystem.Windows.Services;

/// <summary>
/// Implementación de INavigationService para navegación entre vistas.
/// </summary>
public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private object? _currentViewModel;

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            if (ReferenceEquals(_currentViewModel, value)) return;
            _currentViewModel = value;
            Navigated?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? Navigated;

    public void NavigateTo(object? viewModel) => CurrentViewModel = viewModel;

    public void NavigateToInicio() =>
        CurrentViewModel = _services.GetRequiredService<ViewModels.InicioViewModel>();
}
