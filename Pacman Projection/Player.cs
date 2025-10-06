using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman_Projection
{ 
    internal class Player
    {
        public string Name { get; set; }
        public int StartLevel { get; set; }
        public int EndLevel { get; set; }
        public int Score { get; set; }
        public int FruitEaten { get; set; }
        public int GhostsEaten { get; set; }
        public int HighestGhostCombo { get; set; }

        public Player (string name, int startLevel, int endLevel, int score, int fruitEaten, int ghostsEaten, int highestGhostCombo)
        {
            Name = name;
            StartLevel = startLevel;
            EndLevel = endLevel;
            Score = score;
            FruitEaten = fruitEaten;
            GhostsEaten = ghostsEaten;
            HighestGhostCombo = highestGhostCombo;
        }
    }
}
