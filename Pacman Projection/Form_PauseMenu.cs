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

        Button button_resumeGame;
        Button button_exitGame;
        Label label_name;
        Label label_score;

        internal bool saveScore { get; private set; } = false; // Default to not saving the score

        public Form_PauseMenu(FormManager formManager, EventManager eventManager, GlobalVariables globalVariables)
        {
            InitializeComponent();

            ClientSize = new Size(250, 200);
            this.Text = "Pause Menu";
            this.formManager = formManager;
            this.eventManager = eventManager;
            this.globalVariables = globalVariables;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void Form_PauseMenu_Load(object sender, EventArgs e)
        {
            button_resumeGame = new Button
            {
                Text = "Resume Game",
                Location = new Point(50, 50),
                Size = new Size(150, 30),
            };
            Controls.Add(button_resumeGame);
            button_resumeGame.Click += button_resumeGame_Click;

            button_exitGame = new Button
            {
                Text = "Exit Game",
                Location = new Point(50, 100),
                Size = new Size(150, 30)
            };
            Controls.Add(button_exitGame);
            button_exitGame.Click += button_exitGame_Click;

            this.Focus();
            this.KeyPreview = true;
        }

        private void button_resumeGame_Click(object sender, EventArgs e)
        {
            ClosePauseMenu();
        }

        private void button_exitGame_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you want to save your score?", "Exit", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
            switch (result)
            {
                case DialogResult.Yes:
                    globalVariables.EndLevel = globalVariables.CurrentLevel; // Set the end level to the current level before saving
                    formManager.SaveScoreToJson(globalVariables.PlayerName, globalVariables.StartLevel, globalVariables.EndLevel, globalVariables.Score, globalVariables.FruitEaten, globalVariables.GhostsEaten, globalVariables.HighestGhostCombo);
                    formManager.CloseForm(formManager.FormMain);
                    formManager.SwitchToForm(this, formManager.FormMenu);
                    break;
                case DialogResult.No:
                    formManager.CloseForm(formManager.FormMain);
                    formManager.SwitchToForm(this, formManager.FormMenu);
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
            formManager.FormMain.UnpauseGame();
        }
    }
}
