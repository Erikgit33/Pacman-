using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacman_Projection
{
    class Ghost
    {
        internal PictureBox pictureBox;

        public Ghost(PictureBox pictureBox)
        {
            this.pictureBox = pictureBox;
        }
    }
}
