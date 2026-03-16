using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.Nomencladores.Commands.CreateProductoServicio;
using ContractSystem.Application.Nomencladores.Commands.DeleteProductoServicio;
using ContractSystem.Application.Nomencladores.Commands.ReactivarProductoServicio;
using ContractSystem.Application.Nomencladores.Commands.UpdateProductoServicio;
using ContractSystem.Application.Nomencladores.Queries.GetAllProductosServicios;
using ContractSystem.Application.Nomencladores.Queries.GetAllUnidadesMedida;
using ContractSystem.Domain.Nomencladores;
using MediatR;
using System.Windows;

namespace ContractSystem.Windows.ViewModels;

/// <summary>
/// ViewModel para el CRUD de Productos/Servicios (catálogo).
/// </summary>
public sealed partial class ProductoServicioViewModel : ObservableObject
{
    private readonly ISender _sender;

    [ObservableProperty]
    private ObservableCollection<ProductoServicio> _productos = new();

    [ObservableProperty]
    private ProductoServicio? _seleccionado;

    [ObservableProperty]
    private bool _estaCargando;

    [ObservableProperty]
    private string? _mensajeError;

    [ObservableProperty]
    private bool _includeDeleted;

    [ObservableProperty]
    private TipoProductoServicio? _filtroTipo;

    /// <summary>
    /// Lista de unidades de medida para los diálogos.
    /// </summary>
    private IReadOnlyList<UnidadMedida> _unidadesMedida = Array.Empty<UnidadMedida>();

    public ProductoServicioViewModel(ISender sender)
    {
        _sender = sender;
        _ = CargarAsync();
    }

    [RelayCommand]
    private async Task CargarAsync(CancellationToken cancellationToken = default)
    {
        MensajeError = null;
        EstaCargando = true;
        try
        {
            var lista = await _sender.Send(new GetAllProductosServiciosQuery(IncludeDeleted, FiltroTipo), cancellationToken);
            Productos.Clear();
            foreach (var item in lista)
                Productos.Add(item);

            _unidadesMedida = await _sender.Send(new GetAllUnidadesMedidaQuery(), cancellationToken);
        }
        catch (Exception ex)
        {
            MensajeError = "No se pudo cargar la lista: " + ex.Message;
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private void Nuevo()
    {
        var dialog = new Views.Configuracion.ProductoServicioDialogWindow();
        dialog.Owner = System.Windows.Application.Current.MainWindow;
        dialog.CargarUnidadesMedida(_unidadesMedida);
        if (dialog.ShowDialog() == true)
        {
            _ = CrearAsync(dialog);
        }
    }

    private async Task CrearAsync(Views.Configuracion.ProductoServicioDialogWindow dialog)
    {
        try
        {
            await _sender.Send(new CreateProductoServicioCommand(
                dialog.Codigo, dialog.NombreProducto, dialog.Descripcion,
                dialog.TipoProducto, dialog.UnidadMedidaId, dialog.PrecioEstimado));

            MessageBox.Show("Producto/Servicio creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await CargarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al crear: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(HaySeleccionado))]
    private void Editar()
    {
        if (Seleccionado is null) return;

        var dialog = new Views.Configuracion.ProductoServicioDialogWindow();
        dialog.Owner = System.Windows.Application.Current.MainWindow;
        dialog.CargarUnidadesMedida(_unidadesMedida);
        dialog.CargarProducto(Seleccionado);
        if (dialog.ShowDialog() == true)
        {
            _ = ActualizarAsync(dialog, Seleccionado.Id);
        }
    }

    private async Task ActualizarAsync(Views.Configuracion.ProductoServicioDialogWindow dialog, int id)
    {
        try
        {
            await _sender.Send(new UpdateProductoServicioCommand(
                id, dialog.Codigo, dialog.NombreProducto, dialog.Descripcion,
                dialog.TipoProducto, dialog.UnidadMedidaId, dialog.PrecioEstimado));

            MessageBox.Show("Producto/Servicio actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await CargarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al actualizar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool HaySeleccionado() => Seleccionado is not null;

    [RelayCommand(CanExecute = nameof(PuedeEliminar))]
    private async Task EliminarAsync(CancellationToken cancellationToken = default)
    {
        if (Seleccionado is null) return;

        var result = MessageBox.Show(
            $"¿Eliminar '{Seleccionado.Nombre}'?",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _sender.Send(new DeleteProductoServicioCommand(Seleccionado.Id), cancellationToken);
            MessageBox.Show("Producto/Servicio eliminado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await CargarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al eliminar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool PuedeEliminar() => Seleccionado is not null && !Seleccionado.IsDeleted;

    [RelayCommand(CanExecute = nameof(PuedeReactivar))]
    private async Task ReactivarAsync(CancellationToken cancellationToken = default)
    {
        if (Seleccionado is null || !Seleccionado.IsDeleted) return;

        var result = MessageBox.Show(
            $"¿Reactivar '{Seleccionado.Nombre}'?",
            "Confirmar reactivación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _sender.Send(new ReactivarProductoServicioCommand(Seleccionado.Id), cancellationToken);
            MessageBox.Show("Producto/Servicio reactivado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await CargarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al reactivar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool PuedeReactivar() => Seleccionado is not null && Seleccionado.IsDeleted;

    partial void OnIncludeDeletedChanged(bool value) => _ = CargarAsync();
    partial void OnFiltroTipoChanged(TipoProductoServicio? value) => _ = CargarAsync();

    partial void OnSeleccionadoChanged(ProductoServicio? value)
    {
        EditarCommand.NotifyCanExecuteChanged();
        EliminarCommand.NotifyCanExecuteChanged();
        ReactivarCommand.NotifyCanExecuteChanged();
    }
}
