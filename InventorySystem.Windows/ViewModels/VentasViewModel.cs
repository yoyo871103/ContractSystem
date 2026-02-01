using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventorySystem.Windows.Shared.Dialogs;
using InventorySystem.Windows.ViewModels.Base;

namespace InventorySystem.Windows.ViewModels;

/// <summary>
/// ViewModel de la vista de Ventas.
/// Ejemplo de integración MVVM con binding, comandos y validación.
/// </summary>
public sealed partial class VentasViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _almacenSeleccionado = string.Empty;

    [ObservableProperty]
    private string _descuentoGeneral = "0";

    [ObservableProperty]
    private string _totalVenta = "0,00";

    public ObservableCollection<LineaCarritoItem> ItemsCarrito { get; } = new();

    public VentasViewModel()
    {
        // Datos de ejemplo
        ItemsCarrito.Add(new LineaCarritoItem("Producto ejemplo A", "2", "10,00", "5", "19,00"));
        ItemsCarrito.Add(new LineaCarritoItem("Producto ejemplo B", "1", "25,50", "0", "25,50"));
        TotalVenta = "44,50";
    }

    [RelayCommand]
    private void AgregarArticulo()
    {
        // Placeholder: abrir diálogo o vista para agregar artículo
    }

    [RelayCommand]
    private void Cancelar()
    {
        // Placeholder: limpiar o volver
    }

    [RelayCommand]
    private void FinalizarVenta(Window? owner)
    {
        ClearErrors();

        var result = MessageDialog.Show(
            owner,
            "Confirmar venta",
            "¿Desea finalizar la venta con los artículos del carrito?",
            MessageDialogButtons.YesNo,
            MessageDialogIcon.Information);

        if (result == MessageDialogResult.Yes)
        {
            MessageDialog.Show(
                owner,
                "Venta registrada",
                "La venta se ha registrado correctamente.",
                MessageDialogButtons.Ok,
                MessageDialogIcon.Information);
        }
    }
}

/// <summary>
/// Elemento de línea del carrito para binding.
/// </summary>
public sealed record LineaCarritoItem(
    string Articulo,
    string Cantidad,
    string PrecioUnitario,
    string DescuentoPorcentaje,
    string Subtotal);
