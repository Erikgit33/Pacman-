using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Deployment.Application;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Pacman_Projection
{  
    public class FormManager
    {
        GlobalVariables globalVariables;
        EventManager eventManager = new EventManager();

        public Form_Menu form_Menu { get; private set; }
        public Form_Highscore form_Highscore { get; private set; }
        public Form_Ghosts form_Ghosts { get; private set; }
        public Form_Main form_Main { get; private set; }
        public Form_PauseMenu form_PauseMenu { get; private set; }

        public FormManager() 
        { 
            // Get all player names to check for duplicates when choosing a name
            if (File.Exists("highscores.json"))
            {
                string savedJson = File.ReadAllText("highscores.json"); 
                List<Player> playerEntries = JsonSerializer.Deserialize<List<Player>>(savedJson);
                List<string> playerNames = playerEntries.Select(p => p.Name).ToList();

                globalVariables = new GlobalVariables(playerNames);
            }
            else
            {
                globalVariables = new GlobalVariables();
            }

            form_Menu = new Form_Menu(this, eventManager, globalVariables);
            form_Highscore = new Form_Highscore(this, eventManager, globalVariables);
            form_Ghosts = new Form_Ghosts(this, eventManager, globalVariables);
            form_Main = new Form_Main(this, eventManager, globalVariables);
            form_PauseMenu = new Form_PauseMenu(this, eventManager, globalVariables);
        }

        public void SwitchToForm(Form formToClose, Form formToOpen)
        {
            // Only dispose of form_Main, hide all other forms 
            if (formToClose.Equals(form_Main))
            {
                formToClose.Dispose();
                form_Main = new Form_Main(this, eventManager, globalVariables);
            }
            else // Only close (dispose) FormMain
            {
                formToClose.Hide();
            }

            OpenForm(formToOpen);
        }

        public void OpenForm(Form formToOpen)
        {
            if (formToOpen != null) 
            {
                // All other forms
                formToOpen.Show();
            }
            else
            {
                // form_Main
                form_Main = new Form_Main(this, eventManager, globalVariables);
                form_Main.Show();
            }
        }

        public void CloseForm(Form formToClose)
        {
            if (formToClose.Equals(form_Main))
            {
                form_Main.DisposeForm();
                //formToClose.Dispose();
                form_Main = new Form_Main(this, eventManager, globalVariables);
            }
            else
            {
                formToClose.Hide();
            }
        }

        public void SaveScoreToJson(string playerName, int startLevel, int endLevel, int score, int fruitEaten, int ghostsEaten, int highestGhostCombo, int ghostCount)
        {
            Player player = new Player(playerName, startLevel, endLevel, score, fruitEaten, ghostsEaten, highestGhostCombo, ghostCount);  
            
            if (File.Exists("highscores.json"))
            {
                string savedJson = File.ReadAllText("highscores.json");
                List<Player> playerEntries = JsonSerializer.Deserialize<List<Player>>(savedJson);

                playerEntries.Add(player);

                string jsonToSave = JsonSerializer.Serialize(playerEntries);
                File.WriteAllText("highscores.json", jsonToSave);
            }
            else
            {
                List<Player> scoresToSave = new List<Player> { player };
                string jsonToSave = JsonSerializer.Serialize(scoresToSave);
                File.WriteAllText("highscores.json", jsonToSave);
            }
        }

        internal void ExitGame()
        {
            Application.Exit();
        }
    }
}
