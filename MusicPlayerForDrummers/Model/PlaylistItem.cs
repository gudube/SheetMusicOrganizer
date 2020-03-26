using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MusicPlayerForDrummers.Model
{
    public class PlaylistItem : CustomListBoxItem
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public bool Locked { get; private set; }
        // bool IsRenaming { get; set; } 

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

    public class AddPlaylistItem : CustomListBoxItem {
        /*private bool _IsAddingPlaylistProperty;

        public bool IsAddingPlaylist
        {
            get
            {
                return _IsAddingPlaylistProperty;
            }
            set
            {
                if (value == _IsAddingPlaylistProperty)
                    return;
                _IsAddingPlaylistProperty = value;
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }*/
    };

    public interface  CustomListBoxItem { };
}
