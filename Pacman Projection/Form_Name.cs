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
    public partial class Form_Name : Form
    {
        public string player_name;
        internal int levelToBeginAt;

        Form_Menu form_menu;
        Form_Main form_main;

        Label label = new Label();
        TextBox inputBox = new TextBox();
        Button buttonContinue = new Button();
        Button buttonCancel = new Button();

        public Form_Name(Form_Menu form_menu, int levelToBeginAt)
        {
            InitializeComponent();
            this.form_menu = form_menu;
            this.levelToBeginAt = levelToBeginAt;
        }

        private void Form_Name_Load(object sender, EventArgs e)
        {
            // Set form size to fit projector
            ClientSize = new Size(500, 500);
            this.Location = new Point(388, 85);
            this.BackColor = Color.Blue;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            // Create the label
            label.Text = "Enter your name:";
            label.Location = new Point(50, 50);
            label.Size = new Size(200, 50);
            label.ForeColor = Color.Yellow;
            Controls.Add(label);
            // Create the input box
            inputBox.Location = new Point(50, 100);
            inputBox.Size = new Size(200, 50);

            inputBox.MaxLength = 15;

            Controls.Add(inputBox);
            // Create the continue button
            buttonContinue.Text = "Continue";
            buttonContinue.Location = new Point(50, 150);
            buttonContinue.Size = new Size(100, 50);
            buttonContinue.Click += ButtonContinue_Click;
            Controls.Add(buttonContinue);
            // Create the cancel button
            buttonCancel.Text = "Cancel";
            buttonCancel.Location = new Point(200, 150);
            buttonCancel.Size = new Size(100, 50);
            buttonCancel.Click += ButtonCancel_Click;
            Controls.Add(buttonCancel);
        }

        private void ButtonContinue_Click(object sender, EventArgs e)
        {
            form_main = new Form_Main(form_menu, inputBox.Text, levelToBeginAt);
            // Update form_menu with the new form_main instance so 
            // both forms point to the same instance
            form_menu.form_main = form_main;

            form_menu.soundManager.PlaySound("buttonReady", false);
            form_menu.SwitchToForm(form_main, this);
            this.Dispose();
            form_menu.form_name = null;
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            form_menu.soundManager.PlaySound("buttonReady", false);
            form_menu.SwitchToForm(form_menu, this);
        }

        private void Form_Name_FormClosing(object sender, FormClosingEventArgs e)
        {
            form_menu.SwitchToForm(form_menu, this);
        }
    }
}
