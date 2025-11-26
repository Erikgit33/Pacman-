using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacman_Projection
{
    public partial class Form_MapSelectGrid : Form
    {
        FormManager formManager;
        EventManager eventManager;
        GlobalVariables globalVariables;

        Form_Ghosts form_Ghosts;

        IndexPictureBox[,] boxes;
        PictureBox previewGhost;

        GhostImageType selectedImageType;
        GhostChaseType selectedChaseType;

        MapCorner selectedScatterCorner;
        int[] selectedStartingPosition;

        Label label_MapScatterHeader;
        Label label_StartPosHeader;

        Button button_Confirm;

        public Form_MapSelectGrid(FormManager formManager, EventManager eventManager, GlobalVariables globalVariables, Form_Ghosts form_Ghosts, GhostImageType selectedImageType, GhostChaseType selectedChaseType)
        {
            this.formManager = formManager;
            this.eventManager = eventManager;
            this.globalVariables = globalVariables;

            this.form_Ghosts = form_Ghosts;

            this.selectedImageType = selectedImageType;
            this.selectedChaseType = selectedChaseType;

            InitializeComponent();
        }

        private void Form_MapSelectGrid_Load(object sender, EventArgs e)
        {
            ClientSize = new Size(GameConstants.FormWidth, GameConstants.FormHeight + GameConstants.BoxSize * 3);
            this.Location = new Point(GameConstants.FormXOffset, GameConstants.FormYOffset);
            this.BackColor = Color.Black;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true;

            // label_ScatterCornerHeader properties
            label_MapScatterHeader = new Label()
            {
                Location = new Point(GameConstants.FormWidth / 2 - (int)(GameConstants.BoxSize * 20 / 2), GameConstants.BoxSize / 3),
                Size = new Size(GameConstants.BoxSize * 20, GameConstants.BoxSize * 2),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Text = "Select Scatter Corner",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Popup
            };
            Controls.Add(label_MapScatterHeader);
            label_MapScatterHeader.BringToFront();

            // label_StartPosHeader properties
            label_StartPosHeader = new Label()
            {
                Location = new Point(GameConstants.FormWidth / 2 - (int)(GameConstants.BoxSize * 20 / 2), GameConstants.BoxSize / 3),
                Size = new Size(GameConstants.BoxSize * 20, GameConstants.BoxSize * 2),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Text = "Select Starting Position",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Popup
            };
            Controls.Add(label_StartPosHeader);
            label_StartPosHeader.BringToFront();
            label_StartPosHeader.Hide();

            // button_Confirm properties
            button_Confirm = new Button()
            {
                Location = new Point((int)(GameConstants.BoxSize * 11.5), (int)(GameConstants.BoxSize * 9.15)),
                Size = new Size(GameConstants.BoxSize * 7, (int)(GameConstants.BoxSize * 1.7)),
                Font = new Font("Arial", 11, FontStyle.Bold),
                Text = "Confirm",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Yellow
            };
            Controls.Add(button_Confirm);
            button_Confirm.Click += button_Confirm_Click;
            button_Confirm.BringToFront();
            button_Confirm.Hide();

            button_Confirm.MouseEnter += (s, args) =>
            {
                Cursor = Cursors.Hand;
            };

            button_Confirm.MouseLeave += (s, args) => 
            {
                Cursor = Cursors.Default;
            };

            // previewGhost properties 
            previewGhost = new PictureBox()
            {
                Size = new Size(GameConstants.EntitySize, GameConstants.EntitySize),
                BorderStyle = BorderStyle.FixedSingle,
            };
            Controls.Add(previewGhost);
            previewGhost.Hide();

            UpdateGrid_ScatterCorner();
        }

        private void button_Confirm_Click(object sender, EventArgs e)
        {
            eventManager.ButtonPress();

            if (selectedStartingPosition == null)
            {
                UpdateGrid_StartPos();
            }
            else
            {
                formManager.form_Ghosts.ghosts.Add(new Ghost(selectedImageType, selectedChaseType, selectedScatterCorner, selectedStartingPosition));
                formManager.form_Ghosts.UpdateGhostList();
                this.Close();
            }
        }

        private void UpdateGrid_ScatterCorner()
        {
            // Get and place all boxes to re-assign mouse events
            boxes = formManager.form_Main.GetSetUpSelectMap();

            // Loop through all boxes and mark all scatter (map) corner indexes
            for (int indexX = 0; indexX < GameConstants.Boxes_Horizontally; indexX++)
            {
                for (int indexY = 0; indexY < GameConstants.Boxes_Vertically; indexY++)
                {
                    var box = boxes[indexX, indexY];

                    Controls.Add(box);
                    box.BringToFront();

                    Dictionary<MapCorner, (int indexX, int indexY)> scatterCornerIndexes = new Dictionary<MapCorner, (int indexX, int indexY)>
                    {
                        { MapCorner.BottomLeft, (GhostConstants.MapCorner_BottomLeftIndex[0], GhostConstants.MapCorner_BottomLeftIndex[1]) },
                        { MapCorner.TopLeft, (GhostConstants.MapCorner_TopLeftIndex[0], GhostConstants.MapCorner_TopLeftIndex[1]) },
                        { MapCorner.TopRight, (GhostConstants.MapCorner_TopRightIndex[0], GhostConstants.MapCorner_TopRightIndex[1]) },
                        { MapCorner.BottomRight, (GhostConstants.MapCorner_BottomRightIndex[0], GhostConstants.MapCorner_BottomRightIndex[1]) }
                    };

                    if (scatterCornerIndexes.Values.Contains((indexX, indexY)))
                    {
                        box.BackColor = Color.Cyan;

                        box.MouseEnter += (s, e) =>
                        {
                            box.Size = new Size((int)(box.Size.Width * 1.5), (int)(box.Size.Height * 1.5));
                            box.Location = new Point(box.Location.X - (int)(box.Size.Width / 6), box.Location.Y - (int)(box.Size.Height / 6));
                            box.BringToFront();
                            Cursor = Cursors.Hand;
                        };

                        box.Click += (s, e) =>
                        {
                            eventManager.ButtonPress();

                            if (box.Selected)
                            {
                                box.Selected = false;
                                box.BackColor = Color.Cyan;

                                selectedScatterCorner = MapCorner.None;

                                button_Confirm.Hide();
                            }
                            else
                            {
                                // Deselect all scatter corners
                                foreach (var index in scatterCornerIndexes)
                                {
                                    boxes[index.Value.indexX, index.Value.indexY].Selected = false;
                                    boxes[index.Value.indexX, index.Value.indexY].BackColor = Color.Cyan;
                                }

                                box.Selected = true;
                                box.BackColor = Color.Green;
                                
                                foreach (var index in scatterCornerIndexes)
                                {
                                    if (box.Index_2D[0] == index.Value.indexX && box.Index_2D[1] == index.Value.indexY)
                                    {
                                        selectedScatterCorner = index.Key;
                                    }
                                }

                                button_Confirm.Show();
                                button_Confirm.BringToFront();
                            }
                        };

                        box.MouseLeave += (s, e) =>
                        {
                            box.Size = new Size((int)(box.Size.Width / 1.5), (int)(box.Size.Height / 1.5));
                            box.Location = new Point(box.Location.X + (int)(box.Size.Width / 4), box.Location.Y + (int)(box.Size.Height / 4));
                            box.BringToFront();
                            Cursor = Cursors.Default;
                        };
                    }
                }
            }
        }

        private void UpdateGrid_StartPos()
        {
            label_MapScatterHeader.Hide();
            label_StartPosHeader.Show();

            button_Confirm.Hide();

            List<IndexPictureBox> previousSelectedBoxes = new List<IndexPictureBox>();
            List<IndexPictureBox> currentCollidingBoxes = new List<IndexPictureBox>();

            // Remove all boxes from controls 
            for (int indexX = boxes.GetLength(1) - 1; indexX <= 0; indexX--)
            {
                for (int indexY = boxes.GetLength(2) - 1; indexY <= 0; indexY--)
                {
                    Controls.Remove(boxes[indexX, indexY]);
                }
            }

            // Get and place all boxes to re-assign mouse events
            boxes = formManager.form_Main.GetSetUpSelectMap();

            foreach (var box in boxes)
            {
                Controls.Add(box);
                box.BringToFront();

                box.MouseEnter += (boxSender, boxArgs) =>
                {
                    if (!previewGhost.Visible)
                    {
                        previewGhost.Show();
                        previewGhost.BringToFront();
                    }
                    previewGhost.Location = box.Location;

                    // Clear and update currentCollidingBoxes everytime a new box is hovered on
                    currentCollidingBoxes.Clear();

                    foreach (var sorroundBox in boxes)
                    {
                        if (previewGhost.Bounds.IntersectsWith(sorroundBox.Bounds))
                        {
                            currentCollidingBoxes.Add(sorroundBox);
                            if (currentCollidingBoxes.Count == 4)
                            {
                                break;
                            }
                        }
                    }

                    bool obstructed = false;
                    foreach (var collidingBox in currentCollidingBoxes)
                    {
                        if (collidingBox.Obstructed)
                        {
                            obstructed = true; 
                            break; 
                        }
                    }

                    if (!obstructed)
                    {
                        previewGhost.BackColor = Color.Green;
                        Cursor = Cursors.Hand;
                    }
                    else
                    {
                        previewGhost.BackColor = Color.IndianRed;
                        Cursor = Cursors.No;
                    }
                };
            }

            previewGhost.Click += (previewGhostSender, previewGhostArgs) =>
            {
                bool obstructed = false;
                foreach (var collidingBox in currentCollidingBoxes)
                {
                    if (collidingBox.Obstructed)
                    {
                        obstructed = true;
                        break;
                    }
                }

                if (!obstructed)
                {
                    eventManager.ButtonPress();

                    selectedStartingPosition = new int[] { previewGhost.Location.X, previewGhost.Location.Y};
                    button_Confirm.Show();
                    button_Confirm.BringToFront();

                    foreach (var previousCollidingBox in previousSelectedBoxes)
                    {
                        previousCollidingBox.BackColor = GameConstants.Color_Background;
                    }

                    if (!obstructed)
                    {
                        foreach (var collidingBox in currentCollidingBoxes)
                        {
                            collidingBox.BackColor = Color.Green;
                        }
                    }

                    // Register the boxes in the previousSelectedBoxes list
                    previousSelectedBoxes = new List<IndexPictureBox>(currentCollidingBoxes);
                }
                else
                {
                    foreach (var previousCollidingBox in previousSelectedBoxes)
                    {
                        previousCollidingBox.BackColor = GameConstants.Color_Background;
                    }

                    button_Confirm.Hide();
                    return;
                }
            };

            previewGhost.MouseLeave += (previewGhostSender, previewGhostArgs) =>
            {
                Cursor = Cursors.Default;
            };
        }
    }
}
