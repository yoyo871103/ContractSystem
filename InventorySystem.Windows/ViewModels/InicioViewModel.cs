using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventorySystem.Windows.Services;
using InventorySystem.Windows.Views.Ventas;
using Microsoft.Extensions.DependencyInjection;

namespace InventorySystem.Windows.ViewModels;

/// <summary>
/// ViewModel de la pantalla de bienvenida/inicio.
/// </summary>
public sealed partial class InicioViewModel : ObservableObject
{
    private readonly IViewDialogService _viewDialog;
    private readonly IServiceProvider _services;

    public InicioViewModel(IViewDialogService viewDialog, IServiceProvider services)
    {
        _viewDialog = viewDialog;
        _services = services;
    }

    [ObservableProperty]
    private string _mensajeBienvenida = "Bienvenido al Sistema de Inventario";

    [ObservableProperty]
    private string _mensajeSecundario = "Selecciona una opción del menú para comenzar";

    [RelayCommand]
    private void AbrirVentasEnVentana()
    {
        var vm = _services.GetRequiredService<VentasViewModel>();
        var view = new VentasView { DataContext = vm };
        _viewDialog.ShowInDialog(view, "Ventas");
    }
}
