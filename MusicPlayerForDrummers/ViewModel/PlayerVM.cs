using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel.Tools;
using NaudioWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;

namespace MusicPlayerForDrummers.ViewModel
{
    public class PlayerVM : BaseViewModel
    {
        public override string ViewModelName => "PlayerVM";

        public PlayerVM(SessionContext session) : base(session)
        {
            Volume = 0.75f;
            LengthDS = 20;
            PositionDS = 0;

            PlayCommand = new DelegateCommand(Play);
            PauseCommand = new DelegateCommand(Pause);
            StopCommand = new DelegateCommand(Stop);
            NextCommand = new DelegateCommand(Next);

            _timer = new Timer();
            _timer.Interval = 100;
            _timer.Elapsed += Timer_Elapsed;
        }

        private AudioPlayer _audioPlayer;

        private Timer _timer;

        private float _volume;
        public float Volume { get => _volume; set { if (SetField(ref _volume, value)) Volume_PropertyChanged(); } }

        private int _lengthDS;
        public int LengthDS { get => _lengthDS; set => SetField(ref _lengthDS, value); }

        private int _positionDS;
        public int PositionDS { get => _positionDS; set { if (SetField(ref _positionDS, value)) PositionDS_PropertyChanged(); } }


        private void ResetAudioPlayer()
        {
            if (_audioPlayer != null)
                _audioPlayer.Stop();

            PositionDS = 0;

            if (Session.PlayingSong == null)
            {
                _audioPlayer = null;
                LengthDS = 20;
            }
            else
            {
                _audioPlayer = new AudioPlayer(Session.PlayingSong.AudioDirectory, Volume, false);
                _lengthDS = _audioPlayer.LengthDS;
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_audioPlayer != null && _audioPlayer.PlayerState == NAudio.Wave.PlaybackState.Playing)
            {
               // dontUpdate = true;
                PositionDS = _audioPlayer.PositionDS;
            }
        }

        #region PropertyChanged
        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.PlayingSong))
                ResetAudioPlayer();
        }

        private bool dontUpdate = false;

        private void Volume_PropertyChanged()
        {
            if (_audioPlayer != null)
                _audioPlayer.Volume = Volume;
        }

        private void PositionDS_PropertyChanged()
        {
            /*if (dontUpdate)
            {
                dontUpdate = false;
                return;
            }
             
            if(_audioPlayer != null && _audioPlayer.PositionDS != null)
                _audioPlayer.PositionDS = PositionDS;*/
        }
        #endregion

        #region Controls
        public DelegateCommand PlayCommand { get; }
        private bool CanPlay(object obj)
        {
            return Session.PlayingSong != null && Session.PlayingSong.AudioDirectory != null
                && _audioPlayer != null && _audioPlayer.PlayerState != NAudio.Wave.PlaybackState.Playing;
        }
        private void Play(object obj)
        {
            if (!CanPlay(null))
                return;

            _timer.Start();
            _audioPlayer.Play();
        }

        public DelegateCommand PauseCommand { get; }
        private bool CanPause(object obj)
        {
            return _audioPlayer != null && _audioPlayer.PlayerState == NAudio.Wave.PlaybackState.Playing;
        }
        private void Pause(object obj)
        {
            if (!CanPause(null))
                return;

            _audioPlayer.Pause();
            _timer.Stop();
        }

        public DelegateCommand StopCommand { get; }
        private bool CanStop(object obj)
        {
            return _audioPlayer != null;
        }
        private void Stop(object obj)
        {
            if (!CanStop(null))
                return;
            _timer.Stop();
            Session.PlayingSong = null;
        }   

        public DelegateCommand NextCommand { get; }
        private bool CanNext(object obj)
        {
            return _audioPlayer != null && _audioPlayer.PlayerState != NAudio.Wave.PlaybackState.Stopped
                && Session.PlayingSong != null;
        }
        private void Next(object obj)
        {
            if (!CanNext(null))
                return;

            _audioPlayer.Stop();
            SongItem nextSong = DBHandler.FindNextSong(Session.PlayingSong.ID, Session.PlayingPlaylist.ID, Session.PlayingMasteryLevels.Select(x => x.ID).ToArray());
            Session.PlayingSong = nextSong;
        }
        #endregion
    }
}
