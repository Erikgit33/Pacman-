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
        internal PictureBox pictureBox;

        internal bool isWall { get; set; }
        internal bool isGate { get; set; }
        internal bool isTeleporter { get; set; }

        internal bool isFood { get; set; }
        internal bool isPowerPellet { get; set; }
        internal bool isEaten { get; set; }

        public Box(PictureBox pictureBox, bool isWall, bool isTeleporter, bool isGate, bool isFood, bool isPowerPellet)
        {
            this.pictureBox = pictureBox;
            this.isWall = isWall;
            this.isTeleporter = isTeleporter;
            this.isFood = isFood;
            this.isPowerPellet = isPowerPellet;
        }

        internal void Eaten()
        {
            if (isFood || isPowerPellet)
            {
                isEaten = true;
                pictureBox.Visible = false;
            }
            else
            {
                throw new InvalidOperationException("Cannot eat a box that is not food or a power pellet.");
            }
        }
    }
}
