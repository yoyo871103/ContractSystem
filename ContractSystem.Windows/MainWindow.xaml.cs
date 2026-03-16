using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using ContractSystem.Windows.ViewModels;

namespace ContractSystem.Windows
{
    /// <summary>
    /// Ventana principal con área de trabajo (ContentArea) para cargar vistas dinámicamente.
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _closeButtonPressed;
        private DateTime _lastDragEndTime = DateTime.MinValue;
        private const int DragEndIgnoreMs = 300;

        /// <summary>Maximizar = rellenar área de trabajo (barra de tareas visible). No usamos WindowState.Maximized.</summary>
        private bool _isMaximizedToWorkArea;
        private Rect? _restoreBoundsForMaximize;

        /// <summary>Pantalla completa = ventana cubre todo (incluye barra de tareas).</summary>
        private bool _isFullScreen;
        private Rect? _restoreBoundsBeforeFullScreen;

        private static string WindowStatePath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ContractSystem",
                "window.json");

        public MainWindow()
        {
            InitializeComponent();
            RestoreWindowState();
            DataContextChanged += (_, _) =>
            {
                if (DataContext is MainViewModel vm)
                    vm.PropertyChanged += MainVm_PropertyChanged;
            };
        }

        private void MainVm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.ActiveSection))
                UpdateActiveIndicators();
        }

        private void UpdateActiveIndicators()
        {
            if (DataContext is not MainViewModel vm) return;
            var s = vm.ActiveSection;
            IndInicio.Visibility = s == "inicio" ? Visibility.Visible : Visibility.Collapsed;
            IndContratos.Visibility = s == "contratos" ? Visibility.Visible : Visibility.Collapsed;
            IndArbol.Visibility = s == "arbol" ? Visibility.Visible : Visibility.Collapsed;
            IndExpediente.Visibility = s == "expediente" ? Visibility.Visible : Visibility.Collapsed;
            IndTerceros.Visibility = s == "terceros" ? Visibility.Visible : Visibility.Collapsed;
            IndProductos.Visibility = s == "productos" ? Visibility.Visible : Visibility.Collapsed;
            IndPlantillas.Visibility = s == "plantillas" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowState();
        }

        private void RestoreWindowState()
        {
            try
            {
                var path = WindowStatePath;
                if (!File.Exists(path)) return;

                var json = File.ReadAllText(path);
                var state = JsonSerializer.Deserialize<SavedWindowState>(json);
                if (state is null) return;

                // Asegurar que la ventana quede dentro de algún monitor
                var (left, top) = ClampToWorkingArea(state.Left, state.Top, state.Width, state.Height);

                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = left;
                Top = top;
                Width = Math.Max(MinWidth, state.Width);
                Height = Math.Max(MinHeight, state.Height);

                if (state.IsMaximized)
                {
                    ApplyMaximizeToWorkArea();
                    TxtMaximizeIcon.Text = "\uE923";
                }
            }
            catch
            {
                // Si falla la lectura (archivo corrupto, etc.), usar valores por defecto
            }
        }

        private void SaveWindowState()
        {
            try
            {
                double left, top, width, height;
                bool isMaximized;
                if (_isFullScreen && _restoreBoundsBeforeFullScreen.HasValue)
                {
                    var r = _restoreBoundsBeforeFullScreen.Value;
                    left = r.Left;
                    top = r.Top;
                    width = r.Width;
                    height = r.Height;
                    isMaximized = _isMaximizedToWorkArea;
                }
                else if (_isMaximizedToWorkArea && _restoreBoundsForMaximize.HasValue)
                {
                    var r = _restoreBoundsForMaximize.Value;
                    left = r.Left;
                    top = r.Top;
                    width = r.Width;
                    height = r.Height;
                    isMaximized = true;
                }
                else
                {
                    left = Left;
                    top = Top;
                    width = Width;
                    height = Height;
                    isMaximized = false;
                }

                var state = new SavedWindowState
                {
                    Left = left,
                    Top = top,
                    Width = width,
                    Height = height,
                    IsMaximized = isMaximized
                };

                var dir = Path.GetDirectoryName(WindowStatePath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = false });
                File.WriteAllText(WindowStatePath, json);
            }
            catch
            {
                // Ignorar errores al guardar
            }
        }

        private static (double left, double top) ClampToWorkingArea(double left, double top, double width, double height)
        {
            try
            {
                var workingArea = System.Windows.SystemParameters.WorkArea;
                var maxLeft = workingArea.Right - Math.Max(width, 400);
                var maxTop = workingArea.Bottom - Math.Max(height, 300);
                var clampedLeft = Math.Max(workingArea.Left, Math.Min(left, maxLeft));
                var clampedTop = Math.Max(workingArea.Top, Math.Min(top, maxTop));
                return (clampedLeft, clampedTop);
            }
            catch
            {
                return (left, top);
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || e.ButtonState != MouseButtonState.Pressed)
                return;

            // Doble clic en la barra de título: maximizar o restaurar (como ventanas de Windows)
            if (e.ClickCount == 2)
            {
                BtnMaximize_Click(sender, e);
                e.Handled = true;
                return;
            }

            try
            {
                DragMove();
                _lastDragEndTime = DateTime.UtcNow;
            }
            catch (InvalidOperationException)
            {
                // DragMove solo es válido cuando el botón está presionado; en doble clic u otros casos se ignora.
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>Maximiza la ventana al área de trabajo (respeta la barra de tareas de Windows).</summary>
        private void ApplyMaximizeToWorkArea()
        {
            WindowState = WindowState.Normal;
            _restoreBoundsForMaximize = new Rect(Left, Top, Width, Height);
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Left;
            Top = workArea.Top;
            Width = workArea.Width;
            Height = workArea.Height;
            _isMaximizedToWorkArea = true;
        }

        /// <summary>Restaura desde maximizado al área de trabajo.</summary>
        private void RestoreFromMaximizeToWorkArea()
        {
            if (!_restoreBoundsForMaximize.HasValue) return;
            var r = _restoreBoundsForMaximize.Value;
            Left = r.Left;
            Top = r.Top;
            Width = Math.Max(MinWidth, r.Width);
            Height = Math.Max(MinHeight, r.Height);
            _restoreBoundsForMaximize = null;
            _isMaximizedToWorkArea = false;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (_isFullScreen)
            {
                ExitFullScreen();
                ApplyMaximizeToWorkArea();
                TxtMaximizeIcon.Text = "\uE923";
                TxtFullScreenIcon.Text = "\uE740";
                return;
            }

            if (_isMaximizedToWorkArea)
            {
                RestoreFromMaximizeToWorkArea();
                TxtMaximizeIcon.Text = "\uE922";
            }
            else
            {
                ApplyMaximizeToWorkArea();
                TxtMaximizeIcon.Text = "\uE923";
            }
        }

        private void BtnFullScreen_Click(object sender, RoutedEventArgs e)
        {
            if (_isFullScreen)
            {
                ExitFullScreen();
                TxtFullScreenIcon.Text = "\uE740";
                return;
            }

            _restoreBoundsBeforeFullScreen = new Rect(Left, Top, Width, Height);
            Left = 0;
            Top = 0;
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;
            WindowState = WindowState.Normal;
            _isFullScreen = true;
            TxtFullScreenIcon.Text = "\uE73F"; // icono "salir pantalla completa"
        }

        private void ExitFullScreen()
        {
            if (!_restoreBoundsBeforeFullScreen.HasValue) return;
            var r = _restoreBoundsBeforeFullScreen.Value;
            Left = r.Left;
            Top = r.Top;
            Width = Math.Max(MinWidth, r.Width);
            Height = Math.Max(MinHeight, r.Height);
            _restoreBoundsBeforeFullScreen = null;
            _isFullScreen = false;
        }

        private void BtnClose_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _closeButtonPressed = true;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (!_closeButtonPressed) return;
            _closeButtonPressed = false;

            // Evitar cierre accidental: si el usuario acaba de soltar tras arrastrar la ventana,
            // el MouseLeftButtonUp puede llegar al botón Cerrar y disparar Click sin intención.
            var elapsed = (DateTime.UtcNow - _lastDragEndTime).TotalMilliseconds;
            if (elapsed >= 0 && elapsed < DragEndIgnoreMs)
                return;

            Close();
        }

        private void BtnUser_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = true;
        }

        private void UserMenu_OnOptionClick(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = false;
        }
    }

    internal sealed class SavedWindowState
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsMaximized { get; set; }
    }
}