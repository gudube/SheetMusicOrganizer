using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel.Tools;
using NAudioWrapper;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using MusicPlayerForDrummers.Model.Items;
using Serilog;

namespace MusicPlayerForDrummers.ViewModel
{
    public class SessionContext : Model.Tools.BaseNotifyPropertyChanged
    {
        public SessionContext()
        {
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
            Player.PlaybackStarting += () => timer.Start();
            Player.PlaybackStopping += () => timer.Stop();
        }

        #region Playlists
        private SmartCollection<BaseModelItem> _playlists = new SmartCollection<BaseModelItem>();
        public SmartCollection<BaseModelItem> Playlists { get => _playlists; set => SetField(ref _playlists, value); }

        private INotifyPropertyChanged? _selectedPlaylist;
        public BaseModelItem? SelectedPlaylist { get => (BaseModelItem?) _selectedPlaylist; set => SetField(ref _selectedPlaylist, value); }
        #endregion

        #region Mastery Levels
        private SmartCollection<MasteryItem> _masteryLevels = new SmartCollection<MasteryItem>();
        public SmartCollection<MasteryItem> MasteryLevels { get => _masteryLevels; set => SetField(ref _masteryLevels, value); }

        private SmartCollection<MasteryItem> _selectedMasteryLevels = new SmartCollection<MasteryItem>();
        public SmartCollection<MasteryItem> SelectedMasteryLevels { get => _selectedMasteryLevels; set => SetField(ref _selectedMasteryLevels, value); }
        #endregion

        #region Songs
        //All songs in selected playlist (no matter the mastery level)
        private SmartCollection<SongItem> _songs = new SmartCollection<SongItem>();
        public SmartCollection<SongItem> Songs { get => _songs; set => SetField(ref _songs, value); }

        private SmartCollection<SongItem> _selectedSongs = new SmartCollection<SongItem>();
        public SmartCollection<SongItem> SelectedSongs { get => _selectedSongs; set => SetField(ref _selectedSongs, value); }
        #endregion

        #region PlayingSong
        private INotifyPropertyChanged? _playingSong;
        public SongItem? PlayingSong { get => (SongItem?)_playingSong; private set => SetField(ref _playingSong, value); }

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

        private void SetPlayingSong(SongItem song)
        {
            PlayingSong = song;
            Player.SetSong(song.AudioDirectory);
            //SelectedSongs.Reset(song); //todo: make the playing song a different color to recognize when it changes
        }

        private void SetPlayingSong(SongItem song, PlaylistItem playlist, SmartCollection<MasteryItem> masteryLevels)
        {
            PlayingPlaylist = playlist;
            PlayingMasteryLevels.Reset(masteryLevels);
            SetPlayingSong(song);
        }

        public void SetSelectedSongPlaying()
        {
            if (!(this.SelectedSongs.Count > 0))
                Log.Warning("Tried to start playing a song without songs selected.");
            else if (!(this.SelectedPlaylist is PlaylistItem pl))
                Log.Warning("Tried to start playing a song without a valid playlist selected, is {playlist}", SelectedPlaylist);
            else
                SetPlayingSong(this.SelectedSongs[0], pl, this.SelectedMasteryLevels);
        }

        public void SetNextPlayingSong(bool next)
        {
            if (PlayingSong == null || PlayingPlaylist == null)
            {
                Log.Warning("Playing song or playing playlist is null when trying to go to play {next} song", (next ? "next" : "previous"));
                return;
            }

            //TODO: add a symbol next to the playing playlist and mastery levels to make it less confusing
            SongItem? newSong;
            if(next)
                newSong = DbHandler.FindNextSong(PlayingSong.Id, PlayingPlaylist.Id, PlayingMasteryLevels.Select(x => x.Id).ToArray());
            else
                newSong = DbHandler.FindPreviousSong(PlayingSong.Id, PlayingPlaylist.Id, PlayingMasteryLevels.Select(x => x.Id).ToArray());
            
            if (newSong == null)
                StopPlayingSong();
            else
                SetPlayingSong(newSong);
        }
        #endregion

        #region AudioPlayer
        public AudioPlayer Player { get; }

        public event Action PlayerTimerUpdate;
        #endregion

        #region Status
        private int _loadingStatus = 0;
        public int LoadingStatus { get => _loadingStatus; set => SetField(ref _loadingStatus, value); }

        private int _savingStatus = 0;
        public int SavingStatus { get => _savingStatus; set => SetField(ref _savingStatus, value); }
        #endregion
    }
}
