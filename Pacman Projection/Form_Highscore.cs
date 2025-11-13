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

        Label label_HighscoreLabel;
        Label label_Highscore;

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
            ClientSize = new Size(GameConstants.BoxSize * 30, GameConstants.BoxSize * 38);
            this.Location = new Point(388, 85);
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create the highscoreLabel label
            label_HighscoreLabel = new Label()
            {
                Location = new Point(10, 10),
                Size = new Size(GameConstants.BoxSize * 30, GameConstants.BoxSize * 6),
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White
            };
            Controls.Add(label_HighscoreLabel);

            // Create the highscore label   
            label_Highscore = new Label()
            {
                Location = new Point(10, GameConstants.BoxSize * 7),
                Size = new Size(GameConstants.BoxSize * 30, GameConstants.BoxSize * 30),
                Font = new Font("Arial", 9, FontStyle.Regular),
                ForeColor = Color.Yellow,
                AutoScrollOffset = new Point(0, 0)
            };
            Controls.Add(label_Highscore);

            if (File.Exists("highscores.json"))
            {
                string savedJson = File.ReadAllText("highscores.json");
                List<Player> playerEntries = JsonSerializer.Deserialize<List<Player>>(savedJson);

                label_HighscoreLabel.Text = "HIGHSCORES\n\n";
                label_HighscoreLabel.Text += "RANK, STATS, START/ENDLEVEL\n";
                label_HighscoreLabel.Text += "----------------------------------------------------------------------------\n";

                UpdateHighscoreLabel(playerEntries);
            }
            else
            {
                label_Highscore.Text = "No saved highscores!";
            }
        }

        public void UpdateHighscoreLabel()
        {
            if (File.Exists("highscores.json"))
            {
                string savedJson = File.ReadAllText("highscores.json");
                List<Player> playerEntries = JsonSerializer.Deserialize<List<Player>>(savedJson);

                label_HighscoreLabel.Text = "HIGHSCORES\n\n";
                label_HighscoreLabel.Text += "RANK, STATS, START/ENDLEVEL\n";
                label_HighscoreLabel.Text += "----------------------------------------------------------------------------\n";

                UpdateHighscoreLabel(playerEntries);
            }
            else
            {
                label_Highscore.Text = "No saved highscores!";
            }
        }
        private void UpdateHighscoreLabel(List<Player> playerEntries)
        {
            // Reset text
            label_Highscore.Text = string.Empty;
            
            // Remove all IndexButtons
            foreach (object obj in Controls)
            {
                if (obj is IndexButton)
                {
                    IndexButton indexButton = (IndexButton)obj;
                    Controls.Remove(indexButton);
                    indexButton.Dispose();
                }
            }

            List<Player> sortedPlayerEntries = playerEntries.OrderByDescending(p => p.Score).ToList();

            if (sortedPlayerEntries.Count > 0)
            {
                foreach (Player playerEntry in sortedPlayerEntries)
                {
                    label_Highscore.Text += (sortedPlayerEntries.IndexOf(playerEntry) + 1).ToString() + ". "
                        + GetPlayerEntryString(playerEntry)
                        + Environment.NewLine
                        + Environment.NewLine;

                    var indexButton = new IndexButton(sortedPlayerEntries.IndexOf(playerEntry));
                    indexButton.Size = new Size(22, 22);
                    indexButton.Location = new Point(GameConstants.BoxSize * 26 + GameConstants.BoxSize / 2, 95 + (Height + 8) * sortedPlayerEntries.IndexOf(playerEntry));
                    indexButton.Image = Resources.Trashcan;
                    indexButton.ImageAlign = ContentAlignment.MiddleCenter;    
                    indexButton.Click += IndexButton_Click;
                    Controls.Add(indexButton);
                    indexButton.BringToFront();
                }
            }
            else
            {
                label_Highscore.Text = "No saved highscores!";
            }
        }

        private List<Player> GetSortedPlayerEntries()
        {
            var playerEntries = JsonSerializer.Deserialize<List<Player>>(File.ReadAllText("highscores.json"));
            return playerEntries.OrderByDescending(p => p.Score).ToList();
        }

        private string GetPlayerEntryString(Player playerEntry)
        {
            // [Name], Highscore: [Score], Fruit: [FruitEaten], Ghosts: [GhostsEaten], Combo: [HighestGhostCombo], ([StartLevel]/[EndLevel])
            return $"{playerEntry.Name} - Highscore: "
                + $"{playerEntry.Score}, "
                + $"Fruit: {playerEntry.FruitEaten}, "
                + $"Ghosts: {playerEntry.GhostsEaten}, "
                + $"Combo: {playerEntry.HighestGhostCombo}, "
                + $"({playerEntry.StartLevel}/{playerEntry.EndLevel})";
        }

        private void IndexButton_Click(object sender, EventArgs e)
        {
            IndexButton indexButton = sender as IndexButton;

            var playerEntry = GetSortedPlayerEntries()[indexButton.Index];

            var result = MessageBox.Show($"Are you sure you want to delete the entry by '{playerEntry.Name}', '{GetPlayerEntryString(playerEntry)}'?", "Delete Entry", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            switch (result)
            {
                case DialogResult.Yes:
                    if (indexButton != null)
                    {
                        int index = indexButton.Index;

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

        private void Form_Highscore_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == true)
            {
                UpdateHighscoreLabel();
            }
        }
    }
}
