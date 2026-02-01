using System.Windows;

namespace InventorySystem.Windows.Shared.Dialogs
{
    /// <summary>
    /// Ventana de diálogo que muestra una vista (p. ej. VentasView) como contenido.
    /// Se puede usar para mostrar la misma vista que se incrusta en el área de trabajo.
    /// </summary>
    public partial class ViewDialogWindow : Window
    {
        public ViewDialogWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Contenido del diálogo (vista a mostrar). Asignar antes de Show/ShowDialog.
        /// </summary>
        public object? DialogContent
        {
            get => DialogContentHost.Content;
            set => DialogContentHost.Content = value;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
