namespace MusicPlayerForDrummers.Model.Tables
{
    public class PlaylistSongTable : BaseTable
    {
        public PlaylistSongTable() : base("PlaylistSong")
        {
            PlaylistTable playlistTable = new PlaylistTable();
            SongTable songTable = new SongTable();

            //TODO: Improve the DB by adding constraints (like not null and OnUpdate)
            PlaylistId = new SqlColumn("PlaylistId", EType.Int, playlistTable.TableName, playlistTable.Id.Name, true);
            SongId = new SqlColumn("SongId", EType.Int, songTable.TableName, songTable.Id.Name, true);
            PreviousPSId = new SqlColumn("PreviousPSId", EType.Int);
            NextPSId = new SqlColumn("NextPSId", EType.Int);
        }
        public override SqlColumn[] GetCustomColumns()
        {
            return new [] { PlaylistId, SongId, PreviousPSId, NextPSId };
        }

        #region Custom Columns
        public readonly SqlColumn PlaylistId;
        public readonly SqlColumn SongId;
        public readonly SqlColumn PreviousPSId;
        public readonly SqlColumn NextPSId;
        #endregion
    }
}
