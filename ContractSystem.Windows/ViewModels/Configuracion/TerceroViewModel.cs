using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.Nomencladores.Commands.CreateTercero;
using ContractSystem.Application.Nomencladores.Commands.DeleteTercero;
using ContractSystem.Application.Nomencladores.Commands.ReactivarTercero;
using ContractSystem.Application.Nomencladores.Commands.UpdateTercero;
using ContractSystem.Application.Nomencladores.Queries.GetAllTerceros;
using ContractSystem.Application.Nomencladores.Queries.GetTerceroById;
using ContractSystem.Domain.Nomencladores;
using MediatR;
using System.Windows;

namespace ContractSystem.Windows.ViewModels;

/// <summary>
/// ViewModel para el CRUD de Terceros (Clientes/Proveedores).
/// </summary>
public sealed partial class TerceroViewModel : ObservableObject
{
    private readonly ISender _sender;

    [ObservableProperty]
    private ObservableCollection<Tercero> _terceros = new();

    [ObservableProperty]
    private Tercero? _seleccionado;

    [ObservableProperty]
    private bool _estaCargando;

    [ObservableProperty]
    private string? _mensajeError;

    [ObservableProperty]
    private bool _includeDeleted;

    [ObservableProperty]
    private TipoTercero? _filtroTipo;

    public TerceroViewModel(ISender sender)
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
            var lista = await _sender.Send(new GetAllTercerosQuery(IncludeDeleted, FiltroTipo), cancellationToken);
            Terceros.Clear();
            foreach (var item in lista)
                Terceros.Add(item);
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
        var dialog = new Views.Configuracion.TerceroDialogWindow();
        dialog.Owner = System.Windows.Application.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            _ = CrearTerceroAsync(dialog);
        }
    }

    private async Task CrearTerceroAsync(Views.Configuracion.TerceroDialogWindow dialog)
    {
        try
        {
            var contactos = dialog.Contactos.Select(c =>
                new ContactoTerceroDto(c.Nombre, c.Cargo, c.Email, c.Telefono)).ToList();

            await _sender.Send(new CreateTerceroCommand(
                dialog.NombreTercero,
                dialog.RazonSocial,
                dialog.NifCif,
                dialog.Direccion,
                dialog.TelefonoTercero,
                dialog.EmailTercero,
                dialog.TipoTercero,
                contactos));

            MessageBox.Show("Tercero creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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

        var dialog = new Views.Configuracion.TerceroDialogWindow();
        dialog.Owner = System.Windows.Application.Current.MainWindow;
        dialog.CargarTercero(Seleccionado);
        if (dialog.ShowDialog() == true)
        {
            _ = ActualizarTerceroAsync(dialog, Seleccionado.Id);
        }
    }

    private async Task ActualizarTerceroAsync(Views.Configuracion.TerceroDialogWindow dialog, int id)
    {
        try
        {
            var contactos = dialog.Contactos.Select(c =>
                new ContactoTerceroDto(c.Nombre, c.Cargo, c.Email, c.Telefono)).ToList();

            await _sender.Send(new UpdateTerceroCommand(
                id,
                dialog.NombreTercero,
                dialog.RazonSocial,
                dialog.NifCif,
                dialog.Direccion,
                dialog.TelefonoTercero,
                dialog.EmailTercero,
                dialog.TipoTercero,
                contactos));

            MessageBox.Show("Tercero actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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
            $"¿Eliminar el tercero '{Seleccionado.Nombre}'?",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _sender.Send(new DeleteTerceroCommand(Seleccionado.Id), cancellationToken);
            MessageBox.Show("Tercero eliminado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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
            $"¿Reactivar el tercero '{Seleccionado.Nombre}'?",
            "Confirmar reactivación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _sender.Send(new ReactivarTerceroCommand(Seleccionado.Id), cancellationToken);
            MessageBox.Show("Tercero reactivado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await CargarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al reactivar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool PuedeReactivar() => Seleccionado is not null && Seleccionado.IsDeleted;

    partial void OnIncludeDeletedChanged(bool value) => _ = CargarAsync();
    partial void OnFiltroTipoChanged(TipoTercero? value) => _ = CargarAsync();

    partial void OnSeleccionadoChanged(Tercero? value)
    {
        EditarCommand.NotifyCanExecuteChanged();
        EliminarCommand.NotifyCanExecuteChanged();
        ReactivarCommand.NotifyCanExecuteChanged();
    }
}
