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
        internal bool isWall;
        internal bool isTeleporter;
        internal bool toContainFood;
        internal bool isBigFood;
        public Box(PictureBox pictureBox, bool isWall, bool isTeleporter, bool toContainFood, bool isBigFood)
        {
            this.pictureBox = pictureBox;
            this.isWall = isWall;
            this.isTeleporter = isTeleporter;
            this.toContainFood = toContainFood;
            this.isBigFood = isBigFood;
        }
    }
}
