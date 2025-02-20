using Pacman_Projection.Properties;
using NAudio.Wave;
using System.IO;
using System.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Media;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Pacman_Projection
{
    public partial class Form1 : Form
    {
        // --FRUKTER--
        // BANAN
        // KÖRSBÄR
        // ÄPPLE
        // JORDGUBBE
        // DRAKFRUKT

        const int step = 14; // Pixels per step and size of blocks (one block per step)

        const int boxesHorizontally = 30; 
        const int boxesVertically = 39;

        const int boxSize = step; 
        const int entitySize = boxSize * 2;

        const int verticalOffset = boxSize * 2;

        // Create the main array containing all boxes
        internal Box[,] boxes = new Box[boxesHorizontally, boxesVertically];
        // Food variables
        const int foodsHorizontally = 29;
        const int foodsVertically = 37;
        // Create a list for all food boxes
        internal Box[,] food = new Box[foodsHorizontally, foodsVertically];

        // Create pacman globally as to have access to his position globally
        internal PictureBox pacman = new PictureBox();

        const int horizontalFoodOffset = boxSize + boxSize / 2;
        const int verticalFoodOffset = boxSize * 3 + boxSize / 2;

        // Declare scores to get when eating different types of food
        const int foodScore = 10;
        const int foodScoreBig = 50;

        // Score and scoreLabel
        internal int score;
        internal System.Windows.Forms.Label labelScore = new System.Windows.Forms.Label();

        SoundPlayer pacman_beginning = 
            new SoundPlayer(Resources.pacman_beginning);
        SoundPlayer pacman_chomp =
            new SoundPlayer(Resources.pacman_chomp);
        SoundPlayer pacman_death =
            new SoundPlayer(Resources.pacman_death);
        SoundPlayer pacman_intermission =
            new SoundPlayer(Resources.pacman_intermission);
        SoundPlayer pacman_eatghost =
            new SoundPlayer(Resources.pacman_eatghost);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Set size of client to be the size of the boxes and to
            // match the transparent background of pacman and ghosts 
            ClientSize = new Size(boxesHorizontally * boxSize, boxesVertically * boxSize + boxSize);
            this.BackColor = Color.Black;

            // Set the location of the client to match the wall painting
            this.Location = new Point(380, 47);
            
            //
            // Create all boxes, the map
            //

            for (int horizontalIndex = 0; horizontalIndex < boxesHorizontally; horizontalIndex++)
            {
                for (int verticalIndex = 0; verticalIndex < boxesVertically; verticalIndex++)
                {
                    // Create the box
                    Box box = new Box(new PictureBox(), false, false, true, false);
                    // Box properties
                    box.pictureBox.Size = new Size(boxSize, boxSize);
                    box.pictureBox.Location = new Point(horizontalIndex * boxSize, verticalIndex * boxSize + verticalOffset);
                    box.pictureBox.BackColor = Color.Black;
                    Controls.Add(box.pictureBox);

                    if (horizontalIndex == 0 && verticalIndex == 18 || horizontalIndex == 0 && verticalIndex == 19 || horizontalIndex == 0 && verticalIndex == 20 ||
                        horizontalIndex == boxesHorizontally - 1 && verticalIndex == 18 || horizontalIndex == boxesHorizontally - 1 && verticalIndex == 19 || horizontalIndex == boxesHorizontally - 1 && verticalIndex == 20)
                    {
                        box.isTeleporter = true;
                    }

                    // Put box into the array at designated index
                    // E.g. the third box to be generated will have the index [0, 2]
                    boxes[horizontalIndex, verticalIndex] = box;
                }
            }

            // Pacman properties (+ foodBox)
            pacman.Location = new Point(boxSize * 14, boxSize * 25);
            pacman.Size = new Size(entitySize, entitySize);
            boxFood.Size = new Size(boxSize, boxSize);
            pacman.Image = Resources.Pacman_right;
            pacman.SizeMode = PictureBoxSizeMode.StretchImage;
            Controls.Add(pacman);
            Controls.Add(boxFood);
            boxFood.Hide();
            pacman.LocationChanged += Pacman_LocationChanged;
            pacman.BringToFront();
            pacman.Hide();

            // labelScore properties
            labelScore.Location = new Point(2, 2);
            labelScore.Size = new Size(100, 20);
            labelScore.FlatStyle = FlatStyle.Popup;
            labelScore.Font = new Font("Arial", 10, FontStyle.Bold);
            labelScore.ForeColor = Color.White;
            labelScore.Text = "Score: 0";
            Controls.Add(labelScore);

            // Create the play button
            Button buttonPlay = new Button();
            // playButton properties
            buttonPlay.Location = new Point(boxSize * 12, boxSize * 19);
            buttonPlay.Size = new Size(boxSize * 6, boxSize * 5);
            buttonPlay.Font = new Font("Arial", 22, FontStyle.Bold);
            buttonPlay.Text = "Play";
            buttonPlay.ForeColor = Color.Yellow;
            buttonPlay.Click += playButton_Click;
            Controls.Add(buttonPlay);
            buttonPlay.BringToFront();

            //
            // Add all the walls according to the map
            //

            // Upper wall
            for (int indexX = 0; indexX < boxesHorizontally; indexX++)
            {
                boxes[indexX, 0].isWall = true;
                boxes[indexX, 0].pictureBox.BackColor = Color.Blue;
            }
            // Left & right upper walls
            for (int indexY = 0; indexY < 12; indexY++)
            {
                boxes[0, indexY].isWall = true;
                boxes[0, indexY].pictureBox.BackColor = Color.Blue;

                boxes[boxesHorizontally - 1, indexY].isWall = true;
                boxes[boxesHorizontally - 1, indexY].pictureBox.BackColor = Color.Blue;
            }
            // Lower wall
            for (int indexX = 0; indexX < boxesHorizontally; indexX++)
            {
                boxes[indexX, boxesVertically - 2].isWall = true;
                boxes[indexX, boxesVertically - 2].pictureBox.BackColor = Color.Blue;
            }
            // Left & right lower walls
            for (int indexY = 27; indexY < boxesVertically - 1; indexY++)
            {
                boxes[0, indexY].isWall = true;
                boxes[0, indexY].pictureBox.BackColor = Color.Blue;

                boxes[boxesHorizontally - 1, indexY].isWall = true;
                boxes[boxesHorizontally - 1, indexY].pictureBox.BackColor = Color.Blue;
            }

            // Left middle walls
            for (int indexX = 0; indexX < 5; indexX++)
            {
                boxes[indexX, 12].isWall = true;
                boxes[indexX, 12].pictureBox.BackColor = Color.Blue;

                boxes[indexX, boxesHorizontally - 4].isWall = true;
                boxes[indexX, boxesHorizontally - 4].pictureBox.BackColor = Color.Blue;
            }
            for (int indexY = 12; indexY < 17; indexY++)
            {
                boxes[5, indexY].isWall = true;
                boxes[5, indexY].pictureBox.BackColor = Color.Blue;

                boxes[5, boxesHorizontally - indexY + 8].isWall = true;
                boxes[5, boxesHorizontally - indexY + 8].pictureBox.BackColor = Color.Blue;
            }
            for (int indexX = 5; indexX >= 0; indexX--)
            {
                boxes[indexX, 17].isWall = true;
                boxes[indexX, 17].pictureBox.BackColor = Color.Blue;

                boxes[indexX, 21].isWall = true;
                boxes[indexX, 21].pictureBox.BackColor = Color.Blue;
            }

            // Right middle walls
            for (int indexX = boxesHorizontally - 1; indexX > boxesHorizontally - 6; indexX--)
            {
                boxes[indexX, 12].isWall = true;
                boxes[indexX, 12].pictureBox.BackColor = Color.Blue;

                boxes[indexX, boxesHorizontally - 4].isWall = true;
                boxes[indexX, boxesHorizontally - 4].pictureBox.BackColor = Color.Blue;
            }
            for (int indexY = 12; indexY < 17; indexY++)
            {
                boxes[boxesHorizontally - 6, indexY].isWall = true;
                boxes[boxesHorizontally - 6, indexY].pictureBox.BackColor = Color.Blue;

                boxes[boxesHorizontally - 6, boxesHorizontally - indexY + 8].isWall = true;
                boxes[boxesHorizontally - 6, boxesHorizontally - indexY + 8].pictureBox.BackColor = Color.Blue;
            }
            for (int indexX = boxesHorizontally - 6; indexX < boxesHorizontally; indexX++)
            {
                boxes[indexX, 17].isWall = true;
                boxes[indexX, 17].pictureBox.BackColor = Color.Blue;

                boxes[indexX, 21].isWall = true;
                boxes[indexX, 21].pictureBox.BackColor = Color.Blue;
            }

            // Middle walls
            for (int indexY = 8; indexY < 27; indexY++)
            {
                if (indexY != 18 && indexY != 19 && indexY != 20)
                {
                    boxes[8, indexY].isWall = true;
                    boxes[8, indexY].pictureBox.BackColor = Color.Blue;

                    boxes[21, indexY].isWall = true;
                    boxes[21, indexY].pictureBox.BackColor = Color.Blue;
                }

                if (indexY == 12)
                {
                    boxes[9, indexY].isWall = true;
                    boxes[9, indexY].pictureBox.BackColor = Color.Blue;
                    boxes[10, indexY].isWall = true;
                    boxes[10, indexY].pictureBox.BackColor = Color.Blue;

                    boxes[20, indexY].isWall = true;
                    boxes[20, indexY].pictureBox.BackColor = Color.Blue;
                    boxes[19, indexY].isWall = true;
                    boxes[19, indexY].pictureBox.BackColor = Color.Blue;
                }
            }

            // Other walls

            for (int indexX = 5; indexX < 12; indexX++)
            {
                boxes[indexX, 3].isWall = true;
                boxes[indexX, 3].pictureBox.BackColor = Color.Blue;
                if (indexX == 9 || indexX == 10)
                {
                    boxes[indexX, 4].isWall = true;
                    boxes[indexX, 4].pictureBox.BackColor = Color.Blue;
                }
            }

            boxes[1, 4].isWall = true;
            boxes[1, 4].pictureBox.BackColor = Color.Blue;
            boxes[1, 5].isWall = true;
            boxes[1, 5].pictureBox.BackColor = Color.Blue;
            boxes[2, 4].isWall = true;
            boxes[2, 4].pictureBox.BackColor = Color.Blue;
            boxes[2, 5].isWall = true;
            boxes[2, 5].pictureBox.BackColor = Color.Blue;

            boxes[5, 7].isWall = true;
            boxes[5, 7].pictureBox.BackColor = Color.Blue;
            boxes[5, 8].isWall = true;
            boxes[5, 8].pictureBox.BackColor = Color.Blue;
            boxes[5, 9].isWall = true;
            boxes[5, 9].pictureBox.BackColor = Color.Blue;
            boxes[4, 9].isWall = true;
            boxes[4, 9].pictureBox.BackColor = Color.Blue;
            boxes[3, 9].isWall = true;
            boxes[3, 9].pictureBox.BackColor = Color.Blue;

            boxes[14, 1].isWall = true;
            boxes[14, 1].pictureBox.BackColor = Color.Blue;
            boxes[14, 2].isWall = true;
            boxes[14, 2].pictureBox.BackColor = Color.Blue;
            boxes[14, 3].isWall = true;
            boxes[14, 3].pictureBox.BackColor = Color.Blue;
            boxes[14, 4].isWall = true;
            boxes[14, 4].pictureBox.BackColor = Color.Blue;

            boxes[17, 4].isWall = true;
            boxes[17, 4].pictureBox.BackColor = Color.Blue;
            boxes[17, 3].isWall = true;
            boxes[17, 3].pictureBox.BackColor = Color.Blue;
            boxes[18, 3].isWall = true;
            boxes[18, 3].pictureBox.BackColor = Color.Blue;
            boxes[19, 3].isWall = true;
            boxes[19, 3].pictureBox.BackColor = Color.Blue;
            boxes[20, 3].isWall = true;
            boxes[20, 3].pictureBox.BackColor = Color.Blue;
            boxes[20, 4].isWall = true;
            boxes[20, 4].pictureBox.BackColor = Color.Blue;

            boxes[24, 4].isWall = true;
            boxes[24, 4].pictureBox.BackColor = Color.Blue;
            boxes[25, 4].isWall = true;
            boxes[25, 4].pictureBox.BackColor = Color.Blue;
            boxes[26, 4].isWall = true;
            boxes[26, 4].pictureBox.BackColor = Color.Blue;
            boxes[24, 3].isWall = true;
            boxes[24, 3].pictureBox.BackColor = Color.Blue;
            boxes[25, 3].isWall = true;
            boxes[25, 3].pictureBox.BackColor = Color.Blue;
            boxes[26, 3].isWall = true;
            boxes[26, 3].pictureBox.BackColor = Color.Blue;

            boxes[24, 7].isWall = true;
            boxes[24, 7].pictureBox.BackColor = Color.Blue;
            boxes[25, 7].isWall = true;
            boxes[25, 7].pictureBox.BackColor = Color.Blue;
            boxes[26, 7].isWall = true;
            boxes[26, 7].pictureBox.BackColor = Color.Blue;
            boxes[26, 8].isWall = true;
            boxes[26, 8].pictureBox.BackColor = Color.Blue;
            boxes[26, 9].isWall = true;
            boxes[26, 9].pictureBox.BackColor = Color.Blue;
            boxes[25, 9].isWall = true;
            boxes[25, 9].pictureBox.BackColor = Color.Blue;
            boxes[24, 9].isWall = true;
            boxes[24, 9].pictureBox.BackColor = Color.Blue;
            boxes[24, 8].isWall = true;
            boxes[24, 8].pictureBox.BackColor = Color.Blue;

            for (int indexX = 11; indexX < 19; indexX++)
            {
                boxes[indexX, 7].isWall = true;
                boxes[indexX, 7].pictureBox.BackColor = Color.Blue;

                boxes[indexX, 25].isWall = true;
                boxes[indexX, 25].pictureBox.BackColor = Color.Blue;
            }
            for (int indexX = 11; indexX < 19; indexX++)
            {
                boxes[indexX, 8].isWall = true;
                boxes[indexX, 8].pictureBox.BackColor = Color.Blue;

                boxes[indexX, 26].isWall = true;
                boxes[indexX, 26].pictureBox.BackColor = Color.Blue;
            }
            boxes[14, 9].isWall = true;
            boxes[14, 9].pictureBox.BackColor = Color.Blue;
            boxes[14, 10].isWall = true;
            boxes[14, 10].pictureBox.BackColor = Color.Blue;
            boxes[14, 11].isWall = true;
            boxes[14, 11].pictureBox.BackColor = Color.Blue;
            boxes[14, 12].isWall = true;
            boxes[14, 12].pictureBox.BackColor = Color.Blue;
            boxes[15, 9].isWall = true;
            boxes[15, 9].pictureBox.BackColor = Color.Blue;
            boxes[15, 10].isWall = true;
            boxes[15, 10].pictureBox.BackColor = Color.Blue;
            boxes[15, 11].isWall = true;
            boxes[15, 11].pictureBox.BackColor = Color.Blue;
            boxes[15, 12].isWall = true;
            boxes[15, 12].pictureBox.BackColor = Color.Blue;

            boxes[14, 27].isWall = true;
            boxes[14, 27].pictureBox.BackColor = Color.Blue;
            boxes[14, 28].isWall = true;
            boxes[14, 28].pictureBox.BackColor = Color.Blue;
            boxes[14, 29].isWall = true;
            boxes[14, 29].pictureBox.BackColor = Color.Blue;
            boxes[15, 27].isWall = true;
            boxes[15, 27].pictureBox.BackColor = Color.Blue;
            boxes[15, 28].isWall = true;
            boxes[15, 28].pictureBox.BackColor = Color.Blue;
            boxes[15, 29].isWall = true;
            boxes[15, 29].pictureBox.BackColor = Color.Blue;

            boxes[12, 16].isWall = true;
            boxes[12, 16].pictureBox.BackColor = Color.Blue;
            boxes[11, 16].isWall = true;
            boxes[11, 16].pictureBox.BackColor = Color.Blue;
            boxes[11, 17].isWall = true;
            boxes[11, 17].pictureBox.BackColor = Color.Blue;
            boxes[11, 18].isWall = true;
            boxes[11, 18].pictureBox.BackColor = Color.Blue;
            boxes[11, 19].isWall = true;
            boxes[11, 19].pictureBox.BackColor = Color.Blue;
            boxes[11, 20].isWall = true;
            boxes[11, 20].pictureBox.BackColor = Color.Blue;
            boxes[11, 21].isWall = true;
            boxes[11, 21].pictureBox.BackColor = Color.Blue;
            boxes[11, 22].isWall = true;
            boxes[11, 22].pictureBox.BackColor = Color.Blue;
            boxes[12, 22].isWall = true;
            boxes[12, 22].pictureBox.BackColor = Color.Blue;
            boxes[13, 22].isWall = true;
            boxes[13, 22].pictureBox.BackColor = Color.Blue;
            boxes[14, 22].isWall = true;
            boxes[14, 22].pictureBox.BackColor = Color.Blue;
            boxes[15, 22].isWall = true;
            boxes[15, 22].pictureBox.BackColor = Color.Blue;
            boxes[16, 22].isWall = true;
            boxes[16, 22].pictureBox.BackColor = Color.Blue;
            boxes[17, 22].isWall = true;
            boxes[17, 22].pictureBox.BackColor = Color.Blue;
            boxes[18, 22].isWall = true;
            boxes[18, 22].pictureBox.BackColor = Color.Blue;
            boxes[18, 21].isWall = true;
            boxes[18, 21].pictureBox.BackColor = Color.Blue;
            boxes[18, 20].isWall = true;
            boxes[18, 20].pictureBox.BackColor = Color.Blue;
            boxes[18, 19].isWall = true;
            boxes[18, 19].pictureBox.BackColor = Color.Blue;
            boxes[18, 18].isWall = true;
            boxes[18, 18].pictureBox.BackColor = Color.Blue;
            boxes[18, 17].isWall = true;
            boxes[18, 17].pictureBox.BackColor = Color.Blue;
            boxes[18, 16].isWall = true;
            boxes[18, 16].pictureBox.BackColor = Color.Blue;
            boxes[17, 16].isWall = true;
            boxes[17, 16].pictureBox.BackColor = Color.Blue;

            boxes[3, 29].isWall = true;
            boxes[3, 29].pictureBox.BackColor = Color.Blue;
            boxes[4, 29].isWall = true;
            boxes[4, 29].pictureBox.BackColor = Color.Blue;
            boxes[5, 29].isWall = true;
            boxes[5, 29].pictureBox.BackColor = Color.Blue;
            boxes[5, 30].isWall = true;
            boxes[5, 30].pictureBox.BackColor = Color.Blue;
            boxes[5, 31].isWall = true;
            boxes[5, 31].pictureBox.BackColor = Color.Blue;
            boxes[4, 31].isWall = true;
            boxes[4, 31].pictureBox.BackColor = Color.Blue;
            boxes[3, 31].isWall = true;
            boxes[3, 31].pictureBox.BackColor = Color.Blue;
            boxes[3, 30].isWall = true;
            boxes[3, 30].pictureBox.BackColor = Color.Blue;

            boxes[8, 29].isWall = true;
            boxes[8, 29].pictureBox.BackColor = Color.Blue;
            boxes[9, 29].isWall = true;
            boxes[9, 29].pictureBox.BackColor = Color.Blue;
            boxes[10, 29].isWall = true;
            boxes[10, 29].pictureBox.BackColor = Color.Blue;
            boxes[11, 29].isWall = true;
            boxes[11, 29].pictureBox.BackColor = Color.Blue;

            boxes[18, 29].isWall = true;
            boxes[18, 29].pictureBox.BackColor = Color.Blue;
            boxes[19, 29].isWall = true;
            boxes[19, 29].pictureBox.BackColor = Color.Blue;
            boxes[20, 29].isWall = true;
            boxes[20, 29].pictureBox.BackColor = Color.Blue;
            boxes[21, 29].isWall = true;
            boxes[21, 29].pictureBox.BackColor = Color.Blue;

            boxes[24, 29].isWall = true;
            boxes[24, 29].pictureBox.BackColor = Color.Blue;
            boxes[25, 29].isWall = true;
            boxes[25, 29].pictureBox.BackColor = Color.Blue;
            boxes[26, 29].isWall = true;
            boxes[26, 29].pictureBox.BackColor = Color.Blue;
            boxes[24, 30].isWall = true;
            boxes[24, 30].pictureBox.BackColor = Color.Blue;
            boxes[24, 31].isWall = true;
            boxes[24, 31].pictureBox.BackColor = Color.Blue;

            boxes[3, 34].isWall = true;
            boxes[3, 34].pictureBox.BackColor = Color.Blue;
            boxes[4, 34].isWall = true;
            boxes[4, 34].pictureBox.BackColor = Color.Blue;
            boxes[5, 34].isWall = true;
            boxes[5, 34].pictureBox.BackColor = Color.Blue;

            boxes[8, 32].isWall = true;
            boxes[8, 32].pictureBox.BackColor = Color.Blue;
            boxes[9, 32].isWall = true;
            boxes[9, 32].pictureBox.BackColor = Color.Blue;
            boxes[10, 32].isWall = true;
            boxes[10, 32].pictureBox.BackColor = Color.Blue;
            boxes[11, 32].isWall = true;
            boxes[11, 32].pictureBox.BackColor = Color.Blue;
            boxes[11, 33].isWall = true;
            boxes[11, 33].pictureBox.BackColor = Color.Blue;
            boxes[11, 34].isWall = true;
            boxes[11, 34].pictureBox.BackColor = Color.Blue;
            boxes[10, 34].isWall = true;
            boxes[10, 34].pictureBox.BackColor = Color.Blue;
            boxes[9, 34].isWall = true;
            boxes[9, 34].pictureBox.BackColor = Color.Blue;
            boxes[8, 34].isWall = true;
            boxes[8, 34].pictureBox.BackColor = Color.Blue;
            boxes[8, 33].isWall = true;
            boxes[8, 33].pictureBox.BackColor = Color.Blue;

            boxes[14, 32].isWall = true;
            boxes[14, 32].pictureBox.BackColor = Color.Blue;
            boxes[14, 33].isWall = true;
            boxes[14, 33].pictureBox.BackColor = Color.Blue;
            boxes[14, 34].isWall = true;
            boxes[14, 34].pictureBox.BackColor = Color.Blue;
            boxes[15, 34].isWall = true;
            boxes[15, 34].pictureBox.BackColor = Color.Blue;
            boxes[15, 33].isWall = true;
            boxes[15, 33].pictureBox.BackColor = Color.Blue;
            boxes[15, 32].isWall = true;
            boxes[15, 32].pictureBox.BackColor = Color.Blue;
            boxes[16, 32].isWall = true;
            boxes[16, 32].pictureBox.BackColor = Color.Blue;
            boxes[17, 32].isWall = true;
            boxes[17, 32].pictureBox.BackColor = Color.Blue;

            boxes[20, 32].isWall = true;
            boxes[20, 32].pictureBox.BackColor = Color.Blue;
            boxes[21, 32].isWall = true;
            boxes[21, 32].pictureBox.BackColor = Color.Blue;
            boxes[21, 33].isWall = true;
            boxes[21, 33].pictureBox.BackColor = Color.Blue;
            boxes[20, 33].isWall = true;
            boxes[20, 33].pictureBox.BackColor = Color.Blue;
            boxes[20, 34].isWall = true;
            boxes[20, 34].pictureBox.BackColor = Color.Blue;
            boxes[21, 34].isWall = true;
            boxes[21, 34].pictureBox.BackColor = Color.Blue;
            boxes[22, 34].isWall = true;
            boxes[22, 34].pictureBox.BackColor = Color.Blue;
            boxes[23, 34].isWall = true;
            boxes[23, 34].pictureBox.BackColor = Color.Blue;
            boxes[24, 34].isWall = true;
            boxes[24, 34].pictureBox.BackColor = Color.Blue;
            boxes[25, 34].isWall = true;
            boxes[25, 34].pictureBox.BackColor = Color.Blue;

            boxes[28, 32].isWall = true;
            boxes[28, 32].pictureBox.BackColor = Color.Blue;

            //
            // Set all walls not to contain food's bool "toContainFood" to false
            //

            // Walls
            for (int horizontalIndex = 0; horizontalIndex < boxesHorizontally; horizontalIndex++)
            {
                for (int verticalIndex = 0; verticalIndex < boxesVertically; verticalIndex++)
                {
                    if (boxes[horizontalIndex, verticalIndex].isWall == true)
                    {
                        boxes[horizontalIndex, verticalIndex].toContainFood = false;
                    }
                }
            }

            // Others
            boxes[1, 3].toContainFood = false;
            boxes[2, 3].toContainFood = false;
            boxes[5, 4].toContainFood = false;
            boxes[7, 4].toContainFood = false;
            boxes[11, 4].toContainFood = false;
            boxes[21, 4].toContainFood = false;
            boxes[21, 7].toContainFood = false;
            boxes[21, 3].toContainFood = false;
            boxes[4, 8].toContainFood = false;
            boxes[3, 8].toContainFood = false;
            boxes[8, 7].toContainFood = false;
            boxes[9, 11].toContainFood = false;
            boxes[10, 11].toContainFood = false;
            boxes[9, 13].toContainFood = false;
            boxes[10, 13].toContainFood = false;
            boxes[13, 9].toContainFood = false;
            boxes[13, 11].toContainFood = false;
            boxes[13, 13].toContainFood = false;
            boxes[14, 13].toContainFood = false;
            boxes[15, 13].toContainFood = false;
            boxes[16, 13].toContainFood = false;
            boxes[16, 11].toContainFood = false;
            boxes[16, 9].toContainFood = false;
            boxes[19, 11].toContainFood = false;
            boxes[20, 11].toContainFood = false;
            boxes[19, 13].toContainFood = false;
            boxes[20, 13].toContainFood = false;
            boxes[19, 4].toContainFood = false;
            boxes[18, 4].toContainFood = false;
            boxes[0, 13].toContainFood = false;
            boxes[0, 14].toContainFood = false;
            boxes[0, 16].toContainFood = false;
            boxes[2, 13].toContainFood = false;
            boxes[2, 14].toContainFood = false;
            boxes[2, 16].toContainFood = false;
            boxes[4, 13].toContainFood = false;
            boxes[4, 14].toContainFood = false;
            boxes[4, 16].toContainFood = false;
            boxes[0, 18].toContainFood = false;
            boxes[0, 20].toContainFood = false;
            boxes[2, 18].toContainFood = false;
            boxes[2, 20].toContainFood = false;
            boxes[3, 18].toContainFood = false;
            boxes[3, 20].toContainFood = false;
            boxes[5, 18].toContainFood = false;
            boxes[5, 20].toContainFood = false;
            boxes[0, 22].toContainFood = false;
            boxes[0, 23].toContainFood = false;
            boxes[0, 25].toContainFood = false;
            boxes[2, 22].toContainFood = false;
            boxes[2, 23].toContainFood = false;
            boxes[2, 25].toContainFood = false;
            boxes[4, 22].toContainFood = false;
            boxes[4, 23].toContainFood = false;
            boxes[4, 25].toContainFood = false;
            boxes[8, 18].toContainFood = false;

            boxes[12, 17].toContainFood = false;
            boxes[14, 17].toContainFood = false;
            boxes[14, 16].toContainFood = false;
            boxes[16, 16].toContainFood = false;
            boxes[7, 4].toContainFood = false;
            boxes[16, 17].toContainFood = false;
            boxes[12, 19].toContainFood = false;
            boxes[14, 19].toContainFood = false;
            boxes[16, 19].toContainFood = false;
            boxes[12, 21].toContainFood = false;
            boxes[14, 21].toContainFood = false;
            boxes[16, 21].toContainFood = false;

            boxes[21, 18].toContainFood = false;
            boxes[25, 13].toContainFood = false;
            boxes[25, 14].toContainFood = false;
            boxes[25, 16].toContainFood = false;
            boxes[27 ,13].toContainFood = false;
            boxes[27, 14].toContainFood = false;
            boxes[27, 16].toContainFood = false;
            boxes[29, 13].toContainFood = false;
            boxes[29, 14].toContainFood = false;
            boxes[29, 16].toContainFood = false;
            boxes[24, 18].toContainFood = false;
            boxes[24, 20].toContainFood = false;
            boxes[26, 18].toContainFood = false;
            boxes[26, 20].toContainFood = false;
            boxes[27, 18].toContainFood = false;
            boxes[27, 20].toContainFood = false;
            boxes[29, 18].toContainFood = false;
            boxes[29, 20].toContainFood = false;
            boxes[25, 22].toContainFood = false;
            boxes[25, 23].toContainFood = false;
            boxes[25, 25].toContainFood = false;
            boxes[27, 22].toContainFood = false;
            boxes[27 ,23].toContainFood = false;
            boxes[27, 25].toContainFood = false;
            boxes[29, 22].toContainFood = false;
            boxes[29, 23].toContainFood = false;
            boxes[29, 25].toContainFood = false;
            boxes[25, 30].toContainFood = false;
            boxes[25, 31].toContainFood = false;
            boxes[28, 33].toContainFood = false;
            boxes[28, 34].toContainFood = false;
            boxes[16, 33].toContainFood = false;
            boxes[17, 34].toContainFood = false;

            //
            // Food
            //

            // For loop which fills the food list and places it on the map while
            // checking if it collides with any walls, if so, they are removed
            for (int indexY = 0; indexY < foodsVertically; indexY++)
            {
                for (int indexX = 0; indexX < foodsHorizontally; indexX++)
                {
                    // PUT MORE BIG FOOD HERE 
                    if (indexX == 0 && indexY == 0
                     || indexX == 26 && indexY == 0
                     || indexX == 0 && indexY == 34
                     || indexX == 26 && indexY == 34
                     )
                    {
                        food[indexX, indexY] = new Box(new PictureBox(), false, false, true, true);
                        food[indexX, indexY].pictureBox.Image = Resources.FoodBig;
                    }
                    else
                    {
                        food[indexX, indexY] = new Box(new PictureBox(), false, false, true, false);
                        food[indexX, indexY].pictureBox.Image = Resources.Food;
                    }

                    // Food properties
                    food[indexX, indexY].pictureBox.Size = new Size(boxSize, boxSize);

                    // Place all foods in a grid-pattern over the map
                    // If a food collides with a wall, it will be removed
                    // The same applies to foods that are placed beside others foods,
                    // creating areas of dense foods, as well as foods placed outside the map or generally where they are not supposed to be
                    food[indexX, indexY].pictureBox.Location = new Point(indexX * boxSize + horizontalFoodOffset, indexY * boxSize + verticalFoodOffset);

                    if (AbleToPlaceFood(indexX, indexY))
                    {
                        Controls.Add(food[indexX, indexY].pictureBox);
                        food[indexX, indexY].pictureBox.BringToFront();
                    }
                    else
                    {
                        food[indexX, indexY] = null;
                    }
                }
            }
        }

        // Each time pacman is moved he needs to be moved to the front
        private void Pacman_LocationChanged(object sender, EventArgs e)
        {
            pacman.BringToFront();
        }

        private async void playButton_Click(object sender, EventArgs e)
        {
            // Create labelReady
            System.Windows.Forms.Label labelReady = new System.Windows.Forms.Label();
            // labelReady properties
            labelReady.Location = new Point(boxSize * 11, boxSize * 15);
            labelReady.Size = new Size(boxSize * 9, boxSize * 3);
            labelReady.Font = new Font("Arial", 22, FontStyle.Bold);
            labelReady.Text = "Ready!";
            labelReady.ForeColor = Color.Yellow;
            labelReady.BackColor = Color.Transparent;
            Controls.Add(labelReady);
            labelReady.BringToFront();

            // Show pacman and bring him to the front
            pacman.Show();
            pacman.BringToFront();
            // Remove playButton from controls, making it invisible
            Controls.Remove((Control)sender);

            // Play sound asynchronously without freezing UI
            await Task.Run(() => pacman_beginning.PlaySync());

            // After pacman_beginning has played, hide labelReady and start timers
            labelReady.Hide();
            pacTickTimer.Start();
            pacImageTimer.Start();
        }

        string currentKey = "";
        string latestKey = "";

        private void View_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) {
                currentKey = "Left";
            } 
            else if (e.KeyCode == Keys.Right) {
                currentKey = "Right";
            }
            else if (e.KeyCode == Keys.Up) {
                currentKey = "Up";
            }
            else if (e.KeyCode == Keys.Down) {
                currentKey = "Down";
            }
        }

        bool chompIsPlaying;
        bool intermissionIsPlaying;

        private void FoodEaten(int indexX, int indexY, bool bigFood) 
        {
            // Don't play chomp if intermission is playing
            if (!bigFood)
            { 
                if (!chompIsPlaying)
                {
                    // set chompIsPlaying to true
                    // This is so chomp cannot be queued and played multiple times
                    chompIsPlaying = true;
                    Task.Run(() => 
                    {
                        // After chomp has finished playing, chompIsPlaying is set to false 
                        pacman_chomp.PlaySync();
                        chompIsPlaying = false;
                    });
                }

                // Remove the eaten food and increase score by the relevant value
                Controls.Remove(food[indexX, indexY].pictureBox);
                food[indexX, indexY] = null;

                score += foodScore;
                labelScore.Text = "Score: " + score;
            }
            else if (bigFood)
            {
                intermissionIsPlaying = true;
                Task.Run(() =>
                {
                    pacman_intermission.PlaySync();
                    intermissionIsPlaying = false;
                });

                Controls.Remove(food[indexX, indexY].pictureBox);
                food[indexX, indexY] = null;

                score += foodScoreBig;
                labelScore.Text = "Score: " + score;
            }
        }

        bool teleportedLastTick;
        bool teleporting;
        int blocksIntoTeleporter;
        PictureBox boxFood = new PictureBox();

        private void pacTickTimer_Tick(object sender, EventArgs e)
        {
            bool canChangeDirection = false;

            //////////////////
            //2     //     3//        
            //  //////////  //             
            //  //  //  //  //      
            ///////Food/////// 
            //  //  //  //  //     
            //  //////////  //      
            //1     //     4//      
            //////////////////

            //(Pacman is 2*2 boxes big)

            // Check if pacman can change direciton 
            // If currentKey is different from latestKey, check if pacman can change direction
            // If pacman is, for instance, going through a corridor with walls below and above, he
            // cannot change direction to up or down because there are walls there
            
            if (currentKey != latestKey)
            {
                if (currentKey == "Left")
                {
                    int box1X = (pacman.Left - boxSize) / boxSize;
                    int box1Y = (pacman.Top - boxSize) / boxSize;

                    int box2X = box1X;
                    int box2Y = box1Y - 1;

                    if (!CheckForWall(box1X, box1Y, box2X, box2Y))
                    {
                        latestKey = currentKey;
                    }
                    else
                    {
                        canChangeDirection = false;
                    }
                }
                else if (currentKey == "Right")
                {
                    int box1X = (pacman.Left + boxSize) / boxSize;
                    int box1Y = (pacman.Top - boxSize) / boxSize;

                    int box3X = box1X + 1;
                    int box3Y = box1Y - 1;

                    int box4X = box1X + 1;
                    int box4Y = box1Y;

                    if (!CheckForWall(box3X, box3Y, box4X, box4Y))
                    {
                        latestKey = currentKey;
                    }
                    else
                    {
                        canChangeDirection = false;
                    }
                }
                else if (currentKey == "Up")
                {
                    int box1X = pacman.Left / boxSize;
                    int box1Y = (pacman.Top - boxSize * 2) / boxSize;

                    int box2X = box1X;
                    int box2Y = box1Y - 1;

                    int box3X = box1X + 1;
                    int box3Y = box1Y - 1;

                    if (!CheckForWall(box2X, box2Y, box3X, box3Y))
                    {
                        latestKey = currentKey;
                    }
                    else
                    {
                        canChangeDirection = false;
                    }
                }
                else if (currentKey == "Down")
                {
                    int box1X = pacman.Left / boxSize;
                    int box1Y = pacman.Top / boxSize;

                    int box4X = box1X + 1;
                    int box4Y = box1Y;

                    if (!CheckForWall(box1X, box1Y, box4X, box4Y))
                    {
                        latestKey = currentKey;
                    }
                    else
                    {
                        canChangeDirection = false;
                    }
                }
            }


            // If the boxes in front of pacman, based on his direction, are not walls, move pacman
            // The directions "Left" and "Right" also check for teleporter boxes to the left and right of the map
            // Also check if boxFood is intersecting with a box with food, if so, change the aforementioned box's picture and increase score by foodScore
            if (latestKey == "Left")
            {
                int box1X = (pacman.Left - boxSize) / boxSize;
                int box1Y = (pacman.Top - boxSize) / boxSize;

                int box2X = box1X;
                int box2Y = box1Y - 1;

                // Check if pacman is inside teleporter box 
                if (CheckForTeleporter(box1X, box1Y, box2X, box2Y) && teleportedLastTick == false || teleporting == true)
                {
                    teleporting = true;
                    pacman.Left -= step;
                    blocksIntoTeleporter++;
                    if (blocksIntoTeleporter == 3)
                    {
                        teleporting = false;
                        pacman.Left = boxesHorizontally * boxSize;
                        teleportedLastTick = true;
                        blocksIntoTeleporter = 0;
                    }
                }
                else if (!CheckForWall(box1X, box1Y, box2X, box2Y) && teleporting == false)
                {
                    pacman.Left -= step;
                    if (teleportedLastTick == true)
                    {
                        teleportedLastTick = false;
                    }
                }
                else
                {
                    latestKey = "";
                }
            }
            else if (latestKey == "Right")
            { 
                int box1X = (pacman.Left + boxSize) / boxSize;
                int box1Y = (pacman.Top - boxSize) / boxSize;

                int box3X = box1X + 1;
                int box3Y = box1Y - 1;

                int box4X = box1X + 1;
                int box4Y = box1Y;
              
                if (CheckForTeleporter(box3X, box3Y, box4X, box4Y) && teleportedLastTick == false || teleporting == true)
                {
                    teleporting = true;
                    pacman.Left += step;
                    blocksIntoTeleporter++;
                    if (blocksIntoTeleporter == 3)
                    {
                        teleporting = false; 
                        pacman.Left = -boxSize * 2;
                        teleportedLastTick = true;
                        blocksIntoTeleporter = 0;
                    }
                }
                else if (!CheckForWall(box3X, box3Y, box4X, box4Y) && teleporting == false)
                {
                    pacman.Left += step;
                    if (teleportedLastTick == true)
                    {
                        teleportedLastTick = false;
                    }
                }
                else
                {
                    latestKey = "";
                }
            }
            else if (latestKey == "Up")
            {
                int box1X = pacman.Left / boxSize;
                int box1Y = (pacman.Top - boxSize*2) / boxSize;
                
                int box2X = box1X;
                int box2Y = box1Y - 1;

                int box3X = box1X + 1;
                int box3Y = box1Y - 1;

                if (!CheckForWall(box2X, box2Y, box3X, box3Y))
                {
                    pacman.Top -= step;
                }
                else
                {
                    latestKey = "";
                }    
            }
            else if (latestKey == "Down")
            {   
                int box1X = pacman.Left / boxSize;
                int box1Y = pacman.Top / boxSize;   

                int box4X = box1X + 1;
                int box4Y = box1Y;

                if (!CheckForWall(box1X, box1Y, box4X, box4Y))
                {
                    pacman.Top += step;
                }
                else
                {
                    latestKey = "";
                }
            }

            // Move boxFood to the appropriate location before checking foodCollide
            boxFood.Location = new Point(pacman.Left + boxSize / 2, pacman.Top + boxSize / 2);

            // Check if boxFood (Pacman) is intersecting with food
            // FoodEaten(false) == food
            // FoodEaten(true) == big food
            if (latestKey != "")
            {
                // Send the index of the food that boxFood is colliding
                // with (the food that is eaten) to FoodEaten to be removed accordingly 
                if (CheckForFoodCollide(boxFood) == (true, false))
                {
                    FoodEaten((pacman.Left + boxSize / 2) / boxSize - horizontalFoodOffset / boxSize, 
                             (pacman.Top + boxSize / 2) / boxSize - verticalFoodOffset / boxSize, false);
                }
                else if (CheckForFoodCollide(boxFood) == (true, true))
                {
                    FoodEaten((pacman.Left + boxSize / 2) / boxSize - horizontalFoodOffset / boxSize, 
                             (pacman.Top + boxSize / 2) / boxSize - verticalFoodOffset / boxSize, true);
                }
            }


            // If pacman can change direction, latestKey is updated
            if (canChangeDirection == true)
            {
                latestKey = currentKey;
            }
        }

        private bool CheckForWall(int box1X, int box1Y, int box2X, int box2Y)
        {
            // True == wall
            // False == no wall

            try
            {
                if (boxes[box1X, box1Y].isWall ||boxes[box2X, box2Y].isWall)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return true;
            }
        }

        private bool AbleToPlaceFood(int indexXfood, int indexYfood)
        {
            // True == To contain food
            // False == Not to contain food

            for (int indexX = 0; indexX < boxesHorizontally; indexX++)
            {
                for (int indexY = 0; indexY < boxesVertically; indexY++)
                {
                    if (food[indexXfood, indexYfood].pictureBox.Bounds.IntersectsWith(boxes[indexX, indexY].pictureBox.Bounds))
                    {
                        if (boxes[indexX, indexY].isWall || boxes[indexX, indexY].toContainFood == false) 
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool CheckForTeleporter(int box1X, int box1Y, int box2X, int box2Y)
        {
            // True == teleporter
            // False == no teleporter
            try
            {
                if (boxes[box1X, box1Y].isTeleporter && boxes[box2X, box2Y].isTeleporter)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }
        private (bool food, bool bigFood) CheckForFoodCollide(PictureBox foodBox) 
        {
            // true, false == food
            // true, true == bigFood
            try
            {
                for (int indexX = 0; indexX < foodsHorizontally; indexX++)
                {
                    for (int indexY = 0; indexY < foodsVertically; indexY++)
                    {
                        if (food[indexX, indexY] != null)
                        {
                            if (food[indexX, indexY].pictureBox.Bounds.IntersectsWith(foodBox.Bounds))
                            {
                                if (food[indexX, indexY].isBigFood == false)
                                {
                                    return (true, false);
                                }
                                else
                                { 
                                    return (true, true);
                                }
                            }
                        }
                    }
                }
                return (false, false);
            }
            catch (Exception) 
            {
                return (false, false);
            }
        }


        internal bool isOpenPicture;
        private void pacImageTimer_Tick(object sender, EventArgs e)
        {
            isOpenPicture = !isOpenPicture;

            if (latestKey == "Left")
            {
                if (isOpenPicture)
                {
                    pacman.Image = Resources.Pacman_left;
                }
                else
                {
                    pacman.Image = Resources.Pacman_left_closed;
                }
            }
            else if (latestKey == "Right")
            {
                if (isOpenPicture)
                {
                    pacman.Image = Resources.Pacman_right;
                }
                else
                {
                    pacman.Image = Resources.Pacman_right_closed;
                }
            }
            else if (latestKey == "Up")
            {
                if (isOpenPicture)
                {
                    pacman.Image = Resources.Pacman_up;
                }
                else
                {
                    pacman.Image = Resources.Pacman_up_closed;
                }
            }
            else if (latestKey == "Down")
            {

                if (isOpenPicture)
                {
                    pacman.Image = Resources.Pacman_down;
                }
                else
                {
                    pacman.Image = Resources.Pacman_down_closed;
                }
            }
        }
    }
}
