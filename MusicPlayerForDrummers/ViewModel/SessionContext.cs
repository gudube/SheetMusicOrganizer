using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.Model.Tools;
using MusicPlayerForDrummers.ViewModel.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Windows.Media.Playlists;

namespace MusicPlayerForDrummers.ViewModel
{
    public class SessionContext : BaseNotifyPropertyChanged
    {
        public SessionContext()
        {

        }

        #region Playlists
        private SmartCollection<BaseModelItem> _playlists = new SmartCollection<BaseModelItem>();
        public SmartCollection<BaseModelItem> Playlists { get => _playlists; set => SetField(ref _playlists, value); }

        public INotifyPropertyChanged _selectedPlaylist;
        public BaseModelItem SelectedPlaylist { get => (BaseModelItem) _selectedPlaylist; set => SetField(ref _selectedPlaylist, value); }

        public int AllMusicPlaylistID { get => DBHandler.ALLMUSIC_PL_ID; }
        #endregion

        #region Mastery Levels
        private SmartCollection<MasteryItem> _masteryLevels = new SmartCollection<MasteryItem>();
        public SmartCollection<MasteryItem> MasteryLevels { get => _masteryLevels; set => SetField(ref _masteryLevels, value); }

        private SmartCollection<MasteryItem> _selectedMasteryLevels = new SmartCollection<MasteryItem>();
        public SmartCollection<MasteryItem> SelectedMasteryLevels { get => _selectedMasteryLevels; set => SetField(ref _selectedMasteryLevels, value); }
        #endregion

        #region Songs
        private SmartCollection<SongItem> songsInPlaylist = new SmartCollection<SongItem>();
        public SmartCollection<SongItem> SongsInPlaylist { get => songsInPlaylist; set => SetField(ref songsInPlaylist, value); }

        private SmartCollection<SongItem> _selectedSongs = new SmartCollection<SongItem>();
        public SmartCollection<SongItem> SelectedSongs { get => _selectedSongs; set => SetField(ref _selectedSongs, value); }
        #endregion

        #region Playing
        private INotifyPropertyChanged _playingSong;
        public SongItem PlayingSong { get => (SongItem)_playingSong; private set => SetField(ref _playingSong, value); }

        private INotifyPropertyChanged _playingPlaylist;
        public PlaylistItem PlayingPlaylist { get => (PlaylistItem)_playingPlaylist; private set => SetField(ref _playingPlaylist, value); }

        private SmartCollection<MasteryItem> _playingMasteryLevels;
        public SmartCollection<MasteryItem> PlayingMasteryLevels { get => _playingMasteryLevels; private set => SetField(ref _playingMasteryLevels, value); }

        public void StopPlayingSong()
        {
            PlayingSong = null;
            PlayingPlaylist = null;
            PlayingMasteryLevels.Clear();
        }

        private void SetPlayingSong(SongItem song, PlaylistItem playlist, SmartCollection<MasteryItem> masteryLevels)
        {
            PlayingSong = song;
            PlayingPlaylist = playlist;
            PlayingMasteryLevels = masteryLevels;
        }

        public void SetSelectedSongPlaying()
        {
            if (!(this.SelectedSongs.Count > 0))
                Trace.WriteLine("Tried to start playing a song without songs selected.");
            else if (!(this.SelectedPlaylist is PlaylistItem pl))
                Trace.WriteLine("Tried to start playing a song without a playlist selected.");
            else
            {
                SetPlayingSong(this.SelectedSongs[0], pl, this.SelectedMasteryLevels);
            }
        }

        public void SetNextPlayingSong()
        {
            SongItem next = DBHandler.FindNextSong(PlayingSong.ID, PlayingPlaylist.ID, PlayingMasteryLevels.Select(x => x.ID).ToArray());
            if (next == null && SelectedSongs.Count > 0)
                SetSelectedSongPlaying();
            else
                PlayingSong = next;
        }

        public void SetPreviousPlayingSong()
        {
            SongItem previous = DBHandler.FindPreviousSong(PlayingSong.ID, PlayingPlaylist.ID, PlayingMasteryLevels.Select(x => x.ID).ToArray());
            if(previous == null && SelectedSongs.Count > 0)
                SetSelectedSongPlaying();
            else
                PlayingSong = previous;
        }
        #endregion
    }
}
