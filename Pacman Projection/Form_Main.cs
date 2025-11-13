using NAudio.Dmo;
using NAudio.Wave;
using Pacman_Projection.Properties;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Instrumentation;
using System.Media;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.XPath;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;


public enum GhostTemplate
{
    Blinky,
    Pinky,
    Inky,
    Clyde,
    TestGhost,
    Custom
}


public enum GhostBehaviour
{
    Scatter,
    Chase,
    Frightened,
    Returning,
    ExitingHouse
}

public enum PossiblePaths
{
    Up, 
    Down,
    Left,
    Right,
    UpThroughPortal,
    DownThroughPortal,
    LeftThroughPortal,
    RightThroughPortal
}

public enum Direction
{
    Stationary,
    Up,
    Down,
    Left,
    Right
}

public enum ImageType
{
    Stationary,
    Stationary2,
    Up,
    Up2,
    Down,
    Down2,
    Left,
    Left2,
    Right,
    Right2,
    FrightenedBlue,
    FrightenedBlue2,
    FrightenedWhite,
    FrightenedWhite2
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

public enum TeleportSide 
{
    None,
    Left,
    Right
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
        const int boxSize = GameConstants.BoxSize;
        const int step = GameConstants.Step;
        const int entitySize = GameConstants.EntitySize;
        const int boxOffset_Vertical = GameConstants.BoxOffset_Vertical;

        const int boxes_Horizontally = GameConstants.Boxes_Horizontally; 
        const int boxes_Vertically = GameConstants.Boxes_Vertically;

        const int gameBoxes_Horizontally = GameConstants.GameBoxes_Horizontally;
        const int gameBoxes_Vertically = GameConstants.GameBoxes_Vertically;

        //
        // Local variables
        //

        int level = 1; // Max level 10

        bool gamePaused;

        SoundManager soundManager = new SoundManager();

        Key pressedKey;
        Key registeredKey;

        System.Windows.Forms.Label label_Ready;
        System.Windows.Forms.Label label_GameOver;
        System.Windows.Forms.Label label_Level;
        System.Windows.Forms.Label label_FruitSpawnChance;
        System.Windows.Forms.Label label_Score;

        Box[,] boxes = new Box[GameConstants.Boxes_Horizontally, GameConstants.Boxes_Vertically];
        List<Box> walls = new List<Box>();
        
        Pacman pacman = new Pacman();
        bool pacPic_open;

        List<PictureBox> pacmanLives = new List<PictureBox>
        {
            new PictureBox(),
            new PictureBox(),
            new PictureBox(),
            new PictureBox()
        };

        // Declare list containing the ghosts
        List<Ghost> ghosts = new List<Ghost>();
        List<Ghost> ghostsFrightened = new List<Ghost>();

        int ghostsEatenDuringPeriod;
        bool ghostsToBlink;
        int currentEatGhostDuration;

        bool ghostPic_ver2;
        bool ghostFrightenedPic_ver2;

        GhostBehaviour mostRecentGlobalBehaviour;
        GhostBehaviour currentGlobalBehaviour;

        bool toChangeBehaviourSound = true; // Start as true so sound plays at the start of the game, before any behaviour changes
        int secondsOfSameBehaviour; 

        Box[,] gameGrid = new Box[GameConstants.GameBoxes_Horizontally, GameConstants.GameBoxes_Vertically];

        int foodEaten;
        int powerPelletsEaten;
        bool powerPellets_Filled;

        int foodOnMap;

        PictureBox fruitBox = new PictureBox();
        int fruitsSpawnedTotal;
        Fruit currentFruit;

        bool timersDisabled;

        public Form_Main(FormManager formManager, EventManager eventManager, GlobalVariables globalVariables)
        {
            InitializeComponent();
            this.formManager = formManager;
            this.eventManager = eventManager;
            this.globalVariables = globalVariables;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            ClientSize = new Size(GameConstants.Boxes_Horizontally * GameConstants.BoxSize, GameConstants.Boxes_Vertically * boxSize + boxSize);
            this.BackColor = Color.Black;
            this.Location = new Point(388, 57);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true;

            level = globalVariables.StartLevel;

            // Get all ghosts to use in the game from globalVariables
            ghosts = new List<Ghost>(globalVariables.Ghosts);

            UpdateTimerIntervals();

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

            // label_Score properties
            label_Score = new System.Windows.Forms.Label
            {
                Location = new Point(2, 2),
                Size = new Size(50, 20),
                Font = new Font("Pixelify Sans", 10, FontStyle.Bold),
                Text = "0",
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Popup
            };
            Controls.Add(label_Score);

            // label_Level properties
            label_Level = new System.Windows.Forms.Label
            {
                Location = new Point(boxSize * 12, 2),
                Size = new Size(120, 20),
                Font = new Font("Pixelify Sans", 12, FontStyle.Bold),
                Text = "Level " + level,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Popup
            };
            Controls.Add(label_Level);

            // label_FruitSpawnChance properties
            label_FruitSpawnChance = new System.Windows.Forms.Label
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
            Controls.Add(label_FruitSpawnChance);
            label_FruitSpawnChance.BringToFront();

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
            pacman.UpdateLocation();

            // fruitBox properties
            fruitBox.BackColor = Color.Transparent;
            fruitBox.Size = new Size(entitySize, entitySize);
            fruitBox.Location = new Point(PacConstants.StartX, PacConstants.StartY);
            fruitBox.SizeMode = PictureBoxSizeMode.StretchImage;
            Controls.Add(fruitBox);
            fruitBox.BringToFront();

            //                              //
            // ********** Ghosts ********** //
            //                              //

            foreach (Ghost ghost in ghosts)
            { 
                Controls.Add(ghost.box);
                Controls.Add(ghost.navBox);
                ghost.box.BringToFront();
                ghost.box.Hide();
            }

            //                             //
            // ********** Walls ********** //
            //                             //

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

            // Make the gate into the ghosts encolsure
            // Set the outer gates GhostsCanEnter to false to force ghosts returning to the ghost
            // house to go down when going to the return index to prevent them getting stuck
            boxes[13, 16].isGate = true;
            boxes[13, 16].GhostsCanEnter = false;
            boxes[13, 16].pictureBox.BackColor = Color.LightPink;
            boxes[14, 16].isGate = true;
            boxes[14, 16].pictureBox.BackColor = Color.LightPink;
            boxes[15, 16].isGate = true;
            boxes[15, 16].pictureBox.BackColor = Color.LightPink;
            boxes[16, 16].isGate = true;
            boxes[16, 16].GhostsCanEnter = false;
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

            // Set GhostsCanEnter of chosen boxes to false, so that ghosts cannot enter but pacman still can.
            // This it to prevent them from getting stuck in endless loops while attempting to reach their target

            boxes[17, 34].GhostsCanEnter = false;
            boxes[28, 34].GhostsCanEnter = false;
            boxes[25, 31].GhostsCanEnter = false;
            boxes[5, 4].GhostsCanEnter = false;
            boxes[3, 8].GhostsCanEnter = false;
            boxes[2, 3].GhostsCanEnter = false;
            boxes[7, 4].GhostsCanEnter = false;
            boxes[21, 3].GhostsCanEnter = false;
            boxes[18, 4].GhostsCanEnter = false;
            boxes[13, 10].GhostsCanEnter = false;
            boxes[10, 11].GhostsCanEnter = false;
            boxes[10, 13].GhostsCanEnter = false;
            boxes[19, 11].GhostsCanEnter = false;
            boxes[19, 13].GhostsCanEnter = false;
            boxes[16, 10].GhostsCanEnter = false;
            boxes[16, 12].GhostsCanEnter = false;
            boxes[13, 12].GhostsCanEnter = false;
            boxes[13, 13].GhostsCanEnter = false;
            boxes[14, 13].GhostsCanEnter = false;
            boxes[15, 13].GhostsCanEnter = false;
            boxes[16, 13].GhostsCanEnter = false;

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
                lifeBox.Location = new Point((GameConstants.Boxes_Horizontally - 2) * boxSize - (entitySize + 5) * pacmanLives.IndexOf(lifeBox), 0);
                Controls.Add(lifeBox);
                lifeBox.BringToFront(); 
            }

            // label_Ready properties
            label_Ready = new System.Windows.Forms.Label
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
            Controls.Add(label_Ready);
            label_Ready.BringToFront();


            // label_GameOver properties
            label_GameOver = new System.Windows.Forms.Label
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
            Controls.Add(label_GameOver);
            label_GameOver.BringToFront();
            label_GameOver.Hide();

            soundManager.PlaySound(Sounds.pacman_beginning, false);

            await Task.Delay(GameConstants.EventTimes.betweenGames);

            SetStart();

            UpdateTimerIntervals();
            ghostReturnTickTimer.Interval = GhostConstants.StandardReturnInterval;

            foreach (Ghost ghost in ghosts)
            {
                ghost.box.Show();
                ghost.box.BringToFront();
            }

            // Remove one life from pacman
            Controls.Remove(pacmanLives[pacmanLives.Count - 1]);
            pacmanLives.RemoveAt(pacmanLives.Count - 1);

            // Timed to be complete when the sound pacman_beginning has finished playing
            await Task.Delay(GameConstants.EventTimes.afterGhostsAppear);

            // Hide labelReady and start timers
            label_Ready.Hide();
            StartTimers();

            SetSound_Scatter();
            PlaceFruitLoop();
        }

        /// <summary>
        /// Updates all timer intervals to required intervals depending on the current level
        /// </summary>
        private void UpdateTimerIntervals()
        {
            pacTickTimer.Interval = PacConstants.SpeedForLevel[level];
            pacImageTimer.Interval = pacTickTimer.Interval;

            ghostTickTimer.Interval = GhostConstants.SpeedForLevel[level];
            ghostImageTimer.Interval = ghostTickTimer.Interval;

            ghostFrightenedTickTimer.Interval = GhostConstants.SpeedForLevel_Frightened[level];
            ghostFrightenedImageTimer.Interval = ghostFrightenedTickTimer.Interval;
        }

        //                                                                                        //
        //  ******************************  game-related methods  ******************************  //
        //                                                                                        //


        /// <summary>
        /// Set pacman and the ghosts to their respective startDirection, StartImage, starting behaviour and/or starting state. 
        /// </summary>
        private void SetStart()
        {
            ResetPacmanKey();
            pacman.box.Location = new Point(PacConstants.StartX, PacConstants.StartY);
            pacman.box.Image = Resources.Pacman_stationary;

            foreach (Ghost ghost in ghosts)
            {
                ghost.box.Location = new Point(ghost.StartX, ghost.StartY);
                ghost.SetState(EntityState.Standard);
                if (ghost.ExitingGhostHouse)
                {
                    ghost.SetBehaviour(GhostBehaviour.ExitingHouse);
                }
                else
                {
                    ghost.SetScatter();
                }
                ghost.box.Image = ghost.StartImage;
                ghost.SetDirection(ghost.StartDirection);
            }

            currentGlobalBehaviour = GhostBehaviour.Scatter;
        }

        private async void Game(bool win)
        {
            PauseGame(false);

            // Take all ghosts out of frightened mode
            foreach (Ghost ghost in ghostsFrightened)
            {
                ghost.SetBehaviour(GhostBehaviour.Scatter);
            }
            UpdateGhostLists();

            pacman.box.Image = Resources.Pacman_stationary;
            foreach (Ghost ghost in ghosts)
            {
                switch (ghost.Template)
                {
                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Blinky):
                        if (ghost.CurrentState.Equals(EntityState.Eaten))
                        {
                            ghost.box.Image = GhostConstants.Images.eyesStationary;
                        }
                        else
                        {
                            ghost.box.Image = GhostConstants.Images.Blinky.stationary;
                        }
                        break;
                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Pinky):
                        if (ghost.CurrentState.Equals(EntityState.Eaten))
                        {
                            ghost.box.Image = GhostConstants.Images.eyesStationary;
                        }
                        else
                        {
                            ghost.box.Image = GhostConstants.Images.Pinky.stationary;
                        }
                        break;
                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Inky):
                        if (ghost.CurrentState.Equals(EntityState.Eaten))
                        {
                            ghost.box.Image = GhostConstants.Images.eyesStationary;
                        }
                        else
                        {
                            ghost.box.Image = GhostConstants.Images.Inky.stationary;
                        }
                        break;
                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Clyde):
                        if (ghost.CurrentState.Equals(EntityState.Eaten))
                        {
                            ghost.box.Image = GhostConstants.Images.eyesStationary;
                        }
                        else
                        {
                            ghost.box.Image = GhostConstants.Images.Clyde.stationary;
                        }
                        break;
                        /* default: set custom ghost picture 
                         * 
                         if (ghost.CurrentState.Equals(EntityState.Eaten))
                            {

                            }
                            else
                            {

                            }
                        */
                }
            }
               
            await Task.Delay(GameConstants.EventTimes.betweenGames);

            foreach (Ghost ghost in ghosts)
            {
                ghost.box.Hide();
            }
            fruitBox.Hide();

            if (win)
            {
                level++;

                soundManager.PlaySound(Sounds.pacman_win, false);

                // Hid the fruitSpawnChange label so it doesn't cover any blinking walls
                label_FruitSpawnChance.Hide();

                int timesToBlink = 10;
                while (timesToBlink > 0)
                {
                    foreach (var box in walls)
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

                    label_GameOver.Show();
                    label_GameOver.BringToFront();

                    Task.Delay(GameConstants.EventTimes.gameOverDisplayed).Wait();

                    // Register the end level reached for the highscore table
                    globalVariables.EndLevel = level;

                    formManager.OpenForm(formManager.form_PauseMenu);
                }
            }
            else
            {
                restart = true;
            }

            if (restart)
            {
                SetStart();

                if (win)
                {
                    // Reset all variables  
                    foodEaten = 0;
                    powerPelletsEaten = 0;
                    currentEatGhostDuration = 0;
                    fruitsSpawnedTotal = 0;

                    PlaceAllFood();

                    // Update the all timer intervals to match the next 
                    UpdateTimerIntervals();

                    label_Level.Text = "Level " + level.ToString();
                }

                foreach (Ghost ghost in ghosts)
                {
                    ghost.box.Show();
                    ghost.box.BringToFront();
                }
                pacman.box.Show();
                pacman.box.BringToFront();

                fruitBox.Show();
                label_FruitSpawnChance.Show();

                label_Ready.Show();
                label_Ready.BringToFront();

                await Task.Delay(GameConstants.EventTimes.beforeRestart);

                label_Ready.Hide();

                UnpauseGame();
            }
        }

        private void UpdateScore(int scoreToChangeBy, bool addToScore)
        {
            if (addToScore)
            {
                globalVariables.Score += scoreToChangeBy;
                label_Score.Text = globalVariables.Score.ToString();
            }
            else
            {
                globalVariables.Score -= scoreToChangeBy;
                label_Score.Text = globalVariables.Score.ToString();
            }
        }

        private async Task UpdateScore(int scoreToChangeBy, bool addToScore, bool createLabelScoreChange, PictureBox entityToPutLabelAt)
        {
            if (createLabelScoreChange)
            {
                System.Windows.Forms.Label labelScoreChange = new System.Windows.Forms.Label();
                labelScoreChange.Location = new Point(entityToPutLabelAt.Location.X, entityToPutLabelAt.Location.Y);
                labelScoreChange.Size = new Size(40, 30);
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

                label_Score.Text = globalVariables.Score.ToString();

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
                formManager.OpenForm(formManager.form_PauseMenu);
            }
        }

        internal void UnpauseGame()
        {
            timersDisabled = false;

            soundManager.UnpauseAllSounds();

            while (soundManager.pausedSounds.Count > 0)
            {
                int index = soundManager.pausedSounds.Count - 1;
                var sound = soundManager.pausedSounds.ElementAt(index).Key;

                soundManager.UnpauseSound(sound);
            }

            this.KeyPreview = true;

            StartTimers();
            gamePaused = false;
        }

        private void StopTimers()
        {
            pacTickTimer.Stop();
            pacImageTimer.Stop();
            ghostTickTimer.Stop();
            ghostFrightenedTickTimer.Stop();
            ghostReturnTickTimer.Stop();
            ghostImageTimer.Stop();
            ghostFrightenedImageTimer.Stop();
            powerPelletBlinkTimer.Stop();
            ghostBehaviourTimeTimer.Stop();
        }

        private void StartTimers()
        {
            if (!timersDisabled)
            {
                pacTickTimer.Start();
                pacImageTimer.Start();
                ghostTickTimer.Start();
                ghostFrightenedTickTimer.Start();
                ghostReturnTickTimer.Start();
                ghostImageTimer.Start();
                ghostFrightenedImageTimer.Start();
                powerPelletBlinkTimer.Start();
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
            // Ensure all ghosts are placed in the correct list before checking for any collisions with pacman
            UpdateGhostLists();

            // Loop through all non-frightened ghosts to check if pacman is colliding with any, and, if he is, if he should die
            foreach (Ghost ghost in ghosts)
            {
                if (pacman.box.Bounds.IntersectsWith(ghost.box.Bounds))
                {
                    if (!ghost.CurrentState.Equals(EntityState.Eaten))
                    {
                        
                        Game(false);
                        return;
                    }
                    else
                    {
                        pacman.box.BringToFront();
                        return;
                    }
                }
            }

            // Loop through all frightened ghosts to check if pacman is colliding with any, and, if he is, he should eat them
            foreach (Ghost ghost in ghostsFrightened)
            {
                if (pacman.box.Bounds.IntersectsWith(ghost.box.Bounds))
                {
                    GhostEaten(ghost);
                    return;
                }
            }

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
            // Cannot change directions while teleporting
            if (registeredKey != pressedKey && !pacman.CurrentState.Equals(EntityState.Teleporting))
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
                    if (!pacman.CurrentDirection.Equals(Direction.Left))
                    {
                        pacman.SetDirection(Direction.Left);
                    }
                    // Check if pacman is inside a teleporter box 
                    if ((CheckForTeleporter(pacman) && !pacman.Teleported) || pacman.CurrentState.Equals(EntityState.Teleporting))
                    {
                        if (!pacman.CurrentState.Equals(EntityState.Teleporting))
                        {
                            pacman.SetState(EntityState.Teleporting);
                        }

                        pacman.box.Left -= step;

                        if (!pacman.Teleported)
                        {
                            // If pacman has not teleported yet, he is entering the teleporter
                            pacman.BlocksIntoTeleporter++;
                        }
                        else
                        { 
                            // Otherwise, pacman has teleported and is exiting the teleporter instead
                            pacman.BlocksIntoTeleporter--;
                        }

                        // If pacman is fully inside the teleporter box (going to teleport) or if he has 
                        // teleported (going out of teleporter) either teleport or check if pacman is out of the teleporter
                        if (pacman.BlocksIntoTeleporter.Equals(GameConstants.MaxStepsIntoTeleporter) || pacman.Teleported)
                        {
                            // If pacman has teleported, he is now exiting the teleporter
                            // If pacman is out of the teleporter, his state should switch back to standard
                            if (pacman.Teleported && pacman.BlocksIntoTeleporter == 0)
                            {
                                pacman.SetState(EntityState.Standard);
                                pacman.Teleported = false;

                                // Update pacman's standard positions manually when exiting teleporting state
                                // so all collision logic works as intended again
                                pacman.UpdateStandardPositions();
                            }

                            // If pacman hasn't teleported yet, but is in the teleporting state, teleport him
                            if (!pacman.Teleported && pacman.CurrentState.Equals(EntityState.Teleporting))
                            {
                                pacman.box.Left = boxes_Horizontally * boxSize;
                                pacman.Teleported = true;
                            }
                        }
                    }
                    else if (!CheckForWall(pacman) && !pacman.CurrentState.Equals(EntityState.Teleporting))
                    {
                        pacman.box.Left -= step;
                    }
                    else
                    {
                        StopEntityMovement(pacman);
                    }
                    break;
                case Key.Right:
                    {
                        if (!pacman.CurrentDirection.Equals(Direction.Right))
                        {
                            pacman.SetDirection(Direction.Right);
                        }
                        // Check if pacman is inside a teleporter box 
                        if ((CheckForTeleporter(pacman) && !pacman.Teleported) || pacman.CurrentState.Equals(EntityState.Teleporting))
                        {
                            if (!pacman.CurrentState.Equals(EntityState.Teleporting))
                            {
                                pacman.SetState(EntityState.Teleporting);
                            }

                            pacman.box.Left += step;

                            if (!pacman.Teleported)
                            {
                                // If pacman has not teleported yet, he is entering the teleporter
                                pacman.BlocksIntoTeleporter++;
                            }
                            else
                            {
                                // Otherwise, pacman has teleported and is exiting the teleporter instead
                                pacman.BlocksIntoTeleporter--;
                            }

                            // If pacman is fully inside the teleporter box (going to teleport) or if he has 
                            // teleported (going out of teleporter) either teleport or check if pacman is out of the teleporter
                            if (pacman.BlocksIntoTeleporter.Equals(GameConstants.MaxStepsIntoTeleporter) || pacman.Teleported)
                            {
                                // If pacman has teleported, he is now exiting the teleporter
                                // If pacman is out of the teleporter, his state should switch back to standard
                                if (pacman.Teleported && pacman.BlocksIntoTeleporter == 0)
                                {
                                    pacman.SetState(EntityState.Standard);
                                    pacman.Teleported = false;

                                    // Update pacman's standard positions manually when exiting teleporting state
                                    // so all collision logic works as intended again
                                    pacman.UpdateStandardPositions();
                                }

                                // If pacman hasn't teleported yet, but is in the teleporting state, teleport him
                                if (!pacman.Teleported && pacman.CurrentState.Equals(EntityState.Teleporting))
                                {
                                    pacman.box.Left = -boxSize*2;
                                    pacman.Teleported = true;
                                }
                            }
                        }
                        else if (!CheckForWall(pacman) && !pacman.CurrentState.Equals(EntityState.Teleporting))
                        {
                            pacman.box.Left += step;
                        }
                        else
                        {
                            StopEntityMovement(pacman);
                        }
                    }
                    break;
                case Key.Up:
                    {
                        if (!pacman.CurrentDirection.Equals(Direction.Up))
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
                        if (!pacman.CurrentDirection.Equals(Direction.Down))
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

            // After pacman has moved, update all ghost's target tiles
            foreach (Ghost ghost in ghosts)
            {
                // Only update the ghost's target if it is not leaving
                // the ghost house - all ghosts have to leave before updating their target tile
                if (!ghost.ExitingGhostHouse)
                {
                    UpdateGhostTargets();
                }
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
        private bool CheckForWall(Entity entity)
        {
            switch (entity.CurrentDirection)
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
                    try
                    {
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
                    }
                    catch
                    {
                        return true;
                    }
                case Direction.Right:
                    try
                    {
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
                    }
                    catch
                    {
                        return true;
                    }
                case Direction.Up:
                    try
                    {
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
                    }
                    catch                     
                    {
                        return true;
                    }
                case Direction.Down:
                    try
                    {
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
                    }
                    catch
                    {
                        return true;
                    }
                default:
                    return true;
            }
        }

        private bool CheckForGate(Entity entity)
        {
            if (entity.CurrentDirection.Equals(Direction.Up))
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
            else if (entity.CurrentDirection.Equals(Direction.Down))
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

        /// <summary>
        /// Used to check if an entity (ghost) can enter a specific box, even though the box is not a wall nor gate. 
        /// Returns true if entry is allowed, false if not.
        /// </summary>
        private bool CheckForEntry(Entity entity, Direction directionToCheckIn)
        {
            switch (directionToCheckIn)
            {
                case Direction.Left:
                    int[] lowerLeft = entity.GetStandardPosition(EntityBox.LowerLeft);
                    int[] upperLeft = entity.GetStandardPosition(EntityBox.UpperLeft);

                    // Adjust positions for direction
                    lowerLeft[0] -= 1;
                    upperLeft[0] -= 1;

                    if (boxes[lowerLeft[0], lowerLeft[1]].GhostsCanEnter && boxes[upperLeft[0], upperLeft[1]].GhostsCanEnter)
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

                    if (boxes[lowerRight[0], lowerRight[1]].GhostsCanEnter && boxes[upperRight[0], upperRight[1]].GhostsCanEnter)
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

                    if (boxes[upperLeft_Up[0], upperLeft_Up[1]].GhostsCanEnter && boxes[upperRight_Up[0], upperRight_Up[1]].GhostsCanEnter)
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

                    if (boxes[lowerLeft_Down[0], lowerLeft_Down[1]].GhostsCanEnter && boxes[lowerRight_Down[0], lowerRight_Down[1]].GhostsCanEnter)
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
            try
            {
                // An entity can only enter a teleporter while going left or right
                if (entity.CurrentDirection.Equals(Direction.Left))
                {
                    int[] lowerLeft = entity.GetStandardPosition(EntityBox.LowerLeft);
                    int[] upperLeft = entity.GetStandardPosition(EntityBox.UpperLeft);

                    // Don't adjust the index according to direction, since the teleporter is entered from the current position 
                    // If adjusted, lowerLeft & upperLeft would be outside the teleporter box, outside the bounds of the array (map) and cause an error

                    if (boxes[lowerLeft[0], lowerLeft[1]].isTeleporter && boxes[upperLeft[0], upperLeft[1]].isTeleporter)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (entity.CurrentDirection.Equals(Direction.Right))
                {
                    int[] lowerRight = entity.GetStandardPosition(EntityBox.LowerRight);
                    int[] upperRight = entity.GetStandardPosition(EntityBox.UpperRight);

                    // Don't adjust the index according to direction, since the teleporter is entered from the current position 
                    // If adjusted, lowerRight & upperRight would be outside the teleporter box, outside the bounds of the array (map) and cause an error

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
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }

        //                                                                                                //
        //  ******************************  Food & fruit-related methods  ******************************  //
        //                                                                                                //

        private void PlaceAllFood()
        {
            // Fills the food list and places it on the map while
            // checking if it collides with any walls, if so, they are removed
            for (int indexY = 0; indexY < gameBoxes_Vertically; indexY++)
            {
                for (int indexX = 0; indexX < gameBoxes_Horizontally; indexX++)
                {
                    var foodElement = gameGrid[indexX, indexY];
                    var foodIndex = new int[] { indexX, indexY };

                    // Only declare the foods the first time PlaceAllFood is run
                    if (gameGrid[indexX, indexY] == null)
                    { 
                        bool powerPellet = false;
                        foreach (var index in GameConstants.PowerPelletIndexes)
                        {
                            if (foodIndex[0] == index[0] && foodIndex[1] == index[1]) 
                            { 
                                powerPellet = true; 
                            }
                        }
                        if (powerPellet)
                        {
                            foodElement = new Box(new PictureBox(), false, false, false, true, true);
                            foodElement.pictureBox.Image = Resources.PowerPellet_Filled;
                            foodElement.isEaten = false;

                            gameGrid[indexX, indexY] = foodElement;
                        }
                        else
                        {
                            foodElement = new Box(new PictureBox(), false, false, false, true, false);
                            foodElement.pictureBox.Image = Resources.Food;
                            foodElement.isEaten = false;

                            gameGrid[indexX, indexY] = foodElement;
                        }

                        foodElement.pictureBox.Size = new Size(boxSize, boxSize);
                        Controls.Add(foodElement.pictureBox);

                        // Place all foods in a grid-pattern over the map
                        // If a food collides with a wall, it will be removed
                        // The same applies to foods that are placed beside others foods,
                        // creating areas of dense foods, as well as foods placed outside the map or generally where they are not supposed to be
                        foodElement.pictureBox.Location = new Point(indexX * boxSize + GameConstants.GameGridOffset_Horizontal, indexY * boxSize + GameConstants.GameGridOffset_Vertical);

                        if (AbleToPlaceFood(indexX, indexY))
                        {   
                            foodElement.pictureBox.BringToFront();
                            foodOnMap++;  
                        }
                    }
                    else
                    {
                        if (GameConstants.PowerPelletIndexes.Contains(foodIndex))
                        {
                            foodElement.pictureBox.Image = Resources.PowerPellet_Filled;
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
                if (gameGrid[indexX, indexY].pictureBox.Bounds.IntersectsWith(box.pictureBox.Bounds) && (box.isWall || !box.isFood))
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
                for (int indexX = 0; indexX < gameBoxes_Horizontally; indexX++)
                {
                    for (int indexY = 0; indexY < gameBoxes_Vertically; indexY++)
                    {
                        var food = gameGrid[indexX, indexY];
                        if (food.pictureBox.Bounds.IntersectsWith(eatBox.Bounds) && !food.isEaten && food.pictureBox.Visible)
                        {
                            return gameGrid[indexX, indexY];
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
                for (int indexX = 0; indexX < gameBoxes_Horizontally; indexX++)
                {
                    for (int indexY = 0; indexY < gameBoxes_Vertically; indexY++)
                    {
                        if (gameGrid[indexX, indexY] != null)
                        {
                            if (gameGrid[indexX, indexY].pictureBox.Bounds.IntersectsWith(eatBox.Bounds) && gameGrid[indexX, indexY].pictureBox.Image != null)
                            {
                                if (gameGrid[indexX, indexY].isPowerPellet == false)
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
                        // If the ghosts are already scared, reset their frightened-related status
                        if (currentGlobalBehaviour.Equals(GhostBehaviour.Frightened))
                        {
                            foreach (Ghost ghost in ghosts)
                            {
                                ghost.HasBeenEaten = false;
                            }
                            
                            foreach (Ghost ghost in ghostsFrightened)
                            {
                                ghost.White = false;
                            }
                            ghostsToBlink = false;
                        }

                        currentEatGhostDuration += GameConstants.EventTimes.powerPellet;                   

                        // Ensure all ghosts are frightened
                        SetGhosts_Frightened();

                        ghostBehaviourTimeTimer.Stop();

                        UpdateScore(GameConstants.Scores.powerPellet, true);
                        foodBox.Eaten();

                        powerPelletsEaten++;
                    }
                }
                foodEaten++;
                foodOnMap--;

                // If all food is eaten, the player wins
                if (foodOnMap == 0)
                {
                    Game(true);
                }
            }
        }

        private void powerPelletBlinkTimer_Tick(object sender, EventArgs e)
        {
            powerPellets_Filled = !powerPellets_Filled;

            foreach (var index in GameConstants.PowerPelletIndexes)
            {
                if (!gameGrid[index[0], index[1]].isEaten)
                {
                    if (powerPellets_Filled)
                    {
                        gameGrid[index[0], index[1]].pictureBox.Image = Resources.PowerPellet_Empty;
                    }
                    else
                    {
                        gameGrid[index[0], index[1]].pictureBox.Image = Resources.PowerPellet_Filled;
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
                    label_FruitSpawnChance.Text = Convert.ToInt32(fruitSpawnChancePercent * 100).ToString() + "%";
                }
                else
                {
                    label_FruitSpawnChance.Text = "0%";
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

            // Switch from scatter if the max time for the level is met
            if (currentGlobalBehaviour.Equals(GhostBehaviour.Scatter))
            {
                if (secondsOfSameBehaviour >= int.Parse(GhostConstants.ScatterChaseTimesForLevel[level].Split(',')[0]))
                {
                    SetGhosts_Chase(false);

                    secondsOfSameBehaviour = 0;
                    behaviourChangeThisTick = true;
                }
            }
            else if (currentGlobalBehaviour.Equals(GhostBehaviour.Chase))
            {
                // Switch from chase if the max time for the level is met
                if (secondsOfSameBehaviour >= int.Parse(GhostConstants.ScatterChaseTimesForLevel[level].Split(',')[1]))
                {
                    SetGhosts_Scatter(false);

                    secondsOfSameBehaviour = 0;
                    behaviourChangeThisTick = true;
                }
            }

            if (!behaviourChangeThisTick)
            {
                secondsOfSameBehaviour++;
            }
        }

        private void ghostTickTimer_Tick(object sender, EventArgs e)
        {
            // Before looping through all ghost behaviours, set all ghosts who 
            // are going to teleport into the teleporting state
            foreach (Ghost ghost in ghosts)
            {
                if (CheckForTeleporter(ghost))
                {
                    ghost.SetState(EntityState.Teleporting);
                }
            }

            foreach (Ghost ghost in ghosts)
            {
                if (ghost.Template.Equals(GhostTemplate.Pinky))
                {

                }

                // If a ghost is inside the ghost house but hasn't updated it's target, update it
                if (ghost.ExitingGhostHouse && (ghost.TargetPosX != GhostConstants.OutOfHouseIndex[0] && ghost.TargetPosY != GhostConstants.OutOfHouseIndex[1]))
                {
                    ghost.SetTarget(GhostConstants.OutOfHouseIndex);
                    continue;
                }

                // After reaching the OutOfHouseIndex the ghost is no longer inside the ghost house
                if (ghost.CurrentPosX == GhostConstants.OutOfHouseIndex[0] && ghost.CurrentPosY == GhostConstants.OutOfHouseIndex[1] && ghost.ExitingGhostHouse)
                {
                    ghost.ExitingGhostHouse = false;
                    ghost.BehaviourOverridden = false;
                    ghost.SetBehaviour(currentGlobalBehaviour);
                    UpdateGhostTarget(ghost);
                }
                
                switch (ghost.CurrentState)
                {
                    case EntityState.Standard:
                        
                        switch (ghost.CurrentDirection)
                        {
                            case Direction.Left:
                                if (!CheckForPacman(ghost))
                                {
                                    if (!CheckForWall(ghost))
                                    {
                                        ghost.box.Left -= step;
                                    }

                                    // Get all available directions the ghost can move in from its current position
                                    var directions = CheckForDirections(ghost);

                                    // Create a dictionary to hold the distances for each possible path
                                    Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>> distancesToTarget = new Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>>();

                                    // Use a "testGhost" to measure distances to the ghost's target tile in each direction the ghost can move in
                                    Ghost testGhost = new Ghost(GhostTemplate.TestGhost);
                                    testGhost.box.Size = ghost.box.Size;
                                    testGhost.box.Location = ghost.box.Location;
                                    Controls.Add(testGhost.box);

                                    testGhost.navBox.Size = ghost.navBox.Size;
                                    Controls.Add(testGhost.navBox);

                                    if (directions[Direction.Up])
                                    {
                                        testGhost.box.Top -= step;
                                        testGhost.UpdateLocation();

                                        distancesToTarget[PossiblePaths.Up] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));  
                                            
                                        var distanceThroughClosestPortal = CalculateDistanceToTargetThroughPortal(testGhost.CurrentPos, ghost.TargetPos);
                                        distancesToTarget[PossiblePaths.UpThroughPortal] = new KeyValuePair<TeleportSide, double>(distanceThroughClosestPortal.leftOrRightPortal, distanceThroughClosestPortal.distance);

                                        testGhost.box.Top += step;
                                        testGhost.UpdateLocation();
                                    }
                                    if (directions[Direction.Down])
                                    {
                                        testGhost.box.Top += step;
                                        testGhost.UpdateLocation();

                                        distancesToTarget[PossiblePaths.Down] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                        var distanceThroughClosestPortal = CalculateDistanceToTargetThroughPortal(testGhost.CurrentPos, ghost.TargetPos);
                                        distancesToTarget[PossiblePaths.DownThroughPortal] = new KeyValuePair<TeleportSide, double>(distanceThroughClosestPortal.leftOrRightPortal, distanceThroughClosestPortal.distance);

                                        testGhost.UpdateLocation();
                                        testGhost.box.Top -= step;
                                    }
                                    if (directions[Direction.Left])
                                    {
                                        testGhost.box.Left -= step;
                                        testGhost.UpdateLocation();

                                        distancesToTarget[PossiblePaths.Left] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                        var distanceThroughClosestPortal = CalculateDistanceToTargetThroughPortal(testGhost.CurrentPos, ghost.TargetPos);
                                        distancesToTarget[PossiblePaths.LeftThroughPortal] = new KeyValuePair<TeleportSide, double>(distanceThroughClosestPortal.leftOrRightPortal, distanceThroughClosestPortal.distance);
                                    }

                                    testGhost.box.Dispose();
                                    Controls.Remove(testGhost.box);

                                    var path = GetPathToFollow(ghost.CurrentDirection, distancesToTarget);
                                    ghost.SetPath(path.pathToTake, path.portalToTake);
                                }
                                else if (!ghost.CurrentBehaviour.Equals(GhostBehaviour.Frightened))
                                {
                                    ghost.box.Left -= step;
                                    Game(false);
                                }
                                break;
                            case Direction.Right:
                                if (!CheckForPacman(ghost))
                                {
                                    if (!CheckForWall(ghost))
                                    {
                                        ghost.box.Left += step;
                                    }

                                    // Get all available directions the ghost can move in from its current position
                                    var directions = CheckForDirections(ghost);

                                    // Create a dictionary to hold the distances for each possible path
                                    Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>> distancesToTarget = new Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>>();

                                    // Use a "testGhost" to measure distances to the ghost's target tile in each direction the ghost can move in
                                    Ghost testGhost = new Ghost(GhostTemplate.TestGhost);
                                    testGhost.box.Size = ghost.box.Size;
                                    testGhost.box.Location = ghost.box.Location;
                                    testGhost.UpdateLocation();
                                    Controls.Add(testGhost.box);

                                    testGhost.navBox.Size = ghost.navBox.Size;
                                    Controls.Add(testGhost.navBox);

                                    if (directions[Direction.Up])
                                    {
                                        testGhost.box.Top -= step;
                                        testGhost.UpdateLocation();

                                        distancesToTarget[PossiblePaths.Up] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                        var distanceThroughClosestPortal = CalculateDistanceToTargetThroughPortal(testGhost.CurrentPos, ghost.TargetPos);
                                        distancesToTarget[PossiblePaths.UpThroughPortal] = new KeyValuePair<TeleportSide, double>(distanceThroughClosestPortal.leftOrRightPortal, distanceThroughClosestPortal.distance);

                                        testGhost.box.Top += step;
                                        testGhost.UpdateLocation();
                                    }
                                    if (directions[Direction.Down])
                                    {
                                        testGhost.box.Top += step;
                                        testGhost.UpdateLocation();

                                        distancesToTarget[PossiblePaths.Down] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                        var distanceThroughClosestPortal = CalculateDistanceToTargetThroughPortal(testGhost.CurrentPos, ghost.TargetPos);
                                        distancesToTarget[PossiblePaths.DownThroughPortal] = new KeyValuePair<TeleportSide, double>(distanceThroughClosestPortal.leftOrRightPortal, distanceThroughClosestPortal.distance);

                                        testGhost.box.Top -= step;
                                        testGhost.UpdateLocation();
                                    }
                                    if (directions[Direction.Right])
                                    {
                                        testGhost.box.Left += step;
                                        testGhost.UpdateLocation();

                                        distancesToTarget[PossiblePaths.Right] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                        var distanceThroughClosestPortal = CalculateDistanceToTargetThroughPortal(testGhost.CurrentPos, ghost.TargetPos);
                                        distancesToTarget[PossiblePaths.DownThroughPortal] = new KeyValuePair<TeleportSide, double>(distanceThroughClosestPortal.leftOrRightPortal, distanceThroughClosestPortal.distance);
                                    }

                                    testGhost.box.Dispose();
                                    Controls.Remove(testGhost.box);

                                    var path = GetPathToFollow(ghost.CurrentDirection, distancesToTarget);
                                    ghost.SetPath(path.pathToTake, path.portalToTake);
                                }
                                else if (!ghost.CurrentBehaviour.Equals(GhostBehaviour.Frightened))
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

                                    // Get all available directions the ghost can move in from its current position
                                    var directions = CheckForDirections(ghost);

                                    // Create a dictionary to hold the distances for each possible path
                                    Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>> distancesToTarget = new Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>>();

                                    // Use a "testGhost" to measure distances to the ghost's target tile in each direction the ghost can move in
                                    Ghost testGhost = new Ghost(GhostTemplate.TestGhost);
                                    testGhost.box.Size = ghost.box.Size;
                                    testGhost.box.Location = ghost.box.Location;
                                    testGhost.UpdateLocation();
                                    Controls.Add(testGhost.box);

                                    testGhost.navBox.Size = ghost.navBox.Size;
                                    Controls.Add(testGhost.navBox);

                                    if (directions[Direction.Up])
                                    {
                                        testGhost.box.Top -= step;
                                        testGhost.UpdateLocation();

                                        distancesToTarget[PossiblePaths.Up] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                        var distanceThroughClosestPortal = CalculateDistanceToTargetThroughPortal(testGhost.CurrentPos, ghost.TargetPos);
                                        distancesToTarget[PossiblePaths.UpThroughPortal] = new KeyValuePair<TeleportSide, double>(distanceThroughClosestPortal.leftOrRightPortal, distanceThroughClosestPortal.distance);

                                        testGhost.box.Top += step;
                                        testGhost.UpdateLocation();
                                    }
                                    if (directions[Direction.Right])
                                    {
                                        testGhost.box.Left += step;
                                        testGhost.UpdateLocation();

                                        distancesToTarget[PossiblePaths.Right] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                        var distanceThroughClosestPortal = CalculateDistanceToTargetThroughPortal(testGhost.CurrentPos, ghost.TargetPos);
                                        distancesToTarget[PossiblePaths.RightThroughPortal] = new KeyValuePair<TeleportSide, double>(distanceThroughClosestPortal.leftOrRightPortal, distanceThroughClosestPortal.distance);

                                        testGhost.box.Left -= step;
                                        testGhost.UpdateLocation();
                                    }
                                    if (directions[Direction.Left])
                                    {
                                        testGhost.box.Left -= step;
                                        testGhost.UpdateLocation();

                                        distancesToTarget[PossiblePaths.Left] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                        var distanceThroughClosestPortal = CalculateDistanceToTargetThroughPortal(testGhost.CurrentPos, ghost.TargetPos);
                                        distancesToTarget[PossiblePaths.LeftThroughPortal] = new KeyValuePair<TeleportSide, double>(distanceThroughClosestPortal.leftOrRightPortal, distanceThroughClosestPortal.distance);
                                    }

                                    testGhost.box.Dispose();
                                    Controls.Remove(testGhost.box);

                                    var path = GetPathToFollow(ghost.CurrentDirection, distancesToTarget);
                                    ghost.SetPath(path.pathToTake, path.portalToTake);
                                }
                                else if (!ghost.CurrentBehaviour.Equals(GhostBehaviour.Frightened))
                                {
                                    ghost.box.Top -= step;
                                    Game(false);
                                }
                                break;
                            case Direction.Down:
                                if (!CheckForPacman(ghost))
                                {
                                    if (!CheckForWall(ghost) && !CheckForGate(ghost))
                                    {
                                        ghost.box.Top += step;
                                    }

                                    // Get all available directions the ghost can move in from its current position
                                    var directions = CheckForDirections(ghost);

                                    // Create a dictionary to hold the distances for each possible path
                                    Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>> distancesToTarget = new Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>>();

                                    // Use a "testGhost" to measure distances to the ghost's target tile in each direction the ghost can move in
                                    Ghost testGhost = new Ghost(GhostTemplate.TestGhost);
                                    testGhost.box.Size = ghost.box.Size;
                                    testGhost.box.Location = ghost.box.Location;
                                    testGhost.UpdateLocation();
                                    Controls.Add(testGhost.box);

                                    testGhost.navBox.Size = ghost.navBox.Size;
                                    Controls.Add(testGhost.navBox);

                                    if (directions[Direction.Down])
                                    {
                                        testGhost.box.Top += step;
                                        testGhost.UpdateLocation();

                                        distancesToTarget[PossiblePaths.Down] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                        var distanceThroughClosestPortal = CalculateDistanceToTargetThroughPortal(testGhost.CurrentPos, ghost.TargetPos);
                                        distancesToTarget[PossiblePaths.DownThroughPortal] = new KeyValuePair<TeleportSide, double>(distanceThroughClosestPortal.leftOrRightPortal, distanceThroughClosestPortal.distance);

                                        testGhost.box.Top -= step;
                                        testGhost.UpdateLocation();
                                    }
                                    if (directions[Direction.Left])
                                    {
                                        testGhost.box.Left -= step;
                                        testGhost.UpdateLocation();

                                        distancesToTarget[PossiblePaths.Left] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                        var distanceThroughClosestPortal = CalculateDistanceToTargetThroughPortal(testGhost.CurrentPos, ghost.TargetPos);
                                        distancesToTarget[PossiblePaths.LeftThroughPortal] = new KeyValuePair<TeleportSide, double>(distanceThroughClosestPortal.leftOrRightPortal, distanceThroughClosestPortal.distance);

                                        testGhost.box.Left += step;
                                        testGhost.UpdateLocation();
                                    }
                                    if (directions[Direction.Right])
                                    {
                                        testGhost.box.Left += step;
                                        testGhost.UpdateLocation();

                                        distancesToTarget[PossiblePaths.Right] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                        var distanceThroughClosestPortal = CalculateDistanceToTargetThroughPortal(testGhost.CurrentPos, ghost.TargetPos);
                                        distancesToTarget[PossiblePaths.RightThroughPortal] = new KeyValuePair<TeleportSide, double>(distanceThroughClosestPortal.leftOrRightPortal, distanceThroughClosestPortal.distance);
                                    }

                                    testGhost.box.Dispose();
                                    Controls.Remove(testGhost.box);

                                    var path = GetPathToFollow(ghost.CurrentDirection, distancesToTarget);
                                    ghost.SetPath(path.pathToTake, path.portalToTake);
                                }
                                else if (!ghost.CurrentBehaviour.Equals(GhostBehaviour.Frightened))
                                {
                                    ghost.box.Top += step;
                                    Game(false);
                                }
                                break;
                        }
                        
                        break;
                    case EntityState.Teleporting:
                        if (ghost.CurrentDirection.Equals(Direction.Left))
                        {
                            ghost.box.Left -= step;

                            if (!ghost.Teleported)
                            {
                                // If the ghost has not teleported yet, it is entering the teleporter
                                ghost.BlocksIntoTeleporter++;
                            }
                            else
                            {
                                // Otherwise, the ghost has teleported and is exiting the teleporter instead
                                ghost.BlocksIntoTeleporter--;
                            }

                            // If the ghost is fully inside the teleporter box (going to teleport) or if it has 
                            // teleported (going out of teleporter) either teleport or check if the ghost is out of the teleporter
                            if (ghost.BlocksIntoTeleporter.Equals(GameConstants.MaxStepsIntoTeleporter) || ghost.Teleported)
                            {
                                // If the ghost has teleported, it is now exiting the teleporter
                                // If the ghost is out of the teleporter, it's state should switch back to standard
                                if (ghost.Teleported && ghost.BlocksIntoTeleporter == 0)
                                {
                                    ghost.SetState(EntityState.Standard);
                                    ghost.Teleported = false;

                                    // Update the ghost's standard positions manually when exiting teleporting state
                                    // so all collision logic works as intended again
                                    ghost.UpdateStandardPositions();
                                }

                                // If ghost hasn't teleported yet, but is in the teleporting state, teleport it
                                if (!ghost.Teleported && ghost.CurrentState.Equals(EntityState.Teleporting))
                                {
                                    ghost.box.Left = boxes_Horizontally * boxSize;
                                    ghost.Teleported = true;
                                }
                            }
                        }
                        else if (ghost.CurrentDirection.Equals(Direction.Right))
                        {
                            ghost.box.Left += step;

                            if (!ghost.Teleported)
                            {
                                // If the ghost has not teleported yet, it is entering the teleporter
                                ghost.BlocksIntoTeleporter++;
                            }
                            else
                            {
                                // Otherwise, the ghost has teleported and is exiting the teleporter instead
                                ghost.BlocksIntoTeleporter--;
                            }

                            // If the ghost is fully inside the teleporter box (going to teleport) or if it has 
                            // teleported (going out of teleporter) either teleport or check if the ghost is out of the teleporter
                            if (ghost.BlocksIntoTeleporter.Equals(GameConstants.MaxStepsIntoTeleporter) || ghost.Teleported)
                            {
                                // If the ghost has teleported, it is now exiting the teleporter
                                // If the ghost is out of the teleporter, it's state should switch back to standard
                                if (ghost.Teleported && ghost.BlocksIntoTeleporter == 0)
                                {
                                    ghost.SetState(EntityState.Standard);
                                    ghost.Teleported = false;

                                    // Update the ghost's standard positions manually when exiting teleporting state
                                    // so all collision logic works as intended again
                                    ghost.UpdateStandardPositions();
                                }

                                // If the ghost hasn't teleported yet, but is in the teleporting state, teleport it
                                if (!ghost.Teleported && ghost.CurrentState.Equals(EntityState.Teleporting))
                                {
                                    ghost.box.Left = -boxSize * 2;
                                    ghost.Teleported = true;
                                }
                            }
                        }
                        break;
                }

                // All entities pass over (in front of) the fruit box
                if (ghost.box.Bounds.IntersectsWith(fruitBox.Bounds))
                {
                    ghost.box.BringToFront();
                }
            }

            // After looping through each ghost, update the ghost lists so
            // each ghost moves according to the appropriate timer
            UpdateGhostLists();
        }

        private void ghostFrightenedTickTimer_Tick(object sender, EventArgs e)
        {
            // Before looping through all frightened ghosts, set all ghosts who 
            // are going to teleport into the teleporting state
            foreach (Ghost ghost in ghostsFrightened)
            {
                if (CheckForTeleporter(ghost))
                {
                    ghost.SetState(EntityState.Teleporting);
                }
            }

            foreach (Ghost ghost in ghostsFrightened)
            {
                switch (ghost.CurrentState)
                {
                    case EntityState.Standard:
                        // Get all available directions the ghost can move in from its current position
                        var directions = CheckForDirections(ghost);
                        List<Direction> possibleDirections = new List<Direction>();
                        foreach (var direction in directions)
                        {
                            // If a direction is available, add it to the possibleDirections list
                            if (direction.Value == true)
                            {
                                possibleDirections.Add(direction.Key);
                            }
                        }

                        // Only choose a new direction if there are multiple available directions
                        if (possibleDirections.Count > 1)
                        {
                            // Randomly choose one of the available directions to move in
                            int random = new Random().Next(0, possibleDirections.Count);
                            switch (random)
                            {
                                case 0:
                                    ghost.SetDirection(possibleDirections[0]);
                                    break;
                                case 1:
                                    ghost.SetDirection(possibleDirections[1]);
                                    break;
                                case 2:
                                    ghost.SetDirection(possibleDirections[2]);
                                    break;
                            }

                            switch (ghost.CurrentDirection)
                            {
                                case Direction.Left:
                                    ghost.box.Left -= step;
                                    break;
                                case Direction.Right:
                                    ghost.box.Left += step;
                                    break;
                                case Direction.Up:
                                    ghost.box.Top -= step;
                                    break;
                                case Direction.Down:
                                    ghost.box.Top += step;
                                    break;
                            }
                        }
                        else if (possibleDirections.Count == 1)
                        {
                            // If the ghost isn't already moving in the only possible direction, change it
                            if (!ghost.CurrentDirection.Equals(possibleDirections[0]))
                            {
                                ghost.SetDirection(possibleDirections[0]);
                            }

                            switch (ghost.CurrentDirection)
                            {
                                case Direction.Left:
                                    ghost.box.Left -= step;
                                    break;
                                case Direction.Right:
                                    ghost.box.Left += step;
                                    break;
                                case Direction.Up:
                                    ghost.box.Top -= step;
                                    break;
                                case Direction.Down:
                                    ghost.box.Top += step;
                                    break;
                            }
                        }
                        break;
                    case EntityState.Teleporting:
                        if (ghost.CurrentDirection.Equals(Direction.Left))
                        {
                            ghost.box.Left -= step;

                            if (!ghost.Teleported)
                            {
                                // If the ghost has not teleported yet, it is entering the teleporter
                                ghost.BlocksIntoTeleporter++;
                            }
                            else
                            {
                                // Otherwise, the ghost has teleported and is exiting the teleporter instead
                                ghost.BlocksIntoTeleporter--;
                            }

                            // If the ghost is fully inside the teleporter box (going to teleport) or if it has 
                            // teleported (going out of teleporter) either teleport or check if the ghost is out of the teleporter
                            if (ghost.BlocksIntoTeleporter.Equals(GameConstants.MaxStepsIntoTeleporter) || ghost.Teleported)
                            {
                                // If the ghost has teleported, it is now exiting the teleporter
                                // If the ghost is out of the teleporter, it's state should switch back to standard
                                if (ghost.Teleported && ghost.BlocksIntoTeleporter == 0)
                                {
                                    ghost.SetState(EntityState.Standard);
                                    ghost.Teleported = false;

                                    // Update the ghost's standard positions manually when exiting teleporting state
                                    // so all collision logic works as intended again
                                    ghost.UpdateStandardPositions();
                                }

                                // If ghost hasn't teleported yet, but is in the teleporting state, teleport it
                                if (!ghost.Teleported && ghost.CurrentState.Equals(EntityState.Teleporting))
                                {
                                    ghost.box.Left = boxes_Horizontally * boxSize;
                                    ghost.Teleported = true;
                                }
                            }
                        }
                        else if (ghost.CurrentDirection.Equals(Direction.Right))
                        {
                            ghost.box.Left += step;

                            if (!ghost.Teleported)
                            {
                                // If the ghost has not teleported yet, it is entering the teleporter
                                ghost.BlocksIntoTeleporter++;
                            }
                            else
                            {
                                // Otherwise, the ghost has teleported and is exiting the teleporter instead
                                ghost.BlocksIntoTeleporter--;
                            }

                            // If the ghost is fully inside the teleporter box (going to teleport) or if it has 
                            // teleported (going out of teleporter) either teleport or check if the ghost is out of the teleporter
                            if (ghost.BlocksIntoTeleporter.Equals(GameConstants.MaxStepsIntoTeleporter) || ghost.Teleported)
                            {
                                // If the ghost has teleported, it is now exiting the teleporter
                                // If the ghost is out of the teleporter, it's state should switch back to standard
                                if (ghost.Teleported && ghost.BlocksIntoTeleporter == 0)
                                {
                                    ghost.SetState(EntityState.Standard);
                                    ghost.Teleported = false;

                                    // Update the ghost's standard positions manually when exiting teleporting state
                                    // so all collision logic works as intended again
                                    ghost.UpdateStandardPositions();
                                }

                                // If the ghost hasn't teleported yet, but is in the teleporting state, teleport it
                                if (!ghost.Teleported && ghost.CurrentState.Equals(EntityState.Teleporting))
                                {
                                    ghost.box.Left = -boxSize * 2;
                                    ghost.Teleported = true;
                                }
                            }
                        }
                        break;
                }

                // All entities pass ove (in front of) the fruit box
                if (ghost.box.Bounds.IntersectsWith(fruitBox.Bounds))
                {
                    ghost.box.BringToFront();
                }
            }

            if (currentEatGhostDuration > 0)
            {
                currentEatGhostDuration -= GhostConstants.BlinkDuration;

                // If all frightened ghosts have been eaten (a ghost can only be eaten once per frightened mode), exit frightened mode
                if (currentEatGhostDuration <= 0 || ghostsFrightened.Count == 0)
                {
                    currentEatGhostDuration = 0;

                    // Update the highestGhostCombo if a new max has been reached
                    if (ghostsEatenDuringPeriod > globalVariables.HighestGhostCombo)
                    {
                        globalVariables.HighestGhostCombo = ghostsEatenDuringPeriod;
                    }
                    ghostsEatenDuringPeriod = 0;

                    if (mostRecentGlobalBehaviour.Equals(GhostBehaviour.Scatter))
                    {
                        // Override all standard behaviours to scatter after exiting frightened mode
                        SetGhosts_Scatter(true);
                    }
                    else if (mostRecentGlobalBehaviour.Equals(GhostBehaviour.Chase))
                    {
                        // Override all standard behaviours to chase after exiting frightened mode
                        SetGhosts_Chase(true);
                    }

                    foreach (Ghost ghost in ghosts)
                    {
                        ghost.White = false;
                        ghost.HasBeenEaten = false;
                    }
                    ghostsToBlink = false;

                    ghostBehaviourTimeTimer.Start();
                }
            }

            if (currentGlobalBehaviour.Equals(GhostBehaviour.Frightened))
            {
                if (currentEatGhostDuration <= (GhostConstants.BlinkDuration * GhostConstants.TimesToBlink) || ghostsToBlink)
                {
                    ghostsToBlink = true;
                    foreach (Ghost ghost in ghostsFrightened)
                    {
                        if (currentEatGhostDuration / GhostConstants.BlinkDuration % 2 == 0)
                        {
                            ghost.White = true;
                        }
                        else
                        {
                            ghost.White = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the distance between two tiles.
        /// </summary
        double CalculateDistanceToTarget(int[] position, int[] targetPosition)
        {
            return Math.Sqrt((position[0] - targetPosition[0]) * (position[0]- targetPosition[0]) + (position[1] - targetPosition[1]) * (position[1] - targetPosition[1])); 
        }

        /// <summary>
        /// Returns the distance between two tiles if the entity were to pass through the portal closest to it first.
        /// </summary>
        (double distance, TeleportSide leftOrRightPortal) CalculateDistanceToTargetThroughPortal(int[] position, int[] targetPosition)
        {
            int[] portalPos = new int[2];
            TeleportSide leftOrRightPortal;

            // Determine which portal is closer
            double distanceToLeftPortal = Math.Sqrt((position[0] - GameConstants.TeleporterLeftIndex[0]) * (position[0] - GameConstants.TeleporterLeftIndex[0]) + (position[1] - GameConstants.TeleporterLeftIndex[1]) * (position[1] - GameConstants.TeleporterLeftIndex[1]));
            double distanceToRightPortal = Math.Sqrt((position[0] - GameConstants.TeleporterRightIndex[0]) * (position[0] - GameConstants.TeleporterRightIndex[0]) + (position[1] - GameConstants.TeleporterRightIndex[1]) * (position[1] - GameConstants.TeleporterRightIndex[1]));

            if (distanceToLeftPortal < distanceToRightPortal)
            {
                leftOrRightPortal = TeleportSide.Left;
            }
            else 
            {
                leftOrRightPortal = TeleportSide.Right;
            }


            // Get the index of the closest portal
            if (leftOrRightPortal.Equals(TeleportSide.Left))
            {
                portalPos[0] = GameConstants.TeleporterLeftIndex[0];
                portalPos[1] = GameConstants.TeleporterLeftIndex[1];
            }
            else
            {
                portalPos[0] = GameConstants.TeleporterRightIndex[0];
                portalPos[1] = GameConstants.TeleporterRightIndex[0];
            }

            // Calculate the distance to the portal
            double distanceToPortal = Math.Sqrt((position[0] - portalPos[0]) * (position[0] - portalPos[0]) + (position[1] - portalPos[1]) * (position[1] - portalPos[1]));

            // Calculate the distance from the portal to the target tile
            double distanceToTarget = Math.Sqrt((portalPos[0] - targetPosition[0]) * (portalPos[0] - targetPosition[0]) + (portalPos[1] - targetPosition[1]) * (portalPos[1] - targetPosition[1]));

            // Return the total distance along with which portal was used
            return (distanceToPortal + distanceToTarget, leftOrRightPortal);
        }

        /// <summary>
        /// Returns the path (through a portal or not) the entity should follow to reach its target 
        /// tile the fastest, moving to the tile the shortest distance fom its target.
        /// </summary>
        (PossiblePaths pathToTake, TeleportSide portalToTake) GetPathToFollow(Direction currentDirection, Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>> distancesToTarget)
        {
            // Hierarchy: Up > UpThroughPortal > Down > DownThroughPortal > Left > LeftThroughPortal > Right > RightThroughPortal

            if (distancesToTarget.Count() == 1)
            {
                return (distancesToTarget.First().Key, distancesToTarget.First().Value.Key);
            }
            else
            {
                // Variable to hold the path with the currently shortest distance
                KeyValuePair<PossiblePaths, KeyValuePair<TeleportSide, double>> shortestDistancePath = new KeyValuePair<PossiblePaths, KeyValuePair<TeleportSide, double>>();

                // Create a list for holding the paths with same distances to the target to be chosen based according to the hierarchy
                List<KeyValuePair<PossiblePaths, KeyValuePair<TeleportSide, double>>> sameDistancePaths = new List<KeyValuePair<PossiblePaths, KeyValuePair<TeleportSide, double>>>();

                // Update the shortestDistancePath each time a path with a shorter distance to the target tile is found
                foreach (var path in distancesToTarget)
                {
                    if (path.Value.Value < shortestDistancePath.Value.Value || shortestDistancePath.Value.Value == 0)
                    {
                        shortestDistancePath = new KeyValuePair<PossiblePaths, KeyValuePair<TeleportSide, double>>(path.Key, path.Value);
                    }
                    else if (path.Value.Value == shortestDistancePath.Value.Value)
                    {
                        // Add the element with the same distance to the list
                        sameDistancePaths.Add(path);
                    }
                }

                // If the shortestDistancePath's distance is shorter than any previously 
                // same distance in the sameDistancePaths list, remove them as no longer are the same
                // The only elements left in sameDistancePaths is thus paths with the shortest, and same, distance

                // Identify all paths to be removed
                List<int> sameDistanceIndexexToRemove = new List<int>();
                foreach (var path in sameDistancePaths)
                {
                    sameDistanceIndexexToRemove.Add(sameDistancePaths.IndexOf(path));
                }
                // Remove all identified indexes from sameDistancePaths
                foreach (int index in  sameDistanceIndexexToRemove)
                {
                    try
                    {
                        var temp = sameDistancePaths[index];
                        sameDistancePaths.RemoveAt(index);
                    }
                    catch { }
                }

                if (sameDistancePaths.Count == 0)
                {
                    // If there is no other path left with the same distance, return the shortest one
                    return (shortestDistancePath.Key, shortestDistancePath.Value.Key);
                }
                else // There can only be two paths with the same distance at a time
                { 
                    var pathA = shortestDistancePath.Key;
                    var pathAIndex = GetHierarchyIndexOfPath(pathA);

                    var pathB = sameDistancePaths[0].Key;
                    var pathBIndex = GetHierarchyIndexOfPath(pathB);    

                    // The distances for both paths should be the same
                    var pathADistance = shortestDistancePath.Value.Value;
                    var pathBDistance = sameDistancePaths[0].Value.Value;

                    var pathATeleportSide = shortestDistancePath.Value.Key;
                    var pathBTeleportSide = shortestDistancePath.Value.Key;


                    // The smallest pathIndex has the highest priority when both path-distances are the same
                    if (pathAIndex < pathBIndex)
                    {
                        return (pathA, pathATeleportSide);
                    }
                    else
                    {
                        return (pathB, pathBTeleportSide);
                    }
                }
            }
        }


        /// <summary>
        /// Returns an integer indicating where in the hierarchy a path has it's place. The lower the integer, the higher the priority. 
        /// </summary>
        private int GetHierarchyIndexOfPath(PossiblePaths path)
        {
            switch (path)
            {
                case PossiblePaths.Up:
                    return 0;
                case PossiblePaths.UpThroughPortal:
                    return 1;

                case PossiblePaths.Down:
                    return 2;
                case PossiblePaths.DownThroughPortal:
                    return 3;

                case PossiblePaths.Left:
                    return 4;
                case PossiblePaths.LeftThroughPortal:
                    return 5;

                case PossiblePaths.Right:
                    return 6;
                case PossiblePaths.RightThroughPortal:
                    return 7;
                default:
                    return int.MaxValue;
            }
        }

        private void ghostReturnTickTimer_Tick(object sender, EventArgs e)
        {
            foreach (Ghost ghost in ghosts)
            {
                if (ghost.CurrentState.Equals(EntityState.Eaten))
                { 
                    if (ghost.CurrentDirection.Equals(Direction.Left))
                    {
                        // Don't check for gate if ghost is eaten
                        if (!CheckForWall(ghost) && CheckForEntry(ghost, ghost.CurrentDirection))
                        {
                            ghost.box.Left -= step;
                        }

                        // If an eaten ghost's is at the OutOfHouseIndex, it is entering the house and only needs to go straight down to reach the returnIndex
                        if (ghost.CurrentPosX == GhostConstants.OutOfHouseIndex[0] && ghost.CurrentPosY == GhostConstants.OutOfHouseIndex[1])
                        {
                            ghost.EnteringGhostHouse = true;
                        }
                        else if (!ghost.EnteringGhostHouse)
                        {
                            // Get all available directions the ghost can move in from its current position
                            var directions = CheckForDirections(ghost);

                            // Create a dictionary to hold the distances for each possible path
                            Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>> distancesToTarget = new Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>>();

                            // Use a "testGhost" to measure distances to the ghost's target tile in each direction the ghost can move in
                            Ghost testGhost = new Ghost(GhostTemplate.TestGhost);
                            testGhost.box.Size = ghost.box.Size;
                            testGhost.box.Location = ghost.box.Location;
                            Controls.Add(testGhost.box);

                            testGhost.navBox.Size = ghost.navBox.Size;
                            Controls.Add(testGhost.navBox);

                            if (directions[Direction.Up])
                            {
                                testGhost.box.Top -= step;
                                testGhost.UpdateLocation();

                                distancesToTarget[PossiblePaths.Up] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                testGhost.box.Top += step;
                                testGhost.UpdateLocation();
                            }
                            if (directions[Direction.Down])
                            {
                                testGhost.box.Top += step;
                                testGhost.UpdateLocation();

                                distancesToTarget[PossiblePaths.Down] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                testGhost.UpdateLocation();
                                testGhost.box.Top -= step;
                            }
                            if (directions[Direction.Left])
                            {
                                testGhost.box.Left -= step;
                                testGhost.UpdateLocation();

                                distancesToTarget[PossiblePaths.Left] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));
                            }

                            testGhost.box.Dispose();
                            Controls.Remove(testGhost.box);

                            var path = GetPathToFollow(ghost.CurrentDirection, distancesToTarget);
                            ghost.SetPath(path.pathToTake);
                        }
                        
                        if (ghost.EnteringGhostHouse)
                        {
                            ghost.SetDirection(Direction.Down);
                        }
                    }
                    else if (ghost.CurrentDirection.Equals(Direction.Right))
                    {
                        // Don't check for gate if ghost is eaten
                        if (!CheckForWall(ghost) && CheckForEntry(ghost, ghost.CurrentDirection))
                        {
                            ghost.box.Left += step;
                        }

                        // If an eaten ghost's is at the OutOfHouseIndex, it is entering the house and only needs to go straight down to reach the returnIndex
                        if (ghost.CurrentPosX == GhostConstants.OutOfHouseIndex[0] && ghost.CurrentPosY == GhostConstants.OutOfHouseIndex[1])
                        {
                            ghost.EnteringGhostHouse = true;
                        }
                        else if (!ghost.EnteringGhostHouse)
                        {
                            // Get all available directions the ghost can move in from its current position
                            var directions = CheckForDirections(ghost);

                            // Create a dictionary to hold the distances for each possible path
                            Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>> distancesToTarget = new Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>>();

                            // Use a "testGhost" to measure distances to the ghost's target tile in each direction the ghost can move in
                            Ghost testGhost = new Ghost(GhostTemplate.TestGhost);
                            testGhost.box.Size = ghost.box.Size;
                            testGhost.box.Location = ghost.box.Location;
                            testGhost.UpdateLocation();
                            Controls.Add(testGhost.box);

                            testGhost.navBox.Size = ghost.navBox.Size;
                            Controls.Add(testGhost.navBox);

                            if (directions[Direction.Up])
                            {
                                testGhost.box.Top -= step;
                                testGhost.UpdateLocation();

                                distancesToTarget[PossiblePaths.Up] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                testGhost.box.Top += step;
                                testGhost.UpdateLocation();
                            }
                            if (directions[Direction.Down])
                            {
                                testGhost.box.Top += step;
                                testGhost.UpdateLocation();

                                distancesToTarget[PossiblePaths.Down] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                testGhost.box.Top -= step;
                                testGhost.UpdateLocation();
                            }
                            if (directions[Direction.Right])
                            {
                                testGhost.box.Left += step;
                                testGhost.UpdateLocation();

                                distancesToTarget[PossiblePaths.Right] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));
                            }

                            testGhost.box.Dispose();
                            Controls.Remove(testGhost.box);

                            var path = GetPathToFollow(ghost.CurrentDirection, distancesToTarget);
                            ghost.SetPath(path.pathToTake);
                        }
                        
                        if (ghost.EnteringGhostHouse)
                        {
                            ghost.SetDirection(Direction.Down);
                        }
                    }
                    else if (ghost.CurrentDirection.Equals(Direction.Up))
                    {
                        // Don't check for gate if ghost is eaten
                        if (!CheckForWall(ghost) && CheckForEntry(ghost, ghost.CurrentDirection))
                        {
                            ghost.box.Top -= step;
                        }

                        // If an eaten ghost's is at the OutOfHouseIndex, it is entering the house and only needs to go straight down to reach the returnIndex
                        if (ghost.CurrentPosX == GhostConstants.OutOfHouseIndex[0] && ghost.CurrentPosY == GhostConstants.OutOfHouseIndex[1])
                        {
                            ghost.EnteringGhostHouse = true;
                        }
                        else if (!ghost.EnteringGhostHouse)
                        {
                            // Get all available directions the ghost can move in from its current position
                            var directions = CheckForDirections(ghost);

                            // Create a dictionary to hold the distances for each possible path
                            Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>> distancesToTarget = new Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>>();

                            // Use a "testGhost" to measure distances to the ghost's target tile in each direction the ghost can move in
                            Ghost testGhost = new Ghost(GhostTemplate.TestGhost);
                            testGhost.box.Size = ghost.box.Size;
                            testGhost.box.Location = ghost.box.Location;
                            testGhost.UpdateLocation();
                            Controls.Add(testGhost.box);

                            testGhost.navBox.Size = ghost.navBox.Size;
                            Controls.Add(testGhost.navBox);

                            if (directions[Direction.Up])
                            {
                                testGhost.box.Top -= step;
                                testGhost.UpdateLocation();

                                distancesToTarget[PossiblePaths.Up] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                testGhost.box.Top += step;
                                testGhost.UpdateLocation();
                            }
                            if (directions[Direction.Right])
                            {
                                testGhost.box.Left += step;
                                testGhost.UpdateLocation();

                                distancesToTarget[PossiblePaths.Right] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                testGhost.box.Left -= step;
                                testGhost.UpdateLocation();
                            }
                            if (directions[Direction.Left])
                            {
                                testGhost.box.Left -= step;
                                testGhost.UpdateLocation();

                                distancesToTarget[PossiblePaths.Left] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));
                            }

                            testGhost.box.Dispose();
                            Controls.Remove(testGhost.box);

                            var path = GetPathToFollow(ghost.CurrentDirection, distancesToTarget);
                            ghost.SetPath(path.pathToTake);
                        }
                        
                        if (ghost.EnteringGhostHouse)
                        {
                            ghost.SetDirection(Direction.Down);
                        }
                    }
                    else if (ghost.CurrentDirection.Equals(Direction.Down))
                    {
                        // Don't check for gate if ghost is eaten
                        if (!CheckForWall(ghost) && CheckForEntry(ghost, ghost.CurrentDirection))
                        {
                            ghost.box.Top += step;
                        }

                        // If an eaten ghost's is at the OutOfHouseIndex, it is entering the house and only needs to go straight down to reach the returnIndex
                        if (ghost.CurrentPosX == GhostConstants.OutOfHouseIndex[0] && ghost.CurrentPosY == GhostConstants.OutOfHouseIndex[1])
                        {
                            ghost.EnteringGhostHouse = true;
                        }
                        else if (!ghost.EnteringGhostHouse)
                        {
                            // Get all available directions the ghost can move in from its current position
                            var directions = CheckForDirections(ghost);

                            // Create a dictionary to hold the distances for each possible path
                            Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>> distancesToTarget = new Dictionary<PossiblePaths, KeyValuePair<TeleportSide, double>>();

                            // Use a "testGhost" to measure distances to the ghost's target tile in each direction the ghost can move in
                            Ghost testGhost = new Ghost(GhostTemplate.TestGhost);
                            testGhost.box.Size = ghost.box.Size;
                            testGhost.box.Location = ghost.box.Location;
                            testGhost.UpdateLocation();
                            Controls.Add(testGhost.box);

                            testGhost.navBox.Size = ghost.navBox.Size;
                            Controls.Add(testGhost.navBox);

                            if (directions[Direction.Down])
                            {
                                testGhost.box.Top += step;
                                testGhost.UpdateLocation();

                                distancesToTarget[PossiblePaths.Down] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                testGhost.box.Top -= step;
                                testGhost.UpdateLocation();
                            }
                            if (directions[Direction.Left])
                            {
                                testGhost.box.Left -= step;
                                testGhost.UpdateLocation();

                                distancesToTarget[PossiblePaths.Left] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));

                                testGhost.box.Left += step;
                                testGhost.UpdateLocation();
                            }
                            if (directions[Direction.Right])
                            {
                                testGhost.box.Left += step;
                                testGhost.UpdateLocation();

                                distancesToTarget[PossiblePaths.Right] = new KeyValuePair<TeleportSide, double>(TeleportSide.None, CalculateDistanceToTarget(testGhost.CurrentPos, ghost.TargetPos));
                            }

                            testGhost.box.Dispose();
                            Controls.Remove(testGhost.box);

                            var path = GetPathToFollow(ghost.CurrentDirection, distancesToTarget);
                            ghost.SetPath(path.pathToTake);
                        }
                        
                        if (ghost.EnteringGhostHouse)
                        {
                            ghost.SetDirection(Direction.Down);
                        }
                    }

                    // If the ghost has reached the ghost house (the return index), change its state back to standard
                    if (ghost.CurrentPosX == GhostConstants.ReturnIndex[0] && ghost.CurrentPosY == GhostConstants.ReturnIndex[1])
                    {
                        ghost.SetState(EntityState.Standard);
                        ghost.SetBehaviour(GhostBehaviour.ExitingHouse);

                        // Make a list containing all currently eaten ghosts
                        List<Ghost> eatenGhosts = ghosts.Where(g => g.CurrentState.Equals(EntityState.Eaten)).ToList();

                        // If there are no more ghosts in the eaten state after this ghost has returned, stop the sound
                        if (eatenGhosts.Count == 0)
                        {
                            soundManager.StopSound(Sounds.ghost_return);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a dictionary indicating which direction can be moved in.
        /// Usually, a ghost can never turn 180 degrees, never going back the way it came.
        /// </summary>
        private Dictionary<Direction, bool> CheckForDirections(Ghost ghost)
        {
            Dictionary<Direction, bool> availableDirections = new Dictionary<Direction, bool>();

            switch (ghost.CurrentDirection)
            {
                case Direction.Left:
                    // Left
                    if ((!CheckForWall(ghost, Direction.Left) && CheckForEntry(ghost, Direction.Left)) || CheckForTeleporter(ghost))
                    {
                        availableDirections.Add(Direction.Left, true);
                    }
                    else
                    {
                        availableDirections.Add(Direction.Left, false);
                    }

                    // Ghost can not turn around (right)
                    availableDirections.Add(Direction.Right, false);

                    // Up
                    if (!CheckForWall(ghost, Direction.Up) && CheckForEntry(ghost, Direction.Up))
                    {
                        availableDirections.Add(Direction.Up, true);
                    }
                    else
                    {
                        availableDirections.Add(Direction.Up, false);
                    }

                    // Down
                    if (!ghost.CurrentState.Equals(EntityState.Eaten))
                    {
                        if (!CheckForWall(ghost, Direction.Down) && !CheckForGate(ghost, Direction.Down) && CheckForEntry(ghost, Direction.Down))
                        {
                            availableDirections.Add(Direction.Down, true);
                        }
                        else
                        {
                            availableDirections.Add(Direction.Down, false);
                        }
                    }
                    else
                    {
                        // Entities (ghosts) that are eaten can pass through gates to return to the ghost house
                        if (!CheckForWall(ghost, Direction.Down) && CheckForEntry(ghost, Direction.Down))
                        {
                            availableDirections.Add(Direction.Down, true);
                        }
                        else
                        {
                            availableDirections.Add(Direction.Down, false);
                        }
                    }
                    return availableDirections;
                case Direction.Right:

                    // Ghost can not turn around (left)
                    availableDirections.Add(Direction.Left, false);

                    // Right
                    if ((!CheckForWall(ghost, Direction.Right) && CheckForEntry(ghost, Direction.Right)) || CheckForTeleporter(ghost))
                    {
                        availableDirections.Add(Direction.Right, true);
                    }
                    else
                    {
                        availableDirections.Add(Direction.Right, false);
                    }

                    // Up
                    if (!CheckForWall(ghost, Direction.Up) && CheckForEntry(ghost, Direction.Up))
                    {
                        availableDirections.Add(Direction.Up, true);
                    }
                    else
                    {
                        availableDirections.Add(Direction.Up, false);
                    }

                    // Down
                    if (!ghost.CurrentState.Equals(EntityState.Eaten))
                    {
                        if (!CheckForWall(ghost, Direction.Down) && !CheckForGate(ghost, Direction.Down) && CheckForEntry(ghost, Direction.Down))
                        {
                            availableDirections.Add(Direction.Down, true);
                        }
                        else
                        {
                            availableDirections.Add(Direction.Down, false);
                        }
                    }
                    else
                    {
                        // Entities (ghosts) that are eaten can pass through gates to return to the ghost house
                        if (!CheckForWall(ghost, Direction.Down) && CheckForEntry(ghost, Direction.Down))
                        {
                            availableDirections.Add(Direction.Down, true);
                        }
                        else
                        {
                            availableDirections.Add(Direction.Down, false);
                        }
                    }
                    return availableDirections;
                case Direction.Up:
                    // Left
                    if ((!CheckForWall(ghost, Direction.Left) && CheckForEntry(ghost, Direction.Left)) || CheckForTeleporter(ghost))
                    {
                        availableDirections.Add(Direction.Left, true);
                    }
                    else
                    {
                        availableDirections.Add(Direction.Left, false);
                    }

                    // Right
                    if ((!CheckForWall(ghost, Direction.Right) && CheckForEntry(ghost, Direction.Right)) || CheckForTeleporter(ghost))
                    {
                        availableDirections.Add(Direction.Right, true);
                    }
                    else
                    {
                        availableDirections.Add(Direction.Right, false);
                    }

                    // Up
                    if (!CheckForWall(ghost, Direction.Up) && CheckForEntry(ghost, Direction.Up))
                    {
                        availableDirections.Add(Direction.Up, true);
                    }
                    else
                    {
                        availableDirections.Add(Direction.Up, false);
                    }

                    // Ghost can not turn around (down)
                    availableDirections.Add(Direction.Down, false);

                    return availableDirections;
                case Direction.Down:
                    // Left
                    if ((!CheckForWall(ghost, Direction.Left) && CheckForEntry(ghost, Direction.Left)) || CheckForTeleporter(ghost))
                    {
                        availableDirections.Add(Direction.Left, true);
                    }
                    else
                    {
                        availableDirections.Add(Direction.Left, false);
                    }

                    // Right
                    if ((!CheckForWall(ghost, Direction.Right) && CheckForEntry(ghost, Direction.Right)) || CheckForTeleporter(ghost))
                    {
                        availableDirections.Add(Direction.Right, true);
                    }
                    else
                    {
                        availableDirections.Add(Direction.Right, false);
                    }

                    // Ghost can not turn around (up)
                    availableDirections.Add(Direction.Up, false);

                    // Down
                    if (!ghost.CurrentState.Equals(EntityState.Eaten))
                    {
                        if ((!CheckForWall(ghost, Direction.Down) && !CheckForGate(ghost, Direction.Down) && CheckForEntry(ghost, Direction.Down)) || CheckForTeleporter(ghost))
                        {
                            availableDirections.Add(Direction.Down, true);
                        }
                        else
                        {
                            availableDirections.Add(Direction.Down, false);
                        }
                    }
                    else
                    {
                        // Entities (ghosts) that are eaten can pass through gates to return to the ghost house
                        if ((!CheckForWall(ghost, Direction.Down) && CheckForEntry(ghost, Direction.Down)) || CheckForTeleporter(ghost))
                        {
                            availableDirections.Add(Direction.Down, true);
                        }
                        else
                        {
                            availableDirections.Add(Direction.Down, false);
                        }
                    }
                    return availableDirections;
                default:
                    availableDirections.Add(Direction.Left, false);
                    availableDirections.Add(Direction.Right, false);
                    availableDirections.Add(Direction.Up, false);
                    availableDirections.Add(Direction.Down, false);

                    return availableDirections;
            }
        }

        private void SetGhostBhaviourToGobalBeaviour(Ghost ghost)
        {
            switch (currentGlobalBehaviour)
            {
                case GhostBehaviour.Chase:
                    ghost.SetChase();
                    break;
                case GhostBehaviour.Scatter:
                    ghost.SetScatter();
                    break;
                case GhostBehaviour.Frightened: 
                    ghost.SetFrightened();
                    break;
            }
        }

        private void UpdateGhostLists()
        {
            // Add all ghosts who are no longer frightened in the frightened list to the ordinary list
            for (int index = ghostsFrightened.Count - 1; index >= 0; index--)
            {
                if (!ghostsFrightened[index].CurrentBehaviour.Equals(GhostBehaviour.Frightened))
                {
                    ghosts.Add(ghostsFrightened[index]);
                }
            }

            // Add all ghosts who now are frightened in the ordinary list to the frightened list
            for (int index = ghosts.Count - 1; index >= 0; index--)
            {
                if (ghosts[index].CurrentBehaviour.Equals(GhostBehaviour.Frightened))
                {
                    ghostsFrightened.Add(ghosts[index]);
                }
            }


            // Remove all non-frightened ghosts from the frightened ghost list
            for (int index = ghostsFrightened.Count - 1; index >= 0; index--)
            {
                if (!ghostsFrightened[index].CurrentBehaviour.Equals(GhostBehaviour.Frightened))
                {
                    ghostsFrightened.Remove(ghostsFrightened[index]);
                }
            }

            // Remove all ghosts who are frightened from the ordinary list
            for (int index = ghosts.Count - 1; index >= 0; index--)
            {
                if (ghosts[index].CurrentBehaviour.Equals(GhostBehaviour.Frightened))
                {
                    ghosts.Remove(ghosts[index]);
                }
            }
        }

        private void SetGhosts_Frightened()
        {
            // Switch current behaviour to most recent behaviour before it's updated
            mostRecentGlobalBehaviour = currentGlobalBehaviour;
            currentGlobalBehaviour = GhostBehaviour.Frightened;

            SetSound_Scared();

            foreach (Ghost ghost in ghosts)
            {
                if (ghost.BehaviourCanBeChangedTo(GhostBehaviour.Frightened))
                {
                    ghost.SetFrightened();

                    // Ghosts turn around when frightened
                    switch (ghost.CurrentDirection)
                    {
                        case Direction.Left:
                            ghost.SetDirection(Direction.Right);
                            break;
                        case Direction.Right:
                            ghost.SetDirection(Direction.Left);
                            break;
                        case Direction.Up:
                            ghost.SetDirection(Direction.Down);
                            break;
                        case Direction.Down:
                            ghost.SetDirection(Direction.Up);
                            break;
                    }
                }
            }

            UpdateGhostLists();
            UpdateTimerIntervals();
        }

        private void SetGhosts_Scatter(bool overrideAllStandardBehaviours)
        {
            mostRecentGlobalBehaviour = currentGlobalBehaviour;
            currentGlobalBehaviour = GhostBehaviour.Scatter;

            SetSound_Scatter();

            // If the preivous global behaviour had a higher place in the default behaviour hierarchy, set overrideAllStandardBehavoiurs to true so the ghost's change behaviour
            if (GhostConstants.DefaultBehaviourHierarchy.IndexOf(mostRecentGlobalBehaviour) < GhostConstants.DefaultBehaviourHierarchy.IndexOf(currentGlobalBehaviour))
            {
                overrideAllStandardBehaviours = true;
            }

            foreach (Ghost ghost in ghosts)
            {
                if (ghost.BehaviourCanBeChangedTo(GhostBehaviour.Scatter) || overrideAllStandardBehaviours)
                {
                    // Only ghosts who aren't eaten or are exiting the ghost house can have their behaviour overridden
                    if (!ghost.CurrentState.Equals(EntityState.Eaten) && !ghost.CurrentBehaviour.Equals(GhostBehaviour.ExitingHouse))
                    {
                        ghost.SetScatter();
                    }
                }
            }
           
            foreach (Ghost ghost in ghostsFrightened)
            {
                if (ghost.BehaviourCanBeChangedTo(GhostBehaviour.Scatter) || overrideAllStandardBehaviours)
                {
                    // Only ghosts who aren't eaten or are exiting the ghost house can have their behaviour overridden
                    if (!ghost.CurrentState.Equals(EntityState.Eaten) && !ghost.CurrentBehaviour.Equals(GhostBehaviour.ExitingHouse))
                    {
                        ghost.SetScatter();
                    }
                }
            }

            UpdateGhostLists();
            UpdateTimerIntervals();
        }

        private void SetGhosts_Chase(bool overrideAllStandardBehaviours)
        {
            mostRecentGlobalBehaviour = currentGlobalBehaviour;
            currentGlobalBehaviour = GhostBehaviour.Chase;
            
            SetSound_Chase();

            // If the preivous global behaviour had a higher place in the default behaviour hierarchy, set overrideAllStandardBehavoiurs to true so the ghost's change behaviour
            if (GhostConstants.DefaultBehaviourHierarchy.IndexOf(mostRecentGlobalBehaviour) < GhostConstants.DefaultBehaviourHierarchy.IndexOf(currentGlobalBehaviour))
            {
                overrideAllStandardBehaviours = true;
            }

            foreach (Ghost ghost in ghosts)
            {
                if (ghost.BehaviourCanBeChangedTo(GhostBehaviour.Chase) || overrideAllStandardBehaviours)
                {
                    // Only ghosts who aren't eaten or are exiting the ghost house can have their behaviour overridden
                    if (!ghost.CurrentState.Equals(EntityState.Eaten) && !ghost.CurrentBehaviour.Equals(GhostBehaviour.ExitingHouse))
                    {
                        ghost.SetChase();

                        // Ghosts turn around when chasing
                        switch (ghost.CurrentDirection)
                        {
                            case Direction.Left:
                                ghost.SetDirection(Direction.Right);
                                break;
                            case Direction.Right:
                                ghost.SetDirection(Direction.Left);
                                break;
                            case Direction.Up:
                                ghost.SetDirection(Direction.Down);
                                break;
                            case Direction.Down:
                                ghost.SetDirection(Direction.Up);
                                break;
                        }
                    }
                }
            }

            foreach (Ghost ghost in ghostsFrightened)
            {
                if (ghost.BehaviourCanBeChangedTo(GhostBehaviour.Chase) || overrideAllStandardBehaviours)
                {
                    // Only ghosts who aren't eaten or are exiting the ghost house can have their behaviour overridden
                    if (!ghost.CurrentState.Equals(EntityState.Eaten) && !ghost.CurrentBehaviour.Equals(GhostBehaviour.ExitingHouse))
                    {
                        ghost.SetChase();

                        // Ghosts turn around when chasing
                        switch (ghost.CurrentDirection)
                        {
                            case Direction.Left:
                                ghost.SetDirection(Direction.Right);
                                break;
                            case Direction.Right:
                                ghost.SetDirection(Direction.Left);
                                break;
                            case Direction.Up:
                                ghost.SetDirection(Direction.Down);
                                break;
                            case Direction.Down:
                                ghost.SetDirection(Direction.Up);
                                break;
                        }
                    }
                }
            }

            UpdateGhostLists();
            UpdateGhostTargets();

            UpdateTimerIntervals();
        }

        private bool CheckForPacman(Ghost ghost)
        {
            if (ghost.CurrentState.Equals(EntityState.Standard))
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
                    switch (ghost.CurrentDirection)
                    {
                        case Direction.Left:
                            testGhost.Left -= step;
                            if (testGhost.Bounds.IntersectsWith(pacman.box.Bounds))
                            {
                                testGhost.Dispose();
                                return true;
                            }
                            testGhost.Dispose();
                            return false;
                        case Direction.Right:
                            testGhost.Left += step;
                            if (testGhost.Bounds.IntersectsWith(pacman.box.Bounds))
                            {
                                testGhost.Dispose();
                                return true;
                            }
                            testGhost.Dispose();
                            return false;
                        case Direction.Up:
                            testGhost.Top -= step;
                            if (testGhost.Bounds.IntersectsWith(pacman.box.Bounds))
                            {
                                testGhost.Dispose();
                                return true;
                            }
                            testGhost.Dispose();
                            return false;
                        case Direction.Down:
                            testGhost.Top += step;
                            if (testGhost.Bounds.IntersectsWith(pacman.box.Bounds))
                            {
                                testGhost.Dispose();
                                return true;
                            }
                            testGhost.Dispose();
                            return false;
                    }
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
            return false;
        }

        /// <summary>
        /// Update a specific ghost's target according to the current global behaviour
        /// </summary>
        private void UpdateGhostTarget(Ghost ghost)
        {
            // Ghosts inside the ghost house don't follow the global behaviour until they leave
            if (!ghost.ExitingGhostHouse)
            {
                switch (currentGlobalBehaviour)
                {
                    case GhostBehaviour.Chase:
                        switch (ghost.Template)
                        {
                            case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Blinky):
                                ghost.SetChase();
                                ghost.SetTarget(pacman.CurrentPos);
                                break;
                            case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Pinky):
                                ghost.SetChase();

                                switch (pacman.CurrentDirection)
                                {
                                    case Direction.Left:
                                        ghost.SetTarget(pacman.CurrentPosX - 4, pacman.CurrentPosY);
                                        break;
                                    case Direction.Right:
                                        ghost.SetTarget(pacman.CurrentPosX + 4, pacman.CurrentPosY);
                                        break;
                                    case Direction.Up:
                                        ghost.SetTarget(pacman.CurrentPosX - 4, pacman.CurrentPosY - 4);
                                        break;
                                    case Direction.Down:
                                        ghost.SetTarget(pacman.CurrentPosX, pacman.CurrentPosY + 4);
                                        break;
                                    case Direction.Stationary:
                                        ghost.SetTarget(pacman.CurrentPos);
                                        break;

                                }
                                break;
                            case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Inky):
                                ghost.SetChase();

                                int targetXpreliminary_Inky = 0;
                                int targetYpreliminary_Inky = 0;
                                switch (pacman.CurrentDirection)
                                {
                                    case Direction.Left:
                                        targetXpreliminary_Inky = pacman.CurrentPosX - 2;
                                        targetYpreliminary_Inky = pacman.CurrentPosY;
                                        break;
                                    case Direction.Right:
                                        targetXpreliminary_Inky = pacman.CurrentPosX + 2;
                                        targetYpreliminary_Inky = pacman.CurrentPosY;
                                        break;
                                    case Direction.Up:
                                        targetXpreliminary_Inky = pacman.CurrentPosX - 2;
                                        targetYpreliminary_Inky = pacman.CurrentPosY - 2;
                                        break;
                                    case Direction.Down:
                                        targetXpreliminary_Inky = pacman.CurrentPosX;
                                        targetYpreliminary_Inky = pacman.CurrentPosY + 2;
                                        break;
                                    case Direction.Stationary:
                                        targetXpreliminary_Inky = pacman.CurrentPosX;
                                        targetYpreliminary_Inky = pacman.CurrentPosY;
                                        break;
                                }

                                // Inky's target is the position mirrored across Blinky's position
                                Ghost blinky = null;
                                foreach (Ghost g in ghosts)
                                {
                                    if (g.Template.Equals(GhostTemplate.Blinky))
                                    {
                                        blinky = g; 
                                    }
                                }

                                int disDiffX_Inky = targetXpreliminary_Inky - blinky.CurrentPosX;
                                int disDiffY_Inky = targetYpreliminary_Inky - blinky.CurrentPosY;

                                if (blinky.CurrentPosX < targetXpreliminary_Inky && blinky.CurrentPosY > targetYpreliminary_Inky)
                                {
                                    ghost.SetTarget(targetXpreliminary_Inky + disDiffX_Inky, targetYpreliminary_Inky - disDiffY_Inky);
                                }
                                else if (blinky.CurrentPosX < targetXpreliminary_Inky && blinky.CurrentPosY < targetYpreliminary_Inky)
                                {
                                    ghost.SetTarget(targetXpreliminary_Inky + disDiffX_Inky, targetYpreliminary_Inky + disDiffY_Inky);
                                }
                                else if (blinky.CurrentPosX > targetXpreliminary_Inky && blinky.CurrentPosY < targetYpreliminary_Inky)
                                {
                                    ghost.SetTarget(targetXpreliminary_Inky - disDiffX_Inky, targetYpreliminary_Inky + disDiffY_Inky);
                                }
                                else if (blinky.CurrentPosX > targetXpreliminary_Inky && blinky.CurrentPosY > targetYpreliminary_Inky)
                                {
                                    ghost.SetTarget(targetXpreliminary_Inky - disDiffX_Inky, targetYpreliminary_Inky - disDiffY_Inky);
                                }
                                break;
                            case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Clyde):
                                // Calculate the distance Clyde is from Pacman
                                int disDiffX_Clyde = ghost.CurrentPosX - pacman.CurrentPosX;
                                int disDiffY_Clyde = ghost.CurrentPosY - pacman.CurrentPosY;

                                // It doesn't matter if diffX or diffY is negative, as squaring them will make them positive
                                // If Clyde is within 8 tiles of Pacman, enter scatter mode
                                if (Math.Sqrt((disDiffX_Clyde * disDiffX_Clyde) + (disDiffY_Clyde * disDiffY_Clyde)) <= GhostConstants.Clyde.BehaviourOverrideDistance)
                                {
                                    if (ghost.BehaviourOverridden)
                                    {
                                        // If Clyde's behaviour is already overridden, reset his duration counter
                                        ghost.BehaviourOverrideDuration = 0;
                                    }
                                    else
                                    {
                                        // Check if Clyde's behaviour can be overridden by Scatter
                                        if (ghost.BehaviourCanBeChangedTo(GhostBehaviour.Scatter))
                                        {
                                            ghost.SetScatter();
                                            ghost.BehaviourOverridden = true;
                                        }
                                    }
                                }

                                if (ghost.BehaviourOverridden)
                                {
                                    if (ghost.BehaviourOverrideDuration >= GhostConstants.Clyde.BevahiourOverrideScatterTime)
                                    {
                                        SetGhostBhaviourToGobalBeaviour(ghost);

                                        ghost.BehaviourOverridden = false;
                                        ghost.BehaviourOverrideDuration = 0;
                                    }

                                    // Increase the behaviourDuration after each tick if overridden
                                    ghost.BehaviourOverrideDuration += ghostTickTimer.Interval;
                                }
                                ghost.SetChase();
                                ghost.SetTarget(pacman.CurrentPos);
                                break;
                            default:
                                // Default to pacman's position
                                ghost.SetTarget(pacman.CurrentPos);
                                break;
                        }
                        break;
                    case GhostBehaviour.Scatter:
                        ghost.SetScatter();
                        break;

                    case GhostBehaviour.Frightened:
                        // A ghost can only be eaten once per frightened mode
                        // When an eaten ghost exits the ghost house after being eaten, they enter chase mode.
                        if (ghost.HasBeenEaten)
                        {
                            ghost.SetChase();
                        }
                        else
                        {
                            ghost.SetFrightened();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Updates all ghost's respective targets according to their current behaviour.
        /// </summary>
        private void UpdateGhostTargets()
        {
            // Blinky: Chase Pacman directly
            // Pinky: Chase Pacman 4 boxes ahead in his direction (up => 4 tiles up and right)
            // Inky: Chase Pacman 4 boxes ahead in his direction + Blinky's position mirrored (180 degrees) 
            // Clyde: Chase Pacman directly, but if within 8 boxes of Pacman, enter scatter mode

            foreach (Ghost ghost in ghosts)
            {
                if (ghost.Template.Equals(GhostTemplate.Clyde))
                {
                    // Calculate the distance Clyde is from Pacman
                    int disDiffX_Clyde = ghost.CurrentPosX - pacman.CurrentPosX;
                    int disDiffY_Clyde = ghost.CurrentPosY - pacman.CurrentPosY;

                    // It doesn't matter if diffX or diffY is negative, as squaring them will make them positive
                    // If Clyde is within 8 tiles of Pacman, enter scatter mode
                    if (Math.Sqrt((disDiffX_Clyde * disDiffX_Clyde) + (disDiffY_Clyde * disDiffY_Clyde)) <= GhostConstants.Clyde.BehaviourOverrideDistance)
                    {
                        if (ghost.BehaviourOverridden)
                        {
                            // If Clyde's behaviour is already overridden, reset his duration counter
                            ghost.BehaviourOverrideDuration = 0;
                        }
                        else
                        {
                            // Override Clyde's behavour so he follows his own behaviour hierarchy
                            ghost.BehaviourOverridden = true;

                            // Check if Clyde's behaviour can be changed to Scatter
                            if (ghost.BehaviourCanBeChangedTo(GhostBehaviour.Scatter))
                            {
                                ghost.SetScatter();
                                ghost.BehaviourOverridden = true;
                            }
                        }
                    }

                    if (ghost.BehaviourOverridden)
                    {
                        if (ghost.BehaviourOverrideDuration >= GhostConstants.Clyde.BevahiourOverrideScatterTime)
                        {
                            SetGhostBhaviourToGobalBeaviour(ghost);

                            ghost.BehaviourOverridden = false;
                            ghost.BehaviourOverrideDuration = 0;
                        }

                        // Increase the behaviourDuration after each tick if overridden
                        ghost.BehaviourOverrideDuration += ghostTickTimer.Interval;
                    }
                }

                // Ghosts inside the ghost house don't follow the global behaviour until they have left the ghost house
                if (!ghost.ExitingGhostHouse)
                {
                    if (currentGlobalBehaviour == GhostBehaviour.Chase)
                    {
                        switch (ghost.Template)
                        {
                            case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Blinky):
                                ghost.SetTarget(pacman.CurrentPos);
                                break;
                            case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Pinky):
                                switch (pacman.CurrentDirection)
                                {
                                    case Direction.Left:
                                        ghost.SetTarget(pacman.CurrentPosX - 4, pacman.CurrentPosY);
                                        break;
                                    case Direction.Right:
                                        ghost.SetTarget(pacman.CurrentPosX + 4, pacman.CurrentPosY);
                                        break;
                                    case Direction.Up:
                                        ghost.SetTarget(pacman.CurrentPosX - 4, pacman.CurrentPosY - 4);
                                        break;
                                    case Direction.Down:
                                        ghost.SetTarget(pacman.CurrentPosX, pacman.CurrentPosY + 4);
                                        break;
                                    case Direction.Stationary:
                                        ghost.SetTarget(pacman.CurrentPos);
                                        break;
                                }
                                break;
                            case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Inky):
                                int targetXpreliminary_Inky = 0;
                                int targetYpreliminary_Inky = 0;
                                switch (pacman.CurrentDirection)
                                {
                                    case Direction.Left:
                                        targetXpreliminary_Inky = pacman.CurrentPosX - 2;
                                        targetYpreliminary_Inky = pacman.CurrentPosY;
                                        break;
                                    case Direction.Right:
                                        targetXpreliminary_Inky = pacman.CurrentPosX + 2;
                                        targetYpreliminary_Inky = pacman.CurrentPosY;
                                        break;
                                    case Direction.Up:
                                        targetXpreliminary_Inky = pacman.CurrentPosX - 2;
                                        targetYpreliminary_Inky = pacman.CurrentPosY - 2;
                                        break;
                                    case Direction.Down:
                                        targetXpreliminary_Inky = pacman.CurrentPosX;
                                        targetYpreliminary_Inky = pacman.CurrentPosY + 2;
                                        break;
                                    case Direction.Stationary:
                                        targetXpreliminary_Inky = pacman.CurrentPosX;
                                        targetYpreliminary_Inky = pacman.CurrentPosY;
                                        break;
                                }

                                // Inky's target is the position mirrored across Blinky's position
                                Ghost blinky = null;
                                foreach (Ghost g in ghosts)
                                {
                                    if (g.Template.Equals(GhostTemplate.Blinky))
                                    {
                                        blinky = g;
                                        break;
                                    }
                                }

                                int disDiffX_Inky = targetXpreliminary_Inky - blinky.CurrentPosX;
                                int disDiffY_Inky = targetYpreliminary_Inky - blinky.CurrentPosY;

                                if (blinky.CurrentPosX < targetXpreliminary_Inky && blinky.CurrentPosY > targetYpreliminary_Inky)
                                {
                                    ghost.SetTarget(targetXpreliminary_Inky + disDiffX_Inky, targetYpreliminary_Inky - disDiffY_Inky);
                                }
                                else if (blinky.CurrentPosX < targetXpreliminary_Inky && blinky.CurrentPosY < targetYpreliminary_Inky)
                                {
                                    ghost.SetTarget(targetXpreliminary_Inky + disDiffX_Inky, targetYpreliminary_Inky + disDiffY_Inky);
                                }
                                else if (blinky.CurrentPosX > targetXpreliminary_Inky && blinky.CurrentPosY < targetYpreliminary_Inky)
                                {
                                    ghost.SetTarget(targetXpreliminary_Inky - disDiffX_Inky, targetYpreliminary_Inky + disDiffY_Inky);
                                }
                                else if (blinky.CurrentPosX > targetXpreliminary_Inky && blinky.CurrentPosY > targetYpreliminary_Inky)
                                {
                                    ghost.SetTarget(targetXpreliminary_Inky - disDiffX_Inky, targetYpreliminary_Inky - disDiffY_Inky);
                                }
                                break;
                            case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Clyde):
                                if (!ghost.BehaviourOverridden)
                                {
                                    ghost.SetTarget(pacman.CurrentPos);
                                }
                                break;
                            default:
                                // Default to pacman's position TODO: implement custom ghost template
                                ghost.SetTarget(pacman.CurrentPos);
                                break;
                        }
                    }
                }
            }
        }

        private async void GhostEaten(Ghost ghost)
        {
            StopTimers();

            ghost.SetState(EntityState.Eaten);
            ghost.SetBehaviour(GhostBehaviour.Returning);

            ghost.SetTarget(GhostConstants.ReturnIndex);

            UpdateGhostLists();

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

            // Don't play more than one ghost_return playback at a time
            soundManager.PlaySound(Sounds.ghost_return, true, true);
        }

        private void ghostImageTimer_Tick(object sender, EventArgs e)
        {
            // Switch value on each tick for animation effect
            ghostPic_ver2 = !ghostPic_ver2;

            foreach (Ghost ghost in ghosts)
            {
                if (!ghost.CurrentState.Equals(EntityState.Eaten))
                {  
                    switch (ghost.CurrentDirection)
                    {
                        case Direction.Left:
                            if (ghostPic_ver2)
                            {
                                switch (ghost.Template)
                                {
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Blinky):
                                        ghost.box.Image = GhostConstants.Images.Blinky.left2;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Pinky):
                                        ghost.box.Image = GhostConstants.Images.Pinky.left2;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Inky):
                                        ghost.box.Image = GhostConstants.Images.Inky.left2;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Clyde):
                                        ghost.box.Image = GhostConstants.Images.Clyde.left2;
                                        continue;
                                }
                            }
                            else
                            {
                                switch (ghost.Template)
                                {
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Blinky):
                                        ghost.box.Image = GhostConstants.Images.Blinky.left;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Pinky):
                                        ghost.box.Image = GhostConstants.Images.Pinky.left;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Inky):
                                        ghost.box.Image = GhostConstants.Images.Inky.left;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Clyde):
                                        ghost.box.Image = GhostConstants.Images.Clyde.left;
                                        continue;
                                }
                            }
                            continue;
                        case Direction.Right:
                            if (ghostPic_ver2)
                            {
                                switch (ghost.Template)
                                {
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Blinky):
                                        ghost.box.Image = GhostConstants.Images.Blinky.right2;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Pinky):
                                        ghost.box.Image = GhostConstants.Images.Pinky.right2;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Inky):
                                        ghost.box.Image = GhostConstants.Images.Inky.right2;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Clyde):
                                        ghost.box.Image = GhostConstants.Images.Clyde.right2;
                                        continue;
                                }
                            }
                            else
                            {
                                switch (ghost.Template)
                                {
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Blinky):
                                        ghost.box.Image = GhostConstants.Images.Blinky.right;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Pinky):
                                        ghost.box.Image = GhostConstants.Images.Pinky.right;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Inky):
                                        ghost.box.Image = GhostConstants.Images.Inky.right;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Clyde):
                                        ghost.box.Image = GhostConstants.Images.Clyde.right;
                                        continue;
                                }
                            }
                            continue;
                        case Direction.Up:
                            if (ghostPic_ver2)
                            {
                                switch (ghost.Template)
                                {
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Blinky):
                                        ghost.box.Image = GhostConstants.Images.Blinky.up2;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Pinky):
                                        ghost.box.Image = GhostConstants.Images.Pinky.up2;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Inky):
                                        ghost.box.Image = GhostConstants.Images.Inky.up2;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Clyde):
                                        ghost.box.Image = GhostConstants.Images.Clyde.up2;
                                        continue;
                                }
                            }
                            else
                            {
                                switch (ghost.Template)
                                {
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Blinky):
                                        ghost.box.Image = GhostConstants.Images.Blinky.up;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Pinky):
                                        ghost.box.Image = GhostConstants.Images.Pinky.up;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Inky):
                                        ghost.box.Image = GhostConstants.Images.Inky.up;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Clyde):
                                        ghost.box.Image = GhostConstants.Images.Clyde.up;
                                        continue;
                                }
                            }
                            continue;
                        case Direction.Down:
                            if (ghostPic_ver2)
                            {
                                switch (ghost.Template)
                                {
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Blinky):
                                        ghost.box.Image = GhostConstants.Images.Blinky.down2;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Pinky):
                                        ghost.box.Image = GhostConstants.Images.Pinky.down2;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Inky):
                                        ghost.box.Image = GhostConstants.Images.Inky.down2;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Clyde):
                                        ghost.box.Image = GhostConstants.Images.Clyde.down2;
                                        continue;
                                }
                            }
                            else
                            {
                                switch (ghost.Template)
                                {
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Blinky):
                                        ghost.box.Image = GhostConstants.Images.Blinky.down;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Pinky):
                                        ghost.box.Image = GhostConstants.Images.Pinky.down;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Inky):
                                        ghost.box.Image = GhostConstants.Images.Inky.down;
                                        continue;
                                    case GhostTemplate _ when ghost.Template.Equals(GhostTemplate.Clyde):
                                        ghost.box.Image = GhostConstants.Images.Clyde.down;
                                        continue;
                                }
                            }
                            continue;
                    }
                    
                }
            }
        }

        private void ghostFrightenedImageTimer_Tick(object sender, EventArgs e)
        {
            ghostFrightenedPic_ver2 = !ghostFrightenedPic_ver2;

            foreach (Ghost ghost in ghostsFrightened)
            {
                if (ghostPic_ver2)
                {
                    if (ghost.White)
                    {
                        ghost.box.Image = GhostConstants.Images.frightenedWhite2;
                        continue;
                    }
                    else
                    {
                        ghost.box.Image = GhostConstants.Images.frightenedBlue2;
                        continue;
                    }
                }
                else
                {
                    if (ghost.White)
                    {
                        ghost.box.Image = GhostConstants.Images.frightenedWhite;
                        continue;
                    }
                    else
                    {
                        ghost.box.Image = GhostConstants.Images.frightenedBlue;
                        continue;
                    }
                }
            }
        }
    }
}