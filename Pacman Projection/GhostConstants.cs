using Pacman_Projection.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman_Projection
{
    /// <summary>
    /// Contains constants for the ghosts, such as their starting positions, speeds for each level, and scatter/chase times.
    /// </summary>
    public static class GhostConstants
    {
        public const int StandardInterval = 270;

        public const int StandardReturnInterval = 60;
        /// <summary>
        /// The index the ghosts return to when eaten
        /// </summary>
        public static int[] ReturnIndex = { 14, 18}; 

        /// <summary>
        /// The index the ghosts target when leaving the ghost house
        /// </summary>
        public static int[] OutOfHouseIndex = {14, 13};
        /// <summary>
        /// Contains the speed of the ghosts for each level when they are scared, in milliseconds.
        /// Key is the level number, value is the speed (interval) in milliseconds.
        /// </summary>
        public static Dictionary<int, int> SpeedForLevel_Frightened = new Dictionary<int, int>
        {
            {1, StandardInterval + 5*32}, // 430ms
            {2, StandardInterval + 5*31},
            {3, StandardInterval + 5*30},
            {4, StandardInterval + 5*29},
            {5, StandardInterval + 5*28},
            {6, StandardInterval + 5*27},
            {7, StandardInterval + 5*26},
            {8, StandardInterval + 5*25},
            {9, StandardInterval + 5*24},
            {10, StandardInterval + 5*23} // 385ms
        };

        /// <summary>
        /// Contains the speed of the ghosts for each level, in milliseconds.
        /// Key is the level number, value is the speed (interval) in milliseconds.
        /// </summary>
        public static Dictionary<int, int> SpeedForLevel = new Dictionary<int, int>
        {
            {1, StandardInterval}, // 270ms
            {2, StandardInterval - 3},
            {3, StandardInterval - 3*2},
            {4, StandardInterval - 3*3},
            {5, StandardInterval - 3*4},
            {6, StandardInterval - 3*5},
            {7, StandardInterval - 3*6},
            {8, StandardInterval - 3*7},
            {9, StandardInterval - 3*8},
            {10, StandardInterval - 3*9} // 243ms
        };

        public const int BlinkDuration = 250;
        public const int TimesToBlink = 6;

        /// <summary>
        /// Contains the time for ghosts to scatter and chase in the format "scatter,chase" for each level, in whole seconds.
        /// Key is the level number, value is a string which contains each time seperated by a comma.
        /// </summary>
        public static Dictionary<int, string> ScatterChaseTimesForLevel = new Dictionary<int, string>
        {
            // scatter,chase 
            {1, "10,10"},
            {2, "10,11"},
            {3, "9,11"},
            {4, "9,12"},
            {5, "8,12"},
            {6, "8,13"},
            {7, "7,13"},
            {8, "6,14"},
            {9, "5,15"},
            {10,"5,17"}
        };

        /// <summary>
        /// The bottom-left corner index of the game grid for ghost scatter targeting
        /// </summary>
        public static int[] MapCorner_BottomLeftIndex = { 3, GameConstants.GameBoxes_Vertically - 1};
        /// <summary>
        /// The top-left corner index of the game grid for ghost scatter targeting
        /// </summary>
        public static int[] MapCorner_TopLeftIndex = { 4, 0 };
        /// <summary>
        /// The top-right corner index of the game grid for ghost scatter targeting
        /// </summary>
        public static int[] MapCorner_TopRightIndex = { GameConstants.GameBoxes_Horizontally - 2 , 3 };
        /// <summary>
        /// The bottom-right corner index of the game grid for ghost scatter targeting
        /// </summary>
        public static int[] MapCorner_BottomRightIndex = { GameConstants.GameBoxes_Horizontally - 4, GameConstants.GameBoxes_Vertically - 2};


        /// <summary>
        /// The standard hierarchy for ghost behaviours, Frightened > Chase > Scatter for typical behaviours
        /// This can be changed when creating a custom ghost.
        /// </summary>
        public static List<GhostBehaviour> DefaultBehaviourHierarchy = new List<GhostBehaviour>
        {
            GhostBehaviour.ExitingHouse,
            GhostBehaviour.Returning,
            GhostBehaviour.Frightened,
            GhostBehaviour.Chase,
            GhostBehaviour.Scatter,
        };

        public static class Images
        {
            public static Image frightenedBlue = Resources.Ghost_Scared_Blue;
            public static Image frightenedBlue2 = Resources.Ghost_Scared_Blue_ver__2;

            public static Image frightenedWhite = Resources.Ghost_Scared_White;
            public static Image frightenedWhite2 = Resources.Ghost_Scared_White_ver__2;

            public static Image eyesLeft = Resources.Ghost_Eyes_left;
            public static Image eyesRight = Resources.Ghost_Eyes_right;
            public static Image eyesUp = Resources.Ghost_Eyes_up;
            public static Image eyesDown = Resources.Ghost_Eyes_down;
            public static Image eyesStationary = Resources.Ghost_Eyes_stationary;

            public static class Blinky
            {
                public static Image StartImage = Resources.Blinky_left;

                public static Image left = Resources.Blinky_left;
                public static Image right = Resources.Blinky_right;
                public static Image up = Resources.Blinky_up;
                public static Image down = Resources.Blinky_down;
                
                public static Image left2 = Resources.Blinky_left_ver__2;
                public static Image right2 = Resources.Blinky_right_ver__2;
                public static Image up2 = Resources.Blinky_up_ver__2;
                public static Image down2 = Resources.Blinky_down_ver__2;

                public static Image stationary = Resources.Blinky_stationary;
                public static Image stationary2 = Resources.Blinky_stationary_ver__2;
            }

            public static class Pinky
            {
                public static Image StartImage = Resources.Pinky_down;

                public static Image left = Resources.Pinky_left;
                public static Image right = Resources.Pinky_right;
                public static Image up = Resources.Pinky_up;
                public static Image down = Resources.Pinky_down;

                public static Image left2 = Resources.Pinky_left_ver__2;
                public static Image right2 = Resources.Pinky_right_ver__2;
                public static Image up2 = Resources.Pinky_up_ver__2;
                public static Image down2 = Resources.Pinky_down_ver__2;

                public static Image stationary = Resources.Pinky_stationary;
                public static Image stationary2 = Resources.Pinky_stationary_ver__2;
            }

            public static class Inky
            {
                public static Image StartImage = Resources.Inky_up;

                public static Image left = Resources.Inky_left;
                public static Image right = Resources.Inky_right;
                public static Image up = Resources.Inky_up;
                public static Image down = Resources.Inky_down;

                public static Image left2 = Resources.Inky_left_ver__2;
                public static Image right2 = Resources.Inky_right_ver__2;
                public static Image up2 = Resources.Inky_up_ver__2;
                public static Image down2 = Resources.Inky_down_ver__2;

                public static Image stationary = Resources.Inky_stationary;
                public static Image stationary2 = Resources.Inky_stationary_ver__2;
            }

            public static class Clyde
            {
                public static Image StartImage = Resources.Clyde_up;

                public static Image left = Resources.Clyde_left;
                public static Image right = Resources.Clyde_right;
                public static Image up = Resources.Clyde_up;
                public static Image down = Resources.Clyde_down;

                public static Image left2 = Resources.Clyde_left_ver__2;
                public static Image right2 = Resources.Clyde_right_ver__2;
                public static Image up2 = Resources.Clyde_up_ver__2;
                public static Image down2 = Resources.Clyde_down_ver__2;

                public static Image stationary = Resources.Clyde_stationary;
                public static Image stationary2 = Resources.Clyde_stationary_ver__2;
            }
        }

        public static class Blinky
        {
            public const int StartX = GameConstants.BoxSize * 14;
            public const int StartY = GameConstants.BoxSize * 16;

            public const Direction StartDirection = Direction.Left;
            public const MapCorner ScatterCorner = MapCorner.TopRight;
        }

        public static class Pinky
        {
            public const int StartX = GameConstants.BoxSize * 14;
            public const int StartY = GameConstants.BoxSize * 21;

            public const Direction StartDirection = Direction.Down;
            public const MapCorner ScatterCorner = MapCorner.TopLeft;
        }

       public static class Inky
        {
            public const int StartX = GameConstants.BoxSize * 12;
            public const int StartY = GameConstants.BoxSize * 20;

            public const Direction StartDirection = Direction.Up;
            public const MapCorner ScatterCorner = MapCorner.BottomRight;
        }

        public static class Clyde
        {
            public const int StartX = GameConstants.BoxSize * 16;
            public const int StartY = GameConstants.BoxSize * 20;

            public const Direction StartDirection = Direction.Up;
            public const MapCorner ScatterCorner = MapCorner.BottomLeft;

            public const int BevahiourOverrideScatterTime = StandardInterval * 25;
            // Clyde will switch to scatter mode if within this distance of Pacman
            public const int BehaviourOverrideDistance = 8; // In boxes
        }
    }
}
