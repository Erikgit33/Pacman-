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
    class Ghost : Entity
    {
        internal PictureBox navBox = new PictureBox();

        internal bool scatter { get; set; } 
        internal bool chase { get; set; }
        internal bool frightened { get; set; }

        internal bool white { get; set; }
        internal bool teleportedLastTick { get; set; } // Used to prevent teleporting twice in a row
        internal int blocksIntoTeleporter { get; set; }

        internal int targetPosX { get; set; }
        internal int targetPosY { get; set; }

        internal int currentPosX { get; set; }
        internal int currentPosY { get; set; }

        internal MapCorner cornerDuringScatter { get; set; }

        public Ghost(GhostName name) 
        {
            switch (name)
            {
                case GhostName.Blinky:
                    cornerDuringScatter = MapCorner.TopRight;
                    break;
                case GhostName.Pinky:
                    cornerDuringScatter = MapCorner.TopLeft;
                    break;
                case GhostName.Inky:
                    cornerDuringScatter = MapCorner.BottomRight;
                    break;
                case GhostName.Clyde:
                    cornerDuringScatter = MapCorner.BottomLeft;
                    break;
                default:
                    cornerDuringScatter = MapCorner.None;
                    break;
            }
        }

        internal void UpdateLocation(int left, int top)
        {
            currentPosX = left;
            currentPosY = top;

            navBox.Location = new Point(currentPosX + navBox.Width / 2, currentPosY + navBox.Width / 2);
        }
        
        internal void SetTarget(int left, int top)
        {
            targetPosX = left;
            targetPosY = top;
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
