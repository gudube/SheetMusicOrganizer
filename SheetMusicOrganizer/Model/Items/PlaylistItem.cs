using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Serilog;
using SheetMusicOrganizer.Model.Tables;

namespace SheetMusicOrganizer.Model.Items
{
    public class PlaylistItem : BaseModelItem
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
                if (SetField(ref _sortCol, value))
                {
                    DbHandler.UpdatePlaylist(this, new PlaylistTable().SortCol, value);
                }
            }
        }
        
        private bool _sortAsc;
        public bool SortAsc { get => _sortAsc; 
            set {
                if(SetField(ref _sortAsc, value))
                {
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
        private ObservableCollection<SongItem> _songs = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Songs { get => _songs; set => SetField(ref _songs, value); }
        
        private ObservableCollection<object> _selectedSongs = new ObservableCollection<object>();
        public ObservableCollection<object> SelectedSongs { get => _selectedSongs; set => SetField(ref _selectedSongs, value); }
        private List<object>? _tempSelected;

        public void SortSongs()
        {
            var propInfo = typeof(SongItem).GetProperty(SortCol);
            if (propInfo == null)
                return;
            if (SortAsc)
                Songs = new ObservableCollection<SongItem>(Songs.OrderBy(source => propInfo.GetValue(source)));
            else
                Songs = new ObservableCollection<SongItem>(Songs.OrderByDescending(source => propInfo.GetValue(source)));
        }

        public bool HasSong(SongItem song)
        {
            return _songs.Any(x => x.Equals(song));
        }

        public bool AddSongs(IEnumerable<SongItem> newSongs)
        {
            List<int> addedSongs = new List<int>();
            foreach (SongItem song in newSongs)
            {
                if(HasSong(song))
                    continue;
                Songs.Add((SongItem)song.Clone());
                addedSongs.Add(song.Id);
            }
            DbHandler.AddSongsToPlaylist(Id, addedSongs.ToArray());
            SortSongs();
            return newSongs.Count() == addedSongs.Count;
        }

        public void UpdateSong(SongItem updatedSong)
        {
            for(var i = 0; i < Songs.Count; i++)
            {
                if (Songs[i].Id == updatedSong.Id)
                {
                    Songs[i] = updatedSong;
                    OnPropertyChanged(nameof(Songs));
                    return;
                }
            }
        }

        public void RemoveSongs(IEnumerable<SongItem> songsToRemove)
        {
            List<int> removedSongs = new List<int>();
            foreach(SongItem song in songsToRemove.ToArray())
            {
                if (HasSong(song))
                {
                    Songs.Remove(song);
                    SelectedSongs.Remove(song);
                    removedSongs.Add(song.Id);
                }
            }
            DbHandler.RemoveSongsFromPlaylist(Id, removedSongs);
        }

        public void PrepareChange()
        {
            IsEditing = false;
            _tempSelected = SelectedSongs.ToList();
        }

        public void ApplyChange()
        {
            if (_tempSelected != null && SelectedSongs.Count == 0)
            {
                foreach (var item in _tempSelected)
                    SelectedSongs.Add(item);
            }
            _tempSelected = null;
        }

        #endregion
    }
}
