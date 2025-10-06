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


public enum GhostName
{
    Blinky,
    Pinky,
    Inky,
    Clyde
}


public enum GhostBehaviour
{
    Scatter,
    Chase,
    Frightened
}

public enum Direction
{
    Stationary,
    Up,
    Down,
    Left,
    Right
}

public enum Key
{
    None,
    Up = 38, // Up arrow key
    Down = 40, // Down arrow key
    Left = 37, // Left arrow key
    Right = 39, // Right arrow key
    Escape = 27 // Escape key
}

public enum MapCorner
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    None
}

public enum Fruit
{
    Cherry,
    Strawberry,
    Apple,
    Banana, 
    Melon
}

public enum EntityBox
{
    LowerLeft,
    UpperLeft,
    UpperRight,
    LowerRight,
}

public enum EntityState
{
    Standard,
    Teleporting,
    Eaten
}

namespace Pacman_Projection
{
    public partial class Form_Main : Form
    {
        FormManager formManager;
        EventManager eventManager;
        GlobalVariables globalVariables;

        // Often used constants from GameConstants
        const int boxSize = GameConstants.boxSize;
        const int step = GameConstants.step;
        const int entitySize = GameConstants.entitySize;
        const int boxOffset_Vertical = GameConstants.boxOffset_Vertical;

        const int boxes_Horizontally = GameConstants.boxes_Horizontally; 
        const int boxes_Vertically = GameConstants.boxes_Vertically;

        const int food_Horizontally = GameConstants.food_Horizontally;
        const int food_Vertically = GameConstants.food_Vertically;

        // Local variables

        internal int level = 1; // Max level 10

        internal bool gamePaused;

        internal SoundManager soundManager = new SoundManager();

        internal Key pressedKey;
        internal Key registeredKey;

        internal System.Windows.Forms.Label labelReady;
        internal System.Windows.Forms.Label labelGameOver;
        internal System.Windows.Forms.Label labelLevel;
        internal System.Windows.Forms.Label labelFruitSpawnChance;

        internal Box[,] boxes = new Box[GameConstants.boxes_Horizontally, GameConstants.boxes_Vertically];
        internal List<Box> walls = new List<Box>(); 
        
        internal Pacman pacman = new Pacman();

        internal bool pacPic_open;

        internal List<PictureBox> pacmanLives = new List<PictureBox>
        {
            new PictureBox(),
            new PictureBox(),
            new PictureBox()
        };

        // Declare list containing the ghosts
        internal List<Ghost> ghosts = new List<Ghost>();
        internal Ghost Blinky;
        internal Ghost Pinky;
        internal Ghost Inky;
        internal Ghost Clyde;
        

        internal int ghostsEatenDuringPeriod;
        internal bool ghostsToBlink;
        internal int currentEatGhostDuration;
        internal int maxGhostsEatenInRow; // For score calculation

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
        internal int fruitsSpawnedTotal;
        internal Fruit currentFruit;

        internal bool timersDisabled;

        internal System.Windows.Forms.Label labelScore = new System.Windows.Forms.Label();

        public Form_Main(FormManager formManager, EventManager eventManager, GlobalVariables globalVariables)
        {
            InitializeComponent();
            this.formManager = formManager;
            this.eventManager = eventManager;
            this.globalVariables = globalVariables;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            ClientSize = new Size(GameConstants.boxes_Horizontally * GameConstants.boxSize, GameConstants.boxes_Vertically * boxSize + boxSize);
            this.BackColor = Color.Black;
            this.Location = new Point(388, 57);
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            level = globalVariables.StartLevel;

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
            labelScore = new System.Windows.Forms.Label
            {
                Location = new Point(2, 2),
                Size = new Size(50, 20),
                Font = new Font("Pixelify Sans", 10, FontStyle.Bold),
                Text = "0",
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Popup
            };
            Controls.Add(labelScore);

            // labelLevel properties
            labelLevel = new System.Windows.Forms.Label
            {
                Location = new Point(boxSize * 12, 2),
                Size = new Size(120, 20),
                Font = new Font("Pixelify Sans", 12, FontStyle.Bold),
                Text = "Level " + level,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Popup
            };
            Controls.Add(labelLevel);

            // labelFruitSpawnChance properties
            labelFruitSpawnChance = new System.Windows.Forms.Label
            {
                Location = new Point(boxSize * 14, boxSize * 27),
                Size = new Size(30, 30),
                Font = new Font("Pixelify Sans", 8, FontStyle.Bold),
                Text = "0%",
                ForeColor = Color.White,
                BackColor = Color.Blue,
                FlatStyle = FlatStyle.Popup,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
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
            Blinky = new Ghost(GhostName.Blinky);
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
            Pinky = new Ghost(GhostName.Pinky);
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
            Inky = new Ghost(GhostName.Inky);
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
            Clyde = new Ghost(GhostName.Clyde);
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
            if (sender.Equals(this)) 
            {
                e.Cancel = true;
                PauseGame(true);
            }
           
            //soundManager.StopAllSounds(); // Stop all sounds so they restart on next game start
        }

        private async void InitializeGame()
        {
            // Show pacman and bring him to the front
            pacman.box.Show();
            pacman.box.BringToFront();

            // Set up pacman's lives display
            foreach (var lifeBox in pacmanLives)
            {
                lifeBox.Size = new Size(entitySize, entitySize);
                lifeBox.Image = Resources.Pacman_left;
                lifeBox.Location = new Point((GameConstants.boxes_Horizontally - 2) * boxSize - (entitySize + 5) * pacmanLives.IndexOf(lifeBox), 0);
                Controls.Add(lifeBox);
                lifeBox.BringToFront(); 
            }

            // labelReady properties
            labelReady = new System.Windows.Forms.Label
            {
                Location = new Point(boxSize * 11, boxSize * 11),
                Size = new Size(boxSize * 8, boxSize * 3),
                Font = new Font("Pixelify Sans", 20, FontStyle.Bold),
                Text = "Ready!",
                ForeColor = Color.Yellow,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            Controls.Add(labelReady);
            labelReady.BringToFront();


            // labelGameOver properties
            labelGameOver = new System.Windows.Forms.Label
            {
                Location = new Point(boxSize * 9, boxSize * 10),
                Size = new Size(boxSize * 12, boxSize * 4),
                Font = new Font("Pixelify Sans", 18, FontStyle.Bold),
                Text = "Game Over",
                ForeColor = Color.Red,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            Controls.Add(labelGameOver);
            labelGameOver.BringToFront();
            labelGameOver.Hide();

            soundManager.PlaySound(Sounds.pacman_beginning, false);

            await Task.Delay(GameConstants.EventTimes.betweenGames);

            // Set starting directions
            pacman.SetDirection(PacConstants.StartDirection);

            Blinky.SetDirection(GhostConstants.Blinky.StartDirection);
            Pinky.SetDirection(GhostConstants.Pinky.StartDirection);
            Inky.SetDirection(GhostConstants.Inky.StartDirection);
            Clyde.SetDirection(GhostConstants.Clyde.StartDirection);

            // Set starting behaviours
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
            PauseGame(false);
            soundManager.StopAllSounds();

            pacman.box.Image = Resources.Pacman_stationary;
            if (Blinky.currentState.Equals(EntityState.Eaten))
            {
                Blinky.box.Image = Resources.Ghost_Eyes_stationary;
            }
            else
            {
                Blinky.box.Image = Resources.Blinky_stationary;
            }
            if (Pinky.currentState.Equals(EntityState.Eaten))
            {
                Pinky.box.Image = Resources.Ghost_Eyes_stationary;
            }
            else
            {
                Pinky.box.Image = Resources.Pinky_stationary;
            }
            if (Inky.currentState.Equals(EntityState.Eaten))
            {
                Inky.box.Image = Resources.Ghost_Eyes_stationary;
            }
            else
            {
                Inky.box.Image = Resources.Inky_stationary;
            }
            if (Clyde.currentState.Equals(EntityState.Eaten))
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

                soundManager.PlaySound(Sounds.pacman_win, false);

                int timesToBlink = 10;
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
                soundManager.PlaySound(Sounds.pacman_death, false);
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
                    soundManager.toPlaySounds = false;

                    labelGameOver.Show();
                    labelGameOver.BringToFront();

                    Task.Delay(GameConstants.EventTimes.gameOverDisplayed).Wait();

                    // Register the end level reached for the highscore table
                    globalVariables.EndLevel = level;

                    formManager.OpenForm(formManager.FormPauseMenu);
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

                Blinky.SetState(EntityState.Standard);
                Pinky.SetState(EntityState.Standard);
                Inky.SetState(EntityState.Standard);
                Clyde.SetState(EntityState.Standard);

                currentEatGhostDuration = 0;

                if (win)
                {
                    // Reset all variables  
                    foodEaten = 0;
                    powerPelletsEaten = 0;
                    currentEatGhostDuration = 0;
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
                globalVariables.Score += scoreToChangeBy;
                labelScore.Text = globalVariables.Score.ToString();
            }
            else
            {
                globalVariables.Score -= scoreToChangeBy;
                labelScore.Text = globalVariables.Score.ToString();
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

                globalVariables.Score += scoreToChangeBy;

                labelScore.Text = globalVariables.Score.ToString();

                await Task.Delay(GameConstants.EventTimes.afterGhostEaten);

                Controls.Remove(labelScoreChange);
            }
        }

        private void PauseGame(bool showPauseMenu)
        {
            StopTimers();
            timersDisabled = true;
            gamePaused = true;

            // Stop all non-looping sounds and pause looping ones when the game is paused
            while (soundManager.activeSounds.Count > 0)
            {
                int index = soundManager.activeSounds.Count - 1;
                var sound = soundManager.activeSounds.ElementAt(index).Value;

                if (sound.Looping)
                {
                    soundManager.PauseLoopedSound(soundManager.activeSounds.ElementAt(index).Key);
                }
                else
                {
                    soundManager.PauseSound(soundManager.activeSounds.ElementAt(index).Key);
                }
            }
            
            if (showPauseMenu)
            {
                globalVariables.CurrentLevel = level; // Update CurrentLevel in globalVariables in case it changed since last pause
                formManager.OpenForm(formManager.FormPauseMenu);
            }
        }

        internal void UnpauseGame()
        {
            timersDisabled = false;

            while (soundManager.pausedSounds.Count > 0)
            {
                int index = soundManager.pausedSounds.Count - 1;
                var sound = soundManager.pausedSounds.ElementAt(index).Key;

                soundManager.UnpauseSound(sound);
            }

            this.Focus();
            this.KeyPreview = true;

            StartTimers();
            gamePaused = false;
        }

        private void StopTimers()
        {
            pacTickTimer.Stop();
            pacImageTimer.Stop();
            ghostTickTimer.Stop();
            ghostImageTimer.Stop();
            powerPelletBlinkTimer.Stop();
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
                powerPelletBlinkTimer.Start();
                updateEatGhostDurationTimer.Start();
                ghostBehaviourTimeTimer.Start();
            }
        }                                               

        //                                                                                         //
        //  ******************************  sound-related methods  ******************************  //
        //                                                                                         //

        private void SetSound_Scared()
        {
            soundManager.StopSound(Sounds.ghost_scatter);
            soundManager.StopSound(Sounds.ghost_chase1);
            soundManager.StopSound(Sounds.ghost_chase2);
            soundManager.StopSound(Sounds.ghost_chase3);

            soundManager.PlaySound(Sounds.ghost_scared, true);
        }

        private void SetSound_Scatter()
        {
            soundManager.StopSound(Sounds.ghost_scared);
            soundManager.StopSound(Sounds.ghost_chase1);
            soundManager.StopSound(Sounds.ghost_chase2);
            soundManager.StopSound(Sounds.ghost_chase3);

            soundManager.PlaySound(Sounds.ghost_scatter, true);
        }

        private void SetSound_Chase()
        {
            soundManager.StopSound(Sounds.ghost_scared);
            soundManager.StopSound(Sounds.ghost_scatter);

            if (level < 4)
            {
                soundManager.PlaySound(Sounds.ghost_chase1, true);
            }
            else if (level >= 4 && level < 7)
            {
                soundManager.PlaySound(Sounds.ghost_chase2, true);
            }
            else if (level >= 7)
            {
                soundManager.PlaySound(Sounds.ghost_chase3, true);
            }
        }

        //                                                                                             //
        //  ******************************  pacman & movement-methods  ******************************  //
        //                                                                                             //

        private void pacImageTimer_Tick(object sender, EventArgs e)
        {
            pacPic_open = !pacPic_open;

            if (registeredKey.Equals(Key.Left)) 
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
            else if (registeredKey.Equals(Key.Right))
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
            else if (registeredKey.Equals(Key.Up))
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
            else if (registeredKey.Equals(Key.Down))
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
                    if (ghost.currentState.Equals(EntityState.Eaten))
                    {
                        pacman.box.BringToFront();
                        return;
                    }
                    else if (!ghost.currentState.Equals(EntityState.Eaten) && pacman.box.Bounds.IntersectsWith(ghost.box.Bounds))
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
                        if (ghost.currentState.Equals(EntityState.Eaten))
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
            if (e.KeyCode == Keys.Left) 
            {
                pressedKey = Key.Left;
            } 
            else if (e.KeyCode == Keys.Right) 
            {
                pressedKey = Key.Right;
            }
            else if (e.KeyCode == Keys.Up) 
            {
                pressedKey = Key.Up;
            }
            else if (e.KeyCode == Keys.Down) 
            {
                pressedKey = Key.Down;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                 PauseGame(true);
            }
        }

        private void pacTickTimer_Tick(object sender, EventArgs e)
        {
            if (registeredKey != pressedKey)
            {
                Key latestKeyRegistered = registeredKey;
                switch (pressedKey)
                {
                    case Key.Left:
                        if (!CheckForWall(pacman, Direction.Left))
                        {
                            registeredKey = pressedKey;
                        }
                        else
                        {
                            registeredKey = latestKeyRegistered;
                        }
                        break;
                    case Key.Right:
                        if (!CheckForWall(pacman, Direction.Right))
                        {
                            registeredKey = pressedKey;
                        }
                        else
                        {
                            registeredKey = latestKeyRegistered;
                        }
                        break;
                    case Key.Up:
                        if (!CheckForWall(pacman, Direction.Up))
                        {
                            registeredKey = pressedKey;
                        }
                        else
                        {
                            registeredKey = latestKeyRegistered;
                        }
                        break;
                    case Key.Down:
                        if (!CheckForWall(pacman, Direction.Down) && !CheckForGate(pacman, Direction.Down)) // Pacman can only encounter gates when going down
                        {
                            registeredKey = pressedKey;
                        }
                        else
                        {
                            registeredKey = latestKeyRegistered;
                        }
                        break;
                }
            }

            switch (registeredKey)
            {
                case Key.Left:
                    if (!pacman.currentDirection.Equals(Direction.Left))
                    {
                        pacman.SetDirection(Direction.Left);
                    }

                    // Check if pacman is inside teleporter box 
                    if ((CheckForTeleporter(pacman) && !pacman.teleportedLastTick) || pacman.currentState.Equals(EntityState.Teleporting))
                    {
                        pacman.SetState(EntityState.Teleporting);
                        pacman.box.Left -= step;
                        pacman.blocksIntoTeleporter++;
                        if (pacman.blocksIntoTeleporter.Equals(GameConstants.maxStepsIntoTeleporter))
                        {
                            pacman.SetState(EntityState.Standard);
                            pacman.box.Left = boxes_Horizontally * boxSize;
                            pacman.teleportedLastTick = true;
                            pacman.blocksIntoTeleporter = 0;
                        }
                    }
                    else if (!CheckForWall(pacman) && !pacman.currentState.Equals(EntityState.Teleporting))
                    {
                        pacman.box.Left -= step;
                        if (pacman.teleportedLastTick == true)
                        {
                            pacman.teleportedLastTick = false;
                        }
                    }
                    else
                    {
                        StopEntityMovement(pacman);
                    }
                    break;
                case Key.Right:
                    {
                        if (!pacman.currentDirection.Equals(Direction.Right))
                        {
                            pacman.SetDirection(Direction.Right);
                        }

                        if (CheckForTeleporter(pacman) && !pacman.teleportedLastTick || pacman.currentState.Equals(EntityState.Teleporting))
                        {
                            pacman.SetState(EntityState.Teleporting);
                            pacman.box.Left += step;
                            pacman.blocksIntoTeleporter++;
                            if (pacman.blocksIntoTeleporter == GameConstants.maxStepsIntoTeleporter)
                            {
                                pacman.SetState(EntityState.Standard);
                                pacman.box.Left = -boxSize * 2;
                                pacman.teleportedLastTick = true;
                                pacman.blocksIntoTeleporter = 0;
                            }
                        }
                        else if (!CheckForWall(pacman) && !pacman.currentState.Equals(EntityState.Teleporting))
                        {
                            pacman.box.Left += step;
                            if (pacman.teleportedLastTick == true)
                            {
                                pacman.teleportedLastTick = false;
                            }
                        }
                        else
                        {
                            StopEntityMovement(pacman);
                        }
                    }
                    break;
                case Key.Up:
                    {
                        if (!pacman.currentDirection.Equals(Direction.Up))
                        {
                            pacman.SetDirection(Direction.Up);
                        }

                        if (!CheckForWall(pacman))
                        {
                            pacman.box.Top -= step;
                        }
                        else
                        {
                            StopEntityMovement(pacman);
                        }
                    }
                    break;
                case Key.Down:
                    {
                        if (!pacman.currentDirection.Equals(Direction.Down))
                        {
                            pacman.SetDirection(Direction.Down);
                        }

                        if (!CheckForWall(pacman) && !CheckForGate(pacman))
                        {
                            pacman.box.Top += step;
                        }
                        else
                        {
                            StopEntityMovement(pacman);
                        }
                    }
                    break;
            }

            FoodEaten(GetFoodCollide(pacman.eatBox), CheckForFoodCollide(pacman.eatBox).powerPellet);

            if (CheckForFruitCollide(pacman.eatBox))
            {
                FruitEaten();
            }
        }

        private void ResetPacmanKey()
        {
            pressedKey = Key.None;
            registeredKey = Key.None;
        }

        private void StopEntityMovement(Entity entity)
        {
            entity.SetDirection(Direction.Stationary);

            if (entity.Equals(pacman))
            {
                ResetPacmanKey();
            }
        }

        //                                                                                             //
        //  ******************************  Collision-related methods  ******************************  //
        //                                                                                             //

        private void ColorEntityBoxes(Entity entity)
        {
            int[] ll = new int[2];
            int[] ul = new int[2];
            int[] ur = new int[2];
            int[] lr = new int[2];

            ll[0] = entity.GetStandardPosition(EntityBox.LowerLeft)[0];
            ll[1] = entity.GetStandardPosition(EntityBox.LowerLeft)[1];

            ul[0] = entity.GetStandardPosition(EntityBox.UpperLeft)[0];
            ul[1] = entity.GetStandardPosition(EntityBox.UpperLeft)[1];

            ur[0] = entity.GetStandardPosition(EntityBox.UpperRight)[0];
            ur[1] = entity.GetStandardPosition(EntityBox.UpperRight)[1];

            lr[0] = entity.GetStandardPosition(EntityBox.LowerRight)[0];
            lr[1] = entity.GetStandardPosition(EntityBox.LowerRight)[1];

            boxes[ll[0], ll[1]].pictureBox.BackColor = Color.Green;
            boxes[ul[0], ul[1]].pictureBox.BackColor = Color.Red;
            boxes[ur[0], ur[1]].pictureBox.BackColor = Color.Purple;
            boxes[lr[0], lr[1]].pictureBox.BackColor = Color.Cyan;

            boxes[ll[0], ll[1]].pictureBox.BringToFront();
            boxes[ul[0], ul[1]].pictureBox.BringToFront();
            boxes[ur[0], ur[1]].pictureBox.BringToFront();
            boxes[lr[0], lr[1]].pictureBox.BringToFront();
        }

        private bool CheckForWall(Entity entity)
        {
            switch (entity.currentDirection)
            {
                case Direction.Left:
                    int[] lowerLeft = entity.GetStandardPosition(EntityBox.LowerLeft);
                    int[] upperLeft = entity.GetStandardPosition(EntityBox.UpperLeft);

                    // Adjust positions for direction
                    lowerLeft[0] -= 1;
                    upperLeft[0] -= 1;

                    if (boxes[lowerLeft[0], lowerLeft[1]].isWall || boxes[upperLeft[0], upperLeft[1]].isWall)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Right:
                    int[] lowerRight = entity.GetStandardPosition(EntityBox.LowerRight);
                    int[] upperRight = entity.GetStandardPosition(EntityBox.UpperRight);

                    // Adjust positions for direction
                    lowerRight[0] += 1;
                    upperRight[0] += 1;

                    if (boxes[lowerRight[0], lowerRight[1]].isWall || boxes[upperRight[0], upperRight[1]].isWall)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Up:
                    int[] upperLeft_Up = entity.GetStandardPosition(EntityBox.UpperLeft);
                    int[] upperRight_Up = entity.GetStandardPosition(EntityBox.UpperRight);

                    // Adjust positions for direction
                    upperLeft_Up[1] -= 1;
                    upperRight_Up[1] -= 1;

                    if (boxes[upperLeft_Up[0], upperLeft_Up[1]].isWall || boxes[upperRight_Up[0], upperRight_Up[1]].isWall)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Down:
                    int[] lowerLeft_Down = entity.GetStandardPosition(EntityBox.LowerLeft);
                    int[] lowerRight_Down = entity.GetStandardPosition(EntityBox.LowerRight);

                    // Adjust positions for direction
                    lowerLeft_Down[1] += 1;
                    lowerRight_Down[1] += 1;

                    if (boxes[lowerLeft_Down[0], lowerLeft_Down[1]].isWall || boxes[lowerRight_Down[0], lowerRight_Down[1]].isWall)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    return true;
            }
        }

        private bool CheckForWall(Entity entity, Direction directionToCheckIn)
        {
            switch (directionToCheckIn)
            {
                case Direction.Left:
                    int[] lowerLeft = entity.GetStandardPosition(EntityBox.LowerLeft);
                    int[] upperLeft = entity.GetStandardPosition(EntityBox.UpperLeft);

                    // Adjust positions for direction
                    lowerLeft[0] -= 1;
                    upperLeft[0] -= 1;

                    if (boxes[lowerLeft[0], lowerLeft[1]].isWall || boxes[upperLeft[0], upperLeft[1]].isWall)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Right:
                    int[] lowerRight = entity.GetStandardPosition(EntityBox.LowerRight);
                    int[] upperRight = entity.GetStandardPosition(EntityBox.UpperRight);

                    // Adjust positions for direction
                    lowerRight[0] += 1;
                    upperRight[0] += 1;

                    if (boxes[lowerRight[0], lowerRight[1]].isWall || boxes[upperRight[0], upperRight[1]].isWall)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Up:
                    int[] upperLeft_Up = entity.GetStandardPosition(EntityBox.UpperLeft);
                    int[] upperRight_Up = entity.GetStandardPosition(EntityBox.UpperRight);

                    // Adjust positions for direction
                    upperLeft_Up[1] -= 1;
                    upperRight_Up[1] -= 1;

                    if (boxes[upperLeft_Up[0],upperLeft_Up[1]].isWall || boxes[upperRight_Up[0], upperRight_Up[1]].isWall)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Down:
                    int[] lowerLeft_Down = entity.GetStandardPosition(EntityBox.LowerLeft);
                    int[] lowerRight_Down = entity.GetStandardPosition(EntityBox.LowerRight);

                    // Adjust positions for direction
                    lowerLeft_Down[1] += 1;
                    lowerRight_Down[1] += 1;

                    if (boxes[lowerLeft_Down[0], lowerLeft_Down[1]].isWall || boxes[lowerRight_Down[0], lowerRight_Down[1]].isWall)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    return true;
            }
        }

        private bool CheckForGate(Entity entity)
        {
            if (entity.currentDirection.Equals(Direction.Up))
            {
                int[] upperLeft_Up = entity.GetStandardPosition(EntityBox.UpperLeft);
                int[] upperRight_Up = entity.GetStandardPosition(EntityBox.UpperRight);

                // Adjust positions for direction
                upperLeft_Up[1] -= 1;
                upperRight_Up[1] -= 1;

                if (boxes[upperLeft_Up[0], upperLeft_Up[1]].isGate || boxes[upperRight_Up[0], upperRight_Up[1]].isGate)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (entity.currentDirection.Equals(Direction.Down))
            {
                int[] lowerLeft_Down = entity.GetStandardPosition(EntityBox.LowerLeft);
                int[] lowerRight_Down = entity.GetStandardPosition(EntityBox.LowerRight);

                // Adjust positions for direction
                lowerLeft_Down[1] += 1;
                lowerRight_Down[1] += 1;

                if (boxes[lowerLeft_Down[0], lowerLeft_Down[1]].isGate || boxes[lowerRight_Down[0], lowerRight_Down[1]].isGate)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        private bool CheckForGate(Entity entity, Direction directionToCheckIn)
        {
            //entity.UpdateStandardPositions();
            switch (directionToCheckIn)
            {
                case Direction.Left:
                    int[] lowerLeft = entity.GetStandardPosition(EntityBox.LowerLeft);
                    int[] upperLeft = entity.GetStandardPosition(EntityBox.UpperLeft);

                    // Adjust positions for direction
                    lowerLeft[0] -= 1;
                    upperLeft[0] -= 1;

                    if (boxes[lowerLeft[0], lowerLeft[1]].isGate || boxes[upperLeft[0], upperLeft[1]].isGate)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Right:
                    int[] lowerRight = entity.GetStandardPosition(EntityBox.LowerLeft);
                    int[] upperRight = entity.GetStandardPosition(EntityBox.UpperLeft);

                    // Adjust positions for direction
                    lowerRight[0] += 1;
                    upperRight[0] += 1;

                    if (boxes[lowerRight[0], lowerRight[1]].isGate || boxes[upperRight[0], upperRight[1]].isGate)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Up:
                    int[] upperLeftUp = entity.GetStandardPosition(EntityBox.LowerLeft);
                    int[] upperRightUp = entity.GetStandardPosition(EntityBox.UpperLeft);

                    // Adjust positions for direction
                    upperLeftUp[1] -= 1;
                    upperRightUp[1] -= 1;

                    if (boxes[upperLeftUp[0], upperLeftUp[1]].isGate || boxes[upperRightUp[0], upperRightUp[1]].isGate)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case Direction.Down:
                    int[] lowerLeftDown = entity.GetStandardPosition(EntityBox.LowerLeft);
                    int[] lowerRightDown = entity.GetStandardPosition(EntityBox.UpperLeft);

                    // Adjust positions for direction
                    lowerLeftDown[1] += 1;
                    lowerRightDown[1] += 1;

                    if (boxes[lowerLeftDown[0], lowerLeftDown[1]].isGate || boxes[lowerRightDown[0], lowerRightDown[1]].isGate)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    return true;
            }
        }

        private bool CheckForTeleporter(Entity entity)
        {
            // An entity can only enter a teleporter while going left or right
            if (entity.currentDirection.Equals(Direction.Left))
            {
                int[] lowerLeft = entity.GetStandardPosition(EntityBox.LowerLeft);
                int[] upperLeft = entity.GetStandardPosition(EntityBox.UpperLeft);

                // Adjust positions for direction
                lowerLeft[0] -= 1;
                upperLeft[0] -= 1;

                if (boxes[lowerLeft[0], lowerLeft[1]].isTeleporter && boxes[upperLeft[0], upperLeft[1]].isTeleporter)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (entity.currentDirection.Equals(Direction.Right))
            {
                int[] lowerRight = entity.GetStandardPosition(EntityBox.LowerLeft);
                int[] upperRight = entity.GetStandardPosition(EntityBox.UpperLeft);

                // Adjust positions for direction
                lowerRight[0] += 1;
                upperRight[0] += 1;

                if (boxes[lowerRight[0], lowerRight[1]].isTeleporter && boxes[upperRight[0], upperRight[1]].isTeleporter)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        //                                                                                                //
        //  ******************************  Food & fruit-related methods  ******************************  //
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
                        bool powerPellet = false;
                        foreach (var index in GameConstants.powerPelletIndexes)
                        {
                            if (foodIndex[0] == index[0] && foodIndex[1] == index[1]) 
                            { 
                                powerPellet = true; 
                            }
                        }
                        if (powerPellet)
                        {
                            foodElement = new Box(new PictureBox(), false, false, false, true, true);
                            foodElement.pictureBox.Image = Resources.PowerPellet;
                            foodElement.isEaten = false;

                            foodGrid[indexX, indexY] = foodElement;
                        }
                        else
                        {
                            foodElement = new Box(new PictureBox(), false, false, false, true, false);
                            foodElement.pictureBox.Image = Resources.Food;
                            foodElement.isEaten = false;

                            foodGrid[indexX, indexY] = foodElement;
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

        private bool AbleToPlaceFood(int indexX, int indexY)
        {
            foreach (Box box in boxes)
            {
                if (foodGrid[indexX, indexY].pictureBox.Bounds.IntersectsWith(box.pictureBox.Bounds) && (box.isWall || !box.isFood))
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
                        soundManager.PlaySound(Sounds.pacman_chomp, false);
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

        private void powerPelletBlinkTimer_Tick(object sender, EventArgs e)
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
            soundManager.PlaySound(Sounds.pacman_eatFruit, false);
            foreach (var fruit in GameConstants.Scores.fruitScore.Keys)
            {
                if (fruit == currentFruit)
                {
                    UpdateScore(GameConstants.Scores.fruitScore[fruit], true);
                    fruitBox.Image = null;
                    globalVariables.FruitEaten++;
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
                if (secondsOfSameBehaviour == int.Parse(GhostConstants.ScatterChaseTimesForLevel[level].Split(',')[0])) 
                {
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
                    // Update maxGhostsEatenInRow if a new max has been reached
                    if (ghostsEatenDuringPeriod > maxGhostsEatenInRow)
                    {
                        maxGhostsEatenInRow = ghostsEatenDuringPeriod;
                    }
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
            foreach (Ghost ghost in ghosts)
            {
                switch (ghost.currentDirection)
                {
                    case Direction.Left:
                        if (CheckForTeleporter(ghost) && !ghost.teleportedLastTick || ghost.currentState.Equals(EntityState.Teleporting))
                        {
                            ghost.SetState(EntityState.Teleporting);
                            ghost.box.Left -= step;
                            ghost.blocksIntoTeleporter++;
                            if (ghost.blocksIntoTeleporter == GameConstants.maxStepsIntoTeleporter)
                            {
                                ghost.SetState(EntityState.Standard);
                                ghost.box.Left = boxes_Horizontally * boxSize;
                                ghost.teleportedLastTick = true;
                                ghost.blocksIntoTeleporter = 0;
                            }
                        }
                        else if (!CheckForPacman(ghost))
                        {
                            if (!CheckForWall(ghost))
                            {
                                ghost.box.Left -= step;
                                if (ghost.teleportedLastTick == true)
                                {
                                    ghost.teleportedLastTick = false;
                                }
                            }
                            else
                            {
                                NewDirection(ghost);
                                UpdateGhostTarget(ghost);
                            }
                        }
                        else if (!ghost.frightened && !ghost.currentState.Equals(EntityState.Standard))
                        {
                            ghost.box.Left -= step;
                            Game(false);
                        }
                        break;
                    case Direction.Right:
                        if (CheckForTeleporter(ghost) && !ghost.teleportedLastTick || ghost.currentState.Equals(EntityState.Teleporting))
                        {
                            ghost.SetState(EntityState.Teleporting);
                            ghost.box.Left += step;
                            ghost.blocksIntoTeleporter++;
                            if (ghost.blocksIntoTeleporter == GameConstants.maxStepsIntoTeleporter)
                            {
                                ghost.SetState(EntityState.Standard);
                                ghost.box.Left = -boxSize * 2;
                                ghost.teleportedLastTick = true;
                                ghost.blocksIntoTeleporter = 0;
                            }
                        }
                        else if (!CheckForPacman(ghost))
                        {
                            if (!CheckForWall(ghost))
                            {
                                ghost.box.Left += step;
                                if (ghost.teleportedLastTick == true)
                                {
                                    ghost.teleportedLastTick = false;
                                }
                            }
                            else
                            {
                                NewDirection(ghost);
                                UpdateGhostTarget(ghost);
                            }

                        }
                        else if (!ghost.frightened && !ghost.currentState.Equals(EntityState.Eaten))
                        {
                            ghost.box.Left += step;
                            Game(false);
                        }
                        break;
                    case Direction.Up:
                        if (!CheckForPacman(ghost))
                        {
                            if (!CheckForWall(ghost))
                            {
                                ghost.box.Top -= step;
                            }
                            else
                            {
                                NewDirection(ghost);
                                UpdateGhostTarget(ghost);
                            }
                        }
                        else if (!ghost.frightened && !ghost.currentState.Equals(EntityState.Eaten))
                        {
                            ghost.box.Top -= step;
                            Game(false);
                        }
                        break;
                    case Direction.Down:
                        if (!CheckForPacman(ghost))
                        {
                            if (!CheckForWall(ghost))
                            {
                                ghost.box.Top += step;
                            }
                            else
                            {
                                NewDirection(ghost);
                                UpdateGhostTarget(ghost);
                            }
                        }
                        else if (!ghost.frightened && !ghost.currentState.Equals(EntityState.Eaten))
                        {
                            ghost.box.Top += step;
                            Game(false);
                        }
                        break;
                }
            }
        }

        private void NewDirection(Ghost ghost)
        {
            StopEntityMovement(ghost);

            Random rnd = new Random();
            int randomInt = rnd.Next(0, 4); 

            if (randomInt == 0)
            {
                ghost.SetDirection(Direction.Left);
            }
            else if (randomInt == 1)
            {
                ghost.SetDirection(Direction.Right);
            }
            else if (randomInt == 2)
            {
                ghost.SetDirection(Direction.Up);
            }
            else if (randomInt == 3)
            {
                ghost.SetDirection(Direction.Down);
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
            if (!ghost.currentState.Equals(EntityState.Eaten))
            {
                // Create a temporary pictureBox to move in the direction the entity wants 
                // to move, checking if it will collide with pacman or another ghost
                // Put testGhost at the entity's location with it's relevant attributes
                PictureBox testGhost = new PictureBox();
                testGhost.Size = ghost.box.Size;
                testGhost.Location = ghost.box.Location;
                Controls.Add(testGhost);

                try
                {
                    if (ghost.currentDirection.Equals(Direction.Up))
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
                    else if (ghost.currentDirection.Equals(Direction.Down))
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
                    else if (ghost.currentDirection.Equals(Direction.Left))
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
                    else if (ghost.currentDirection.Equals(Direction.Right))
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

            PictureBox testGhost = new PictureBox()
            {
                Size = ghost.box.Size,
                Location = ghost.box.Location
            }; 

            // Always dispose as to not take up memory
            testGhost.Dispose();        
        }

        private async void GhostEaten(Ghost ghost)
        {
            StopTimers();

            ghost.SetState(EntityState.Eaten);
            ghost.box.Image = GhostConstants.Images.eyesStationary; // Default image when eaten
            ghostsEatenDuringPeriod++;
            globalVariables.GhostsEaten++;

            UpdateScore(GameConstants.Scores.ghost * ghostsEatenDuringPeriod, true, true, ghost.box);

            ghost.box.Hide();
            pacman.box.Hide();

            soundManager.PlaySound(Sounds.pacman_eatGhost, false);

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

            foreach (Ghost ghost in ghosts)
            { 
                if (!ghostPic_ver2)
                {
                    if (!ghost.currentState.Equals(EntityState.Eaten))
                    {
                        if (ghost.frightened)
                        {
                            if (ghost.white)
                            {
                                ghost.box.Image = GhostConstants.Images.frightenedWhite;  
                            }
                            else
                            {
                                ghost.box.Image = GhostConstants.Images.frightenedBlue;
                            }
                        }
                        else
                        {
                            if (ghost.Equals(Blinky))
                            {
                                if (ghost.currentDirection.Equals(Direction.Left))
                                {
                                    ghost.box.Image = GhostConstants.Images.Blinky.left;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Right))
                                {
                                    ghost.box.Image = GhostConstants.Images.Blinky.right;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Up))
                                {
                                    ghost.box.Image = GhostConstants.Images.Blinky.up;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Down))
                                {
                                    ghost.box.Image = GhostConstants.Images.Blinky.down;
                                }
                            }
                            else if (ghost.Equals(Pinky))
                            {
                                if (ghost.currentDirection.Equals(Direction.Left))
                                {
                                    ghost.box.Image = GhostConstants.Images.Pinky.left;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Right))
                                {
                                    ghost.box.Image = GhostConstants.Images.Pinky.right;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Up))
                                {
                                    ghost.box.Image = GhostConstants.Images.Pinky.up;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Down))
                                {
                                    ghost.box.Image = GhostConstants.Images.Pinky.down;
                                }
                            }
                            else if (ghost.Equals(Inky))
                            {
                                if (ghost.currentDirection.Equals(Direction.Left))
                                {
                                    ghost.box.Image = GhostConstants.Images.Inky.left;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Right))
                                {
                                    ghost.box.Image = GhostConstants.Images.Inky.right;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Up))
                                {
                                    ghost.box.Image = GhostConstants.Images.Inky.up;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Down))
                                {
                                    ghost.box.Image = GhostConstants.Images.Inky.down;
                                }
                            }
                            else if (ghost.Equals(Clyde))
                            {
                                if (ghost.currentDirection.Equals(Direction.Left))
                                {
                                    ghost.box.Image = GhostConstants.Images.Clyde.left;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Right))
                                {
                                    ghost.box.Image = GhostConstants.Images.Clyde.right;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Up))
                                {
                                    ghost.box.Image = GhostConstants.Images.Clyde.up;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Down))
                                {
                                    ghost.box.Image = GhostConstants.Images.Clyde.down;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (ghost.currentDirection.Equals(Direction.Left))
                        {
                            ghost.box.Image = GhostConstants.Images.eyesLeft;
                        }
                        else if (ghost.currentDirection.Equals(Direction.Right))
                        {
                            ghost.box.Image = GhostConstants.Images.eyesRight;
                        }
                        else if (ghost.currentDirection.Equals(Direction.Up))
                        {
                            ghost.box.Image = GhostConstants.Images.eyesUp;
                        }
                        else if (ghost.currentDirection.Equals(Direction.Down))
                        {
                            ghost.box.Image = GhostConstants.Images.eyesDown;
                        }
                    }
                }
                else
                {
                    if (!ghost.currentState.Equals(EntityState.Eaten))
                    {
                        if (ghost.frightened)
                        {
                            if (ghost.white)
                            {
                                ghost.box.Image = GhostConstants.Images.frightenedWhite2;
                            }
                            else
                            {
                                ghost.box.Image = GhostConstants.Images.frightenedBlue2;
                            }
                        }
                        else
                        {
                            if (ghost.Equals(Blinky))
                            {
                                if (ghost.currentDirection.Equals(Direction.Left))
                                {
                                    ghost.box.Image = GhostConstants.Images.Blinky.left2;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Right))
                                {
                                    ghost.box.Image = GhostConstants.Images.Blinky.right2;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Up))
                                {
                                    ghost.box.Image = GhostConstants.Images.Blinky.up2;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Down))
                                {
                                    ghost.box.Image = GhostConstants.Images.Blinky.down2;
                                }
                            }
                            else if (ghost.Equals(Pinky))
                            {
                                if (ghost.currentDirection.Equals(Direction.Left))
                                {
                                    ghost.box.Image = GhostConstants.Images.Pinky.left2;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Right))
                                {
                                    ghost.box.Image = GhostConstants.Images.Pinky.right2;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Up))
                                {
                                    ghost.box.Image = GhostConstants.Images.Pinky.up2;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Down))
                                {
                                    ghost.box.Image = GhostConstants.Images.Pinky.down2;
                                }
                            }
                            else if (ghost.Equals(Inky))
                            {
                                if (ghost.currentDirection.Equals(Direction.Left))
                                {
                                    ghost.box.Image = GhostConstants.Images.Inky.left2;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Right))
                                {
                                    ghost.box.Image = GhostConstants.Images.Inky.right2;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Up))
                                {
                                    ghost.box.Image = GhostConstants.Images.Inky.up2;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Down))
                                {
                                    ghost.box.Image = GhostConstants.Images.Inky.down2;
                                }
                            }
                            else if (ghost.Equals(Clyde))
                            {
                                if (ghost.currentDirection.Equals(Direction.Left))
                                {
                                    ghost.box.Image = GhostConstants.Images.Clyde.left2;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Right))
                                {
                                    ghost.box.Image = GhostConstants.Images.Clyde.right2;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Up))
                                {
                                    ghost.box.Image = GhostConstants.Images.Clyde.up2;
                                }
                                else if (ghost.currentDirection.Equals(Direction.Down))
                                {
                                    ghost.box.Image = GhostConstants.Images.Clyde.down2;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (ghost.currentDirection.Equals(Direction.Left))
                        {
                            ghost.box.Image = GhostConstants.Images.eyesLeft;
                        }
                        else if (ghost.currentDirection.Equals(Direction.Right))
                        {
                            ghost.box.Image = GhostConstants.Images.eyesRight;
                        }
                        else if (ghost.currentDirection.Equals(Direction.Up))
                        {
                            ghost.box.Image = GhostConstants.Images.eyesUp;
                        }
                        else if (ghost.currentDirection.Equals(Direction.Down))
                        {
                            ghost.box.Image = GhostConstants.Images.eyesDown;
                        }
                    }
                }
            }
        }
    }
}