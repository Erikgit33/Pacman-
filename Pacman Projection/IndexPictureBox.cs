using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacman_Projection
{
    internal class IndexPictureBox : PictureBox
    {
        public Label label { get; set; }
        public bool Selected { get; set; } = false;
        public int Index { get; private set; }
        public bool Obstructed { get; set; } = false;
        public IndexPictureBox(int index) 
        {
            Index = index;
            this.LocationChanged += UpdateLabelLocation;
            this.Disposed += DisposePeripherals;
        }


        // "Box" somewhat merged with "IndexPictureBox", for use in Form_MapSelectGrid
        public bool isWall { get; set; }
        public bool isGate { get; set; }
        public  bool isTeleporter { get; set; }
        public bool GhostsCanEnter { get; set; } = true;
        public int[] Index_2D { get; private set; } = new int[2];

        public IndexPictureBox(int indexX, int indexY) 
        {
            Index_2D[0] = indexX;
            Index_2D[1] = indexY;
            this.LocationChanged += UpdateLabelLocation;
            this.Disposed += DisposePeripherals;
        }

        private void DisposePeripherals(object sender, EventArgs e)
        {
            if (label != null)
            {
                label.Dispose();
            }
        }

        private void UpdateLabelLocation(object sender, EventArgs e)
        {
            if (label != null)
            {
                label.Location = new Point(this.Location.X + this.Width / 2 - label.Width / 2, this.Location.Y + this.Height + 5);
            }
        }

        internal void UpdateLabelLocation()
        {
            if (label != null)
            {
                label.Location = new Point(this.Location.X + this.Width / 2 - label.Width / 2, this.Location.Y + this.Height + 5);
            }
        }

        internal void UpdateObstructed()
        {
            if (!GhostsCanEnter || isWall || isGate || isTeleporter) 
            {
                Obstructed = true;
            }
        }
    }
}
