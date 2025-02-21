using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacman_Projection
{
    class Ghost
    {
        internal PictureBox pictureBox;
        internal bool scatter;
        internal bool chase;
        internal bool frightened;

        internal bool direction_up;
        internal bool direction_down;
        internal bool direction_left;
        internal bool direction_right;

        public Ghost(PictureBox pictureBox, bool scatter, bool chase, bool frightened)
        {
            this.pictureBox = pictureBox;
            this.scatter = scatter;
            this.chase = chase;
            this.frightened = frightened;
        }

        void SetScatter()
        {
            scatter = true;
            chase = false;
            frightened = false;
        }

        void SetChase()
        {
            scatter = false;
            chase = true;
            frightened = false;
        }

        void SetFrightened()
        {
            scatter = false;
            chase = false;
            frightened = true;
        }
    }
}
