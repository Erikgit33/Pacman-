using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Pacman_Projection
{
    class Pacman
    {
        internal PictureBox box;
        internal PictureBox eatBox;

        internal bool teleportedLastTick;
        internal bool teleporting;
        internal int blocksIntoTeleporter;

        internal int currentPosX;
        internal int currentPosY;

        public Pacman(PictureBox box, PictureBox eatBox)
        {
            this.box = box;
            this.eatBox = eatBox;
        }

        public void UpdateLocation(int x, int y)
        {
            currentPosX = x;
            currentPosY = y;

            eatBox.Location = new Point(currentPosX + eatBox.Width / 2, currentPosY + eatBox.Width / 2);
        }
    }
}
