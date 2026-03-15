using System.Windows;
using System.Windows.Input;
using ContractSystem.Application.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace ContractSystem.Windows.Views.Auth;

public partial class ChangePasswordWindow : Window
{
    /// <summary>
    /// True cuando el cambio es obligatorio (primer login); false cuando se abre desde el perfil.
    /// </summary>
    public bool IsObligatory { get; set; } = true;

    public ChangePasswordWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (IsObligatory)
        {
            Title = "Cambiar contraseña obligatoria";
            TxtHeader.Text = "Debe cambiar su contraseña antes de continuar";
        }
        else
        {
            Title = "Cambiar contraseña";
            TxtHeader.Text = "Indique la contraseña actual y la nueva.";
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
        {
            try { DragMove(); } catch { /* ignorar */ }
        }
    }

    private void TxtActualVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (TxtActual.Password != TxtActualVisible.Text)
            TxtActual.Password = TxtActualVisible.Text;
    }

    private void TxtNuevaVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (TxtNueva.Password != TxtNuevaVisible.Text)
            TxtNueva.Password = TxtNuevaVisible.Text;
    }

    private void TxtConfirmarVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (TxtConfirmar.Password != TxtConfirmarVisible.Text)
            TxtConfirmar.Password = TxtConfirmarVisible.Text;
    }

    private void BtnVerActual_Click(object sender, RoutedEventArgs e)
    {
        if (TxtActualVisible.Visibility == Visibility.Visible)
        {
            TxtActual.Password = TxtActualVisible.Text;
            TxtActual.Visibility = Visibility.Visible;
            TxtActualVisible.Visibility = Visibility.Collapsed;
            TxtActualVisible.Clear();
            IconoOjoActual.Text = "\uE890";
            BtnVerActual.ToolTip = "Mostrar contraseña";
        }
        else
        {
            TxtActualVisible.Text = TxtActual.Password;
            TxtActualVisible.Visibility = Visibility.Visible;
            TxtActual.Visibility = Visibility.Collapsed;
            IconoOjoActual.Text = "\uED1A";
            BtnVerActual.ToolTip = "Ocultar contraseña";
            TxtActualVisible.Focus();
        }
    }

    private void BtnVerNueva_Click(object sender, RoutedEventArgs e)
    {
        if (TxtNuevaVisible.Visibility == Visibility.Visible)
        {
            TxtNueva.Password = TxtNuevaVisible.Text;
            TxtNueva.Visibility = Visibility.Visible;
            TxtNuevaVisible.Visibility = Visibility.Collapsed;
            TxtNuevaVisible.Clear();
            IconoOjoNueva.Text = "\uE890";
            BtnVerNueva.ToolTip = "Mostrar contraseña";
        }
        else
        {
            TxtNuevaVisible.Text = TxtNueva.Password;
            TxtNuevaVisible.Visibility = Visibility.Visible;
            TxtNueva.Visibility = Visibility.Collapsed;
            IconoOjoNueva.Text = "\uED1A";
            BtnVerNueva.ToolTip = "Ocultar contraseña";
            TxtNuevaVisible.Focus();
        }
    }

    private void BtnVerConfirmar_Click(object sender, RoutedEventArgs e)
    {
        if (TxtConfirmarVisible.Visibility == Visibility.Visible)
        {
            TxtConfirmar.Password = TxtConfirmarVisible.Text;
            TxtConfirmar.Visibility = Visibility.Visible;
            TxtConfirmarVisible.Visibility = Visibility.Collapsed;
            TxtConfirmarVisible.Clear();
            IconoOjoConfirmar.Text = "\uE890";
            BtnVerConfirmar.ToolTip = "Mostrar contraseña";
        }
        else
        {
            TxtConfirmarVisible.Text = TxtConfirmar.Password;
            TxtConfirmarVisible.Visibility = Visibility.Visible;
            TxtConfirmar.Visibility = Visibility.Collapsed;
            IconoOjoConfirmar.Text = "\uED1A";
            BtnVerConfirmar.ToolTip = "Ocultar contraseña";
            TxtConfirmarVisible.Focus();
        }
    }

    private async void BtnAceptar_Click(object sender, RoutedEventArgs e)
    {
        var actual = TxtActual.Password ?? "";
        var nueva = TxtNueva.Password ?? "";
        var confirmar = TxtConfirmar.Password ?? "";

        if (string.IsNullOrEmpty(actual))
        {
            MessageBox.Show("Indique la contraseña actual.", "Cambiar contraseña", MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtActual.Focus();
            return;
        }
        if (string.IsNullOrEmpty(nueva) || nueva.Length < 6)
        {
            MessageBox.Show("La nueva contraseña debe tener al menos 6 caracteres.", "Cambiar contraseña", MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtNueva.Focus();
            return;
        }
        if (nueva != confirmar)
        {
            MessageBox.Show("La confirmación de la nueva contraseña no coincide.", "Cambiar contraseña", MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtConfirmar.Focus();
            return;
        }

        var authContext = App.Services?.GetRequiredService<IAuthContext>();
        var usuarioId = authContext?.UsuarioId;
        if (usuarioId == null)
        {
            MessageBox.Show("Sesión no disponible.", "Cambiar contraseña", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        BtnAceptar.IsEnabled = false;
        try
        {
            var authService = App.Services?.GetRequiredService<IAuthService>();
            var ok = await authService!.ChangePasswordAsync(usuarioId.Value, actual, nueva, default);
            if (ok)
            {
                authContext?.ClearRequiresPasswordChange();
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("La contraseña actual no es correcta.", "Cambiar contraseña", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtActual.Clear();
                TxtActual.Focus();
            }
        }
        finally
        {
            BtnAceptar.IsEnabled = true;
        }
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        DialogResult = false;
        Close();
    }
}
