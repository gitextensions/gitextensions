namespace GitUI.HelperDialogs
{
    partial class FormStatus
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
            this.Ok = new System.Windows.Forms.Button();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.KeepDialogOpen = new System.Windows.Forms.CheckBox();
            this.Abort = new System.Windows.Forms.Button();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.picBoxSuccessFail = new System.Windows.Forms.PictureBox();
            this.MainPanel.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxSuccessFail)).BeginInit();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.KeepDialogOpen);
            this.MainPanel.Controls.Add(this.Abort);
            this.MainPanel.Controls.Add(this.tableLayoutPanel);
            this.MainPanel.Controls.Add(this.Ok);
            this.MainPanel.Padding = new System.Windows.Forms.Padding(16, 20, 40, 20);
            this.MainPanel.Size = new System.Drawing.Size(549, 243);
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(314, 234);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(105, 25);
            this.Ok.TabIndex = 0;
            this.Ok.Text = "OK";
            this.Ok.UseCompatibleTextRendering = true;
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // ProgressBar
            // 
            this.ProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ProgressBar.Location = new System.Drawing.Point(0, 0);
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
            this.KeepDialogOpen.Location = new System.Drawing.Point(194, 248);
            this.KeepDialogOpen.Name = "KeepDialogOpen";
            this.KeepDialogOpen.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.KeepDialogOpen.Size = new System.Drawing.Size(111, 22);
            this.KeepDialogOpen.TabIndex = 2;
            this.KeepDialogOpen.Text = "Keep dialog open";
            this.KeepDialogOpen.UseCompatibleTextRendering = true;
            this.KeepDialogOpen.UseVisualStyleBackColor = true;
            this.KeepDialogOpen.CheckedChanged += new System.EventHandler(this.KeepDialogOpen_CheckedChanged);
            // 
            // Abort
            // 
            this.Abort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Abort.Location = new System.Drawing.Point(444, 234);
            this.Abort.Name = "Abort";
            this.Abort.Size = new System.Drawing.Size(86, 25);
            this.Abort.TabIndex = 1;
            this.Abort.Text = "Abort";
            this.Abort.UseCompatibleTextRendering = true;
            this.Abort.UseVisualStyleBackColor = true;
            this.Abort.Click += new System.EventHandler(this.Abort_Click);
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.ConsoleOutput, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.picBoxSuccessFail, 0, 0);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(10, 24);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 3F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(515, 209);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // picBoxSuccessFail
            // 
            this.picBoxSuccessFail.Dock = System.Windows.Forms.DockStyle.Right;
            this.picBoxSuccessFail.Image = global::GitUI.Properties.Images.StatusBadgeWaiting;
            this.picBoxSuccessFail.Location = new System.Drawing.Point(3, 3);
            this.picBoxSuccessFail.Name = "picBoxSuccessFail";
            this.picBoxSuccessFail.Size = new System.Drawing.Size(47, 200);
            this.picBoxSuccessFail.TabIndex = 1;
            this.picBoxSuccessFail.TabStop = false;
            // 
            // FormStatus
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Abort;
            this.ClientSize = new System.Drawing.Size(549, 288);
            this.ControlBox = false;
            this.Controls.Add(this.ProgressBar);
            this.MinimumSize = new System.Drawing.Size(500, 200);
            this.Name = "FormStatus";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Process";
            this.Controls.SetChildIndex(this.ProgressBar, 0);
            this.Controls.SetChildIndex(this.MainPanel, 0);
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.tableLayoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picBoxSuccessFail)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar ProgressBar;
        protected System.Windows.Forms.Button Ok;
        protected System.Windows.Forms.CheckBox KeepDialogOpen;
        protected System.Windows.Forms.Button Abort;
        private System.Windows.Forms.PictureBox picBoxSuccessFail;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    }
}
