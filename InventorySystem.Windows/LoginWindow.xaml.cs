using System.Windows;
using System.Windows.Input;

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
            ActualizarTextoConnexion("(no configurada)");
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
            // Por ahora solo visual: no se implementa configuración de conexión
            MessageBox.Show("Configuración de conexión (próximamente).", "Conexión", MessageBoxButton.OK);
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
