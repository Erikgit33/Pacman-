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
        Stream soundResource;

        public Sound(Stream soundResource) 
        {
            this.soundResource = soundResource;

            
        }
    }
}
