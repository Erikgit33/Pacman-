using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacman_Projection
{
    internal class IndexButton : Button
    {
        public int index { get; private set; }
        public IndexButton(int index)
        {
            this.index = index;
        }
    }
}
