using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacman_Projection
{
    internal class IndexPictureBox : PictureBox
    {
        public int Index { get; private set; }
        public IndexPictureBox(int index) 
        {
            Index = index;
        }
    }
}
