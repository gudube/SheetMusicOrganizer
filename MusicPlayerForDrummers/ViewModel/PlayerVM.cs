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
            Length = 1;
            Position = 0;

            PlayCommand = new DelegateCommand(Play);
            PauseCommand = new DelegateCommand(Pause);
            StopCommand = new DelegateCommand(Stop);
            NextCommand = new DelegateCommand(Next);
            SeekCommand = new DelegateCommand(Seek);

            _timer = new Timer(100);
            _timer.Elapsed += Timer_Elapsed;
        }

        private AudioPlayer _audioPlayer;

        private Timer _timer;

        private void ResetAudioPlayer()
        {
            _timer.Stop();
            if (_audioPlayer != null)
            {
                _audioPlayer.Stop();
                _audioPlayer = null;
            }

            Position = 0;
            if (Session.PlayingSong == null)
            {
                Length = 1;
            }
            else
            {
                _audioPlayer = new AudioPlayer(Session.PlayingSong.AudioDirectory, Volume, Position);
                Length = _audioPlayer.Length;
                //Play(null);
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //if (_audioPlayer != null && _audioPlayer.PlayerState == NAudio.Wave.PlaybackState.Playing)
            //{
               // dontUpdate = true;
                Position = _audioPlayer.Position;
            //}
        }

        #region PropertyChanged
        protected override void Session_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.PlayingSong))
                ResetAudioPlayer();
        }

        private void Volume_PropertyChanged()
        {
            if (_audioPlayer != null)
                _audioPlayer.Volume = Volume;
        }
        #endregion

        #region Controls
        private float _volume;
        public float Volume { get => _volume; set { if (SetField(ref _volume, value)) Volume_PropertyChanged(); } }

        private double _length;
        public double Length { get => _length; set => SetField(ref _length, value); }

        private double _position;
        public double Position { get => _position; private set => SetField(ref _position, value); }

        public bool IsPlaying { get => _audioPlayer != null && _audioPlayer.PlayerState == NAudio.Wave.PlaybackState.Playing; }

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
            
            _timer.Stop();
            _audioPlayer.Pause();
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

            SongItem nextSong = DBHandler.FindNextSong(Session.PlayingSong.ID, Session.PlayingPlaylist.ID, Session.PlayingMasteryLevels.Select(x => x.ID).ToArray());
            Session.PlayingSong = nextSong;
        }

        public DelegateCommand SeekCommand { get; }
        private bool CanSeek(object obj)
        {
            return _audioPlayer != null && _audioPlayer.PlayerState != NAudio.Wave.PlaybackState.Stopped
                && Session.PlayingSong != null;
        }
        private void Seek(object obj)
        {
            if (!CanSeek(null))
                return;

            _position = (double)obj;
            _audioPlayer.Position = Position;
        }
        #endregion
    }
}
