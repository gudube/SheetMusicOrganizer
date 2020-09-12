using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel.Tools;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using MusicPlayerForDrummers.Model.Items;
using Serilog;

namespace MusicPlayerForDrummers.ViewModel
{
    public class LibraryVM : BaseViewModel
    {
        public override string ViewModelName => "LIBRARY";

        //TODO: Separer le LibraryVM est plusieurs VM?
        public LibraryVM(SessionContext session) : base(session)
        {
            CreateDelegateCommands();
            Session.PropertyChanged += Session_PropertyChanged1;
        }

        private void Session_PropertyChanged1(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                //case nameof(Session.SelectedPlaylist):
                case nameof(Session.PlayingPlaylist):
                case nameof(Session.PlayingSong):
                    RefreshSongShowedAsPlaying();
                    break;
            }
        }

        public async Task InitializeData()
        {
            UpdatePlaylistsFromDb();
            UpdateMasteryLevelsFromDb();
            Task songs = UpdateSongsFromDb();
            await songs.ConfigureAwait(false);
            Playlists.CollectionChanged += Playlists_CollectionChanged;
        }

        private void CreateDelegateCommands()
        {
            CreateNewPlaylistCommand = new DelegateCommand(x => CreateNewPlaylist(x == null ? "" : (string)x));
            DeleteSelectedPlaylistCommand = new DelegateCommand(_ => DeleteSelectedPlaylist());
            RenameSelectedPlaylistCommand = new DelegateCommand(x => RenameSelectedPlaylist(x == null ? "" : (string)x));
            PlaySelectedSongCommand = new DelegateCommand(_ => SetSelectedSongPlaying(true));
            RemoveSelectedSongsCommand = new DelegateCommand(_ => RemoveSelectedSongs());
        }

        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.SelectedPlaylist))
                SelectedPlaylist_PropertyChanged();
        }

        #region Playlists
        private SmartCollection<BaseModelItem> _playlists = new SmartCollection<BaseModelItem>();
        public SmartCollection<BaseModelItem> Playlists { get => _playlists; set => SetField(ref _playlists, value); }
        private readonly AddPlaylistItem _addPlaylist = new AddPlaylistItem();
        
        public DelegateCommand? CreateNewPlaylistCommand { get; private set; }
        public DelegateCommand? DeleteSelectedPlaylistCommand { get; private set; }
        public DelegateCommand? RenameSelectedPlaylistCommand { get; private set; }

        private void UpdatePlaylistsFromDb()
        {
            List<BaseModelItem> playlists = new List<BaseModelItem>(DbHandler.GetAllPlaylists()){ _addPlaylist };
            Playlists.Reset(playlists);
            Session.SelectedPlaylist = playlists[0];
        }
        private void Playlists_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Session.SelectedPlaylist != null && !Playlists.Contains(Session.SelectedPlaylist))
                Session.SelectedPlaylist = null;
        }

        private async void SelectedPlaylist_PropertyChanged()
        {
            Session.Status.SelectingPlaylist = true;
            await UpdateSongsFromDb();
            Session.Status.SelectingPlaylist = false;
        }

        private void CreateNewPlaylist(string playlistName)
        {
            PlaylistItem newPlaylist = new PlaylistItem(playlistName);
            DbHandler.CreateNewPlaylist(newPlaylist);
            Playlists.Insert(Playlists.Count - 1, newPlaylist);
            Session.SelectedPlaylist = newPlaylist;
        }

        private void DeleteSelectedPlaylist()
        {
            if (!(Session.SelectedPlaylist is PlaylistItem plItem))
            {
                Log.Warning("Expected to have a PlaylistItem selected when DeleteSelectedPlaylist is called but is {selected}", Session.SelectedPlaylist);
                return;
            }
            int index = Playlists.IndexOf(plItem) + 1;
            if (index > 0 && index < Playlists.Count)
                Session.SelectedPlaylist = Playlists[index];
            Playlists.Remove(plItem);
            DbHandler.DeletePlaylist(plItem);
        }

        private void RenameSelectedPlaylist(string playlistName)
        {
            if (!(Session.SelectedPlaylist is PlaylistItem plItem))
            {
                Log.Warning("Expected to have a PlaylistItem selected when RenameSelectedPlaylist is called but is {selected}", Session.SelectedPlaylist);
                return;
            }
            plItem.Name = playlistName;
            DbHandler.UpdatePlaylist(plItem);
        }

        public void CopySongToPlaylist(PlaylistItem playlist, SongItem song)
        {
            Session.Status.SavingSongPlaylist = true;
            DbHandler.AddPlaylistSongLink(playlist.Id, song.Id);
            Session.Status.SavingSongPlaylist = false;
        }

        public void CopySongsToPlaylist(PlaylistItem playlist, IEnumerable<SongItem> songs)
        {
            Session.Status.SavingSongPlaylist = true;
            DbHandler.AddSongsToPlaylist(playlist.Id, songs.Select(x => x.Id));
            Session.Status.SavingSongPlaylist = false;
        }

        public bool IsSongInPlaylist(PlaylistItem playlist, SongItem song)
        {
            return DbHandler.IsSongInPlaylist(playlist.Id, song.Id);
        }
        #endregion

        //TODO: Add icon to represent mastery (poker face, crooked smile, smile, fire?)
        //TODO: Multiple mastery levels are selectable using CTRL only, button to activate/deactivate mastery filter besides the expander
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

        public void SetSongMastery(SongItem song, MasteryItem mastery)
        {
            Session.Status.SettingSongMastery = true;
            Session.Status.SavingSongMastery = true;
            DbHandler.SetSongMastery(song, mastery);
            Session.Status.SavingSongMastery = false;
            //if (Session.SelectedMasteryLevels.Count > 0 && !Session.SelectedMasteryLevels.Contains(mastery) && Session.Songs.Contains(song))
            //    Session.Songs.Remove(song);
        }
        public void SetSongsMastery(IEnumerable<SongItem> songs, MasteryItem mastery)
        {
            Session.Status.SettingSongMastery = true;
            Session.Status.SavingSongMastery = true;
            IEnumerable<SongItem> songItems = songs as SongItem[] ?? songs.ToArray();
            DbHandler.SetSongsMastery(songItems, mastery);
            Session.Status.SavingSongMastery = false;
            /*if (!Session.SelectedMasteryLevels.Contains(mastery))
            {
                foreach (SongItem song in songItems)
                    if (Session.Songs.Contains(song))
                        Session.Songs.Remove(song);
            }*/
        }
        #endregion

        #region Songs
        //All the songs in the selected playlist, no matter the mastery levels selected
        public readonly SmartCollection<SongItem> ShownSongs = new SmartCollection<SongItem>();

        public void GoToSong(SongItem song)
        {
            if(Session.SelectedMasteryLevels.Count > 0 && Session.SelectedMasteryLevels.All(x => x.Id != song.MasteryId))
                Session.SelectedMasteryLevels.Add(Session.MasteryLevels.First(x => x.Id == song.MasteryId));
            SongItem songToSelect = ShownSongs.FirstOrDefault(x => x.Id == song.Id);
            if(songToSelect == null)
            {
                Session.SelectedPlaylist = Playlists[0]; //todo: will that work? or manually change playlist and call update of shownsongs
                songToSelect = ShownSongs.FirstOrDefault(x => x.Id == song.Id);
                if (songToSelect == null)
                {
                    Log.Error("Could not find the song to select when trying to go to song with id {id}", song.Id);
                    return;
                }
            }
            foreach (SongItem existingSong in ShownSongs)
            {
                existingSong.IsSelected = false;
            }

            songToSelect.IsSelected = true;
        }

        private async Task UpdateSongsFromDb()
        {
            if (Session.SelectedPlaylist == null || Session.SelectedPlaylist == _addPlaylist)
                ShownSongs.Clear();
            else
            {
                //todo: Make the dbhandler methods async and return task? try if that would work
                List<SongItem> songs = await DbHandler.GetSongs(Session.SelectedPlaylist.Id);
                if (Session.PlayingSong != null && Session.SelectedPlaylist != null) //sets if any of the new songs is playing
                {
                    SongItem? playingSong = songs.FirstOrDefault(x => x.Id == Session.PlayingSong.Id);
                    if(playingSong != null)
                        playingSong.ShowedAsPlaying = (Session.SelectedPlaylist.Equals(Session.PlayingPlaylist));
                }
                ShownSongs.Reset(songs);
            }
        }

        public void SortSongs(string propertyName, bool ascending)
        {
            if (Session.SelectedPlaylist == null || Session.SelectedPlaylist == _addPlaylist)
            {
                Log.Warning("Trying to sort songs when the selected playlist is {playlist}", Session.SelectedPlaylist);
                return;
            }

            Session.Status.SavingSongOrder = true;
            Session.Status.SortingSongs = true;
            List<SongItem> sortedSongs =
                DbHandler.SortSongs(Session.SelectedPlaylist.Id, propertyName, ascending);
            Session.Status.SavingSongOrder = false;
            ShownSongs.Reset(sortedSongs);
        }

        //Resets the songs in the database for current playlist from the songIDs in the same order
        public void ResetSongsInCurrentPlaylist(IEnumerable<int> songIDs)
        {
            if (Session.SelectedPlaylist == null)
            {
                Log.Warning("Tried to reset songs in the current playlist when no playlists are selected");
                return;
            }

            if (Session.SelectedPlaylist == _addPlaylist)
            {
                Log.Warning("Tried to reset songs in the current playlist when the selected playlist is the 'Add' playlist");
                return;
            }
            DbHandler.ResetSongsInPlaylist(Session.SelectedPlaylist.Id, songIDs);
        }

        public bool AddSong(SongItem song)
        {
            bool songCreated = false;

            if (DbHandler.IsSongExisting(song.PartitionDirectory))
            {
                song = DbHandler.GetSong(song.PartitionDirectory);
            }
            else
            {
                if (Session.SelectedMasteryLevels.Count == 0)
                    song.MasteryId = Session.MasteryLevels[0].Id;
                else
                    song.MasteryId = Session.SelectedMasteryLevels[0].Id;

                DbHandler.AddSong(song);
                songCreated = true;
            }

            if (Session.SelectedPlaylist is PlaylistItem && Session.SelectedPlaylist != _addPlaylist)
            {
                if (Session.SelectedPlaylist.Id != 0 && (songCreated || ShownSongs.All(x => x.Id != song.Id)))
                {
                    ShownSongs.Add(song);
                    DbHandler.AddPlaylistSongLink(Session.SelectedPlaylist.Id, song.Id);
                }
            }
            else
            {
                Log.Error("Tried adding a new song when selected playlist was {playlist}", Session.SelectedPlaylist);
            }

            return songCreated;
        }

        //TODO: Add advanced options (like import music sheet only, audio only or both)
        //TODO: Add update existing songs (music sheet or audio) vs skip existing songs
        //TODO: Append songs added to a list to be able to push them to the database in transaction
        public void AddFolder(string dir, bool recursive, bool useAudioMD)
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
                foreach(string partition in partitionFiles) //TODO: More performance way to do it?
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
                    //    AddSong(new SongItem(partition)); //todo: Make this work eventually
                }
            }
        }


        public DelegateCommand? RemoveSelectedSongsCommand { get; private set; }
        private void RemoveSelectedSongs()
        {
            if (!(Session.SelectedPlaylist is PlaylistItem)) {
                Log.Warning("Expected selected playlist to be a PlaylistItem when RemoveSelectedSongs(), but is {SelectedPlaylist}", Session.SelectedPlaylist);
                return;
            }

            SongItem[] selectedSongs = ShownSongs.Where(x => x.IsSelected).ToArray();

            if (selectedSongs.Length == 0)
            {
                Log.Warning("No songs selected when trying to RemoveSelectedSongs()");
                return;
            }

            int[] selectedSongIDs = selectedSongs.Select(x => x.Id).ToArray();

            if(Session.PlayingSong != null && selectedSongIDs.Contains(Session.PlayingSong.Id))
                Session.StopPlayingSong();

            if (Session.SelectedPlaylist == Playlists[0])
            {
                DbHandler.DeleteSongs(selectedSongIDs);
            }
            else
            {
                DbHandler.RemoveSongsFromPlaylist(Session.SelectedPlaylist.Id, selectedSongIDs);
            }
            foreach(SongItem song in selectedSongs)
                ShownSongs.Remove(song);
        }
        #endregion
        
        #region PlayingSong
        public DelegateCommand? PlaySelectedSongCommand { get; private set; }

        private void RefreshSongShowedAsPlaying()
        {
            foreach(SongItem song in ShownSongs)
                if (song.ShowedAsPlaying)
                    song.ShowedAsPlaying = false;

            if (Session.PlayingSong != null && Session.SelectedPlaylist != null)
            {
                SongItem? songStartedPlaying = ShownSongs.FirstOrDefault(x => x.Id == Session.PlayingSong.Id);
                if(songStartedPlaying != null)
                    songStartedPlaying.ShowedAsPlaying = (Session.SelectedPlaylist.Equals(Session.PlayingPlaylist));
            }
        }

        public bool SetSelectedSongPlaying(bool startPlaying)
        {
            if (!(Session.SelectedPlaylist is PlaylistItem pl))
            {
                Log.Error("Tried to start playing a song without a valid playlist selected, is {playlist}", Session.SelectedPlaylist);
                return false;
            }

            SongItem? song = ShownSongs.FirstOrDefault(x => x.IsSelected) ?? ShownSongs.FirstOrDefault();
            if (song == null)
            {
                Log.Error("Tried to start playing a song without any songs visible");
                return false;
            }

            Session.SetPlayingSong(song, pl, Session.SelectedMasteryLevels, startPlaying);
            return true;
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
