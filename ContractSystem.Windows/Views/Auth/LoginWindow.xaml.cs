using System.Windows;
using System.Windows.Input;
using ContractSystem.Application.Auth;
using ContractSystem.Application.Configuration;
using ContractSystem.Windows.Views.Configuracion;
using Microsoft.Extensions.DependencyInjection;

namespace ContractSystem.Windows.Views.Auth;

/// <summary>
/// Ventana de login. Valida credenciales contra usuarios de la aplicación o, si corresponde,
/// contra administrador SQL (solo cuando la conexión es SQL Server).
/// </summary>
public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        CargarTextoConnexionActual();
        ActualizarVisibilidadAdminSql();
    }

    private void CargarTextoConnexionActual()
    {
        var store = App.Services?.GetService(typeof(IConnectionConfigurationStore)) as IConnectionConfigurationStore;
        if (store == null || !store.HasConnectionConfigured)
        {
            ActualizarTextoConnexion("(no configurada)");
            return;
        }
        var info = store.GetSqlServerConnectionInfo();
        if (!info.HasValue)
        {
            ActualizarTextoConnexion("(no configurada)");
            return;
        }
        var (server, database) = info.Value;
        var texto = string.IsNullOrEmpty(database)
            ? $"SQL Server — {server ?? ""}"
            : $"SQL Server — {server ?? ""}\\{database}";
        ActualizarTextoConnexion(string.IsNullOrWhiteSpace(texto) ? "(no configurada)" : texto);
    }

    /// <summary>
    /// Actualiza el texto que indica la conexión actual (solo visual por ahora).
    /// </summary>
    public void ActualizarTextoConnexion(string texto)
    {
        TxtConexionActual.Text = string.IsNullOrWhiteSpace(texto) ? "(no configurada)" : texto;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
        {
            try { DragMove(); } catch { /* ignorar */ }
        }
    }

    private void BtnConfigConexion_Click(object sender, RoutedEventArgs e)
    {
        var configWindow = new ConnectionConfigWindow
        {
            Owner = this
        };
        if (configWindow.ShowDialog() == true)
        {
            CargarTextoConnexionActual();
            ActualizarVisibilidadAdminSql();
        }
    }

    private void ActualizarVisibilidadAdminSql()
    {
        var store = App.Services?.GetService(typeof(IConnectionConfigurationStore)) as IConnectionConfigurationStore;
        var info = store?.GetSqlServerConnectionInfo();
        var visible = info.HasValue ? Visibility.Visible : Visibility.Collapsed;
        if (ChkAdminSql != null) ChkAdminSql.Visibility = visible;
        if (PanelAdminSql != null) PanelAdminSql.Visibility = visible;
    }

    private void BtnCerrar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void BtnIniciarSesion_Click(object sender, RoutedEventArgs e)
    {
        await AceptarLoginAsync();
    }

    private void TxtUsuario_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            TxtPassword.Focus();
        }
    }

    private async void TxtPassword_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await AceptarLoginAsync();
        }
    }

    private void TxtPasswordVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (TxtPassword.Password != TxtPasswordVisible.Text)
            TxtPassword.Password = TxtPasswordVisible.Text;
    }

    private async void TxtPasswordVisible_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await AceptarLoginAsync();
        }
    }

    private void BtnVerPassword_Click(object sender, RoutedEventArgs e)
    {
        if (TxtPasswordVisible.Visibility == Visibility.Visible)
        {
            TxtPassword.Password = TxtPasswordVisible.Text;
            TxtPassword.Visibility = Visibility.Visible;
            TxtPasswordVisible.Visibility = Visibility.Collapsed;
            TxtPasswordVisible.Clear();
            IconoOjo.Text = "\uE890"; // View: ojo para mostrar contraseña
            BtnVerPassword.ToolTip = "Mostrar contraseña";
        }
        else
        {
            TxtPasswordVisible.Text = TxtPassword.Password;
            TxtPasswordVisible.Visibility = Visibility.Visible;
            TxtPassword.Visibility = Visibility.Collapsed;
            IconoOjo.Text = "\uED1A"; // Hide: ojo tachado para ocultar contraseña
            BtnVerPassword.ToolTip = "Ocultar contraseña";
            TxtPasswordVisible.Focus();
        }
    }

    private async System.Threading.Tasks.Task AceptarLoginAsync()
    {
        var usuario = TxtUsuario.Text?.Trim() ?? "";
        var contraseña = TxtPassword.Password ?? "";

        if (ChkAdminSql?.IsChecked == true)
        {
            var store = App.Services?.GetService(typeof(IConnectionConfigurationStore)) as IConnectionConfigurationStore;
            var info = store?.GetSqlServerConnectionInfo();
            if (!info.HasValue)
            {
                MessageBox.Show("No hay conexión SQL Server configurada.", "Iniciar sesión", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var (server, database) = info.Value;
            if (string.IsNullOrWhiteSpace(usuario))
            {
                MessageBox.Show("Indique el usuario de administrador SQL (p. ej. sa).", "Iniciar sesión", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            BtnIniciarSesion.IsEnabled = false;
            try
            {
                var sqlAdmin = App.Services?.GetRequiredService<ISqlServerAdminAuthService>();
                var result = await sqlAdmin.ValidateSqlAdminAsync(server ?? "", usuario, contraseña, database, default);
                if (result.IsSuccess)
                {
                    var seedService = App.Services?.GetRequiredService<ISeedDataService>();
                    if (seedService is not null)
                        await seedService.EnsureSeedAsync(default);

                    var authContext = App.Services?.GetRequiredService<IAuthContext>();
                    authContext?.SetSqlAdminOnly();
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage ?? "No se pudo validar el administrador SQL.", "Iniciar sesión", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Iniciar sesión", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                BtnIniciarSesion.IsEnabled = true;
            }
            return;
        }

        if (string.IsNullOrWhiteSpace(usuario))
        {
            MessageBox.Show("Indique el usuario.", "Iniciar sesión", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        BtnIniciarSesion.IsEnabled = false;
        try
        {
            var authService = App.Services?.GetRequiredService<IAuthService>();
            var authContext = App.Services?.GetRequiredService<IAuthContext>();
            if (authService == null || authContext == null)
            {
                MessageBox.Show("Servicios de autenticación no disponibles.", "Iniciar sesión", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = await authService.AuthenticateAsync(usuario, contraseña, default);
            if (result.IsSuccess && result.User != null)
            {
                authContext.SetUser(result.User);
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(result.ErrorMessage ?? "Usuario o contraseña incorrectos.", "Iniciar sesión", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Iniciar sesión", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al conectar con la base de datos. Verifique la conexión.\n" + ex.Message, "Iniciar sesión", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            BtnIniciarSesion.IsEnabled = true;
        }
    }
}
