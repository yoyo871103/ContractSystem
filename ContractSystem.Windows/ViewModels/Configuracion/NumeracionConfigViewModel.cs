using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.Contratos.Commands.UpdateConfiguracionNumeracion;
using ContractSystem.Application.Contratos.Queries.GetConfiguracionNumeracion;
using ContractSystem.Application.Contratos.Queries.GetVistaPreviaNumeracion;
using MediatR;
using System.Windows;

namespace ContractSystem.Windows.ViewModels;

/// <summary>
/// ViewModel para la configuración de numeración automática de documentos.
/// </summary>
public sealed partial class NumeracionConfigViewModel : ObservableObject
{
    private readonly ISender _sender;

    [ObservableProperty]
    private string _formato = "CON-{TIPO}-{YYYY}-{CONTADOR}";

    [ObservableProperty]
    private int _digitosPadding = 4;

    [ObservableProperty]
    private bool _contadorPorAnio = true;

    [ObservableProperty]
    private string _vistaPrevia = string.Empty;

    [ObservableProperty]
    private bool _estaCargando;

    [ObservableProperty]
    private string? _mensajeError;

    public NumeracionConfigViewModel(ISender sender)
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
            var config = await _sender.Send(new GetConfiguracionNumeracionQuery(), cancellationToken);
            if (config is not null)
            {
                Formato = config.Formato;
                DigitosPadding = config.DigitosPadding;
                ContadorPorAnio = config.ContadorPorAnio;
            }

            await ActualizarVistaPreviaAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MensajeError = "No se pudo cargar la configuración: " + ex.Message;
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand(CanExecute = nameof(FormatoValido))]
    private async Task GuardarAsync(CancellationToken cancellationToken = default)
    {
        MensajeError = null;
        try
        {
            await _sender.Send(new UpdateConfiguracionNumeracionCommand(Formato, DigitosPadding, ContadorPorAnio), cancellationToken);
            MessageBox.Show("Configuración de numeración guardada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MensajeError = "Error al guardar: " + ex.Message;
            MessageBox.Show(MensajeError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool FormatoValido() => !string.IsNullOrWhiteSpace(Formato);

    [RelayCommand]
    private async Task ActualizarVistaPreviaAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(Formato))
        {
            VistaPrevia = string.Empty;
            return;
        }

        try
        {
            VistaPrevia = await _sender.Send(
                new GetVistaPreviaNumeracionQuery(Formato, DigitosPadding, ContadorPorAnio),
                cancellationToken);
        }
        catch
        {
            VistaPrevia = "(formato inválido)";
        }
    }

    partial void OnFormatoChanged(string value)
    {
        GuardarCommand.NotifyCanExecuteChanged();
        _ = ActualizarVistaPreviaAsync();
    }

    partial void OnDigitosPaddingChanged(int value)
    {
        _ = ActualizarVistaPreviaAsync();
    }

    partial void OnContadorPorAnioChanged(bool value)
    {
        _ = ActualizarVistaPreviaAsync();
    }
}
