namespace Pacman_Projection
{
    partial class Form_PauseMenu
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Form_PauseMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "Form_PauseMenu";
            this.Text = "Form_PauseMenu";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_PauseMenu_FormClosing);
            this.Load += new System.EventHandler(this.Form_PauseMenu_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form_PauseMenu_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion
    }
}