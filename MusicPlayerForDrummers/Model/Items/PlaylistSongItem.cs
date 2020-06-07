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

        private int _previousID;
        public int PreviousID { get => _previousID; set => SetField(ref _previousID, value); }

        public PlaylistSongItem(int playlistID, int songID, int previousID = -1) : base()
        {
            PlaylistID = playlistID;
            SongID = songID;
            PreviousID = previousID;
        }

        public PlaylistSongItem(SqliteDataReader dataReader) : base(dataReader)
        {
            PlaylistSongTable playlistSongTable = new PlaylistSongTable();
            PlaylistID = (int) GetSafeInt(dataReader, playlistSongTable.PlaylistID.Name);
            SongID = (int) GetSafeInt(dataReader, playlistSongTable.SongID.Name);
            PreviousID = (int) GetSafeInt(dataReader, playlistSongTable.PreviousID.Name);
        }

        public override object[] GetCustomValues()
        {
            return new object[] { PlaylistID, SongID, PreviousID };
        }
    }
}
