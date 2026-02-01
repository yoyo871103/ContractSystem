using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventorySystem.Application.Auth;
using InventorySystem.Windows.Services;
using InventorySystem.Windows.Views.Ventas;
using Microsoft.Extensions.DependencyInjection;

namespace InventorySystem.Windows.ViewModels;

/// <summary>
/// ViewModel principal. Gestiona la navegación entre módulos.
/// En modo administrador SQL solo muestra Gestión de usuarios.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly IServiceProvider _services;
    private readonly IAuthContext _authContext;
    private readonly ILogoutService _logoutService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsExtractVisible))]
    private object? _currentViewModel;

    [ObservableProperty]
    private string _userDisplayName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUserEmail))]
    private string _userEmail = string.Empty;

    [ObservableProperty]
    private byte[]? _userPhotoBytes;

    /// <summary>
    /// True cuando la sesión es solo administrador SQL (solo gestión de usuarios).
    /// </summary>
    public bool IsSqlAdminOnly => _authContext.IsSqlAdminOnly;

    /// <summary>
    /// True cuando el usuario puede editar su perfil (login normal, no admin SQL).
    /// </summary>
    public bool CanEditProfile => _authContext.IsAuthenticated;

    /// <summary>
    /// True cuando hay email para mostrar en el menú de usuario.
    /// </summary>
    public bool HasUserEmail => !string.IsNullOrEmpty(UserEmail);

    public MainViewModel(INavigationService navigation, IServiceProvider services, IAuthContext authContext, ILogoutService logoutService)
    {
        _navigation = navigation;
        _services = services;
        _authContext = authContext;
        _logoutService = logoutService;

        _navigation.Navigated += (_, _) => CurrentViewModel = _navigation.CurrentViewModel;
        CurrentViewModel = _navigation.CurrentViewModel;

        RefreshUserInfo();

        if (CurrentViewModel is null)
        {
            if (_authContext.IsSqlAdminOnly)
                NavigateToGestionUsuariosCommand.Execute(null);
            else
                NavigateToInicioCommand.Execute(null);
        }
    }

    /// <summary>
    /// Actualiza nombre, email y foto de perfil desde el contexto de autenticación (p. ej. tras editar perfil).
    /// </summary>
    public void RefreshUserInfo()
    {
        UserDisplayName = !string.IsNullOrEmpty(_authContext.NombreParaMostrar)
            ? _authContext.NombreParaMostrar
            : (_authContext.NombreUsuario ?? "");
        if (_authContext.IsSqlAdminOnly)
            UserDisplayName = "Administrador SQL";
        UserEmail = _authContext.Email ?? "";
        UserPhotoBytes = _authContext.FotoPerfil;
    }

    [RelayCommand]
    private void NavigateToInicio() => _navigation.NavigateToInicio();

    [RelayCommand]
    private void NavigateToGestionUsuarios() =>
        _navigation.NavigateTo(_services.GetRequiredService<GestionUsuariosViewModel>());

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
    /// Indica si el botón "Extraer" debe mostrarse (cuando hay una vista de módulo, no Inicio, y no es modo admin SQL).
    /// </summary>
    public bool IsExtractVisible => !IsSqlAdminOnly && CurrentViewModel is not null and not InicioViewModel;

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

    [RelayCommand]
    private void EditProfile()
    {
        if (!CanEditProfile) return;
        var window = new Views.Auth.EditProfileWindow(
            _services.GetRequiredService<IAuthContext>(),
            _services.GetRequiredService<IUsuarioStore>());
        if (window.ShowDialog() == true)
            RefreshUserInfo();
    }

    [RelayCommand]
    private void Logout()
    {
        _logoutService.RequestLogout();
    }
}
