using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.Business.Commands.UpdateBusinessInfo;
using ContractSystem.Application.Business.Queries.GetBusinessInfo;
using ContractSystem.Application.Auth;
using ContractSystem.Windows.ViewModels.Shared;
using ContractSystem.Application.LlenadoContratos;
using MediatR;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace ContractSystem.Windows.ViewModels;

public partial class BusinessInfoViewModel : ObservableObject
{
    private readonly ISender _sender;
    private readonly IAuthContext _authContext;

    [ObservableProperty]
    private string _nombre = string.Empty;

    [ObservableProperty]
    private byte[]? _logo;

    [ObservableProperty]
    private string _nit = string.Empty;

    [ObservableProperty]
    private string _direccion = string.Empty;

    [ObservableProperty]
    private string _telefono = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _eslogan = string.Empty;

    [ObservableProperty]
    private string _nombreDueno = string.Empty;

    /// <summary>REEUP, registro, constitución, representantes y cuentas bancarias.</summary>
    public DatosLegalesEditModel DatosLegales { get; } = new();

    /// <summary>Plantilla del bloque de comparecencia (con marcadores {Campo}).</summary>
    [ObservableProperty]
    private string _plantillaComparecencia = BloquesContrato.ComparecenciaPredeterminada;

    /// <summary>Plantilla del bloque de firma (con marcadores {Campo}).</summary>
    [ObservableProperty]
    private string _plantillaFirma = BloquesContrato.FirmaPredeterminada;

    /// <summary>Lista de marcadores disponibles, para la ayuda en pantalla.</summary>
    public string AyudaMarcadores { get; } = string.Join("   ",
        BloquesContrato.Marcadores.Select(m => $"{m.Marcador} {m.Descripcion.ToLowerInvariant()}"));

    public bool PuedeEditarDatosNegocio => _authContext.TienePermiso(Permissions.DatosNegocioEditar);

    public BusinessInfoViewModel(ISender sender, IAuthContext authContext)
    {
        _sender = sender;
        _authContext = authContext;
        LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            var info = await _sender.Send(new GetBusinessInfoQuery());
            if (info != null)
            {
                Nombre = info.Nombre;
                Logo = info.Logo;
                Nit = info.Nit;
                Direccion = info.Direccion;
                Telefono = info.Telefono;
                Email = info.Email;
                Eslogan = info.Eslogan;
                NombreDueno = info.NombreDueno;
                PlantillaComparecencia = string.IsNullOrWhiteSpace(info.PlantillaComparecencia)
                    ? BloquesContrato.ComparecenciaPredeterminada : info.PlantillaComparecencia;
                PlantillaFirma = string.IsNullOrWhiteSpace(info.PlantillaFirma)
                    ? BloquesContrato.FirmaPredeterminada : info.PlantillaFirma;
                DatosLegales.Cargar(
                    info.CodigoReeup, info.RegistroMercantil, info.DocumentoConstitucion,
                    info.Representantes, info.CuentasBancarias);
            }
        }
        catch (Exception ex)
        {
            // Log error?
            System.Diagnostics.Debug.WriteLine($"Error loading business info: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(PuedeEditarDatosNegocio))]
    private async Task SaveAsync()
    {
        var errorDatosLegales = DatosLegales.Validar();
        if (errorDatosLegales is not null)
        {
            MessageBox.Show(errorDatosLegales, "Datos legales", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var command = new UpdateBusinessInfoCommand(
                Nombre, Logo, Nit, Direccion, Telefono, Email, Eslogan, NombreDueno,
                DatosLegales.ToDto(),
                new PlantillasBloquesDto(PlantillaComparecencia, PlantillaFirma)
            );

            await _sender.Send(command);
            MessageBox.Show("Datos del negocio actualizados correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void UploadLogo()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp",
            Title = "Seleccionar Logo"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                Logo = File.ReadAllBytes(openFileDialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la imagen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void RestaurarComparecencia() => PlantillaComparecencia = BloquesContrato.ComparecenciaPredeterminada;

    [RelayCommand]
    private void RestaurarFirma() => PlantillaFirma = BloquesContrato.FirmaPredeterminada;

    [RelayCommand]
    private void RemoveLogo()
    {
        Logo = null;
    }
}
