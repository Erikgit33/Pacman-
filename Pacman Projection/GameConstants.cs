using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman_Projection
{
    /// <summary>
    /// Contains constants for the game, such as sizes, offsets, scores, and event times.
    /// </summary>
    public static class GameConstants
    {
        public const int step = 14; 
        public const int boxSize = step;
        public const int entitySize = boxSize * 2;

        public const int maxStepsIntoTeleporter = 3;

        public const int boxes_Horizontally = 30;
        public const int boxes_Vertically = 39;

        public const int food_Horizontally = 29;
        public const int food_Vertically = 37;

        public const int boxOffset_Vertical = boxSize * 2;

        public const int foodOffset_Horizontal = boxSize + boxSize / 2;
        public const int foodOffset_Vertical = boxSize * 3 + boxSize / 2;

        public static List<int[]> powerPelletIndexes = new List<int[]>
        {
            new int[] { 0, 0 }, 
            new int[] { 26, 0 }, 
            new int[] { 0, 34 }, 
            new int[] { 26, 34 } 
        };

        /// <summary>
        /// Contains the scores for different game elements.
        /// </summary>
        internal class Scores
        {
            /// <summary>
            /// Contains the scores for each type of fruit. 
            /// </summary>
            public static Dictionary<Fruit, int> fruitScore = new Dictionary<Fruit, int>
            { 
                { Fruit.Cherry, 200 },
                { Fruit.Strawberry, 500 },
                { Fruit.Apple, 800 },
                { Fruit.Banana, 1200 },
                { Fruit.Melon, 1500 }
            };

            public const int food = 10;
            public const int powerPellet = 50;
            public const int ghost = 200;
        }


        /// <summary>
        /// Contains the times for various game events in milliseconds.
        /// </summary>
        internal class EventTimes
        {
            public const int powerPellet = 11000;
            public const int perDeathSequence = 160;
            public const int afterGhostsAppear = 2800;
            public const int betweenGames = 1500;
            public const int beforeRestart = 2500;
            public const int afterDeath = 800;
            public const int afterGhostEaten = 1000;
            public const int wallBlink = 220;
            public const int gameOverDisplayed = 2000;

            public const int buttonDelay = 200;
        }
    }
}
