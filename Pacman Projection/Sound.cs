using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman_Projection
{
    internal class Sound
    {
        internal string SoundName { get; set; }
        internal bool Looping { get; set; }
        internal MemoryStream MemoryStream { get; set; }
        internal WaveFileReader Reader { get; set; }
        internal WaveOutEvent WaveOut { get; set; }
        internal WaveStream WaveStream { get; set; }

        public long PausedPosition = 0; // Position in the sound, used for looping

        public Sound(string soundName, bool looping, MemoryStream memoryStream, WaveFileReader reader, WaveOutEvent waveOut, WaveStream waveStream)
        {
            SoundName = soundName;
            Looping = looping;
            MemoryStream = memoryStream;
            Reader = reader;
            WaveOut = waveOut;
            WaveStream = waveStream;
        }
    }
}
