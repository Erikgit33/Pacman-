using Pacman_Projection.Properties;
using System;
using System.Collections.Generic;
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
        public const int StandardInterval = 180;

        /// <summary>
        /// Contains the speed of the ghosts for each level when they are scared, in milliseconds.
        /// Key is the level number, value is the speed (interval) in milliseconds.
        /// </summary>
        public static Dictionary<int, int> SpeedForLevel_Frightened = new Dictionary<int, int>
        {
            {1, StandardInterval + 5*24}, // 300ms
            {2, StandardInterval + 5*23},
            {3, StandardInterval + 5*22},
            {4, StandardInterval + 5*21},
            {5, StandardInterval + 5*20},
            {6, StandardInterval + 5*19},
            {7, StandardInterval + 5*18},
            {8, StandardInterval + 5*17},
            {9, StandardInterval + 5*16},
            {10, StandardInterval + 5*15} // 255ms
        };

        /// <summary>
        /// Contains the speed of the ghosts for each level, in milliseconds.
        /// Key is the level number, value is the speed (interval) in milliseconds.
        /// </summary>
        public static Dictionary<int, int> SpeedForLevel = new Dictionary<int, int>
        {
            {1, StandardInterval}, // 180ms
            {2, StandardInterval - 3},
            {3, StandardInterval - 3*2},
            {4, StandardInterval - 3*3},
            {5, StandardInterval - 3*4},
            {6, StandardInterval - 3*5},
            {7, StandardInterval - 3*6},
            {8, StandardInterval - 3*7},
            {9, StandardInterval - 3*8},
            {10, StandardInterval - 3*9} // 153ms
        };

        public const int blinkDuration = 250;
        public const int timesToBlink = 6;

        /// <summary>
        /// Contains the time for ghosts to scatter and chase in the format "scatter,chase" for each level, in whole seconds.
        /// Key is the level number, value is a string which contains each time seperated by a comma.
        /// </summary>
        public static Dictionary<int, string> ScatterChaseTimesForLevel = new Dictionary<int, string>
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
                public static Image left = Resources.Blinky_left;
                public static Image right = Resources.Blinky_left;
                public static Image up = Resources.Blinky_left;
                public static Image down = Resources.Blinky_left;
                
                public static Image left2 = Resources.Blinky_left_ver__2;
                public static Image right2 = Resources.Blinky_right_ver__2;
                public static Image up2 = Resources.Blinky_up_ver__2;
                public static Image down2 = Resources.Blinky_down_ver__2;

                public static Image stationary = Resources.Blinky_stationary;
                public static Image stationary2 = Resources.Blinky_stationary_ver__2;
            }

            public static class Pinky
            {
                public static Image left = Resources.Pinky_left;
                public static Image right = Resources.Pinky_left;
                public static Image up = Resources.Pinky_left;
                public static Image down = Resources.Pinky_left;

                public static Image left2 = Resources.Pinky_left_ver__2;
                public static Image right2 = Resources.Pinky_right_ver__2;
                public static Image up2 = Resources.Pinky_up_ver__2;
                public static Image down2 = Resources.Pinky_down_ver__2;

                public static Image stationary = Resources.Pinky_stationary;
                public static Image stationary2 = Resources.Pinky_stationary_ver__2;
            }

            public static class Inky
            {
                public static Image left = Resources.Inky_left;
                public static Image right = Resources.Inky_left;
                public static Image up = Resources.Inky_left;
                public static Image down = Resources.Inky_left;

                public static Image left2 = Resources.Inky_left_ver__2;
                public static Image right2 = Resources.Inky_right_ver__2;
                public static Image up2 = Resources.Inky_up_ver__2;
                public static Image down2 = Resources.Inky_down_ver__2;

                public static Image stationary = Resources.Inky_stationary;
                public static Image stationary2 = Resources.Inky_stationary_ver__2;
            }

            public static class Clyde
            {
                public static Image left = Resources.Clyde_left;
                public static Image right = Resources.Clyde_left;
                public static Image up = Resources.Clyde_left;
                public static Image down = Resources.Clyde_left;

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
            public const int StartX = GameConstants.boxSize * 14;
            public const int StartY = GameConstants.boxSize * 16;

            public const Direction StartDirection = Direction.Left;
            public static Image StartImage = Resources.Blinky_left;
        }

        public static class Pinky
        {
            public const int StartX = GameConstants.boxSize * 14;
            public const int StartY = GameConstants.boxSize * 21;

            public const Direction StartDirection = Direction.Down;
            public static Image StartImage = Resources.Pinky_down;
        }

       public static class Inky
        {
            public const int StartX = GameConstants.boxSize * 12;
            public const int StartY = GameConstants.boxSize * 20;

            public const Direction StartDirection = Direction.Up;
            public static Image StartImage = Resources.Inky_up;
        }

        public static class Clyde
        {
            public const int StartX = GameConstants.boxSize * 16;
            public const int StartY = GameConstants.boxSize * 20;

            public const Direction StartDirection = Direction.Up;
            public static Image StartImage = Resources.Clyde_up;
        }
    }
}
