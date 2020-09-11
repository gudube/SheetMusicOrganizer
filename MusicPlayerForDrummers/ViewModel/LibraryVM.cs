using System;
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
        }

        public async Task InitializeData()
        {
            UpdatePlaylistsFromDb();
            UpdateMasteryLevelsFromDb();
            Task songs = UpdateSongsFromDb();
            await songs.ConfigureAwait(false);
            Session.Playlists.CollectionChanged += Playlists_CollectionChanged;
        }

        private void CreateDelegateCommands()
        {
            CreateNewPlaylistCommand = new DelegateCommand(x => CreateNewPlaylist(x == null ? "" : (string)x));
            DeleteSelectedPlaylistCommand = new DelegateCommand(_ => DeleteSelectedPlaylist());
            RenameSelectedPlaylistCommand = new DelegateCommand(x => RenameSelectedPlaylist(x == null ? "" : (string)x));
            PlaySelectedSongCommand = new DelegateCommand(_ => Session.SetSelectedSongPlaying());
            RemoveSelectedSongsCommand = new DelegateCommand(_ => RemoveSelectedSongs());
        }

        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.SelectedPlaylist))
                SelectedPlaylist_PropertyChanged();
        }

        #region Playlists
        private readonly AddPlaylistItem _addPlaylist = new AddPlaylistItem();
        
        public DelegateCommand? CreateNewPlaylistCommand { get; private set; }
        public DelegateCommand? DeleteSelectedPlaylistCommand { get; private set; }
        public DelegateCommand? RenameSelectedPlaylistCommand { get; private set; }

        private void UpdatePlaylistsFromDb()
        {
            List<BaseModelItem> playlists = new List<BaseModelItem>(DbHandler.GetAllPlaylists()){ _addPlaylist };
            Session.Playlists.Reset(playlists);
            Session.SelectedPlaylist = playlists[0];
        }
        private void Playlists_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Session.SelectedPlaylist != null && !Session.Playlists.Contains(Session.SelectedPlaylist))
                Session.SelectedPlaylist = null;
        }

        private async void SelectedPlaylist_PropertyChanged()
        {
            Session.Status.SelectingPlaylist = true;
            Session.SelectedSongs.Clear();
            await UpdateSongsFromDb().ConfigureAwait(false);
            Session.Status.SelectingPlaylist = false;
        }

        private void CreateNewPlaylist(string playlistName)
        {
            PlaylistItem newPlaylist = new PlaylistItem(playlistName);
            DbHandler.CreateNewPlaylist(newPlaylist);
            Session.Playlists.Insert(Session.Playlists.Count - 1, newPlaylist);
            Session.SelectedPlaylist = newPlaylist;
        }

        private void DeleteSelectedPlaylist()
        {
            if (!(Session.SelectedPlaylist is PlaylistItem plItem))
            {
                Log.Warning("Expected to have a PlaylistItem selected when DeleteSelectedPlaylist is called but is {selected}", Session.SelectedPlaylist);
                return;
            }
            int index = Session.Playlists.IndexOf(plItem) + 1;
            if (index > 0 && index < Session.Playlists.Count)
                Session.SelectedPlaylist = Session.Playlists[index];
            Session.Playlists.Remove(plItem);
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
        public void GoToSong(SongItem song)
        {
            Session.SelectedSongs.Clear();
            if(Session.SelectedMasteryLevels.Count > 0 && Session.SelectedMasteryLevels.All(x => x.Id != song.MasteryId))
                Session.SelectedMasteryLevels.Add(Session.MasteryLevels.First(x => x.Id == song.MasteryId));
            SongItem songToSelect = Session.Songs.FirstOrDefault(x => x.Id == song.Id);
            if(songToSelect == null)
            {
                Session.SelectedPlaylist = Session.Playlists[0];
                songToSelect = Session.Songs.FirstOrDefault(x => x.Id == song.Id);
            }
            Session.SelectedSongs.Add(songToSelect);
        }

        private async Task UpdateSongsFromDb()
        {
            if (Session.SelectedPlaylist == null || Session.SelectedPlaylist == _addPlaylist)
                Session.Songs.Clear();
            else
            {
                //todo: Make the dbhandler methods async and return task? try if that would work
                List<SongItem> songs = await DbHandler.GetSongs(Session.SelectedPlaylist.Id);
                Session.Songs.Reset(songs);
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
            Session.Songs.Reset(sortedSongs);
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
                if (Session.SelectedPlaylist.Id != 0 && (songCreated || Session.Songs.All(x => x.Id != song.Id)))
                {
                    Session.Songs.Add(song);
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

        public DelegateCommand? PlaySelectedSongCommand { get; private set; }

        public DelegateCommand? RemoveSelectedSongsCommand { get; private set; }
        private void RemoveSelectedSongs()
        {
            if (!(Session.SelectedPlaylist is PlaylistItem)) {
                Log.Warning("Expected selected playlist to be a PlaylistItem when RemoveSelectedSongs(), but is {SelectedPlaylist}", Session.SelectedPlaylist);
                return;
            }

            if (Session.SelectedSongs.Count == 0)
            {
                Log.Warning("Expected songs to be selected when RemoveSelectedSongs()");
                return;
            }

            if(Session.PlayingSong != null && Session.SelectedSongs.Contains(Session.PlayingSong))
                Session.StopPlayingSong();

            int[] songIDs = Session.SelectedSongs.Select(x => x.Id).ToArray();
            if (Session.SelectedPlaylist == Session.Playlists[0])
            {
                DbHandler.DeleteSongs(songIDs);
            }
            else
            {
                DbHandler.RemoveSongsFromPlaylist(Session.SelectedPlaylist.Id, songIDs);
            }
            foreach(SongItem song in Session.SelectedSongs.ToList())
                Session.Songs.Remove(song);
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
