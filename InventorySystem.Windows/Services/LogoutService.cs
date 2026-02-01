using System.Windows;
using InventorySystem.Application.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace InventorySystem.Windows.Services;

/// <summary>
/// Implementación de logout: oculta MainWindow, muestra LoginWindow y opcionalmente ChangePasswordWindow, luego muestra de nuevo MainWindow.
/// </summary>
public sealed class LogoutService : ILogoutService
{
    private readonly IServiceProvider _services;
    private Window? _mainWindow;

    public LogoutService(IServiceProvider services)
    {
        _services = services;
    }

    public void SetMainWindow(Window? mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public void RequestLogout()
    {
        var main = _mainWindow;
        main?.Hide();

        var authContext = _services.GetRequiredService<IAuthContext>();
        authContext.Clear();

        var loginWindow = new Views.Auth.LoginWindow();
        if (loginWindow.ShowDialog() != true)
        {
            main?.Close();
            System.Windows.Application.Current.Shutdown();
            return;
        }

        if (authContext.RequiereCambioContraseña)
        {
            var changePwdWindow = new Views.Auth.ChangePasswordWindow();
            if (changePwdWindow.ShowDialog() != true)
            {
                authContext.Clear();
                main?.Close();
                System.Windows.Application.Current.Shutdown();
                return;
            }
        }

        // Refrescar el MainViewModel con el nuevo usuario (nombre, permisos, etc.) y navegar a la vista inicial correcta
        if (main?.DataContext is ViewModels.MainViewModel mainVm)
        {
            mainVm.RefreshUserInfo();
            mainVm.NavigateToInicioCommand.Execute(null);
            if (authContext.IsSqlAdminOnly)
                mainVm.NavigateToConfiguracionCommand.Execute(null);
        }

        main?.Show();
    }
}
