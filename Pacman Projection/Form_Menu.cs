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
using System.Xml.Serialization;

namespace Pacman_Projection
{
    public partial class Form_Menu : Form
    {
        public Form_Menu()
        {
            InitializeComponent();
        }

        const int boxSize = 14;

        public Form_Main form_main;
        internal Form_Name form_name;
        internal Form_Highscore form_highscore;

        ComboBox levelToBeginAtDropdown;

        internal SoundManager soundManager = new SoundManager();

        private void FormMenu_Load(object sender, EventArgs e)
        {
            // Set form size to fit projector
            ClientSize = new Size(boxSize * 30, boxSize * 38);
            this.Location = new Point(388, 85);
            this.BackColor = Color.Black;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

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

            // Create the highscore button
            Button buttonHighscore = new Button();
            // playButton properties
            buttonHighscore.Location = new Point(boxSize * 12, boxSize * 12);
            buttonHighscore.Size = new Size(boxSize * 6, boxSize * 5);
            buttonHighscore.Font = new Font("Arial", 22, FontStyle.Bold);
            buttonHighscore.Text = "Highscore";
            buttonHighscore.ForeColor = Color.Yellow;
            buttonHighscore.Click += buttonHighscore_Click;
            Controls.Add(buttonHighscore);
            buttonHighscore.BringToFront();

            // Create the levelToBeginAt dropdown
            levelToBeginAtDropdown = new ComboBox();
            levelToBeginAtDropdown.DisplayMember = Text;
            levelToBeginAtDropdown.MaxDropDownItems = 10;
            levelToBeginAtDropdown.DropDownStyle = ComboBoxStyle.DropDownList;
            levelToBeginAtDropdown.Items.Add(1);
            levelToBeginAtDropdown.Items.Add(2);
            levelToBeginAtDropdown.Items.Add(3);
            levelToBeginAtDropdown.Items.Add(4);
            levelToBeginAtDropdown.Items.Add(5);
            levelToBeginAtDropdown.Items.Add(6);
            levelToBeginAtDropdown.Items.Add(7);
            levelToBeginAtDropdown.Items.Add(8);
            levelToBeginAtDropdown.Items.Add(9);
            levelToBeginAtDropdown.Items.Add(10);
            levelToBeginAtDropdown.SelectedIndex = 0;
            levelToBeginAtDropdown.Size = new Size(50, 50);
            Controls.Add(levelToBeginAtDropdown);
            levelToBeginAtDropdown.BringToFront();

            soundManager.toPlaySounds = true;
            soundManager.PlaySound("menuMusic", true);
        }

        private async void playButton_Click(object sender, EventArgs e)
        {
            if (form_name == null)
            {
                if (levelToBeginAtDropdown.SelectedIndex == 0) // level 1
                {
                    form_name = new Form_Name(this, 1);
                }
                else
                {
                    form_name = new Form_Name(this, levelToBeginAtDropdown.SelectedIndex + 1);
                }
                    SwitchToForm(form_name, this);
            }
            else
            {
                soundManager.PlaySound("buttonReady", false);
                await Task.Delay(250);

                SwitchToForm(form_name, this);
            }
        }

        private async void buttonHighscore_Click(object sender, EventArgs e)
        {
            if (form_highscore == null)
            {
                form_highscore = new Form_Highscore(this);
                SwitchToForm(form_highscore, this);
            }
            else
            {
                soundManager.PlaySound("buttonReady", false);
                await Task.Delay(250);

                SwitchToForm(form_highscore, this);
            } 
        }
        
        public void SwitchToForm(Form formToShow, Form formToClose)
        {
            if (formToClose == this && formToShow == form_main || formToClose == form_name && formToShow == form_main)
            {
                soundManager.StopSound("menuMusic");
            } 
            else if (formToShow == this && formToClose == form_main)
            {
                soundManager.PlaySound("menuMusic", true);
                levelToBeginAtDropdown.SelectedIndex = 0;
            }
            formToClose.Hide();
            formToShow.Show();
        }

        public void SwitchToMenuAndSaveScore(int score, int beganAtLevel, string player_name)
        {
            // SAVE HIGHSCORE WITH LEVEL THE PLAYER BEGAN AT

            soundManager.PlaySound("menuMusic", true);

            form_main.Dispose();
            form_main = null;
            levelToBeginAtDropdown.SelectedIndex = 0;
            this.Show();
        }
    }
}
