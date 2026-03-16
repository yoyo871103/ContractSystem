using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.Auth;
using ContractSystem.Windows.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ContractSystem.Windows.ViewModels;

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
    [NotifyPropertyChangedFor(nameof(ActiveSection))]
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
    /// True cuando el usuario puede acceder a Configuración (admin SQL, rol Administrador o permiso Gestionar Usuarios).
    /// </summary>
    public bool CanAccessConfiguracion => _authContext.IsSqlAdminOnly || _authContext.EsAdministrador
        || _authContext.TienePermiso(Permissions.GestionarUsuarios);

    /// <summary>
    /// True cuando el usuario puede editar su perfil (login normal, no admin SQL).
    /// </summary>
    public bool CanEditProfile => _authContext.IsAuthenticated;

    /// <summary>
    /// True cuando hay email para mostrar en el menú de usuario.
    /// </summary>
    public bool HasUserEmail => !string.IsNullOrEmpty(UserEmail);

    /// <summary>
    /// Devuelve el nombre de la sección activa para resaltar el botón del menú lateral.
    /// </summary>
    public string ActiveSection => CurrentViewModel switch
    {
        InicioViewModel => "inicio",
        ContratosViewModel => "contratos",
        ArbolContratosViewModel => "arbol",
        ExpedienteTerceroViewModel => "expediente",
        TerceroViewModel => "terceros",
        ProductoServicioViewModel => "productos",
        ConfiguracionViewModel => "config",
        _ => ""
    };

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
                NavigateToConfiguracionCommand.Execute(null);
            else
                NavigateToInicioCommand.Execute(null);
        }
    }

    /// <summary>
    /// Actualiza nombre, email y foto de perfil desde el contexto de autenticación (p. ej. tras editar perfil o tras un nuevo login).
    /// También notifica las propiedades calculadas (IsSqlAdminOnly, CanAccessConfiguracion, etc.) para que la UI se actualice.
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

        OnPropertyChanged(nameof(IsSqlAdminOnly));
        OnPropertyChanged(nameof(CanAccessConfiguracion));
        OnPropertyChanged(nameof(CanEditProfile));
    }

    [RelayCommand]
    private void NavigateToInicio() => _navigation.NavigateToInicio();

    [RelayCommand]
    private void NavigateToGestionUsuarios() =>
        _navigation.NavigateTo(_services.GetRequiredService<GestionUsuariosViewModel>());


    [RelayCommand]
    private void NavigateToContratos() =>
        _navigation.NavigateTo(_services.GetRequiredService<ContratosViewModel>());

    [RelayCommand]
    private void NavigateToArbol() =>
        _navigation.NavigateTo(_services.GetRequiredService<ArbolContratosViewModel>());

    [RelayCommand]
    private void NavigateToExpediente() =>
        _navigation.NavigateTo(_services.GetRequiredService<ExpedienteTerceroViewModel>());

    [RelayCommand]
    private void NavigateToTerceros() =>
        _navigation.NavigateTo(_services.GetRequiredService<TerceroViewModel>());

    [RelayCommand]
    private void NavigateToProductos() =>
        _navigation.NavigateTo(_services.GetRequiredService<ProductoServicioViewModel>());

    [RelayCommand]
    private void NavigateToConfiguracion() =>
        _navigation.NavigateTo(_services.GetRequiredService<ConfiguracionViewModel>());

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
