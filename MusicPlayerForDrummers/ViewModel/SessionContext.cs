using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel.Tools;
using NaudioWrapper;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;
using MusicPlayerForDrummers.Model.Items;

namespace MusicPlayerForDrummers.ViewModel
{
    public class SessionContext : Model.Tools.BaseNotifyPropertyChanged
    {
        public SessionContext()
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            timer.Tick += (sender, e) => PlayerTimerUpdate();
            PlayerTimerUpdate += () => Player.OnPropertyChanged(nameof(Player.Position));

            Player = new AudioPlayer(0.25f);
            Player.PlaybackStarting += () => timer.Start();
            Player.PlaybackStopping += () => timer.Stop();
        }

        #region Playlists
        private SmartCollection<BaseModelItem> _playlists = new SmartCollection<BaseModelItem>();
        public SmartCollection<BaseModelItem> Playlists { get => _playlists; set => SetField(ref _playlists, value); }

        private INotifyPropertyChanged _selectedPlaylist;
        public BaseModelItem SelectedPlaylist { get => (BaseModelItem) _selectedPlaylist; set => SetField(ref _selectedPlaylist, value); }
        #endregion

        #region Mastery Levels
        private SmartCollection<MasteryItem> _masteryLevels = new SmartCollection<MasteryItem>();
        public SmartCollection<MasteryItem> MasteryLevels { get => _masteryLevels; set => SetField(ref _masteryLevels, value); }

        private SmartCollection<MasteryItem> _selectedMasteryLevels = new SmartCollection<MasteryItem>();
        public SmartCollection<MasteryItem> SelectedMasteryLevels { get => _selectedMasteryLevels; set => SetField(ref _selectedMasteryLevels, value); }
        #endregion

        #region Songs
        private SmartCollection<SongItem> _songs = new SmartCollection<SongItem>();
        public SmartCollection<SongItem> Songs { get => _songs; set => SetField(ref _songs, value); }

        private SmartCollection<SongItem> _selectedSongs = new SmartCollection<SongItem>();
        public SmartCollection<SongItem> SelectedSongs { get => _selectedSongs; set => SetField(ref _selectedSongs, value); }
        #endregion

        #region PlayingSong
        private INotifyPropertyChanged _playingSong;
        public SongItem PlayingSong { get => (SongItem)_playingSong; private set => SetField(ref _playingSong, value); }

        private INotifyPropertyChanged _playingPlaylist;
        public PlaylistItem PlayingPlaylist { get => (PlaylistItem)_playingPlaylist; private set => SetField(ref _playingPlaylist, value); }

        private SmartCollection<MasteryItem> _playingMasteryLevels;
        public SmartCollection<MasteryItem> PlayingMasteryLevels { get => _playingMasteryLevels; private set => SetField(ref _playingMasteryLevels, value); }

        public void StopPlayingSong()
        {
            Player.Stop();
            PlayingSong = null;
            PlayingPlaylist = null;
            PlayingMasteryLevels.Clear();
        }

        private void SetPlayingSong(SongItem song)
        {
            PlayingSong = song;
            Player.SetSong(song.AudioDirectory);
        }

        private void SetPlayingSong(SongItem song, PlaylistItem playlist, SmartCollection<MasteryItem> masteryLevels)
        {
            PlayingPlaylist = playlist;
            PlayingMasteryLevels = masteryLevels;
            SetPlayingSong(song);
        }

        public void SetSelectedSongPlaying()
        {
            if (!(this.SelectedSongs.Count > 0))
                Trace.WriteLine("Tried to start playing a song without songs selected.");
            else if (!(this.SelectedPlaylist is PlaylistItem pl))
                Trace.WriteLine("Tried to start playing a song without a playlist selected.");
            else
                SetPlayingSong(this.SelectedSongs[0], pl, this.SelectedMasteryLevels);
        }

        public void SetNextPlayingSong()
        {
            SongItem next = DbHandler.FindNextSong(PlayingSong.Id, PlayingPlaylist.Id, PlayingMasteryLevels.Select(x => x.Id).ToArray());

            if (next == null)
                StopPlayingSong();
            else
                SetPlayingSong(next);
        }

        public void SetPreviousPlayingSong()
        {
            SongItem previous = DbHandler.FindPreviousSong(PlayingSong.Id, PlayingPlaylist.Id, PlayingMasteryLevels.Select(x => x.Id).ToArray());

            if (previous == null)
                StopPlayingSong();
            else
                SetPlayingSong(previous);
        }
        #endregion

        #region AudioPlayer
        public AudioPlayer Player { get; }

        public event Action PlayerTimerUpdate;
        #endregion
    }
}
