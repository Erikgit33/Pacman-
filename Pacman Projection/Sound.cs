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
        internal string soundName;
        internal bool looping;
        internal MemoryStream memoryStream;
        internal WaveFileReader reader;
        internal WaveOutEvent waveOut;
        internal WaveStream waveStream;

        internal long pausedPosition = 0; // Position in the sound, used for looping

        public Sound(string soundName, bool looping, MemoryStream memoryStream, WaveFileReader reader, WaveOutEvent waveOut, WaveStream waveStream)
        {
            this.soundName = soundName;
            this.looping = looping;
            this.memoryStream = memoryStream;
            this.reader = reader;
            this.waveOut = waveOut;
            this.waveStream = waveStream;
        }
    }
}
