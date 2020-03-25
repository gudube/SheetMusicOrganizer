using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel;
using MusicPlayerForDrummers.ViewModel.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace MusicPlayerForDrummers.ViewModel
{
    public class LibraryVM : BaseViewModel
    {
        public override string ViewModelName => "LIBRARY";

        private ObservableCollection<CustomListBoxItem> _playlists = new ObservableCollection<CustomListBoxItem>();
        public ObservableCollection<CustomListBoxItem> Playlists
        {
            get => _playlists;
            set => SetField(ref _playlists, value);
        }

        private AddPlaylistItem _addPlaylist = new AddPlaylistItem();

        public DelegateCommand SelectPlaylistCommand { get; private set; }
        public DelegateCommand CreateNewPlaylistCommand { get; private set; }

        public LibraryVM()
        {
            UpdatePlaylistsFromDB();

            SelectPlaylistCommand = new DelegateCommand(SelectPlaylist);
            SelectPlaylistCommand = new DelegateCommand(CreateNewPlaylist);
        }

        private void UpdatePlaylistsFromDB()
        {
            Playlists.Clear();
            DBHandler.GetAllPlaylists().ForEach(Playlists.Add);
            Playlists.Add(_addPlaylist);
            OnPropertyChanged("Playlists");
        }

        private void SelectPlaylist(object playlist)
        {
            if (playlist is AddPlaylistItem)
            {
                //ADD PLAYLIST
                if(_addPlaylist.IsAddingPlaylist == false)
                {
                    _addPlaylist.IsAddingPlaylist = true;
                }

            }
            else if (playlist is PlaylistItem)
            {
                //READ PLAYLIST
                _addPlaylist.IsAddingPlaylist = false;
            }
            else
            {
                Trace.WriteLine("Invalid selected item in playlist ListBox");
            }
        }

        private void CreateNewPlaylist(object param)
        {
            string playlistName = (string)param;
        }
    }
}
