using System.Windows;
using InventorySystem.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace InventorySystem.Windows
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

            // Configurar DI: Infrastructure (Application + Db) + Windows (ViewModels, servicios UI)
            var services = new ServiceCollection();
            services.AddInfrastructure();
            services.AddWindows();
            Services = services.BuildServiceProvider();

            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                var mainWindow = new MainWindow();
                mainWindow.DataContext = Services.GetRequiredService<ViewModels.MainViewModel>();
                mainWindow.Closed += (_, _) => Shutdown();
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }
    }
}
