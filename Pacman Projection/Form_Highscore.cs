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
    public partial class Form_Highscore : Form
    {
        Form_Menu form_menu;

        public Form_Highscore(Form_Menu form_Menu)
        {
            InitializeComponent();
            this.form_menu = form_Menu;
        }

        const int boxSize = 14;

        private void Form_Highscore_Load(object sender, EventArgs e)
        {
            // Set form size to fit projector
            ClientSize = new Size(boxSize * 30, boxSize * 38);
            this.Location = new Point(388, 85);
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            // Create the highscore label
            Label labelHighscore = new Label();
            labelHighscore.Location = new Point(10, 10);
            labelHighscore.Size = new Size(boxSize*30, boxSize*38);
            labelHighscore.Font = new Font("Arial", 14, FontStyle.Bold);
            labelHighscore.Text = "Highscores:\n\n1. Player1 - 1000\n2. Player2 - 900\n3. Player3 - 800\n4. Player4 - 700\n5. Player5 - 600";
            labelHighscore.ForeColor = Color.White;
            Controls.Add(labelHighscore);
        }

        private void Form_Highscore_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Don't dispose of the form when closing, just hide it
            e.Cancel = true;
            form_menu.SwitchToForm(form_menu, this);
        }
    }
}
