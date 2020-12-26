using NAudio.Wave;
using System;
using System.Threading.Tasks;
using SoundTouch;
using SoundTouch.Net.NAudioSupport;

namespace NAudioWrapper
{
    public class AudioPlayer : BaseNotifyPropertyChanged
    {
        private SoundTouchWaveStream? _stream;
        //private SoundTouchProcessor ? _processor;
        //private SoundTouchWaveProvider? _provider;
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

            Stop(true, false);
            
            _output = new WaveOutEvent();
            _output.PlaybackStopped += _output_PlaybackStopped;
            //todo: add try catch and show error window if there is an error. for example, cant find the file
            _audioFileReader = new AudioFileReader(filepath) {Volume = this.Volume}; //TODO: Check other options
            if (newPosition < _audioFileReader.Length)
                _audioFileReader.Position = newPosition;
            _stream = new SoundTouchWaveStream(_audioFileReader);
            //_processor = new SoundTouchProcessor();
            //processor.SetSetting(SettingId.SequenceDurationMs, 10);
            //_provider = new SoundTouchWaveProvider(_audioFileReader, _processor);
            UpdateProviderSpeed();
            _output.Init(_stream);

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

        private bool _useCustomSpeed = false;
        public bool UseCustomSpeed
        {
            get => _useCustomSpeed;
            set { if (SetField(ref _useCustomSpeed, value)) UpdateProviderSpeed(); }
        }

        private double _customSpeed = 1.0;
        public double CustomSpeed
        {
            get => _customSpeed;
            set { if (SetField(ref _customSpeed, value)) UpdateProviderSpeed(); }
        }

        private bool _keepPitch = false;
        public bool KeepPitch
        {
            get => _keepPitch;
            set { if (SetField(ref _keepPitch, value)) UpdateProviderSpeed(); }
        }
        
        private bool _isLooping = false;
        public bool IsLooping
        {
            get => _isLooping;
            set => SetField(ref _isLooping, value);
        }

        private double _loopStart = 0.0;
        public double LoopStart
        {
            get => _loopStart ;
            set => SetField(ref _loopStart , value);
        }

        private double _loopEnd = 1.0;
        public double LoopEnd
        {
            get => _loopEnd;
            set => SetField(ref _loopEnd, value);
        }
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

            bool wasPlaying = IsPlaying;

            if (wasPlaying)
            {
                PlaybackStopping?.Invoke(this, EventArgs.Empty);
                _output.Pause();
            }
            else if (!force && _output.PlaybackState == PlaybackState.Paused)
                Play();

            return wasPlaying;
        }

        public bool Stop(bool disposeOutput = true, bool notifyChanges = true)
        {
            bool wasPlaying = IsPlaying;
            if (_output != null && wasPlaying)
            {
                _stopMeansEnded = false;
                PlaybackStopping?.Invoke(this, EventArgs.Empty);
                _output.Stop();
            }

            if (disposeOutput)
                DisposeOutput(notifyChanges);

            return wasPlaying;
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
        private void UpdateProviderSpeed()
        {
            if (_stream == null)
                return;

            if (UseCustomSpeed)
            {
                if (KeepPitch)
                {
                    _stream.Rate = 1.0;
                    _stream.Tempo = CustomSpeed;
                }
                else
                {
                    _stream.Tempo = 1.0;
                    _stream.Rate = CustomSpeed;
                }
            }
            else
            {
                _stream.Rate = 1.0;
                _stream.Tempo = 1.0;
            }
        }

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

            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }

            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }

            CustomSpeed = 1.0;
            KeepPitch = false;

            if (notifyChanges)
            {
                OnPropertyChanged(nameof(Length));
                OnPropertyChanged(nameof(Position));
            }
        }
        #endregion
    }
}
