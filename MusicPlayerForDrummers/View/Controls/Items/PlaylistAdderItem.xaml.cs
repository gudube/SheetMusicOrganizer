using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MusicPlayerForDrummers.ViewModel;
using Serilog;

namespace MusicPlayerForDrummers.View.Controls.Items
{
    /// <summary>
    /// Interaction logic for Adder.xaml
    /// </summary>
    public partial class PlaylistAdderItem : UserControl
    {
        public PlaylistAdderItem()
        {
            InitializeComponent();
            DataContextChanged += PlaylistAdderItem_DataContextChanged;
        }

        private void PlaylistAdderItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.OldValue is LibraryVM oldLibraryVM)
                oldLibraryVM.PropertyChanged -= LibraryVM_PropertyChanged;
            if(e.NewValue is LibraryVM libraryVM)
                libraryVM.PropertyChanged += LibraryVM_PropertyChanged;
        }

        private void LibraryVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LibraryVM.SelectedPlaylistIndex))
                OnSelectedPlaylistChanged();
        }

        private void OnSelectedPlaylistChanged()
        {
            if (!(DataContext is LibraryVM libraryVM))
            {
                Log.Warning("OnSelectedPlaylistChanged when the DataContext of PlaylistAdderItem is not LibraryVM");
                return;
            }

            if (libraryVM.SelectedPlaylistIndex == libraryVM.Playlists.Count - 1 && AdderTextBox.Visibility == Visibility.Hidden)
            {
                AdderTextBox.Visibility = Visibility.Visible;
                Keyboard.Focus(AdderTextBox);
            }
            else
            {
                AdderTextBox.Visibility = Visibility.Hidden;
            }
        }
    }
}
