using MusicPlayerForDrummers.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicPlayerForDrummers.Controls
{
    /// <summary>
    /// Interaction logic for LibraryMenu.xaml
    /// </summary>
    public partial class LibraryMenu : Grid
    {
        private List<PlaylistItem> _playlists;
        private string _tempAddName = "+++";

        public LibraryMenu()
        {
            InitializeComponent();
            UpdatePlaylistsFromDB();
        }

        private void UpdatePlaylistsFromDB()
        {
            PlaylistsListBox.Items.Clear();
            _playlists = DBHandler.GetAllPlaylists();
            PlaylistsListBox.ItemsSource = _playlists;
        }

        private void AddPlaylist()
        {

        }
    }
}
