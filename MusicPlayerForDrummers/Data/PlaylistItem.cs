using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.Data
{
    public class PlaylistItem
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public bool Locked { get; private set; }

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
}
