using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerForDrummers.Data
{
    public class PlaylistDBItem : DBItem
    {
        private SqlProperty _playlistName = new SqlProperty("Name", EType.TEXT);
        private SqlProperty _playlistLocked = new SqlProperty("Locked", EType.BOOL);

        public PlaylistDBItem(string playlistName = "", bool playlistLocked = false) : base("Playlist")
        {
            _playlistName.Value = playlistName;
            _playlistLocked.Value = playlistLocked ? "1" : "0";
            setCustomProperties(_playlistName, _playlistLocked);
        }
    }
}
