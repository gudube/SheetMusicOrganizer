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

        private bool _locked;
        public bool Locked { get => _locked; set => SetField(ref _locked, value); }

        public PlaylistItem(string name, bool locked = false) : base()
        {
            Name = name;
            Locked = locked;
        }

        public PlaylistItem(SqliteDataReader dataReader) : base(dataReader)
        {
            PlaylistTable playlistTable = new PlaylistTable();
            Name = dataReader.GetString(dataReader.GetOrdinal(playlistTable.Name.Name));
            Locked = dataReader.GetBoolean(dataReader.GetOrdinal(playlistTable.Locked.Name));
        }

        public override object[] GetCustomValues()
        {
            return new object[] { Name, Locked };
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
