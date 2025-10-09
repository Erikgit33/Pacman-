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

        internal bool Scatter { get; set; } 
        internal bool Chase { get; set; }
        internal bool Frightened { get; set; }

        internal bool White { get; set; }
        internal bool TeleportedLastTick { get; set; } // Used to prevent teleporting twice in a row
        internal int BlocksIntoTeleporter { get; set; }

        /// <summary>
        /// The target's X index.
        /// </summary>
        internal int TargetPosX { get; set; }
        /// <summary>
        /// The target's Y index.
        /// </summary>
        internal int TargetPosY { get; set; }
        /// <summary>
        /// The ghost's target position as [x, y] indexes.
        /// </summary>
        internal int[] TargetPos { get; set; }
        /// <summary>
        /// The ghost's current X index.
        /// </summary>
        internal int CurrentPosX { get; private set; }
        /// <summary>
        /// The ghost's current Y index.
        /// </summary>
        internal int CurrentPosY { get; private set; }
        /// <summary>
        /// The ghosts's current position as [x, y] indexes.    
        /// </summary>
        internal int[] CurrentPos { get;  set; }

        internal MapCorner ScatterCorner { get; private set; }

        public Ghost(GhostName name) 
        {
            switch (name)
            {
                case GhostName.Blinky:
                    ScatterCorner = MapCorner.TopRight;
                    break;
                case GhostName.Pinky:
                    ScatterCorner = MapCorner.TopLeft;
                    break;
                case GhostName.Inky:
                    ScatterCorner = MapCorner.BottomRight;
                    break;
                case GhostName.Clyde:
                    ScatterCorner = MapCorner.BottomLeft;
                    break;
                default:
                    ScatterCorner = MapCorner.None;
                    break;
            }
        }

        internal void UpdateLocation(int left, int top)
        {
            navBox.Location = new Point(left + navBox.Width / 2, top + navBox.Width / 2);

            CurrentPosX = left / GameConstants.boxSize;
            CurrentPosY = top / GameConstants.boxSize;

            CurrentPos = new int[] { CurrentPosX, CurrentPosY };
        }

        internal void SetTarget(int[] targetIndex)
        {
            TargetPos = targetIndex;

            TargetPosX = targetIndex[0];
            TargetPosY = targetIndex[1];
        }

        internal void SetTarget(MapCorner corner)
        {
            int[] targetIndex = new int[2];
            switch (corner)
            {
                case MapCorner.BottomLeft:
                    targetIndex[0] = 0;
                    targetIndex[1] = 0;
                    break;
                case MapCorner.TopLeft:
                    targetIndex[0] = 0;
                    targetIndex[1] = 0;
                    break;
                case MapCorner.TopRight:
                    targetIndex[0] = 0;
                    targetIndex[1] = 0;
                    break;
                case MapCorner.BottomRight:
                    targetIndex[0] = 0;
                    targetIndex[1] = 0;
                    break;
            }
            TargetPos = targetIndex;

            TargetPosX = targetIndex[0];
            TargetPosY = targetIndex[1];
        }

        internal void SetScatter()
        {
            Scatter = true;
            Chase = false;
            Frightened = false;
            if (White)
            {
                White = false;
            }
        }

        internal void SetChase()
        {
            Scatter = false;
            Chase = true;
            Frightened = false;
            if (White)
            {
                White = false;
            }
        }

        internal void SetFrightened()
        {
            Scatter = false;
            Chase = false;
            Frightened = true;
        }
    }
}
