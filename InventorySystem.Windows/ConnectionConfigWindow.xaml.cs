using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InventorySystem.Application.Configuration;
using InventorySystem.Domain;
using InventorySystem.Application.DatabaseSetup;
using Microsoft.Win32;

namespace InventorySystem.Windows
{
    public partial class ConnectionConfigWindow : Window
    {
        private const string NuevaBasePlaceholder = " Crear nueva base de datos ";

        private readonly IConnectionConfigurationStore _configStore = App.Services.GetService(typeof(IConnectionConfigurationStore)) as IConnectionConfigurationStore
            ?? throw new InvalidOperationException("IConnectionConfigurationStore no registrado.");
        private readonly ISqlServerConnectionService _sqlService = App.Services.GetService(typeof(ISqlServerConnectionService)) as ISqlServerConnectionService
            ?? throw new InvalidOperationException("ISqlServerConnectionService no registrado.");
        private readonly IDatabaseSetupService _setupService = App.Services.GetService(typeof(IDatabaseSetupService)) as IDatabaseSetupService
            ?? throw new InvalidOperationException("IDatabaseSetupService no registrado.");

        public ConnectionConfigWindow()
        {
            InitializeComponent();
            CmbProvider.SelectedIndex = 0;
            CmbProvider_SelectionChanged(null!, null!);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                try { DragMove(); } catch { /* ignorar */ }
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CmbProvider_SelectionChanged(object sender, SelectionChangedEventArgs? e)
        {
            var isSqlServer = CmbProvider.SelectedIndex == 0;
            PanelSqlServer.Visibility = isSqlServer ? Visibility.Visible : Visibility.Collapsed;
            PanelSqlite.Visibility = isSqlServer ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void BtnProbarSql_Click(object sender, RoutedEventArgs e)
        {
            var server = TxtServer.Text?.Trim();
            var user = TxtSqlUser.Text?.Trim();
            var password = TxtSqlPassword.Password;
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(user))
            {
                MessageBox.Show("Indique servidor y usuario.", "Conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            BtnProbarSql.IsEnabled = false;
            try
            {
                var ok = await _sqlService.TestConnectionAsync(server, user, password, null, default);
                MessageBox.Show(ok ? "Conexión correcta." : "No se pudo conectar. Verifique servidor, usuario y contraseña.", "Conexión",
                    MessageBoxButton.OK, ok ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            finally
            {
                BtnProbarSql.IsEnabled = true;
            }
        }

        private async void BtnCargarBases_Click(object sender, RoutedEventArgs e)
        {
            var server = TxtServer.Text?.Trim();
            var user = TxtSqlUser.Text?.Trim();
            var password = TxtSqlPassword.Password;
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(user))
            {
                MessageBox.Show("Indique servidor y usuario antes de cargar bases de datos.", "Conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            BtnCargarBases.IsEnabled = false;
            CmbDatabases.Items.Clear();
            try
            {
                var list = await _sqlService.ListDatabasesAsync(server, user, password, default);
                foreach (var name in list)
                    CmbDatabases.Items.Add(name);
                CmbDatabases.Items.Add(NuevaBasePlaceholder);
                if (CmbDatabases.Items.Count > 0)
                    CmbDatabases.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar bases de datos: " + ex.Message, "Conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                BtnCargarBases.IsEnabled = true;
            }
        }

        private void CmbDatabases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = CmbDatabases.SelectedItem?.ToString();
            PanelNuevaBase.Visibility = item == NuevaBasePlaceholder ? Visibility.Visible : Visibility.Collapsed;
            if (item != NuevaBasePlaceholder)
                TxtNuevaBaseNombre.Clear();
        }

        private void BtnNuevaBase_Click(object sender, RoutedEventArgs e)
        {
            if (CmbDatabases.Items.Count == 0 || CmbDatabases.Items[CmbDatabases.Items.Count - 1]?.ToString() != NuevaBasePlaceholder)
            {
                CmbDatabases.Items.Add(NuevaBasePlaceholder);
            }
            CmbDatabases.SelectedItem = NuevaBasePlaceholder;
            PanelNuevaBase.Visibility = Visibility.Visible;
            TxtNuevaBaseNombre.Focus();
        }

        private void BtnExaminarSqlite_Click(object sender, RoutedEventArgs e)
        {
            if (RbSqliteNueva.IsChecked == true)
            {
                var dlg = new SaveFileDialog
                {
                    Filter = "Base de datos SQLite (*.db)|*.db|Todos los archivos (*.*)|*.*",
                    DefaultExt = ".db",
                    FileName = "Inventario.db"
                };
                if (dlg.ShowDialog() == true)
                    TxtSqlitePath.Text = dlg.FileName;
            }
            else
            {
                var dlg = new OpenFileDialog
                {
                    Filter = "Base de datos SQLite (*.db)|*.db|Todos los archivos (*.*)|*.*",
                    DefaultExt = ".db"
                };
                if (dlg.ShowDialog() == true)
                    TxtSqlitePath.Text = dlg.FileName;
            }
        }

        private void RbSqliteMode_Changed(object sender, RoutedEventArgs e)
        {
            // Solo actualizar hint del botón Examinar según modo
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (CmbProvider.SelectedIndex == 0)
            {
                // SQL Server
                var server = TxtServer.Text?.Trim();
                var user = TxtSqlUser.Text?.Trim();
                var password = TxtSqlPassword.Password;
                if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(user))
                {
                    MessageBox.Show("Indique servidor y usuario.", "Conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                string databaseName;
                var selected = CmbDatabases.SelectedItem?.ToString();
                if (selected == NuevaBasePlaceholder)
                {
                    databaseName = TxtNuevaBaseNombre.Text?.Trim() ?? "";
                    if (string.IsNullOrEmpty(databaseName))
                    {
                        MessageBox.Show("Indique el nombre de la nueva base de datos.", "Conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    BtnGuardar.IsEnabled = false;
                    try
                    {
                        var created = await _sqlService.CreateDatabaseAsync(server, user, password, databaseName, default);
                        if (!created)
                        {
                            MessageBox.Show("No se pudo crear la base de datos. Verifique permisos del usuario.", "Conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al crear la base de datos: " + ex.Message, "Conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    finally
                    {
                        BtnGuardar.IsEnabled = true;
                    }
                }
                else
                {
                    databaseName = selected ?? "";
                    if (string.IsNullOrEmpty(databaseName))
                    {
                        MessageBox.Show("Seleccione una base de datos o cree una nueva.", "Conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                var connectionString = _sqlService.BuildConnectionString(server, user, password, databaseName);
                _configStore.SaveSettings(ConnectionSettings.ForSqlServer(connectionString));
            }
            else
            {
                // SQLite
                var path = TxtSqlitePath.Text?.Trim();
                if (string.IsNullOrEmpty(path))
                {
                    MessageBox.Show("Indique la ruta del archivo de base de datos.", "Conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (RbSqliteNueva.IsChecked == true)
                {
                    BtnGuardar.IsEnabled = false;
                    try
                    {
                        var result = await _setupService.SetupSqliteAsync(new SqliteSetupRequest
                        {
                            DatabasePath = path,
                            CreateDirectoryIfNotExists = true
                        }, default);
                        if (!result.IsSuccess)
                        {
                            MessageBox.Show("Error al crear la base de datos: " + result.ErrorMessage, "Conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (result.Settings != null)
                            _configStore.SaveSettings(result.Settings);
                    }
                    finally
                    {
                        BtnGuardar.IsEnabled = true;
                    }
                }
                else
                {
                    if (!System.IO.File.Exists(path))
                    {
                        MessageBox.Show("El archivo indicado no existe. Use 'Crear nueva base de datos' o seleccione un archivo existente.", "Conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (!path.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                        path += ".db";
                    _configStore.SaveSettings(ConnectionSettings.ForSqlite(path));
                }
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
}
