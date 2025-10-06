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
    public enum Forms
    {
        FormMenu,
        FormHighscore,
        FormMain,
        FormaPauseMenu
    }

    public class FormManager
    {
        GlobalVariables globalVariables;
        EventManager eventManager = new EventManager();

        public Form_Menu FormMenu { get; private set; }
        public Form_Highscore FormHighscore { get; private set; }
        public Form_Main FormMain { get; private set; }
        public Form_PauseMenu FormPauseMenu { get; private set; }

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

            FormMenu = new Form_Menu(this, eventManager, globalVariables);
            FormHighscore = new Form_Highscore(this, eventManager, globalVariables);
            FormMain = new Form_Main(this, eventManager, globalVariables);
            FormPauseMenu = new Form_PauseMenu(this, eventManager, globalVariables);
        }

        public void SwitchToForm(Form formToClose, Form formToOpen)
        {
            // Only dispose of (to create new later) FormMain and FormHighscore to reset their states 
            if (formToClose.Equals(FormMain))
            {
                formToClose.Dispose();
                FormMain = null;
            }
            else // Only close (dispose) FormMain
            {
                formToClose.Hide();
            }

            OpenForm(formToOpen);
        }

        public void OpenForm(Form formToOpen)
        {
            if (formToOpen != null) // Only FormMain is set to null after closing
            {
                formToOpen.Show();
            }
            else
            {
                FormMain = new Form_Main(this, eventManager, globalVariables);
                FormMain.Show();
            }
        }

        public void CloseForm(Form formToClose)
        {
            formToClose.Hide();
        }

        public void SaveScoreToJson(string playerName, int startLevel, int endLevel, int score, int fruitEaten, int ghostsEaten, int highestGhostCombo)
        {
            Player player = new Player(playerName, startLevel, endLevel, score, fruitEaten, ghostsEaten, highestGhostCombo);  
            
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
    }
}
