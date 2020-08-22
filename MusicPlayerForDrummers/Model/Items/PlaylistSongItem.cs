
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

        private int? _previousPSId;
        public int? PreviousPSId { get => _previousPSId; set => SetField(ref _previousPSId, value); }

        private int? _nextPSId;
        public int? NextPSId { get => _nextPSId; set => SetField(ref _nextPSId, value); }

        public PlaylistSongItem(int playlistId, int songId, int? previousPSId, int? nextPSId) : base()
        {
            _playlistId = playlistId;
            _songId = songId;
            _previousPSId = previousPSId;
            _nextPSId = nextPSId;
        }
        
        public PlaylistSongItem(SqliteDataReader dataReader) : base(dataReader)
        {
            PlaylistSongTable playlistSongTable = new PlaylistSongTable();
            int? pId = GetSafeInt(dataReader, playlistSongTable.PlaylistId.Name);
            if (!pId.HasValue)
                Log.Error("The playlistSongItem with the id {id} doesn't have a valid playlistId", this.Id);
            PlaylistId = pId.GetValueOrDefault(0);
            
            int? sId = GetSafeInt(dataReader, playlistSongTable.SongId.Name);
            if (!sId.HasValue)
                Log.Error("The playlistSongItem with the id {id} doesn't have a valid songId", this.Id);
            SongId = sId.GetValueOrDefault(0);

            PreviousPSId = GetSafeInt(dataReader, playlistSongTable.PreviousPSId.Name);

            NextPSId = GetSafeInt(dataReader, playlistSongTable.NextPSId.Name);
        }

        public override object[] GetCustomValues()
        {
            return new object[] { PlaylistId, SongId, PreviousPSId, NextPSId };
        }
    }
}
