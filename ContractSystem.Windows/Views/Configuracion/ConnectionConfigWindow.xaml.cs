using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Application.Configuration;
using ContractSystem.Domain;

namespace ContractSystem.Windows.Views.Configuracion;

/// <summary>
/// Configuración de conexión a SQL Server (la app Windows solo usa SQL Server).
/// </summary>
public partial class ConnectionConfigWindow : Window
{
    private const string NuevaBasePlaceholder = " Crear nueva base de datos ";

    private readonly IConnectionConfigurationStore _configStore = App.Services.GetService(typeof(IConnectionConfigurationStore)) as IConnectionConfigurationStore
        ?? throw new InvalidOperationException("IConnectionConfigurationStore no registrado.");
    private readonly ISqlServerConnectionService _sqlService = App.Services.GetService(typeof(ISqlServerConnectionService)) as ISqlServerConnectionService
        ?? throw new InvalidOperationException("ISqlServerConnectionService no registrado.");

    public ConnectionConfigWindow()
    {
        InitializeComponent();
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

    private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
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
        DialogResult = true;
        Close();
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
