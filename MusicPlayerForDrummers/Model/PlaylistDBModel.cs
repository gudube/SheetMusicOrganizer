using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.Model
{
    //The model contains the table name and its columns
    public static class PlaylistDBModel
    {
        public static string TableName { get { return "Playlist"; } }
        
        private static SqlColumn _ID = new SqlColumn(TableName + "ID", EType.INT, true);
        public static string IDColName { get { return _ID.Name; } }
        
        private static SqlColumn _playlistName = new SqlColumn("Name", EType.TEXT);
        public static string NameColName { get { return _playlistName.Name; } }

        private static SqlColumn _playlistLocked = new SqlColumn("Locked", EType.BOOL);
        public static string LockedColName { get { return _playlistLocked.Name; } }

        public static SqlColumn[] GetAllColumns()
        {
            return new SqlColumn[] { _ID, _playlistName, _playlistLocked};
        }

        public static SqlColumn[] GetCustomColumns()
        {
            return new SqlColumn[] { _playlistName, _playlistLocked };
        }
    }
}
