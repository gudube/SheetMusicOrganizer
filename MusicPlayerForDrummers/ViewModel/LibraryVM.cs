using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel.Tools;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.IO;
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
            UpdatePlaylistsFromDb();
            UpdateMasteryLevelsFromDb();
            UpdateSongsFromDb();
            CreateDelegateCommands();
            Session.SelectedMasteryLevels.CollectionChanged += SelectedMasteryLevels_CollectionChanged;
            Session.Playlists.CollectionChanged += Playlists_CollectionChanged;
        }

        private void CreateDelegateCommands()
        {
            CreateNewPlaylistCommand = new DelegateCommand(x => CreateNewPlaylist((string)x));
            DeleteSelectedPlaylistCommand = new DelegateCommand(_ => DeleteSelectedPlaylist());
            RenameSelectedPlaylistCommand = new DelegateCommand(x => RenameSelectedPlaylist((string)x));
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
        private readonly PlaylistItem _allMusicPlaylist = new PlaylistItem("All music", true);
        
        public DelegateCommand CreateNewPlaylistCommand { get; private set; }
        public DelegateCommand DeleteSelectedPlaylistCommand { get; private set; }
        public DelegateCommand RenameSelectedPlaylistCommand { get; private set; }

        private void UpdatePlaylistsFromDb()
        {
            List<BaseModelItem> playlists = new List<BaseModelItem>{ _allMusicPlaylist };
            playlists.AddRange(DbHandler.GetAllPlaylists());
            playlists.Add(_addPlaylist);
            Session.Playlists.Reset(playlists);
            Session.SelectedPlaylist = _allMusicPlaylist;
        }
        private void Playlists_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Session.SelectedPlaylist != null && !Session.Playlists.Contains(Session.SelectedPlaylist))
                Session.SelectedPlaylist = null;
        }

        private void SelectedPlaylist_PropertyChanged()
        {
            Session.SelectedSongs.Clear();
            UpdateSongsFromDb();
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
            //Session.SelectedPlaylist = null; //TODO: Go to the next one, or last one if no next
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
            DbHandler.AddPlaylistSongLink(playlist.Id, song.Id);
        }

        public void CopySongsToPlaylist(PlaylistItem playlist, IEnumerable<SongItem> songs)
        {
            DbHandler.AddSongsToPlaylist(playlist.Id, songs.Select(x => x.Id));
        }

        public bool IsSongInPlaylist(PlaylistItem playlist, SongItem song)
        {
            return DbHandler.IsSongInPlaylist(playlist.Id, song.Id);
        }
        #endregion

        //TODO: Add icon to represent mastery (poker face, crooked smile, smile, fire?)
        //TODO: Multiple mastery levels are selectable using CTRL only, button to activate/deactivate mastery filter besides the expander
        #region Mastery Levels
        private void SelectedMasteryLevels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateSongsFromDb();
        }

        private void UpdateMasteryLevelsFromDb()
        {
            Session.MasteryLevels.Reset(DbHandler.GetAllMasteryLevels());
        }

        public bool IsSongInMastery(MasteryItem mastery, SongItem song)
        {
            return DbHandler.IsSongInMastery(mastery.Id, song.Id);
        }

        public void SetSongMastery(SongItem song, MasteryItem mastery)
        {
            DbHandler.SetSongMastery(song, mastery);
            if (!Session.SelectedMasteryLevels.Contains(mastery) && Session.Songs.Contains(song))
                Session.Songs.Remove(song);
        }
        public void SetSongsMastery(IEnumerable<SongItem> songs, MasteryItem mastery)
        {
            IEnumerable<SongItem> songItems = songs as SongItem[] ?? songs.ToArray();
            DbHandler.SetSongsMastery(songItems, mastery);
            if (!Session.SelectedMasteryLevels.Contains(mastery))
            {
                foreach (SongItem song in songItems)
                    if (Session.Songs.Contains(song))
                        Session.Songs.Remove(song);
            }
        }
        #endregion

        #region Songs
        public void GoToSong(SongItem song)
        {
            Session.SelectedSongs.Clear();
            if(Session.SelectedMasteryLevels.Count > 0 && !Session.SelectedMasteryLevels.Any(x => x.Id == song.MasteryId))
                Session.SelectedMasteryLevels.Add(Session.MasteryLevels.First(x => x.Id == song.MasteryId));
            SongItem songToSelect = Session.Songs.FirstOrDefault(x => x.Id == song.Id);
            if(songToSelect == null)
            {
                Session.SelectedPlaylist = _allMusicPlaylist;
                songToSelect = Session.Songs.FirstOrDefault(x => x.Id == song.Id);
            }
            Session.SelectedSongs.Add(songToSelect);
        }

        private void UpdateSongsFromDb()
        {
            int[] masteryIDs = Session.SelectedMasteryLevels.Select(x => x.Id).ToArray();

            if (Session.SelectedPlaylist == _allMusicPlaylist)
                Session.Songs.Reset(DbHandler.GetAllSongs(masteryIDs));
            else if (Session.SelectedPlaylist == null || Session.SelectedPlaylist == _addPlaylist)
                Session.Songs.Clear();
            else
                Session.Songs.Reset(DbHandler.GetSongs(Session.SelectedPlaylist.Id, masteryIDs));
        }

        public bool AddSong(SongItem song)
        {
            if (!DbHandler.IsSongExisting(song.PartitionDirectory))
            {
                AddNewSong(song);
                return true;
            }

            return false;
        }

        private void AddNewSong(SongItem song)
        {
            if (Session.SelectedMasteryLevels.Count == 0)
                song.MasteryId = Session.MasteryLevels[0].Id;
            else
                song.MasteryId = Session.SelectedMasteryLevels[0].Id;

            if (Session.SelectedPlaylist is PlaylistItem && Session.SelectedPlaylist != _allMusicPlaylist)
                DbHandler.AddSong(song, Session.SelectedPlaylist.Id);
            else
                DbHandler.AddSong(song);

            Session.Songs.Add(song);
        }

        //TODO: Add advanced options (like import music sheet only, audio only or both)
        //TODO: Add update existing songs (music sheet or audio) vs skip existing songs
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
                AddSong(new SongItem(partitionFiles[0], audioFiles[0], 0, useAudioMD));
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

        public DelegateCommand PlaySelectedSongCommand { get; private set; }

        public DelegateCommand RemoveSelectedSongsCommand { get; private set; }
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

            int[] songIDs = Session.SelectedSongs.Select(x => x.Id).ToArray();
            if (Session.SelectedPlaylist == _allMusicPlaylist)
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
