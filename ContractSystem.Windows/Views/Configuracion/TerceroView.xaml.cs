using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Application.Nomencladores;
using ContractSystem.Application.Nomencladores.Commands.CreateTercero;
using ContractSystem.Domain.Nomencladores;
using ContractSystem.Windows.ViewModels;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace ContractSystem.Windows.Views.Configuracion;

public partial class TerceroView : UserControl
{
    public TerceroView()
    {
        InitializeComponent();
    }

    private void CmbFiltroTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not TerceroViewModel vm) return;
        if (CmbFiltroTipo.SelectedItem is not ComboBoxItem item) return;

        var tag = item.Tag?.ToString();
        vm.FiltroTipo = tag switch
        {
            "0" => TipoTercero.Cliente,
            "1" => TipoTercero.Proveedor,
            "2" => TipoTercero.Ambos,
            _ => null
        };
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is TerceroViewModel vm && vm.EditarCommand.CanExecute(null))
            vm.EditarCommand.Execute(null);
    }

    private void BtnDescargarPlantilla_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Guardar plantilla CSV",
            Filter = "Archivos CSV (*.csv)|*.csv",
            FileName = "plantilla_terceros.csv",
            DefaultExt = ".csv"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Codigo;Nombre;RazonSocial;NifCif;Tipo;Direccion;Telefono;Email;UbicacionExpediente");
            sb.AppendLine("T001;Empresa Ejemplo S.A.;Empresa Ejemplo S.A. de C.V.;B12345678;Cliente;Calle Principal 123;+34 912345678;contacto@ejemplo.com;Estante 3-A");
            sb.AppendLine("T002;Proveedor Demo S.L.;;A87654321;Proveedor;Av. Secundaria 456;+34 987654321;info@proveedor.com;");

            File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show($"Plantilla guardada en:\n{dialog.FileName}\n\nValores válidos para Tipo: Cliente, Proveedor, Ambos",
                "Plantilla descargada", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al guardar la plantilla: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnImportarCsv_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Seleccionar archivo CSV de terceros",
            Filter = "Archivos CSV (*.csv)|*.csv|Todos los archivos (*.*)|*.*",
            DefaultExt = ".csv"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var lines = await File.ReadAllLinesAsync(dialog.FileName, Encoding.UTF8);
            if (lines.Length < 2)
            {
                MessageBox.Show("El archivo está vacío o solo contiene la cabecera.", "Importar CSV", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Detect separator: ; or ,
            var header = lines[0];
            var separator = header.Contains(';') ? ';' : ',';
            var columns = header.Split(separator).Select(c => c.Trim().ToLowerInvariant()).ToArray();

            // Map column indices
            int colCodigo = Array.IndexOf(columns, "codigo");
            int colNombre = Array.IndexOf(columns, "nombre");
            int colRazon = Array.IndexOf(columns, "razonsocial");
            int colNif = Array.IndexOf(columns, "nifcif");
            int colTipo = Array.IndexOf(columns, "tipo");
            int colDireccion = Array.IndexOf(columns, "direccion");
            int colTelefono = Array.IndexOf(columns, "telefono");
            int colEmail = Array.IndexOf(columns, "email");
            int colUbicacion = Array.IndexOf(columns, "ubicacionexpediente");

            if (colNombre < 0)
            {
                MessageBox.Show("La columna 'Nombre' es obligatoria y no se encontró en la cabecera.\n\nColumnas detectadas: " + string.Join(", ", columns),
                    "Error de formato", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sender_ = App.Services.GetRequiredService<ISender>();
            int importados = 0;
            int errores = 0;
            var erroresDetalle = new StringBuilder();

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var fields = ParseCsvLine(line, separator);
                try
                {
                    var nombre = GetField(fields, colNombre);
                    if (string.IsNullOrWhiteSpace(nombre))
                    {
                        errores++;
                        erroresDetalle.AppendLine($"Fila {i + 1}: Nombre vacío, se omite.");
                        continue;
                    }

                    var tipoStr = GetField(fields, colTipo)?.Trim() ?? "";
                    var tipo = ParseTipoTercero(tipoStr);

                    await sender_.Send(new CreateTerceroCommand(
                        Codigo: GetField(fields, colCodigo),
                        Nombre: nombre,
                        RazonSocial: GetField(fields, colRazon) ?? "",
                        NifCif: GetField(fields, colNif) ?? "",
                        Direccion: GetField(fields, colDireccion) ?? "",
                        Telefono: GetField(fields, colTelefono) ?? "",
                        Email: GetField(fields, colEmail) ?? "",
                        Tipo: tipo,
                        UbicacionExpediente: GetField(fields, colUbicacion),
                        Contactos: Array.Empty<ContactoTerceroDto>()));

                    importados++;
                }
                catch (Exception ex)
                {
                    errores++;
                    erroresDetalle.AppendLine($"Fila {i + 1}: {ex.Message}");
                }
            }

            var msg = $"Importación completada.\n\nImportados: {importados}\nErrores: {errores}";
            if (errores > 0)
                msg += $"\n\nDetalle de errores:\n{erroresDetalle}";

            MessageBox.Show(msg, "Importar CSV",
                MessageBoxButton.OK,
                errores > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);

            // Reload list
            if (DataContext is TerceroViewModel vm)
                vm.CargarCommand.Execute(null);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al leer el archivo CSV: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static string? GetField(string[] fields, int index)
    {
        if (index < 0 || index >= fields.Length) return null;
        var val = fields[index].Trim();
        return string.IsNullOrEmpty(val) ? null : val;
    }

    private static TipoTercero ParseTipoTercero(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return TipoTercero.Cliente;

        return value.ToLowerInvariant() switch
        {
            "proveedor" => TipoTercero.Proveedor,
            "1" => TipoTercero.Proveedor,
            "ambos" => TipoTercero.Ambos,
            "2" => TipoTercero.Ambos,
            _ => TipoTercero.Cliente
        };
    }

    /// <summary>
    /// Simple CSV line parser that handles quoted fields.
    /// </summary>
    private static string[] ParseCsvLine(string line, char separator)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++; // skip escaped quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == separator && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        fields.Add(current.ToString());
        return fields.ToArray();
    }
}
