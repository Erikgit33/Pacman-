using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman_Projection
{ 
    public class GlobalVariables
    {
        /// <summary>
        /// Contains all names currently in use.
        /// A player entry cannot be saved under the same name as another.
        /// </summary>
        public List<string> NameList { get; private set; }
        /// <summary>
        /// Contains all ghost object currently in use.
        /// </summary>
        internal List<Ghost> Ghosts { get; set; }
        /// <summary>
        /// The name of the player.
        /// </summary>
        public string PlayerName { get; set; }
        /// <summary>
        /// The level the player started at.
        /// </summary>
        public int StartLevel { get; set; }
        /// <summary>
        /// The current level.
        /// </summary>
        public int CurrentLevel { get; set; }
        /// <summary>
        /// The player the player died or quit at.
        /// </summary>
        public int EndLevel { get; set; } 
        /// <summary>
        /// The score.
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// How many fruits the player has eaten.
        /// </summary>
        public int FruitEaten { get; set; }
        /// <summary>
        /// How many ghosts the player has eaten.
        /// </summary>
        public int GhostsEaten { get; set; }
        /// <summary>
        /// The highest number of ghosts the player has eaten during a consecutive frightened mode.
        /// </summary>
        public int HighestGhostCombo { get; set; }

        public GlobalVariables(List<string> nameList) 
        { 
            NameList = new List<string>(nameList);
        }
    }
}
