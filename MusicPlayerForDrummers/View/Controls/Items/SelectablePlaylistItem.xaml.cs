using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicPlayerForDrummers.View.Controls.Items
{
    /// <summary>
    /// Interaction logic for SelectableNameItem.xaml
    /// </summary>
    public partial class SelectablePlaylistItem : UserControl
    {
        public SelectablePlaylistItem()
        {
            InitializeComponent();
        }

        private void MainTextBox_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue)
            {
                Keyboard.Focus(MainTextBox);
                MainTextBox.SelectAll();
            }
        }
    }
}
