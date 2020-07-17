
namespace MusicPlayerForDrummers.Model.Items
{
    public class PlaylistSongItem : BaseModelItem
    {
        private int _playlistId;
        public int PlaylistId { get => _playlistId; set => SetField(ref _playlistId, value); }

        private int _songId;
        public int SongId { get => _songId; set => SetField(ref _songId, value); }

        public PlaylistSongItem(int playlistId, int songId) : base()
        {
            PlaylistId = playlistId;
            SongId = songId;
        }

        /*
        public PlaylistSongItem(SqliteDataReader dataReader) : base(dataReader)
        {
            PlaylistSongTable playlistSongTable = new PlaylistSongTable();
            int? playlistId = GetSafeInt(dataReader, playlistSongTable.PlaylistId.Name);
            if(playlistId == null)
                Log.Error("Missing playlist id for PlaylistSongItem with Id {Id}", Id);
            PlaylistId = playlistId.GetValueOrDefault()
            SongId = GetSafeInt(dataReader, playlistSongTable.SongId.Name);
        }*/

        public override object[] GetCustomValues()
        {
            return new object[] { PlaylistId, SongId };
        }
    }
}
