using System;
using Microsoft.Data.Sqlite;
using MusicPlayerForDrummers.Model.Tables;
using Serilog;

namespace MusicPlayerForDrummers.Model.Items
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
        }

        public PlaylistItem(string name, string smartDir, bool locked = false) : base()
        {
            _name = name;
            _isLocked = locked;
            _isSmart = true;
            _smartDir = smartDir;
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
        }

        public override object[] GetCustomValues()
        {
            return new object[] { Name, IsLocked, IsSmart, SmartDir };
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
    }

    public class AddPlaylistItem : BasePlaylistItem
    {
        public override object[] GetCustomValues()
        {
            throw new NotImplementedException();
        }
    };
}
