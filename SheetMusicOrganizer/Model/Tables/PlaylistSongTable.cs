namespace SheetMusicOrganizer.Model.Tables
{
    public class PlaylistSongTable : BaseTable
    {
        public PlaylistSongTable() : base("PlaylistSong")
        {
            PlaylistTable playlistTable = new PlaylistTable();
            SongTable songTable = new SongTable();

            PlaylistId = new SqlColumn("PlaylistId", EType.Int, playlistTable.TableName, playlistTable.Id.Name, true);
            SongId = new SqlColumn("SongId", EType.Int, songTable.TableName, songTable.Id.Name, true);
        }
        public override SqlColumn[] GetCustomColumns()
        {
            return new [] { PlaylistId, SongId };
        }

        #region Custom Columns
        public readonly SqlColumn PlaylistId;
        public readonly SqlColumn SongId;
        #endregion
    }
}
