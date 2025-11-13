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
        /// <summary>
        /// The box used to detect when Pacman eats a pellet or power pellet.
        /// Also used for ghost navigation and targeting.
        /// </summary>
        internal PictureBox eatBox = new PictureBox();

        public Pacman() 
        {
            box.LocationChanged += UpdateLocation;
        }

        public void UpdateLocation(object sender, EventArgs e)
        {
            eatBox.Location = new Point(box.Left + eatBox.Width / 2, box.Top + eatBox.Width / 2);

            CurrentPosX = eatBox.Left / GameConstants.BoxSize;
            CurrentPosY = (eatBox.Top - GameConstants.GameGridOffset_Vertical) / GameConstants.BoxSize;

            CurrentPos = new int[] { CurrentPosX, CurrentPosY };
        }

        public void UpdateLocation()
        {
            eatBox.Location = new Point(box.Left + eatBox.Width / 2, box.Top + eatBox.Width / 2);

            CurrentPosX = eatBox.Left / GameConstants.BoxSize;
            CurrentPosY = (eatBox.Top - GameConstants.GameGridOffset_Vertical) / GameConstants.BoxSize;

            CurrentPos = new int[] { CurrentPosX, CurrentPosY };
        }
    }
}
