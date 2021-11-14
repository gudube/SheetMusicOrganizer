using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using SheetMusicOrganizer.Model;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.ViewModel.Tools;
using System;

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
            Playlists.CollectionChanged += Playlists_CollectionChanged;
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
            if(e.PropertyName == nameof(Session.PlayingSong))
                RefreshSongShowedAsPlaying();
        }
        #endregion

        #region Playlists
        private SmartCollection<BasePlaylistItem> _playlists = new SmartCollection<BasePlaylistItem>();
        public SmartCollection<BasePlaylistItem> Playlists { get => _playlists; set => SetField(ref _playlists, value); }

        #region All Playlists

        private async Task UpdatePlaylistsFromDb()
        {
            List<BasePlaylistItem> playlists = new List<BasePlaylistItem>(await DbHandler.GetAllPlaylists()){ AddPlaylist };
            Playlists.Reset(playlists);
            SelectedPlaylistIndex = 0;
        }

        private void Playlists_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (BasePlaylistItem item in Playlists) 
                item.PropertyChanged += Playlist_PropertyChanged;
        }

        private void Playlist_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlaylistItem.IsPlaying))
                RefreshSongShowedAsPlaying();
            else if (e.PropertyName == nameof(PlaylistItem.Songs) && Playlists[SelectedPlaylistIndex] == sender)
                ShownSongs.Reset((Playlists[SelectedPlaylistIndex] as PlaylistItem)?.Songs);
        }

        #endregion

        #region Selected Playlist

        private int _selectedPlaylistIndex = 0;

        public int SelectedPlaylistIndex
        {
            get => _selectedPlaylistIndex;
            set
            {
                if (Playlists.ElementAtOrDefault(_selectedPlaylistIndex) is PlaylistItem pl)
                    pl.IsEditing = false; //sets IsEditing false to unselected playlist
                SetField(ref _selectedPlaylistIndex, value);
                ShownSongs.Reset((Playlists[value] as PlaylistItem)?.Songs);
            }
        }

        public DelegateCommand? DeleteSelectedPlaylistCommand { get; private set; }
        public DelegateCommand? EditSelectedPlaylistCommand { get; private set; }
        public DelegateCommand? CancelEditPlaylistCommand { get; private set; }
        public DelegateCommand? RenameSelectedPlaylistCommand { get; private set; }

        private void DeleteSelectedPlaylist()
        {
            if (SelectedPlaylistIndex < 0 || SelectedPlaylistIndex >= Playlists.Count - 1 
                                          || !(Playlists.ElementAtOrDefault(SelectedPlaylistIndex) is PlaylistItem playlist))
            {
                Log.Warning("Trying to delete a playlist that is not a playlist item or is null");
                return;
            }

            if (playlist.IsLocked)
            {
                Log.Warning("Trying to delete a locked playlist: {playlistName}", playlist.Name);
                return;
            }

            int newIndex = SelectedPlaylistIndex;
            if (newIndex < 0 || newIndex >= Playlists.Count - 2) {
                newIndex = SelectedPlaylistIndex - 1;
            }
            
            Playlists.RemoveAt(SelectedPlaylistIndex);
            SelectedPlaylistIndex = newIndex;
            DbHandler.DeletePlaylist(playlist);
        }

        private string _editPlaylistName = "";
        public string EditPlaylistName { get => _editPlaylistName; set => SetField(ref _editPlaylistName, value); }

        private void EditSelectedPlaylist()
        {
            if (!(Playlists.ElementAtOrDefault(SelectedPlaylistIndex) is PlaylistItem selectedPlaylist))
            {
                Log.Warning("Trying to edit a playlist that is not a playlist item or is null");
                return;
            }

            if (selectedPlaylist.IsLocked)
            {
                Log.Warning("Trying to edit a locked playlist: {playlistName}", selectedPlaylist.Name);
                return;
            }

            foreach (BasePlaylistItem item in Playlists)
            {
                if (item is PlaylistItem playlist)
                    playlist.IsEditing = false;
            }

            EditPlaylistName = selectedPlaylist.Name;
            selectedPlaylist.IsEditing = true;
        }

        private string ValidateEditPlaylistName()
        {
            if (string.IsNullOrWhiteSpace(EditPlaylistName))
                return "Playlist name is empty";

            for (int i = 0; i < Playlists.Count; i++)
            {
                BasePlaylistItem item = Playlists[i];
                if (SelectedPlaylistIndex != i && item is PlaylistItem playlist && playlist.Name == EditPlaylistName)
                    return "Playlist name already exists";
            }

            return "";
        }

        private void CancelEditSelectedPlaylist()
        {
            if (!(Playlists.ElementAtOrDefault(SelectedPlaylistIndex) is PlaylistItem selectedPlaylist))
            {
                Log.Warning("Trying to edit a playlist that is not a playlist item or is null");
                return;
            }

            selectedPlaylist.IsEditing = false;
        }

        private void RenameSelectedPlaylist()
        {
            if (!(Playlists.ElementAtOrDefault(SelectedPlaylistIndex) is PlaylistItem selectedPlaylist))
            {
                Log.Warning("Trying to rename a playlist that is not a playlist item or is null");
                return;
            }
            if (selectedPlaylist.IsLocked)
            {
                Log.Warning("Trying to rename a locked playlist: {playlistName}", selectedPlaylist.Name);
                return;
            }

            if (string.IsNullOrWhiteSpace(ValidateEditPlaylistName()))
            {
                if (!selectedPlaylist.Name.Equals(EditPlaylistName))
                {
                    selectedPlaylist.Name = EditPlaylistName;
                    DbHandler.UpdatePlaylist(selectedPlaylist, DbHandler.playlistTable.Name, EditPlaylistName);
                }
                selectedPlaylist.IsEditing = false;
            }
        }
        #endregion
        
        #region Adding playlist
        public AddPlaylistItem AddPlaylist { get; } = new AddPlaylistItem();
        
        private string _addingPlaylistName = "";
        public string AddingPlaylistName { get => _addingPlaylistName; set => SetField(ref _addingPlaylistName, value); }

        private string ValidateAddingPlaylistName()
        {
            if (string.IsNullOrWhiteSpace(AddingPlaylistName))
                return "Playlist name is empty";

            foreach (BasePlaylistItem item in Playlists)
            {
                if(item is PlaylistItem playlist && playlist.Name == AddingPlaylistName)
                    return "Playlist name already exists";
            }
            
            return "";
        }
        public DelegateCommand? CancelNewPlaylistCommand { get; private set; }
        private void CancelNewPlaylist()
        {
            SelectedPlaylistIndex = Playlists.Count - 2;
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
            int beforeLast = Playlists.Count - 1;
            Playlists.Insert(beforeLast, newPlaylist);
            SelectedPlaylistIndex = beforeLast;
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

            //if (Session.SelectedMasteryLevels.Count > 0 && !Session.SelectedMasteryLevels.Contains(mastery) && Session.Songs.Contains(song))
            //    Session.Songs.Remove(song);
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
            /*if (!Session.SelectedMasteryLevels.Contains(mastery))
            {
                foreach (SongItem song in songItems)
                    if (Session.Songs.Contains(song))
                        Session.Songs.Remove(song);
            }*/
        }
        #endregion

        #region Songs

        private SmartCollection<SongItem> _shownSongs = new SmartCollection<SongItem>();
        public SmartCollection<SongItem> ShownSongs { get => _shownSongs; private set => SetField(ref _shownSongs, value); }

        //All the songs in the selected playlist, no matter the mastery levels selected
        //public readonly SmartCollection<SongItem> ShownSongs = new SmartCollection<SongItem>();

        public void GoToSong(SongItem song)
        {
            if(!song.Mastery.IsSelected)
                song.Mastery.IsSelected = true;
            SongItem? songToSelect = ShownSongs.First(x => x.Equals(song));
            if(songToSelect == null)
            {
                SelectedPlaylistIndex = 0;
                songToSelect = ShownSongs.First(x => x.Equals(song));
                if (songToSelect == null)
                {
                    GlobalEvents.raiseErrorEvent(new InvalidOperationException($"Could not find the song id '{song.Id}' when trying to go to the song. Song name '{song.Title}'"));
                    return;
                }
            }
            foreach (SongItem existingSong in ShownSongs)
            {
                existingSong.IsSelected = false;
            }

            songToSelect.IsSelected = true;
        }

        public bool AddSong(SongItem song)
        {
            if (DbHandler.IsSongExisting(song.PartitionDirectory))
                return false;
            
            MasteryItem[] selectedMasteryItems = Session.MasteryLevels.Where(x => x.IsSelected).ToArray();
            song.MasteryId = selectedMasteryItems.Length > 0 ? selectedMasteryItems[0].Id : Session.MasteryLevels[0].Id;

            DbHandler.AddSong(song);

            if (!(Playlists.ElementAtOrDefault(SelectedPlaylistIndex) is PlaylistItem selectedPlaylist))
            {
                Log.Warning("Trying to add a new song in the database when there are no selected PlaylistItem");
            }
            else
            {
                selectedPlaylist.AddSongs(song);
            }

            return true;
        }

        public void AddDirByFolder(string dir, bool recursive, bool useAudioMD)
        {
            if (recursive)
                foreach (string subDir in Directory.GetDirectories(dir))
                    AddDirByFolder(subDir, true, useAudioMD);

            List<string> pdfFiles = new List<string>();
            List<string> audioFiles = new List<string>();
            foreach (string fileDir in Directory.GetFiles(dir))
            {
                string ext = Path.GetExtension(fileDir);
                if (ext == ".pdf")
                    pdfFiles.Add(fileDir);
                else if (ext == ".mp3" || ext == ".flac" || ext == ".wav" || ext == ".m4a")
                    audioFiles.Add(fileDir);
            }

            string audio1 = "";
            string audio2 = "";
            if (audioFiles.Count >= 1)
            {
                audio1 = audioFiles[0];
                if (audioFiles.Count >= 2)
                {
                    if (Path.GetFileNameWithoutExtension(audio1).Length >
                        Path.GetFileNameWithoutExtension(audioFiles[1]).Length)
                    {
                        audio2 = audio1;
                        audio1 = audioFiles[1];
                    }
                    else
                    {
                        audio2 = audioFiles[1];
                    }
                }
            }

            foreach (string pdfFile in pdfFiles)
            {
                AddSong(new SongItem(pdfFile, audio1, audio2, 0, useAudioMD));
            }
        }

        public void AddDirByFilename(string dir, bool recursive, bool useAudioMD)
        {
            List<string> pdfFiles = new List<string>();
            List<string> audioFiles = new List<string>();
            GetAllFilenames(dir, recursive, pdfFiles, audioFiles);
            foreach (string pdfFile in pdfFiles)
            {
                string pdfName = Path.GetFileNameWithoutExtension(pdfFile);
                List<string> audios = audioFiles.FindAll(x => Path.GetFileNameWithoutExtension(x).StartsWith(pdfName));
                string audio1 = "";
                string audio2 = "";
                if (audios.Count >= 1)
                {
                    audio1 = audios[0];
                    audioFiles.Remove(audio1);
                    if (audios.Count >= 2)
                    {
                        audio2 = audios[1];
                        audioFiles.Remove(audio2);
                    }
                }
                if(Path.GetFileNameWithoutExtension(audio1).Length <= Path.GetFileNameWithoutExtension(audio2).Length)
                    AddSong(new SongItem(pdfFile, audio1, audio2, 0, useAudioMD));
                else
                    AddSong(new SongItem(pdfFile, audio2, audio1, 0, useAudioMD));
            }

        }

        public void GetAllFilenames(string dir, bool recursive, List<string> pdfFiles, List<string> audioFiles)
        {
            if (recursive)
                foreach (string subDir in Directory.GetDirectories(dir))
                    GetAllFilenames(subDir, true, pdfFiles, audioFiles);

            string[] allFiles = Directory.GetFiles(dir);
            pdfFiles.AddRange(allFiles.Where(x => Path.GetExtension(x) == ".pdf"));
            audioFiles.AddRange(allFiles.Where(x => {
                string ext = Path.GetExtension(x);
                return ext == ".mp3" || ext == ".flac" || ext == ".wav" || ext == ".m4a";
            }));
        }

        public void AddDirWithoutAudio(string dir, bool recursive)
        {
            if (recursive)
                foreach (string subDir in Directory.GetDirectories(dir))
                    AddDirWithoutAudio(subDir, true);

            foreach (string fileDir in Directory.GetFiles(dir))
            {
                string ext = Path.GetExtension(fileDir);
                if (ext == ".pdf")
                    AddSong(new SongItem(fileDir, "", "", 0, false));
            }
        }
        
        /*public void AddFolder(string dir, bool recursive, bool useAudioMD)
        {
            if (recursive)
            {
                foreach (string subDir in Directory.GetDirectories(dir))
                {
                    AddFolder(subDir, true, useAudioMD);
                }
            }

            List<string> partitionFiles = new List<string>();
            List<string> audioFiles = new List<string>();
            foreach (string fileDir in Directory.GetFiles(dir))
            {
                string ext = Path.GetExtension(fileDir);
                if (ext == ".pdf")
                    partitionFiles.Add(fileDir);
                else if (ext == ".mp3" || ext == ".flac" || ext == ".wav" || ext == ".m4a")
                    audioFiles.Add(fileDir);
            }

            if(partitionFiles.Count == 1 && audioFiles.Count == 1)
            {
                AddSong(new SongItem(partitionFiles[0] ?? "", audioFiles[0] ?? "", 0, useAudioMD));
            }
            else
            {
                foreach(string partition in partitionFiles)
                {
                    //bool audioFound = false;
                    foreach(string audio in audioFiles)
                    {
                        if (Path.GetFileNameWithoutExtension(partition) == Path.GetFileNameWithoutExtension(audio))
                        {
                            AddSong(new SongItem(partition, audio, 0, useAudioMD));
                            //audioFound = true;
                            break;
                        }
                    }
                    //if(!audioFound)
                    //    AddSong(new SongItem(partition));
                }
            }
        }*/


        public DelegateCommand? RemoveSelectedSongsCommand { get; private set; }
        private void RemoveSelectedSongs()
        {
            SongItem[] selectedSongs = ShownSongs.Where(x => x.IsSelected).ToArray();

            if (selectedSongs.Length == 0)
            {
                Log.Warning("No songs selected when trying to RemoveSelectedSongs()");
                return;
            }

            int[] selectedSongIDs = selectedSongs.Select(x => x.Id).ToArray();

            if(Session.PlayingSong != null && selectedSongIDs.Contains(Session.PlayingSong.Id))
                StopPlayingSong();

            if (!(Playlists.ElementAtOrDefault(SelectedPlaylistIndex) is PlaylistItem selectedPlaylist))
            {
                Log.Warning("Trying to remove selected songs when there is no selected PlaylistItem");
                return;
            }
            if (SelectedPlaylistIndex == 0)
                DbHandler.DeleteSongs(selectedSongIDs);
            else
                DbHandler.RemoveSongsFromPlaylist(selectedPlaylist.Id, selectedSongIDs);
            
            (Playlists[SelectedPlaylistIndex] as PlaylistItem)?.RemoveSongs(selectedSongs);
        }
        #endregion
        
        #region PlayingSong
        public DelegateCommand? PlaySelectedSongCommand { get; private set; }

        private void RefreshSongShowedAsPlaying()
        {
            foreach (SongItem song in ShownSongs)
                song.ShowedAsPlaying = false;

            if (Session.PlayingSong != null && Playlists[SelectedPlaylistIndex] is PlaylistItem playlist && playlist.IsPlaying)
            {
                SongItem? songStartedPlaying = ShownSongs.FirstOrDefault(x => x.Id == Session.PlayingSong.Id);
                if (songStartedPlaying != null)
                    songStartedPlaying.ShowedAsPlaying = true;
            }
        }

        public bool SetSelectedSongPlaying(bool startPlaying, SongItem? specificSong = null)
        {
            if(!(Playlists[SelectedPlaylistIndex] is PlaylistItem playlist))
                return false;

            SongItem? song = specificSong ?? playlist.Songs.FirstOrDefault(x => x.IsSelected) ?? playlist.Songs.FirstOrDefault();
            if (song == null)
            {
                Log.Warning("Tried to start playing a song without any songs visible");
                return false;
            }

            for (int i = 0; i < Playlists.Count; i++)
            {
                BasePlaylistItem item = Playlists[i];
                if (item is PlaylistItem pl)
                    pl.IsPlaying = i == SelectedPlaylistIndex;
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
                Session.PlayingSong = song;
                Session.Player.SetSong(song.AudioDirectory1, startPlaying, false);
            }
        }

        public void StopPlayingSong()
        {
            Session.Player.Stop();
            Session.PlayingSong = null;
            
            foreach (BasePlaylistItem item in Playlists)
                if (item is PlaylistItem playlist)
                    playlist.IsPlaying = false;
            
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

            PlaylistItem? playingPlaylist = (Playlists.FirstOrDefault(x => x is PlaylistItem pl && pl.IsPlaying) ?? Playlists[SelectedPlaylistIndex]) as PlaylistItem;
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
