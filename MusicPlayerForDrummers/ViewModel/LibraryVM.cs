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

        private CustomListBoxItem _selectedPlaylist;
        public CustomListBoxItem SelectedPlaylist
        {
            get => _selectedPlaylist;
            set
            {
                SetField(ref _selectedPlaylist, value);
                SelectedPlaylistChanged();
            }
        }

        private bool _isEditingSelectedPlaylist;
        public bool IsEditingSelectedPlaylist
        {
            get => _isEditingSelectedPlaylist;
            set => SetField(ref _isEditingSelectedPlaylist, value);
        }

        private string _newPlaylistName;
        public string NewPlaylistName
        {
            get => _newPlaylistName;
            set => SetField(ref _newPlaylistName, value);
        }

        public DelegateCommand CreateNewPlaylistCommand { get; private set; }
        public DelegateCommand DeletePlaylistCommand { get; private set; }
        public DelegateCommand RenamePlaylistCommand { get; private set; }

        public LibraryVM()
        {
            UpdatePlaylistsFromDB();

            CreateNewPlaylistCommand = new DelegateCommand(x => CreateNewPlaylist());
            DeletePlaylistCommand = new DelegateCommand(x => DeletePlaylist());
            RenamePlaylistCommand = new DelegateCommand(x => RenamePlaylist());
        }

        private void UpdatePlaylistsFromDB()
        {
            Playlists.Clear();
            DBHandler.GetAllPlaylists().ForEach(Playlists.Add);
            Playlists.Add(_addPlaylist);
            OnPropertyChanged("Playlists");
            SelectedPlaylist = Playlists[0];
        }

        private void SelectedPlaylistChanged()
        {
            IsEditingSelectedPlaylist = false;
        }

        private void CreateNewPlaylist()
        {
            PlaylistItem newPlaylist = new PlaylistItem(NewPlaylistName);
            //DBHandler.AddPlaylist(newPlaylist);
            Playlists.Insert(Playlists.Count - 1, newPlaylist);
            OnPropertyChanged("Playlists");
            SelectedPlaylist = newPlaylist;
            NewPlaylistName = "";
        }

        private void DeletePlaylist()
        {
            //DBHandler.DeletePlaylist(SelectedPlaylist);
            Playlists.Remove(SelectedPlaylist);
            SelectedPlaylist = null;
        }

        private void RenamePlaylist()
        {
            IsEditingSelectedPlaylist = true;
        }
    }
}
