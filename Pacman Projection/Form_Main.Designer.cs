namespace Pacman_Projection
{
    partial class Form_Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Main));
            this.pacTickTimer = new System.Windows.Forms.Timer(this.components);
            this.pacImageTimer = new System.Windows.Forms.Timer(this.components);
            this.ghostTickTimer = new System.Windows.Forms.Timer(this.components);
            this.ghostImageTimer = new System.Windows.Forms.Timer(this.components);
            this.powerPelletBlinkTimer = new System.Windows.Forms.Timer(this.components);
            this.updateEatGhostDurationTimer = new System.Windows.Forms.Timer(this.components);
            this.ghostBehaviourTimeTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // pacTickTimer
            // 
            this.pacTickTimer.Interval = 200;
            this.pacTickTimer.Tick += new System.EventHandler(this.pacTickTimer_Tick);
            // 
            // pacImageTimer
            // 
            this.pacImageTimer.Tick += new System.EventHandler(this.pacImageTimer_Tick);
            // 
            // ghostTickTimer
            // 
            this.ghostTickTimer.Interval = 200;
            this.ghostTickTimer.Tick += new System.EventHandler(this.ghostTickTimer_Tick);
            // 
            // ghostImageTimer
            // 
            this.ghostImageTimer.Tick += new System.EventHandler(this.ghostImageTimer_Tick);
            // 
            // powerPelletBlinkTimer
            // 
            this.powerPelletBlinkTimer.Interval = 200;
            this.powerPelletBlinkTimer.Tick += new System.EventHandler(this.powerPelletBlinkTimer_Tick);
            // 
            // updateEatGhostDurationTimer
            // 
            this.updateEatGhostDurationTimer.Tick += new System.EventHandler(this.updateEatGhostDurationTimer_Tick);
            // 
            // ghostBehaviourTimeTimer
            // 
            this.ghostBehaviourTimeTimer.Interval = 1000;
            this.ghostBehaviourTimeTimer.Tick += new System.EventHandler(this.ghostBehaviourTimeTimer_Tick);
            // 
            // Form_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(804, 697);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form_Main";
            this.Text = "Pacman";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Main_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.View_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer pacTickTimer;
        private System.Windows.Forms.Timer pacImageTimer;
        private System.Windows.Forms.Timer ghostTickTimer;
        private System.Windows.Forms.Timer ghostImageTimer;
        private System.Windows.Forms.Timer powerPelletBlinkTimer;
        private System.Windows.Forms.Timer updateEatGhostDurationTimer;
        private System.Windows.Forms.Timer ghostBehaviourTimeTimer;
    }
}

