namespace Pacman_Projection
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.pacTickTimer = new System.Windows.Forms.Timer(this.components);
            this.pacImageTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // pacTickTimer
            // 
            this.pacTickTimer.Interval = 200;
            this.pacTickTimer.Tick += new System.EventHandler(this.pacTickTimer_Tick);
            // 
            // pacImageTimer
            // 
            this.pacImageTimer.Interval = 90;
            this.pacImageTimer.Tick += new System.EventHandler(this.pacImageTimer_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 697);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form1";
            this.Text = "Pacman";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.View_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer pacTickTimer;
        private System.Windows.Forms.Timer pacImageTimer;
    }
}

