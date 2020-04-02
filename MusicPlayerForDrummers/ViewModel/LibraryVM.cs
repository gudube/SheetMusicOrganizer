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

        public LibraryVM()
        {
            UpdatePlaylistsFromDB();
            UpdateMasteryLevelsFromDB();
            UpdateSongsFromDB();
            CreateNewPlaylistCommand = new DelegateCommand(x => CreateNewPlaylist(x));
            DeleteSelectedPlaylistCommand = new DelegateCommand(x => DeleteSelectedPlaylist());
            RenameSelectedPlaylistCommand = new DelegateCommand(x => RenameSelectedPlaylist(x));
            AddSongFileCommand = new DelegateCommand(x => AddSongFile(x));
        }

        #region Playlists
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
                if (SetField(ref _selectedPlaylist, value))
                {
                    SelectedPlaylistChanged();
                }
            }
        }

        public DelegateCommand CreateNewPlaylistCommand { get; private set; }
        public DelegateCommand DeleteSelectedPlaylistCommand { get; private set; }
        public DelegateCommand RenameSelectedPlaylistCommand { get; private set; }
        public DelegateCommand IsRenamingPlaylistCommand { get; private set; }

        private void UpdatePlaylistsFromDB()
        {
            Playlists = new ObservableCollection<BaseModelItem>(DBHandler.GetAllPlaylists());
            Playlists.Add(_addPlaylist);
            _selectedPlaylist = Playlists[0];
        }

        private void SelectedPlaylistChanged()
        {
            UpdateSongsFromDB();
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
        #endregion

        #region Mastery Levels
        private ObservableCollection<MasteryItem> _masteryLevels = new ObservableCollection<MasteryItem>();
        public ObservableCollection<MasteryItem> MasteryLevels
        {
            get => _masteryLevels;
            set => SetField(ref _masteryLevels, value);
        }

        private ObservableCollection<MasteryItem> _selectedMasteryLevels = new ObservableCollection<MasteryItem>();
        public ObservableCollection<MasteryItem> SelectedMasteryLevels
        {
            get => _selectedMasteryLevels;
            set
            {
                if(SetField(ref _selectedMasteryLevels, value))
                {
                    SelectedMasteryLevelsChanged();
                }
                
            }
        }

        private void SelectedMasteryLevelsChanged()
        {
            UpdateSongsFromDB();
        }

        private void UpdateMasteryLevelsFromDB()
        {
            MasteryLevels = new ObservableCollection<MasteryItem>(DBHandler.GetAllMasteryLevels());
        }
        #endregion

        #region Songs
        private ObservableCollection<SongItem> _songs = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Songs
        {
            get => _songs;
            set => SetField(ref _songs, value);
        }

        private void UpdateSongsFromDB()
        {
            int[] masteryIDs = new int[SelectedMasteryLevels.Count];
            for(int i=0; i < SelectedMasteryLevels.Count; i++)
            {
                masteryIDs[i] = SelectedMasteryLevels[i].ID;
            }
            DBHandler.GetSongs(SelectedPlaylist.ID, masteryIDs);
        }

        public DelegateCommand AddSongFileCommand { get; private set; }
        public void AddSongFile(object songDirObj)
        {
            if (!(songDirObj is string songDir))
            {
                Trace.WriteLine("Expected AddSongFile to receive a string. Received : " + songDirObj.GetType().Name);
                return;
            }

            SongItem newSong = new SongItem(songDir, MasteryLevels[0].ID);
            DBHandler.AddSong(newSong);
            Songs.Add(newSong);
        }
        #endregion
    }
}
