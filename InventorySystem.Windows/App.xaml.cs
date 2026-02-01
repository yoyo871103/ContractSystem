using System.Windows;

namespace InventorySystem.Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Evitar que al cerrar el login (única ventana) la app se cierre antes de abrir MainWindow
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                var mainWindow = new MainWindow();
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
