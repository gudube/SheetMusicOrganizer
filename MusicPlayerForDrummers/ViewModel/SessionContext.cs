using MusicPlayerForDrummers.ViewModel.Tools;
using NAudioWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using MusicPlayerForDrummers.Model.Items;
using Serilog;

namespace MusicPlayerForDrummers.ViewModel
{
    public class SessionContext : Model.Tools.BaseNotifyPropertyChanged
    {
        public SessionContext()
        {
            Status = new StatusContext();

            Player = new AudioPlayer();

            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            timer.Tick += (sender, e) => PlayerTimerUpdate?.Invoke();
            PlayerTimerUpdate += () => Player.OnPropertyChanged(nameof(Player.Position));
            
            if (Settings.Default.Volume >= 0 && Settings.Default.Volume <= 1)
            {
                Player.Volume = Settings.Default.Volume;
            }
            else
            {
                Log.Warning("Got invalid volume from settings {volume}", Settings.Default.Volume);
                Player.Volume = 0.75f;
            }
            Player.PlaybackStarting += (o, e) => timer.Start();
            Player.PlaybackStopping += (o, e) => timer.Stop();
        }

        #region Mastery Levels
        private SmartCollection<MasteryItem> _masteryLevels = new SmartCollection<MasteryItem>();
        public SmartCollection<MasteryItem> MasteryLevels { get => _masteryLevels; set => SetField(ref _masteryLevels, value); }
        #endregion

        #region PlayingSong
        private INotifyPropertyChanged? _playingSong;
        public SongItem? PlayingSong { get => (SongItem?) _playingSong; private set => SetField(ref _playingSong, value); }

        private INotifyPropertyChanged? _playingPlaylist;
        public PlaylistItem? PlayingPlaylist { get => (PlaylistItem?)_playingPlaylist; private set => SetField(ref _playingPlaylist, value); }

        private SmartCollection<MasteryItem> _playingMasteryLevels = new SmartCollection<MasteryItem>();
        public SmartCollection<MasteryItem> PlayingMasteryLevels { get => _playingMasteryLevels; private set => SetField(ref _playingMasteryLevels, value); }

        public void StopPlayingSong()
        {
            Player.Stop();
            PlayingSong = null;
            PlayingPlaylist = null;
            PlayingMasteryLevels.Clear();
        }

        //Sets the playing song with the same playing playlist and mastery level
        public void SetPlayingSong(SongItem song, bool startPlaying)
        {
            PlayingSong = song;
            Player.SetSong(song.AudioDirectory, startPlaying);
        }

        public void SetPlayingSong(SongItem song, PlaylistItem playlist, IEnumerable<MasteryItem> masteryLevels, bool startPlaying)
        {
            PlayingPlaylist = playlist;
            PlayingMasteryLevels.Reset(masteryLevels);
            SetPlayingSong(song, startPlaying);
        }
        #endregion

        #region AudioPlayer
        public AudioPlayer Player { get; }

        public event Action PlayerTimerUpdate;
        #endregion

        public StatusContext Status { get; }
    }
}
