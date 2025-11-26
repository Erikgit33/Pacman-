using Pacman_Projection.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacman_Projection
{
    public partial class Form_Ghosts : Form
    {
        FormManager formManager;
        EventManager eventManager;
        GlobalVariables globalVariables;

        form_GhostCreator form_GhostCreator;

        Label label_Title;
        Label label_Info;

        internal List<Ghost> ghosts = new List<Ghost>();
        List<IndexPictureBox> ghostBoxes = new List<IndexPictureBox>();
        IndexPictureBox ghostInfoBox;
        PictureBox addBox;

        public Form_Ghosts(FormManager formManager, EventManager eventManager, GlobalVariables globalVariables)
        {
            this.formManager = formManager;
            this.eventManager = eventManager;
            this.globalVariables = globalVariables;

            InitializeComponent();
        }

        private void Form_Ghosts_Load(object sender, EventArgs e)
        {
            // Set form size to fit projector
            ClientSize = new Size(GameConstants.FormWidth, GameConstants.FormHeight);
            this.Location = new Point(GameConstants.FormXOffset, GameConstants.FormYOffset);
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Add the four default ghosts Blinky, Pinky, Inky and Clyde
            ghosts.Add(new Ghost(GhostTemplate.Blinky));
            ghosts.Add(new Ghost(GhostTemplate.Pinky));
            ghosts.Add(new Ghost(GhostTemplate.Inky));
            ghosts.Add(new Ghost(GhostTemplate.Clyde));

            // Update the globalVariables ghost list
            globalVariables.Ghosts = new List<Ghost>(ghosts);

            label_Title = new Label()
            {
                Size = new Size(GameConstants.BoxSize * 15, GameConstants.BoxSize * 4), 
                Font = new Font("Arial", 20, FontStyle.Bold),
                Text = "Ghost Editor",
                Location = new Point(GameConstants.BoxSize * 8 + GameConstants.BoxSize / 2, GameConstants.BoxSize),
                ForeColor = Color.Yellow,
                BackColor = Color.Black
            };
            Controls.Add(label_Title);
            label_Title.BringToFront();

            label_Info = new Label()
            {
                Size = new Size(GameConstants.BoxSize * 26, GameConstants.BoxSize * 2),
                Font = new Font("Arial", 12, FontStyle.Regular),
                Text = "Hover over a ghost to remove it. Click the + to add a new ghost.",
                Location = new Point(GameConstants.BoxSize * 2 + GameConstants.BoxSize / 4, GameConstants.BoxSize * 3 + ((GameConstants.BoxSize * 3) / 4)),
                ForeColor = Color.White,
                BackColor = Color.Black
            };
            Controls.Add(label_Info);
            label_Info.BringToFront();

            UpdateGhostList();
        }

        internal void GetGhostsFromGlobalVariables()
        {
            ghosts = new List<Ghost>(globalVariables.Ghosts);
        }

        internal void UpdateGhostList()
        {
            // Remove all IndexPictureBoxes that need to be redrawn
            if (addBox != null)
            {
                Controls.Remove(addBox);
                addBox.Dispose();
            }

            int ghostBoxIndex = ghostBoxes.Count - 1;
            while (ghostBoxIndex >= 0)
            {
                var ghostBox = ghostBoxes[ghostBoxIndex];
                Controls.Remove(ghostBox);
                ghostBox.Dispose();
                ghostBoxes.RemoveAt(ghostBoxIndex);
                ghostBoxIndex--;
            }   

            int row = 0;
            for (int index = 0; index <= ghosts.Count; index++)
            {
                if (index == ghosts.Count)
                {
                    PictureBox addBox = new PictureBox()
                    {
                        Size = new Size(GameConstants.BoxSize * 6, GameConstants.BoxSize * 6),
                        Location = new Point(GameConstants.BoxSize * 2 + GameConstants.BoxSize / 4 + index * (GameConstants.BoxSize * 6 + GameConstants.BoxSize / 2) - row * 4 * (GameConstants.BoxSize * 6 + GameConstants.BoxSize / 2),
                                             GameConstants.BoxSize * 7 + row * (GameConstants.BoxSize * 6 + (int)(GameConstants.BoxSize * 2.5))),
                        BorderStyle = BorderStyle.FixedSingle,
                        ForeColor = Color.Green,
                        Image = Resources.AddSign,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Padding = new Padding(20),
                        Cursor = Cursors.Hand
                    };
                    Controls.Add(addBox);
                    addBox.BringToFront();
                    addBox.Click += AddBox_Click;

                    // Save reference to addBox for use during redraw operations
                    this.addBox = addBox;

                    break;
                }           
                
                Ghost ghost = ghosts[index];

                IndexPictureBox ghostBox = new IndexPictureBox(ghosts.IndexOf(ghost))
                {
                    Size = new Size(GameConstants.BoxSize * 6, GameConstants.BoxSize * 6),
                    Location = new Point(GameConstants.BoxSize * 2 + GameConstants.BoxSize / 4 + ghosts.IndexOf(ghost) * (GameConstants.BoxSize * 6 + GameConstants.BoxSize / 2) - row * 4 * (GameConstants.BoxSize * 6 + GameConstants.BoxSize / 2),
                                         GameConstants.BoxSize * 7 + row * (GameConstants.BoxSize * 6 + (int)(GameConstants.BoxSize * 2.5))),
                    BorderStyle = BorderStyle.FixedSingle,
                    ForeColor = Color.Gray,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Padding = new Padding(10)
                };
                Controls.Add(ghostBox);
                ghostBox.BringToFront();

                // Keep track of all ghost boxes    
                ghostBoxes.Add(ghostBox);

                // Create label for image type name
                Label infoLabel = new Label()
                {
                    Size = new Size(GameConstants.BoxSize * 4, GameConstants.BoxSize * 2),
                    Font = new Font("Arial", 8, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Black,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Popup
                };
                Controls.Add(infoLabel);
                infoLabel.BringToFront();

                // Assign the label to the box for easy access later
                ghostBox.label = infoLabel;
                ghostBox.UpdateLabelLocation();

                infoLabel.Text = ghosts[ghostBox.Index].GhostImageType.ToString() + "\n" + ghosts[ghostBox.Index].GhostChaseType.ToString();

                // Randomize the picture 
                int random = new Random().Next(0, 2);
                if (random == 0)
                {
                    ghostBox.Image = ghost.Images[ImageType.Stationary];
                }
                else
                {
                    ghostBox.Image = ghost.Images[ImageType.Stationary2];
                }

                if ((ghosts.IndexOf(ghost) + 1) % 4 == 0)
                {
                    row++;
                }

                ghostBox.MouseEnter += (s, e) =>
                {
                    IndexPictureBox removeBox = new IndexPictureBox(ghostBox.Index)
                    {
                        Size = ghostBox.Size,
                        Location = ghostBox.Location,
                        BackColor = Color.Transparent,
                        ForeColor = Color.Red,
                        BorderStyle = BorderStyle.FixedSingle,
                        Image = Resources.Trashcan,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Padding = new Padding(20),
                        Cursor = Cursors.Hand
                    };
                    Controls.Add(removeBox);   
                    removeBox.BringToFront();
                    removeBox.Click += RemoveBox_Click;

                    removeBox.MouseLeave += (send, args) =>
                    {
                        Controls.Remove(removeBox);
                        removeBox.Dispose();

                        ghostBox.BringToFront();
                    };
                }; 
            }
        }

        private void AddBox_Click(object sender, EventArgs e)
        {
            eventManager.ButtonPress();

            MessageBox.Show("Feature not implemented, sorry!");
            return;
          
            // Update the ghost list in globalVariables before opening the ghost creator form
            globalVariables.Ghosts = new List<Ghost>(ghosts);

            form_GhostCreator = new form_GhostCreator(formManager, eventManager, globalVariables, this);
            form_GhostCreator.Show();
            this.Enabled = false;
            form_GhostCreator.FormClosed += (s, args) =>
            {
                form_GhostCreator = null;
                this.Enabled = true;
            };
        }

        private void RemoveBox_Click(object sender, EventArgs e)
        {
            eventManager.ButtonPress();

            MessageBox.Show("Feature not implemented, sorry!");
            return;

            var result = MessageBox.Show("Are you sure you want to remove the ghost?", "Remove Ghost", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result.Equals(DialogResult.Yes))
            {
                var removeBox = (IndexPictureBox)sender;
                ghosts.RemoveAt(removeBox.Index);

                Controls.Remove(removeBox);
                removeBox.Dispose();

                UpdateGhostList();
            }
        }

        private void Form_Ghosts_FormClosing(object sender, FormClosingEventArgs e)
        {
            eventManager.ButtonPress();

            e.Cancel = true;

            // Update the ghost list in globalVariables when closing the form
            globalVariables.Ghosts = new List<Ghost>(ghosts);

            formManager.CloseForm(this);
        }
    }
}
