using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.Nomencladores.Commands.CreateUnidadMedida;
using ContractSystem.Application.Nomencladores.Commands.DeleteUnidadMedida;
using ContractSystem.Application.Nomencladores.Commands.ReactivarUnidadMedida;
using ContractSystem.Application.Nomencladores.Commands.UpdateUnidadMedida;
using ContractSystem.Application.Nomencladores.Queries.GetAllUnidadesMedida;
using ContractSystem.Domain.Nomencladores;
using MediatR;
using System.Windows;

namespace ContractSystem.Windows.ViewModels;

/// <summary>
/// ViewModel para el CRUD de Unidades de medida (nomenclador).
/// </summary>
public sealed partial class UnidadMedidaViewModel : ObservableObject
{
    private readonly ISender _sender;

    [ObservableProperty]
    private ObservableCollection<UnidadMedida> _unidades = new();

    [ObservableProperty]
    private UnidadMedida? _seleccionado;

    [ObservableProperty]
    private string _nombreCorto = string.Empty;

    [ObservableProperty]
    private string _descripcion = string.Empty;

    [ObservableProperty]
    private bool _estaCargando;

    [ObservableProperty]
    private string? _mensajeError;

    [ObservableProperty]
    private bool _esModoEdicion;

    [ObservableProperty]
    private bool _includeDeleted;

    public UnidadMedidaViewModel(ISender sender)
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
            var lista = await _sender.Send(new GetAllUnidadesMedidaQuery(IncludeDeleted), cancellationToken);
            Unidades.Clear();
            foreach (var item in lista)
                Unidades.Add(item);
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

    [RelayCommand(CanExecute = nameof(HaySeleccionado))]
    private void EditarSeleccionado()
    {
        if (Seleccionado is null) return;

        NombreCorto = Seleccionado.NombreCorto;
        Descripcion = Seleccionado.Descripcion;
        EsModoEdicion = true;
    }

    private bool HaySeleccionado() => Seleccionado is not null;

    [RelayCommand]
    private void Nuevo()
    {
        LimpiarFormulario();
        EsModoEdicion = false;
    }

    [RelayCommand(CanExecute = nameof(FormularioValido))]
    private async Task GuardarAsync(CancellationToken cancellationToken = default)
    {
        MensajeError = null;
        var nombre = NombreCorto.Trim();
        var desc = Descripcion?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nombre))
        {
            MensajeError = "El nombre corto es obligatorio.";
            return;
        }

        try
        {
            if (EsModoEdicion && Seleccionado is not null)
            {
                await _sender.Send(new UpdateUnidadMedidaCommand(Seleccionado.Id, nombre, desc), cancellationToken);
                MessageBox.Show("Unidad de medida actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                await _sender.Send(new CreateUnidadMedidaCommand(nombre, desc), cancellationToken);
                MessageBox.Show("Unidad de medida creada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LimpiarFormulario();
            EsModoEdicion = false;
            await CargarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MensajeError = "Error al guardar: " + ex.Message;
            MessageBox.Show(MensajeError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool FormularioValido() => !string.IsNullOrWhiteSpace(NombreCorto);

    [RelayCommand(CanExecute = nameof(PuedeEliminar))]
    private async Task EliminarAsync(CancellationToken cancellationToken = default)
    {
        if (Seleccionado is null) return;

        var result = MessageBox.Show(
            $"¿Eliminar la unidad de medida '{Seleccionado.NombreCorto}'?",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        MensajeError = null;
        try
        {
            await _sender.Send(new DeleteUnidadMedidaCommand(Seleccionado.Id), cancellationToken);
            MessageBox.Show("Unidad de medida eliminada.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            LimpiarFormulario();
            EsModoEdicion = false;
            await CargarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MensajeError = "Error al eliminar: " + ex.Message;
            MessageBox.Show(MensajeError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool PuedeEliminar() => Seleccionado is not null && !Seleccionado.IsDeleted;

    [RelayCommand(CanExecute = nameof(PuedeReactivar))]
    private async Task ReactivarAsync(CancellationToken cancellationToken = default)
    {
        if (Seleccionado is null || Seleccionado.IsDeleted == false) return;

        var result = MessageBox.Show(
            $"¿Reactivar la unidad de medida '{Seleccionado.NombreCorto}'?",
            "Confirmar reactivación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        MensajeError = null;
        try
        {
            await _sender.Send(new ReactivarUnidadMedidaCommand(Seleccionado.Id), cancellationToken);
            MessageBox.Show("Unidad de medida reactivada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            LimpiarFormulario();
            EsModoEdicion = false;
            await CargarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MensajeError = "Error al reactivar: " + ex.Message;
            MessageBox.Show(MensajeError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool PuedeReactivar() => Seleccionado is not null && Seleccionado.IsDeleted;

    private void LimpiarFormulario()
    {
        NombreCorto = string.Empty;
        Descripcion = string.Empty;
        Seleccionado = null;
    }

    partial void OnIncludeDeletedChanged(bool value)
    {
        _ = CargarAsync();
    }

    partial void OnSeleccionadoChanged(UnidadMedida? value)
    {
        EditarSeleccionadoCommand.NotifyCanExecuteChanged();
        EliminarCommand.NotifyCanExecuteChanged();
        ReactivarCommand.NotifyCanExecuteChanged();
    }

    partial void OnNombreCortoChanged(string value)
    {
        GuardarCommand.NotifyCanExecuteChanged();
    }
}
