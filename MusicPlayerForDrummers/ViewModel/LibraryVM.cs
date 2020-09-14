﻿using MusicPlayerForDrummers.Model;
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
        }

        #region Initialization
        public async Task InitializeData()
        {
            Playlists.CollectionChanged += Playlists_CollectionChanged;
            UpdatePlaylistsFromDb();
            UpdateMasteryLevelsFromDb();
            Task songs = UpdateSongsFromDb();
            await songs.ConfigureAwait(false);
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
            if(e.PropertyName == nameof(Session.PlayingSong))
                RefreshSongShowedAsPlaying();
        }
        #endregion

        #region Playlists
        private SmartCollection<BasePlaylistItem> _playlists = new SmartCollection<BasePlaylistItem>();
        public SmartCollection<BasePlaylistItem> Playlists { get => _playlists; set => SetField(ref _playlists, value); }

        private readonly AddPlaylistItem _addPlaylist = new AddPlaylistItem();
        
        public DelegateCommand? CreateNewPlaylistCommand { get; private set; }
        public DelegateCommand? DeleteSelectedPlaylistCommand { get; private set; }
        public DelegateCommand? RenameSelectedPlaylistCommand { get; private set; }

        //We are assuming there is always only one selected playlist
        private PlaylistItem? GetSelectedPlaylist()
        {
            PlaylistItem? selectedPlaylist = Playlists.OfType<PlaylistItem>().FirstOrDefault(x => x.IsSelected);
            if (selectedPlaylist == null)
            {
                Log.Warning("Trying to get selected playlist when there is no selected playlist");
            }
            return selectedPlaylist;
        }

        private void UpdatePlaylistsFromDb()
        {
            List<BasePlaylistItem> playlists = new List<BasePlaylistItem>(DbHandler.GetAllPlaylists()){ _addPlaylist };
            if(playlists.First() is PlaylistItem playlist)
                playlist.IsSelected = true;
            Playlists.Reset(playlists);
        }
        private void Playlists_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (BasePlaylistItem item in Playlists) 
                item.PropertyChanged += Playlist_PropertyChanged;
        }

        private void Playlist_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlaylistItem.IsSelected) || e.PropertyName == nameof(AddPlaylistItem.IsSelected))
                if(sender is PlaylistItem plItem && !plItem.IsSelected
                || sender is AddPlaylistItem addItem && !addItem.IsSelected) //so that it doesn't refresh twice for selecting and then unselecting
                    SelectedPlaylistChanged();
            if(e.PropertyName == nameof(PlaylistItem.IsPlaying))
                RefreshSongShowedAsPlaying();

        }

        private async void SelectedPlaylistChanged()
        {
            Session.Status.SelectingPlaylist = true;
            await UpdateSongsFromDb();
            Session.Status.SelectingPlaylist = false;
        }

        private void CreateNewPlaylist(string playlistName)
        {
            PlaylistItem newPlaylist = new PlaylistItem(playlistName);
            DbHandler.CreateNewPlaylist(newPlaylist);

            if (Playlists.Last() is AddPlaylistItem addPlaylist)
                addPlaylist.IsSelected = false;
            newPlaylist.IsSelected = true;
            Playlists.Insert(Playlists.Count - 1, newPlaylist);
        }

        //todo: add confirmation window when trying to delete a playlist e.g. Are you sure to... it will delete the playlist but the songs are still available in All Songs
        private void DeleteSelectedPlaylist()
        {
            PlaylistItem? selectedPlaylist = GetSelectedPlaylist();
            if (selectedPlaylist == null)
                return;

            int currentIndex = Playlists.IndexOf(selectedPlaylist);
            int nextIndex = currentIndex + 1;
            if (nextIndex > 0 && nextIndex < Playlists.Count - 1)
            {
                ((PlaylistItem)Playlists[nextIndex]).IsSelected = true;
            } else {
                int previousIndex = currentIndex - 1;
                if(previousIndex >= 0)
                    ((PlaylistItem)Playlists[previousIndex]).IsSelected = true;
            }
            
            Playlists.Remove(selectedPlaylist);
            DbHandler.DeletePlaylist(selectedPlaylist);
        }

        private void RenameSelectedPlaylist(string playlistName)
        {
            PlaylistItem? selectedPlaylist = GetSelectedPlaylist();
            if (selectedPlaylist != null)
            {
                selectedPlaylist.Name = playlistName;
                DbHandler.UpdatePlaylist(selectedPlaylist);
            }
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
            if(!song.Mastery.IsSelected)
                song.Mastery.IsSelected = true;
            SongItem songToSelect = ShownSongs.FirstOrDefault(x => x.Id == song.Id);
            if(songToSelect == null)
            {
                foreach (BasePlaylistItem playlist in Playlists)
                    playlist.IsSelected = false;
                ((PlaylistItem)Playlists[0]).IsSelected = true; //todo: will that work? or manually change playlist and call update of shownsongs
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
            PlaylistItem? selectedPlaylist = GetSelectedPlaylist();
            if (selectedPlaylist == null)
                ShownSongs.Clear();
            else
            {
                //todo: Make the dbhandler methods async and return task? try if that would work
                List<SongItem> songs = await DbHandler.GetSongs(selectedPlaylist.Id);
                if (Session.PlayingSong != null) //sets if any of the new songs is playing
                {
                    SongItem? playingSong = songs.FirstOrDefault(x => x.Id == Session.PlayingSong.Id);
                    if(playingSong != null)
                        playingSong.ShowedAsPlaying = Playlists.OfType<PlaylistItem>().Any(x => x.IsPlaying && x.IsSelected);
                }
                ShownSongs.Reset(songs);
            }
        }

        public void SortSongs(string propertyName, bool ascending)
        {
            PlaylistItem? selectedPlaylist = GetSelectedPlaylist();
            if (selectedPlaylist == null)
            {
                Log.Warning("Trying to sort songs when there are no selected playlist");
                return;
            }

            Session.Status.SavingSongOrder = true;
            Session.Status.SortingSongs = true;
            List<SongItem> sortedSongs =
                DbHandler.SortSongs(selectedPlaylist.Id, propertyName, ascending);
            Session.Status.SavingSongOrder = false;
            ShownSongs.Reset(sortedSongs);
        }

        //Resets the songs in the database for current playlist from the songIDs in the same order
        public void ResetSongsInCurrentPlaylist(IEnumerable<int> songIDs)
        {
            PlaylistItem? selectedPlaylist = GetSelectedPlaylist();
            if (selectedPlaylist == null)
            {
                Log.Warning("Trying to save songs in the database when there are no selected playlist");
                return;
            }
            
            DbHandler.ResetSongsInPlaylist(selectedPlaylist.Id, songIDs);
        }

        public bool AddSong(SongItem song)
        {
            if (DbHandler.IsSongExisting(song.PartitionDirectory))
            {
                return false;
                //song = DbHandler.GetSong(song.PartitionDirectory);
            }
            MasteryItem[] selectedMasteryItems = Session.MasteryLevels.Where(x => x.IsSelected).ToArray();
            if (selectedMasteryItems.Length == 0)
                song.MasteryId = Session.MasteryLevels[0].Id;
            else
                song.MasteryId = selectedMasteryItems[0].Id;

            DbHandler.AddSong(song);

            PlaylistItem? selectedPlaylist = GetSelectedPlaylist();
            if (selectedPlaylist == null)
            {
                Log.Warning("Trying to add a new song in the database when there are no selected playlist");
            }
            else
            {
                ShownSongs.Add(song);
                DbHandler.AddPlaylistSongLink(selectedPlaylist.Id, song.Id);
            }

            return true;
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
            SongItem[] selectedSongs = ShownSongs.Where(x => x.IsSelected).ToArray();

            if (selectedSongs.Length == 0)
            {
                Log.Warning("No songs selected when trying to RemoveSelectedSongs()");
                return;
            }

            int[] selectedSongIDs = selectedSongs.Select(x => x.Id).ToArray();

            if(Session.PlayingSong != null && selectedSongIDs.Contains(Session.PlayingSong.Id))
                StopPlayingSong();

            PlaylistItem? selectedPlaylist = GetSelectedPlaylist();

            if (selectedPlaylist == null)
                return;
            else if (Equals(Playlists[0], selectedPlaylist))
                DbHandler.DeleteSongs(selectedSongIDs);
            else
                DbHandler.RemoveSongsFromPlaylist(selectedPlaylist.Id, selectedSongIDs);
            
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

            PlaylistItem? selectedPlaylist = GetSelectedPlaylist();
            if (Session.PlayingSong != null && selectedPlaylist != null)
            {
                SongItem? songStartedPlaying = ShownSongs.FirstOrDefault(x => x.Id == Session.PlayingSong.Id);
                if(songStartedPlaying != null)
                    songStartedPlaying.ShowedAsPlaying = Playlists.OfType<PlaylistItem>().Any(x => x.IsPlaying && x.IsSelected);
            }
        }

        public bool SetSelectedSongPlaying(bool startPlaying)
        {
            //todo: should also consider if there is a song playing before. what if no song playing nor shown?
            SongItem? song = ShownSongs.FirstOrDefault(x => x.IsSelected) ?? ShownSongs.FirstOrDefault();
            if (song == null)
            {
                Log.Error("Tried to start playing a song without any songs visible");
                return false;
            }

            foreach(BasePlaylistItem item in Playlists)
                if (item is PlaylistItem pl)
                    pl.IsPlaying = pl.IsSelected;

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
            Session.PlayingSong = song;
            Session.Player.SetSong(song.AudioDirectory, startPlaying);
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

        public void SetNextPlayingSong(bool next)
        {
            //TODO: add a symbol next to the playing playlist and mastery levels to make it less confusing
            if (Session.PlayingSong == null)
            {
                SetSelectedSongPlaying(true);
                return;
            }
            
            SongItem? newSong;

            PlaylistItem? playingPlaylist = Playlists.FirstOrDefault(x => x is PlaylistItem pl && pl.IsPlaying) as PlaylistItem;

            if (playingPlaylist == null)
            {
                Log.Warning("Playing playlist is null when trying to go to play {next} song", (next ? "next" : "previous"));
                return;
            }

            if(next)
                newSong = DbHandler.FindNextSong(Session.PlayingSong.Id, playingPlaylist.Id, 
                    Session.MasteryLevels.Where(x => x.IsPlaying).Select(x => x.Id).ToArray());
            else
                newSong = DbHandler.FindPreviousSong(Session.PlayingSong.Id, playingPlaylist.Id, 
                    Session.MasteryLevels.Where(x => x.IsPlaying).Select(x => x.Id).ToArray());
            
            if (newSong == null)
                StopPlayingSong();
            else
                SetPlayingSong(newSong, true);
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
