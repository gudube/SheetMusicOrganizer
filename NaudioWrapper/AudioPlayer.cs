using NAudio.Wave;
using System;
using System.Threading.Tasks;
using SoundTouch;
using SoundTouch.Net.NAudioSupport;

namespace NAudioWrapper
{
    public class AudioPlayer : BaseNotifyPropertyChanged
    {
        private AudioFileReader? _audioFileReader;
        private WaveOutEvent? _output;
        private bool _stopMeansEnded = true;

        public event EventHandler? PlaybackFinished;
        public event EventHandler? PlaybackStarting;
        public event EventHandler? PlaybackStopping;

        public AudioPlayer()
        {
        }

        public void SetSong(string filepath, bool startPlaying, bool keepPosition)
        {
            //todo: crash when switching version fast, bc of changing song quickly?
            long newPosition = 0;
            if (_audioFileReader != null && keepPosition)
            {
                newPosition = _audioFileReader.Position;
            }

            Stop(false);
            
            _output = new WaveOutEvent();
            _output.PlaybackStopped += _output_PlaybackStopped;
            //todo: add try catch and show error window if there is an error. for example, cant find the file
            _audioFileReader = new AudioFileReader(filepath) {Volume = this.Volume}; //TODO: Check other options
            if (newPosition < _audioFileReader.Length)
                _audioFileReader.Position = newPosition;
            SoundTouchProcessor processor = new SoundTouchProcessor(); //change any option here
            //processor.SetSetting(SettingId.SequenceDurationMs, 10);
            SoundTouchWaveProvider provider = new SoundTouchWaveProvider(_audioFileReader, processor);
            _output.Init(provider);

            OnPropertyChanged(nameof(Position));
            OnPropertyChanged(nameof(Length));

            if (startPlaying)
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

        public double Length => _audioFileReader?.TotalTime.TotalSeconds ?? 1;

        public bool IsPlaying => _output != null && _output.PlaybackState == PlaybackState.Playing;

        #endregion

        #region Play Controls
        public void Play()
        {
            if (_audioFileReader == null || _output == null)
                return;

            if (IsPlaying)
                Position = 0;
            else
            {
                _output.Play();
                PlaybackStarting?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool Pause(bool force = false)
        {
            if (_audioFileReader == null || _output == null)
                return false;

            bool isPlaying = IsPlaying;

            if (isPlaying)
            {
                PlaybackStopping?.Invoke(this, EventArgs.Empty);
                _output.Pause();
            }
            else if (!force && _output.PlaybackState == PlaybackState.Paused)
                Play();

            return isPlaying;
        }

        public void Stop(bool notifyChanges = true)
        {
            if (_output != null && IsPlaying)
            {
                _stopMeansEnded = false;
                PlaybackStopping?.Invoke(this, EventArgs.Empty);
                _output.Stop();
            }
            DisposeOutput(notifyChanges);
        }

        #endregion

        #region Events
        private void _output_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            //TODO: Find a way to make it work
            if (_stopMeansEnded)
            {
                _stopMeansEnded = false;
                PlaybackFinished?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _stopMeansEnded = true;
            }
        }
        #endregion

        #region Tools
        private void DisposeOutput(bool notifyChanges = true)
        {
            if (_output != null)
            {
                if (IsPlaying)
                {
                    _stopMeansEnded = false;
                    PlaybackStopping?.Invoke(this, EventArgs.Empty);
                    _output.Stop();
                }
                _output.Dispose();
                _output = null;
            }

            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }

            if (notifyChanges)
            {
                OnPropertyChanged(nameof(Length));
                OnPropertyChanged(nameof(Position));
            }
        }
        #endregion
    }
}
