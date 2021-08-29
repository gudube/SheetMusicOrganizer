using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SheetMusicOrganizer.View.Controls.Partition
{
    /// <summary>
    /// Interaction logic for SyncSettingsScrolling.xaml
    /// </summary>
    public partial class SyncSettingsScrolling : UserControl
    {
        public SyncSettingsScrolling()
        {
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && sender is TextBox textbox)
            {
                // FocusManager.SetFocusedElement(FocusManager.GetFocusScope(sender as DependencyObject), null);
                // Keyboard.ClearFocus();
                BindingExpression? be = textbox.GetBindingExpression(TextBox.TextProperty);
                be?.UpdateSource();
            }
        }
    }
}
