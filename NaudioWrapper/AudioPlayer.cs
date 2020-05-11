using NAudio.Wave;
using System;

namespace NaudioWrapper
{
    public class AudioPlayer
    {
        private AudioFileReader _audioFileReader;
        private DirectSoundOut _output;
        public PlaybackState PlayerState { get => _output.PlaybackState; }

        public AudioPlayer(string filepath, float volume, bool play = false)
        {
            _audioFileReader = new AudioFileReader(filepath) { Volume = volume }; //TODO: Check other options
            _output = new DirectSoundOut(200); //TODO: 200?

            var wc = new WaveChannel32(_audioFileReader) { PadWithZeroes = false };
            wc.PadWithZeroes = false;

            _output.Init(wc);
            
            if (play)
                Play();
        }

        #region Play Controls
        public void Play()
        {
            if (_output != null && _output.PlaybackState == PlaybackState.Playing)
                PositionDS = 0;
            else
                _output.Play();
        }

        public void Pause()
        {
            if(_output != null)
                _output.Pause();
        }

        /// <summary>
        /// Stops the AudioPlayer entirely. Should not use the same AudioPlayer instance after.
        /// </summary>
        public void Stop()
        {
            if(_output != null)
                _output.Stop();
            Dispose();
        }

        public int PositionDS { get => Convert.ToInt32(_audioFileReader.CurrentTime.TotalMilliseconds)/100; set => _audioFileReader.CurrentTime = TimeSpan.FromMilliseconds(value*100); }
        
        public float Volume { get => _audioFileReader.Volume; set => _audioFileReader.Volume = value; }

        public int LengthDS { get => Convert.ToInt32(_audioFileReader.TotalTime.TotalMilliseconds)/100; }
        #endregion

        #region Tools
        private void Dispose()
        {
            if(_output != null)
            {
                if (_output.PlaybackState == PlaybackState.Playing)
                    _output.Stop();
                _output.Dispose();
                _output = null;
            }
            if(_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }
        }
        #endregion
    }
}
