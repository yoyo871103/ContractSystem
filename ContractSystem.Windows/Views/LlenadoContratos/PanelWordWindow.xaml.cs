using System.Windows;
using System.Windows.Media;
using ContractSystem.Domain.Contratos;
using ContractSystem.Windows.Services;
using ContractSystem.Windows.ViewModels.LlenadoContratos;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ContractSystem.Windows.Views.LlenadoContratos;

/// <summary>
/// Panel flotante (siempre visible) que inserta datos en Word con un clic.
/// Hay una sola instancia: abrirlo de nuevo desde otro contrato solo cambia el contexto.
/// </summary>
public partial class PanelWordWindow : Window
{
    private static PanelWordWindow? _instancia;
    private readonly PanelWordViewModel _vm;

    private PanelWordWindow(PanelWordViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
        SourceInitialized += (_, _) => ColocarJuntoAWord();
    }

    /// <summary>Muestra el panel; si se indica un contrato, toma de él el papel propio y la otra parte.</summary>
    public static async void Mostrar(Contrato? contrato = null)
    {
        try
        {
            if (_instancia is not null)
            {
                if (_instancia.WindowState == WindowState.Minimized)
                    _instancia.WindowState = WindowState.Normal;
                _instancia.Activate();
                await _instancia._vm.AplicarContratoAsync(contrato);
                return;
            }

            var vm = new PanelWordViewModel(App.Services.GetRequiredService<ISender>());
            _instancia = new PanelWordWindow(vm);
            _instancia.Closed += (_, _) => _instancia = null;
            _instancia.Show();
            await vm.InicializarAsync(contrato);
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo abrir el panel: " + ex.Message, "Llenar contrato en Word",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Coloca el panel en el borde derecho de la ventana de Word (si está abierto) o de la pantalla,
    /// ocupando toda la altura, para que se vean a la vez el documento y los datos.
    /// </summary>
    private void ColocarJuntoAWord()
    {
        var area = SystemParameters.WorkArea;
        var izquierda = area.Right - Width;
        var arriba = area.Top;
        var alto = area.Height;

        if (WordInterop.VentanaWord() is { } word && PresentationSource.FromVisual(this)?.CompositionTarget is { } destino)
        {
            // Píxeles físicos → unidades WPF (tiene en cuenta el escalado de pantalla).
            Matrix m = destino.TransformFromDevice;
            var esquinaSup = m.Transform(new Point(word.Left, word.Top));
            var esquinaInf = m.Transform(new Point(word.Right, word.Bottom));
            if (esquinaInf.X - esquinaSup.X > Width * 2)
            {
                izquierda = esquinaInf.X - Width - 8;
                arriba = Math.Max(esquinaSup.Y, area.Top);
                alto = Math.Min(esquinaInf.Y, area.Bottom) - arriba;
            }
        }

        Left = izquierda;
        Top = arriba;
        Height = Math.Max(MinHeight, alto);
    }
}
