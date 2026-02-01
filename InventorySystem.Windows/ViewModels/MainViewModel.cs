using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventorySystem.Windows.Services;
using InventorySystem.Windows.Views.Ventas;
using Microsoft.Extensions.DependencyInjection;

namespace InventorySystem.Windows.ViewModels;

/// <summary>
/// ViewModel principal. Gestiona la navegación entre módulos.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly IServiceProvider _services;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsExtractVisible))]
    private object? _currentViewModel;

    public MainViewModel(INavigationService navigation, IServiceProvider services)
    {
        _navigation = navigation;
        _services = services;

        _navigation.Navigated += (_, _) => CurrentViewModel = _navigation.CurrentViewModel;
        CurrentViewModel = _navigation.CurrentViewModel;

        // Vista inicial
        if (CurrentViewModel is null)
            NavigateToInicioCommand.Execute(null);
    }

    [RelayCommand]
    private void NavigateToInicio() => _navigation.NavigateToInicio();

    [RelayCommand]
    private void NavigateToVentas() =>
        _navigation.NavigateTo(_services.GetRequiredService<VentasViewModel>());

    [RelayCommand]
    private void NavigateToInventario()
    {
        // Placeholder para cuando exista InventarioViewModel
    }

    [RelayCommand]
    private void NavigateToProductos()
    {
        // Placeholder para cuando exista ProductosViewModel
    }

    [RelayCommand]
    private void NavigateToAlmacenes()
    {
        // Placeholder para cuando exista AlmacenesViewModel
    }

    [RelayCommand]
    private void NavigateToImportarExportar()
    {
        // Placeholder para cuando exista ImportarExportarViewModel
    }

    [RelayCommand]
    private void NavigateToInformes()
    {
        // Placeholder para cuando exista InformesViewModel
    }

    [RelayCommand]
    private void NavigateToConfiguracion()
    {
        // Placeholder para cuando exista ConfiguracionViewModel
    }

    /// <summary>
    /// Indica si el botón "Extraer" debe mostrarse (cuando hay una vista de módulo, no Inicio).
    /// </summary>
    public bool IsExtractVisible => CurrentViewModel is not null and not InicioViewModel;

    [RelayCommand]
    private void ExtraerVista()
    {
        if (CurrentViewModel is null or InicioViewModel)
            return;

        var (view, title) = CurrentViewModel switch
        {
            VentasViewModel => (new VentasView { DataContext = _services.GetRequiredService<VentasViewModel>() }, "Ventas"),
            _ => (null, "")
        };

        if (view is not null && !string.IsNullOrEmpty(title))
        {
            _services.GetRequiredService<IViewDialogService>().ShowInDialog(view, title);
            NavigateToInicio();
        }
    }
}
