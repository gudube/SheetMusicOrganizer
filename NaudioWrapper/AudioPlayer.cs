using NAudio.Wave;
using System;
using System.Timers;

namespace NaudioWrapper
{
    public class AudioPlayer
    {
        private AudioFileReader _audioFileReader;
        private DirectSoundOut _output;
        private WaveChannel32 _waveChannel;
        private bool _stopMeansEnded = true;
        private Timer _timer;

        public PlaybackState PlayerState { get => _output.PlaybackState; }

        public event Action PlaybackFinished;
        public event Action TimerElapsed;

        public AudioPlayer(string filepath, float volume, double position = 0)
        {
            _audioFileReader = new AudioFileReader(filepath) { Volume = volume }; //TODO: Check other options
            _waveChannel = new WaveChannel32(_audioFileReader) { PadWithZeroes = false };

            _output = new DirectSoundOut(200);
            _output.Init(_waveChannel);
            Position = position;

            _output.PlaybackStopped += _output_PlaybackStopped;

            _timer = new Timer(500);
            _timer.Elapsed += _timer_Elapsed;
        }


        #region Play Controls
        public void Play()
        {
            if (_output.PlaybackState == PlaybackState.Playing)
                Position = 0;
            else
            {
                _output.Play();
                _timer.Start();
            }
        }

        public void Pause()
        {
            _timer.Stop();
            _output.Pause();
        }

        /// <summary>
        /// Stops the AudioPlayer entirely. Should not use the same AudioPlayer instance after.
        /// </summary>
        public void Stop()
        {
            if(_output.PlaybackState != PlaybackState.Stopped)
            {
                _stopMeansEnded = false;
                _timer.Stop();
                _output.Stop();
            }
            //Dispose();
        }

        public double Position { get => _audioFileReader.CurrentTime.TotalSeconds; set => _audioFileReader.CurrentTime = TimeSpan.FromSeconds(value); }
        
        public float Volume { get => _audioFileReader.Volume; set => _audioFileReader.Volume = value; }

        public double Length { get => _audioFileReader.TotalTime.TotalSeconds; }
        #endregion

        #region Events
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimerElapsed?.Invoke();
        }

        private void _output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            //TODO: Find a way to make it work
            Dispose();
            if (_stopMeansEnded)
            {
                _stopMeansEnded = false;
                PlaybackFinished?.Invoke();
            }
        }
        #endregion

        #region Tools
        private void Dispose()
        {
            if(_timer != null)
            {
                _timer.Elapsed -= _timer_Elapsed;
                _timer.Dispose();
            }
            if (_output != null)
            {
                if (_output.PlaybackState == PlaybackState.Playing)
                    _output.Stop();
                _output.PlaybackStopped -= _output_PlaybackStopped;
                _output.Dispose();
                _output = null;
            }
            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }
        }
        #endregion
    }
}
