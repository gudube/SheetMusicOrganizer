using System;
using MusicPlayerForDrummers.ViewModel.Tools;
using System.ComponentModel;

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
        }

        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        #region Events for MainVM
        public event EventHandler<bool>? SetSelectedSongPlaying;
        public event EventHandler? StopPlayingSong;
        public event EventHandler? PlayNextSong;
        public event EventHandler? PlayPreviousSong;
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

    }
}
