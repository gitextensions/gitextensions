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
            Ok = new Button();
            ProgressBar = new ProgressBar();
            KeepDialogOpen = new CheckBox();
            Abort = new Button();
            pnlOutput = new Panel();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(pnlOutput);
            MainPanel.Padding = new Padding(0);
            MainPanel.Size = new Size(549, 246);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(Abort);
            ControlsPanel.Controls.Add(Ok);
            ControlsPanel.Controls.Add(KeepDialogOpen);
            ControlsPanel.Location = new Point(0, 249);
            ControlsPanel.Size = new Size(549, 39);
            // 
            // Ok
            // 
            Ok.AutoSize = true;
            Ok.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Ok.Location = new Point(380, 8);
            Ok.MinimumSize = new Size(75, 23);
            Ok.Name = "Ok";
            Ok.Size = new Size(75, 23);
            Ok.TabIndex = 0;
            Ok.Text = "OK";
            Ok.UseCompatibleTextRendering = true;
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += Ok_Click;
            // 
            // ProgressBar
            // 
            ProgressBar.Dock = DockStyle.Bottom;
            ProgressBar.Location = new Point(0, 246);
            ProgressBar.Margin = new Padding(0);
            ProgressBar.MarqueeAnimationSpeed = 1;
            ProgressBar.Name = "ProgressBar";
            ProgressBar.Size = new Size(549, 3);
            ProgressBar.Step = 50;
            ProgressBar.Style = ProgressBarStyle.Marquee;
            ProgressBar.TabIndex = 0;
            // 
            // KeepDialogOpen
            // 
            KeepDialogOpen.AutoSize = true;
            KeepDialogOpen.Location = new Point(254, 8);
            KeepDialogOpen.Name = "KeepDialogOpen";
            KeepDialogOpen.Size = new Size(120, 22);
            KeepDialogOpen.TabIndex = 2;
            KeepDialogOpen.Text = "Keep dialog open";
            KeepDialogOpen.UseCompatibleTextRendering = true;
            KeepDialogOpen.UseVisualStyleBackColor = true;
            KeepDialogOpen.CheckedChanged += KeepDialogOpen_CheckedChanged;
            // 
            // Abort
            // 
            Abort.AutoSize = true;
            Abort.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Abort.DialogResult = DialogResult.Cancel;
            Abort.Location = new Point(461, 8);
            Abort.MinimumSize = new Size(75, 23);
            Abort.Name = "Abort";
            Abort.Size = new Size(75, 23);
            Abort.TabIndex = 1;
            Abort.Text = "Abort";
            Abort.UseCompatibleTextRendering = true;
            Abort.UseVisualStyleBackColor = true;
            Abort.Click += Abort_Click;
            // 
            // pnlOutput
            // 
            pnlOutput.Dock = DockStyle.Fill;
            pnlOutput.Location = new Point(0, 0);
            pnlOutput.Name = "pnlOutput";
            pnlOutput.Padding = new Padding(12);
            pnlOutput.Size = new Size(549, 246);
            pnlOutput.TabIndex = 0;
            // 
            // FormStatus
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = Abort;
            ClientSize = new Size(549, 288);
            Controls.Add(ProgressBar);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(500, 200);
            Name = "FormStatus";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Process";
            Controls.SetChildIndex(ControlsPanel, 0);
            Controls.SetChildIndex(ProgressBar, 0);
            Controls.SetChildIndex(MainPanel, 0);
            MainPanel.ResumeLayout(false);
            ControlsPanel.ResumeLayout(false);
            ControlsPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private ProgressBar ProgressBar;
        protected Button Ok;
        protected CheckBox KeepDialogOpen;
        protected Button Abort;
        protected Panel pnlOutput;
    }
}
