using NAudio.Wave;
using System;
using System.IO;

namespace NAudioWrapper
{
    public class AudioPlayer : BaseNotifyPropertyChanged
    {
        private CustomStream? _stream;
        //private SoundTouchProcessor ? _processor;
        //private SoundTouchWaveProvider? _provider;
        private AudioFileReader? _audioFileReader;
        private WaveOutEvent _output;
        private bool _stopMeansEnded = true;

        public event EventHandler? PlaybackFinished;
        public event EventHandler? PlaybackStarting;
        public event EventHandler? PlaybackStopping;

        public AudioPlayer()
        {
            _output = new WaveOutEvent();
            _output.PlaybackStopped += _output_PlaybackStopped;
        }

        public void SetSong(string filepath, bool startPlaying, bool keepPosition)
        {
            long newPosition = 0;
            if (_audioFileReader != null && keepPosition)
                newPosition = _audioFileReader.Position; // when changing track version

            Stop(true, false); // stop if playing

            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException("Trying to play from an audio file that doesn't exist.", filepath);
            }
            try
            {
                _audioFileReader = new AudioFileReader(filepath) { Volume = this.Volume };
            } catch(Exception ex)
            {
                throw new FileFormatException(new Uri(filepath), ex.Message);
            }
            if (newPosition < _audioFileReader.Length)
                _audioFileReader.Position = newPosition; //set the new position if is valid
            LoopStart = 0; // set the loop times to be the whole song by default
            LoopEnd = _audioFileReader.TotalTime.TotalSeconds;

            _stream = new CustomStream(_audioFileReader, IsLooping, LoopStart, LoopEnd);
            UpdateStreamSpeed();
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
                    if (IsLooping)
                    {
                        if (value < LoopStart)
                            value = LoopStart;
                        else if (value > LoopEnd)
                            value = LoopEnd;
                    }
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
            set { if (SetField(ref _useCustomSpeed, value)) UpdateStreamSpeed(); }
        }

        private double _customSpeed = 1;
        public double CustomSpeed
        {
            get => _customSpeed;
            set {
                double newValue = Math.Round(value, 2, MidpointRounding.ToEven);
                newValue = Math.Max(Math.Min(newValue, 2), 0.5);
                if (SetField(ref _customSpeed, newValue)) UpdateStreamSpeed();
            }
        }

        private bool _keepPitch = false;
        public bool KeepPitch
        {
            get => _keepPitch;
            set { if (SetField(ref _keepPitch, value)) UpdateStreamSpeed(); }
        }
        
        private bool _isLooping = false;
        public bool IsLooping
        {
            get => _isLooping;
            set
            {
                if (SetField(ref _isLooping, value) && _stream != null)
                {
                    _stream.EnableLooping = IsLooping;
                    if (IsLooping && (Position < LoopStart || Position > LoopEnd))
                        Position = LoopStart;
                }
            }
        }

        private double _loopStart = 0;
        public double LoopStart
        {
            get => _loopStart ;
            set
            {
                double newValue = Math.Round(value, 1, MidpointRounding.ToZero);
                newValue = Math.Max(Math.Min(LoopEnd - 2, newValue), 0);
                if (SetField(ref _loopStart, newValue))
                {
                    _stream?.SetLoopStart(_loopStart);
                    if (IsLooping && Position < LoopStart || Position > LoopEnd)
                        Position = LoopStart;
                }
            }
        }

        private double _loopEnd = 1;
        public double LoopEnd
        {
            get => _loopEnd;
            set
            {
                double newValue = Math.Round(value, 1, MidpointRounding.ToPositiveInfinity);
                double maxValue = Math.Round(Length, 1, MidpointRounding.ToZero);
                newValue = Math.Min(Math.Max(LoopStart + 2, newValue), maxValue);
                if (SetField(ref _loopEnd, newValue))
                {
                    _stream?.SetLoopEnd(_loopEnd);
                    if (IsLooping && Position < LoopStart || Position > LoopEnd)
                        Position = LoopStart;
                }
            }
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
                // _stream?.Flush();
            }
            else if (!force && _output.PlaybackState == PlaybackState.Paused)
                Play();

            return wasPlaying;
        }

        public bool Stop(bool disposeOutput = true, bool notifyChanges = true)
        {
            bool wasPlaying = IsPlaying;
            if (_output.PlaybackState != PlaybackState.Stopped)
            {
                PlaybackStopping?.Invoke(this, EventArgs.Empty);
                _stopMeansEnded = false;
                _output.Stop();
                _stream?.Flush();
            }

            if (disposeOutput)
                DisposeOutput(notifyChanges);

            return wasPlaying;
        }

        #endregion

        #region Events
        private void _output_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if(_stopMeansEnded)
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
        private void UpdateStreamSpeed()
        {
            if (_stream == null)
                return;

            if (UseCustomSpeed)
            {
                if (KeepPitch)
                {
                    _stream.Rate = 1;
                    _stream.Tempo = CustomSpeed;
                }
                else
                {
                    _stream.Tempo = 1;
                    _stream.Rate = CustomSpeed;
                }
            }
            else
            {
                _stream.Rate = 1;
                _stream.Tempo = 1;
            }
        }

        private void DisposeOutput(bool notifyChanges = true)
        {
            if (IsPlaying)
            {
                _stopMeansEnded = false;
                PlaybackStopping?.Invoke(this, EventArgs.Empty);
                _output.Stop();
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

            CustomSpeed = 1;
            KeepPitch = false;
            IsLooping = false;
            LoopStart = 0;
            LoopEnd = 1;

            if (notifyChanges)
            {
                OnPropertyChanged(nameof(Length));
                OnPropertyChanged(nameof(Position));
            }
        }
        #endregion
    }
}
