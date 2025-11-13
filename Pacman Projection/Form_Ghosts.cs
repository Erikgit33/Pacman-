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

        List<Ghost> ghosts = new List<Ghost>();

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
            ClientSize = new Size(GameConstants.BoxSize * 30, GameConstants.BoxSize * 38);
            this.Location = new Point(388, 85);
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

            UpdateGhostList();
        }

        private void UpdateGhostList()
        {
            // Remove all IndexPictureBoxes and PictureBoxes to redraw them in the correct order and location
            foreach (object obj in Controls)
            {
                if (obj is IndexPictureBox)
                {
                    Controls.Remove(obj as IndexPictureBox);
                    var indexPictureBox = (IndexPictureBox)obj;
                    indexPictureBox.Dispose();
                }
                else if (obj is PictureBox)
                {
                    Controls.Remove(obj as PictureBox);
                    var addBox = (PictureBox)obj;
                    addBox.Dispose();
                }
            }

            int row = 0;
            for (int index = 0; index <= ghosts.Count; index++)
            {
                if (index == ghosts.Count)
                {
                    PictureBox addBox = new PictureBox
                    {
                        Size = new Size(GameConstants.BoxSize * 6, GameConstants.BoxSize * 6),
                        Location = new Point(GameConstants.BoxSize * 2 + GameConstants.BoxSize / 4 + index * (GameConstants.BoxSize * 6 + GameConstants.BoxSize / 2) - row * 4 * (GameConstants.BoxSize * 6 + GameConstants.BoxSize / 2),
                                         GameConstants.BoxSize * 5 + row * (GameConstants.BoxSize * 6 + GameConstants.BoxSize / 2)),
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

                    break;
                }           
                
                Ghost ghost = ghosts[index];

                IndexPictureBox ghostBox = new IndexPictureBox(ghosts.IndexOf(ghost))
                {
                    Size = new Size(GameConstants.BoxSize * 6, GameConstants.BoxSize * 6),
                    Location = new Point(GameConstants.BoxSize * 2 + GameConstants.BoxSize / 4 + ghosts.IndexOf(ghost) * (GameConstants.BoxSize * 6 + GameConstants.BoxSize / 2) - row * 4 * (GameConstants.BoxSize * 6 + GameConstants.BoxSize / 2),
                                         GameConstants.BoxSize * 5 + row * (GameConstants.BoxSize * 6 + GameConstants.BoxSize / 2)),
                    BorderStyle = BorderStyle.FixedSingle,
                    ForeColor = Color.Gray,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Padding = new Padding(10)
                };
                Controls.Add(ghostBox);
                ghostBox.BringToFront();

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
                    Task.Delay(GameConstants.EventTimes.removeBoxDelay).Wait();

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

                    removeBox.MouseLeave += (S, E) =>
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
            
        }
 
        private void RemoveBox_Click(object sender, EventArgs e)
        {
            eventManager.ButtonPress();

            var result = MessageBox.Show("Are you sure you want to delete this ghost?", "Delete Ghost", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result.Equals(DialogResult.Yes))
            {
                var removeBox = (IndexPictureBox)sender;
                ghosts.RemoveAt(removeBox.Index);

                UpdateGhostList();
            }
        }

        private void Form_Ghosts_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;

            // Update the ghost list in globalVariables
            globalVariables.Ghosts = new List<Ghost>(ghosts);

            formManager.CloseForm(this);
        }
    }
}
