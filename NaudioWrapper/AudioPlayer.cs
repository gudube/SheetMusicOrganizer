using NAudio.Wave;
using System;

namespace NAudioWrapper
{
    public class AudioPlayer : BaseNotifyPropertyChanged
    {
        private AudioFileReader _audioFileReader;
        //private DirectSoundOut _output;
        private WaveOutEvent _output;
        //private WaveChannel32 _waveChannel;
        private bool _stopMeansEnded = true;

        public event Action PlaybackFinished;
        public event Action PlaybackStarting;
        public event Action PlaybackStopping;

        public void SetSong(string filepath)
        {
            //bool resumePlaying = false;
            if (_output != null){
                //    resumePlaying = _output.PlaybackState == PlaybackState.Playing;
                Stop();
            }

            _output = new WaveOutEvent();
            _output.PlaybackStopped += _output_PlaybackStopped;
            _audioFileReader = new AudioFileReader(filepath) { Volume = this.Volume }; //TODO: Check other options
            //_waveChannel = new WaveChannel32(_audioFileReader) { PadWithZeroes = false };
            //_output = new DirectSoundOut(200);
            //_output.Init(_waveChannel);
            _output.Init(_audioFileReader);

            OnPropertyChanged(nameof(Position));
            OnPropertyChanged(nameof(Length));

            //if (resumePlaying)
                Play();
        }

        #region Properties
        public double Position
        {
            get => _audioFileReader?.CurrentTime.TotalSeconds ?? 0;
            set
            {
                if (_audioFileReader != null)
                {
                    _audioFileReader.CurrentTime = TimeSpan.FromSeconds(value);
                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        private float _volume = 0.5f;
        public float Volume {
            get { if (IsAudioMuted) return 0; return _volume; }
            set { if (SetField(ref _volume, value) && _audioFileReader != null && !IsAudioMuted) _audioFileReader.Volume = value; }
        }

        private bool _isAudioMuted = false;
        public bool IsAudioMuted
        {
            get => _isAudioMuted;
            set
            {
                if (SetField(ref _isAudioMuted, value))
                {
                    OnPropertyChanged(nameof(Volume));
                    
                    if(_audioFileReader != null)
                        _audioFileReader.Volume = Volume;
                }
            }
        }

        public double Length { get => _audioFileReader?.TotalTime.TotalSeconds ?? 1; }
        #endregion

        #region Play Controls
        public void Play()
        {
            if (_output == null)
                return;

            if (_output.PlaybackState == PlaybackState.Playing)
                Position = 0;
            else
            {
                _output.Play();
                PlaybackStarting?.Invoke();
            }
        }

        public bool Pause(bool force = false)
        {
            if (_output == null)
                return false;

            bool isPlaying = _output.PlaybackState == PlaybackState.Playing;

            if (force || isPlaying)
            {
                PlaybackStopping?.Invoke();
                _output.Pause();
            }
            else if (_output.PlaybackState == PlaybackState.Paused)
                Play();

            return isPlaying;
        }

        //Soft clears the buffer but doesn't dispose anything. Usefull when changing position in song.
        public bool Stop(bool soft = false)
        {
            if (_output == null)
                return false;

            bool isPlaying = _output.PlaybackState == PlaybackState.Playing;

            if (_output.PlaybackState != PlaybackState.Stopped)
            {
                _stopMeansEnded = false;
                PlaybackStopping?.Invoke();
                _output.Stop();
            }

            if (!soft)
                DisposeOutput();

            return isPlaying;
        }

        #endregion

        #region Events
        private void _output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            //TODO: Find a way to make it work
            if (_stopMeansEnded)
            {
                _stopMeansEnded = false;
                PlaybackFinished?.Invoke();
            }
        }
        #endregion

        #region Tools
        private void DisposeOutput()
        {
            if (_output != null && _output.PlaybackState == PlaybackState.Playing)
            {
                PlaybackStopping?.Invoke();
                _output.Stop();
            }

            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }

            if (_output != null)
            {
                _output.Dispose();
                _output = null;
            }
            OnPropertyChanged(nameof(Length));
            OnPropertyChanged(nameof(Position));
        }
        #endregion
    }
}
