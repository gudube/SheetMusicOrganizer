using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel.Tools;
using NaudioWrapper;
using System.ComponentModel;
using System.Linq;

namespace MusicPlayerForDrummers.ViewModel
{
    public class PlayerVM : BaseViewModel
    {
        public override string ViewModelName => "PlayerVM";

        public PlayerVM(SessionContext session) : base(session)
        {
            Volume = 0.75f;

            PlayCommand = new DelegateCommand(Play);
            PauseCommand = new DelegateCommand(Pause);
            StopCommand = new DelegateCommand(Stop);
            NextCommand = new DelegateCommand(Next);
            StartedSeekCommand = new DelegateCommand(StartedSeek);
            StoppedSeekCommand = new DelegateCommand(StoppedSeek);
        }

        private AudioPlayer _audioPlayer;

        #region PropertyChanged
        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.PlayingSong))
            {
                ResetAudioPlayer();
                if (_silentSetPlayingSong)
                    _silentSetPlayingSong = false;
                else
                    _audioPlayer.Play();
            }
        }

        private void ResetAudioPlayer()
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.PlaybackFinished -= PlaybackFinished;
                _audioPlayer.TimerElapsed -= TimerUpdate;
                _audioPlayer.Stop();
                _audioPlayer = null;
            }

            if (Session.PlayingSong != null)
            {
                _audioPlayer = new AudioPlayer(Session.PlayingSong.AudioDirectory, Volume, Position);
                _audioPlayer.PlaybackFinished += PlaybackFinished;
                _audioPlayer.TimerElapsed += TimerUpdate;
            }
            OnPropertyChanged(nameof(Length));
            OnPropertyChanged(nameof(Position));
        }

        private void PlaybackFinished()
        {
            Next(null);
        }

        private void Volume_PropertyChanged()
        {
            if (_audioPlayer != null)
                _audioPlayer.Volume = Volume;
        }

        private void TimerUpdate()
        {
            if (Session.PlayingSong != null)
            {
                OnPropertyChanged("Position");
            }
        }
        #endregion

        #region Controls
        private float _volume;
        public float Volume { get => _volume; set { if (SetField(ref _volume, value)) Volume_PropertyChanged(); } }

        public double Length { get => _audioPlayer == null ? 1 : _audioPlayer.Length; }

        public double Position { get => _audioPlayer == null ? 0 : _audioPlayer.Position; }

        public DelegateCommand PlayCommand { get; }
        //Playing song playing? play from beginning
        //Playing song paused? unpause
        //No Playing Song? Play selected
        //No selected? do nothing
        private void Play(object obj)
        {
            if (Session.PlayingSong != null)
            {
                _audioPlayer.Play();
            }
            else if (Session.SelectedSongs.Count > 0)
            {
                Session.SetSelectedSongPlaying();
            }
        }

        public DelegateCommand PauseCommand { get; }
        //Playing song playing? pause it
        //Playing song paused? play it
        //No playing song? do nothing
        private void Pause(object obj)
        {
            if (Session.PlayingSong == null)
                return;
            
            if (_audioPlayer.PlayerState == NAudio.Wave.PlaybackState.Playing)
            {
                _audioPlayer.Pause();
            }
            else if(_audioPlayer.PlayerState == NAudio.Wave.PlaybackState.Paused)
            {
                _audioPlayer.Play();
            }
        }

        public DelegateCommand StopCommand { get; }
        //Playing song? Stop it and remove playing song
        //No playing song? do nothing
        private void Stop(object obj)
        {
            Session.StopPlayingSong();
        }   

        public DelegateCommand NextCommand { get; }
        //Playing song? Start playing next song
        //No playing song? Play selected song
        //No selected song? do nothing
        private void Next(object obj)
        {
            Session.SetNextPlayingSong();
        }

        private bool _resumePlaying = false;
        public DelegateCommand StartedSeekCommand { get; }
        private void StartedSeek(object obj)
        {
            if(_audioPlayer.PlayerState == NAudio.Wave.PlaybackState.Playing)
            {
                _resumePlaying = true;
                _audioPlayer.Pause();
            }
            else
            {
                _resumePlaying = false;
            }
        }

        public DelegateCommand StoppedSeekCommand { get; }
        private void StoppedSeek(object obj)
        {
            _audioPlayer.Position = (double)obj;

            if (_resumePlaying)
                _audioPlayer.Play();
        }
        #endregion

        #region Tools
        private bool _silentSetPlayingSong = false;
        /// <summary>
        /// Usually, use Session.PlaySelectedSong as it plays the song afterwards.
        /// However, for a few cases, this method sets the playing song without playing it.
        /// </summary>
        public void SilentSetPlayingSong()
        {
            _silentSetPlayingSong = true;
        }
        #endregion
    }
}
