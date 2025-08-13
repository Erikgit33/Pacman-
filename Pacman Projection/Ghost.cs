using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Pacman_Projection
{
    class Ghost
    {
        internal string name; // Name for identification 

        internal PictureBox box;
        internal PictureBox navBox;

        internal bool scatter { get; set; } 
        internal bool chase { get; set; }
        internal bool frightened { get; set; }

        internal bool white { get; set; } 
        internal bool dead { get;  set; }

        internal bool teleportedLastTick { get; set; } // Used to prevent teleporting twice in a row
        internal bool teleporting { get; set; } // Used to indicate that the ghost is currently teleporting
        internal int blocksIntoTeleporter { get; set; }

        internal bool direction_left { get; set; }
        internal bool direction_right { get; set; }
        internal bool direction_up { get; set; }
        internal bool direction_down { get; set; }

        internal int targetPosX;
        internal int targetPosY;

        internal int currentPosX;
        internal int currentPosY;

        internal MapCorner cornerDuringScatter;

        public Ghost(PictureBox box, PictureBox navBox, string name)
        {
            this.box = box;
            this.navBox = navBox;
            this.name = name;
        }

        internal void UpdateLocation(int x, int y)
        {
            currentPosX = x;
            currentPosY = y;

            navBox.Location = new Point(currentPosX + navBox.Width / 2, currentPosY + navBox.Width / 2);

            navBox.BackColor = Color.Green;
        }

        internal void SetDirection(Direction direction)
        {
            if (direction.Equals(Direction.Left))
            {
                direction_left = true;
                direction_right = false;
                direction_up = false;
                direction_down = false;
            }
            else if (direction.Equals(Direction.Right))
            {
                direction_left = false;
                direction_right = true;
                direction_up = false;
                direction_down = false;
            }
            else if (direction.Equals(Direction.Up))
            {
                direction_left = false;
                direction_right = false;
                direction_up = true;
                direction_down = false;
            }
            else if (direction.Equals(Direction.Down))
            {
                direction_left = false;
                direction_right = false;
                direction_up = false;
                direction_down = true;
            }
            else if (direction.Equals(Direction.Stationary))
            {
                direction_left = false;
                direction_right = false;
                direction_up = false;
                direction_down = false;
            }
        }
        
        internal void SetTarget(int x, int y)
        {
            targetPosX = x;
            targetPosY = y;
        }

        internal void SetScatter()
        {
            scatter = true;
            chase = false;
            frightened = false;
            if (white)
            {
                white = false;
            }
        }

        internal void SetChase()
        {
            scatter = false;
            chase = true;
            frightened = false;
            if (white)
            {
                white = false;
            }
        }

        internal void SetFrightened()
        {
            scatter = false;
            chase = false;
            frightened = true;
        }
    }
}
