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
    /// Contains constants for Pacman, such as starting position, speed for each level, and death sequence images.
    /// </summary>
    public static class PacConstants
    {
        public const int StartX = GameConstants.boxSize * 14;
        public const int StartY = GameConstants.boxSize * 23 + GameConstants.boxOffset_Vertical;

        public const Direction StartDirection = Direction.Stationary;

        const int pacTickTimerIntervalStandard = 190;

        /// <summary>
        /// Contains the speed of Pacman for each level, in milliseconds.
        /// Key is the level number, value is the speed (interval) in milliseconds.
        /// </summary>
        public static Dictionary<int, int> SpeedForLevel = new Dictionary<int, int>
        {
            {1, pacTickTimerIntervalStandard}, // 190ms
            {2, pacTickTimerIntervalStandard - 2},
            {3, pacTickTimerIntervalStandard - 2*2},
            {4, pacTickTimerIntervalStandard - 2*3},
            {5, pacTickTimerIntervalStandard - 2*4},
            {6, pacTickTimerIntervalStandard - 2*5},
            {7, pacTickTimerIntervalStandard - 2*6},
            {8, pacTickTimerIntervalStandard - 2*7},
            {9, pacTickTimerIntervalStandard - 2*8},
            {10, pacTickTimerIntervalStandard - 2*9} // 172ms
        };

        public static List<Image> deathSequence = new List<Image>
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
    }
}
