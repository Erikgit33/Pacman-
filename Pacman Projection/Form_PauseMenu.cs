using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacman_Projection
{
    public partial class Form_PauseMenu : Form
    {
        FormManager formManager;
        EventManager eventManager;
        GlobalVariables globalVariables;

        Button button_ResumeGame;
        Button button_ExitGame;
        Label label_Name;
        Label label_Score;
        internal bool saveScore { get; private set; } = false; // Default to not saving the score

        public Form_PauseMenu(FormManager formManager, EventManager eventManager, GlobalVariables globalVariables)
        {
            InitializeComponent();

            ClientSize = new Size(250, 150);
            this.Location = new Point(GameConstants.FormXOffset + GameConstants.FormXOffset / 2 - this.Width / 2, GameConstants.FormYOffset + GameConstants.FormHeight / 2 - ClientSize.Height / 2);
            this.Text = "Pause Menu";
            this.formManager = formManager;
            this.eventManager = eventManager;
            this.globalVariables = globalVariables;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true;
        }

        private void Form_PauseMenu_Load(object sender, EventArgs e)
        {
            // button_ResumeGame properties
            button_ResumeGame = new Button
            {
                Text = "Resume Game",
                Location = new Point(50, 50),
                Size = new Size(150, 30),
            };
            Controls.Add(button_ResumeGame);
            button_ResumeGame.Click += button_resumeGame_Click;

            button_ResumeGame.MouseEnter += (s, args) =>
            {
                Cursor = Cursors.Hand;
            };

            button_ResumeGame.MouseLeave += (s, args) =>
            {
                Cursor = Cursors.Default;
            };

            // button_ExitGame properties
            button_ExitGame = new Button
            {
                Text = "Exit Game",
                Location = new Point(50, 100),
                Size = new Size(150, 30)
            };
            Controls.Add(button_ExitGame);
            button_ExitGame.Click += button_exitGame_Click;

            button_ExitGame.MouseEnter += (s, args) =>
            {
                Cursor = Cursors.Hand;
            };

            button_ExitGame.MouseLeave += (s, args) =>
            {
                Cursor = Cursors.Default;
            };

            // label_Name properties
            label_Name = new Label
            {
                Text = $"{globalVariables.PlayerName}",
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(120, 10),
                Size = new Size(120, 20),
                ForeColor = Color.Black,
                Font = new Font("Arial", 8, FontStyle.Bold)
            };
            Controls.Add(label_Name);

            // label_Score properties
            label_Score = new Label
            {
                Text = $"Current score: {globalVariables.Score}",
                Location = new Point(10, 10),
                Size = new Size(150, 20),
                ForeColor = Color.Black,
                Font = new Font("Arial", 8, FontStyle.Bold)
            };
            Controls.Add(label_Score);
        }

        private void button_resumeGame_Click(object sender, EventArgs e)
        {
            ClosePauseMenu();
        }

        private void button_exitGame_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to save your score?", "Exit", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            switch (result)
            {
                case DialogResult.Yes:
                    // Set the end level to the current level before saving
                    globalVariables.EndLevel = globalVariables.CurrentLevel;

                    // Save the score to JSON with all relevant details
                    formManager.SaveScoreToJson(globalVariables.PlayerName, globalVariables.StartLevel, 
                                                globalVariables.EndLevel, globalVariables.Score, 
                                                globalVariables.FruitEaten, globalVariables.GhostsEaten, 
                                                globalVariables.HighestGhostCombo, globalVariables.Ghosts.Count);

                    formManager.CloseForm(formManager.form_Main);

                    // Clear the name in globalVariables for next time
                    globalVariables.PlayerName = string.Empty;

                    formManager.SwitchToForm(this, formManager.form_Menu);
                    break;
                case DialogResult.No:
                    formManager.CloseForm(formManager.form_Main);
                    formManager.SwitchToForm(this, formManager.form_Menu);
                    break;
                case DialogResult.Cancel:
                    return; // Do not exit the game
            }
        }

        private void Form_PauseMenu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                ClosePauseMenu();
            }
        }

        private void Form_PauseMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; // Don't dispose the form
            ClosePauseMenu();
        }

        private void ClosePauseMenu()
        {
            formManager.CloseForm(this);
            formManager.form_Main.UnpauseGame();
        }
    }
}
