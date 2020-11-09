using System;
using MusicPlayerForDrummers.ViewModel.Tools;
using System.ComponentModel;
using MusicPlayerForDrummers.Model.Items;

namespace MusicPlayerForDrummers.ViewModel
{
    public class PlayerVM : BaseViewModel
    {
        public override string ViewModelName => "PlayerVM";

        public PlayerVM(SessionContext session) : base(session)
        {
            PlayCommand = new DelegateCommand(Play);
            PauseCommand = new DelegateCommand(Pause);
            StopCommand = new DelegateCommand(Stop);
            NextCommand = new DelegateCommand(Next);
            PreviousCommand = new DelegateCommand(Previous);
            StartedSeekCommand = new DelegateCommand(StartedSeek);
            StoppedSeekCommand = new DelegateCommand(StoppedSeek);
            ChangeMuteCommand = new DelegateCommand(ChangeMute);
            Session.Player.PlaybackFinished += PlayNextSong;
            Session.Player.PropertyChanged += Player_PropertyChanged;
            if(UpdateScrollPercentage() && Session.PlayingSong != null) {
                Session.PlayingSong.PropertyChanged += PlayingSong_PropertyChanged;
            }
        }

        private void Player_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //todo: transfom playingsong length from string to int (seconds) and use it here (simpler), also think of displaying it correctly in grid
            if (e.PropertyName == nameof(Session.Player.Length))
            {
                UpdateScrollPercentage();
            }
        }

        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == nameof(Session.PlayingSong)) {
                //if(UpdateScrollPercentage() && Session.PlayingSong != null)
                //    Session.PlayingSong.PropertyChanged += PlayingSong_PropertyChanged;
            //} else 
            //if (e.PropertyName == nameof(Session.Player)) {
            //    if(UpdateScrollPercentage())
            //        Session.Player.PropertyChanged += Player_PropertyChanged;
            //}
        }

        private void PlayingSong_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SongItem.ScrollStartTime) || e.PropertyName == nameof(SongItem.ScrollEndTime))
                UpdateScrollPercentage();
        }

        private bool UpdateScrollPercentage()
        {
            if (Session.PlayingSong == null)
            {
                ScrollStartPercentage = 0;
                ScrollEndPercentage = 1;
                return false;
            }

            ScrollStartPercentage = Session.PlayingSong.ScrollStartTime / Session.Player.Length;
            ScrollEndPercentage = Session.PlayingSong.ScrollEndTime / Session.Player.Length;
            return true;
        }

        #region Events for MainVM
        public event EventHandler<bool>? SetSelectedSongPlaying;
        public event EventHandler? StopPlayingSong;
        public event EventHandler? PlayNextSong;
        public event EventHandler? PlayPreviousSong;
        #endregion

        #region Properties
        private double _scrollStartPercentage;
        public double ScrollStartPercentage
        {
            get => _scrollStartPercentage;
            set => SetField(ref _scrollStartPercentage, value);
        }

        private double _scrollEndPercentage;
        public double ScrollEndPercentage
        {
            get => _scrollEndPercentage;
            set => SetField(ref _scrollEndPercentage, value);
        }
        #endregion

        #region Controls
        public DelegateCommand PlayCommand { get; }
        //Playing song playing? play from beginning
        //Playing song paused? unpause
        //No Playing Song? Play selected
        //No selected? do nothing
        private void Play(object? obj)
        {
            if (Session.PlayingSong != null)
            {
                Session.Player.Play();
            }
            else
            {
                SetSelectedSongPlaying?.Invoke(this, true);
            }
        }

        public DelegateCommand PauseCommand { get; }
        //Playing song playing? pause it
        //Playing song paused? play it
        //No playing song? do nothing
        private void Pause(object? obj)
        {
            Session.Player.Pause();
        }

        public DelegateCommand StopCommand { get; }
        //Playing song? Stop it and remove playing song
        //No playing song? do nothing
        private void Stop(object? obj)
        {
            StopPlayingSong?.Invoke(this, EventArgs.Empty);
        }

        public DelegateCommand PreviousCommand { get; }
        private void Previous(object? obj)
        {
            PlayPreviousSong?.Invoke(this, EventArgs.Empty);
        }

        public DelegateCommand NextCommand { get; }
        private void Next(object? obj)
        {
            PlayNextSong?.Invoke(this, EventArgs.Empty);
        }

        private bool _resumePlaying = false;
        public DelegateCommand StartedSeekCommand { get; }
        private void StartedSeek(object? obj)
        {
            _resumePlaying = Session.Player.Stop(true);
        }

        public DelegateCommand StoppedSeekCommand { get; }
        private void StoppedSeek(object? obj)
        {
            //Session.Player.Position = (double)obj;

            if (_resumePlaying)
                Session.Player.Play();
        }

        public DelegateCommand ChangeMuteCommand { get; }

        private void ChangeMute(object? obj)
        {
            Session.Player.IsAudioMuted = !Session.Player.IsAudioMuted;
        }
        #endregion

        private bool _showAdvancedOptions;
        public bool ShowAdvancedOptions
        {
            get => _showAdvancedOptions;
            set => SetField(ref _showAdvancedOptions, value);
        }
    }
}
