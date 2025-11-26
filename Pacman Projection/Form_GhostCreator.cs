using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacman_Projection
{
    public partial class form_GhostCreator : Form
    {
        FormManager formManager;
        EventManager eventManager;
        GlobalVariables globalVariables;

        Form_Ghosts form_Ghosts;

        List<IndexPictureBox> displayBoxes = new List<IndexPictureBox>();

        Label label_TemplateHeader;
        Label label_ImageTypeHeader;
        Label label_ChaseTypeHeader;

        Button button_Confirm;
        Button button_Custom;

        Button button_Next;
        Button button_Prev;

        int displayBoxes_firstDefaultX;
        int displayBoxes_lastDefaultX;

        List<GhostTemplate> availableGhostTemplates = new List<GhostTemplate>
        {
            GhostTemplate.Blinky,
            GhostTemplate.Pinky,
            GhostTemplate.Inky,
            GhostTemplate.Clyde
        };

        // All ghostImageTypes can always be selected since they only change the appearance of the ghost
        List<GhostImageType> availableGhostImageTypes = new List<GhostImageType>
        {
            GhostImageType.Blinky,
            GhostImageType.Pinky,
            GhostImageType.Inky,
            GhostImageType.Clyde,
            GhostImageType.Sue,
            GhostImageType.Funky,
            GhostImageType.Spunky,
            GhostImageType.Whimsy
        };

        List<GhostChaseType> availableGhostChaseTypes = new List<GhostChaseType>()
        {
            GhostChaseType.Chase,
            GhostChaseType.Ambush,
            GhostChaseType.Flank,
            GhostChaseType.Fallback,
            GhostChaseType.Random
        };
     

        GhostTemplate selectedTemplate;

        GhostImageType selectedImageType;
        bool selectingImageType;
        bool imageTypeConfirmed;

        GhostChaseType selectedChaseType;
        bool selectingChaseType;
        bool chaseTypeConfirmed;

        public form_GhostCreator(FormManager formManager, EventManager eventManager, GlobalVariables globalVariables, Form_Ghosts form_Ghosts)
        {
            this.formManager = formManager;
            this.eventManager = eventManager;
            this.globalVariables = globalVariables;

            this.form_Ghosts = form_Ghosts;

            InitializeComponent();
        }

        private void form_GhostCreator_Load(object sender, EventArgs e)
        {
            ClientSize = new Size(GameConstants.FormWidth, GameConstants.BoxSize * 14);
            this.Location = new Point(GameConstants.FormXOffset, GameConstants.FormYOffset + GameConstants.FormHeight / 2 - ClientSize.Height / 2 - GameConstants.BoxSize * 6);
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // label_template properties
            label_TemplateHeader = new Label()
            {
                Location = new Point(GameConstants.FormWidth / 2 - GameConstants.BoxSize * 9, GameConstants.BoxSize / 2),
                Size = new Size(GameConstants.BoxSize * 18, GameConstants.BoxSize * 2),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Text = "Select Ghost Template",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Popup
            };
            Controls.Add(label_TemplateHeader);
            label_TemplateHeader.BringToFront();
            label_TemplateHeader.Hide();

            // label_imageType properties
            label_ImageTypeHeader = new Label()
            {
                Location = label_TemplateHeader.Location,
                Size = label_TemplateHeader.Size,
                Font = label_TemplateHeader.Font,
                Text = "Select Ghost Image Type",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Popup
            };
            Controls.Add(label_ImageTypeHeader);
            label_ImageTypeHeader.BringToFront();
            label_ImageTypeHeader.Hide();

            // label_chaseType properties
            label_ChaseTypeHeader = new Label()
            {
                Location = label_TemplateHeader.Location,
                Size = label_TemplateHeader.Size,
                Font = label_TemplateHeader.Font,
                Text = "Select Ghost Chase Type",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Popup
            };
            Controls.Add(label_ChaseTypeHeader);
            label_ChaseTypeHeader.BringToFront();
            label_ChaseTypeHeader.Hide();

            // button_confirm properties
            button_Confirm = new Button()
            {
                Location = new Point(GameConstants.FormWidth / 2 - (int)(GameConstants.BoxSize * 3.5), (int)(GameConstants.BoxSize * 10.5)),
                Size = new Size(GameConstants.BoxSize * 7, (int)(GameConstants.BoxSize * 2.5)),
                Font = new Font("Arial", 12, FontStyle.Regular),
                Text = "Confirm",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Yellow
            };
            button_Confirm.Click += button_confirm_Click;
            Controls.Add(button_Confirm);
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

            // button_Custom properties
            button_Custom = new Button()
            {
                Location = new Point(GameConstants.FormWidth / 2 - (int)(GameConstants.BoxSize * 3.5), (int)(GameConstants.BoxSize * 10.5)),
                Size = new Size(GameConstants.BoxSize * 7, (int)(GameConstants.BoxSize * 2.5)),
                Font = new Font("Arial", 12, FontStyle.Regular),
                Text = "Custom",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Yellow
            };
            button_Custom.Click += button_custom_Click;
            Controls.Add(button_Custom);
            button_Custom.BringToFront();

            button_Custom.MouseEnter += (s, args) =>
            {
                Cursor = Cursors.Hand;
            };

            button_Custom.MouseLeave += (s, args) =>
            {
                Cursor = Cursors.Default;
            };

            // button_Next properties
            button_Next = new Button()
            {
                Location = new Point(GameConstants.FormWidth - GameConstants.BoxSize * 7, button_Confirm.Location.Y),
                Size = new Size(GameConstants.BoxSize * 6, (int)(GameConstants.BoxSize * 2.5)),
                Font = new Font("Arial", 10, FontStyle.Regular),
                Text = "Next",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Yellow
            };
            button_Next.Click += button_next_Click;
            Controls.Add(button_Next);
            button_Next.BringToFront();

            button_Next.MouseEnter += (s, args) =>
            {
                Cursor = Cursors.Hand;
            };

            button_Next.MouseLeave += (s, args) =>
            {
                Cursor = Cursors.Default;
            };

            // button_Prev properties
            button_Prev = new Button()
            {
                Location = new Point(GameConstants.BoxSize, button_Confirm.Location.Y),
                Size = new Size(GameConstants.BoxSize * 6, (int)(GameConstants.BoxSize * 2.5)),
                Font = new Font("Arial", 10, FontStyle.Regular),
                Text = "Previous",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Yellow
            };
            button_Prev.Click += button_prev_Click;
            Controls.Add(button_Prev);
            button_Prev.BringToFront();

            button_Prev.MouseEnter += (s, args) =>
            {
                Cursor = Cursors.Hand;
            };

            button_Prev.MouseLeave += (s, args) =>
            {
                Cursor = Cursors.Default;
            };

            List<GhostChaseType> currentChaseTypes = new List<GhostChaseType>();
            foreach (Ghost ghost in globalVariables.Ghosts)
            {
                if (!currentChaseTypes.Contains(ghost.GhostChaseType))
                {
                    currentChaseTypes.Add(ghost.GhostChaseType);
                }
            }

            // If there is no ghost with Chase as ChaseType, there can also be no ghost with Flank as ChaseType
            // Flank is dependant on there being a ghost with Chase as ChaseType
            if (!currentChaseTypes.Contains(GhostChaseType.Chase))
            {
                availableGhostChaseTypes.Remove(GhostChaseType.Flank);
                // Remove all ghostTemplates with Flank as their ChaseType
                int count = availableGhostTemplates.Count;
                for (int index = count - 1; index > 0; index--)
                {
                    Ghost testGhost = new Ghost(availableGhostTemplates[index]);
                    if (testGhost.GhostChaseType == GhostChaseType.Flank)
                    {
                        availableGhostTemplates.RemoveAt(index);
                    }
                }
            }

            UpdateDisplayBoxes_Templates();
        }

        private void button_confirm_Click(object sender, EventArgs e)
        {
            eventManager.ButtonPress();

            if (selectingImageType)
            {
                imageTypeConfirmed = true;
                selectingImageType = false;
            }
            else if (selectingChaseType)
            {
                chaseTypeConfirmed = true;
                selectingChaseType = false;
            }

            // If an image type but no chase type has been selected, show all available chase types
            if (imageTypeConfirmed && !chaseTypeConfirmed)
            {
                UpdateDisplayBoxes_ChaseTypes();
                button_Confirm.Hide();
            }
            // If both an image type and chase type has been selected, display the map grid to choose starting position and scatter corner
            else if (imageTypeConfirmed && chaseTypeConfirmed)
            {
                new Form_MapSelectGrid(formManager, eventManager, globalVariables, form_Ghosts, selectedImageType, selectedChaseType).Show();
                this.Close();
            }
            // If no custom ghost has been initialized
            else if (!selectingImageType)
            {
                switch (selectedTemplate)
                {
                    case GhostTemplate.Blinky:
                        form_Ghosts.ghosts.Add(new Ghost(GhostTemplate.Blinky));
                        break;
                    case GhostTemplate.Pinky:
                        form_Ghosts.ghosts.Add(new Ghost(GhostTemplate.Pinky));
                        break;
                    case GhostTemplate.Inky:
                        form_Ghosts.ghosts.Add(new Ghost(GhostTemplate.Inky));
                        break;
                    case GhostTemplate.Clyde:
                        form_Ghosts.ghosts.Add(new Ghost(GhostTemplate.Clyde));
                        break;
                }

                formManager.form_Ghosts.UpdateGhostList();
                this.Close();
            }
        }

        private void button_custom_Click(object sender, EventArgs e)
        {
            eventManager.ButtonPress();

            button_Custom.Hide();

            // Display image types for custom selection
            UpdateDisplayBoxes_ImageTypes();
            UpdateButtonVisibility();
        }

        private void button_next_Click(object sender, EventArgs e)
        {
            eventManager.ButtonPress();

            // Shift the boxes to the left to show the next set of options
            foreach (var box in displayBoxes)
            {
                box.Left -= (int)(GameConstants.BoxSize * 6.5);
            }

            UpdateButtonVisibility();
        }   

        private void button_prev_Click(object sender, EventArgs e)
        {
            eventManager.ButtonPress();

            // Shift the boxes to the right to show the previous set of options
            foreach (var box in displayBoxes)
            {
                box.Left += (int)(GameConstants.BoxSize * 6.5);
            }

            UpdateButtonVisibility();
        }

        private void UpdateButtonVisibility()
        { 
            if (displayBoxes.First().Location.X == displayBoxes_firstDefaultX)
            {
                button_Prev.Hide();
            }
            else
            {
                button_Prev.Show();
            }

            if (displayBoxes.Last().Location.X <= displayBoxes_lastDefaultX || displayBoxes_lastDefaultX == 0)
            {
                button_Next.Hide();
            }
            else 
            {
                button_Next.Show();
            }
        }

        private void SelectToggle_TemplateBox(IndexPictureBox box)
        {
            if (box.Selected)
            {
                box.BorderStyle = BorderStyle.None;
                box.BackColor = Color.Black;
                box.Selected = false;
            }
            else
            {
                box.BorderStyle = BorderStyle.FixedSingle;
                box.BackColor = Color.Green;
                box.Selected = true;
            }
        }

        private void SelectToggle_Box(IndexPictureBox box)
        {
            if (box.Selected)
            {
                box.BorderStyle = BorderStyle.FixedSingle;
                box.BackColor = Color.Black;
                box.Selected = false;
            }
            else
            {
                box.BorderStyle = BorderStyle.FixedSingle;
                box.BackColor = Color.Green;
                box.Selected = true;
            }
        }

        private void ClearDisplayBoxes_Templates()
        {
            // Dispose of existing boxes 
            int index = displayBoxes.Count - 1;
            while (index >= 0)
            {
                displayBoxes[index].Dispose();
                displayBoxes.RemoveAt(index);
                index--;
            }

            label_ImageTypeHeader.Hide();
            label_ChaseTypeHeader.Hide();

            label_TemplateHeader.Show();

            if (availableGhostTemplates.Count <= 4)
            {
                // Hide navigation buttons if all boxes fit on one page
                button_Prev.Hide();
                button_Next.Hide();
            }
            else
            {
                button_Prev.Show();
                button_Next.Show();
            }
        }

        private void ClearDisplayBoxes_ImageTypes()
        {
            // Dispose of existing boxes 
            int index = displayBoxes.Count - 1;
            while (index >= 0)
            {
                displayBoxes[index].Dispose();
                displayBoxes.RemoveAt(index);
                index--;
            }

            label_TemplateHeader.Hide();
            label_ChaseTypeHeader.Hide();

            label_ImageTypeHeader.Show();

            if (availableGhostImageTypes.Count <= 4)
            {
                // Hide navigation buttons if all boxes fit on one page
                button_Prev.Hide();
                button_Next.Hide();
            }
            else
            {
                button_Prev.Show();
                button_Next.Show();
            }
        }

        private void ClearDisplayBoxes_ChaseTypes()
        {
            // Dispose of existing boxes 
            int index = displayBoxes.Count - 1;
            while (index >= 0)
            {
                displayBoxes[index].Dispose();
                displayBoxes.RemoveAt(index);
                index--;
            }

            label_TemplateHeader.Hide();
            label_ImageTypeHeader.Hide();

            label_ChaseTypeHeader.Show();

            if (availableGhostChaseTypes.Count <= 4)
            {
                // Hide navigation buttons if all boxes fit on one page
                button_Prev.Hide();
                button_Next.Hide();
            }
            else
            {
                button_Prev.Show();
                button_Next.Show();
            }
        }

        private void UpdateDisplayBoxes_Templates()
        {
            ClearDisplayBoxes_Templates();

            int boxesToDisplay = availableGhostTemplates.Count;
           
            for (int index = 0; index < boxesToDisplay; index++)
            {
                IndexPictureBox box = new IndexPictureBox(index)
                {
                    Size = new Size(GameConstants.BoxSize * 6, GameConstants.BoxSize * 6),
                    Location = new Point((int)(GameConstants.BoxSize * 2.25) + index * (int)(GameConstants.BoxSize * 6.5), GameConstants.BoxSize * 2 + GameConstants.BoxSize / 2),
                    BorderStyle = BorderStyle.None,
                    ForeColor = Color.Gray,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Padding = new Padding(10)
                };
                Controls.Add(box);
                box.BringToFront();

                // Register the box in the displayBoxes array
                displayBoxes.Add(box);

                // Create label for template name
                Label templateLabel = new Label()
                {
                    Size = new Size(GameConstants.BoxSize * 4, GameConstants.BoxSize),
                    Font = new Font("Arial", 8, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Black,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Popup
                };
                Controls.Add(templateLabel);
                templateLabel.BringToFront();

                // Assign the label to the box for easy access later
                box.label = templateLabel;
                box.UpdateLabelLocation();

                try
                {
                    var templateType = availableGhostTemplates[index];

                    switch (availableGhostTemplates[index])
                    {
                        case GhostTemplate.Blinky:
                            box.Image = GhostConstants.Images.Blinky.stationary;
                            templateLabel.Text = "Blinky";
                            break;
                        case GhostTemplate.Pinky:
                            box.Image = GhostConstants.Images.Pinky.stationary;
                            templateLabel.Text = "Pinky";
                            break;
                        case GhostTemplate.Inky:
                            box.Image = GhostConstants.Images.Inky.stationary;
                            templateLabel.Text = "Inky";
                            break;
                        case GhostTemplate.Clyde:
                            box.Image = GhostConstants.Images.Clyde.stationary;
                            templateLabel.Text = "Clyde";
                            break;
                    }
                }
                catch
                {
                    box.Image = null;
                    templateLabel.Text = string.Empty;
                }

                AssignMouseEventsToTemplateBox(box);
            }
            UpdateDefaultLocations();
            UpdateButtonVisibility();
        }

        private void UpdateDisplayBoxes_ImageTypes()
        {
            selectingImageType = true;

            ClearDisplayBoxes_ImageTypes();

            int boxesToDisplay = availableGhostImageTypes.Count;

            for (int index = 0; index < boxesToDisplay; index++)
            {
                IndexPictureBox box = new IndexPictureBox(index)
                {
                    Size = new Size(GameConstants.BoxSize * 6, GameConstants.BoxSize * 6),
                    Location = new Point((int)(GameConstants.BoxSize * 2.25) + index * (int)(GameConstants.BoxSize * 6.5), GameConstants.BoxSize * 2 + GameConstants.BoxSize / 2),
                    BorderStyle = BorderStyle.FixedSingle,
                    ForeColor = Color.Gray,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Padding = new Padding(10)
                };
                Controls.Add(box);
                box.BringToFront();

                // Register the box in the displayBoxes array
                displayBoxes.Add(box);

                // Create label for image type name
                Label imageTypeLabel = new Label()
                {
                    Size = new Size(GameConstants.BoxSize * 4, GameConstants.BoxSize),
                    Font = new Font("Arial", 8, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Black,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Popup
                };
                Controls.Add(imageTypeLabel);
                imageTypeLabel.BringToFront();

                // Assign the label to the box for easy access later
                box.label = imageTypeLabel;
                box.UpdateLabelLocation();

                try
                {
                    var imageType = availableGhostImageTypes[index];

                    switch (availableGhostImageTypes[index])
                    {
                        case GhostImageType.Blinky:
                            box.Image = GhostConstants.Images.Blinky.stationary;
                            imageTypeLabel.Text = "Blinky";
                            break;
                        case GhostImageType.Pinky:
                            box.Image = GhostConstants.Images.Pinky.stationary;
                            imageTypeLabel.Text = "Pinky";
                            break;
                        case GhostImageType.Inky:
                            box.Image = GhostConstants.Images.Inky.stationary;
                            imageTypeLabel.Text = "Inky";
                            break;
                        case GhostImageType.Clyde:
                            box.Image = GhostConstants.Images.Clyde.stationary;
                            imageTypeLabel.Text = "Clyde";
                            break;
                        case GhostImageType.Sue:
                            box.Image = GhostConstants.Images.Sue.stationary;
                            imageTypeLabel.Text = "Sue";
                            break;
                        case GhostImageType.Funky:
                            box.Image = GhostConstants.Images.Funky.stationary;
                            imageTypeLabel.Text = "Funky";
                            break;
                        case GhostImageType.Spunky:
                            box.Image = GhostConstants.Images.Spunky.stationary;
                            imageTypeLabel.Text = "Spunky";
                            break;
                        case GhostImageType.Whimsy:
                            box.Image = GhostConstants.Images.Whimsy.stationary;
                            imageTypeLabel.Text = "Whimsy";
                            break;
                    }
                }
                catch
                {
                    box.Image = null;
                    imageTypeLabel.Text = string.Empty;
                }
               
                AssignMouseEventsToBox(box);
            }
            UpdateDefaultLocations();
            UpdateButtonVisibility();
        }

        private void UpdateDisplayBoxes_ChaseTypes()
        {
            selectingChaseType = true;

            ClearDisplayBoxes_ChaseTypes();

            int boxesToDisplay = availableGhostChaseTypes.Count;

            for (int index = 0; index < boxesToDisplay; index++)
            {
                IndexPictureBox box = new IndexPictureBox(index)
                {
                    Size = new Size(GameConstants.BoxSize * 6, GameConstants.BoxSize * 6),
                    Location = new Point((int)(GameConstants.BoxSize * 2.25) + index * (int)(GameConstants.BoxSize * 6.5), GameConstants.BoxSize * 2 + GameConstants.BoxSize / 2),
                    BorderStyle = BorderStyle.FixedSingle,
                    ForeColor = Color.Gray,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Padding = new Padding(10)
                };
                Controls.Add(box);
                box.BringToFront();

                // Register the box in the displayBoxes array
                displayBoxes.Add(box);

                // Create label for chase type name
                Label chaseTypeLabel = new Label()
                {
                    Size = new Size(GameConstants.BoxSize * 7, GameConstants.BoxSize),
                    Font = new Font("Arial", 8, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Black,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Popup
                };
                Controls.Add(chaseTypeLabel);
                chaseTypeLabel.BringToFront();

                // Assign the label to the box for easy access later
                box.label = chaseTypeLabel;
                box.UpdateLabelLocation();

                try
                {
                    var chaseType = availableGhostChaseTypes[index];

                    switch (chaseType)
                    {
                        case GhostChaseType.Chase:
                            box.Image = GhostConstants.Images.ChaseTypes.chase;
                            chaseTypeLabel.Text = "Blinky - Chase";
                            break;
                        case GhostChaseType.Ambush:
                            box.Image = GhostConstants.Images.ChaseTypes.ambush;
                            chaseTypeLabel.Text = "Pinky - Ambush";
                            break;
                        case GhostChaseType.Flank:
                            box.Image = GhostConstants.Images.ChaseTypes.flank;
                            chaseTypeLabel.Text = "Inky - Flank";
                            break;
                        case GhostChaseType.Fallback:
                            box.Image = GhostConstants.Images.ChaseTypes.fallback;
                            chaseTypeLabel.Text = "Clyde - Fallback";
                            break;
                        case GhostChaseType.Random:
                            box.Image = GhostConstants.Images.ChaseTypes.random;
                            chaseTypeLabel.Text = "Random";
                            break;
                    }
                }
                catch
                {
                    box.Image = null;
                    chaseTypeLabel.Text = string.Empty;
                }

                AssignMouseEventsToBox(box);
            }
            UpdateDefaultLocations();
            UpdateButtonVisibility();
        }

        private void AssignMouseEventsToTemplateBox(IndexPictureBox box)
        {
            box.MouseEnter += (s, e) =>
            {
                box.Size = new Size((int)(box.Size.Width * 1.1), (int)(box.Size.Height * 1.1));
                box.Location = new Point(box.Location.X - (int)(box.Size.Width / 22), box.Location.Y - (int)(box.Size.Height / 22));
            };

            box.Click += (s, e) =>
            {
                if (box.Image == null)
                {
                    return;
                }

                if (box.Selected)
                {
                    SelectToggle_TemplateBox(box);
                    // Cannot confirm when no box is selected
                    button_Confirm.Hide();
                    button_Custom.Show();
                }
                else
                {
                    // Deselect all other boxes
                    foreach (var displayBox in displayBoxes)
                    {
                        if (displayBox != null && displayBox.Selected)
                        {
                            SelectToggle_TemplateBox(displayBox);
                        }
                    }

                    SelectToggle_TemplateBox(box);
                    selectedTemplate = availableGhostTemplates[box.Index];
                    button_Confirm.Show();
                    button_Custom.Hide();
                }
            };

            box.MouseLeave += (s, e) =>
            {
                box.Size = new Size((int)(box.Size.Width / 1.1), (int)(box.Size.Height / 1.1));
                box.Location = new Point(box.Location.X + (int)(box.Size.Width / 20), box.Location.Y + (int)(box.Size.Height / 20));
            };
        }

        private void AssignMouseEventsToBox(IndexPictureBox box)
        {
            box.MouseEnter += (s, e) =>
            {
                box.Size = new Size((int)(box.Size.Width * 1.1), (int)(box.Size.Height * 1.1));
                box.Location = new Point(box.Location.X - (int)(box.Size.Width / 22), box.Location.Y - (int)(box.Size.Height / 22));
            };

            box.Click += (s, e) =>
            {
                if (box.Image == null)
                {
                    return;
                }

                if (box.Selected)
                {
                    SelectToggle_Box(box);
                    // Cannot confirm when no box is selected
                    button_Confirm.Hide();
                }
                else
                {
                    // Deselect all other boxes
                    foreach (var displayBox in displayBoxes)
                    {
                        if (displayBox != null && displayBox.Selected)
                        {
                            SelectToggle_Box(displayBox);
                        }
                    }

                    SelectToggle_Box(box);
                    
                    if (!imageTypeConfirmed)
                    {
                        selectedImageType = availableGhostImageTypes[box.Index];
                    }
                    else if (imageTypeConfirmed && !chaseTypeConfirmed)
                    {
                        selectedChaseType = availableGhostChaseTypes[box.Index];
                    }

                    button_Confirm.Show();
                }
            };

            box.MouseLeave += (s, e) =>
            {
                box.Size = new Size((int)(box.Size.Width / 1.1), (int)(box.Size.Height / 1.1));
                box.Location = new Point(box.Location.X + (int)(box.Size.Width / 20), box.Location.Y + (int)(box.Size.Height / 20));
            };
        }

        private void UpdateDefaultLocations()
        {
            if (displayBoxes.Count >= 4)
            {
                displayBoxes_firstDefaultX = displayBoxes.First().Location.X;
                // The third index will be at the last place of the screen, button_Next should appear
                displayBoxes_lastDefaultX = displayBoxes[3].Location.X;
            }
            else
            {
                displayBoxes_firstDefaultX = displayBoxes.First().Location.X;
            }
        }

        private void form_GhostCreator_FormClosing(object sender, FormClosingEventArgs e)
        {
            eventManager.ButtonPress();
        }
    }
}
