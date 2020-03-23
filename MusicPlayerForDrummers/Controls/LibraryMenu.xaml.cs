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

        private List<CustomListBoxItem> _playlists = new List<CustomListBoxItem>();

        public LibraryMenu()
        {
            InitializeComponent();
            UpdatePlaylistsFromDB();
            PlaylistsExpander.IsExpanded = true;
            PlaylistsListBox.SelectedIndex = 0;
        }

        private AddPlaylistItem add = new AddPlaylistItem();
        private void UpdatePlaylistsFromDB()
        {
            PlaylistsListBox.Items.Clear();
            _playlists.Clear();
            _playlists.AddRange(DBHandler.GetAllPlaylists());
            _playlists.Add(add);
            PlaylistsListBox.ItemsSource = _playlists;
            PlaylistsListBox.Items.Refresh();
        }

        private void Playlists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(PlaylistsListBox.SelectedIndex == (PlaylistsListBox.Items.Count - 1))
            {
                //ADD PLAYLIST
                add.IsAddingPlaylist = true;
                
            }
            else if(PlaylistsListBox.SelectedIndex >= 0 && PlaylistsListBox.SelectedIndex < (PlaylistsListBox.Items.Count - 1))
            {
                //READ PLAYLIST
                add.IsAddingPlaylist = false;
            }
            else
            {
                throw new Exception("Invalid selected index in playlist ListBox");
            }
        }
    }
}
