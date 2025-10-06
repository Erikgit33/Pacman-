using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman_Projection
{ 
    public class GlobalVariables
    {
        public List<string> NameList { get; private set; }
        public string PlayerName { get;  set; }
        public int StartLevel { get; set; }
        public int CurrentLevel { get; set; }
        public int EndLevel { get; set; }   
        public int Score { get; set; }
        public int FruitEaten { get; set; }
        public int GhostsEaten { get; set; }
        public int HighestGhostCombo { get; set; }

        public GlobalVariables(List<string> nameList) 
        { 
            NameList = new List<string>(nameList); 
        }
    }
}
