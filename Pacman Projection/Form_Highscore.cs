using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.CodeDom;
using System.Data.SqlClient;
using System.Deployment.Application;
using System.Security.Cryptography;
using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using Pacman_Projection.Properties;

namespace Pacman_Projection
{
    public partial class Form_Highscore : Form
    {
        FormManager formManager;
        EventManager eventManager;
        GlobalVariables GlobalVariables;

        Label labelHighscore;

        public Form_Highscore(FormManager formManager, EventManager eventManager, GlobalVariables globalVariables)
        {
            InitializeComponent();
            this.formManager = formManager;
            this.eventManager = eventManager;
            this.GlobalVariables = globalVariables;
        }

        private void Form_Highscore_Load(object sender, EventArgs e)
        {
            // Set form size to fit projector
            ClientSize = new Size(GameConstants.boxSize * 30, GameConstants.boxSize * 38);
            this.Location = new Point(388, 85);
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create the highscoreLabel label
            Label labelHighscoreLabel = new Label()
            {
                Location = new Point(10, 10),
                Size = new Size(GameConstants.boxSize * 30, GameConstants.boxSize * 6),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            Controls.Add(labelHighscoreLabel);

            // Create the highscore label   
            labelHighscore = new Label()
            {
                Location = new Point(10, GameConstants.boxSize * 7),
                Size = new Size(GameConstants.boxSize * 30, GameConstants.boxSize * 30),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.Yellow,
                AutoScrollOffset = new Point(0, 0)
            };
            Controls.Add(labelHighscore);

            if (File.Exists("highscores.json"))
            {
                string savedJson = File.ReadAllText("highscores.json");
                List<Player> playerEntries = JsonSerializer.Deserialize<List<Player>>(savedJson);

                labelHighscoreLabel.Text = "HIGHSCORES\n\n";
                labelHighscoreLabel.Text += "RANK, STATS, START/ENDLEVEL\n";
                labelHighscoreLabel.Text += "----------------------------------------------------------------------------\n";

                UpdateHighscoreLabel(playerEntries);
            }
            else
            {
                labelHighscore.Text = "No saved highscores!";
            }
        }

        private void UpdateHighscoreLabel(List<Player> playerEntries)
        {
            // Reset text
            labelHighscore.Text = string.Empty;
            
            // Remove all previous indexButtons
            foreach (object obj in Controls)
            {
                if (obj is IndexButton)
                {
                    IndexButton indexButton = obj as IndexButton;
                    Controls.Remove(indexButton);
                    indexButton.Dispose();
                }
            }

            List<Player> sortedPlayerEntries = playerEntries.OrderByDescending(p => p.Score).ToList();

            if (sortedPlayerEntries.Count > 0)
            {
                foreach (Player playerEntry in sortedPlayerEntries)
                {
                    labelHighscore.Text += (sortedPlayerEntries.IndexOf(playerEntry) + 1).ToString() + ". "
                        + GetPlayerEntryString(playerEntry)
                        + Environment.NewLine
                        + Environment.NewLine;

                    var indexButton = new IndexButton(sortedPlayerEntries.IndexOf(playerEntry));
                    indexButton.Size = new Size(25,25);
                    indexButton.Location = new Point(GameConstants.boxSize * 28, 95 + (indexButton.Height + 7) * sortedPlayerEntries.IndexOf(playerEntry));
                    indexButton.Image = Resources.Trashcan;
                    indexButton.Click += indexButton_Click;
                    Controls.Add(indexButton);
                    indexButton.BringToFront();
                }
            }
            else
            {
                labelHighscore.Text = "No saved highscores!";
            }
        }

        private List<Player> GetSavedPlayers()
        {
            return JsonSerializer.Deserialize<List<Player>>(File.ReadAllText("highscores.json"));
        }

        private string GetPlayerEntryString(Player playerEntry)
        {
            // [Name], Highscore: [Score], Fruit: [FruitEaten], Ghosts: [GhostsEaten], Combo: [HighestGhostCombo] ([StartLevel]/[EndLevel])
            return $"{playerEntry.Name}, Highscore: "
                + $"{playerEntry.Score},"
                + $"Fruit: {playerEntry.FruitEaten}, "
                + $"Ghosts: {playerEntry.GhostsEaten}, "
                + $"Combo: {playerEntry.HighestGhostCombo}, "
                + $"({playerEntry.StartLevel}/{playerEntry.EndLevel})";
        }

        private void indexButton_Click(object sender, EventArgs e)
        {
            IndexButton indexButton = sender as IndexButton;

            var result = MessageBox.Show($"Are you sure you want to delete the entry '{GetPlayerEntryString(GetSavedPlayers()[indexButton.index])}'?", "Delete Entry", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            switch (result)
            {
                case DialogResult.Yes:
                    if (indexButton != null)
                    {
                        int index = indexButton.index;

                        if (File.Exists("highscores.json"))
                        {
                            // Get the saved player entries
                            string savedJson = File.ReadAllText("highscores.json");
                            List<Player> playerEntries = JsonSerializer.Deserialize<List<Player>>(savedJson);

                            // Remove specified entry and update the label
                            playerEntries.Remove(playerEntries[index]);
                            UpdateHighscoreLabel(playerEntries);

                            // Save to json with the entry now removed
                            string jsonToSave = JsonSerializer.Serialize(playerEntries);
                            File.WriteAllText("highscores.json", jsonToSave);
                        }
                    }
                    return;
                case DialogResult.No:
                    return;
            }
        }

        private void Form_Highscore_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            formManager.CloseForm(this);
        }
    }
}
