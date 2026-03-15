using System.Windows;
using ContractSystem.Application.Auth;
using ContractSystem.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ContractSystem.Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        /// <summary>
        /// Contenedor de inyección de dependencias para resolver ViewModels y servicios.
        /// </summary>
        public static IServiceProvider Services { get; private set; } = null!;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var services = new ServiceCollection();
            services.AddInfrastructure();
            services.AddWindows();
            Services = services.BuildServiceProvider();

            var loginWindow = new Views.Auth.LoginWindow();
            if (loginWindow.ShowDialog() != true)
            {
                Shutdown();
                return;
            }

            var authContext = Services.GetRequiredService<IAuthContext>();
            if (authContext.RequiereCambioContraseña)
            {
                // No usar loginWindow como Owner: ya está cerrado y el diálogo no se mostraría bien.
                var changePwdWindow = new Views.Auth.ChangePasswordWindow();
                if (changePwdWindow.ShowDialog() != true)
                {
                    authContext.Clear();
                    Shutdown();
                    return;
                }
            }

            var mainWindow = new MainWindow();
            mainWindow.DataContext = Services.GetRequiredService<ViewModels.MainViewModel>();
            Services.GetRequiredService<Services.ILogoutService>().SetMainWindow(mainWindow);
            mainWindow.Closed += (_, _) => Shutdown();
            mainWindow.Show();
        }
    }
}
