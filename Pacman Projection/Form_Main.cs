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

namespace Pacman_Projection
{
    public partial class Form_Main : Form
    {
        string player_name;
        // Declare level, max 10
        internal int level = 1;
        internal int levelToBeginAt;

        internal bool gamePaused;

        // Declare new instance of soundManager for managing form1's sounds
        SoundManager soundManager = new SoundManager();

        const int step = 14; // Pixels per step and size of blocks, one block per step
        // Declare boxesHorizontally and boxesVertically
        const int boxesHorizontally = 30; 
        const int boxesVertically = 39;
        // Declare boxSize to be as big as step, and entitySize to be twice that
        const int boxSize = step; 
        const int entitySize = boxSize * 2;
        // Declare verticalOffset for the boxes (the map)
        const int verticalOffset = boxSize * 2;

        internal string currentKey;
        internal string latestKey;

        // Declare the interval for pacTickTimer and ghostTickTimer, that is, essentially, their speed
        const int pacTickTimerIntervalStandard = 180;
        const int ghostTickTimerIntervalStandard = 180;

        /// <summary>
        /// Contains the speed of the ghosts for each level when they are scared, in milliseconds.
        /// Key is the level number, value is the speed in milliseconds.
        /// </summary>
        internal Dictionary<int, int> ghostSpeedForLevelScared = new Dictionary<int, int>
        {
            {1, ghostTickTimerIntervalStandard + 5*24}, // 300
            {2, ghostTickTimerIntervalStandard + 5*23},     
            {3, ghostTickTimerIntervalStandard + 5*22},          
            {4, ghostTickTimerIntervalStandard + 5*21},          
            {5, ghostTickTimerIntervalStandard + 5*20},       
            {6, ghostTickTimerIntervalStandard + 5*19},       
            {7, ghostTickTimerIntervalStandard + 5*18},                
            {8, ghostTickTimerIntervalStandard + 5*17},          
            {9, ghostTickTimerIntervalStandard + 5*16},      
            {10, ghostTickTimerIntervalStandard + 5*15} // 255
        };

        /// <summary>
        /// Contains the speed of the ghosts for each level, in milliseconds.
        /// Key is the level number, value is the speed in milliseconds.
        /// </summary>
        internal Dictionary<int, int> ghostSpeedForLevel = new Dictionary<int, int>
        {
            {1, ghostTickTimerIntervalStandard}, // 180
            {2, ghostTickTimerIntervalStandard - 3},    
            {3, ghostTickTimerIntervalStandard - 3*2},
            {4, ghostTickTimerIntervalStandard - 3*3},
            {5, ghostTickTimerIntervalStandard - 3*4},
            {6, ghostTickTimerIntervalStandard - 3*5},
            {7, ghostTickTimerIntervalStandard - 3*6},
            {8, ghostTickTimerIntervalStandard - 3*7},
            {9, ghostTickTimerIntervalStandard - 3*8},
            {10, ghostTickTimerIntervalStandard - 3*9} // 153
        };

        /// <summary>
        /// Contains the speed of Pacman for each level, in milliseconds.
        /// Key is the level number, value is the speed in milliseconds.
        /// </summary>
        internal Dictionary<int, int> pacmanSpeedForLevel = new Dictionary<int, int>
        {
            {1, pacTickTimerIntervalStandard}, // 180
            {2, pacTickTimerIntervalStandard - 2}, 
            {3, pacTickTimerIntervalStandard - 2*2}, 
            {4, pacTickTimerIntervalStandard - 2*3}, 
            {5, pacTickTimerIntervalStandard - 2*4}, 
            {6, pacTickTimerIntervalStandard - 2*5}, 
            {7, pacTickTimerIntervalStandard - 2*6}, 
            {8, pacTickTimerIntervalStandard - 2*7}, 
            {9, pacTickTimerIntervalStandard - 2*8}, 
            {10, pacTickTimerIntervalStandard - 2*9} // 162
        };

        // Declare all required labels and buttons
        internal System.Windows.Forms.Label labelReady = new System.Windows.Forms.Label();
        internal System.Windows.Forms.Label labelGameOver = new System.Windows.Forms.Label();
        internal System.Windows.Forms.Label labelLevel = new System.Windows.Forms.Label();
        internal System.Windows.Forms.Label labelFruitSpawnChance = new System.Windows.Forms.Label();
        internal Button buttonRestartGame = new Button();

        // Declare array containing all boxes and a list for all walls
        internal Box[,] boxes = new Box[boxesHorizontally, boxesVertically];
        internal List<Box> walls = new List<Box>(); 
        // Declare Pacman, his start coordinates, and his life list containing three lives
        internal Pacman pacman = new Pacman(new PictureBox(), new PictureBox());
        const int pacman_StartX = boxSize*14;
        const int pacman_StartY = boxSize*25;

        internal bool pacPic_open;

        internal List<PictureBox> pacmanLives = new List<PictureBox>
        {
            new PictureBox(),
            new PictureBox(),
            new PictureBox()
        };

        // Declare list with all pacmans death images in to loop through on death
        internal List<Image> pacmanDeathSequence = new List<Image>
        {
            Resources.pacman_death_0_8_,
            Resources.pacman_death_1_8_,
            Resources.pacman_death_2_8_,
            Resources.pacman_death_3_8_,
            Resources.pacman_death_4_8_,
            Resources.pacman_death_5_8_,
            Resources.pacman_death_6_8_,
            Resources.pacman_death_7_8_,
            Resources.pacman_death_8_8_
        };

        // Declare list containing the ghosts
        internal List<Ghost> ghosts = new List<Ghost>();
        // Declare ghosts and their respective starting positions and directions
        internal Ghost Blinky;
        const int Blinky_StartX = boxSize*14;
        const int Blinky_StartY = boxSize*16;
        internal Ghost Pinky;
        const int Pinky_StartX = boxSize*14;
        const int Pinky_StartY = boxSize*21;
        internal Ghost Inky;
        const int Inky_StartX = boxSize*12;
        const int Inky_StartY = boxSize*20;
        internal Ghost Clyde;
        const int Clyde_StartX = boxSize*16;
        const int Clyde_StartY = boxSize*20;

        internal int ghostsEatenDuringPeriod;
        internal bool ghostBlink;
        internal int currentEatGhostDuration;
        const int ghostBlinkDuration = 250;
        const int timesToBlink = 6;
        internal bool ghostScared;
        internal bool ghostScatter;
        internal bool ghostChase;

        internal bool ghostPic_ver2;

        const string BlinkyStartDirection = "Left";
        internal Image BlinkyStartImage = Resources.Blinky_left;
        const string PinkyStartDirection = "Down";
        internal Image PinkyStartImage = Resources.Pinky_down;
        const string InkyStartDirection = "Up";
        internal Image InkyStartImage = Resources.Inky_up;
        const string ClydeStartDirection = "Up";
        internal Image ClydeStartImage = Resources.Clyde_up;

        internal string mostRecentGlobalBehaviour;
        internal string currentGlobalBehaviour;

        /// <summary>
        /// Contains the time for ghosts to scatter and chase in the format "scatter,chase" for each level, in whole seconds.
        /// Key is the level number, value is a string which contains each speed seperated by a comma.
        /// </summary>
        internal Dictionary<int, string> ghostScatterChaseTimesForLevel = new Dictionary<int, string>
        {
            // scatter,chase 
            {1, "7,15"},
            {2, "7,15"},
            {3, "7,15"},
            {4, "7,17"},
            {5, "5,17"},
            {6, "5,17"},
            {7, "5,17"},
            {8, "5,17"},
            {9, "5,17"},
            {10,"5,17"}
        };

        internal bool toChangeBehaviourSound = true; // Start as true so sound plays at the start of the game, before any behaviour changes
        internal int secondsOfSameBehaviour; 

        // Declare foodsHorizontally and foodsVertically 
        const int foodsHorizontally = 29;
        const int foodsVertically = 37;
        // Declare foodGrid for all food boxes
        internal Box[,] foodGrid = new Box[foodsHorizontally, foodsVertically];
        // Declare list for all big food indexes
        internal List<string> bigFoodIndexes = new List<string>();

        // Declare food offset
        const int horizontalFoodOffset = boxSize + boxSize / 2;
        const int verticalFoodOffset = boxSize * 3 + boxSize / 2;

        // Declare scores to get when eating different types of food and amount of foods placed
        const int foodScore = 10;
        const int foodScoreBig = 50;
        const int ghostScore = 200;
        internal int foodEaten;
        internal int foodEatenBig;
        internal bool filled;
        internal int foodOnMap;

        internal PictureBox fruitBox = new PictureBox();
        internal int fruitEaten;
        internal int fruitSpawned;
        internal string currentFruit;

        internal Dictionary<string, int> fruitScore = new Dictionary<string, int>
        {
            { "cherry", 200 },
            { "strawberry", 500 },
            { "apple", 800 },
            { "banana", 1200 },
            { "melon", 1500 }
        };

        const int msToAddAfterBigFood = 8000;
        const int msDeathSequence = 160;
        const int msToWaitAfterGhostsAppear = 2800;
        const int msToWaitBetweenGames = 1500;
        const int msToWaitBeforeRestart = 2500;
        const int msToWaitAfterDeath = 800;
        const int msToWaitAfterGhostEaten = 1000;
        const int msToWaitBetweenWallBlink = 180;

        internal bool timersDisabled;

        // Declare score and scoreLabel
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
            ClientSize = new Size(boxesHorizontally * boxSize, boxesVertically * boxSize + boxSize);
            this.BackColor = Color.Black;
            this.Location = new Point(388, 57);
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Set timerIntervals to designated interval
            pacTickTimer.Interval = pacmanSpeedForLevel[level];
            ghostTickTimer.Interval = ghostSpeedForLevel[level];
            updateEatGhostDurationTimer.Interval = ghostBlinkDuration;

            //
            // Create all boxes
            //

            for (int horizontalIndex = 0; horizontalIndex < boxesHorizontally; horizontalIndex++)
            {
                for (int verticalIndex = 0; verticalIndex < boxesVertically; verticalIndex++)
                {
                    // Create the box
                    Box box = new Box(new PictureBox(), false, false, false, true, false);
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
            pacman.box.Location = new Point(pacman_StartX, pacman_StartY);
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
            fruitBox.Location = new Point(pacman_StartX, pacman_StartY);
            fruitBox.SizeMode = PictureBoxSizeMode.StretchImage;
            Controls.Add(fruitBox);
            fruitBox.BringToFront();

            //
            // Ghosts properties
            //

            // Blinky
            Blinky = new Ghost(new PictureBox(), new PictureBox(), "blinky");
            Blinky.box.Size = new Size(entitySize, entitySize);
            Blinky.box.Image = Resources.Blinky_left;
            Blinky.box.LocationChanged += Blinky_LocationChanged;
            Blinky.box.Location = new Point(Blinky_StartX, Blinky_StartY);
            Blinky.cornerDuringScatter = "TopRight";

            Controls.Add(Blinky.box);
            Blinky.box.BringToFront();
            Blinky.box.Hide();
            ghosts.Add(Blinky);

            // Blinky.navBox
            Blinky.navBox.Size = new Size(boxSize, boxSize);
            Controls.Add(Blinky.navBox);
            Blinky.UpdateLocation(Blinky.box.Left, Blinky.box.Top);


            // Pinky
            Pinky = new Ghost(new PictureBox(), new PictureBox(), "pinky");
            Pinky.box.Size = new Size(entitySize, entitySize);
            Pinky.box.Image = Resources.Pinky_down;
            Pinky.box.LocationChanged += Pinky_LocationChanged;
            Pinky.box.Location = new Point(Pinky_StartX, Pinky_StartY);
            Pinky.cornerDuringScatter = "TopLeft";

            Controls.Add(Pinky.box);
            Pinky.box.BringToFront();
            Pinky.box.Hide();
            ghosts.Add(Pinky);

            // Pinky.navBox
            Pinky.navBox.Size = new Size(boxSize, boxSize);
            Controls.Add(Pinky.navBox);
            Pinky.UpdateLocation(Pinky.box.Left, Pinky.box.Top);


            // Inky
            Inky = new Ghost(new PictureBox(), new PictureBox(), "inky");
            Inky.box.Size = new Size(entitySize, entitySize);
            Inky.box.Image = Resources.Inky_up;
            Inky.box.LocationChanged += Inky_LocationChanged;
            Inky.box.Location = new Point(Inky_StartX, Inky_StartY);
            Inky.cornerDuringScatter = "BottomRight";

            Controls.Add(Inky.box);
            Inky.box.BringToFront();
            Inky.box.Hide();
            ghosts.Add(Inky);

            // Inky.navBox
            Inky.navBox.Size = new Size(boxSize, boxSize);
            Controls.Add(Inky.navBox);
            Inky.UpdateLocation(Inky.box.Left, Inky.box.Top);


            // Clyde
            Clyde = new Ghost(new PictureBox(), new PictureBox(), "clyde");
            Clyde.box.Size = new Size(entitySize, entitySize);
            Clyde.box.Image = Resources.Clyde_up;
            Clyde.box.LocationChanged += Clyde_LocationChanged;
            Clyde.box.Location = new Point(Clyde_StartX, Clyde_StartY);
            Clyde.cornerDuringScatter = "BottomLeft";

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
            for (int indexX = 0; indexX < boxesHorizontally; indexX++)
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

                boxes[boxesHorizontally - 1, indexY].isWall = true;
                boxes[boxesHorizontally - 1, indexY].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[boxesHorizontally - 1, indexY]);
            }
            // Lower wall
            for (int indexX = 0; indexX < boxesHorizontally; indexX++)
            {
                boxes[indexX, boxesVertically - 2].isWall = true;
                boxes[indexX, boxesVertically - 2].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, boxesVertically - 2]);
            }
            // Left & right lower walls
            for (int indexY = 27; indexY < boxesVertically - 1; indexY++)
            {
                boxes[0, indexY].isWall = true;
                boxes[0, indexY].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[0, indexY]);

                boxes[boxesHorizontally - 1, indexY].isWall = true;
                boxes[boxesHorizontally - 1, indexY].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[boxesHorizontally - 1, indexY]);
            }

            // Left middle walls
            for (int indexX = 0; indexX < 5; indexX++)
            {
                boxes[indexX, 12].isWall = true;
                boxes[indexX, 12].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, 12]);

                boxes[indexX, boxesHorizontally - 4].isWall = true;
                boxes[indexX, boxesHorizontally - 4].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, boxesHorizontally - 4]);
            }
            for (int indexY = 12; indexY < 17; indexY++)
            {
                boxes[5, indexY].isWall = true;
                boxes[5, indexY].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[5, indexY]);

                boxes[5, boxesHorizontally - indexY + 8].isWall = true;
                boxes[5, boxesHorizontally - indexY + 8].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[5, boxesHorizontally - indexY + 8]);
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
            for (int indexX = boxesHorizontally - 1; indexX > boxesHorizontally - 6; indexX--)
            {
                boxes[indexX, 12].isWall = true;
                boxes[indexX, 12].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, 12]);

                boxes[indexX, boxesHorizontally - 4].isWall = true;
                boxes[indexX, boxesHorizontally - 4].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[indexX, boxesHorizontally - 4]);
            }
            for (int indexY = 12; indexY < 17; indexY++)
            {
                boxes[boxesHorizontally - 6, indexY].isWall = true;
                boxes[boxesHorizontally - 6, indexY].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[boxesHorizontally - 6, indexY]);

                boxes[boxesHorizontally - 6, boxesHorizontally - indexY + 8].isWall = true;
                boxes[boxesHorizontally - 6, boxesHorizontally - indexY + 8].pictureBox.BackColor = Color.Blue;
                walls.Add(boxes[boxesHorizontally - 6, boxesHorizontally - indexY + 8]);
            }
            for (int indexX = boxesHorizontally - 6; indexX < boxesHorizontally; indexX++)
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

            await Task.Delay(msToWaitBetweenGames);
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
            await Task.Delay(msToWaitBetweenGames);

            // Set ghosts starting directions
            Blinky.SetDirection(BlinkyStartDirection);
            Pinky.SetDirection(PinkyStartDirection);
            Inky.SetDirection(InkyStartDirection);
            Clyde.SetDirection(ClydeStartDirection);

            Blinky.SetScatter();
            Pinky.SetScatter();
            Inky.SetScatter();
            Clyde.SetScatter();
            currentGlobalBehaviour = "scatter";

            ghostTickTimer.Interval = ghostSpeedForLevel[level];

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
            await Task.Delay(msToWaitAfterGhostsAppear);

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
                
            await Task.Delay(msToWaitBetweenGames);

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
                    await Task.Delay(msToWaitBetweenWallBlink);
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
                foreach (Image image in pacmanDeathSequence)
                {
                    pacman.box.Image = image;
                    await Task.Delay(msDeathSequence);
                }

                pacman.box.Hide();
                Restart(false);
            }
        }
        private async void Restart(bool win)
        {
            await Task.Delay(msToWaitAfterDeath);

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
                Blinky.box.Location = new Point(Blinky_StartX, Blinky_StartY);
                Pinky.box.Location = new Point(Pinky_StartX, Pinky_StartY);
                Inky.box.Location = new Point(Inky_StartX, Inky_StartY);
                Clyde.box.Location = new Point(Clyde_StartX, Clyde_StartY);
                pacman.box.Location = new Point(pacman_StartX, pacman_StartY);
                pacman.box.Image = Resources.Pacman_stationary;
                ResetPacmanKey();
                // Set ghosts starting directions, pictures, and make them visible 
                Blinky.SetDirection(BlinkyStartDirection);
                Blinky.box.Image = BlinkyStartImage;
                Pinky.SetDirection(PinkyStartDirection);
                Pinky.box.Image = PinkyStartImage;
                Inky.SetDirection(InkyStartDirection);
                Inky.box.Image = InkyStartImage;
                Clyde.SetDirection(ClydeStartDirection);
                Clyde.box.Image = ClydeStartImage;

                Blinky.dead = false;
                Pinky.dead = false;
                Inky.dead = false;
                Clyde.dead = false;

                currentEatGhostDuration = 0;

                if (win)
                {
                    // Reset all variables  
                    foodEaten = 0;
                    foodEatenBig = 0;
                    currentEatGhostDuration = 0;
                    fruitEaten = 0;
                    fruitSpawned = 0;

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

                await Task.Delay(msToWaitBeforeRestart);

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

                await Task.Delay(msToWaitAfterGhostEaten);

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

        private void pacman_LocationChanged(object sencer, EventArgs e)
        {
            if (!ghostScared)
            {
                foreach (Ghost ghost in ghosts)
                {
                    if (ghost.dead)
                    {
                        pacman.box.BringToFront();
                        return;
                    }

                    if (pacman.box.Bounds.IntersectsWith(ghost.box.Bounds))
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
                            GhostEaten(Clyde);
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
            
            FoodEaten(GetFoodCollide(pacman.eatBox), CheckForFoodCollide(pacman.eatBox).bigFood);

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
            latestKey = "";
            currentKey = "";
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
            // For loop which fills the food list and places it on the map while
            // checking if it collides with any walls, if so, they are removed
            for (int indexY = 0; indexY < foodsVertically; indexY++)
            {
                // Don't add a food to where pacman starts
                for (int indexX = 0; indexX < foodsHorizontally; indexX++)
                {
                    // Only declare the foods the first time PlaceAllFood is run
                    if (foodGrid[indexX, indexY] == null)
                    {
                        if (indexX == 0 && indexY == 0
                        || indexX == 26 && indexY == 0
                        || indexX == 0 && indexY == 34
                        || indexX == 26 && indexY == 34)
                        {
                            foodGrid[indexX, indexY] = new Box(new PictureBox(), false, false, false, true, true);
                            foodGrid[indexX, indexY].pictureBox.Image = Resources.FoodBig;
                            foodGrid[indexX, indexY].eaten = false;
                            // Add big food index to the list for use "pacTickTimer" method
                            bigFoodIndexes.Add(indexX.ToString() + "_" + indexY.ToString());
                        }
                        else
                        {
                            foodGrid[indexX, indexY] = new Box(new PictureBox(), false, false, false, true, false);
                            foodGrid[indexX, indexY].pictureBox.Image = Resources.Food;
                            foodGrid[indexX, indexY].eaten = false;
                        }

                        foodGrid[indexX, indexY].pictureBox.Size = new Size(boxSize, boxSize);
                        Controls.Add(foodGrid[indexX, indexY].pictureBox);

                        // Place all foods in a grid-pattern over the map
                        // If a food collides with a wall, it will be removed
                        // The same applies to foods that are placed beside others foods,
                        // creating areas of dense foods, as well as foods placed outside the map or generally where they are not supposed to be
                        foodGrid[indexX, indexY].pictureBox.Location = new Point(indexX * boxSize + horizontalFoodOffset, indexY * boxSize + verticalFoodOffset);

                        if (AbleToPlaceFood(indexX, indexY))
                        {   
                            foodGrid[indexX, indexY].pictureBox.BringToFront();
                            foodOnMap++;  
                        }
                        else
                        {
                            foodGrid[indexX, indexY].pictureBox.Hide();
                        }
                    }
                    else
                    {
                        if (indexX == 0 && indexY == 0
                        || indexX == 26 && indexY == 0
                        || indexX == 0 && indexY == 34
                        || indexX == 26 && indexY == 34)
                        {
                            foodGrid[indexX, indexY].pictureBox.Image = Resources.FoodBig;
                            foodGrid[indexX, indexY].eaten = false;
                        }
                        else
                        {
                            foodGrid[indexX, indexY].pictureBox.Image = Resources.Food;
                            foodGrid[indexX, indexY].eaten = false;
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
                for (int indexX = 0; indexX < foodsHorizontally; indexX++)
                {
                    for (int indexY = 0; indexY < foodsVertically; indexY++)
                    {
                        if (foodGrid[indexX, indexY].pictureBox.Bounds.IntersectsWith(eatBox.Bounds) && !foodGrid[indexX, indexY].eaten)
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

        private (bool food, bool bigFood) CheckForFoodCollide(PictureBox eatBox)
        {
            try
            {
                for (int indexX = 0; indexX < foodsHorizontally; indexX++)
                {
                    for (int indexY = 0; indexY < foodsVertically; indexY++)
                    {
                        if (foodGrid[indexX, indexY] != null)
                        {
                            if (foodGrid[indexX, indexY].pictureBox.Bounds.IntersectsWith(eatBox.Bounds) && foodGrid[indexX, indexY].pictureBox.Image != null)
                            {
                                if (foodGrid[indexX, indexY].isBigFood == false)
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

        private void FoodEaten(Box foodBox, bool bigFood)
        {
            if (foodBox != null)
            {
                if (!foodBox.eaten)
                {
                    if (!bigFood)
                    {
                        soundManager.PlaySound("pacman_chomp", false);
                        UpdateScore(foodScore, true);
                        foodBox.pictureBox.Image = null;
                        foodBox.eaten = true;
                    }
                    else if (bigFood)
                    {
                        currentEatGhostDuration += msToAddAfterBigFood;
                        // If the ghosts are blinking, make them stop as
                        // currentGhostEatDuration is now over the threshold,
                        // regardless of its previous value
                        Blinky.white = false;
                        Pinky.white = false;
                        Inky.white = false;
                        Clyde.white = false;
                        ghostBlink = false;

                        // Ensure all ghosts are frightened
                        if (!ghostScared)
                        {
                            SetGhosts_Scared();
                        }
                        ghostBehaviourTimeTimer.Stop();

                        UpdateScore(foodScoreBig, true);
                        foodBox.pictureBox.Image = null;
                        foodBox.eaten = true;

                        foodEatenBig++;
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
            filled = !filled;

            if (filled)
            {
                for (int index = 0; index < 4; index++)
                {
                    string[] indexes = bigFoodIndexes[index].Split('_');
                    int indexX = Convert.ToInt32(indexes[0]);
                    int indexY = Convert.ToInt32(indexes[1]);
                    if (foodGrid[indexX, indexY].pictureBox.Image != null)
                    {
                        foodGrid[Convert.ToInt32(indexes[0]), Convert.ToInt32(indexes[1])].pictureBox.Show();
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
                    if (foodGrid[indexX, indexY].pictureBox.Image != null)
                    {
                        foodGrid[Convert.ToInt32(indexes[0]), Convert.ToInt32(indexes[1])].pictureBox.Hide();
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
            foreach (string fruitName in fruitScore.Keys)
            {
                if (fruitName == currentFruit)
                {
                    UpdateScore(fruitScore[fruitName], true);
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
                if (fruitSpawned > 0)
                {
                    fruitSpawnChancePercent = foodEaten / (110.0 * (fruitSpawned * 2)) + level / 30;
                }

                if (AbleToPlaceFruit() && fruitSpawnChancePercent >= 1)
                {
                    if (level <= 3)
                    {
                        int fruit = new Random().Next(0, 5);
                        if (fruit <= 1)
                        {
                            fruitBox.Image = Resources.Cherry; // 40%
                            currentFruit = "cherry";
                        }
                        else if (fruit <= 3)
                        {
                            fruitBox.Image = Resources.Strawberry; // 40%
                            currentFruit = "strawberry";
                        }
                        else if (fruit >= 4)
                        {
                            fruitBox.Image = Resources.Apple; // 20%
                            currentFruit = "apple";
                        }
                    }
                    else if (level >= 4 && level < 7)
                    {
                        int fruit = new Random().Next(0, 20);
                        if (fruit <= 3)
                        {
                            fruitBox.Image = Resources.Cherry; // 20% 
                            currentFruit = "cherry";
                        }
                        else if (fruit <= 10)
                        {
                            fruitBox.Image = Resources.Strawberry; // 35%
                            currentFruit = "strawberry";
                        }
                        else if (fruit <= 15)
                        {
                            fruitBox.Image = Resources.Apple; // 25%
                            currentFruit = "apple";
                        }
                        else if (fruit >= 16)
                        {
                            fruitBox.Image = Resources.Banana; // 20%
                            currentFruit = "banana";
                        }
                    }
                    else if (level >= 7)
                    {
                        int fruit = new Random().Next(0, 20);
                        if (fruit == 0)
                        {
                            fruitBox.Image = Resources.Cherry; // 5% 
                            currentFruit = "cherry";
                        }
                        else if (fruit <= 3)
                        {
                            fruitBox.Image = Resources.Strawberry; // 15%
                            currentFruit = "strawberry";
                        }
                        else if (fruit <= 8)
                        {
                            fruitBox.Image = Resources.Apple; // 25%
                            currentFruit = "apple";
                        }
                        else if (fruit <= 14)
                        {
                            fruitBox.Image = Resources.Banana; // 30%
                            currentFruit = "banana";
                        }
                        else if (fruit >= 15)
                        {
                            fruitBox.Image = Resources.Melon; // 25%
                            currentFruit = "melon";
                        }
                    }

                    fruitBox.BringToFront();
                    fruitSpawned++;
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

            if (Blinky.scatter) 
            {
                if (secondsOfSameBehaviour == Convert.ToInt32(ghostScatterChaseTimesForLevel[level].Split(',')[0])) {
                    SetGhosts_Chase();
                    secondsOfSameBehaviour = 0;
                    behaviourChangeThisTick = true;
                }
            }
            else if (Blinky.chase)
            {
                if (secondsOfSameBehaviour == Convert.ToInt32(ghostScatterChaseTimesForLevel[level].Split(',')[1]))
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
            if (currentEatGhostDuration == ghostBlinkDuration) // currentEatGhostDuration ends this tick
            {
                toStopScared = true;
            }

            if (currentEatGhostDuration > 0)
            {
                if (!ghostScared)
                {
                    SetGhosts_Scared();
                }

                currentEatGhostDuration -= ghostBlinkDuration;
                if (toStopScared && currentEatGhostDuration == 0)
                {
                    ghostScared = false;
                    ghostsEatenDuringPeriod = 0;

                    if (mostRecentGlobalBehaviour == "scatter")
                    {
                        SetGhosts_Scatter();
                    }
                    else if (mostRecentGlobalBehaviour == "chase")
                    {
                        SetGhosts_Chase();
                    }

                    ghostBehaviourTimeTimer.Start();
                }
            }

            if (ghostScared)
            {
                if (currentEatGhostDuration <= (ghostBlinkDuration * timesToBlink) || ghostBlink)
                {
                    ghostBlink = true;
                    if (currentEatGhostDuration / ghostBlinkDuration % 2 == 0)
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
                        UpdateGhostTarget(Blinky);
                    }
                }
                else if (!Blinky.scared && !Blinky.dead)
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
                else if (!Blinky.scared && !Blinky.dead)
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
                else if (!Blinky.scared && !Blinky.dead)
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
                else if (!Blinky.scared && !Blinky.dead)
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
                        UpdateGhostTarget(Pinky);
                    }
                }
                else if (!Pinky.scared && !Pinky.dead)
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
                else if (!Pinky.scared && !Pinky.dead)
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
                else if (!Pinky.scared && !Pinky.dead)
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
                else if (!Pinky.scared && !Pinky.dead)
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
                        UpdateGhostTarget(Inky);
                    }
                }
                else if (!Inky.scared && !Inky.dead)
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
                else if (!Inky.scared && !Inky.dead)
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
                else if (!Inky.scared && !Inky.dead)
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
                else if (!Inky.scared && !Inky.dead)
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
                        UpdateGhostTarget(Clyde);
                    }
                }
                else if (!Clyde.scared && !Clyde.dead)
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
                else if (!Clyde.scared && !Clyde.dead)
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
                else if (!Clyde.scared && !Clyde.dead)
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
                else if (!Clyde.scared && !Clyde.dead)
                {
                    Clyde.box.Top += step;
                    Game(false);
                }
            }
        }

        private void SetGhosts_Scared()
        {
            // Switch current behaviour to most recent behaviour before its updated
            mostRecentGlobalBehaviour = currentGlobalBehaviour;
            currentGlobalBehaviour = "scared";
            ghostScared = true;
            SetSound_Scared();

            Blinky.SetScared();
            Pinky.SetScared();
            Inky.SetScared();
            Clyde.SetScared();

            ghostTickTimer.Interval = ghostSpeedForLevelScared[level];
        }

        private void SetGhosts_Scatter()
        {
            mostRecentGlobalBehaviour = currentGlobalBehaviour;
            currentGlobalBehaviour = "scatter";
            SetSound_Scatter();

            Blinky.SetScatter();
            Pinky.SetScatter();
            Inky.SetScatter();
            Clyde.SetScatter();

            ghostTickTimer.Interval = ghostSpeedForLevel[level];
        }

        private void SetGhosts_Chase()
        {
            mostRecentGlobalBehaviour = currentGlobalBehaviour;
            currentGlobalBehaviour = "chase";
            SetSound_Chase();

            Blinky.SetChase();
            Pinky.SetChase();
            Inky.SetChase();
            Clyde.SetChase();

            ghostTickTimer.Interval = ghostSpeedForLevel[level];
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

            if (ghost.name == "blinky")
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
                        testGhost.Top -= step;
                    }
                    else if (direction == 1)
                    {
                        testGhost.Top += step;
                    }
                    else if (direction == 2)
                    {
                        testGhost.Left -= step;
                    }
                    else
                    {
                        testGhost.Left += step;
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
            }
        }

        private async void GhostEaten(Ghost ghost)
        {
            StopTimers();
            
            ghost.dead = true;
            ghost.box.Image = Resources.Ghost_Eyes_up;
            ghostsEatenDuringPeriod++;

            UpdateScore(ghostScore * ghostsEatenDuringPeriod, true, true, ghost.box);

            ghost.box.Hide();
            pacman.box.Hide();

            soundManager.PlaySound("pacman_eatGhost", false);

            // add msToWaitAfterGhostEaten to currentEatGhostDuraiton to negate the fact
            // that eatGhostDecreaseDuration still decreases currentEatGhostDuration while 
            // the timers are paused
            currentEatGhostDuration += msToWaitAfterGhostEaten;
            await Task.Delay(msToWaitAfterGhostEaten);
            
            ghost.box.Show();
            pacman.box.Show();

            StartTimers();
        }

        private void ghostImageTimer_Tick(object sender, EventArgs e)
        {
            ghostPic_ver2 = !ghostPic_ver2;

            //
            // Blinky
            //
            if (!ghostPic_ver2)
            {
                if (!Blinky.dead)
                {
                    if (Blinky.scared)
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
                    if (Pinky.scared)
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
                    if (Inky.scared)
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
                    if (Clyde.scared)
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
                    if (Blinky.scared)
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
                    if (Pinky.scared)
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
                    if (Inky.scared)
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
                    if (Clyde.scared)
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