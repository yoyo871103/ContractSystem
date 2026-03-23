using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.Business.Commands.UpdateBusinessInfo;
using ContractSystem.Application.Business.Queries.GetBusinessInfo;
using ContractSystem.Application.Auth;
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
        try
        {
            var command = new UpdateBusinessInfoCommand(
                Nombre, Logo, Nit, Direccion, Telefono, Email, Eslogan, NombreDueno
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
    private void RemoveLogo()
    {
        Logo = null;
    }
}
