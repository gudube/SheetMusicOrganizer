using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel;
using MusicPlayerForDrummers.ViewModel.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Linq;

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
            PlaySelectedSongCommand = new DelegateCommand(x => PlaySelectedSong());
            RemoveSelectedSongsCommand = new DelegateCommand(x => RemoveSelectedSongs());
        }

        #region Playlists
        private ObservableCollection<BaseModelItem> _playlists = new ObservableCollection<BaseModelItem>();
        public ObservableCollection<BaseModelItem> Playlists
        {
            get => _playlists;
            set => SetField(ref _playlists, value);
        }

        private AddPlaylistItem _addPlaylist = new AddPlaylistItem();
        private PlaylistItem _allMusicPlaylist = new PlaylistItem("All music", true);

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
            List<BaseModelItem> playlists = new List<BaseModelItem>{ _allMusicPlaylist };
            playlists.AddRange(DBHandler.GetAllPlaylists());
            playlists.Add(_addPlaylist);
            Playlists = new ObservableCollection<BaseModelItem>(playlists);
            _selectedPlaylist = Playlists[0];
        }

        private void SelectedPlaylistChanged()
        {
            if(SelectedPlaylist is PlaylistItem)
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
            DBHandler.CreateNewPlaylist(newPlaylist);
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

        public void CopySongToPlaylist(PlaylistItem playlist, SongItem song)
        {
            DBHandler.AddPlaylistSongLink(playlist.ID, song.ID);
        }

        public void CopySongsToPlaylist(PlaylistItem playlist, IEnumerable<SongItem> songs)
        {
            DBHandler.AddPlaylistSongLinks(playlist.ID, songs.Select(x => x.ID));
        }

        public bool IsSongInPlaylist(PlaylistItem playlist, SongItem song)
        {
            return DBHandler.IsSongInPlaylist(playlist.ID, song.ID);
        }
        #endregion

        //TODO: Add icon to represent mastery (poker face, crooked smile, smile, fire?)
        //TODO: Multiple mastery levels are selectable using CTRL only, button to activate/deactivate mastery filter besides the expander
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

        private ObservableCollection<SongItem> _selectedSongs;
        public ObservableCollection<SongItem> SelectedSongs
        {
            get => _selectedSongs;
            set => SetField(ref _selectedSongs, value);
        }

        private SongItem _playingSong;
        public SongItem PlayingSong
        {
            get => _playingSong;
            set => SetField(ref _playingSong, value);
        }

        private void UpdateSongsFromDB()
        {
            int[] masteryIDs = new int[SelectedMasteryLevels.Count];
            for(int i=0; i < SelectedMasteryLevels.Count; i++)
            {
                masteryIDs[i] = SelectedMasteryLevels[i].ID;
            }
            //code smell?
            if(SelectedPlaylist == _allMusicPlaylist)
            {
                Songs = new ObservableCollection<SongItem>(DBHandler.GetAllSongs(masteryIDs));
            }
            else
            {
                Songs = new ObservableCollection<SongItem>(DBHandler.GetSongs(SelectedPlaylist.ID, masteryIDs));
            }
        }

        public DelegateCommand AddSongFileCommand { get; private set; }
        public void AddSongFile(object songDirObj)
        {
            if (!(songDirObj is string songDir))
            {
                Trace.WriteLine("Expected AddSongFile to receive a string. Received : " + songDirObj.GetType().Name);
                return;
            }
            if (Songs.Any(x => x.Directory == songDir))
                return;

            int masteryID;
            if(SelectedMasteryLevels.Count == 0)
                masteryID = MasteryLevels[0].ID;
            else
                masteryID = SelectedMasteryLevels[0].ID;

            SongItem newSong;
            if (SelectedPlaylist is PlaylistItem && SelectedPlaylist != _allMusicPlaylist)
            {
                newSong = DBHandler.AddSong(songDir, masteryID, SelectedPlaylist.ID);
            }
            else
            {
                newSong = DBHandler.AddSong(songDir, masteryID);

            }
            Songs.Add(newSong);
        }

        public DelegateCommand PlaySelectedSongCommand { get; private set; }
        public void PlaySelectedSong()
        {
        }

        public DelegateCommand RemoveSelectedSongsCommand { get; private set; }
        private void RemoveSelectedSongs()
        {
            if (!(SelectedPlaylist is PlaylistItem)) {
                Trace.WriteLine("Expected selected playlist to be a PlaylistItem when RemoveSelectedSongs(), but is : " + SelectedPlaylist.GetType().Name);
                return;
            }

            if (SelectedSongs.Count == 0)
            {
                Trace.WriteLine("Expected songs to be selected when RemoveSelectedSongs()");
                return;
            }

            int[] songIDs = SelectedSongs.Select(x => x.ID).ToArray();
            if (SelectedPlaylist == _allMusicPlaylist)
            {
                DBHandler.DeleteSongs(songIDs);
            }
            else
            {
                DBHandler.RemoveSongsFromPlaylist(SelectedPlaylist.ID, songIDs);
            }
            foreach (SongItem song in SelectedSongs)
                Songs.Remove(song); //TODO: does it call the NotifyProperty on each song removed? way of doing it once at the end instead?
        }
        #endregion

        public enum PlaybackOrder
        {
            Default,
            Random,
            Repeat
        }
    }
}
