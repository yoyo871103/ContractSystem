using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.Nomencladores.Commands.CreatePlantillaDocumento;
using ContractSystem.Application.Nomencladores.Commands.DeletePlantillaDocumento;
using ContractSystem.Application.Nomencladores.Queries.GetAllPlantillasDocumento;
using ContractSystem.Application.Nomencladores.Queries.GetPlantillaDocumentoById;
using ContractSystem.Domain.Nomencladores;
using MediatR;
using Microsoft.Win32;
using System.Windows;

namespace ContractSystem.Windows.ViewModels;

/// <summary>
/// ViewModel para el CRUD de Plantillas de Documentos Base.
/// </summary>
public sealed partial class PlantillaDocumentoViewModel : ObservableObject
{
    private readonly ISender _sender;

    [ObservableProperty]
    private ObservableCollection<PlantillaDocumento> _plantillas = new();

    [ObservableProperty]
    private PlantillaDocumento? _seleccionado;

    [ObservableProperty]
    private bool _estaCargando;

    [ObservableProperty]
    private string? _mensajeError;

    public PlantillaDocumentoViewModel(ISender sender)
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
            var lista = await _sender.Send(new GetAllPlantillasDocumentoQuery(), cancellationToken);
            Plantillas.Clear();
            foreach (var item in lista)
                Plantillas.Add(item);
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
        var dialog = new Views.Configuracion.PlantillaDialogWindow();
        dialog.Owner = System.Windows.Application.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            _ = CrearAsync(dialog);
        }
    }

    private async Task CrearAsync(Views.Configuracion.PlantillaDialogWindow dialog)
    {
        try
        {
            await _sender.Send(new CreatePlantillaDocumentoCommand(
                dialog.NombrePlantilla,
                dialog.TipoDocumento,
                dialog.RolPlantilla,
                dialog.ArchivoBytes,
                dialog.NombreArchivo,
                dialog.RevisadoPorLegal));

            MessageBox.Show("Plantilla creada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await CargarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al crear: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(HaySeleccionado))]
    private async Task DescargarAsync(CancellationToken cancellationToken = default)
    {
        if (Seleccionado is null) return;

        try
        {
            var plantilla = await _sender.Send(new GetPlantillaDocumentoByIdQuery(Seleccionado.Id), cancellationToken);
            if (plantilla is null || plantilla.Archivo.Length == 0)
            {
                MessageBox.Show("No se encontró el archivo.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new SaveFileDialog
            {
                FileName = plantilla.NombreArchivo,
                Filter = "Todos los archivos (*.*)|*.*"
            };

            if (sfd.ShowDialog() == true)
            {
                File.WriteAllBytes(sfd.FileName, plantilla.Archivo);
                MessageBox.Show("Archivo descargado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al descargar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(HaySeleccionado))]
    private async Task EliminarAsync(CancellationToken cancellationToken = default)
    {
        if (Seleccionado is null) return;

        var result = MessageBox.Show(
            $"¿Eliminar la plantilla '{Seleccionado.Nombre}'?\nEsta acción es permanente.",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _sender.Send(new DeletePlantillaDocumentoCommand(Seleccionado.Id), cancellationToken);
            MessageBox.Show("Plantilla eliminada.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await CargarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al eliminar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool HaySeleccionado() => Seleccionado is not null;

    partial void OnSeleccionadoChanged(PlantillaDocumento? value)
    {
        DescargarCommand.NotifyCanExecuteChanged();
        EliminarCommand.NotifyCanExecuteChanged();
    }
}
