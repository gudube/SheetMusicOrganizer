using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Serilog;
using SheetMusicOrganizer.Model.Tables;

namespace SheetMusicOrganizer.Model.Items
{
    public class BasePlaylistItem : BaseModelItem
    {
        // ReSharper disable once InconsistentNaming
        //protected bool _isSelected = false;
        //public virtual bool IsSelected { get => _isSelected; set => SetField(ref _isSelected, value); }
        public override object[] GetCustomValues()
        {
            throw new NotImplementedException();
        }
    }

    public class PlaylistItem : BasePlaylistItem
    {
        #region Properties
        private string _name;
        public string Name { get => _name; set => SetField(ref _name, value); }

        private bool _isLocked;
        public bool IsLocked { get => _isLocked; set => SetField(ref _isLocked, value); }

        private bool _isSmart;
        public bool IsSmart { get => _isSmart; set => SetField(ref _isSmart, value); }

        private string _smartDir;
        public string SmartDir { get => _smartDir; set => SetField(ref _smartDir, value); }

        private string _sortCol;
        public string SortCol { get => _sortCol;
            set {
                if(SetField(ref _sortCol, value))
                {
                    SetSongs(_songs);
                    DbHandler.UpdatePlaylist(this, new PlaylistTable().SortCol, value);
                }
            }
        }
        
        private bool _sortAsc;
        public bool SortAsc { get => _sortAsc; 
            set {
                if(SetField(ref _sortAsc, value))
                {
                    SetSongs(_songs);
                    DbHandler.UpdatePlaylist(this, new PlaylistTable().SortAsc, value);
                }
            }
        }
        #endregion

        #region Other Properties
        private bool _isPlaying = false;
        public bool IsPlaying { get => _isPlaying; set => SetField(ref _isPlaying, value); }

        private bool _isEditing = false;
        public bool IsEditing { get => _isEditing; set => SetField(ref _isEditing, value); }

        #endregion

        public PlaylistItem(string name, bool locked = false) : base()
        {
            _name = name;
            _isLocked = locked;
            _isSmart = false;
            _smartDir = "";
            _sortCol = new SongTable().Title.Name;
            _sortAsc = true;
        }

        public PlaylistItem(SqliteDataReader dataReader)
        {
            PlaylistTable playlistTable = new PlaylistTable();
            int? id = GetSafeInt(dataReader, playlistTable.Id.Name);
            if (!id.HasValue)
                Log.Error("Could not find the id when reading a PlaylistItem from the SqliteDataReader.");
            _id = id.GetValueOrDefault(-1);

            _name = GetSafeString(dataReader, playlistTable.Name.Name);
            _isLocked = GetSafeBool(dataReader, playlistTable.IsLocked.Name);
            _isSmart = GetSafeBool(dataReader, playlistTable.IsSmart.Name);
            _smartDir = GetSafeString(dataReader, playlistTable.SmartDir.Name);
            _sortCol = GetSafeString(dataReader, playlistTable.SortCol.Name);
            _sortAsc = GetSafeBool(dataReader, playlistTable.SortAsc.Name);
        }

        public override object[] GetCustomValues()
        {
            return new object[] { Name, IsLocked, IsSmart, SmartDir, SortCol, SortAsc };
        }

        //might be a bad idea? keep it for now
        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is PlaylistItem pl))
                return false;
            return Id.Equals(pl.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #region songs
        private SortedSet<SongItem> _songs = new SortedSet<SongItem>();

        public IEnumerable<SongItem> GetSongs()
        {
            return _songs;
        }

        public void SetSongs(IEnumerable<SongItem> newSongs)
        {
            PropertyInfo? songCol = typeof(SongItem).GetProperty(SortCol);
            if (songCol == null)
            {
                Log.Error("Sort column not existing in SongItem: {col}", SortCol);
                return;
            }
            var comparer = Comparer<SongItem>.Create((s1, s2) => {
                IComparable? val1 = songCol.GetValue(s1) as IComparable;
                IComparable? val2 = songCol.GetValue(s2) as IComparable;
                if(val1 == null || val2 == null) return 0;
                return SortAsc ? val1.CompareTo(val2) : val2.CompareTo(val1);
                }
            );
            _songs = new SortedSet<SongItem>(newSongs, comparer);
        }

        public bool HasSong(SongItem song)
        {
            return _songs.Contains(song);
        }

        public bool AddSongs(params SongItem[] newSongs)
        {
            List<int> addedSongs = new List<int>();
            foreach(SongItem song in newSongs)
            {
                if(_songs.Add(song)) addedSongs.Add(song.Id);
            }
            DbHandler.AddSongsToPlaylist(Id, addedSongs.ToArray());
            return newSongs.Length == addedSongs.Count;
        }

        public void RemoveSongs(params SongItem[] songsToRemove)
        {
            List<int> removedSongs = new List<int>();
            foreach(SongItem song in songsToRemove)
            {
                if(_songs.Remove(song)) removedSongs.Add(song.Id);
            }
            DbHandler.RemoveSongsFromPlaylist(Id, removedSongs);
        }

        #endregion
    }

    public class AddPlaylistItem : BasePlaylistItem
    {
        public override object[] GetCustomValues()
        {
            throw new NotImplementedException();
        }
    };
}
