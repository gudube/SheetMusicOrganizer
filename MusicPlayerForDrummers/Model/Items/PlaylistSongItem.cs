
using Microsoft.Data.Sqlite;
using MusicPlayerForDrummers.Model.Tables;
using Serilog;

namespace MusicPlayerForDrummers.Model.Items
{
    public class PlaylistSongItem : BaseModelItem
    {
        private int _playlistId;
        public int PlaylistId { get => _playlistId; set => SetField(ref _playlistId, value); }

        private int _songId;
        public int SongId { get => _songId; set => SetField(ref _songId, value); }

        private int _posInPlaylist;
        public int PosInPlaylist { get => _posInPlaylist; set => SetField(ref _posInPlaylist, value); }

        public PlaylistSongItem(int playlistId, int songId, int posInPlaylist) : base()
        {
            PlaylistId = playlistId;
            SongId = songId;
            PosInPlaylist = posInPlaylist;
        }

        
        public PlaylistSongItem(SqliteDataReader dataReader)
        {
            PlaylistSongTable playlistSongTable = new PlaylistSongTable();
            int? id = GetSafeInt(dataReader, playlistSongTable.Id.Name);
            if (!id.HasValue)
                Log.Error("Could not find the id when reading a PlaylistSongItem from the SqliteDataReader.");
            _id = id.GetValueOrDefault(-1);

            int? playlistId = GetSafeInt(dataReader, playlistSongTable.PlaylistId.Name);
            if(!playlistId.HasValue)
                Log.Error("Missing playlist id for PlaylistSongItem with Id {Id}", Id);
            PlaylistId = playlistId.GetValueOrDefault(-1);
            int? songId = GetSafeInt(dataReader, playlistSongTable.SongId.Name);
            if (!songId.HasValue)
                Log.Error("Missing song id for PlaylistSongItem with Id {Id}", Id);
            SongId = songId.GetValueOrDefault(-1);
            int? posInPlaylist = GetSafeInt(dataReader, playlistSongTable.PosInPlaylist.Name);
            if (!posInPlaylist.HasValue)
                Log.Error("Missing position in playlist for PlaylistSongItem with Id {Id}", Id);
            PosInPlaylist = posInPlaylist.GetValueOrDefault(-1);
        }

        public override object[] GetCustomValues()
        {
            return new object[] { PlaylistId, SongId, PosInPlaylist };
        }
    }
}
