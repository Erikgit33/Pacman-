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
        public int Index { get; private set; }
        public IndexButton(int index)
        {
            Index = index;
        }
    }
}
