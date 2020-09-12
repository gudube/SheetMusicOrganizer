using System;
using MusicPlayerForDrummers.ViewModel.Tools;
using System.ComponentModel;
using System.Linq;
using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.Model.Items;
using Serilog;

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
            Session.Player.PlaybackFinished += (o, e) => SetNextPlayingSong(true);
        }

        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        public event EventHandler<bool>? SetSelectedSongPlaying;

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
            Session.StopPlayingSong();
        }

        public DelegateCommand PreviousCommand { get; }
        private void Previous(object? obj)
        {
            SetNextPlayingSong(false);
        }

        public DelegateCommand NextCommand { get; }
        private void Next(object? obj)
        {
            SetNextPlayingSong(true);
        }

        public void SetNextPlayingSong(bool next)
        {
            //TODO: add a symbol next to the playing playlist and mastery levels to make it less confusing
            if (Session.PlayingSong == null)
            {
                SetSelectedSongPlaying?.Invoke(this, true);
                return;
            }
            
            SongItem? newSong;

            if (Session.PlayingPlaylist == null)
            {
                Log.Warning("Playing playlist is null when trying to go to play {next} song", (next ? "next" : "previous"));
                return;
            }

            if(next)
                newSong = DbHandler.FindNextSong(Session.PlayingSong.Id, Session.PlayingPlaylist.Id, Session.PlayingMasteryLevels.Select(x => x.Id).ToArray());
            else
                newSong = DbHandler.FindPreviousSong(Session.PlayingSong.Id, Session.PlayingPlaylist.Id, Session.PlayingMasteryLevels.Select(x => x.Id).ToArray());
            
            if (newSong == null)
                Session.StopPlayingSong();
            else
                Session.SetPlayingSong(newSong, true);
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
