using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacman_Projection
{
    internal class Box
    {
        /// <summary>
        /// The picturebox component of the Box.
        /// </summary>
        internal PictureBox pictureBox;

        /// <summary>
        /// Indicates whether the box is a wall or not.
        /// </summary>
        internal bool isWall { get; set; }
        /// <summary>
        /// Indicates whether the box is a gate or not.
        /// </summary>
        internal bool isGate { get; set; }
        /// <summary>
        /// Indicates whether the box is a teleporter or not.
        /// </summary>
        internal bool isTeleporter { get; set; }

        /// <summary>
        /// Indicates whether ghosts can pass through this box or not. 
        /// Used for spaces in the map where ghosts get stuck.
        /// </summary>
        public bool GhostsCanEnter { get; set; } = true; 

        /// <summary>
        /// Indicates whether the box can hold food or not.
        /// </summary>
        internal bool isFood { get; set; }
        /// <summary>
        /// Indicates whether the box can hold power-pellets ot not.
        /// </summary>
        internal bool isPowerPellet { get; set; }
        /// <summary>
        /// Indicates whether the food in the box is eaten or not.
        /// </summary>
        internal bool isEaten { get; set; }

        public Box(PictureBox pictureBox, bool isWall, bool isTeleporter, bool isGate, bool isFood, bool isPowerPellet)
        {
            this.pictureBox = pictureBox;
            this.isWall = isWall;
            this.isTeleporter = isTeleporter;
            this.isFood = isFood;
            this.isPowerPellet = isPowerPellet;

            if (isWall)
            {
                GhostsCanEnter = false;
            }
        }

        internal void Eaten()
        {
            if (isFood || isPowerPellet)
            {
                isEaten = true;
                pictureBox.Image = null;
            }
        }
    }
}
