using Pacman_Projection.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacman_Projection
{
    public partial class FormMenu : Form
    {
        public FormMenu()
        {
            InitializeComponent();
        }

        const int boxSize = 14;

        SoundPlayer readyButton_sound = new SoundPlayer(Resources.buttonReady_sound);

        private void FormMenu_Load(object sender, EventArgs e)
        {
            // Set form size to fit projector
            ClientSize = new Size(boxSize * 30, boxSize * 38);
            this.Location = new Point(388, 85);
            this.BackColor = Color.Black;

            // Create the play button
            Button buttonPlay = new Button();
            // playButton properties
            buttonPlay.Location = new Point(boxSize * 12, boxSize * 17);
            buttonPlay.Size = new Size(boxSize * 6, boxSize * 5);
            buttonPlay.Font = new Font("Arial", 22, FontStyle.Bold);
            buttonPlay.Text = "Play";
            buttonPlay.ForeColor = Color.Yellow;
            buttonPlay.Click += playButton_Click;
            Controls.Add(buttonPlay);
            buttonPlay.BringToFront();
        }

        private async void playButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => readyButton_sound.Play());

            new Form1(this).Show();
            this.Hide();
        }
    }
}
