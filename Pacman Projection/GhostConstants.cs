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

        internal static class Blinky
        {
            public const int StartX = GameConstants.boxSize * 14;
            public const int StartY = GameConstants.boxSize * 16;

            public const Direction StartDirection = Direction.Left;
            public static Image StartImage = Resources.Blinky_left;
        }

        internal static class Pinky
        {
            public const int StartX = GameConstants.boxSize * 14;
            public const int StartY = GameConstants.boxSize * 21;

            public const Direction StartDirection = Direction.Down;
            public static Image StartImage = Resources.Pinky_down;
        }

        internal static class Inky
        {
            public const int StartX = GameConstants.boxSize * 12;
            public const int StartY = GameConstants.boxSize * 20;

            public const Direction StartDirection = Direction.Up;
            public static Image StartImage = Resources.Inky_up;
        }

        internal static class Clyde
        {
            public const int StartX = GameConstants.boxSize * 16;
            public const int StartY = GameConstants.boxSize * 20;

            public const Direction StartDirection = Direction.Up;
            public static Image StartImage = Resources.Clyde_up;
        }
    }
}
