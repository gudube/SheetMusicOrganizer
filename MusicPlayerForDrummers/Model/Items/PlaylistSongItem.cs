using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.Model
{
    public class PlaylistSongItem : BaseModelItem
    {
        private int _playlistID;
        public int PlaylistID { get => _playlistID; set => SetField(ref _playlistID, value); }

        private int _songID;
        public int SongID { get => _songID; set => SetField(ref _songID, value); }

        public PlaylistSongItem(int playlistID, int songID) : base()
        {
            PlaylistID = playlistID;
            SongID = songID;
        }

        public PlaylistSongItem(SqliteDataReader dataReader) : base(dataReader)
        {
            PlaylistSongTable playlistSongTable = new PlaylistSongTable();
            PlaylistID = dataReader.GetInt32(dataReader.GetOrdinal(playlistSongTable.PlaylistID.Name));
            SongID = dataReader.GetInt32(dataReader.GetOrdinal(playlistSongTable.SongID.Name));
        }

        public override object[] GetCustomValues()
        {
            return new object[] { PlaylistID, SongID };
        }
    }
}
