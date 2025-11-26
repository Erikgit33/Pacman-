using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
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
        public const int FormWidth = Boxes_Horizontally * BoxSize;
        public const int FormHeight = Boxes_Vertically * BoxSize;

        public const int FormXOffset = 388;
        public const int FormYOffset = 85;

        public const int Step = 14;
        public const int BoxSize = Step;
        public const int EntitySize = BoxSize * 2;

        public static int[] TeleporterLeftIndex = { 0, 19 };
        public static int[] TeleporterRightIndex = { Boxes_Horizontally - 1, 19 };

        public static Color Color_Background = Color.Black;
        public static Color Color_Wall = Color.Blue;
        public static Color Color_Gate = Color.LightPink;

        /// <summary>
        /// The amount of steps an entity can take into a teleporter before being teleported out the other side.
        /// Also used when exiting a teleporter: when reaching zero, the teleporter has been fully exited.
        /// </summary>
        public const int MaxStepsIntoTeleporter = 3;

        /// <summary>
        /// "Boxes" refers to the grid units that make up the game map (none, wall teleporter etc.).
        /// </summary>
        public const int Boxes_Horizontally = 30;
        /// <summary>
        /// "Boxes" refers to the grid units that make up the game map (none, wall teleporter etc.).
        /// </summary>
        public const int Boxes_Vertically = 39;

        /// <summary>
        /// "gameBoxes" refers to the grid overlapping the game map, used for positioning, movement calculations and food.
        /// </summary>
        public const int GameBoxes_Horizontally = 29;
        /// <summary>
        /// "gameBoxes" refers to the grid overlapping the game map, used for positioning, movement calculations and food.
        /// </summary>
        public const int GameBoxes_Vertically = 36;

        /// <summary>
        /// Vertical offset that applies to the box-grid.
        /// </summary>
        public const int BoxOffset_Vertical = BoxSize * 2;

        /// <summary>
        /// Horizontal offset that applies to the gameBoxes-grid only.
        /// </summary>
        public const int GameGridOffset_Horizontal = BoxSize / 2;
        /// <summary>
        /// Vertical offset that applies to the gameBoxes-grid only.
        /// </summary>
        public const int GameGridOffset_Vertical = BoxSize * 3 + BoxSize / 2;

        public static List<int[]> PowerPelletIndexes = new List<int[]>
        {
            new int[] { 1, 0 }, 
            new int[] { 27, 0 }, 
            new int[] { 1, 34 }, 
            new int[] { 27, 34 } 
        };

        /// <summary>
        /// The index for the top-most, left-most box in the ghost house.
        /// </summary>
        public static int[] GhostHouse_TopLeftIndex = { 12, 17 };
        /// <summary>
        /// The index for the bottom-most, right-most box in the ghost house.
        /// </summary>
        public static int[] GhostHouse_BottomRightIndex = { 17, 21 };

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
            public const int powerPellet = 7000;
            public const int perDeathSequence = 160;
            public const int afterGhostsAppear = 2800;
            public const int betweenGames = 1500;
            public const int beforeRestart = 2500;
            public const int afterDeath = 800;
            public const int afterGhostEaten = 1000;
            public const int wallBlink = 220;
            public const int gameOverDisplayed = 2000;

            public const int buttonDelay = 30;
            public const int mapSelectGridDelay = 150;
        }
    }
}
