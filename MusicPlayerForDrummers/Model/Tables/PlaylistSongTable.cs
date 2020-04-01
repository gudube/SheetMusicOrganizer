using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.Model
{
    public class PlaylistSongTable : BaseTable
    {
        public PlaylistSongTable() : base()
        {
            PlaylistTable playlistTable = new PlaylistTable();
            SongTable songTable = new SongTable();

            PlaylistID = new SqlColumn("PlaylistID", EType.TEXT, playlistTable.TableName, playlistTable.ID.Name);
            SongID = new SqlColumn("SongID", EType.TEXT, songTable.TableName, songTable.ID.Name);
        }
        public override string TableName => "PlaylistSong";

        public override SqlColumn[] GetCustomColumns()
        {
            return new SqlColumn[] { PlaylistID, SongID };
        }

        #region Custom Columns
        public readonly SqlColumn PlaylistID;
        public readonly SqlColumn SongID;
        #endregion
    }
}
