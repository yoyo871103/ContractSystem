using System.Windows;
using System.Windows.Input;

namespace InventorySystem.Windows.Views.GestionUsuarios;

public partial class ResetPasswordWindow : Window
{
    public ResetPasswordWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Nombre del usuario al que se le resetea la contraseña (solo para mostrar).
    /// </summary>
    public string NombreUsuario
    {
        get => TxtUsuario.Text ?? "";
        set => TxtUsuario.Text = value;
    }

    /// <summary>
    /// Contraseña temporal introducida o generada (solo tiene valor si DialogResult == true).
    /// </summary>
    public string ContraseñaTemporal => TxtPassword.Password ?? "";

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
        {
            try { DragMove(); } catch { /* ignorar */ }
        }
    }

    private void BtnGenerar_Click(object sender, RoutedEventArgs e)
    {
        TxtPassword.Password = GenerarContraseñaTemporal();
    }

    private static string GenerarContraseñaTemporal()
    {
        const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string Lower = "abcdefghjkmnpqrstuvwxyz";
        const string Digits = "23456789";
        const string Symbols = "!@#$%&*";
        const int Length = 12;

        var password = new char[Length];
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(Length);

        password[0] = Upper[bytes[0] % Upper.Length];
        password[1] = Lower[bytes[1] % Lower.Length];
        password[2] = Digits[bytes[2] % Digits.Length];
        password[3] = Symbols[bytes[3] % Symbols.Length];

        var allChars = Upper + Lower + Digits + Symbols;
        for (var i = 4; i < Length; i++)
            password[i] = allChars[bytes[i] % allChars.Length];

        for (var i = Length - 1; i > 0; i--)
        {
            var j = bytes[i % 4] % (i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }

    private void BtnAceptar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtPassword.Password) || TxtPassword.Password.Length < 6)
        {
            MessageBox.Show("La contraseña temporal debe tener al menos 6 caracteres.", "Resetear contraseña",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        DialogResult = true;
        Close();
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
