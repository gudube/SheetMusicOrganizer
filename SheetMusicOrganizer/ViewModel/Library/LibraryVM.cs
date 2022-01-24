using System.Linq;
using System.ComponentModel;
using Serilog;
using SheetMusicOrganizer.Model;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.ViewModel.Tools;
using System.Collections.ObjectModel;
using System.Windows;

namespace SheetMusicOrganizer.ViewModel
{
    public class LibraryVM : BaseViewModel, IDataErrorInfo
    {
        public override string ViewModelName => "LIBRARY";

        public LibraryVM(SessionContext session) : base(session)
        {
            CreateDelegateCommands();
        }

        #region Initialization
        public async Task InitializeData()
        {
            await UpdatePlaylistsFromDb();
            UpdateMasteryLevelsFromDb();
        }

        private void CreateDelegateCommands()
        {
            CreateNewPlaylistCommand = new DelegateCommand(_ => CreateNewPlaylist());
            CancelNewPlaylistCommand = new DelegateCommand(_ => CancelNewPlaylist());
            EditSelectedPlaylistCommand = new DelegateCommand(_ => EditSelectedPlaylist());
            CancelEditPlaylistCommand = new DelegateCommand(_ => CancelEditSelectedPlaylist());
            RenameSelectedPlaylistCommand = new DelegateCommand(_ => RenameSelectedPlaylist());
            DeleteSelectedPlaylistCommand = new DelegateCommand(_ => DeleteSelectedPlaylist());
            PlaySelectedSongCommand = new DelegateCommand(song => SetSelectedSongPlaying(true, song as SongItem));
            RemoveSelectedSongsCommand = new DelegateCommand(_ => RemoveSelectedSongs());
        }

        protected override void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            //if(e.PropertyName == nameof(Session.PlayingSong))
            //    RefreshSongShowedAsPlaying();
        }
        #endregion

        #region Playlists
        private readonly SmartCollection<PlaylistItem> _playlists = new SmartCollection<PlaylistItem>();
        public SmartCollection<PlaylistItem> Playlists { get => _playlists; }

        #region All Playlists

        private async Task UpdatePlaylistsFromDb()
        {
            Playlists.Reset(await DbHandler.GetAllPlaylists());
            SelectedPlaylist = Playlists.ElementAtOrDefault(0);
        }

        #endregion

        #region Selected Playlist

        private PlaylistItem? _selectedPlaylist;

        public PlaylistItem? SelectedPlaylist
        {
            get => _selectedPlaylist;
            set
            {
                PlaylistItem? oldValue = _selectedPlaylist;
                oldValue?.PrepareChange();
                SetField(ref _selectedPlaylist, value);
            }
        }

        public DelegateCommand? DeleteSelectedPlaylistCommand { get; private set; }
        public DelegateCommand? EditSelectedPlaylistCommand { get; private set; }
        public DelegateCommand? CancelEditPlaylistCommand { get; private set; }
        public DelegateCommand? RenameSelectedPlaylistCommand { get; private set; }

        private void DeleteSelectedPlaylist()
        {
            if (SelectedPlaylist  == null)
            {
                Log.Warning("Trying to delete a playlist that is not a playlist item or is null");
                return;
            }

            if (SelectedPlaylist.IsLocked)
            {
                Log.Warning("Trying to delete a locked playlist: {playlistName}", SelectedPlaylist.Name);
                return;
            }

            int newIndex = Playlists.IndexOf(SelectedPlaylist);
            if (newIndex < 0)
                newIndex = 0;
            if(newIndex >= Playlists.Count - 1)
                newIndex--;

            DbHandler.DeletePlaylist(SelectedPlaylist);
            Playlists.Remove(SelectedPlaylist);
            SelectedPlaylist = Playlists.ElementAtOrDefault(newIndex);
        }

        private string _editPlaylistName = "";
        public string EditPlaylistName { get => _editPlaylistName; set => SetField(ref _editPlaylistName, value); }

        private void EditSelectedPlaylist()
        {
            if (SelectedPlaylist  == null)
            {
                Log.Warning("Trying to edit a playlist that is not a playlist item or is null");
                return;
            }

            if (SelectedPlaylist.IsLocked)
            {
                Log.Warning("Trying to edit a locked playlist: {playlistName}", SelectedPlaylist.Name);
                return;
            }

            foreach (PlaylistItem item in Playlists)
                item.IsEditing = false;

            EditPlaylistName = SelectedPlaylist.Name;
            SelectedPlaylist.IsEditing = true;
        }

        private string ValidateEditPlaylistName()
        {
            if (string.IsNullOrWhiteSpace(EditPlaylistName))
                return "Playlist name is empty";

            foreach(var playlist in Playlists)
            {
                if (SelectedPlaylist != playlist && playlist.Name == EditPlaylistName)
                    return "Playlist name already exists";
            }

            return "";
        }

        private void CancelEditSelectedPlaylist()
        {
            if (SelectedPlaylist == null)
            {
                Log.Warning("Trying to edit a playlist that is not a playlist item or is null");
                return;
            }

            SelectedPlaylist.IsEditing = false;
        }

        private void RenameSelectedPlaylist()
        {
            if (SelectedPlaylist == null)
            {
                Log.Warning("Trying to rename a playlist that is not a playlist item or is null");
                return;
            }
            if (SelectedPlaylist.IsLocked)
            {
                Log.Warning("Trying to rename a locked playlist: {playlistName}", SelectedPlaylist.Name);
                return;
            }

            if (string.IsNullOrWhiteSpace(ValidateEditPlaylistName()))
            {
                if (!SelectedPlaylist.Name.Equals(EditPlaylistName))
                {
                    SelectedPlaylist.Name = EditPlaylistName;
                    DbHandler.UpdatePlaylist(SelectedPlaylist, DbHandler.playlistTable.Name, EditPlaylistName);
                }
                SelectedPlaylist.IsEditing = false;
            }
        }
        #endregion
        
        #region Adding playlist
        private string _addingPlaylistName = "";
        public string AddingPlaylistName { get => _addingPlaylistName; set => SetField(ref _addingPlaylistName, value); }

        private string ValidateAddingPlaylistName()
        {
            if (string.IsNullOrWhiteSpace(AddingPlaylistName))
                return "Playlist name is empty";

            foreach (PlaylistItem item in Playlists)
            {
                if(item.Name == AddingPlaylistName)
                    return "Playlist name already exists";
            }
            
            return "";
        }
        public DelegateCommand? CancelNewPlaylistCommand { get; private set; }
        private void CancelNewPlaylist()
        {
            AddingPlaylistName = "";
        }

        public DelegateCommand? CreateNewPlaylistCommand { get; private set; }
        private void CreateNewPlaylist()
        {
            string error = ValidateAddingPlaylistName();
            if (!string.IsNullOrWhiteSpace(error))
            {
                Log.Warning("Trying to create a new playlist with an error: {error}", error);
                return;
            }

            PlaylistItem newPlaylist = new PlaylistItem(AddingPlaylistName);
            DbHandler.CreateNewPlaylist(newPlaylist);
            Playlists.Add(newPlaylist);
            SelectedPlaylist = newPlaylist;
            AddingPlaylistName = "";
        }
        #endregion

        #endregion

        #region Mastery Levels
        private void UpdateMasteryLevelsFromDb()
        {
            Session.MasteryLevels.Reset(DbHandler.GetAllMasteryLevels());
        }

        public bool IsSongInMastery(MasteryItem mastery, SongItem song)
        {
            return song.MasteryId == mastery.Id;
            // return DbHandler.IsSongInMastery(mastery.Id, song.Id);
        }

        public event EventHandler<SongItem[]>? SongMasteryChanged;

        public void SetSongMastery(SongItem song, MasteryItem mastery)
        {
            StatusContext.addLoadingStatus(LoadingStatus.SettingSongMastery);
            StatusContext.addSavingStatus(SavingStatus.SongMastery);
            DbHandler.SetSongMastery(song, mastery, mastery.Id);
            this.SongMasteryChanged?.Invoke(this, new SongItem[]{ song });
            StatusContext.removeSavingStatus(SavingStatus.SongMastery);
            StatusContext.removeLoadingStatus(LoadingStatus.SettingSongMastery);
        }
        public void SetSongsMastery(IEnumerable<SongItem> songs, MasteryItem mastery)
        {
            StatusContext.addLoadingStatus(LoadingStatus.SettingSongMastery);
            StatusContext.addSavingStatus(SavingStatus.SongMastery);
            SongItem[] songItems = songs as SongItem[] ?? songs.ToArray();
            DbHandler.SetSongsMastery(songItems, mastery, mastery.Id);
            this.SongMasteryChanged?.Invoke(this, songItems);
            StatusContext.removeSavingStatus(SavingStatus.SongMastery);
            StatusContext.removeLoadingStatus(LoadingStatus.SettingSongMastery);
        }
        #endregion

        #region Songs
        public event EventHandler? ScrollToSong;
        public SongItem? TempScrollSong = null;

        public void GoToSong(SongItem song, bool exactSameSong)
        {
            // there's a mastery selected, but not the song's one, add it as selected
            if(Session.MasteryLevels.Any(mastery => mastery.IsSelected) && !song.Mastery.IsSelected)
                song.Mastery.IsSelected = true;

            SongItem? songToSelect = null;
            if (exactSameSong)
            {
                var playlist = Playlists.FirstOrDefault(pl => pl.Songs.Contains(song));
                if(playlist != null)
                {
                    SelectedPlaylist = playlist;
                    songToSelect = song;
                }
            } else {
                songToSelect = SelectedPlaylist?.Songs.FirstOrDefault(x => x.Id == song.Id);
                if (songToSelect == null) // song is not visible in this playlist
                {
                    SelectedPlaylist = Playlists.ElementAtOrDefault(0);
                    songToSelect = SelectedPlaylist?.Songs.First(x => x.Id == song.Id);
                }
            }

            if (songToSelect == null)
            {
                GlobalEvents.raiseErrorEvent(new InvalidOperationException($"Could not find the song id '{song.Id}' when trying to go to the song. Song name '{song.Title}'"));
            } else { 
                SelectedPlaylist?.SelectedSongs.Clear();
                SelectedPlaylist?.SelectedSongs.Add(songToSelect);
                TempScrollSong = songToSelect;
                ScrollToSong?.Invoke(this, EventArgs.Empty);
            }
        }
        
        public DelegateCommand? RemoveSelectedSongsCommand { get; private set; }
        private void RemoveSelectedSongs()
        {
            if (SelectedPlaylist == null)
            {
                Log.Warning("Trying to remove selected songs when there is no selected PlaylistItem");
                return;
            }

            int[]? selectedSongIDs = SelectedPlaylist.SelectedSongs.Select(x => ((SongItem)x).Id).ToArray();
            if (selectedSongIDs == null || selectedSongIDs.Length == 0)
            {
                Log.Warning("No songs selected when trying to RemoveSelectedSongs()");
                return;
            }

            if (SelectedPlaylist == Playlists.ElementAtOrDefault(0))
            {
                if (Session.PlayingSong != null && selectedSongIDs.Contains(Session.PlayingSong.Id))
                    StopPlayingSong();
                DbHandler.DeleteSongs(selectedSongIDs);
                var songsToRemove = SelectedPlaylist.SelectedSongs.OfType<SongItem>().ToArray();
                foreach (var playlist in Playlists)
                    playlist.RemoveSongs(songsToRemove);
            } else {
                if (Session.PlayingSong != null && SelectedPlaylist.IsPlaying && SelectedPlaylist.SelectedSongs.Contains(Session.PlayingSong))
                    StopPlayingSong();
                DbHandler.RemoveSongsFromPlaylist(SelectedPlaylist.Id, selectedSongIDs);
                SelectedPlaylist.RemoveSongs(SelectedPlaylist.SelectedSongs.OfType<SongItem>());
            }
        }
        #endregion

        #region PlayingSong
        public DelegateCommand? PlaySelectedSongCommand { get; private set; }

        //private void RefreshSongShowedAsPlaying()
        //{
        //    foreach (var playlist in Playlists)
        //        foreach (SongItem song in playlist.Songs)
        //            song.ShowedAsPlaying = false;

        //    if (Session.PlayingSong != null && SelectedPlaylist.IsPlaying == true)
        //    {
        //        SongItem? songStartedPlaying = SelectedPlaylist.Songs.FirstOrDefault(x => x.Id == Session.PlayingSong.Id);
        //        if (songStartedPlaying != null)
        //            songStartedPlaying.ShowedAsPlaying = true;
        //    }
        //}

        public bool SetSelectedSongPlaying(bool startPlaying, SongItem? specificSong = null)
        {
            SongItem? song = specificSong;
            if (song == null)
            {
                if (Settings.Default.PartitionSelectionMode == 0)
                    song = SelectedPlaylist?.SelectedSongs.FirstOrDefault() as SongItem ?? Session.PlayingSong ?? SelectedPlaylist?.Songs.FirstOrDefault();
                else
                    song = Session.PlayingSong ?? SelectedPlaylist?.SelectedSongs.FirstOrDefault() as SongItem ?? SelectedPlaylist?.Songs.FirstOrDefault();
            }

            if (song == null)
            {
                Log.Warning("Tried to start playing a song without any songs visible");
                return false;
            }

            bool anyMasterySelected = false;
            foreach (MasteryItem mastery in Session.MasteryLevels)
            {
                if (mastery.IsSelected)
                {
                    mastery.IsPlaying = true;
                    anyMasterySelected = true;
                }
                else
                    mastery.IsPlaying = false;
            }

            if(!anyMasterySelected)
                foreach (MasteryItem mastery in Session.MasteryLevels)
                    mastery.IsPlaying = true;

            SetPlayingSong(song, startPlaying);

            return true;
        }

        //Sets the playing song with the same playing playlist and mastery level
        public void SetPlayingSong(SongItem song, bool startPlaying)
        {
            if (Session.PlayingSong != song)
            {
                foreach (var playlist in Playlists)
                    playlist.IsPlaying = playlist.Songs.Contains(song);


                Session.PlayingSong = song;
                if(String.IsNullOrWhiteSpace(song.AudioDirectory1))
                    Session.Player.Stop();
                else
                    Session.Player.SetSong(song.AudioDirectory1, startPlaying, false);
            }
        }

        public void StopPlayingSong()
        {
            Session.Player.Stop();
            Session.PlayingSong = null;
            
            foreach (PlaylistItem item in Playlists)
                item.IsPlaying = false;
            
            foreach (MasteryItem mastery in Session.MasteryLevels)
                mastery.IsPlaying = false;
        }

        public enum SongToFind
        {
            Next,
            Previous,
            Same,
            Random
        }

        public void SetNextPlayingSong(SongToFind type)
        {
            if (Session.PlayingSong == null)
            {
                SetSelectedSongPlaying(true);
                return;
            }

            if (type == SongToFind.Same)
            {
                Session.Player.Position = 0;
                Session.Player.Play();
                return;
            }
            
            SongItem? newSong = null;

            PlaylistItem? playingPlaylist = Playlists.FirstOrDefault(x => x is PlaylistItem pl && pl.IsPlaying) ?? SelectedPlaylist;
            if (playingPlaylist == null)
                return;

            int[] masteryIds = Session.MasteryLevels.Where(x => x.IsPlaying).Select(x => x.Id).ToArray();

            try
            {
                switch (type)
                {
                    case SongToFind.Next:
                        newSong = playingPlaylist.Songs.SkipWhile(x => x.Id != Session.PlayingSong.Id).Skip(1).FirstOrDefault(x => masteryIds.Contains(x.MasteryId));
                        break;
                    case SongToFind.Previous:
                        newSong = playingPlaylist.Songs.TakeWhile(x => x.Id != Session.PlayingSong.Id).LastOrDefault(x => masteryIds.Contains(x.MasteryId));
                        break;
                    case SongToFind.Random:
                        Random rand = new Random();
                        var songs = playingPlaylist.Songs.Where(x => masteryIds.Contains(x.MasteryId));
                        int toSkip = rand.Next(0, songs.Count());
                        newSong = songs.Skip(toSkip).First();
                        break;
                }
            }catch(Exception)
            {
                newSong = null;
            }
            
            if (newSong == null)
                StopPlayingSong();
            else
                SetPlayingSong(newSong, true);
        }


        #endregion

        #region Validation

        public string? Error => null;

        public string? this[string columnName] {
            get
            {
                string? error = null;
                switch (columnName)
                {
                    case (nameof(AddingPlaylistName)):
                        error = ValidateAddingPlaylistName();
                        break;
                    case (nameof(EditPlaylistName)):
                        error = ValidateEditPlaylistName();
                        break;
                }

                return error;
            }
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
