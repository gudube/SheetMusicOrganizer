using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MusicPlayerForDrummers.Model
{
    public class PlaylistItem : BaseModelItem
    {
        public int ID { get; private set; }

        private string _name;
        public string Name { get => _name; set => SetField(ref _name, value); }

        private bool _locked;
        public bool Locked { get => _locked; set => SetField(ref _locked, value); }

        public PlaylistItem(string name, bool locked = false)
        {
            Name = name;
            Locked = locked;
        }

        public PlaylistItem(SqliteDataReader dataReader)
        {
            ID = dataReader.GetInt32(dataReader.GetOrdinal(PlaylistDBModel.IDColName));
            Name = dataReader.GetString(dataReader.GetOrdinal(PlaylistDBModel.NameColName));
            Locked = dataReader.GetBoolean(dataReader.GetOrdinal(PlaylistDBModel.LockedColName));
        }

        public string[] GetFormatedCustomValues()
        {
            return new string[] { "'" + Name + "'", Locked ? "1" : "0" };
        }
    }

    public class AddPlaylistItem : BaseModelItem {
    };
}
