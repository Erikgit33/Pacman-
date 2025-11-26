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
        List<Label> highscoreLabels = new List<Label>();

        Label label_NoHighscores;

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
            ClientSize = new Size(GameConstants.BoxSize * 60, GameConstants.FormHeight);
            this.Location = new Point(GameConstants.FormXOffset - (this.Width - GameConstants.FormWidth) / 2, GameConstants.FormYOffset);
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // label_HighscoreLabel properties
            label_HighscoreLabel = new Label()
            {
                Location = new Point(0, GameConstants.BoxSize),
                Size = new Size(this.Width, GameConstants.BoxSize * 6),
                Font = new Font("Arial", 20, FontStyle.Bold),
                ForeColor = Color.Yellow,
                Text = "HIGHSCORES \n ----------------------------------------------------------------------------\n",
                TextAlign = ContentAlignment.TopCenter
            };
            Controls.Add(label_HighscoreLabel);

            // label_NoHighscores properties    
            label_NoHighscores = new Label()
            {
                Location = new Point(0, GameConstants.BoxSize * 8),
                Size = new Size(this.Width, GameConstants.BoxSize * 8),
                Font = new Font("Arial", 16, FontStyle.Regular),
                ForeColor = Color.White,
                Text = "No saved highscores!",
                TextAlign = ContentAlignment.TopCenter
            };
            Controls.Add(label_NoHighscores);
            label_NoHighscores.BringToFront();
            label_NoHighscores.Hide();

            if (File.Exists("highscores.json"))
            {
                string savedJson = File.ReadAllText("highscores.json");
                List<Player> playerEntries = JsonSerializer.Deserialize<List<Player>>(savedJson);

                UpdateHighscoreLabel(playerEntries);
            }
            else
            {
                label_NoHighscores.Show();   
            }
        }
        private void UpdateHighscoreLabel(List<Player> playerEntries)
        {
            // Remove all existing highscore labels
            int index = highscoreLabels.Count - 1;
            while (index >= 0)
            {
                Controls.Remove(highscoreLabels[index]);
                highscoreLabels[index].Dispose();
                highscoreLabels.RemoveAt(index);
                index--;
            }

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
                    highscoreLabels.Add(new Label()
                    {
                        Location = new Point(10, GameConstants.BoxSize * 7 + sortedPlayerEntries.IndexOf(playerEntry) * GameConstants.BoxSize * 2 + 5),
                        Size = new Size(this.Width - GameConstants.BoxSize * 7, GameConstants.BoxSize * 2),
                        Font = new Font("Arial", 9, FontStyle.Regular),
                        Text = (sortedPlayerEntries.IndexOf(playerEntry) + 1).ToString() + ". "
                        + GetPlayerEntryString(playerEntry),
                        ForeColor = Color.Yellow,
                    });
                    Controls.Add(highscoreLabels[sortedPlayerEntries.IndexOf(playerEntry)]);
                    highscoreLabels[sortedPlayerEntries.IndexOf(playerEntry)].BringToFront();

                    var label = highscoreLabels[sortedPlayerEntries.IndexOf(playerEntry)];

                    var indexButton = new IndexButton(sortedPlayerEntries.IndexOf(playerEntry)) 
                    {
                        Size = new Size(26, 26),
                        Location = new Point(label.Location.X + label.Width + GameConstants.BoxSize, label.Location.Y - 26 / 4),
                        Image = Resources.Trashcan,
                        ImageAlign = ContentAlignment.MiddleCenter,
                    };    
                    indexButton.Click += IndexButton_Click;
                    Controls.Add(indexButton);
                    indexButton.BringToFront();
                }
            }
            else
            {
                label_NoHighscores.Show();
            }
        }

        private List<Player> GetSortedPlayerEntries()
        {
            var playerEntries = JsonSerializer.Deserialize<List<Player>>(File.ReadAllText("highscores.json"));
            return playerEntries.OrderByDescending(p => p.Score).ToList();
        }

        private string GetPlayerEntryString(Player playerEntry)
        {
            // [Name], Highscore: [Score], Fruit: [FruitEaten], Ghosts: [GhostsEaten], Combo: [HighestGhostCombo], Number of ghosts: [GhostCount], Started at level: [StartLevel] / Ended at leve: [EndLevel])
            return $"{playerEntry.Name} - Highscore: "
                + $"{playerEntry.Score}, "
                + $"Fruit eaten: {playerEntry.FruitEaten}, "
                + $"Ghosts eaten: {playerEntry.GhostsEaten}, "
                + $"Highest combo: {playerEntry.HighestGhostCombo}, "
                + $"Number of ghosts: {playerEntry.GhostCount}, "
                + $"(Started at level: {playerEntry.StartLevel} / Ended at level: {playerEntry.EndLevel})";
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
            eventManager.ButtonPress();

            e.Cancel = true;
            formManager.CloseForm(this);
        }

        private void Form_Highscore_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == true)
            {
                if (File.Exists("highscores.json"))
                {
                    string savedJson = File.ReadAllText("highscores.json");
                    List<Player> playerEntries = JsonSerializer.Deserialize<List<Player>>(savedJson);

                    UpdateHighscoreLabel(playerEntries);
                }
                else
                {
                    label_NoHighscores.Show();
                }
            }
        }
    }
}
