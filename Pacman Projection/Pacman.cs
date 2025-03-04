using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacman_Projection
{
    class Pacman
    {
        internal PictureBox box;

        internal bool teleportedLastTick;
        internal bool teleporting;
        internal int blocksIntoTeleporter;

        public Pacman(PictureBox pictureBox)
        {
            box = pictureBox;
        }
    }
}
