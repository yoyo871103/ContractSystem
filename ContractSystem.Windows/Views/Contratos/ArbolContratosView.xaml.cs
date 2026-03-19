using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Application.Contratos;
using ContractSystem.Application.Contratos.Commands.CambiarEstadoContrato;
using ContractSystem.Application.Contratos.Commands.CreateSuplemento;
using ContractSystem.Application.Contratos.Commands.DeleteContrato;
using ContractSystem.Application.Contratos.Commands.UpdateContrato;
using ContractSystem.Application.Contratos.Queries.GetAllContratos;
using ContractSystem.Application.Contratos.Queries.GetContratosMarco;
using ContractSystem.Application.Nomencladores.Queries.GetAllTerceros;
using ContractSystem.Domain.Contratos;
using ContractSystem.Windows.ViewModels;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ContractSystem.Windows.Views.Contratos;

public partial class ArbolContratosView : UserControl
{
    public ArbolContratosView()
    {
        InitializeComponent();
    }

    private void CmbFiltroTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ArbolContratosViewModel vm && CmbFiltroTipo.SelectedItem is ComboBoxItem item)
        {
            var tag = item.Tag?.ToString();
            vm.FiltroTipo = string.IsNullOrEmpty(tag) ? null : (TipoDocumentoContrato)int.Parse(tag);
        }
    }

    private void CmbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ArbolContratosViewModel vm && CmbFiltroEstado.SelectedItem is ComboBoxItem item)
        {
            var tag = item.Tag?.ToString();
            vm.FiltroEstado = string.IsNullOrEmpty(tag) ? null : (EstadoContrato)int.Parse(tag);
        }
    }

    private void CmbFiltroRol_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ArbolContratosViewModel vm && CmbFiltroRol.SelectedItem is ComboBoxItem item)
        {
            var tag = item.Tag?.ToString();
            vm.FiltroRol = string.IsNullOrEmpty(tag) ? null : (RolContrato)int.Parse(tag);
        }
    }


    private async void TvArbol_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (TvArbol.SelectedItem is not NodoArbolViewModel nodo) return;

        // Prevent double-fire from child elements
        if (e.OriginalSource is not FrameworkElement fe || fe.DataContext != nodo) return;

        try
        {
            var mediator = App.Services.GetRequiredService<ISender>();

            var todos = await mediator.Send(new GetAllContratosQuery());
            var contrato = todos.FirstOrDefault(c => c.Id == nodo.Id);
            if (contrato is null) return;

            var window = new DetalleContratoWindow(mediator, contrato);
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al abrir detalle: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private NodoArbolViewModel? GetNodoFromContextMenu(object sender)
    {
        if (sender is MenuItem mi && mi.Parent is ContextMenu ctx && ctx.PlacementTarget is FrameworkElement fe)
            return fe.DataContext as NodoArbolViewModel;
        return null;
    }

    private async Task<Contrato?> CargarContratoAsync(int id)
    {
        var mediator = App.Services.GetRequiredService<ISender>();
        var todos = await mediator.Send(new GetAllContratosQuery());
        return todos.FirstOrDefault(c => c.Id == id);
    }

    private async void CtxEditar_Click(object sender, RoutedEventArgs e)
    {
        var nodo = GetNodoFromContextMenu(sender);
        if (nodo is null) return;

        try
        {
            var mediator = App.Services.GetRequiredService<ISender>();
            var contrato = await CargarContratoAsync(nodo.Id);
            if (contrato is null) return;

            var numeracionService = App.Services.GetRequiredService<IDocumentoNumeracionService>();
            var contratosMarco = await mediator.Send(new GetContratosMarcoQuery());
            var terceros = await mediator.Send(new GetAllTercerosQuery());

            var dialog = new ContratoDialogWindow();
            dialog.Owner = System.Windows.Application.Current.MainWindow;
            dialog.CargarDatosAuxiliares(contratosMarco, terceros);
            dialog.CargarContrato(contrato);

            if (dialog.ShowDialog() == true)
            {
                await mediator.Send(new UpdateContratoCommand(
                    contrato.Id,
                    dialog.NumeroDocumento,
                    dialog.ObjetoContrato,
                    dialog.RolContrato,
                    dialog.FechaFirma,
                    dialog.FechaEntradaVigor,
                    dialog.FechaVigencia,
                    dialog.Duracion,
                    dialog.MiEmpresaId,
                    dialog.TerceroId,
                    dialog.ContratoPadreId,
                    dialog.ValorTotal,
                    dialog.CondicionesEntrega,
                    dialog.CostosAsociados));

                if (DataContext is ArbolContratosViewModel vm)
                    await vm.CargarCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al editar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void CtxCambiarEstado_Click(object sender, RoutedEventArgs e)
    {
        var nodo = GetNodoFromContextMenu(sender);
        if (nodo is null) return;

        try
        {
            var mediator = App.Services.GetRequiredService<ISender>();
            var contrato = await CargarContratoAsync(nodo.Id);
            if (contrato is null) return;

            var dialog = new CambiarEstadoWindow(contrato.Estado);
            dialog.Owner = System.Windows.Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                await mediator.Send(new CambiarEstadoContratoCommand(contrato.Id, dialog.NuevoEstado));
                MessageBox.Show("Estado actualizado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                if (DataContext is ArbolContratosViewModel vm)
                    await vm.CargarCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cambiar estado: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void CtxHistorial_Click(object sender, RoutedEventArgs e)
    {
        var nodo = GetNodoFromContextMenu(sender);
        if (nodo is null) return;

        try
        {
            var mediator = App.Services.GetRequiredService<ISender>();
            var window = new HistorialCambiosWindow(mediator, nodo.Id, nodo.Numero);
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al abrir historial: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CtxAdjuntos_Click(object sender, RoutedEventArgs e)
    {
        var nodo = GetNodoFromContextMenu(sender);
        if (nodo is null) return;

        var mediator = App.Services.GetRequiredService<ISender>();
        var window = new DocumentosAdjuntosWindow(mediator, nodo.Id, nodo.Numero);
        window.Owner = System.Windows.Application.Current.MainWindow;
        window.ShowDialog();
    }

    private void CtxFacturas_Click(object sender, RoutedEventArgs e)
    {
        var nodo = GetNodoFromContextMenu(sender);
        if (nodo is null) return;

        if (nodo.Tipo == TipoDocumentoContrato.Marco)
        {
            MessageBox.Show("No se pueden asociar facturas a un Contrato Marco.",
                "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var mediator = App.Services.GetRequiredService<ISender>();
        var window = new FacturasWindow(mediator, nodo.Id, nodo.Numero);
        window.Owner = System.Windows.Application.Current.MainWindow;
        window.ShowDialog();
    }

    private async void CtxVerMapa_Click(object sender, RoutedEventArgs e)
    {
        var nodo = GetNodoFromContextMenu(sender);
        if (nodo is null) return;

        try
        {
            var mediator = App.Services.GetRequiredService<ISender>();
            var contrato = await CargarContratoAsync(nodo.Id);
            if (contrato is null) return;

            var modStore = App.Services.GetRequiredService<IModificacionDocumentoStore>();
            var window = new MapaModificacionesWindow(mediator, modStore, contrato);
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al abrir mapa: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void CtxNuevoSuplemento_Click(object sender, RoutedEventArgs e)
    {
        var nodo = GetNodoFromContextMenu(sender);
        if (nodo is null) return;

        try
        {
            var mediator = App.Services.GetRequiredService<ISender>();
            var contrato = await CargarContratoAsync(nodo.Id);
            if (contrato is null) return;

            // Solo se pueden suplementar Marco, Específico o Independiente
            if (contrato.TipoDocumento == TipoDocumentoContrato.Suplemento)
            {
                MessageBox.Show("No se puede crear un suplemento de otro suplemento.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var numeracionService = App.Services.GetRequiredService<IDocumentoNumeracionService>();
            var todosContratos = await mediator.Send(new GetAllContratosQuery());
            var terceros = await mediator.Send(new GetAllTercerosQuery());

            // Filtrar documentos modificables al mismo padre
            var padreId = contrato.ContratoPadreId ?? contrato.Id;
            var documentosModificables = todosContratos
                .Where(c => c.Id == padreId || c.ContratoPadreId == padreId)
                .ToList() as IReadOnlyList<Contrato>;

            var dialog = new SuplementoDialogWindow();
            dialog.Owner = System.Windows.Application.Current.MainWindow;
            dialog.SetNumeracionService(numeracionService);
            dialog.ConfigurarParaContrato(contrato, documentosModificables, terceros);

            if (dialog.ShowDialog() == true)
            {
                var modificaciones = dialog.Modificaciones
                    .Select(m => new ModificacionDto(m.DocumentoDestinoId, m.Descripcion))
                    .ToList();

                await mediator.Send(new CreateSuplementoCommand(
                    dialog.ContratoPadreId,
                    dialog.RolContrato,
                    dialog.NumeroDocumento,
                    dialog.ObjetoContrato,
                    dialog.EsModificacionGenerales,
                    dialog.FechaFirma,
                    dialog.FechaEntradaVigor,
                    dialog.FechaVigencia,
                    dialog.Duracion,
                    dialog.MiEmpresaId,
                    dialog.TerceroId,
                    dialog.ValorTotal,
                    dialog.CondicionesEntrega,
                    dialog.CostosAsociados,
                    modificaciones));

                MessageBox.Show("Suplemento creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                if (DataContext is ArbolContratosViewModel vm)
                    await vm.CargarCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al crear suplemento: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void CtxEliminar_Click(object sender, RoutedEventArgs e)
    {
        var nodo = GetNodoFromContextMenu(sender);
        if (nodo is null) return;

        var result = MessageBox.Show(
            $"¿Eliminar el contrato '{nodo.Numero}'?",
            "Confirmar eliminación",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            var mediator = App.Services.GetRequiredService<ISender>();
            await mediator.Send(new DeleteContratoCommand(nodo.Id));
            MessageBox.Show("Contrato eliminado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            if (DataContext is ArbolContratosViewModel vm)
                await vm.CargarCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al eliminar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
