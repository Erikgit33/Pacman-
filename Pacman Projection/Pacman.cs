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
    class Pacman : Entity
    {
        internal PictureBox eatBox = new PictureBox();

        public Pacman() { }

        public void UpdateLocation(int left, int top)
        {
            currentPosX = left;
            currentPosY = top;

            eatBox.Location = new Point(currentPosX + eatBox.Width / 2, currentPosY + eatBox.Width / 2);
        }
    }
}
