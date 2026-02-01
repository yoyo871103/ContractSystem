using System.Windows;
using System.Windows.Input;
using InventorySystem.Application.Configuration;
using InventorySystem.Domain;

namespace InventorySystem.Windows
{
    /// <summary>
    /// Ventana de login (solo visual). Al aceptar, no valida credenciales y cierra esta ventana;
    /// la aplicación abre entonces la ventana principal.
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            CargarTextoConnexionActual();
        }

        private void CargarTextoConnexionActual()
        {
            var store = App.Services?.GetService(typeof(IConnectionConfigurationStore)) as IConnectionConfigurationStore;
            if (store == null || !store.HasConnectionConfigured)
            {
                ActualizarTextoConnexion("(no configurada)");
                return;
            }
            var settings = store.GetSettings();
            if (settings == null)
            {
                ActualizarTextoConnexion("(no configurada)");
                return;
            }
            var texto = settings.Provider switch
            {
                DatabaseProvider.SqlServer => "SQL Server",
                DatabaseProvider.Sqlite when !string.IsNullOrEmpty(settings.SqliteDatabasePath)
                    => System.IO.Path.GetFileName(settings.SqliteDatabasePath),
                DatabaseProvider.Sqlite => "SQLite",
                _ => "(no configurada)"
            };
            ActualizarTextoConnexion(texto);
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
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            AceptarLogin();
        }

        private void TxtUsuario_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                TxtPassword.Focus();
            }
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                AceptarLogin();
            }
        }

        private void TxtPasswordVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TxtPassword.Password != TxtPasswordVisible.Text)
                TxtPassword.Password = TxtPasswordVisible.Text;
        }

        private void TxtPasswordVisible_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                AceptarLogin();
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

        private void AceptarLogin()
        {
            // Solo visual: no se valida usuario ni contraseña
            DialogResult = true;
            Close();
        }
    }
}
