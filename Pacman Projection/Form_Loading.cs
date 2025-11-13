using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacman_Projection
{
    public partial class Form_Loading : Form
    {
        public Form_Loading()
        {
            InitializeComponent();
        }

        private void startGameTimer_Tick(object sender, EventArgs e)
        {
            var formManager = new FormManager();
            formManager.OpenForm(formManager.form_Menu);
            startGameTimer.Enabled = false;
            this.Hide();
        }
    }
}
