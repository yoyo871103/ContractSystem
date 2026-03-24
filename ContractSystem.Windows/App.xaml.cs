using System.Windows;
using ContractSystem.Application.Auth;
using ContractSystem.Application.Contratos.Commands.ActualizarVencimientos;
using ContractSystem.Application.Licensing;
using ContractSystem.Infrastructure;
using ContractSystem.Windows.Views.Licensing;
using MediatR;
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

            // Validar licencia después del login (la BD ya está configurada)
            if (!authContext.IsSqlAdminOnly && !ValidarLicencia())
            {
                authContext.Clear();
                Shutdown();
                return;
            }

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

            // Verificar vencimientos automáticos al iniciar
            _ = Task.Run(async () =>
            {
                try
                {
                    var sender = Services.GetRequiredService<ISender>();
                    await sender.Send(new ActualizarVencimientosCommand());
                }
                catch { /* silenciar errores de vencimiento automático */ }
            });
        }

        /// <summary>
        /// Valida la licencia y muestra ventana de activación si es necesario.
        /// Retorna true si la app puede continuar, false si debe cerrarse.
        /// </summary>
        private bool ValidarLicencia()
        {
            var licenciaService = Services.GetRequiredService<ILicenciaService>();
            LicenciaValidationResult resultado;

            try
            {
                resultado = Task.Run(() => licenciaService.ValidarLicenciaAsync()).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                // Si hay error al validar, mostrar ventana de activación para no bloquear
                var activacionWindow = new ActivacionWindow(licenciaService,
                    $"Error al validar licencia: {ex.Message}\nIntente activar manualmente.");
                return activacionWindow.ShowDialog() == true;
            }

            // Sin licencia o inválida → mostrar ventana de activación
            if (!resultado.EsValida && !resultado.Expirada)
            {
                var activacionWindow = new ActivacionWindow(licenciaService, resultado.Mensaje, resultado.Fingerprint);
                return activacionWindow.ShowDialog() == true;
            }

            // Licencia expirada → mostrar ventana de activación con mensaje
            if (resultado.Expirada)
            {
                var mensaje = $"Su licencia expiró el {resultado.FechaExpiracion:dd/MM/yyyy}. " +
                              "Contacte al proveedor para renovar.";
                var activacionWindow = new ActivacionWindow(licenciaService, mensaje, resultado.Fingerprint);
                return activacionWindow.ShowDialog() == true;
            }

            // Licencia válida — verificar si está próxima a vencer
            if (resultado.DiasRestantes <= 30)
            {
                var contacto = "yoyo871103@gmail.com | (+53) 5 555-1803";
                var urgencia = resultado.DiasRestantes <= 7 ? "URGENTE: " : "";
                var mensaje = $"{urgencia}Su licencia vence en {resultado.DiasRestantes} día(s) " +
                              $"(el {resultado.FechaExpiracion:dd/MM/yyyy}).\n\n" +
                              $"Contacte al proveedor para renovar:\n{contacto}";
                var icono = resultado.DiasRestantes <= 7 ? MessageBoxImage.Warning : MessageBoxImage.Information;

                MessageBox.Show(mensaje, "Aviso de licencia", MessageBoxButton.OK, icono);
            }

            return true;
        }
    }
}
