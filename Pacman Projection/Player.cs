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
        public string Name { get; private set; }
        public int StartLevel { get; private set; }
        public int EndLevel { get; private set; }
        public int Score { get; private set; }
        public int FruitEaten { get; private set; }
        public int GhostsEaten { get; private set; }
        public int HighestGhostCombo { get; private set; }
        public int GhostCount { get; private set; }

        public Player (string name, int startLevel, int endLevel, int score, int fruitEaten, int ghostsEaten, int highestGhostCombo, int ghostCount)
        {
            Name = name;
            StartLevel = startLevel;
            EndLevel = endLevel;
            Score = score;
            FruitEaten = fruitEaten;
            GhostsEaten = ghostsEaten;
            HighestGhostCombo = highestGhostCombo;
            GhostCount = ghostCount;
        }
    }
}
