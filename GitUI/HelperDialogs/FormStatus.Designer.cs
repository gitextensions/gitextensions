namespace GitUI.HelperDialogs
{
    partial class FormStatus
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Ok = new System.Windows.Forms.Button();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.KeepDialogOpen = new System.Windows.Forms.CheckBox();
            this.Abort = new System.Windows.Forms.Button();
            this.pnlOutput = new System.Windows.Forms.Panel();
            this.MainPanel.SuspendLayout();
            this.ControlsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.pnlOutput);
            this.MainPanel.Padding = new System.Windows.Forms.Padding(0);
            this.MainPanel.Size = new System.Drawing.Size(549, 246);
            // 
            // ControlsPanel
            // 
            this.ControlsPanel.Controls.Add(this.Abort);
            this.ControlsPanel.Controls.Add(this.Ok);
            this.ControlsPanel.Controls.Add(this.KeepDialogOpen);
            this.ControlsPanel.Location = new System.Drawing.Point(0, 249);
            this.ControlsPanel.Size = new System.Drawing.Size(549, 39);
            // 
            // Ok
            // 
            this.Ok.AutoSize = true;
            this.Ok.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Ok.Location = new System.Drawing.Point(380, 8);
            this.Ok.MinimumSize = new System.Drawing.Size(75, 23);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.TabIndex = 0;
            this.Ok.Text = "OK";
            this.Ok.UseCompatibleTextRendering = true;
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // ProgressBar
            // 
            this.ProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ProgressBar.Location = new System.Drawing.Point(0, 246);
            this.ProgressBar.Margin = new System.Windows.Forms.Padding(0);
            this.ProgressBar.MarqueeAnimationSpeed = 1;
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(549, 3);
            this.ProgressBar.Step = 50;
            this.ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.ProgressBar.TabIndex = 0;
            // 
            // KeepDialogOpen
            // 
            this.KeepDialogOpen.AutoSize = true;
            this.KeepDialogOpen.Location = new System.Drawing.Point(254, 8);
            this.KeepDialogOpen.Name = "KeepDialogOpen";
            this.KeepDialogOpen.Size = new System.Drawing.Size(120, 22);
            this.KeepDialogOpen.TabIndex = 2;
            this.KeepDialogOpen.Text = "Keep dialog open";
            this.KeepDialogOpen.UseCompatibleTextRendering = true;
            this.KeepDialogOpen.UseVisualStyleBackColor = true;
            this.KeepDialogOpen.CheckedChanged += new System.EventHandler(this.KeepDialogOpen_CheckedChanged);
            // 
            // Abort
            // 
            this.Abort.AutoSize = true;
            this.Abort.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Abort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Abort.Location = new System.Drawing.Point(461, 8);
            this.Abort.MinimumSize = new System.Drawing.Size(75, 23);
            this.Abort.Name = "Abort";
            this.Abort.Size = new System.Drawing.Size(75, 23);
            this.Abort.TabIndex = 1;
            this.Abort.Text = "Abort";
            this.Abort.UseCompatibleTextRendering = true;
            this.Abort.UseVisualStyleBackColor = true;
            this.Abort.Click += new System.EventHandler(this.Abort_Click);
            // 
            // pnlOutput
            // 
            this.pnlOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlOutput.Location = new System.Drawing.Point(0, 0);
            this.pnlOutput.Name = "pnlOutput";
            this.pnlOutput.Padding = new System.Windows.Forms.Padding(12);
            this.pnlOutput.Size = new System.Drawing.Size(549, 246);
            this.pnlOutput.TabIndex = 0;
            // 
            // FormStatus
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Abort;
            this.ClientSize = new System.Drawing.Size(549, 288);
            this.Controls.Add(this.ProgressBar);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 200);
            this.Name = "FormStatus";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Process";
            this.Controls.SetChildIndex(this.ControlsPanel, 0);
            this.Controls.SetChildIndex(this.ProgressBar, 0);
            this.Controls.SetChildIndex(this.MainPanel, 0);
            this.MainPanel.ResumeLayout(false);
            this.ControlsPanel.ResumeLayout(false);
            this.ControlsPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar ProgressBar;
        protected System.Windows.Forms.Button Ok;
        protected System.Windows.Forms.CheckBox KeepDialogOpen;
        protected System.Windows.Forms.Button Abort;
        protected System.Windows.Forms.Panel pnlOutput;
    }
}
