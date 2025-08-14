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
using System.CodeDom;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using System.Drawing.Drawing2D;
using System.Net.Http.Headers;
using System.Timers;
using System.Management.Instrumentation;

enum Ghost
{
    Blinky,
    Pinky,
    Inky,
    Clyde
}

enum GhostBehaviour
{
    Scatter,
    Chase,
    Frightened
}

enum Direction
{
    Up,
    Down,
    Left,
    Right,
    Stationary
}

enum DirectionKey
{
    Empty,
    Up = 38, // Up arrow key
    Down = 40, // Down arrow key
    Left = 37, // Left arrow key
    Right = 39, // Right arrow key
    Escape = 27 // Escape key
}

enum MapCorner
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

enum Fruit
{
    Cherry,
    Strawberry,
    Apple,
    Banana, 
    Melon
}

namespace Pacman_Projection
{
    public partial class Form_Main : Form
    {
        // Often used constants
        const int boxSize = GameConstants.boxSize;
        const int entitySize = GameConstants.entitySize;
        const int boxOffset_Vertical = GameConstants.boxOffset_Vertical;

        const int boxes_Horizontally = GameConstants.boxes_Horizontally; 
        const int boxes_Vertically = GameConstants.boxes_Vertically;

        const int food_Horizontally = GameConstants.food_Horizontally;
        const int food_Vertically = GameConstants.food_Vertically;


        public string player_name;
        
        internal int level = 1; // Max level 10
        internal int levelToBeginAt;

        internal bool gamePaused;

        SoundManager soundManager = new SoundManager();

        internal DirectionKey currentKey;
        internal DirectionKey latestKey;

        internal System.Windows.Forms.Label labelReady = new System.Windows.Forms.Label();
        internal System.Windows.Forms.Label labelGameOver = new System.Windows.Forms.Label();
        internal System.Windows.Forms.Label labelLevel = new System.Windows.Forms.Label();
        internal System.Windows.Forms.Label labelFruitSpawnChance = new System.Windows.Forms.Label();
        internal Button buttonRestartGame = new Button();

        internal Box[,] boxes = new Box[GameConstants.boxes_Horizontally, GameConstants.boxes_Vertically];
        internal List<Box> walls = new List<Box>(); 

        internal Pacman pacman = new Pacman(new PictureBox(), new PictureBox());

        internal bool pacPic_open;

        internal List<PictureBox> pacmanLives = new List<PictureBox>
        {
            new PictureBox(),
            new PictureBox(),
            new PictureBox()
        };

        // Declare list containing the ghost objects
        internal List<Ghost> ghosts = new List<Ghost>();
        internal Ghost Blinky;
        internal Ghost Pinky;
        internal Ghost Inky;
        internal Ghost Clyde;
        

        internal int ghostsEatenDuringPeriod;
        internal bool ghostsToBlink;
        internal int currentEatGhostDuration;

        internal bool ghostPic_ver2;

        internal GhostBehaviour mostRecentGlobalBehaviour;
        internal GhostBehaviour currentGlobalBehaviour;

        internal bool toChangeBehaviourSound = true; // Start as true so sound plays at the start of the game, before any behaviour changes
        internal int secondsOfSameBehaviour; 

        internal Box[,] foodGrid = new Box[GameConstants.food_Horizontally, GameConstants.food_Vertically];

        internal int foodEaten;
        internal int powerPelletsEaten;
        internal bool powerPellets_Filled;
        internal int foodOnMap;

        internal PictureBox fruitBox = new PictureBox();
        internal int fruitEaten;
        internal int fruitsSpawnedTotal;
        internal Fruit currentFruit;

        internal bool timersDisabled;

        internal int score;
        internal System.Windows.Forms.Label labelScore = new System.Windows.Forms.Label();

        internal Form_Menu form_menu;

        public Form_Main(Form_Menu form_menu, string player_name, int levelToBeginAt)
        {
            InitializeComponent();
            this.form_menu = form_menu;
            this.player_name = player_name;
            this.levelToBeginAt = levelToBeginAt;
            level = levelToBeginAt;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            ClientSize = new Size(GameConstants.boxes_Horizontally * GameConstants.boxSize, GameConstants.boxes_Vertically * boxSize + boxSize);
            this.BackColor = Color.Black;
            this.Location = new Point(388, 57);
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Set timerIntervals to designated interval
            pacTickTimer.Interval = PacConstants.SpeedForLevel[level];
            ghostTickTimer.Interval = GhostConstants.SpeedForLevel[level];
            updateEatGhostDurationTimer.Interval = GhostConstants.blinkDuration;

            //
            // Create all boxes
            //

            for (int horizontalIndex = 0; horizontalIndex < boxes_Horizontally; horizontalIndex++)
            {
                for (int verticalIndex = 0; verticalIndex < boxes_Vertically; verticalIndex++)
                {
                    // Create the box
                    Box box = new Box(new PictureBox(), false, false, false, true, false);
                    // Box properties
                    box.pictureBox.Size = new Size(boxSize, boxSize);
                    box.pictureBox.Location = new Point(horizontalIndex * boxSize, verticalIndex * boxSize + boxOffset_Vertical);
                    box.pictureBox.BackColor = Color.Black;
                    Controls.Add(box.pictureBox);

                    if (horizontalIndex == 0 && verticalIndex == 18 || horizontalIndex == 0 && verticalIndex == 19 || horizontalIndex == 0 && verticalIndex == 20 ||
                        horizontalIndex == boxes_Horizontally - 1 && verticalIndex == 18 || horizontalIndex == boxes_Horizontally - 1 && verticalIndex == 19 || horizontalIndex == boxes_Horizontally - 1 && verticalIndex == 20)
                    {
                        box.isTeleporter = true;
                    }

                    // Put box into the array at designated index
                    boxes[horizontalIndex, verticalIndex] = box;
                }
            }

            // labelScore properties
            labelScore.Location = new Point(2, 2);
            labelScore.Size = new Size(50, 20);
            labelScore.Font = new Font("Pixelify Sans", 10, FontStyle.Bold);
            labelScore.Text = "0";
            labelScore.ForeColor = Color.White;
            labelScore.FlatStyle = FlatStyle.Popup;
            Controls.Add(labelScore);

            // labelLevel properties
            labelLevel.Location = new Point(boxSize * 12, 2);
            labelLevel.Size = new Size(120, 20);
            labelLevel.Font = new Font("Pixelify Sans", 12, FontStyle.Bold);
            labelLevel.Text = "Level " + level;
            labelLevel.ForeColor = Color.White;
            labelLevel.FlatStyle = FlatStyle.Popup;
            Controls.Add(labelLevel);

            // labelFruitSpawnChance properties
            labelFruitSpawnChance.Location = new Point(boxSize * 14, boxSize * 27);
            labelFruitSpawnChance.Size = new Size(30, 30);
            labelFruitSpawnChance.Font = new Font("Pixelify Sans", 8, FontStyle.Bold);
            labelFruitSpawnChance.Text = "0%";
            labelFruitSpawnChance.ForeColor = Color.White;
            labelFruitSpawnChance.BackColor = Color.Blue;
            labelFruitSpawnChance.FlatStyle = FlatStyle.Popup;
            labelFruitSpawnChance.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            Controls.Add(labelFruitSpawnChance);
            labelFruitSpawnChance.BringToFront();

            // Pacman properties
            pacman.box.LocationChanged += pacman_LocationChanged;
            pacman.box.Location = new Point(PacConstants.StartX, PacConstants.StartY);
            pacman.box.Size = new Size(entitySize, entitySize);
            pacman.box.Image = Resources.Pacman_stationary;
            pacman.box.SizeMode = PictureBoxSizeMode.StretchImage;
            Controls.Add(pacman.box);
            pacman.box.BringToFront();
            pacman.box.Hide();

            // eatBox properties
            pacman.eatBox.Size = new Size(boxSize, boxSize);
            Controls.Add(pacman.eatBox);
            pacman.UpdateLocation(pacman.box.Left, pacman.box.Top);

            // fruitBox properties
            fruitBox.BackColor = Color.Transparent;
            fruitBox.Size = new Size(entitySize, entitySize);
            fruitBox.Location = new Point(PacConstants.StartX, PacConstants.StartY);
            fruitBox.SizeMode = PictureBoxSizeMode.StretchImage;
            Controls.Add(fruitBox);
            fruitBox.BringToFront();

            //
            // Ghosts properties
            //

            // Blinky
            Blinky = new Ghost(new PictureBox(), new PictureBox(), "Blinky");
            Blinky.box.Size = new Size(entitySize, entitySize);
            Blinky.box.Image = GhostConstants.Blinky.StartImage;
            Blinky.box.LocationChanged += Blinky_LocationChanged;
            Blinky.box.Location = new Point(GhostConstants.Blinky.StartX, GhostConstants.Blinky.StartY);
            Blinky.cornerDuringScatter = MapCorner.TopRight;

            Controls.Add(Blinky.box);
            Blinky.box.BringToFront();
            Blinky.box.Hide();
            ghosts.Add(Blinky);

            // Blinky.navBox
            Blinky.navBox.Size = new Size(boxSize, boxSize);
            Controls.Add(Blinky.navBox);
            Blinky.UpdateLocation(Blinky.box.Left, Blinky.box.Top);


            // Pinky
            Pinky = new Ghost(new PictureBox(), new PictureBox(), "Pinky");
            Pinky.box.Size = new Size(entitySize, entitySize);
            Pinky.box.Image = GhostConstants.Pinky.StartImage;
            Pinky.box.LocationChanged += Pinky_LocationChanged;
            Pinky.box.Location = new Point(GhostConstants.Pinky.StartX, GhostConstants.Pinky.StartY);
            Pinky.cornerDuringScatter = MapCorner.TopLeft;

            Controls.Add(Pinky.box);
            Pinky.box.BringToFront();
            Pinky.box.Hide();
            ghosts.Add(Pinky);

            // Pinky.navBox
            Pinky.navBox.Size = new Size(boxSize, boxSize);
            Controls.Add(Pinky.navBox);
            Pinky.UpdateLocation(Pinky.box.Left, Pinky.box.Top);


            // Inky
            Inky = new Ghost(new PictureBox(), new PictureBox(), "Inky");
            Inky.box.Size = new Size(entitySize, entitySize);
            Inky.box.Image = GhostConstants.Inky.StartImage;
            Inky.box.LocationChanged += Inky_LocationChanged;
            Inky.box.Location = new Point(GhostConstants.Inky.StartX, GhostConstants.Inky.StartY);
            Inky.cornerDuringScatter = MapCorner.BottomRight;

            Controls.Add(Inky.box);
            Inky.box.BringToFront();
            Inky.box.Hide();
            ghosts.Add(Inky);

            // Inky.navBox
            Inky.navBox.Size = new Size(boxSize, boxSize);
            Controls.Add(Inky.navBox);
            Inky.UpdateLocation(Inky.box.Left, Inky.box.Top);


            // Clyde
            Clyde = new Ghost(new PictureBox(), new PictureBox(), "Clyde");
            Clyde.box.Size = new Size(entitySize, entitySize);
            Clyde.box.Image = GhostConstants.Clyde.StartImage;
            Clyde.box.LocationChanged += Clyde_LocationChanged;
            Clyde.box.Location = new Point(GhostConstants.Clyde.StartX, GhostConstants.Clyde.StartY);
            Clyde.cornerDuringScatter = MapCorner.BottomLeft;

            Controls.Add(Clyde.box);
            Clyde.box.BringToFront();
            Clyde.box.Hide();
            ghosts.Add(Clyde);

            // Clyde.navBox
            Clyde.navBox.Size = new Size(boxSize, boxSize);
            Controls.Add(Clyde.navBox);
            Clyde.UpdateLocation(Clyde.box.Left, Clyde.box.Top);

            //
            // Add all the walls according to the map
            //

            // Upper wall
            for (int indexX = 0; indexX < boxes_Horizontally; indexX++)
            {
                boxes[indexX, 0].isWall = true;
                boxes[indexX, 0].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, 0]);
            }
            // Left & right upper walls
            for (int indexY = 0; indexY < 12; indexY++)
            {
                boxes[0, indexY].isWall = true;
                boxes[0, indexY].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[0, indexY]);

                boxes[boxes_Horizontally - 1, indexY].isWall = true;
                boxes[boxes_Horizontally - 1, indexY].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[boxes_Horizontally - 1, indexY]);
            }
            // Lower wall
            for (int indexX = 0; indexX < boxes_Horizontally; indexX++)
            {
                boxes[indexX, boxes_Vertically - 2].isWall = true;
                boxes[indexX, boxes_Vertically - 2].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, boxes_Vertically - 2]);
            }
            // Left & right lower walls
            for (int indexY = 27; indexY < boxes_Vertically - 1; indexY++)
            {
                boxes[0, indexY].isWall = true;
                boxes[0, indexY].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[0, indexY]);

                boxes[boxes_Horizontally - 1, indexY].isWall = true;
                boxes[boxes_Horizontally - 1, indexY].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[boxes_Horizontally - 1, indexY]);
            }

            // Left middle walls
            for (int indexX = 0; indexX < 5; indexX++)
            {
                boxes[indexX, 12].isWall = true;
                boxes[indexX, 12].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, 12]);

                boxes[indexX, boxes_Horizontally - 4].isWall = true;
                boxes[indexX, boxes_Horizontally - 4].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, boxes_Horizontally - 4]);
            }
            for (int indexY = 12; indexY < 17; indexY++)
            {
                boxes[5, indexY].isWall = true;
                boxes[5, indexY].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[5, indexY]);

                boxes[5, boxes_Horizontally - indexY + 8].isWall = true;
                boxes[5, boxes_Horizontally - indexY + 8].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[5, boxes_Horizontally - indexY + 8]);
            }
            for (int indexX = 5; indexX >= 0; indexX--)
            {
                boxes[indexX, 17].isWall = true;
                boxes[indexX, 17].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, 17]);

                boxes[indexX, 21].isWall = true;
                boxes[indexX, 21].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, 21]);
            }

            // Right middle walls
            for (int indexX = boxes_Horizontally - 1; indexX > boxes_Horizontally - 6; indexX--)
            {
                boxes[indexX, 12].isWall = true;
                boxes[indexX, 12].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, 12]);

                boxes[indexX, boxes_Horizontally - 4].isWall = true;
                boxes[indexX, boxes_Horizontally - 4].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, boxes_Horizontally - 4]);
            }
            for (int indexY = 12; indexY < 17; indexY++)
            {
                boxes[boxes_Horizontally - 6, indexY].isWall = true;
                boxes[boxes_Horizontally - 6, indexY].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[boxes_Horizontally - 6, indexY]);

                boxes[boxes_Horizontally - 6, boxes_Horizontally - indexY + 8].isWall = true;
                boxes[boxes_Horizontally - 6, boxes_Horizontally - indexY + 8].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[boxes_Horizontally - 6, boxes_Horizontally - indexY + 8]);
            }
            for (int indexX = boxes_Horizontally - 6; indexX < boxes_Horizontally; indexX++)
            {
                boxes[indexX, 17].isWall = true;
                boxes[indexX, 17].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, 17]);

                boxes[indexX, 21].isWall = true;
                boxes[indexX, 21].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, 21]);
            }

            // Middle walls
            for (int indexY = 8; indexY < 27; indexY++)
            {
                if (indexY != 18 && indexY != 19 && indexY != 20)
                {
                    boxes[8, indexY].isWall = true;
                    boxes[8, indexY].pictureBox.BackColor = Color.Blue;
                    walls.Add(boxes[8 ,indexY]);

                    boxes[21, indexY].isWall = true;
                    boxes[21, indexY].pictureBox.BackColor = Color.Blue;
                    walls.Add(boxes[21, indexY]);
                }

                if (indexY == 12)
                {
                    boxes[9, indexY].isWall = true;
                    boxes[9, indexY].pictureBox.BackColor = Color.Blue;
                    walls.Add(boxes[9, indexY]);
                    boxes[10, indexY].isWall = true;
                    boxes[10, indexY].pictureBox.BackColor = Color.Blue;
                    walls.Add(boxes[10, indexY]);

                    boxes[19, indexY].isWall = true;
                    boxes[19, indexY].pictureBox.BackColor = Color.Blue;
                    walls.Add(boxes[19, indexY]);
                    boxes[20, indexY].isWall = true;
                    boxes[20, indexY].pictureBox.BackColor = Color.Blue;
                    walls.Add(boxes[20, indexY]);
                }
            }

            // Other walls

            for (int indexX = 5; indexX < 12; indexX++)
            {
                boxes[indexX, 3].isWall = true;
                boxes[indexX, 3].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, 3]);
                if (indexX == 9 || indexX == 10)
                {
                    boxes[indexX, 4].isWall = true;
                    boxes[indexX, 4].pictureBox.BackColor = Color.Blue;
                    walls.Add(boxes[indexX, 4]);
                }
            }

            boxes[1, 4].isWall = true;
            boxes[1, 4].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[1, 4]);
            boxes[1, 5].isWall = true;
            boxes[1, 5].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[1, 5]);
            boxes[2, 4].isWall = true;
            boxes[2, 4].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[2, 4]);
            boxes[2, 5].isWall = true;
            boxes[2, 5].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[2, 5]);

            boxes[5, 7].isWall = true;
            boxes[5, 7].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[5, 7]);
            boxes[5, 8].isWall = true;
            boxes[5, 8].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[5, 8]);
            boxes[5, 9].isWall = true;
            boxes[5, 9].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[5, 9]);
            boxes[4, 9].isWall = true;
            boxes[4, 9].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[4, 9]);
            boxes[3, 9].isWall = true;
            boxes[3, 9].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[3, 9]);

            boxes[14, 1].isWall = true;
            boxes[14, 1].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[14, 1]);
            boxes[14, 2].isWall = true;
            boxes[14, 2].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[14, 2]);
            boxes[14, 3].isWall = true;
            boxes[14, 3].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[14, 3]);
            boxes[14, 4].isWall = true;
            boxes[14, 4].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[14, 4]);

            boxes[17, 4].isWall = true;
            boxes[17, 4].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[17, 4]);
            boxes[17, 3].isWall = true;
            boxes[17, 3].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[17, 3]);
            boxes[18, 3].isWall = true;
            boxes[18, 3].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[18, 3]);
            boxes[19, 3].isWall = true;
            boxes[19, 3].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[19, 3]);
            boxes[20, 3].isWall = true;
            boxes[20, 3].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[20, 3]);
            boxes[20, 4].isWall = true;
            boxes[20, 4].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[20, 4]);

            boxes[24, 4].isWall = true;
            boxes[24, 4].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[24, 4]);
            boxes[25, 4].isWall = true;
            boxes[25, 4].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[25, 4]);
            boxes[26, 4].isWall = true;
            boxes[26, 4].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[26, 4]);
            boxes[24, 3].isWall = true;
            boxes[24, 3].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[24, 3]);
            boxes[25, 3].isWall = true;
            boxes[25, 3].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[25, 3]);
            boxes[26, 3].isWall = true;
            boxes[26, 3].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[26, 3]);

            boxes[24, 7].isWall = true;
            boxes[24, 7].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[24, 7]);
            boxes[25, 7].isWall = true;
            boxes[25, 7].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[25, 7]);
            boxes[26, 7].isWall = true;
            boxes[26, 7].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[26, 7]);
            boxes[26, 8].isWall = true;
            boxes[26, 8].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[26, 8]);
            boxes[26, 9].isWall = true;
            boxes[26, 9].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[26, 9]);
            boxes[25, 9].isWall = true;
            boxes[25, 9].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[25, 9]);
            boxes[24, 9].isWall = true;
            boxes[24, 9].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[24, 9]);
            boxes[24, 8].isWall = true;
            boxes[24, 8].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[24, 8]);

            for (int indexX = 11; indexX < 19; indexX++)
            {
                boxes[indexX, 7].isWall = true;
                boxes[indexX, 7].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, 7]);

                boxes[indexX, 25].isWall = true;
                boxes[indexX, 25].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, 25]);
            }
            for (int indexX = 11; indexX < 19; indexX++)
            {
                boxes[indexX, 8].isWall = true;
                boxes[indexX, 8].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, 8]);

                boxes[indexX, 26].isWall = true;
                boxes[indexX, 26].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, 26]);
            }
            boxes[14, 9].isWall = true;
            boxes[14, 9].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[14, 9]);
            boxes[14, 10].isWall = true;
            boxes[14, 10].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[14, 10]);
            boxes[14, 11].isWall = true;
            boxes[14, 11].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[14, 11]);
            boxes[14, 12].isWall = true;
            boxes[14, 12].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[14, 12]);
            boxes[15, 9].isWall = true;
            boxes[15, 9].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[15, 9]);
            boxes[15, 10].isWall = true;
            boxes[15, 10].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[15, 10]);
            boxes[15, 11].isWall = true;
            boxes[15, 11].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[15, 11]);
            boxes[15, 12].isWall = true;
            boxes[15, 12].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[15, 12]);

            boxes[14, 27].isWall = true;
            boxes[14, 27].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[14, 27]);
            boxes[14, 28].isWall = true;
            boxes[14, 28].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[14, 28]);
            boxes[14, 29].isWall = true;
            boxes[14, 29].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[14, 29]);
            boxes[15, 27].isWall = true;
            boxes[15, 27].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[15, 27]);
            boxes[15, 28].isWall = true;
            boxes[15, 28].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[15, 28]);
            boxes[15, 29].isWall = true;
            boxes[15, 29].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[15, 29]);

            boxes[12, 16].isWall = true;
            boxes[12, 16].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[12, 16]);
            boxes[11, 16].isWall = true;
            boxes[11, 16].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[11, 16]);
            boxes[11, 17].isWall = true;
            boxes[11, 17].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[11, 17]);
            boxes[11, 18].isWall = true;
            boxes[11, 18].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[11, 18]);
            boxes[11, 19].isWall = true;
            boxes[11, 19].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[11, 19]);
            boxes[11, 20].isWall = true;
            boxes[11, 20].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[11, 20]);
            boxes[11, 21].isWall = true;
            boxes[11, 21].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[11, 21]);
            boxes[11, 22].isWall = true;
            boxes[11, 22].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[11, 22]);
            boxes[12, 22].isWall = true;
            boxes[12, 22].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[12, 22]);
            boxes[13, 22].isWall = true;
            boxes[13, 22].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[13, 22]);
            boxes[14, 22].isWall = true;
            boxes[14, 22].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[14, 22]);
            boxes[15, 22].isWall = true;
            boxes[15, 22].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[15, 22]);
            boxes[16, 22].isWall = true;
            boxes[16, 22].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[16, 22]);
            boxes[17, 22].isWall = true;
            boxes[17, 22].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[17, 22]);
            boxes[18, 22].isWall = true;
            boxes[18, 22].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[18, 22]);
            boxes[18, 21].isWall = true;
            boxes[18, 21].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[18, 21]);
            boxes[18, 20].isWall = true;
            boxes[18, 20].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[18, 20]);
            boxes[18, 19].isWall = true;
            boxes[18, 19].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[18, 19]);
            boxes[18, 18].isWall = true;
            boxes[18, 18].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[18, 18]);
            boxes[18, 17].isWall = true;
            boxes[18, 17].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[18, 17]);
            boxes[18, 16].isWall = true;
            boxes[18, 16].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[18, 16]);
            boxes[17, 16].isWall = true;
            boxes[17, 16].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[17, 16]);

            boxes[3, 29].isWall = true;
            boxes[3, 29].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[3, 29]);
            boxes[4, 29].isWall = true;
            boxes[4, 29].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[4, 29]);
            boxes[5, 29].isWall = true;
            boxes[5, 29].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[5, 29]);
            boxes[5, 30].isWall = true;
            boxes[5, 30].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[5, 30]);
            boxes[5, 31].isWall = true;
            boxes[5, 31].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[5, 31]);
            boxes[4, 31].isWall = true;
            boxes[4, 31].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[4, 31]);
            boxes[3, 31].isWall = true;
            boxes[3, 31].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[3, 31]);
            boxes[3, 30].isWall = true;
            boxes[3, 30].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[3, 30]);

            boxes[8, 29].isWall = true;
            boxes[8, 29].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[8, 29]);
            boxes[9, 29].isWall = true;
            boxes[9, 29].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[9, 29]);
            boxes[10, 29].isWall = true;
            boxes[10, 29].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[10, 29]);
            boxes[11, 29].isWall = true;
            boxes[11, 29].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[11, 29]);

            boxes[18, 29].isWall = true;
            boxes[18, 29].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[18, 29]);
            boxes[19, 29].isWall = true;
            boxes[19, 29].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[19, 29]);
            boxes[20, 29].isWall = true;
            boxes[20, 29].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[20, 29]);
            boxes[21, 29].isWall = true;
            boxes[21, 29].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[21, 29]);

            boxes[24, 29].isWall = true;
            boxes[24, 29].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[24, 29]);
            boxes[25, 29].isWall = true;
            boxes[25, 29].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[25, 29]);
            boxes[26, 29].isWall = true;
            boxes[26, 29].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[26, 29]);
            boxes[24, 30].isWall = true;
            boxes[24, 30].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[24, 30]);
            boxes[24, 31].isWall = true;
            boxes[24, 31].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[24, 31]);

            boxes[3, 34].isWall = true;
            boxes[3, 34].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[3, 34]);
            boxes[4, 34].isWall = true;
            boxes[4, 34].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[4, 34]);
            boxes[5, 34].isWall = true;
            boxes[5, 34].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[5, 34]);

            boxes[8, 32].isWall = true;
            boxes[8, 32].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[8, 32]);
            boxes[9, 32].isWall = true;
            boxes[9, 32].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[9, 32]);
            boxes[10, 32].isWall = true;
            boxes[10, 32].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[10, 32]);
            boxes[11, 32].isWall = true;
            boxes[11, 32].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[11, 32]);
            boxes[11, 33].isWall = true;
            boxes[11, 33].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[11, 33]);
            boxes[11, 34].isWall = true;
            boxes[11, 34].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[11, 34]);
            boxes[10, 34].isWall = true;
            boxes[10, 34].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[10, 34]);
            boxes[9, 34].isWall = true;
            boxes[9, 34].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[9 ,34]);
            boxes[8, 34].isWall = true;
            boxes[8, 34].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[8, 34]);
            boxes[8, 33].isWall = true;
            boxes[8, 33].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[8, 33]);

            boxes[14, 32].isWall = true;
            boxes[14, 32].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[14, 32]);
            boxes[14, 33].isWall = true;
            boxes[14, 33].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[14, 33]);
            boxes[14, 34].isWall = true;
            boxes[14, 34].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[14, 34]);
            boxes[15, 34].isWall = true;
            boxes[15, 34].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[15, 34]);
            boxes[15, 33].isWall = true;
            boxes[15, 33].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[15, 33]);
            boxes[15, 32].isWall = true;
            boxes[15, 32].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[15, 32]);
            boxes[16, 32].isWall = true;
            boxes[16, 32].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[16, 32]);
            boxes[17, 32].isWall = true;
            boxes[17, 32].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[17, 32]);

            boxes[20, 32].isWall = true;
            boxes[20, 32].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[20, 32]);
            boxes[21, 32].isWall = true;
            boxes[21, 32].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[21, 32]);
            boxes[21, 33].isWall = true;
            boxes[21, 33].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[21, 33]);
            boxes[20, 33].isWall = true;
            boxes[20, 33].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[20, 33]);
            boxes[20, 34].isWall = true;
            boxes[20, 34].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[20, 34]);
            boxes[21, 34].isWall = true;
            boxes[21, 34].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[21, 34]);
            boxes[22, 34].isWall = true;
            boxes[22, 34].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[22, 34]);
            boxes[23, 34].isWall = true;
            boxes[23, 34].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[23, 34]);
            boxes[24, 34].isWall = true;
            boxes[24, 34].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[24, 34]);
            boxes[25, 34].isWall = true;
            boxes[25, 34].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[25, 34]);

            boxes[28, 32].isWall = true;
            boxes[28, 32].pictureBox.BackColor = Color.Blue;
            walls.Add(boxes[28, 32]);

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
            // Set all walls and boxes not to contain food
            //

            foreach (Box wall in walls)
            {
                wall.isFood = false;
            }

            // Others
            boxes[1, 3].isFood = false;
            boxes[2, 3].isFood = false;
            boxes[5, 4].isFood = false;
            boxes[7, 4].isFood = false;
            boxes[11, 4].isFood = false;
            boxes[21, 4].isFood = false;
            boxes[21, 7].isFood = false;
            boxes[21, 3].isFood = false;
            boxes[4, 8].isFood = false;
            boxes[3, 8].isFood = false;
            boxes[8, 7].isFood = false;
            boxes[9, 11].isFood = false;
            boxes[10, 11].isFood = false;
            boxes[9, 13].isFood = false;
            boxes[10, 13].isFood = false;
            boxes[13, 9].isFood = false;
            boxes[13, 11].isFood = false;
            boxes[13, 13].isFood = false;
            boxes[14, 13].isFood = false;
            boxes[15, 13].isFood = false;
            boxes[16, 13].isFood = false;
            boxes[16, 11].isFood = false;
            boxes[16, 9].isFood = false;
            boxes[19, 11].isFood = false;
            boxes[20, 11].isFood = false;
            boxes[19, 13].isFood = false;
            boxes[20, 13].isFood = false;
            boxes[19, 4].isFood = false;
            boxes[18, 4].isFood = false;
            boxes[0, 13].isFood = false;
            boxes[0, 14].isFood = false;
            boxes[0, 16].isFood = false;
            boxes[2, 13].isFood = false;
            boxes[2, 14].isFood = false;
            boxes[2, 16].isFood = false;
            boxes[4, 13].isFood = false;
            boxes[4, 14].isFood = false;
            boxes[4, 16].isFood = false;
            boxes[0, 18].isFood = false;
            boxes[0, 20].isFood = false;
            boxes[2, 18].isFood = false;
            boxes[2, 20].isFood = false;
            boxes[3, 18].isFood = false;
            boxes[3, 20].isFood = false;
            boxes[5, 18].isFood = false;
            boxes[5, 20].isFood = false;
            boxes[0, 22].isFood = false;
            boxes[0, 23].isFood = false;
            boxes[0, 25].isFood = false;
            boxes[2, 22].isFood = false;
            boxes[2, 23].isFood = false;
            boxes[2, 25].isFood = false;
            boxes[4, 22].isFood = false;
            boxes[4, 23].isFood = false;
            boxes[4, 25].isFood = false;
            boxes[8, 18].isFood = false;

            boxes[12, 17].isFood = false;
            boxes[14, 17].isFood = false;
            boxes[14, 16].isFood = false;
            boxes[16, 16].isFood = false;
            boxes[7, 4].isFood = false;
            boxes[16, 17].isFood = false;
            boxes[12, 19].isFood = false;
            boxes[14, 19].isFood = false;
            boxes[16, 19].isFood = false;
            boxes[12, 21].isFood = false;
            boxes[14, 21].isFood = false;
            boxes[16, 21].isFood = false;

            boxes[21, 18].isFood = false;
            boxes[25, 13].isFood = false;
            boxes[25, 14].isFood = false;
            boxes[25, 16].isFood = false;
            boxes[27, 13].isFood = false;
            boxes[27, 14].isFood = false;
            boxes[27, 16].isFood = false;
            boxes[29, 13].isFood = false;
            boxes[29, 14].isFood = false;
            boxes[29, 16].isFood = false;
            boxes[24, 18].isFood = false;
            boxes[24, 20].isFood = false;
            boxes[26, 18].isFood = false;
            boxes[26, 20].isFood = false;
            boxes[27, 18].isFood = false;
            boxes[27, 20].isFood = false;
            boxes[29, 18].isFood = false;
            boxes[29, 20].isFood = false;
            boxes[25, 22].isFood = false;
            boxes[25, 23].isFood = false;
            boxes[25, 25].isFood = false;
            boxes[27, 22].isFood = false;
            boxes[27, 23].isFood = false;
            boxes[27, 25].isFood = false;
            boxes[29, 22].isFood = false;
            boxes[29, 23].isFood = false;
            boxes[29, 25].isFood = false;
            boxes[25, 30].isFood = false;
            boxes[25, 31].isFood = false;
            boxes[28, 33].isFood = false;
            boxes[28, 34].isFood = false;
            boxes[16, 33].isFood = false;
            boxes[17, 34].isFood = false;

            // Place all food on the map
            PlaceAllFood();

            await Task.Delay(GameConstants.EventTimes.betweenGames);
            InitializeGame();
        }

        private void Form_Main_FormClosing(object sender, FormClosingEventArgs e)
        {   
            soundManager.toPlaySounds = false;

            PauseGame(false);

            DialogResult result = MessageBox.Show("Do you want to save your score?", "Save Highscore", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                UnpauseGame();

                soundManager.toPlaySounds = true;
            }
            else if (result == DialogResult.Yes)
            {
                form_menu.SwitchToMenuAndSaveScore(score, levelToBeginAt, player_name);
            }
            else // result == no
            {
                form_menu.SwitchToForm(form_menu, this);
            }
        }

        /*
        private void Form1_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                soundManager.toPlaySounds = true;
            }
        }
        */

        private async void InitializeGame()
        {
            // Show pacman and bring him to the front
            pacman.box.Show();
            pacman.box.BringToFront();

            // labelReady properties
            labelReady.Location = new Point(boxSize * 11, boxSize * 11);
            labelReady.Size = new Size(boxSize * 8, boxSize * 3);
            labelReady.Font = new Font("Pixelify Sans", 20, FontStyle.Bold);
            labelReady.Text = "Ready!";
            labelReady.ForeColor = Color.Yellow;
            labelReady.BackColor = Color.Transparent;
            labelReady.BorderStyle = BorderStyle.FixedSingle;
            labelReady.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            Controls.Add(labelReady);
            labelReady.BringToFront();

            // labelGameOver properties
            labelGameOver.Location = new Point(boxSize * 9, boxSize * 10);
            labelGameOver.Size = new Size(boxSize * 12, boxSize * 4);
            labelGameOver.Font = new Font("Pixelify Sans", 18, FontStyle.Bold);
            labelGameOver.Text = "Game Over";
            labelGameOver.ForeColor = Color.Red;
            labelGameOver.BackColor = Color.Transparent;
            labelGameOver.BorderStyle = BorderStyle.FixedSingle;
            labelGameOver.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            Controls.Add(labelGameOver);
            labelGameOver.BringToFront();
            labelGameOver.Hide();


            soundManager.PlaySound("pacman_beginning", false);
            // Wait for 'msToWaitBetweenGames' milliseconds before showing ghosts
            await Task.Delay(GameConstants.EventTimes.betweenGames);

            // Set ghosts starting directions
            Blinky.SetDirection(GhostConstants.Blinky.StartDirection);
            Pinky.SetDirection(GhostConstants.Pinky.StartDirection);
            Inky.SetDirection(GhostConstants.Inky.StartDirection);
            Clyde.SetDirection(GhostConstants.Clyde.StartDirection);

            Blinky.SetScatter();
            Pinky.SetScatter();
            Inky.SetScatter();
            Clyde.SetScatter();
            currentGlobalBehaviour = GhostBehaviour.Scatter;

            ghostTickTimer.Interval = GhostConstants.SpeedForLevel[level];

            Blinky.box.Show();
            Blinky.box.BringToFront();
            Pinky.box.Show();
            Pinky.box.BringToFront();
            Inky.box.Show();
            Inky.box.BringToFront();
            Clyde.box.Show();
            Clyde.box.BringToFront();

            // Remove one life from pacman
            Controls.Remove(pacmanLives[pacmanLives.Count - 1]);
            pacmanLives.RemoveAt(pacmanLives.Count - 1);

            // Timed to be complete when pacman_beginning has finished playing
            await Task.Delay(GameConstants.EventTimes.afterGhostsAppear);

            // Hide labelReady and start timers
            labelReady.Hide();
            StartTimers();

            SetSound_Scatter();
            PlaceFruitLoop();
        }

        //                                                                                        //
        //  ******************************  game-related methods  ******************************  //
        //                                                                                        //

        private async void Game(bool win)
        {
            PauseGame(true);
            soundManager.StopAllSounds();

            pacman.box.Image = Resources.Pacman_stationary;
            if (Blinky.dead)
            {
                Blinky.box.Image = Resources.Ghost_Eyes_stationary;
            }
            else
            {
                Blinky.box.Image = Resources.Blinky_stationary;
            }
            if (Pinky.dead)
            {
                Pinky.box.Image = Resources.Ghost_Eyes_stationary;
            }
            else
            {
                Pinky.box.Image = Resources.Pinky_stationary;
            }
            if (Inky.dead)
            {
                Inky.box.Image = Resources.Ghost_Eyes_stationary;
            }
            else
            {
                Inky.box.Image = Resources.Inky_stationary;
            }
            if (Clyde.dead)
            {
                Clyde.box.Image = Resources.Ghost_Eyes_stationary;
            }
            else
            {
                Clyde.box.Image = Resources.Clyde_stationary;
            }
                
            await Task.Delay(GameConstants.EventTimes.betweenGames);

            Blinky.box.Hide();
            Pinky.box.Hide();
            Inky.box.Hide();
            Clyde.box.Hide();
            fruitBox.Hide();

            if (win)
            {
                level++;

                int timesToBlink = 8;
                while (timesToBlink > 0)
                {
                    foreach (Box box in walls)
                    {
                        if (timesToBlink % 2 == 0)
                        {
                            box.pictureBox.BackColor = Color.LightYellow;
                        }
                        else
                        {
                            box.pictureBox.BackColor = Color.Blue;
                            
                        }
                    }
                    timesToBlink--;
                    await Task.Delay(GameConstants.EventTimes.wallBlink);
                }

                if (level != 10)
                {
                    Restart(true);
                }
                else
                {
                    //WIN
                }
            }
            else
            {
                // Play pacman death sound and play his death animation
                soundManager.PlaySound("pacman_death", false);
                foreach (Image image in PacConstants.deathSequence)
                {
                    pacman.box.Image = image;
                    await Task.Delay(GameConstants.EventTimes.perDeathSequence);
                }

                pacman.box.Hide();
                Restart(false);
            }
        }
        private async void Restart(bool win)
        {
            await Task.Delay(GameConstants.EventTimes.afterDeath);

            bool restart = false;
            if (!win)
            {
                try
                {
                    Controls.Remove(pacmanLives[pacmanLives.Count - 1]);
                    pacmanLives.RemoveAt(pacmanLives.Count - 1);
                    restart = true;
                }
                catch
                {
                    labelGameOver.Show();
                    labelGameOver.BringToFront();
                }
            }
            else
            {
                restart = true;
            }

            if (restart)
            {
                // Move ghosts and pacman to their startig positions, reset 
                // pacmans direction to make him start still
                Blinky.box.Location = new Point(GhostConstants.Blinky.StartX, GhostConstants.Blinky.StartY);
                Pinky.box.Location = new Point(GhostConstants.Pinky.StartX, GhostConstants.Pinky.StartY);
                Inky.box.Location = new Point(GhostConstants.Inky.StartX, GhostConstants.Inky.StartY);
                Clyde.box.Location = new Point(GhostConstants.Clyde.StartX, GhostConstants.Clyde.StartY);
                pacman.box.Location = new Point(PacConstants.StartX, PacConstants.StartY);
                pacman.box.Image = Resources.Pacman_stationary;
                ResetPacmanKey();
                // Set ghosts starting directions, pictures, and make them visible 
                Blinky.SetDirection(GhostConstants.Blinky.StartDirection);
                Blinky.box.Image = GhostConstants.Blinky.StartImage;
                Pinky.SetDirection(GhostConstants.Pinky.StartDirection);
                Pinky.box.Image = GhostConstants.Pinky.StartImage;
                Inky.SetDirection(GhostConstants.Inky.StartDirection);
                Inky.box.Image = GhostConstants.Inky.StartImage;
                Clyde.SetDirection(GhostConstants.Clyde.StartDirection);
                Clyde.box.Image = GhostConstants.Clyde.StartImage;

                Blinky.dead = false;
                Pinky.dead = false;
                Inky.dead = false;
                Clyde.dead = false;

                currentEatGhostDuration = 0;

                if (win)
                {
                    // Reset all variables  
                    foodEaten = 0;
                    powerPelletsEaten = 0;
                    currentEatGhostDuration = 0;
                    fruitEaten = 0;
                    fruitsSpawnedTotal = 0;

                    PlaceAllFood();

                    labelLevel.Text = "Level " + level.ToString();
                }

                Blinky.box.Show();
                Blinky.box.BringToFront();
                Pinky.box.Show();
                Pinky.box.BringToFront();
                Inky.box.Show();
                Inky.box.BringToFront();
                Clyde.box.Show();
                Clyde.box.BringToFront();
                pacman.box.Show();
                pacman.box.BringToFront();
                fruitBox.Show();

                labelReady.Show();
                labelReady.BringToFront();

                await Task.Delay(GameConstants.EventTimes.beforeRestart);

                labelReady.Hide();

                UnpauseGame();
            }
        }

        private void UpdateScore(int scoreToChangeBy, bool addToScore)
        {
            if (addToScore)
            {
                score += scoreToChangeBy;
                labelScore.Text = score.ToString();
            }
            else
            {
                score -= scoreToChangeBy;
                labelScore.Text = score.ToString();
            }
        }

        private async Task UpdateScore(int scoreToChangeBy, bool addToScore, bool createLabelScoreChange, PictureBox entityToPutLabelAt)
        {
            if (createLabelScoreChange)
            {
                System.Windows.Forms.Label labelScoreChange = new System.Windows.Forms.Label();
                labelScoreChange.Location = new Point(entityToPutLabelAt.Location.X, entityToPutLabelAt.Location.Y);
                labelScoreChange.Size = new Size(30, 20);
                labelScoreChange.Font = new Font("Arial", 9, FontStyle.Bold);
                labelScoreChange.Text = scoreToChangeBy.ToString();
                labelScoreChange.ForeColor = Color.DarkGreen;
                labelScoreChange.BackColor = Color.Transparent;
                labelScoreChange.FlatStyle = FlatStyle.Popup;
                labelScoreChange.BorderStyle = BorderStyle.None;
                labelScoreChange.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                Controls.Add(labelScoreChange);
                labelScoreChange.BringToFront();

                score += scoreToChangeBy;

                labelScore.Text = score.ToString();

                await Task.Delay(GameConstants.EventTimes.afterGhostEaten);

                Controls.Remove(labelScoreChange);
            }
        }

        private void PauseGame(bool stopAllSounds)
        {
            StopTimers();
            timersDisabled = true;
            gamePaused = true;

            if (!stopAllSounds)
            {
                // Stop all non-looping sounds and pause looping ones
                while (soundManager.activeSounds.Count > 0)
                {
                    int index = soundManager.activeSounds.Count - 1;
                    var sound = soundManager.activeSounds.ElementAt(index).Value;

                    if (sound.looping)
                    {
                        soundManager.PauseLoopedSound(soundManager.activeSounds.ElementAt(index).Key);
                    }
                    else
                    {
                        soundManager.PauseSound(soundManager.activeSounds.ElementAt(index).Key);
                    }
                }
            }
            else
            {
                soundManager.StopAllSounds();
            }
        }

        private void UnpauseGame()
        {
            timersDisabled = false;

            while (soundManager.pausedSounds.Count > 0)
            {
                int index = soundManager.pausedSounds.Count - 1;
                var sound = soundManager.pausedSounds.ElementAt(index).Value;

                soundManager.UnpauseSound(sound.soundName);
            }

            StartTimers();
            gamePaused = false;
        }

        private void StopTimers()
        {
            pacTickTimer.Stop();
            pacImageTimer.Stop();
            ghostTickTimer.Stop();
            ghostImageTimer.Stop();
            bigFoodBlinkTimer.Stop();
            updateEatGhostDurationTimer.Stop();
            ghostBehaviourTimeTimer.Stop();
        }

        private void StartTimers()
        {
            if (!timersDisabled)
            {
                pacTickTimer.Start();
                pacImageTimer.Start();
                ghostTickTimer.Start();
                ghostImageTimer.Start();
                bigFoodBlinkTimer.Start();
                updateEatGhostDurationTimer.Start();
                ghostBehaviourTimeTimer.Start();
            }
        }                                               

        //                                                                                         //
        //  ******************************  sound-related methods  ******************************  //
        //                                                                                         //

        private void SetSound_Scared()
        {
            soundManager.StopSound("ghost_scatter");
            soundManager.StopSound("ghost_chase1");
            soundManager.StopSound("ghost_chase2");
            soundManager.StopSound("ghost_chase3");

            soundManager.PlaySound("ghost_scared", true);
        }

        private void SetSound_Scatter()
        {
            soundManager.StopSound("ghost_scared");
            soundManager.StopSound("ghost_chase1");
            soundManager.StopSound("ghost_chase2");
            soundManager.StopSound("ghost_chase3");

            soundManager.PlaySound("ghost_scatter", true);
        }

        private void SetSound_Chase()
        {
            soundManager.StopSound("ghost_scared");
            soundManager.StopSound("ghost_scatter");

            if (level < 4)
            {
                soundManager.PlaySound("ghost_chase" + 1.ToString(), true);
            }
            else if (level >= 4 && level < 7)
            {
                soundManager.PlaySound("ghost_chase" + 2.ToString(), true);
            }
            else if (level >= 7)
            {
                soundManager.PlaySound("ghost_chase" + 3.ToString(), true);
            }
        }

        //                                                                                             //
        //  ******************************  pacman & movement-methods  ******************************  //
        //                                                                                             //

        private void pacImageTimer_Tick(object sender, EventArgs e)
        {
            pacPic_open = !pacPic_open;

            if (latestKey.Equals(DirectionKey.Left)) 
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
            else if (latestKey.Equals(DirectionKey.Right))
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
            else if (latestKey.Equals(DirectionKey.Up))
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
            else if (latestKey.Equals(DirectionKey.Down))
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

        private void pacman_LocationChanged(object sencer, EventArgs e)
        {
            if (!currentGlobalBehaviour.Equals(GhostBehaviour.Frightened))
            {
                foreach (Ghost ghost in ghosts)
                {
                    if (ghost.dead)
                    {
                        pacman.box.BringToFront();
                        return;
                    }
                    else if (!ghost.dead && pacman.box.Bounds.IntersectsWith(ghost.box.Bounds))
                    {
                        Game(false);
                    }
                }
            }
            else
            {
                foreach (Ghost ghost in ghosts)
                {
                    if (pacman.box.Bounds.IntersectsWith(ghost.box.Bounds))
                    {
                        if (ghost.dead)
                        {
                            pacman.box.BringToFront();
                            return;
                        }
                        else
                        {
                            if (ghost.Equals(Blinky))
                            {
                                GhostEaten(Blinky);
                            }
                            else if (ghost.Equals(Pinky))
                            {
                                GhostEaten(Pinky);
                            }
                            else if (ghost.Equals(Inky))
                            {
                                GhostEaten(Inky);
                            }
                            else if (ghost.Equals(Clyde))
                            {
                                GhostEaten(Clyde);
                            }
                        }
                    }
                }
            }
            pacman.UpdateLocation(pacman.box.Left, pacman.box.Top);
            pacman.box.BringToFront();
        }

        private void View_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) {
                currentKey = DirectionKey.Left;
            } 
            else if (e.KeyCode == Keys.Right) {
                currentKey = DirectionKey.Right;
            }
            else if (e.KeyCode == Keys.Up) {
                currentKey = DirectionKey.Up;
            }
            else if (e.KeyCode == Keys.Down) {
                currentKey = DirectionKey.Down;
            }
        }

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

            // Check if pacman can change direciton 
            // If currentKey (key pressed, to be registered) is different from latestKey (key currently registered),
            // check if pacman can change direction. If pacman is, for instance, going through a corridor with
            // walls below and above, he cannot change direction to up or down because there are walls there
            // If the player presses the up key during this situation, pacman will travel in the latestKey-direction
            // until he can change to the currentKey-direction. 
            


            if (currentKey != latestKey)
            {
                if (currentKey.Equals(DirectionKey.Left))
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
                else if (currentKey.Equals(DirectionKey.Right))
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
                else if (currentKey.Equals(DirectionKey.Up))
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
                else if (currentKey.Equals(DirectionKey.Down))
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
            if (latestKey.Equals(DirectionKey.Left))
            {
                int box1X = (pacman.box.Left - boxSize) / boxSize;
                int box1Y = (pacman.box.Top - boxSize) / boxSize;

                int box2X = box1X;
                int box2Y = box1Y - 1;

                // Check if pacman is inside teleporter box 
                if (CheckForTeleporter(box1X, box1Y, box2X, box2Y) && !pacman.teleportedLastTick || pacman.teleporting)
                {
                    pacman.teleporting = true;
                    pacman.box.Left -= GameConstants.step;
                    pacman.blocksIntoTeleporter++;
                    if (pacman.blocksIntoTeleporter == 3)
                    {
                        pacman.teleporting = false;
                        pacman.box.Left = boxes_Horizontally * boxSize;
                        pacman.teleportedLastTick = true;
                        pacman.blocksIntoTeleporter = 0;
                    }
                }
                else if (!CheckForWall(box1X, box1Y, box2X, box2Y) && !CheckForGate(box1X, box1Y, box2X, box2Y) && !pacman.teleporting)
                {
                    pacman.box.Left -= GameConstants.step;
                    if (pacman.teleportedLastTick == true)
                    {
                        pacman.teleportedLastTick = false;
                    }
                }
                else
                {
                    latestKey = DirectionKey.Empty;
                }
            }
            else if (latestKey.Equals(DirectionKey.Right))
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
                    pacman.box.Left += GameConstants.step;
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
                    pacman.box.Left += GameConstants.step;
                    if (pacman.teleportedLastTick == true)
                    {
                        pacman.teleportedLastTick = false;
                    }
                }
                else
                {
                    latestKey = DirectionKey.Empty;
                }
            }
            else if (latestKey.Equals(DirectionKey.Up))
            {
                int box1X = pacman.box.Left / boxSize;
                int box1Y = (pacman.box.Top - boxSize*2) / boxSize;
                
                int box2X = box1X;
                int box2Y = box1Y - 1;

                int box3X = box1X + 1;
                int box3Y = box1Y - 1;

                if (!CheckForWall(box2X, box2Y, box3X, box3Y) && !CheckForGate(box2X, box2Y, box3X, box3Y))
                {
                    pacman.box.Top -= GameConstants.step;
                }
                else
                {
                    latestKey = DirectionKey.Empty;
                }    
            }
            else if (latestKey.Equals(DirectionKey.Down))
            {   
                int box1X = pacman.box.Left / boxSize;
                int box1Y = pacman.box.Top / boxSize;   

                int box4X = box1X + 1;
                int box4Y = box1Y;

                if (!CheckForWall(box1X, box1Y, box4X, box4Y) && !CheckForGate(box1X, box1Y, box4X, box4Y))
                {
                    pacman.box.Top += GameConstants.step;
                }
                else
                {
                    latestKey = DirectionKey.Empty;
                }
            }

            FoodEaten(GetFoodCollide(pacman.eatBox), CheckForFoodCollide(pacman.eatBox).powerPellet);

            if (CheckForFruitCollide(pacman.eatBox))
            {
                FruitEaten();
            }
            


            // If pacman can change direction, latestKey is updated
            if (canChangeDirection == true)
            {
                latestKey = currentKey;
            }
        }

        private void ResetPacmanKey()
        {
            latestKey = DirectionKey.Empty;
            currentKey = DirectionKey.Empty;
        }

        //                                                                                             //
        //  ******************************  colission-related methods  ******************************  //
        //                                                                                             //

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

        //                                                                                                //
        //  ******************************  food & fruit-related methods  ******************************  //
        //                                                                                                //

        private void PlaceAllFood()
        {
            // Fills the food list and places it on the map while
            // checking if it collides with any walls, if so, they are removed
            for (int indexY = 0; indexY < food_Vertically; indexY++)
            {
                for (int indexX = 0; indexX < food_Horizontally; indexX++)
                {
                    var foodElement = foodGrid[indexX, indexY];
                    var foodIndex = new int[] { indexX, indexY };

                    // Only declare the foods the first time PlaceAllFood is run
                    if (foodGrid[indexX, indexY] == null)
                    {
                        if (GameConstants.powerPelletIndexes.Contains(foodIndex))
                        {
                            foodElement = new Box(new PictureBox(), false, false, false, true, true);
                            foodElement.pictureBox.Image = Resources.PowerPellet;
                            foodElement.isEaten = false;
                        }
                        else
                        {
                            foodElement = new Box(new PictureBox(), false, false, false, true, false);
                            foodElement.pictureBox.Image = Resources.Food;
                            foodElement.isEaten = false;
                        }

                        foodElement.pictureBox.Size = new Size(boxSize, boxSize);
                        Controls.Add(foodElement.pictureBox);

                        // Place all foods in a grid-pattern over the map
                        // If a food collides with a wall, it will be removed
                        // The same applies to foods that are placed beside others foods,
                        // creating areas of dense foods, as well as foods placed outside the map or generally where they are not supposed to be
                        foodElement.pictureBox.Location = new Point(indexX * boxSize + GameConstants.foodOffset_Horizontal, indexY * boxSize + GameConstants.foodOffset_Vertical);

                        if (AbleToPlaceFood(indexX, indexY))
                        {   
                            foodElement.pictureBox.BringToFront();
                            foodOnMap++;  
                        }
                        else
                        {
                            foodElement.pictureBox.Hide();
                        }
                    }
                    else
                    {
                        if (GameConstants.powerPelletIndexes.Contains(foodIndex))
                        {
                            foodElement.pictureBox.Image = Resources.PowerPellet;
                            foodElement.isEaten = false;
                        }   
                        else
                        {
                            foodElement.pictureBox.Image = Resources.Food;
                            foodElement.isEaten = false;
                        }
                    }
                }
            }
        }

        private bool AbleToPlaceFood(int indexXfood, int indexYfood)
        {
            foreach (Box box in boxes)
            {
                if (foodGrid[indexXfood, indexYfood].pictureBox.Bounds.IntersectsWith(box.pictureBox.Bounds) && (box.isWall || !box.isFood))
                {
                    return false;
                }
            }
            return true;
        }

        private Box GetFoodCollide (PictureBox eatBox) 
        {
            try
            {
                for (int indexX = 0; indexX < food_Horizontally; indexX++)
                {
                    for (int indexY = 0; indexY < food_Vertically; indexY++)
                    {
                        if (foodGrid[indexX, indexY].pictureBox.Bounds.IntersectsWith(eatBox.Bounds) && !foodGrid[indexX, indexY].isEaten)
                        {
                            return foodGrid[indexX, indexY];
                        }
                    }
                }
                return null;
            }
            catch (Exception) 
            {
                return null;
            }
        }

        private (bool food, bool powerPellet) CheckForFoodCollide(PictureBox eatBox)
        {
            try
            {
                for (int indexX = 0; indexX < food_Horizontally; indexX++)
                {
                    for (int indexY = 0; indexY < food_Vertically; indexY++)
                    {
                        if (foodGrid[indexX, indexY] != null)
                        {
                            if (foodGrid[indexX, indexY].pictureBox.Bounds.IntersectsWith(eatBox.Bounds) && foodGrid[indexX, indexY].pictureBox.Image != null)
                            {
                                if (foodGrid[indexX, indexY].isPowerPellet == false)
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

        private void FoodEaten(Box foodBox, bool powerPellet)
        {
            if (foodBox != null)
            {
                if (!foodBox.isEaten)
                {
                    if (!powerPellet)
                    {
                        soundManager.PlaySound("pacman_chomp", false);
                        UpdateScore(GameConstants.Scores.food, true);
                        foodBox.Eaten();
                    }
                    else
                    {
                        currentEatGhostDuration += GameConstants.EventTimes.powerPellet;
                        // If the ghosts are blinking, make them stop as
                        // currentGhostEatDuration is now over the threshold,
                        // regardless of its previous value
                        Blinky.white = false;
                        Pinky.white = false;
                        Inky.white = false;
                        Clyde.white = false;
                        ghostsToBlink = false;

                        // Ensure all ghosts are frightened
                        if (!currentGlobalBehaviour.Equals(GhostBehaviour.Frightened))
                        {
                            SetGhosts_Frightened();
                        }
                        ghostBehaviourTimeTimer.Stop();

                        UpdateScore(GameConstants.Scores.powerPellet, true);
                        foodBox.Eaten();

                        powerPelletsEaten++;
                    }
                }
                foodEaten++;
                foodOnMap--;

                // If all foods are eaten, the player wins
                if (foodOnMap == 0)
                {
                    Game(true);
                }
            }
        }

        private void bigFoodBlinkTimer_Tick(object sender, EventArgs e)
        {
            powerPellets_Filled = !powerPellets_Filled;

            foreach (var index in GameConstants.powerPelletIndexes)
            {
                if (!foodGrid[index[0], index[1]].isEaten)
                {
                    if (powerPellets_Filled)
                    {
                        foodGrid[index[0], index[1]].pictureBox.Show();
                    }
                    else
                    {
                        foodGrid[index[0], index[1]].pictureBox.Hide();
                    }
                }
            }
        }

        private bool AbleToPlaceFruit()
        {
            if (fruitBox.Image == null) 
            { 
                return true; 
            } 
            else
            { 
                return false; 
            }
        }

        private bool CheckForFruitCollide(PictureBox eatBox)
        {
            // true == fruit
            // false == no fruit
            if (fruitBox.Bounds.IntersectsWith(eatBox.Bounds) && fruitBox.Image != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void FruitEaten()
        {
            soundManager.PlaySound("pacman_eatFruit", false);
            foreach (var fruit in GameConstants.Scores.fruitScore.Keys)
            {
                if (fruit == currentFruit)
                {
                    UpdateScore(GameConstants.Scores.fruitScore[fruit], true);
                    fruitBox.Image = null;
                    break;
                }
            }
        }

        
        private async Task PlaceFruitLoop()
        {
            while (true)
            {
                double fruitSpawnChancePercent = foodEaten / 110.0 + level / 50;
                if (fruitsSpawnedTotal > 0)
                {
                    fruitSpawnChancePercent = foodEaten / (110.0 * (fruitsSpawnedTotal * 2)) + level / 30;
                }

                if (AbleToPlaceFruit() && fruitSpawnChancePercent >= 1)
                {
                    if (level <= 3)
                    {
                        int fruit = new Random().Next(0, 5);
                        if (fruit <= 1)
                        {
                            fruitBox.Image = Resources.Cherry; // 40%
                            currentFruit = Fruit.Cherry;
                        }
                        else if (fruit <= 3)
                        {
                            fruitBox.Image = Resources.Strawberry; // 40%
                            currentFruit = Fruit.Strawberry;
                        }
                        else if (fruit >= 4)
                        {
                            fruitBox.Image = Resources.Apple; // 20%
                            currentFruit = Fruit.Apple;
                        }
                    }
                    else if (level >= 4 && level < 7)
                    {
                        int fruit = new Random().Next(0, 20);
                        if (fruit <= 3)
                        {
                            fruitBox.Image = Resources.Cherry; // 20% 
                            currentFruit = Fruit.Cherry;
                        }
                        else if (fruit <= 10)
                        {
                            fruitBox.Image = Resources.Strawberry; // 35%
                            currentFruit = Fruit.Strawberry;
                        }
                        else if (fruit <= 15)
                        {
                            fruitBox.Image = Resources.Apple; // 25%
                            currentFruit = Fruit.Apple;
                        }
                        else if (fruit >= 16)
                        {
                            fruitBox.Image = Resources.Banana; // 20%
                            currentFruit = Fruit.Banana;
                        }
                    }
                    else if (level >= 7)
                    {
                        int fruit = new Random().Next(0, 20);
                        if (fruit == 0)
                        {
                            fruitBox.Image = Resources.Cherry; // 5% 
                            currentFruit = Fruit.Cherry;
                        }
                        else if (fruit <= 3)
                        {
                            fruitBox.Image = Resources.Strawberry; // 15%
                            currentFruit = Fruit.Strawberry;
                        }
                        else if (fruit <= 8)
                        {
                            fruitBox.Image = Resources.Apple; // 25%
                            currentFruit = Fruit.Apple;
                        }
                        else if (fruit <= 14)
                        {
                            fruitBox.Image = Resources.Banana; // 30%
                            currentFruit = Fruit.Banana;
                        }
                        else if (fruit >= 15)
                        {
                            fruitBox.Image = Resources.Melon; // 25%
                            currentFruit = Fruit.Melon;
                        }
                    }

                    fruitBox.BringToFront();
                    fruitsSpawnedTotal++;
                }

                if (fruitBox.Image == null)
                {
                    labelFruitSpawnChance.Text = Convert.ToInt32(fruitSpawnChancePercent * 100).ToString() + "%";
                }
                else
                {
                    labelFruitSpawnChance.Text = "0%";
                }

                await Task.Delay(10);
            }
        }

        //                                                                                         //
        //  ******************************  ghost-related methods  ******************************  //
        //                                                                                         //

        private void ghostBehaviourTimeTimer_Tick(object sender, EventArgs e)
        {
            bool behaviourChangeThisTick = false;

            if (currentGlobalBehaviour.Equals(GhostBehaviour.Scatter)) 
            {
                if (secondsOfSameBehaviour == int.Parse(GhostConstants.ScatterChaseTimesForLevel[level].Split(',')[0])) {
                    SetGhosts_Chase();
                    secondsOfSameBehaviour = 0;
                    behaviourChangeThisTick = true;
                }
            }
            else if (currentGlobalBehaviour.Equals(GhostBehaviour.Chase))
            {
                if (secondsOfSameBehaviour == int.Parse(GhostConstants.ScatterChaseTimesForLevel[level].Split(',')[1]))
                {
                    SetGhosts_Scatter();
                    secondsOfSameBehaviour = 0;
                    behaviourChangeThisTick = true;
                }
            }

            if (!behaviourChangeThisTick)
            {
                secondsOfSameBehaviour++;
            }
        }

        private void updateEatGhostDurationTimer_Tick(object sender, EventArgs e)
        {
            bool toStopScared = false;
            if (currentEatGhostDuration == GhostConstants.blinkDuration) // currentEatGhostDuration ends this tick
            {
                toStopScared = true;
            }

            if (currentEatGhostDuration > 0)
            {
                if (!currentGlobalBehaviour.Equals(GhostBehaviour.Frightened))
                {
                    SetGhosts_Frightened();
                }

                currentEatGhostDuration -= GhostConstants.blinkDuration;
                if (toStopScared && currentEatGhostDuration == 0)
                {
                    ghostsEatenDuringPeriod = 0;

                    if (mostRecentGlobalBehaviour.Equals(GhostBehaviour.Scatter))
                    {
                        SetGhosts_Scatter();
                    }
                    else if (mostRecentGlobalBehaviour.Equals(GhostBehaviour.Chase))
                    {
                        SetGhosts_Chase();
                    }

                    ghostBehaviourTimeTimer.Start();
                }
            }

            if (currentGlobalBehaviour.Equals(GhostBehaviour.Frightened))
            {
                if (currentEatGhostDuration <= (GhostConstants.blinkDuration * GhostConstants.timesToBlink) || ghostsToBlink)
                {
                    ghostsToBlink = true;
                    if (currentEatGhostDuration / GhostConstants.blinkDuration % 2 == 0)
                    {
                        Blinky.white = false;
                        Pinky.white = false;
                        Inky.white = false;
                        Clyde.white = false;
                    }
                    else
                    {
                        Blinky.white = true;
                        Pinky.white = true;
                        Inky.white = true;
                        Clyde.white = true;
                    }
                }
            }
        }

        private void Blinky_LocationChanged(object sender, EventArgs e)
        {
            Blinky.UpdateLocation(Blinky.box.Left, Blinky.box.Top);
        }

        private void Pinky_LocationChanged(object sender, EventArgs e)
        {
            Pinky.UpdateLocation(Pinky.box.Left, Pinky.box.Top);
        }

        private void Inky_LocationChanged(object sender, EventArgs e)
        {
            Inky.UpdateLocation(Inky.box.Left, Inky.box.Top);
        }

        private void Clyde_LocationChanged(object sender, EventArgs e)
        {
            Clyde.UpdateLocation(Clyde.box.Left, Clyde.box.Top);
        }

        private void ghostTickTimer_Tick(object sender, EventArgs e)
        {
            int step = GameConstants.step;

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
                    Blinky.box.Left -= GameConstants.step;
                    Blinky.blocksIntoTeleporter++;
                    if (Blinky.blocksIntoTeleporter == 3)
                    {
                        Blinky.teleporting = false;
                        Blinky.box.Left = boxes_Horizontally * boxSize;
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
                        UpdateGhostTarget(Blinky);
                    }
                }
                else if (!Blinky.frightened && !Blinky.dead)
                {
                    Blinky.box.Left -= step;
                    Game(false);
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
                        UpdateGhostTarget(Blinky);
                    }

                }
                else if (!Blinky.frightened && !Blinky.dead)
                {
                    Blinky.box.Left += step;
                    Game(false);
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
                        UpdateGhostTarget(Blinky);
                    }
                }
                else if (!Blinky.frightened && !Blinky.dead)
                {
                    Blinky.box.Top -= step;
                    Game(false);
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
                        UpdateGhostTarget(Blinky);
                    }
                }
                else if (!Blinky.frightened && !Blinky.dead)
                {
                    Blinky.box.Top += step;
                    Game(false);
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
                        Pinky.box.Left = boxes_Horizontally * boxSize;
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
                        UpdateGhostTarget(Pinky);
                    }
                }
                else if (!Pinky.frightened && !Pinky.dead)
                {
                    Pinky.box.Left -= step;
                    Game(false);
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
                        UpdateGhostTarget(Pinky);
                    }
                }
                else if (!Pinky.frightened && !Pinky.dead)
                {
                    Pinky.box.Left += step;
                    Game(false);
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
                        UpdateGhostTarget(Pinky);
                    }
                }
                else if (!Pinky.frightened && !Pinky.dead)
                {
                    Pinky.box.Top -= step;
                    Game(false);
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
                        UpdateGhostTarget(Pinky);
                    }
                }
                else if (!Pinky.frightened && !Pinky.dead)
                {
                    Pinky.box.Top += step;
                    Game(false);
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
                        Inky.box.Left = boxes_Horizontally * boxSize;
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
                        UpdateGhostTarget(Inky);
                    }
                }
                else if (!Inky.frightened && !Inky.dead)
                {
                    Inky.box.Left -= step;
                    Game(false);
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
                        UpdateGhostTarget(Inky);
                    }
                }
                else if (!Inky.frightened && !Inky.dead)
                {
                    Inky.box.Left += step;
                    Game(false);
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
                        UpdateGhostTarget(Inky);
                    }
                }
                else if (!Inky.frightened && !Inky.dead)
                {
                    Inky.box.Top -= step;
                    Game(false);
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
                        UpdateGhostTarget(Inky);
                    }
                }
                else if (!Inky.frightened && !Inky.dead)
                {
                    Inky.box.Top += step;
                    Game(false);
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
                        Clyde.box.Left = boxes_Horizontally * boxSize;
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
                        UpdateGhostTarget(Clyde);
                    }
                }
                else if (!Clyde.frightened && !Clyde.dead)
                {
                    Clyde.box.Left -= step;
                    Game(false);
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
                        UpdateGhostTarget(Clyde);
                    }
                }
                else if (!Clyde.frightened && !Clyde.dead)
                {
                    Clyde.box.Left += step;
                    Game(false);
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
                        UpdateGhostTarget(Clyde);
                    }
                }
                else if (!Clyde.frightened && !Clyde.dead)
                {
                    Clyde.box.Top -= step;
                    Game(false);
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
                        UpdateGhostTarget(Clyde);
                    }
                }
                else if (!Clyde.frightened && !Clyde.dead)
                {
                    Clyde.box.Top += step;
                    Game(false);
                }
            }
        }

        private void SetGhosts_Frightened()
        {
            // Switch current behaviour to most recent behaviour before its updated
            mostRecentGlobalBehaviour = currentGlobalBehaviour;
            currentGlobalBehaviour = GhostBehaviour.Frightened;

            SetSound_Scared();

            Blinky.SetFrightened();
            Pinky.SetFrightened();
            Inky.SetFrightened();
            Clyde.SetFrightened();

            ghostTickTimer.Interval = GhostConstants.SpeedForLevel_Frightened[level];
        }

        private void SetGhosts_Scatter()
        {
            mostRecentGlobalBehaviour = currentGlobalBehaviour;
            currentGlobalBehaviour = GhostBehaviour.Scatter;

            SetSound_Scatter();

            Blinky.SetScatter();
            Pinky.SetScatter();
            Inky.SetScatter();
            Clyde.SetScatter();

            ghostTickTimer.Interval = GhostConstants.SpeedForLevel[level];
        }

        private void SetGhosts_Chase()
        {
            mostRecentGlobalBehaviour = currentGlobalBehaviour;
            currentGlobalBehaviour = GhostBehaviour.Chase;

            SetSound_Chase();

            Blinky.SetChase();
            Pinky.SetChase();
            Inky.SetChase();
            Clyde.SetChase();

            ghostTickTimer.Interval = GhostConstants.SpeedForLevel[level];
        }

        private bool CheckForPacman(Ghost ghost)
        {
            // true == pacman
            if (!ghost.dead)
            {
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
                        testGhost.Top -= GameConstants.step;
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
                        testGhost.Top += GameConstants.step;
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
                        testGhost.Left -= GameConstants.step;
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
                        testGhost.Left += GameConstants.step;
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
            else
            {
                return false;
            }
        }

        private void UpdateGhostTarget(Ghost ghost)
        {
            // Hierarchy: Up > Down > Left > Right  
            // Blinky: Chase pacman directly
            // Pinky: Chase pacman 4 boxes ahead in his direction
            // Inky: Chase pacman 4 boxes ahead in his direction + Blinky's position mirrored (180 degrees) 
            // Clyde: Chase pacman directly, but if within 8 boxes of pacman, enter scatter mode

            if (ghost.Equals(Blinky))
            {
                bool directionUp = false;
                bool directionDown = false;
                bool directionLeft = false;
                bool directionRight = false; 

                Blinky.SetTarget(pacman.currentPosX, pacman.currentPosY);
                PictureBox testGhost = new PictureBox(); 
                testGhost.Size = ghost.box.Size;
                testGhost.Location = ghost.box.Location;

                for (int direction = 0; direction < 4; direction++)
                {
                    if (direction == 0)
                    {
                        testGhost.Top -= GameConstants.step;
                    }
                    else if (direction == 1)
                    {
                        testGhost.Top += GameConstants.step;
                    }
                    else if (direction == 2)
                    {
                        testGhost.Left -= GameConstants.step;
                    }
                    else
                    {
                        testGhost.Left += GameConstants.step;
                    }

                    foreach (Box wall in walls)
                    {
                        if (!testGhost.Bounds.IntersectsWith(wall.pictureBox.Bounds))
                        {
                            if (direction == 0)
                            {
                                directionUp = true;
                            }
                            else if (direction == 1)
                            {
                                directionDown = true;
                            }
                            else if (direction == 2)
                            {
                                directionLeft = true;
                            }
                            else
                            {
                                directionRight = true;
                            }
                            break;
                        }
                    }
                }

                int positionDifferenceX = pacman.currentPosX - ghost.currentPosX;
                if (positionDifferenceX < 0) 
                { 
                    positionDifferenceX = -positionDifferenceX; 
                }
                int positionDifferenceY = pacman.currentPosY - ghost.currentPosY;
                if (positionDifferenceY < 0)
                {
                    positionDifferenceY = -positionDifferenceY;
                }


                // Always dispose as to not take up memory
                testGhost.Dispose();
            }
        }

        private async void GhostEaten(Ghost ghost)
        {
            StopTimers();
            
            ghost.dead = true;
            ghost.box.Image = Resources.Ghost_Eyes_up;
            ghostsEatenDuringPeriod++;

            UpdateScore(GameConstants.Scores.ghost * ghostsEatenDuringPeriod, true, true, ghost.box);

            ghost.box.Hide();
            pacman.box.Hide();

            soundManager.PlaySound("pacman_eatGhost", false);

            // Add the time the game is paused to currentEatGhostDuration so it sums up to no change
            currentEatGhostDuration += GameConstants.EventTimes.afterGhostEaten;

            await Task.Delay(GameConstants.EventTimes.afterGhostEaten);
            
            ghost.box.Show();
            pacman.box.Show();

            StartTimers();
        }

        private void ghostImageTimer_Tick(object sender, EventArgs e)
        {
            ghostPic_ver2 = !ghostPic_ver2;

            /*
            foreach (Ghost ghost in ghosts)
            {
                if (!ghostPic_ver2)
                {
                    
                }
                else
                {

                }

                if (!ghostPic_ver2)
                {
                    if (!ghost.dead)
                    {
                        if (Blinky.frightened)
                        {
                            if (Blinky.white)
                            {
                                ghost.box.Image = Resources.Ghost_Scared_White;
                            }
                            else
                            {
                                ghost.box.Image = Resources.Ghost_Scared_Blue;
                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
            */

            //
            // Blinky
            //
            if (!ghostPic_ver2)
            {
                if (!Blinky.dead)
                {
                    if (Blinky.frightened)
                    {
                        if (Blinky.white)
                        {
                            Blinky.box.Image = Resources.Ghost_Scared_White;
                        }
                        else
                        {
                            Blinky.box.Image = Resources.Ghost_Scared_Blue;
                        }
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
                }
                else
                {
                    if (Blinky.direction_up)
                    {
                        Blinky.box.Image = Resources.Ghost_Eyes_up;
                    }
                    else if (Blinky.direction_down)
                    {
                        Blinky.box.Image = Resources.Ghost_Eyes_down;
                    }
                    else if (Blinky.direction_left)
                    {
                        Blinky.box.Image = Resources.Ghost_Eyes_left;
                    }
                    else if (Blinky.direction_right)
                    {
                        Blinky.box.Image = Resources.Ghost_Eyes_right;
                    }
                }

                //
                // Pinky
                //
                if (!Pinky.dead)
                {
                    if (Pinky.frightened)
                    {
                        if (Pinky.white)
                        {
                            Pinky.box.Image = Resources.Ghost_Scared_White;
                        }
                        else
                        {
                            Pinky.box.Image = Resources.Ghost_Scared_Blue;
                        }
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
                }
                else
                {
                    if (Pinky.direction_up)
                    {
                        Pinky.box.Image = Resources.Ghost_Eyes_up;
                    }
                    else if (Pinky.direction_down)
                    {
                        Pinky.box.Image = Resources.Ghost_Eyes_down;
                    }
                    else if (Pinky.direction_left)
                    {
                        Pinky.box.Image = Resources.Ghost_Eyes_left;
                    }
                    else if (Pinky.direction_right)
                    {
                        Pinky.box.Image = Resources.Ghost_Eyes_right;
                    }
                }
                //
                // Inky
                //
                if (!Inky.dead)
                {
                    if (Inky.frightened)
                    {
                        if (Inky.white)
                        {
                            Inky.box.Image = Resources.Ghost_Scared_White;
                        }
                        else
                        {
                            Inky.box.Image = Resources.Ghost_Scared_Blue;
                        }
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
                }
                else
                {
                    if (Inky.direction_up)
                    {
                        Inky.box.Image = Resources.Ghost_Eyes_up;
                    }
                    else if (Inky.direction_down)
                    {
                        Inky.box.Image = Resources.Ghost_Eyes_down;
                    }
                    else if (Pinky.direction_left)
                    {
                        Inky.box.Image = Resources.Ghost_Eyes_left;
                    }
                    else if (Inky.direction_right)
                    {
                        Inky.box.Image = Resources.Ghost_Eyes_right;
                    }
                }

                //
                // Clyde
                //
                if (!Clyde.dead)
                {
                    if (Clyde.frightened)
                    {
                        if (Clyde.white)
                        {
                            Clyde.box.Image = Resources.Ghost_Scared_White;
                        }
                        else
                        {
                            Clyde.box.Image = Resources.Ghost_Scared_Blue;
                        }
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
                        else if (Inky.direction_right)
                        {
                            Clyde.box.Image = Resources.Clyde_right;
                        }
                    }
                }
                else
                {
                    if (Clyde.direction_up)
                    {
                        Clyde.box.Image = Resources.Ghost_Eyes_up;
                    }
                    else if (Clyde.direction_down)
                    {
                        Clyde.box.Image = Resources.Ghost_Eyes_down;
                    }
                    else if (Clyde.direction_left)
                    {
                        Clyde.box.Image = Resources.Ghost_Eyes_left;
                    }
                    else if (Clyde.direction_right)
                    {
                        Clyde.box.Image = Resources.Ghost_Eyes_right;
                    }
                }
            }
            else
            {
                //
                // Blinky
                //
                if (!Blinky.dead)
                {
                    if (Blinky.frightened)
                    {
                        if (Blinky.white)
                        {
                            Blinky.box.Image = Resources.Ghost_Scared_White_ver__2;
                        }
                        else
                        {
                            Blinky.box.Image = Resources.Ghost_Scared_Blue_ver__2;
                        }
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
                }
                else
                {
                    if (Blinky.direction_up)
                    {
                        Blinky.box.Image = Resources.Ghost_Eyes_up;
                    }
                    else if (Blinky.direction_down)
                    {
                        Blinky.box.Image = Resources.Ghost_Eyes_down;
                    }
                    else if (Blinky.direction_left)
                    {
                        Blinky.box.Image = Resources.Ghost_Eyes_left;
                    }
                    else if (Blinky.direction_right)
                    {
                        Blinky.box.Image = Resources.Ghost_Eyes_right;
                    }
                }
                //
                // Pinky
                //
                if (!Pinky.dead)
                {
                    if (Pinky.frightened)
                    {
                        if (Pinky.white)
                        {
                            Pinky.box.Image = Resources.Ghost_Scared_White_ver__2;
                        }
                        else
                        {
                            Pinky.box.Image = Resources.Ghost_Scared_Blue_ver__2;
                        }
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
                }
                else
                {
                    if (Pinky.direction_up)
                    {
                        Pinky.box.Image = Resources.Ghost_Eyes_up;
                    }
                    else if (Pinky.direction_down)
                    {
                        Pinky.box.Image = Resources.Ghost_Eyes_down;
                    }
                    else if (Pinky.direction_left)
                    {
                        Pinky.box.Image = Resources.Ghost_Eyes_left;
                    }
                    else if (Pinky.direction_right)
                    {
                        Pinky.box.Image = Resources.Ghost_Eyes_right;
                    }
                }
                //
                // Inky
                //
                if (!Inky.dead)
                {
                    if (Inky.frightened)
                    {
                        if (Inky.white)
                        {
                            Inky.box.Image = Resources.Ghost_Scared_White_ver__2;
                        }
                        else
                        {
                            Inky.box.Image = Resources.Ghost_Scared_Blue_ver__2; 
                        }
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
                }
                else
                {
                    if (Inky.direction_up)
                    {
                        Inky.box.Image = Resources.Ghost_Eyes_up;
                    }
                    else if (Inky.direction_down)
                    {
                        Inky.box.Image = Resources.Ghost_Eyes_down;
                    }
                    else if (Pinky.direction_left)
                    {
                        Inky.box.Image = Resources.Ghost_Eyes_left;
                    }
                    else if (Inky.direction_right)
                    {
                        Inky.box.Image = Resources.Ghost_Eyes_right;
                    }
                }
                //
                // Clyde
                //
                if (!Clyde.dead)
                {
                    if (Clyde.frightened)
                    {
                        if (Clyde.white)
                        {
                            Clyde.box.Image = Resources.Ghost_Scared_White_ver__2;
                        }
                        else
                        {
                            Clyde.box.Image = Resources.Ghost_Scared_Blue_ver__2;
                        }
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
                        else if (Inky.direction_right)
                        {
                            Clyde.box.Image = Resources.Clyde_right_ver__2;
                        }
                    }
                }
                else
                {
                    if (Clyde.direction_up)
                    {
                        Clyde.box.Image = Resources.Ghost_Eyes_up;
                    }
                    else if (Clyde.direction_down)
                    {
                        Clyde.box.Image = Resources.Ghost_Eyes_down;
                    }
                    else if (Clyde.direction_left)
                    {
                        Clyde.box.Image = Resources.Ghost_Eyes_left;
                    }
                    else if (Clyde.direction_right)
                    {
                        Clyde.box.Image = Resources.Ghost_Eyes_right;
                    }
                }
            }
        }
    }
}