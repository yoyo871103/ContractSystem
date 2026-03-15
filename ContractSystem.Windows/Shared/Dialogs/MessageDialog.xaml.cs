using System.Windows;
using System.Windows.Input;

namespace ContractSystem.Windows.Shared.Dialogs
{
    /// <summary>
    /// Diálogo de mensaje personalizado reutilizable (marco con solo contenido + botones de control).
    /// Usar <see cref="Show"/> para mostrarlo desde cualquier vista.
    /// </summary>
    public partial class MessageDialog : Window
    {
        public MessageDialogResult Result { get; private set; } = MessageDialogResult.None;

        public MessageDialog()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || e.ButtonState != MouseButtonState.Pressed)
                return;
            try { DragMove(); } catch { }
        }

        private void BtnCloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageDialogResult.Cancel;
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Muestra el diálogo de mensaje de forma modal.
        /// </summary>
        /// <param name="owner">Ventana propietaria (null si no hay).</param>
        /// <param name="title">Título de la ventana.</param>
        /// <param name="message">Texto del mensaje.</param>
        /// <param name="buttons">Botones a mostrar (Ok, OkCancel, YesNo, YesNoCancel).</param>
        /// <param name="icon">Icono opcional: None, Information, Warning, Error.</param>
        /// <param name="showCloseButton">Si es true, muestra el botón Cerrar en la barra de título; si es false, lo oculta.</param>
        /// <returns>Resultado según el botón pulsado.</returns>
        public static MessageDialogResult Show(
            Window? owner,
            string title,
            string message,
            MessageDialogButtons buttons = MessageDialogButtons.Ok,
            MessageDialogIcon icon = MessageDialogIcon.None,
            bool showCloseButton = true)
        {
            var dialog = new MessageDialog
            {
                Owner = owner,
                Title = title
            };
            dialog.MessageText.Text = message;
            dialog.ConfigureButtons(buttons);
            dialog.ConfigureIcon(icon);
            dialog.BtnCloseWindow.Visibility = showCloseButton ? Visibility.Visible : Visibility.Collapsed;
            dialog.ShowDialog();
            return dialog.Result;
        }

        private void ConfigureButtons(MessageDialogButtons buttons)
        {
            switch (buttons)
            {
                case MessageDialogButtons.Ok:
                    BtnOk.Visibility = Visibility.Visible;
                    BtnOk.IsCancel = false;
                    BtnOk.IsDefault = true;
                    break;
                case MessageDialogButtons.OkCancel:
                    BtnOk.Visibility = Visibility.Visible;
                    BtnCancel.Visibility = Visibility.Visible;
                    BtnOk.IsDefault = true;
                    BtnCancel.IsCancel = true;
                    break;
                case MessageDialogButtons.YesNo:
                    BtnYes.Visibility = Visibility.Visible;
                    BtnNo.Visibility = Visibility.Visible;
                    BtnYes.IsDefault = true;
                    BtnNo.IsCancel = true;
                    break;
                case MessageDialogButtons.YesNoCancel:
                    BtnYes.Visibility = Visibility.Visible;
                    BtnNo.Visibility = Visibility.Visible;
                    BtnCancel.Visibility = Visibility.Visible;
                    BtnYes.IsDefault = true;
                    BtnCancel.IsCancel = true;
                    break;
            }
        }

        private void ConfigureIcon(MessageDialogIcon icon)
        {
            string glyph = icon switch
            {
                MessageDialogIcon.Information => "\uE946",   // Info
                MessageDialogIcon.Warning => "\uE7BA",      // Warning
                MessageDialogIcon.Error => "\uE783",        // Error
                _ => ""
            };
            if (string.IsNullOrEmpty(glyph))
            {
                IconText.Visibility = Visibility.Collapsed;
                return;
            }
            IconText.Text = glyph;
            IconText.Visibility = Visibility.Visible;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageDialogResult.Ok;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageDialogResult.Cancel;
            DialogResult = false;
            Close();
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageDialogResult.Yes;
            DialogResult = true;
            Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageDialogResult.No;
            DialogResult = false;
            Close();
        }
    }

    /// <summary>
    /// Icono opcional del diálogo de mensaje.
    /// </summary>
    public enum MessageDialogIcon
    {
        None,
        Information,
        Warning,
        Error
    }
}
