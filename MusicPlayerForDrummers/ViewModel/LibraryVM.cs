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
using MusicPlayerForDrummers.Model.Tools;
using System.ComponentModel;
using Windows.Media.Playlists;
using System.IO;

namespace MusicPlayerForDrummers.ViewModel
{
    public class LibraryVM : BaseViewModel
    {
        public override string ViewModelName => "LIBRARY";

        //TODO: Separer le LibraryVM est plusieurs VM?
        public LibraryVM(SessionContext session) : base(session)
        {
            UpdatePlaylistsFromDB();
            UpdateMasteryLevelsFromDB();
            UpdateSongsFromDB();
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
        public DelegateCommand IsRenamingPlaylistCommand { get; private set; }

        private void UpdatePlaylistsFromDB()
        {
            List<BaseModelItem> playlists = new List<BaseModelItem>{ _allMusicPlaylist };
            playlists.AddRange(DBHandler.GetAllPlaylists());
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
            UpdateSongsFromDB();
        }

        private void CreateNewPlaylist(string playlistName)
        {
            PlaylistItem newPlaylist = new PlaylistItem(playlistName);
            DBHandler.CreateNewPlaylist(newPlaylist);
            Session.Playlists.Insert(Session.Playlists.Count - 1, newPlaylist);
            Session.SelectedPlaylist = newPlaylist;
        }

        private void DeleteSelectedPlaylist()
        {
            if (!(Session.SelectedPlaylist is PlaylistItem plItem))
            {
                Trace.WriteLine("Expected to have a PlaylistItem selected when DeleteSelectedPlaylist is called.");
                return;
            }
            //Session.SelectedPlaylist = null; //TODO: Go to the next one, or last one if no next
            Session.Playlists.Remove(plItem);
            DBHandler.DeletePlaylist(plItem);
        }

        private void RenameSelectedPlaylist(string playlistName)
        {
            if (!(Session.SelectedPlaylist is PlaylistItem plItem))
            {
                Trace.WriteLine("Expected to have a PlaylistItem selected when RenameSelectedPlaylist is called.");
                return;
            }
            plItem.Name = playlistName;
            DBHandler.UpdatePlaylist(plItem);
        }

        public void CopySongToPlaylist(PlaylistItem playlist, SongItem song)
        {
            DBHandler.AddPlaylistSongLink(playlist.ID, song.ID);
        }

        public void CopySongsToPlaylist(PlaylistItem playlist, IEnumerable<SongItem> songs)
        {
            DBHandler.AddSongsToPlaylist(playlist.ID, songs.Select(x => x.ID));
        }

        public bool IsSongInPlaylist(PlaylistItem playlist, SongItem song)
        {
            return DBHandler.IsSongInPlaylist(playlist.ID, song.ID);
        }
        #endregion

        //TODO: Add icon to represent mastery (poker face, crooked smile, smile, fire?)
        //TODO: Multiple mastery levels are selectable using CTRL only, button to activate/deactivate mastery filter besides the expander
        #region Mastery Levels
        private void SelectedMasteryLevels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateSongsFromDB();
        }

        private void UpdateMasteryLevelsFromDB()
        {
            Session.MasteryLevels.Reset(DBHandler.GetAllMasteryLevels());
        }

        public bool IsSongInMastery(MasteryItem mastery, SongItem song)
        {
            return DBHandler.IsSongInMastery(mastery.ID, song.ID);
        }

        public void SetSongMastery(SongItem song, MasteryItem mastery)
        {
            DBHandler.SetSongMastery(song, mastery);
            if (!Session.SelectedMasteryLevels.Contains(mastery) && Session.Songs.Contains(song))
                Session.Songs.Remove(song);
        }
        public void SetSongsMastery(IEnumerable<SongItem> songs, MasteryItem mastery)
        {
            DBHandler.SetSongsMastery(songs, mastery);
            if (!Session.SelectedMasteryLevels.Contains(mastery))
            {
                foreach (SongItem song in songs)
                    if (Session.Songs.Contains(song))
                        Session.Songs.Remove(song);
            }
        }
        #endregion

        #region Songs
        public void GoToSong(SongItem song)
        {
            Session.SelectedSongs.Clear();
            if(Session.SelectedMasteryLevels.Count > 0 && !Session.SelectedMasteryLevels.Any(x => x.ID == song.MasteryID))
                Session.SelectedMasteryLevels.Add(Session.MasteryLevels.First(x => x.ID == song.MasteryID));
            SongItem songToSelect = Session.Songs.FirstOrDefault(x => x.ID == song.ID);
            if(songToSelect == null)
            {
                Session.SelectedPlaylist = _allMusicPlaylist;
                songToSelect = Session.Songs.FirstOrDefault(x => x.ID == song.ID);
            }
            Session.SelectedSongs.Add(songToSelect);
        }

        private void UpdateSongsFromDB()
        {
            int[] masteryIDs = Session.SelectedMasteryLevels.Select(x => x.ID).ToArray();

            if (Session.SelectedPlaylist == _allMusicPlaylist)
                Session.Songs.Reset(DBHandler.GetAllSongs(masteryIDs));
            else if (Session.SelectedPlaylist == null || Session.SelectedPlaylist == _addPlaylist)
                Session.Songs.Clear();
            else
                Session.Songs.Reset(DBHandler.GetSongs(Session.SelectedPlaylist.ID, masteryIDs));
        }

        public bool AddSong(SongItem song)
        {
            if (!DBHandler.IsSongExisting(song.PartitionDirectory))
            {
                AddNewSong(song);
                return true;
            }

            return false;
        }

        private void AddNewSong(SongItem song)
        {
            if (Session.SelectedMasteryLevels.Count == 0)
                song.MasteryID = Session.MasteryLevels[0].ID;
            else
                song.MasteryID = Session.SelectedMasteryLevels[0].ID;

            if (Session.SelectedPlaylist is PlaylistItem && Session.SelectedPlaylist != _allMusicPlaylist)
                DBHandler.AddSong(song, Session.SelectedPlaylist.ID);
            else
                DBHandler.AddSong(song);

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
                    AddFolder(subDir, recursive, useAudioMD);
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
                    bool audioFound = false;
                    foreach(string audio in audioFiles)
                    {
                        if (Path.GetFileNameWithoutExtension(partition) == Path.GetFileNameWithoutExtension(audio))
                        {
                            AddSong(new SongItem(partition, audio, 0, useAudioMD));
                            audioFound = true;
                            break;
                        }
                    }
                    if(!audioFound)
                        AddSong(new SongItem(partition));
                }
            }
        }

        public DelegateCommand PlaySelectedSongCommand { get; private set; }

        public DelegateCommand RemoveSelectedSongsCommand { get; private set; }
        private void RemoveSelectedSongs()
        {
            if (!(Session.SelectedPlaylist is PlaylistItem)) {
                Trace.WriteLine("Expected selected playlist to be a PlaylistItem when RemoveSelectedSongs(), but is : " + Session.SelectedPlaylist.GetType().Name);
                return;
            }

            if (Session.SelectedSongs.Count == 0)
            {
                Trace.WriteLine("Expected songs to be selected when RemoveSelectedSongs()");
                return;
            }

            int[] songIDs = Session.SelectedSongs.Select(x => x.ID).ToArray();
            if (Session.SelectedPlaylist == _allMusicPlaylist)
            {
                DBHandler.DeleteSongs(songIDs);
            }
            else
            {
                DBHandler.RemoveSongsFromPlaylist(Session.SelectedPlaylist.ID, songIDs);
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
