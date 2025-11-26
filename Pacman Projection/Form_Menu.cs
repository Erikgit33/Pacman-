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

        const int boxSize = GameConstants.BoxSize;

        Button button_Play;
        Button button_Highscore;
        Button button_Ghosts;
        TextBox textBox_Name;
        ComboBox comboBox_StartLevel;

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
            ClientSize = new Size(GameConstants.FormWidth, GameConstants.FormHeight);
            this.Location = new Point(GameConstants.FormXOffset, GameConstants.FormYOffset);
            this.BackColor = Color.Black;

            // button_Play properties
            button_Play = new Button
            {
                Location = new Point(boxSize * 12, boxSize * 17),
                Size = new Size(boxSize * 6, boxSize * 5),
                Font = new Font("Arial", 22, FontStyle.Bold),
                Text = "Play",
                ForeColor = Color.Yellow,
            };
            button_Play.Click += button_Play_Click;
            Controls.Add(button_Play);
            button_Play.BringToFront();

            button_Play.MouseEnter += (s, args) =>
            {
                Cursor = Cursors.Hand;
            };

            button_Play.MouseLeave += (s, args) =>
            {
                Cursor = Cursors.Default;
            };

            // button_Highscore properties
            button_Highscore = new Button
            {
                Location = new Point(boxSize * 12, boxSize * 12),
                Size = new Size(boxSize * 6, boxSize * 5),
                Font = new Font("Arial", 22, FontStyle.Bold),
                Text = "Highscore",
                ForeColor = Color.Yellow
            };
            button_Highscore.Click += button_Highscore_Click;
            Controls.Add(button_Highscore);
            button_Highscore.BringToFront();

            button_Highscore.MouseEnter += (s, args) =>
            {
                Cursor = Cursors.Hand;
            };

            button_Highscore.MouseLeave += (s, args) =>
            {
                Cursor = Cursors.Default;
            };

            // button_Ghosts properties
            button_Ghosts = new Button
            {
                Location = new Point(boxSize * 12, boxSize),
                Size = new Size(boxSize * 6, boxSize * 5),
                Font = new Font("Arial", 22, FontStyle.Bold),
                Text = "Ghosts",
                ForeColor = Color.Yellow
            };
            button_Ghosts.Click += button_Ghosts_Click;
            Controls.Add(button_Ghosts);
            button_Ghosts.BringToFront();

            button_Ghosts.MouseEnter += (s, args) =>
            {
                Cursor = Cursors.Hand;
            };

            button_Ghosts.MouseLeave += (s, args) =>
            {
                Cursor = Cursors.Default;
            };

            // nameBox properties 
            textBox_Name = new TextBox
            {
                Location = new Point(boxSize * 10, boxSize * 7),
                Size = new Size(boxSize * 10, boxSize * 3),
                Font = new Font("Arial", 22, FontStyle.Bold),
                Text = "Enter Name",
                ForeColor = Color.Gray,
                TextAlign = HorizontalAlignment.Center
            };
            Controls.Add(textBox_Name);

            textBox_Name.GotFocus += (s, E) =>
            {
                if (textBox_Name.Text == "Enter Name")
                {
                    textBox_Name.Text = "";
                    textBox_Name.ForeColor = Color.Black;
                }
            };

            // Create the levelToBeginAt dropdown
            comboBox_StartLevel = new ComboBox()
            {
                DisplayMember = Text,
                MaxDropDownItems = 10,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(50, 50),
                Font = new Font("Arial", 16, FontStyle.Bold)
            };

            comboBox_StartLevel.Items.Add(1);
            comboBox_StartLevel.Items.Add(2);
            comboBox_StartLevel.Items.Add(3);
            comboBox_StartLevel.Items.Add(4);
            comboBox_StartLevel.Items.Add(5);
            comboBox_StartLevel.Items.Add(6);
            comboBox_StartLevel.Items.Add(7);
            comboBox_StartLevel.Items.Add(8);
            comboBox_StartLevel.Items.Add(9);
            comboBox_StartLevel.Items.Add(10);
            comboBox_StartLevel.SelectedIndex = 0;
            
            Controls.Add(comboBox_StartLevel);
            comboBox_StartLevel.BringToFront();
        }

        private void button_Play_Click(object sender, EventArgs e)
        {
            eventManager.ButtonPress();
            
            if (globalVariables.NameList.Contains(textBox_Name.Text))
            {
                MessageBox.Show("Name already taken, please choose another one.");
                return;
            }
            else if (textBox_Name.Text.Length > 10)
            {
                MessageBox.Show("Name cannot be longer than 10 characters.");
                return;
            }
            else if (string.IsNullOrWhiteSpace(textBox_Name.Text))
            {
                MessageBox.Show("Please enter a valid name.");
                return;
            }
            else if (textBox_Name.Text == "Enter Name")
            {
                // Get the ammount of players with default names "PlayerX" and add one to it to avoid duplicates
                List<string> PlayerEntriesWithNamePlayer = globalVariables.NameList.Where(name => name.StartsWith("Player")).ToList();

                int count = PlayerEntriesWithNamePlayer.Count;

                if (count > 0)
                {
                    globalVariables.PlayerName = "Player" + PlayerEntriesWithNamePlayer.Count;
                }
                else
                {
                    globalVariables.PlayerName = "Player";
                }
            }
            else
            {
                globalVariables.PlayerName = textBox_Name.Text;
            }

            int startLevel = comboBox_StartLevel.SelectedIndex + 1;
            globalVariables.StartLevel = startLevel; 
        
            // If form_Ghosts hasn't been opened, globalVariables.Ghosts is null, so default the list to the four ghosts
            if (globalVariables.Ghosts == null)
            {
                globalVariables.Ghosts = new List<Ghost> 
                { 
                    new Ghost(GhostTemplate.Blinky),
                    new Ghost(GhostTemplate.Pinky),
                    new Ghost(GhostTemplate.Inky),
                    new Ghost(GhostTemplate.Clyde)
                };
            }
            else if (globalVariables.Ghosts.Count == 0)
            {
                MessageBox.Show("Cannot begin the game without atleast one ghost!");
                return;
            }

            formManager.SwitchToForm(this, formManager.form_Main);
            
            soundManager.StopSound(Sounds.menuMusic);
        }

        private void button_Highscore_Click(object sender, EventArgs e)
        {
            eventManager.ButtonPress();
            formManager.OpenForm(formManager.form_Highscore);
        }

        private void button_Ghosts_Click(object sender, EventArgs e)
        {
            eventManager.ButtonPress();
            formManager.OpenForm(formManager.form_Ghosts);
        }

        private void Form_Menu_FormClosing(object sender, FormClosingEventArgs e)
        {
            eventManager.ButtonPress();

            var result = MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButtons.YesNo);
            if (result.Equals(DialogResult.No))
            {
                e.Cancel = true;
                return;
            }

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
