using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace MusicPlayerForDrummers.Model
{
    public class PlaylistItem : BaseModelItem
    {
        private string _name;
        public string Name { get => _name; set => SetField(ref _name, value); }

        private bool _isLocked;
        public bool IsLocked { get => _isLocked; set => SetField(ref _isLocked, value); }

        private bool _isSmart;
        public bool IsSmart { get => _isSmart; set => SetField(ref _isSmart, value); }

        private string _smartDir;
        public string SmartDir { get => _smartDir; set => SetField(ref _smartDir, value); }

        public PlaylistItem(string name, bool locked = false) : base()
        {
            Name = name;
            IsLocked = locked;
            IsSmart = false;
            SmartDir = "";
        }

        public PlaylistItem(string name, string smartDir, bool locked = false) : base()
        {
            Name = name;
            IsLocked = locked;
            IsSmart = true;
            SmartDir = smartDir;
        }

        public PlaylistItem(SqliteDataReader dataReader) : base(dataReader)
        {
            PlaylistTable playlistTable = new PlaylistTable();
            Name = GetSafeString(dataReader, playlistTable.Name.Name);
            IsLocked = GetSafeBool(dataReader, playlistTable.IsLocked.Name);
            IsSmart = GetSafeBool(dataReader, playlistTable.IsSmart.Name);
            SmartDir = GetSafeString(dataReader, playlistTable.SmartDir.Name);
        }

        public override object[] GetCustomValues()
        {
            return new object[] { Name, IsLocked, IsSmart, SmartDir };
        }
    }

    public class AddPlaylistItem : BaseModelItem
    {
        public override object[] GetCustomValues()
        {
            throw new NotImplementedException();
        }
    };
}
