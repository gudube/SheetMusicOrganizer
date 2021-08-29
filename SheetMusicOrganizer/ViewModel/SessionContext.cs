using NAudioWrapper;
using System;
using System.ComponentModel;
using System.Windows.Threading;
using Serilog;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.ViewModel.Tools;

namespace SheetMusicOrganizer.ViewModel
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
                Log.Warning("Got invalid volume from settings: {volume}", Settings.Default.Volume);
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
        public SongItem? PlayingSong { get => (SongItem?) _playingSong; set => SetField(ref _playingSong, value); }
        #endregion

        #region AudioPlayer
        public AudioPlayer Player { get; }

        public event Action PlayerTimerUpdate;
        #endregion
    }
}
