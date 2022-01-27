using System;
using System.ComponentModel;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.ViewModel.Tools;
using System.Collections.Generic;

namespace SheetMusicOrganizer.ViewModel
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
            PlayRandomCommand = new DelegateCommand(PlayRandom);
            StartedSeekCommand = new DelegateCommand(StartedSeek);
            StoppedSeekCommand = new DelegateCommand(StoppedSeek);
            ChangeMuteCommand = new DelegateCommand(ChangeMute);
            ChangeAudioCommand = new DelegateCommand(ChangeAudio);
            Session.Player.PlaybackFinished += Player_PlaybackFinished;
            Session.Player.PropertyChanged += Player_PropertyChanged;
            if(UpdateScrollPercentage() && Session.PlayingSong != null) {
                Session.PlayingSong.PropertyChanged += PlayingSong_PropertyChanged;
            }
        }

        private void Player_PlaybackFinished(object? sender, EventArgs e)
        {
            switch(PlayOrder)
            {
                case PlayOrderType.Default:
                    PlayNextSong?.Invoke(this, EventArgs.Empty);
                    break;
                case PlayOrderType.Random:
                    PlayRandomSong?.Invoke(this, EventArgs.Empty);
                    break;
                case PlayOrderType.Repeat:
                    PlaySameSong?.Invoke(this, EventArgs.Empty);
                    break;
                case PlayOrderType.Stop:
                    Session.Player.Position = 0;
                    break;
            }
        }

        private void Player_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.Player.Length))
            {
                UpdateScrollPercentage();
            }
        }

        protected override void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.PlayingSong))
                PlayingSecondaryAudio = false;
        }

        private void PlayingSong_PropertyChanged(object? sender, PropertyChangedEventArgs e)
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
        public event EventHandler? PlayRandomSong;
        public event EventHandler? PlaySameSong;
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

        private bool _playingSecondaryAudio = false;
        public bool PlayingSecondaryAudio
        {
            get => _playingSecondaryAudio;
            set => SetField(ref _playingSecondaryAudio, value);
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
            if(PlayOrder == PlayOrderType.Random)
            {
                PlayRandomSong?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                PlayPreviousSong?.Invoke(this, EventArgs.Empty);
            }
        }

        public DelegateCommand NextCommand { get; }
        private void Next(object? obj)
        {
            if (PlayOrder == PlayOrderType.Random)
            {
                PlayRandomSong?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                PlayNextSong?.Invoke(this, EventArgs.Empty);
            }
        }

        public DelegateCommand PlayRandomCommand { get; }
        private void PlayRandom(object? obj)
        {
           PlayRandomSong?.Invoke(this, EventArgs.Empty);
        }

        private bool _resumePlaying = false;
        public DelegateCommand StartedSeekCommand { get; }
        private void StartedSeek(object? obj)
        {
            _resumePlaying = Session.Player.Stop(false);
        }

        public DelegateCommand StoppedSeekCommand { get; }
        private void StoppedSeek(object? obj)
        {
            if (_resumePlaying)
                Session.Player.Play();
        }

        public DelegateCommand ChangeMuteCommand { get; }
        private void ChangeMute(object? obj)
        {
            Session.Player.IsAudioMuted = !Session.Player.IsAudioMuted;
        }

        public DelegateCommand ChangeAudioCommand { get; }
        private void ChangeAudio(object? obj)
        {
            if (Session.PlayingSong != null && !string.IsNullOrWhiteSpace(Session.PlayingSong.AudioDirectory2))
            {
                PlayingSecondaryAudio = !PlayingSecondaryAudio;
                string audioDir = PlayingSecondaryAudio ? Session.PlayingSong.AudioDirectory2 : Session.PlayingSong.AudioDirectory1;
                Session.Player.SetSong(audioDir, Session.Player.IsPlaying, true);
            }
        }
        #endregion

        #region Playing Order
        public enum PlayOrderType
        {
            Default,
            Repeat,
            Random,
            Stop
        }

        public Dictionary<PlayOrderType, string> PlayOrderDict { get; } =
            new Dictionary<PlayOrderType, string>() {
                {PlayOrderType.Default, "Default"},
                {PlayOrderType.Repeat, "Repeat Song"},
                {PlayOrderType.Random, "Random Song"},
                {PlayOrderType.Stop, "Stop"},
            };

        private PlayOrderType _playOrder;
        public PlayOrderType PlayOrder
        {
            get { return _playOrder; }
            set { SetField(ref _playOrder, value); }
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
