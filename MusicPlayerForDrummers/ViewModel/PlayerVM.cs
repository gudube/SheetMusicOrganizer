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
            StartedSeekCommand = new DelegateCommand(StartedSeek);
            StoppedSeekCommand = new DelegateCommand(StoppedSeek);
        }

        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        #region Controls
        public DelegateCommand PlayCommand { get; }
        //Playing song playing? play from beginning
        //Playing song paused? unpause
        //No Playing Song? Play selected
        //No selected? do nothing
        private void Play(object obj)
        {
            if (Session.PlayingSong != null)
            {
                Session.Player.Play();
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
            Session.Player.Pause();
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
            _resumePlaying = Session.Player.Stop(true);
        }

        public DelegateCommand StoppedSeekCommand { get; }
        private void StoppedSeek(object obj)
        {
            //Session.Player.Position = (double)obj;

            if (_resumePlaying)
                Session.Player.Play();
        }
        #endregion
    }
}
