using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SheetMusicOrganizer.View.Controls.Items
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

        private void HideInput()
        {
            if (Main.Tag is LibraryVM libraryVM)
                libraryVM.CancelEditPlaylistCommand?.Execute(null);
        }

        private void MainTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HideInput();
        }

        private void MainTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            HideInput();
        }

        private void MainTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (Main.Tag is LibraryVM libraryVM)
            {
                if (e.Key == Key.Escape)
                    HideInput();
                else if (e.Key == Key.Enter)
                    libraryVM.RenameSelectedPlaylistCommand?.Execute(null);
            }
        }
    }
}
