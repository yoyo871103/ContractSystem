using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ContractSystem.Application.Auth;
using Microsoft.Win32;

namespace ContractSystem.Windows.Views.Auth;

/// <summary>
/// Ventana para editar nombre para mostrar, email y foto de perfil del usuario actual.
/// </summary>
public partial class EditProfileWindow : Window
{
    private readonly IAuthContext _authContext;
    private readonly IUsuarioStore _usuarioStore;
    private byte[]? _currentPhotoBytes;

    public EditProfileWindow(IAuthContext authContext, IUsuarioStore usuarioStore)
    {
        InitializeComponent();
        _authContext = authContext;
        _usuarioStore = usuarioStore;

        Loaded += (_, _) =>
        {
            TxtNombreUsuario.Text = _authContext.NombreUsuario ?? "";
            TxtNombre.Text = _authContext.NombreParaMostrar ?? _authContext.NombreUsuario ?? "";
            TxtEmail.Text = _authContext.Email ?? "";
            _currentPhotoBytes = _authContext.FotoPerfil;
            UpdatePhotoDisplay();
        };
    }

    private void UpdatePhotoDisplay()
    {
        if (_currentPhotoBytes is { Length: > 0 })
        {
            try
            {
                using var stream = new MemoryStream(_currentPhotoBytes);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
                ImgFoto.Source = bitmap;
                ImgFoto.Visibility = Visibility.Visible;
                TxtIconoPerfil.Visibility = Visibility.Collapsed;
            }
            catch
            {
                ImgFoto.Source = null;
                ImgFoto.Visibility = Visibility.Collapsed;
                TxtIconoPerfil.Visibility = Visibility.Visible;
            }
        }
        else
        {
            ImgFoto.Source = null;
            ImgFoto.Visibility = Visibility.Collapsed;
            TxtIconoPerfil.Visibility = Visibility.Visible;
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
        {
            try { DragMove(); } catch { }
        }
    }

    private void BtnCerrar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void BtnSeleccionarFoto_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Todos los archivos|*.*",
            Title = "Seleccionar imagen de perfil"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            _currentPhotoBytes = File.ReadAllBytes(dlg.FileName);
            UpdatePhotoDisplay();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "No se pudo cargar la imagen: " + ex.Message, "Editar perfil", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnQuitarFoto_Click(object sender, RoutedEventArgs e)
    {
        _currentPhotoBytes = null;
        UpdatePhotoDisplay();
    }

    private void BtnCambiarContraseña_Click(object sender, RoutedEventArgs e)
    {
        var changePwdWindow = new ChangePasswordWindow
        {
            IsObligatory = false,
            Owner = this
        };
        changePwdWindow.ShowDialog();
    }

    private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        var nombre = (TxtNombre.Text ?? "").Trim();
        if (string.IsNullOrEmpty(nombre))
        {
            MessageBox.Show(this, "El nombre para mostrar es obligatorio.", "Editar perfil", MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtNombre.Focus();
            return;
        }

        var email = (TxtEmail.Text ?? "").Trim();
        if (string.IsNullOrEmpty(email)) email = null;

        var usuarioId = _authContext.UsuarioId;
        if (!usuarioId.HasValue)
        {
            MessageBox.Show(this, "No hay usuario autenticado.", "Editar perfil", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        BtnGuardar.IsEnabled = false;
        try
        {
            await _usuarioStore.UpdateProfileAsync(usuarioId.Value, nombre, email, _currentPhotoBytes);
            _authContext.UpdateProfile(nombre, email, _currentPhotoBytes);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Error al guardar: " + ex.Message, "Editar perfil", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            BtnGuardar.IsEnabled = true;
        }
    }
}
