namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    partial class Dashboard
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.commonSplitContainer = new System.Windows.Forms.SplitContainer();
            this.splitContainer7 = new System.Windows.Forms.SplitContainer();
            this.groupLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();

            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();

            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)(this.commonSplitContainer)).BeginInit();

            this.commonSplitContainer.Panel1.SuspendLayout();
            this.commonSplitContainer.Panel2.SuspendLayout();
            this.commonSplitContainer.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer7)).BeginInit();

            this.splitContainer7.Panel1.SuspendLayout();
            this.splitContainer7.Panel2.SuspendLayout();
            this.splitContainer7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.BackColor = System.Drawing.SystemColors.ControlLight;
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.mainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.mainSplitContainer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.mainSplitContainer.Panel1.Controls.Add(this.commonSplitContainer);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.mainSplitContainer.Panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.mainSplitContainer.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.mainSplitContainer.Size = new System.Drawing.Size(972, 623);
            this.mainSplitContainer.SplitterDistance = 314;
            this.mainSplitContainer.SplitterWidth = 5;
            this.mainSplitContainer.TabIndex = 9;
            // 
            // commonSplitContainer
            // 
            this.commonSplitContainer.BackColor = System.Drawing.SystemColors.ControlLight;
            this.commonSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commonSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.commonSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.commonSplitContainer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.commonSplitContainer.Name = "commonSplitContainer";
            this.commonSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // commonSplitContainer.Panel1
            // 
            this.commonSplitContainer.Panel1.BackColor = System.Drawing.SystemColors.Control;
            // 
            // commonSplitContainer.Panel2
            // 
            this.commonSplitContainer.Panel2.Controls.Add(this.splitContainer7);
            this.commonSplitContainer.Size = new System.Drawing.Size(314, 623);
            this.commonSplitContainer.SplitterDistance = 126;
            this.commonSplitContainer.SplitterWidth = 5;
            this.commonSplitContainer.TabIndex = 0;
            // 
            // splitContainer7
            // 
            this.splitContainer7.BackColor = System.Drawing.SystemColors.ControlLight;
            this.splitContainer7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer7.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer7.Location = new System.Drawing.Point(0, 0);
            this.splitContainer7.Name = "splitContainer7";
            this.splitContainer7.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer7.Panel1
            // 
            this.splitContainer7.Panel1.BackColor = System.Drawing.SystemColors.Control;
            // 
            // splitContainer7.Panel2
            // 
            this.splitContainer7.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer7.Size = new System.Drawing.Size(314, 492);
            this.splitContainer7.SplitterDistance = 412;
            this.splitContainer7.TabIndex = 0;
            // 
            // groupLayoutPanel
            // 
            this.groupLayoutPanel.AllowDrop = true;
            this.groupLayoutPanel.AutoScroll = true;
            this.groupLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.groupLayoutPanel.Location = new System.Drawing.Point(3, 2);
            this.groupLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupLayoutPanel.Name = "groupLayoutPanel";
            this.groupLayoutPanel.Size = new System.Drawing.Size(647, 487);
            this.groupLayoutPanel.TabIndex = 0;
            this.groupLayoutPanel.WrapContents = false;
            this.groupLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | 
                                                                                   System.Windows.Forms.AnchorStyles.Bottom) | 
                                                                                   System.Windows.Forms.AnchorStyles.Left) |
                                                                                   System.Windows.Forms.AnchorStyles.Right)));
            this.groupLayoutPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.groupLayoutPanel_DragDrop);
            this.groupLayoutPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.groupLayoutPanel_DragEnter);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.SystemColors.Control;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Image = global::GitUI.Properties.Resources.git_extensions_logo_final_128;
            this.pictureBox1.Location = new System.Drawing.Point(522, 493);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(128, 128);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupLayoutPanel, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(653, 623);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // Dashboard
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.mainSplitContainer);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Dashboard";
            this.Size = new System.Drawing.Size(972, 623);
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();

            this.mainSplitContainer.ResumeLayout(false);
            this.commonSplitContainer.Panel1.ResumeLayout(false);
            this.commonSplitContainer.Panel1.PerformLayout();
            this.commonSplitContainer.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(this.commonSplitContainer)).EndInit();

            this.commonSplitContainer.ResumeLayout(false);
            this.splitContainer7.Panel1.ResumeLayout(false);
            this.splitContainer7.Panel1.PerformLayout();
            this.splitContainer7.Panel2.ResumeLayout(false);
            this.splitContainer7.Panel2.PerformLayout();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer7)).EndInit();

            this.splitContainer7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.SplitContainer commonSplitContainer;
        private System.Windows.Forms.SplitContainer splitContainer7;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.FlowLayoutPanel groupLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
