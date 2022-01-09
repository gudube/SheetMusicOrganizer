using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Serilog;
using SheetMusicOrganizer.ViewModel;

namespace SheetMusicOrganizer.View.Controls.Items
{
    public partial class PlaylistAdderItem : UserControl
    {
        public PlaylistAdderItem()
        {
            InitializeComponent();
        }

        private void AdderTextBox_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HideInput();
        }

        private void AdderTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            HideInput();
        }

        private void AdderTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!(DataContext is LibraryVM libraryVM))
            {
                Log.Warning("OnSelectedPlaylistChanged when the DataContext of PlaylistAdderItem is not LibraryVM");
                return;
            }

            if (e.Key == Key.Escape)
                HideInput();
            else if (e.Key == Key.Enter)
                libraryVM.CreateNewPlaylistCommand?.Execute(null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (AdderTextBox.Visibility == Visibility.Hidden)
            {
                ShowInput();
            }
        }

        private void ShowInput()
        {
            AdderTextBox.Visibility = Visibility.Visible;
            MainButton.Visibility = Visibility.Hidden;
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                Keyboard.Focus(AdderTextBox);
            }, DispatcherPriority.ApplicationIdle);
        }

        private void HideInput()
        {
            if (AdderTextBox.Visibility == Visibility.Visible)
            {
                MainButton.Visibility = Visibility.Visible;
                AdderTextBox.Visibility = Visibility.Hidden;
                if (!(DataContext is LibraryVM libraryVM))
                {
                    Log.Warning("OnSelectedPlaylistChanged when the DataContext of PlaylistAdderItem is not LibraryVM");
                    return;
                }

                libraryVM.CancelNewPlaylistCommand?.Execute(null);
            }
        }
    }
}
