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
        internal PictureBox box;
        internal bool scatter;
        internal bool chase;
        internal bool scared;
        internal bool white;
        internal bool dead;

        internal int x;
        internal int y;

        internal bool teleportedLastTick;
        internal bool teleporting;
        internal int blocksIntoTeleporter;

        internal bool direction_left;
        internal bool direction_right;
        internal bool direction_up;
        internal bool direction_down;

        internal string cornerDuringScatter; // "TopLeft", "TopRight", "BottomLeft", "BottomRight"

        public Ghost(PictureBox pictureBox)
        {
            box = pictureBox;
        }

        internal void SetDirection(string direction)
        {
            if (direction == "Left")
            {
                direction_left = true;
                direction_right = false;
                direction_up = false;
                direction_down = false;
            }
            else if (direction == "Right")
            {
                direction_left = false;
                direction_right = true;
                direction_up = false;
                direction_down = false;
            }
            else if (direction == "Up")
            {
                direction_left = false;
                direction_right = false;
                direction_up = true;
                direction_down = false;
            }
            else if (direction == "Down")
            {
                direction_left = false;
                direction_right = false;
                direction_up = false;
                direction_down = true;
            }
            else if (direction == "Stationary")
            {
                direction_left = false;
                direction_right = false;
                direction_up = false;
                direction_down = false;
            }
        }
        
        internal void SetTarget(int x, int y)
        {

        }

        internal void SetScatter()
        {
            scatter = true;
            chase = false;
            scared = false;
            if (white)
            {
                white = false;
            }
        }

        internal void SetChase()
        {
            scatter = false;
            chase = true;
            scared = false;
            if (white)
            {
                white = false;
            }
        }

        internal void SetScared()
        {
            scatter = false;
            chase = false;
            scared = true;
        }
    }
}
