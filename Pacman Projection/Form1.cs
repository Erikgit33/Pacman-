﻿using Pacman_Projection.Properties;
//using NAudio.Wave;
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
using System.CodeDom;

namespace Pacman_Projection
{
    public partial class Form1 : Form
    {
        const int step = 14; // Pixels per step and size of blocks (one block per step)
        // Declare boxesHorizontally and boxesVertically
        const int boxesHorizontally = 30; 
        const int boxesVertically = 39;
        // Declare boxSize to be as big as step, and entitySize to be wtice that
        const int boxSize = step; 
        const int entitySize = boxSize * 2;
        // Declare verticalOffset for the boxes (the map)
        const int verticalOffset = boxSize * 2;

        // Declare the interval for pacTickTimer and ghostTickTimer, that is essentiallyt their speed
        internal int pacTickTimerInterval = 200;
        internal int ghostTickTimerInterval = 200;

        // Create array containing all boxes
        internal Box[,] boxes = new Box[boxesHorizontally, boxesVertically];
        // Create Pacman, his start coords, and his life list containing three "lives"
        internal Pacman pacman = new Pacman(new PictureBox());
        const int pacman_StartX = boxSize*14;
        const int pacman_StartY = boxSize*25;

        internal List<Box> pacmanLives = new List<Box> 
        { 
            new Box(new PictureBox(), false, false, false, false),
            new Box(new PictureBox(), false, false, false, false),
            new Box(new PictureBox(), false, false, false, false)
        };
        // Create list containing the ghosts
        internal List<Ghost> ghosts = new List<Ghost>();
        // Create ghosts and declare their respective starting coordinates
        internal Ghost Blinky;
        const int Blinky_StartX = boxSize*14;
        const int Blinky_StartY = boxSize*16;
        internal Ghost Pinky;
        const int Pinky_StartX = boxSize*14;
        const int Pinky_StartY = boxSize*20;
        internal Ghost Inky;
        const int Inky_StartX = boxSize*12;
        const int Inky_StartY = boxSize*20;
        internal Ghost Clyde;
        const int Clyde_StartX = boxSize*16;
        const int Clyde_StartY = boxSize*20;

        // Declare foodsHorizontally and foodsVertically 
        const int foodsHorizontally = 29;
        const int foodsVertically = 37;
        // Create food array
        internal Box[,] food = new Box[foodsHorizontally, foodsVertically];
        // Create list for all big food indexes
        List<string> bigFoodIndexes = new List<string>();

        // Declare food offset variables
        const int horizontalFoodOffset = boxSize + boxSize / 2;
        const int verticalFoodOffset = boxSize * 3 + boxSize / 2;

        // Declare scores to get when eating different types of food and amount of foods placed
        const int foodScore = 10;
        const int foodScoreBig = 50;
        const int fruitScore = 100;
        const int ghostScore = 200;
        int foodsOnMap = 0;

        const int msToWaitAfterGhosts = 2800;
        const int msToWaitBetweenGames = 1500;
        const int msToWaitBeforeRestart = 700;

        // Declare score and scoreLabel
        internal int score;
        internal System.Windows.Forms.Label labelScore = new System.Windows.Forms.Label();

        SoundPlayer pacman_beginning = 
            new SoundPlayer(Resources.pacman_beginning);
        SoundPlayer pacman_chomp =
            new SoundPlayer(Resources.pacman_chomp);
        SoundPlayer pacman_death =
            new SoundPlayer(Resources.pacman_death);
        SoundPlayer ghost_scared =
            new SoundPlayer(Resources.ghost_scared);
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

            // Set timerIntervals to designated interval
            pacTickTimer.Interval = pacTickTimerInterval;
            ghostTickTimer.Interval = ghostTickTimerInterval;
            
            //
            // Create all boxes
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
            pacman.box.Location = new Point(pacman_StartX, pacman_StartY);
            pacman.box.Size = new Size(entitySize, entitySize);
            boxFood.Size = new Size(boxSize, boxSize);
            pacman.box.Image = Resources.Pacman_start;
            pacman.box.SizeMode = PictureBoxSizeMode.StretchImage;
            pacman.box.LocationChanged += pacman_LocationChanged;
            Controls.Add(pacman.box);
            Controls.Add(boxFood);
            boxFood.Hide();
            pacman.box.BringToFront();
            pacman.box.Hide();

            for (int indexLife = 0; indexLife < pacmanLives.Count; indexLife++)
            {
                pacmanLives[indexLife].pictureBox.Image = Resources.Pacman_Life;
                pacmanLives[indexLife].pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                pacmanLives[indexLife].pictureBox.Location = new Point((int)(boxSize * 23 + boxSize * indexLife * 2.2), boxSize*0);
                pacmanLives[indexLife].pictureBox.Size = new Size(entitySize, entitySize);
                Controls.Add(pacmanLives[indexLife].pictureBox);
            }

            // Ghosts properties

            // Blinky
            Blinky = new Ghost(new PictureBox(), false, false, false);
            Blinky.box.Size = new Size(entitySize, entitySize);
            Blinky.box.Image = Resources.Blinky_stationary;
            Blinky.box.Location = new Point(Blinky_StartX, Blinky_StartY);
            Controls.Add(Blinky.box);
            Blinky.box.BringToFront();
            Blinky.box.Hide();
            ghosts.Add(Blinky);

            // Pinky
            Pinky = new Ghost(new PictureBox(), false, false, false);
            Pinky.box.Size = new Size(entitySize, entitySize);
            Pinky.box.Image = Resources.Pinky_stationary;
            Pinky.box.Location = new Point(Pinky_StartX, Pinky_StartY);
            Controls.Add(Pinky.box);
            Pinky.box.BringToFront();
            Pinky.box.Hide();
            ghosts.Add(Pinky);

            // Inky
            Inky = new Ghost(new PictureBox(), false, false, false);
            Inky.box.Size = new Size(entitySize, entitySize);
            Inky.box.Image = Resources.Inky_stationary;
            Inky.box.Location = new Point(Inky_StartX, Inky_StartY);
            Controls.Add(Inky.box);
            Inky.box.BringToFront();
            Inky.box.Hide();
            ghosts.Add(Inky);

            // Clyde
            Clyde = new Ghost(new PictureBox(), false, false, false);
            Clyde.box.Size = new Size(entitySize, entitySize);
            Clyde.box.Image = Resources.Clyde_stationary;
            Clyde.box.Location = new Point(Clyde_StartX, Clyde_StartY);
            Controls.Add(Clyde.box);
            Clyde.box.BringToFront();
            Clyde.box.Hide();
            ghosts.Add(Clyde);

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

            // Make the "gate" into the ghosts encolsure
            boxes[13, 16].isGate = true;
            boxes[13, 16].pictureBox.BackColor = Color.LightPink;
            boxes[14, 16].isGate = true;
            boxes[14, 16].pictureBox.BackColor = Color.LightPink;
            boxes[15, 16].isGate = true;
            boxes[15, 16].pictureBox.BackColor = Color.LightPink;
            boxes[16, 16].isGate = true;
            boxes[16, 16].pictureBox.BackColor = Color.LightPink;

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
                    if (indexX == 0 && indexY == 0
                     || indexX == 26 && indexY == 0
                     || indexX == 0 && indexY == 34
                     || indexX == 26 && indexY == 34)
                    {
                        food[indexX, indexY] = new Box(new PictureBox(), false, false, true, true);
                        food[indexX, indexY].pictureBox.Image = Resources.FoodBig;
                        // Add big food index to the list for use "pacTickTimer" method
                        bigFoodIndexes.Add(indexX.ToString() + "_" + indexY.ToString());
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
                        foodsOnMap++;
                    }
                    else
                    {
                        food[indexX, indexY] = null;
                    }
                }
            }

            // TEST
            Blinky.SetDirection("Left");
            Pinky.SetDirection("Down");
            Inky.SetDirection("Up");
            Clyde.SetDirection("Up");
        }

        private async void playButton_Click(object sender, EventArgs e)
        {
            // Create labelReady
            System.Windows.Forms.Label labelReady = new System.Windows.Forms.Label();
            // labelReady properties
            labelReady.Location = new Point(boxSize * 11, boxSize * 11);
            labelReady.Size = new Size(boxSize * 9, boxSize * 3);
            labelReady.Font = new Font("Pixelify Sans", 22, FontStyle.Bold);
            labelReady.Text = "Ready!";
            labelReady.ForeColor = Color.Yellow;
            labelReady.BackColor = Color.Transparent;
            Controls.Add(labelReady);
            labelReady.BringToFront();

            // Show pacman and bring him to the front
            pacman.box.Show();
            pacman.box.BringToFront();
            // Remove playButton from controls, making it invisible
            Controls.Remove((Control)sender);

            Task.Run(() => pacman_beginning.PlaySync());
            // Wait for 'msToWaitBetweenGames' milliseconds before showing ghosts
            await Task.Delay(msToWaitBetweenGames);

            Blinky.box.Show();
            Blinky.box.BringToFront();
            Pinky.box.Show();
            Pinky.box.BringToFront();
            Inky.box.Show();
            Inky.box.BringToFront();
            Clyde.box.Show();
            Clyde.box.BringToFront();

            // Timed to be complete when pacman_beginning has finished playing
            await Task.Delay(msToWaitAfterGhosts);

            // Hide labelReady and start timers
            labelReady.Hide();
            pacTickTimer.Start();
            pacImageTimer.Start();
            ghostTickTimer.Start();
            ghostImageTimer.Start();
            bigFoodBlinkTimer.Start();
        }

        private async void Game(bool win)
        {
            pacTickTimer.Stop();
            pacImageTimer.Stop();
            ghostTickTimer.Stop();
            ghostImageTimer.Stop();
            bigFoodBlinkTimer.Stop();

            Blinky.box.Image = Resources.Blinky_stationary;
            Pinky.box.Image = Resources.Pinky_stationary;
            Inky.box.Image = Resources.Inky_stationary;
            Clyde.box.Image = Resources.Clyde_stationary;

            await Task.Delay(msToWaitBetweenGames);

            Blinky.box.Hide();
            Pinky.box.Hide();
            Inky.box.Hide();
            Clyde.box.Hide();

            if (win)
            {
                // WIN
            }
            else
            {
                Restart();
            }
        }
        private async void Restart()
        {
            Blinky.box.Location = new Point(Blinky_StartX, Blinky_StartY);
            Blinky.box.Location = new Point(Blinky_StartX, Blinky_StartY);
            Blinky.box.Location = new Point(Blinky_StartX, Blinky_StartY);
            Blinky.box.Location = new Point(Blinky_StartX, Blinky_StartY);
            pacman.box.Location = new Point(pacman_StartX, pacman_StartY);

            await Task.Delay(msToWaitBeforeRestart);

            pacTickTimer.Start();
            pacImageTimer.Start();
            ghostTickTimer.Start();
            ghostTickTimer.Start();
            bigFoodBlinkTimer.Start();
        }

        //
        // Pacman & movement related methods
        //

        internal bool pacPic_open;
        private void pacImageTimer_Tick(object sender, EventArgs e)
        {
            pacPic_open = !pacPic_open;

            if (latestKey == "Left")
            {
                if (pacPic_open)
                {
                    pacman.box.Image = Resources.Pacman_left;
                }
                else
                {
                    pacman.box.Image = Resources.Pacman_left_closed;
                }
            }
            else if (latestKey == "Right")
            {
                if (pacPic_open)
                {
                    pacman.box.Image = Resources.Pacman_right;
                }
                else
                {
                    pacman.box.Image = Resources.Pacman_right_closed;
                }
            }
            else if (latestKey == "Up")
            {
                if (pacPic_open)
                {
                    pacman.box.Image = Resources.Pacman_up;
                }
                else
                {
                    pacman.box.Image = Resources.Pacman_up_closed;
                }
            }
            else if (latestKey == "Down")
            {

                if (pacPic_open)
                {
                    pacman.box.Image = Resources.Pacman_down;
                }
                else
                {
                    pacman.box.Image = Resources.Pacman_down_closed;
                }
            }
        }

        // Every time pacman moves, check if he moves into a ghost, thus losing the game
        private void pacman_LocationChanged(object sencer, EventArgs e)
        {
            if (!ghostScaredIsPlaying)
            {
                for (int index = 0; index < ghosts.Count; index++)
                {
                    if (pacman.box.Bounds.IntersectsWith(ghosts[index].box.Bounds))
                    {
                        Game(false);
                    }
                }
            }
            else
            {
                for (int index = 0; index < ghosts.Count; index++)
                {
                    if (pacman.box.Bounds.IntersectsWith(ghosts[index].box.Bounds))
                    {
                        if (ghosts[index].Equals(Blinky))
                        {
                            GhostEaten(Blinky);
                        }
                        else if (ghosts[index].Equals(Pinky))
                        {
                            GhostEaten(Pinky);
                        }
                        else if (ghosts[index].Equals(Inky))
                        {
                            GhostEaten(Inky);
                        }
                        else if (ghosts[index].Equals(Clyde))
                        {
                            GhostEaten(Clyde);
                        }
                    }
                }
            }
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
            else if (e.KeyCode == Keys.Q)
            {
                MessageBox.Show(foodsOnMap.ToString());
            }
        }

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
            // If currentKey (key pressed, to be registered) is different from latestKey (key currently registered),
            // check if pacman can change direction. If pacman is, for instance, going through a corridor with
            // walls below and above, he cannot change direction to up or down because there are walls there
            // If the player presses the up key during this situation, pacman will travel in the latestKey-direction
            // until he can change to the currentKey-direction. 
            
            if (currentKey != latestKey)
            {
                if (currentKey == "Left")
                {
                    int box1X = (pacman.box.Left - boxSize) / boxSize;
                    int box1Y = (pacman.box.Top - boxSize) / boxSize;

                    int box2X = box1X;
                    int box2Y = box1Y - 1;

                    if (!CheckForWall(box1X, box1Y, box2X, box2Y) && !CheckForGate(box1X, box1Y, box2X, box2Y))
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
                    int box1X = (pacman.box.Left + boxSize) / boxSize;
                    int box1Y = (pacman.box.Top - boxSize) / boxSize;

                    int box3X = box1X + 1;
                    int box3Y = box1Y - 1;

                    int box4X = box1X + 1;
                    int box4Y = box1Y;

                    if (!CheckForWall(box3X, box3Y, box4X, box4Y) && !CheckForGate(box3X, box3Y, box4X, box4Y))
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
                    int box1X = pacman.box.Left / boxSize;
                    int box1Y = (pacman.box.Top - boxSize * 2) / boxSize;

                    int box2X = box1X;
                    int box2Y = box1Y - 1;

                    int box3X = box1X + 1;
                    int box3Y = box1Y - 1;

                    if (!CheckForWall(box2X, box2Y, box3X, box3Y) && !CheckForGate(box2X, box2Y, box3X, box3Y))
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
                    int box1X = pacman.box.Left / boxSize;
                    int box1Y = pacman.box.Top / boxSize;

                    int box4X = box1X + 1;
                    int box4Y = box1Y;

                    if (!CheckForWall(box1X, box1Y, box4X, box4Y) && !CheckForGate(box1X, box1Y, box4X, box4Y))
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
                int box1X = (pacman.box.Left - boxSize) / boxSize;
                int box1Y = (pacman.box.Top - boxSize) / boxSize;

                int box2X = box1X;
                int box2Y = box1Y - 1;

                // Check if pacman is inside teleporter box 
                if (CheckForTeleporter(box1X, box1Y, box2X, box2Y) && !pacman.teleportedLastTick || pacman.teleporting)
                {
                    pacman.teleporting = true;
                    pacman.box.Left -= step;
                    pacman.blocksIntoTeleporter++;
                    if (pacman.blocksIntoTeleporter == 3)
                    {
                        pacman.teleporting = false;
                        pacman.box.Left = boxesHorizontally * boxSize;
                        pacman.teleportedLastTick = true;
                        pacman.blocksIntoTeleporter = 0;
                    }
                }
                else if (!CheckForWall(box1X, box1Y, box2X, box2Y) && !CheckForGate(box1X, box1Y, box2X, box2Y) && !pacman.teleporting)
                {
                    pacman.box.Left -= step;
                    if (pacman.teleportedLastTick == true)
                    {
                        pacman.teleportedLastTick = false;
                    }
                }
                else
                {
                    latestKey = "";
                }
            }
            else if (latestKey == "Right")
            { 
                int box1X = (pacman.box.Left + boxSize) / boxSize;
                int box1Y = (pacman.box.Top - boxSize) / boxSize;

                int box3X = box1X + 1;
                int box3Y = box1Y - 1;

                int box4X = box1X + 1;
                int box4Y = box1Y;
              
                if (CheckForTeleporter(box3X, box3Y, box4X, box4Y) && !pacman.teleportedLastTick || pacman.teleporting)
                {
                    pacman.teleporting = true;
                    pacman.box.Left += step;
                    pacman.blocksIntoTeleporter++;
                    if (pacman.blocksIntoTeleporter == 3)
                    {
                        pacman.teleporting = false; 
                        pacman.box.Left = -boxSize * 2;
                        pacman.teleportedLastTick = true;
                        pacman.blocksIntoTeleporter = 0;
                    }
                }
                else if (!CheckForWall(box3X, box3Y, box4X, box4Y) && !CheckForGate(box3X, box3Y, box4X, box4Y) && !pacman.teleporting)
                {
                    pacman.box.Left += step;
                    if (pacman.teleportedLastTick == true)
                    {
                        pacman.teleportedLastTick = false;
                    }
                }
                else
                {
                    latestKey = "";
                }
            }
            else if (latestKey == "Up")
            {
                int box1X = pacman.box.Left / boxSize;
                int box1Y = (pacman.box.Top - boxSize*2) / boxSize;
                
                int box2X = box1X;
                int box2Y = box1Y - 1;

                int box3X = box1X + 1;
                int box3Y = box1Y - 1;

                if (!CheckForWall(box2X, box2Y, box3X, box3Y) && !CheckForGate(box2X, box2Y, box3X, box3Y))
                {
                    pacman.box.Top -= step;
                }
                else
                {
                    latestKey = "";
                }    
            }
            else if (latestKey == "Down")
            {   
                int box1X = pacman.box.Left / boxSize;
                int box1Y = pacman.box.Top / boxSize;   

                int box4X = box1X + 1;
                int box4Y = box1Y;

                if (!CheckForWall(box1X, box1Y, box4X, box4Y) && !CheckForGate(box1X, box1Y, box4X, box4Y))
                {
                    pacman.box.Top += step;
                }
                else
                {
                    latestKey = "";
                }
            }

            // Move boxFood to the appropriate location before checking foodCollide
            boxFood.Location = new Point(pacman.box.Left + boxSize / 2, pacman.box.Top + boxSize / 2);

            // Check if boxFood (Pacman) is intersecting with food
            // FoodEaten(true, false) == food
            // FoodEaten(true, true) == big food
            if (latestKey != "")
            {
                // Send the index of the food that boxFood is colliding
                // with (the food that is eaten) to FoodEaten to be removed accordingly 
                if (CheckForFoodCollide(boxFood) == (true, false))
                {
                    FoodEaten((pacman.box.Left + boxSize / 2) / boxSize - horizontalFoodOffset / boxSize, 
                             (pacman.box.Top + boxSize / 2) / boxSize - verticalFoodOffset / boxSize, false);
                }
                else if (CheckForFoodCollide(boxFood) == (true, true))
                {
                    FoodEaten((pacman.box.Left + boxSize / 2) / boxSize - horizontalFoodOffset / boxSize, 
                             (pacman.box.Top + boxSize / 2) / boxSize - verticalFoodOffset / boxSize, true);
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
                if (boxes[box1X, box1Y].isWall || boxes[box2X, box2Y].isWall)
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

        private bool CheckForGate(int box1X, int box1Y, int box2X, int box2Y)
        {
            // true == gate
            // false == no gate
            try
            {
                if (boxes[box1X, box1Y].isGate || boxes[box2X, box2Y].isGate)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
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

        private bool CheckForPacman(Ghost ghost)
        {
            // false, false == no entity
            // true, false == ghost
            // false, true == pacman (game over)

            // Create a temporary pictureBox to move in the direction Entity wants 
            // to move, checking if EntityA will collide with pacman or another ghost
            // Put testGhost at EntityA's location with it's relevant attributes
            PictureBox testGhost = new PictureBox();
            testGhost.Size = ghost.box.Size;
            testGhost.Location = ghost.box.Location;
            Controls.Add(testGhost);

            try
            {
                if (ghost.direction_up)
                {
                    testGhost.Top -= step;
                    if (testGhost.Bounds.IntersectsWith(pacman.box.Bounds))
                    {
                        testGhost.Dispose();
                        return true;
                    }
                    testGhost.Dispose();
                    return false;
                }
                else if (ghost.direction_down)
                {
                    testGhost.Top += step;
                    if (testGhost.Bounds.IntersectsWith(pacman.box.Bounds))
                    {
                        testGhost.Dispose();
                        return true;
                    }
                    testGhost.Dispose();
                    return false;
                }
                else if (ghost.direction_left)
                {
                    testGhost.Left -= step;
                    if (testGhost.Bounds.IntersectsWith(pacman.box.Bounds))
                    {
                        testGhost.Dispose();
                        return true;
                    }
                    testGhost.Dispose();
                    return false;
                }
                else if (ghost.direction_right)
                {
                    testGhost.Left += step;
                    if (testGhost.Bounds.IntersectsWith(pacman.box.Bounds))
                    {
                        testGhost.Dispose();
                        return true;
                    }
                    testGhost.Dispose();
                    return false;
                }
                testGhost.Dispose();
                return false;
            }
            catch (Exception)
            {
                testGhost.Dispose();
                return false;
            }
        }


        //
        // Food-related methods
        //


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

        bool chompIsPlaying;
        bool ghostScaredIsPlaying;

        private  void FoodEaten(int indexX, int indexY, bool bigFood)
        {
            if (!bigFood)
            {
                if (!ghostScaredIsPlaying)
                {
                    Blinky.SetChase();
                    Pinky.SetChase();
                    Inky.SetChase();
                    Clyde.SetChase();
                }

                if (!chompIsPlaying)
                {
                    chompIsPlaying = true;
                    Task.Run(() =>
                    {
                        pacman_chomp.PlaySync();
                        chompIsPlaying = false;
                    });
                }
                UpdateScore(foodScore);
                Controls.Remove(food[indexX, indexY].pictureBox);
                food[indexX, indexY] = null;
                foodsOnMap--;
            }
            else
            {
                Blinky.SetFrightened();
                Pinky.SetFrightened();
                Inky.SetFrightened();
                Clyde.SetFrightened();

                if (!ghostScaredIsPlaying)
                {
                    ghostScaredIsPlaying = true;
                    Task.Run(() => 
                    { 
                        ghost_scared.PlaySync();
                        ghostScaredIsPlaying = false;
                    });
                }
                UpdateScore(foodScoreBig);
                Controls.Remove(food[indexX, indexY].pictureBox);
                food[indexX, indexY] = null;
                foodsOnMap--;
            }

            foodsOnMap--;
            if (foodsOnMap == 0)
            {
                Game(true);
            }
        }

        bool filled;
        private void bigFoodBlinkTimer_Tick(object sender, EventArgs e)
        {
            filled = !filled;

            if (filled)
            {
                for (int index = 0; index < 4; index++)
                {
                    string[] indexes = bigFoodIndexes[index].Split('_');
                    int indexX = Convert.ToInt32(indexes[0]);
                    int indexY = Convert.ToInt32(indexes[1]);
                    if (food[indexX, indexY] != null)
                    {
                        food[Convert.ToInt32(indexes[0]), Convert.ToInt32(indexes[1])].pictureBox.Show();
                    }
                }
            }
            else
            {
                for (int index = 0; index < 4; index++)
                {
                    string[] indexes = bigFoodIndexes[index].Split('_');
                    int indexX = Convert.ToInt32(indexes[0]);
                    int indexY = Convert.ToInt32(indexes[1]);
                    if (food[indexX, indexY] != null)
                    {
                        food[Convert.ToInt32(indexes[0]), Convert.ToInt32(indexes[1])].pictureBox.Hide();
                    }
                }
            }
        }


        private void UpdateScore(int scoreToAdd)
        {
            score += scoreToAdd;
            labelScore.Text = "Score: " + score;
        }

        //
        // Ghost-related methods
        //

        private void ghostTickTimer_Tick(object sender, EventArgs e)
        {
            bool game = false;
            // Blinky
            if (Blinky.direction_left)
            {
                int box1X = (Blinky.box.Left - boxSize) / boxSize;
                int box1Y = (Blinky.box.Top - boxSize) / boxSize;

                int box2X = box1X;
                int box2Y = box1Y - 1;

                if (CheckForTeleporter(box1X, box1Y, box2X, box2Y) && !Blinky.teleportedLastTick || Blinky.teleporting)
                {
                    Blinky.teleporting = true;
                    Blinky.box.Left -= step;
                    Blinky.blocksIntoTeleporter++;
                    if (Blinky.blocksIntoTeleporter == 3)
                    {
                        Blinky.teleporting = false;
                        Blinky.box.Left = boxesHorizontally * boxSize;
                        Blinky.teleportedLastTick = true;
                        Blinky.blocksIntoTeleporter = 0;
                    }
                }
                else if (!CheckForPacman(Blinky)) 
                {
                    if (!CheckForWall(box1X, box1Y, box2X, box2Y))
                    {
                        Blinky.box.Left -= step;
                        if (Blinky.teleportedLastTick == true)
                        {
                            Blinky.teleportedLastTick = false;
                        }
                    }
                    else
                    {
                        NewDirection(Blinky);
                    }
                }
                else if (!Blinky.frightened)
                {
                    if (CheckForPacman(Blinky))
                    {
                        Blinky.box.Left -= step;
                        game = true;
                    }
                } 
            }
            else if (Blinky.direction_right)
            {
                int box1X = (Blinky.box.Left + boxSize) / boxSize;
                int box1Y = (Blinky.box.Top - boxSize) / boxSize;

                int box3X = box1X + 1;
                int box3Y = box1Y - 1;

                int box4X = box1X + 1;
                int box4Y = box1Y;

                if (CheckForTeleporter(box3X, box3Y, box4X, box4Y) && !Blinky.teleportedLastTick || Blinky.teleporting)
                {
                    Blinky.teleporting = true;
                    Blinky.box.Left += step;
                    Blinky.blocksIntoTeleporter++;
                    if (Blinky.blocksIntoTeleporter == 3)
                    {
                        Blinky.teleporting = false;
                        Blinky.box.Left = -boxSize*2;
                        Blinky.teleportedLastTick = true;
                        Blinky.blocksIntoTeleporter = 0;
                    }
                }
                else if (!CheckForPacman(Blinky))
                {
                    if (!CheckForWall(box3X, box3Y, box4X, box4Y))
                    {
                        Blinky.box.Left += step;
                        if (Blinky.teleportedLastTick == true)
                        {
                            Blinky.teleportedLastTick = false;
                        }
                    }
                    else
                    {
                        NewDirection(Blinky);
                    }

                }
                else if (!Blinky.frightened)
                {
                    if (CheckForPacman(Blinky))
                    {
                        Blinky.box.Left -= step;
                        game = true;
                    }
                }
            }
            else if (Blinky.direction_up) 
            {
                int box1X = Blinky.box.Left / boxSize;
                int box1Y = (Blinky.box.Top - boxSize * 2) / boxSize;

                int box2X = box1X;
                int box2Y = box1Y - 1;

                int box3X = box1X + 1;
                int box3Y = box1Y - 1;

                if (!CheckForPacman(Blinky))
                {
                    if (!CheckForWall(box2X, box2Y, box3X, box3Y))
                    {
                        Blinky.box.Top -= step;
                    }
                    else
                    {
                        NewDirection(Blinky);
                    }
                }
                else if (!Blinky.frightened)
                {
                    if (CheckForPacman(Blinky))
                    {
                        Blinky.box.Left -= step;
                        game = true;
                    }
                }
            }
            else if (Blinky.direction_down)
            {
                int box1X = Blinky.box.Left / boxSize;
                int box1Y = Blinky.box.Top / boxSize;

                int box4X = box1X + 1;
                int box4Y = box1Y;

                if (!CheckForPacman(Blinky))
                {
                    if (!CheckForWall(box1X, box1Y, box4X, box4Y))
                    {
                        Blinky.box.Top += step;
                    }
                    else
                    {
                        NewDirection(Blinky);
                    }
                }
                else if (!Blinky.frightened)
                {
                    if (CheckForPacman(Blinky))
                    {
                        Blinky.box.Left -= step;
                        game = true;
                    }
                }
            }


            // Pinky
            if (Pinky.direction_left)
            {
                int box1X = (Pinky.box.Left - boxSize) / boxSize;
                int box1Y = (Pinky.box.Top - boxSize) / boxSize;

                int box2X = box1X;
                int box2Y = box1Y - 1;

                if (CheckForTeleporter(box1X, box1Y, box2X, box2Y) && !Pinky.teleportedLastTick || Pinky.teleporting)
                {
                    Pinky.teleporting = true;
                    Pinky.box.Left -= step;
                    Pinky.blocksIntoTeleporter++;
                    if (Pinky.blocksIntoTeleporter == 3)
                    {
                        Pinky.teleporting = false;
                        Pinky.box.Left = boxesHorizontally * boxSize;
                        Pinky.teleportedLastTick = true;
                        Pinky.blocksIntoTeleporter = 0;
                    }
                }
                else if (!CheckForPacman(Pinky))
                {
                    if (!CheckForWall(box1X, box1Y, box2X, box2Y))
                    {
                        Pinky.box.Left -= step;
                        if (Pinky.teleportedLastTick == true)
                        {
                            Pinky.teleportedLastTick = false;
                        }
                    }
                    else
                    {
                        NewDirection(Pinky);
                    }
                }
                else if (!Pinky.frightened)
                {
                    if (CheckForPacman(Pinky))
                    {
                        Pinky.box.Left -= step;
                        game = true;
                    }
                }
            }
            else if (Pinky.direction_right)
            {
                int box1X = (Pinky.box.Left + boxSize) / boxSize;
                int box1Y = (Pinky.box.Top - boxSize) / boxSize;

                int box3X = box1X + 1;
                int box3Y = box1Y - 1;

                int box4X = box1X + 1;
                int box4Y = box1Y;

                if (CheckForTeleporter(box3X, box3Y, box4X, box4Y) && !Pinky.teleportedLastTick || Pinky.teleporting)
                {
                    Pinky.teleporting = true;
                    Pinky.box.Left += step;
                    Pinky.blocksIntoTeleporter++;
                    if (Pinky.blocksIntoTeleporter == 3)
                    {
                        Pinky.teleporting = false;
                        Pinky.box.Left = -boxSize * 2;
                        Pinky.teleportedLastTick = true;
                        Pinky.blocksIntoTeleporter = 0;
                    }
                }
                else if (!CheckForPacman(Pinky))
                {
                    if (!CheckForWall(box3X, box3Y, box4X, box4Y))
                    {
                        Pinky.box.Left += step;
                        if (Pinky.teleportedLastTick == true)
                        {
                            Pinky.teleportedLastTick = false;
                        }
                    }
                    else
                    {
                        NewDirection(Pinky);
                    }
                }
                else if (!Pinky.frightened)
                {
                    if (CheckForPacman(Pinky))
                    {
                        Pinky.box.Left -= step;
                        game = true;
                    }
                }
            }
            else if (Pinky.direction_up)
            {
                int box1X = Pinky.box.Left / boxSize;
                int box1Y = (Pinky.box.Top - boxSize * 2) / boxSize;

                int box2X = box1X;
                int box2Y = box1Y - 1;

                int box3X = box1X + 1;
                int box3Y = box1Y - 1;

                if (!CheckForPacman(Pinky))
                {
                    if (!CheckForWall(box2X, box2Y, box3X, box3Y))
                    {
                        Pinky.box.Top -= step;
                    }
                    else
                    {
                        NewDirection(Pinky);
                    }
                }
                else if (!Pinky.frightened)
                {
                    if (CheckForPacman(Pinky))
                    {
                        Pinky.box.Left -= step;
                        game = true;
                    }
                }
            }
            else if (Pinky.direction_down)
            {
                int box1X = Pinky.box.Left / boxSize;
                int box1Y = Pinky.box.Top / boxSize;

                int box4X = box1X + 1;
                int box4Y = box1Y;

                if (!CheckForPacman(Pinky))
                {
                    if (!CheckForWall(box1X, box1Y, box4X, box4Y))
                    {
                        Pinky.box.Top += step;
                    }
                    else
                    {
                        NewDirection(Pinky);
                    }
                }
                else if (!Pinky.frightened)
                {
                    if (CheckForPacman(Pinky))
                    {
                        Pinky.box.Left -= step;
                        game = true;
                    }
                }
            }


            // Inky
            if (Inky.direction_left)
            {
                int box1X = (Inky.box.Left - boxSize) / boxSize;
                int box1Y = (Inky.box.Top - boxSize) / boxSize;

                int box2X = box1X;
                int box2Y = box1Y - 1;

                if (CheckForTeleporter(box1X, box1Y, box2X, box2Y) && !Inky.teleportedLastTick || Inky.teleporting)
                {
                    Inky.teleporting = true;
                    Inky.box.Left -= step;
                    Inky.blocksIntoTeleporter++;
                    if (Inky.blocksIntoTeleporter == 3)
                    {
                        Inky.teleporting = false;
                        Inky.box.Left = boxesHorizontally * boxSize;
                        Inky.teleportedLastTick = true;
                        Inky.blocksIntoTeleporter = 0;
                    }
                }
                else if (!CheckForPacman(Inky))
                {
                    if (!CheckForWall(box1X, box1Y, box2X, box2Y))
                    {
                        Inky.box.Left -= step;
                        if (Inky.teleportedLastTick == true)
                        {
                            Inky.teleportedLastTick = false;
                        }
                    }
                    else
                    {
                        NewDirection(Inky);
                    }
                }
                else if (!Inky.frightened)
                {
                    if (CheckForPacman(Inky))
                    {
                        Inky.box.Left -= step;
                        game = true;
                    }
                }
            }
            else if (Inky.direction_right)
            {
                int box1X = (Inky.box.Left + boxSize) / boxSize;
                int box1Y = (Inky.box.Top - boxSize) / boxSize;

                int box3X = box1X + 1;
                int box3Y = box1Y - 1;

                int box4X = box1X + 1;
                int box4Y = box1Y;

                if (CheckForTeleporter(box3X, box3Y, box4X, box4Y) && !Inky.teleportedLastTick || Inky.teleporting)
                {
                    Inky.teleporting = true;
                    Inky.box.Left += step;
                    Inky.blocksIntoTeleporter++;
                    if (Inky.blocksIntoTeleporter == 3)
                    {
                        Inky.teleporting = false;
                        Inky.box.Left = -boxSize * 2;
                        Inky.teleportedLastTick = true;
                        Inky.blocksIntoTeleporter = 0;
                    }
                }
                else if (!CheckForPacman(Inky))
                {
                    if (!CheckForWall(box3X, box3Y, box4X, box4Y))
                    {
                        Inky.box.Left += step;
                        if (Inky.teleportedLastTick == true)
                        {
                            Inky.teleportedLastTick = false;
                        }
                    }
                    else
                    {
                        NewDirection(Inky);
                    }
                }
                else if (!Inky.frightened)
                {
                    if (CheckForPacman(Inky))
                    {
                        Inky.box.Left -= step;
                        game = true;
                    }
                }
            }
            else if (Inky.direction_up)
            {
                int box1X = Inky.box.Left / boxSize;
                int box1Y = (Inky.box.Top - boxSize * 2) / boxSize;

                int box2X = box1X;
                int box2Y = box1Y - 1;

                int box3X = box1X + 1;
                int box3Y = box1Y - 1;

                if (!CheckForPacman(Inky))
                {
                    if (!CheckForWall(box2X, box2Y, box3X, box3Y))
                    {
                        Inky.box.Top -= step;
                    }
                    else
                    {
                        NewDirection(Inky);
                    }
                }
                else if (!Inky.frightened)
                {
                    if (CheckForPacman(Inky))
                    {
                        Inky.box.Left -= step;
                        game = true;
                    }
                }
            }
            else if (Inky.direction_down)
            {
                int box1X = Inky.box.Left / boxSize;
                int box1Y = Inky.box.Top / boxSize;

                int box4X = box1X + 1;
                int box4Y = box1Y;

                if (!CheckForPacman(Inky))
                {
                    if (!CheckForWall(box1X, box1Y, box4X, box4Y))
                    {
                        Inky.box.Top += step;
                    }
                    else
                    {
                        NewDirection(Inky);
                    }
                }
                else if (!Inky.frightened)
                {
                    if (CheckForPacman(Inky))
                    {
                        Inky.box.Left -= step;
                        game = true;
                    }
                }
            }


            // Clyde
            if (Clyde.direction_left)
            {
                int box1X = (Clyde.box.Left - boxSize) / boxSize;
                int box1Y = (Clyde.box.Top - boxSize) / boxSize;

                int box2X = box1X;
                int box2Y = box1Y - 1;

                if (CheckForTeleporter(box1X, box1Y, box2X, box2Y) && !Clyde.teleportedLastTick || Clyde.teleporting)
                {
                    Clyde.teleporting = true;
                    Clyde.box.Left -= step;
                    Clyde.blocksIntoTeleporter++;
                    if (Clyde.blocksIntoTeleporter == 3)
                    {
                        Clyde.teleporting = false;
                        Clyde.box.Left = boxesHorizontally * boxSize;
                        Clyde.teleportedLastTick = true;
                        Clyde.blocksIntoTeleporter = 0;
                    }
                }
                else if (!CheckForPacman(Clyde))
                {
                    if (!CheckForWall(box1X, box1Y, box2X, box2Y))
                    {
                        Clyde.box.Left -= step;
                        if (Clyde.teleportedLastTick == true)
                        {
                            Clyde.teleportedLastTick = false;
                        }
                    }
                    else
                    {
                        NewDirection(Clyde);
                    }
                }
                else if (!Clyde.frightened)
                {
                    if (CheckForPacman(Clyde))
                    {
                        Clyde.box.Left -= step;
                        game = true;
                    }
                }
            }
            else if (Clyde.direction_right)
            {
                int box1X = (Clyde.box.Left + boxSize) / boxSize;
                int box1Y = (Clyde.box.Top - boxSize) / boxSize;

                int box3X = box1X + 1;
                int box3Y = box1Y - 1;

                int box4X = box1X + 1;
                int box4Y = box1Y;

                if (CheckForTeleporter(box3X, box3Y, box4X, box4Y) && !Clyde.teleportedLastTick || Clyde.teleporting)
                {
                    Clyde.teleporting = true;
                    Clyde.box.Left += step;
                    Clyde.blocksIntoTeleporter++;
                    if (Clyde.blocksIntoTeleporter == 3)
                    {
                        Clyde.teleporting = false;
                        Clyde.box.Left = -boxSize * 2;
                        Clyde.teleportedLastTick = true;
                        Clyde.blocksIntoTeleporter = 0;
                    }
                }
                else if (!CheckForPacman(Clyde))
                {
                    if (!CheckForWall(box3X, box3Y, box4X, box4Y))
                    {
                        Clyde.box.Left += step;
                        if (Clyde.teleportedLastTick == true)
                        {
                            Clyde.teleportedLastTick = false;
                        }
                    }
                    else
                    {
                        NewDirection(Clyde);
                    }
                }
                else if (!Clyde.frightened)
                {
                    if (CheckForPacman(Clyde))
                    {
                        Clyde.box.Left -= step;
                        game = true;
                    }
                }
            }
            else if (Clyde.direction_up)
            {
                int box1X = Clyde.box.Left / boxSize;
                int box1Y = (Clyde.box.Top - boxSize * 2) / boxSize;

                int box2X = box1X;
                int box2Y = box1Y - 1;

                int box3X = box1X + 1;
                int box3Y = box1Y - 1;

                if (!CheckForPacman(Clyde))
                {
                    if (!CheckForWall(box2X, box2Y, box3X, box3Y))
                    {
                        Clyde.box.Top -= step;
                    }
                    else
                    {
                        NewDirection(Clyde);
                    }
                }
                else if (!Clyde.frightened)
                {
                    if (CheckForPacman(Clyde))
                    {
                        Clyde.box.Left -= step;
                        game = true;
                    }
                }
            }
            else if (Clyde.direction_down)
            {
                int box1X = Clyde.box.Left / boxSize;
                int box1Y = Clyde.box.Top / boxSize;

                int box4X = box1X + 1;
                int box4Y = box1Y;

                if (!CheckForPacman(Clyde))
                {
                    if (!CheckForWall(box1X, box1Y, box4X, box4Y))
                    {
                        Clyde.box.Top += step;
                    }
                    else
                    {
                        NewDirection(Clyde);
                    }
                }
                else if (!Clyde.frightened)
                {
                    if (CheckForPacman(Clyde))
                    {
                        Clyde.box.Left -= step;
                        game = true;
                    }
                }
            }


            if (game)
            {
                Game(false);
            }
        }

        private void NewDirection(Ghost ghost)
        {
            Random rng = new Random();
            int direction = rng.Next(0, 4);
            if (direction == 0)
            {
                ghost.SetDirection("Left");
            }
            else if (direction == 1)
            {
                ghost.SetDirection("Right");
            }
            else if (direction == 2)
            {
                ghost.SetDirection("Up");
            }
            else if (direction == 3)
            {
                ghost.SetDirection("Down");
            }
        }

        private void GhostEaten(Ghost ghost)
        {
            //TEST
            Controls.Remove(ghost.box);
            score += ghostScore;
        }

        bool ghostPic_ver2;
        private void ghostImageTimer_Tick(object sender, EventArgs e)
        {
            ghostPic_ver2 = !ghostPic_ver2;

            if (!ghostPic_ver2)
            {
                // Blinky
                if (Blinky.frightened)
                {
                    Blinky.box.Image = Resources.Ghost_Scared_Blue;
                }
                else
                {
                    if (Blinky.direction_up)
                    {
                        Blinky.box.Image = Resources.Blinky_up;
                    }
                    else if (Blinky.direction_down)
                    {
                        Blinky.box.Image = Resources.Blinky_down;
                    }
                    else if (Blinky.direction_left)
                    {
                        Blinky.box.Image = Resources.Blinky_left;
                    }
                    else if (Blinky.direction_right)
                    {
                        Blinky.box.Image = Resources.Blinky_right;
                    }
                } 

                // Pinky
                if (Pinky.frightened)
                {
                    Pinky.box.Image = Resources.Ghost_Scared_Blue;
                }
                else
                {
                    if (Pinky.direction_up)
                    {
                        Pinky.box.Image = Resources.Pinky_up;
                    }
                    else if (Pinky.direction_down)
                    {
                        Pinky.box.Image = Resources.Pinky_down;
                    }
                    else if (Pinky.direction_left)
                    {
                        Pinky.box.Image = Resources.Pinky_left;
                    }
                    else if (Pinky.direction_right)
                    {
                        Pinky.box.Image = Resources.Pinky_right;
                    }
                }

                // Inky 
                if (Inky.frightened)
                {
                    Inky.box.Image = Resources.Ghost_Scared_Blue;
                }
                else
                {
                    if (Inky.direction_up)
                    {
                        Inky.box.Image = Resources.Inky_up;
                    }
                    else if (Inky.direction_down)
                    {
                        Inky.box.Image = Resources.Inky_down;
                    }
                    else if (Inky.direction_left)
                    {
                        Inky.box.Image = Resources.Inky_left;
                    }
                    else if (Inky.direction_right)
                    {
                        Inky.box.Image = Resources.Inky_right;
                    }
                }

                // Clyde
                if (Clyde.frightened)
                {
                    Clyde.box.Image = Resources.Ghost_Scared_Blue;
                }
                else
                {
                    if (Clyde.direction_up)
                    {
                        Clyde.box.Image = Resources.Clyde_up;
                    }
                    else if (Clyde.direction_down)
                    {
                        Clyde.box.Image = Resources.Clyde_down;
                    }
                    else if (Clyde.direction_left)
                    {
                        Clyde.box.Image = Resources.Clyde_left;
                    }
                    else if (Clyde.direction_right)
                    {
                        Clyde.box.Image = Resources.Clyde_right;
                    }
                }
            }
            else
            {
                // Blinky
                if (Blinky.frightened)
                {
                    Blinky.box.Image = Resources.Ghost_Scared_Blue_ver__2;
                }
                else
                {
                    if (Blinky.direction_up)
                    {
                        Blinky.box.Image = Resources.Blinky_up_ver__2;
                    }
                    else if (Blinky.direction_down)
                    {
                        Blinky.box.Image = Resources.Blinky_down_ver__2;
                    }
                    else if (Blinky.direction_left)
                    {
                        Blinky.box.Image = Resources.Blinky_left_ver__2;
                    }
                    else if (Blinky.direction_right)
                    {
                        Blinky.box.Image = Resources.Blinky_right_ver__2;
                    }
                }

                // Pinky
                if (Pinky.frightened)
                {
                    Pinky.box.Image = Resources.Ghost_Scared_Blue_ver__2;
                }
                else
                {
                    if (Pinky.direction_up)
                    {
                        Pinky.box.Image = Resources.Pinky_up_ver__2;
                    }
                    else if (Pinky.direction_down)
                    {
                        Pinky.box.Image = Resources.Pinky_down_ver__2;
                    }
                    else if (Pinky.direction_left)
                    {
                        Pinky.box.Image = Resources.Pinky_left_ver__2;
                    }
                    else if (Pinky.direction_right)
                    {
                        Pinky.box.Image = Resources.Pinky_right_ver__2;
                    }
                }

                // Inky 
                if (Inky.frightened)
                {
                    Inky.box.Image = Resources.Ghost_Scared_Blue_ver__2;
                }
                else
                {
                    if (Inky.direction_up)
                    {
                        Inky.box.Image = Resources.Inky_up_ver__2;
                    }
                    else if (Inky.direction_down)
                    {
                        Inky.box.Image = Resources.Inky_down_ver__2;
                    }
                    else if (Inky.direction_left)
                    {
                        Inky.box.Image = Resources.Inky_left_ver__2;
                    }
                    else if (Inky.direction_right)
                    {
                        Inky.box.Image = Resources.Inky_right_ver__2;
                    }
                }

                // Clyde
                if (Clyde.frightened)
                {
                    Clyde.box.Image = Resources.Ghost_Scared_Blue_ver__2;
                }
                else
                {
                    if (Clyde.direction_up)
                    {
                        Clyde.box.Image = Resources.Clyde_up_ver__2;
                    }
                    else if (Clyde.direction_down)
                    {
                        Clyde.box.Image = Resources.Clyde_down_ver__2;
                    }
                    else if (Clyde.direction_left)
                    {
                        Clyde.box.Image = Resources.Clyde_left_ver__2;
                    }
                    else if (Clyde.direction_right)
                    {
                        Clyde.box.Image = Resources.Clyde_right_ver__2;
                    }
                }
            }
        }
    }
}
