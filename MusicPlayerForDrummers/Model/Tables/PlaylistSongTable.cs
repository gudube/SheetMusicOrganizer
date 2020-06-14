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

            //TODO: Improve the DB by adding constraints (like not null and OnUpdate)
            PlaylistID = new SqlColumn("PlaylistID", EType.INT, playlistTable.TableName, playlistTable.ID.Name, true);
            SongID = new SqlColumn("SongID", EType.INT, songTable.TableName, songTable.ID.Name, true);
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
