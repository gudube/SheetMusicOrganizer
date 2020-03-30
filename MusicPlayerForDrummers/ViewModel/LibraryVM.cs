using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel;
using MusicPlayerForDrummers.ViewModel.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace MusicPlayerForDrummers.ViewModel
{
    public class LibraryVM : BaseViewModel
    {
        public override string ViewModelName => "LIBRARY";

        private ObservableCollection<BaseModelItem> _playlists = new ObservableCollection<BaseModelItem>();
        public ObservableCollection<BaseModelItem> Playlists
        {
            get => _playlists;
            set => SetField(ref _playlists, value);
        }

        private AddPlaylistItem _addPlaylist = new AddPlaylistItem();

        private BaseModelItem _selectedPlaylist;
        public BaseModelItem SelectedPlaylist
        {
            get => _selectedPlaylist;
            set
            {
                if(SetField(ref _selectedPlaylist, value))
                {
                    SelectedPlaylistChanged();
                }
            }
        }

        public DelegateCommand CreateNewPlaylistCommand { get; private set; }
        public DelegateCommand DeleteSelectedPlaylistCommand { get; private set; }
        public DelegateCommand RenameSelectedPlaylistCommand { get; private set; }
        public DelegateCommand IsRenamingPlaylistCommand { get; private set; }

        public LibraryVM()
        {
            UpdatePlaylistsFromDB();

            CreateNewPlaylistCommand = new DelegateCommand(x => CreateNewPlaylist(x));
            DeleteSelectedPlaylistCommand = new DelegateCommand(x => DeleteSelectedPlaylist());
            RenameSelectedPlaylistCommand = new DelegateCommand(x => RenameSelectedPlaylist(x));
        }

        private void UpdatePlaylistsFromDB()
        {
            Playlists.Clear();
            DBHandler.GetAllPlaylists().ForEach(Playlists.Add);
            Playlists.Add(_addPlaylist);
            SelectedPlaylist = Playlists[0];
        }

        private void SelectedPlaylistChanged()
        {
        }

        private void CreateNewPlaylist(object playlistName)
        {
            if(!(playlistName is string plName))
            {
                Trace.WriteLine("Expected CreateNewPlaylist to receive a string. Received : " + playlistName.GetType().Name);
                return;
            }
            PlaylistItem newPlaylist = new PlaylistItem(plName);
            //DBHandler.AddPlaylist(newPlaylist);
            Playlists.Insert(Playlists.Count - 1, newPlaylist);
            SelectedPlaylist = newPlaylist;
        }

        private void DeleteSelectedPlaylist()
        {
            if (!(SelectedPlaylist is PlaylistItem plItem))
            {
                Trace.WriteLine("Expected to have a PlaylistItem selected when DeleteSelectedPlaylist is called.");
                return;
            }
            //DBHandler.DeletePlaylist(plItem);
            Playlists.Remove(plItem);
            SelectedPlaylist = null; //TODO: Go to the next one, or last one if no next
        }

        private void RenameSelectedPlaylist(object playlistName)
        {
            if (!(playlistName is string plName))
            {
                Trace.WriteLine("Expected RenameSelectedPlaylist to receive a string. Received : " + playlistName.GetType().Name);
                return;
            }
            if (!(SelectedPlaylist is PlaylistItem plItem))
            {
                Trace.WriteLine("Expected to have a PlaylistItem selected when RenameSelectedPlaylist is called.");
                return;
            }
            //DBHandler.RenamePlaylist((PlaylistItem) SelectedPlaylist, NewPlaylistName);
            plItem.Name = plName;
        }
    }
}
