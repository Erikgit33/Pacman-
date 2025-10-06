using Pacman_Projection.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Pacman_Projection
{
    public partial class Form_Menu : Form
    {
        SoundManager soundManager = new SoundManager();

        FormManager formManager;
        EventManager eventManager;
        GlobalVariables globalVariables; 

        const int boxSize = GameConstants.boxSize;

        Button buttonPlay;
        Button buttonHighscore;
        TextBox nameBox;
        ComboBox levelToBeginAtDropdown;

        public Form_Menu(FormManager formManager, EventManager eventManager, GlobalVariables globalVariables)
        {
            InitializeComponent();
            this.formManager = formManager;
            this.eventManager = eventManager;
            this.globalVariables = globalVariables;
        }

        private void FormMenu_Load(object sender, EventArgs e)
        {
            // Set form size to fit projector
            ClientSize = new Size(boxSize * 30, boxSize * 38);
            this.Location = new Point(388, 85);
            this.BackColor = Color.Black;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // playButton properties
            buttonPlay = new Button
            {
                Location = new Point(boxSize * 12, boxSize * 17),
                Size = new Size(boxSize * 6, boxSize * 5),
                Font = new Font("Arial", 22, FontStyle.Bold),
                Text = "Play",
                ForeColor = Color.Yellow,
            };
            buttonPlay.Click += playButton_Click;
            Controls.Add(buttonPlay);
            buttonPlay.BringToFront();

            // playButton properties
            buttonHighscore = new Button
            {
                Location = new Point(boxSize * 12, boxSize * 12),
                Size = new Size(boxSize * 6, boxSize * 5),
                Font = new Font("Arial", 22, FontStyle.Bold),
                Text = "Highscore",
                ForeColor = Color.Yellow
            };
            buttonHighscore.Click += buttonHighscore_Click;
            Controls.Add(buttonHighscore);
            buttonHighscore.BringToFront();

            // nameBox properties 
            nameBox = new TextBox
            {
                Location = new Point(boxSize * 10, boxSize * 7),
                Size = new Size(boxSize * 10, boxSize * 3),
                Font = new Font("Arial", 22, FontStyle.Bold),
                Text = "Enter Name",
                ForeColor = Color.Gray,
                TextAlign = HorizontalAlignment.Center
            };
            Controls.Add(nameBox);

            nameBox.GotFocus += (s, E) =>
            {
                if (nameBox.Text == "Enter Name")
                {
                    nameBox.Text = "";
                    nameBox.ForeColor = Color.Black;
                }
            };

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

            //soundManager.PlaySound(Sounds.menuMusic, true);
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            eventManager.ButtonPress();
            
            if (globalVariables.NameList.Contains(nameBox.Text))
            {
                MessageBox.Show("Name already taken, please choose another one.");
                return;
            }
            else if (nameBox.Text.Length > 20)
            {
                MessageBox.Show("Name cannot be longer than 20 characters.");
                return;
            }
            else if (string.IsNullOrWhiteSpace(nameBox.Text))
            {
                MessageBox.Show("Please enter a valid name.");
                return;
            }
            else if (nameBox.Text == "Enter Name")
            {
                globalVariables.PlayerName = "Player";
            }
            else
            {
                globalVariables.PlayerName = nameBox.Text;
            }

            int startLevel = levelToBeginAtDropdown.SelectedIndex + 1; // +1 because index starts at 0
            globalVariables.StartLevel = startLevel;    
            formManager.SwitchToForm(this, formManager.FormMain);

            soundManager.PauseSound(Sounds.menuMusic);
        }

        private void buttonHighscore_Click(object sender, EventArgs e)
        {
            eventManager.ButtonPress();
            formManager.OpenForm(formManager.FormHighscore);
        }

        private void Form_Menu_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();      
        }

        private void Form_Menu_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                soundManager.PlaySound(Sounds.menuMusic, true);
            }
            else
            {
                soundManager.PauseSound(Sounds.menuMusic);
            }
        }
    }
}
