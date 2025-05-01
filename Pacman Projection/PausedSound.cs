using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman_Projection
{
    internal class PausedSound
    {
        internal WaveFileReader reader = new WaveFileReader(Stream.Null);
        internal WaveOutEvent waveOut = new WaveOutEvent();
        internal bool loop;

        public PausedSound(WaveFileReader reader, WaveOutEvent waveOut, bool loop)
        {
            this.reader = reader;
            this.waveOut = waveOut;
            this.loop = loop;
        }
    }
}
