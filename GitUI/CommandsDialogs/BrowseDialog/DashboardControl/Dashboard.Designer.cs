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
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.flpnlStart = new System.Windows.Forms.FlowLayoutPanel();
            this.flpnlContribute = new System.Windows.Forms.FlowLayoutPanel();
            this.lblContribute = new System.Windows.Forms.Label();
            this.pnlLogo = new System.Windows.Forms.Panel();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.recentRepositoriesList1 = new GitUI.CommandsDialogs.BrowseDialog.DashboardControl.UserRepositoriesList();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pnlLeft.SuspendLayout();
            this.flpnlContribute.SuspendLayout();
            this.pnlLogo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlLeft
            // 
            this.pnlLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(228)))), ((int)(((byte)(235)))));
            this.pnlLeft.Controls.Add(this.flpnlStart);
            this.pnlLeft.Controls.Add(this.flpnlContribute);
            this.pnlLeft.Controls.Add(this.pnlLogo);
            this.pnlLeft.Location = new System.Drawing.Point(33, 0);
            this.pnlLeft.Margin = new System.Windows.Forms.Padding(0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(213, 358);
            this.pnlLeft.TabIndex = 0;
            // 
            // flpnlStart
            // 
            this.flpnlStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(50)))), ((int)(((byte)(58)))));
            this.flpnlStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpnlStart.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpnlStart.Location = new System.Drawing.Point(0, 68);
            this.flpnlStart.Margin = new System.Windows.Forms.Padding(2);
            this.flpnlStart.Name = "flpnlStart";
            this.flpnlStart.Padding = new System.Windows.Forms.Padding(20);
            this.flpnlStart.Size = new System.Drawing.Size(213, 135);
            this.flpnlStart.TabIndex = 1;
            this.flpnlStart.WrapContents = false;
            // 
            // flpnlContribute
            // 
            this.flpnlContribute.BackColor = System.Drawing.Color.Transparent;
            this.flpnlContribute.Controls.Add(this.lblContribute);
            this.flpnlContribute.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flpnlContribute.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpnlContribute.Location = new System.Drawing.Point(0, 203);
            this.flpnlContribute.Margin = new System.Windows.Forms.Padding(2);
            this.flpnlContribute.Name = "flpnlContribute";
            this.flpnlContribute.Padding = new System.Windows.Forms.Padding(20, 20, 20, 30);
            this.flpnlContribute.Size = new System.Drawing.Size(213, 155);
            this.flpnlContribute.TabIndex = 2;
            this.flpnlContribute.WrapContents = false;
            // 
            // lblContribute
            // 
            this.lblContribute.AutoSize = true;
            this.lblContribute.Font = new System.Drawing.Font("Segoe UI", 14.25F);
            this.lblContribute.ForeColor = System.Drawing.Color.Gray;
            this.lblContribute.Location = new System.Drawing.Point(22, 20);
            this.lblContribute.Margin = new System.Windows.Forms.Padding(2, 0, 2, 8);
            this.lblContribute.Name = "lblContribute";
            this.lblContribute.Size = new System.Drawing.Size(102, 25);
            this.lblContribute.TabIndex = 0;
            this.lblContribute.Text = "Contribute";
            // 
            // pnlLogo
            // 
            this.pnlLogo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(40)))), ((int)(((byte)(57)))));
            this.pnlLogo.Controls.Add(this.pbLogo);
            this.pnlLogo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLogo.Location = new System.Drawing.Point(0, 0);
            this.pnlLogo.Margin = new System.Windows.Forms.Padding(8);
            this.pnlLogo.Name = "pnlLogo";
            this.pnlLogo.Padding = new System.Windows.Forms.Padding(20, 0, 20, 14);
            this.pnlLogo.Size = new System.Drawing.Size(213, 68);
            this.pnlLogo.TabIndex = 0;
            // 
            // pbLogo
            // 
            this.pbLogo.Image = global::GitUI.Properties.Resources.git_extensions_logo;
            this.pbLogo.Location = new System.Drawing.Point(23, 19);
            this.pbLogo.Name = "pbLogo";
            this.pbLogo.Size = new System.Drawing.Size(167, 32);
            this.pbLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbLogo.TabIndex = 0;
            this.pbLogo.TabStop = false;
            // 
            // recentRepositoriesList1
            // 
            this.recentRepositoriesList1.BranchNameColor = System.Drawing.SystemColors.HotTrack;
            this.recentRepositoriesList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.recentRepositoriesList1.HeaderBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(183)))), ((int)(((byte)(226)))));
            this.recentRepositoriesList1.HeaderColor = System.Drawing.Color.Empty;
            this.recentRepositoriesList1.HeaderHeight = 70;
            this.recentRepositoriesList1.HoverColor = System.Drawing.Color.Empty;
            this.recentRepositoriesList1.Location = new System.Drawing.Point(246, 0);
            this.recentRepositoriesList1.MainBackColor = System.Drawing.Color.Empty;
            this.recentRepositoriesList1.Margin = new System.Windows.Forms.Padding(0);
            this.recentRepositoriesList1.Name = "recentRepositoriesList1";
            this.recentRepositoriesList1.Size = new System.Drawing.Size(405, 358);
            this.recentRepositoriesList1.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.142857F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 213F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85.71428F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.142857F));
            this.tableLayoutPanel1.Controls.Add(this.pnlLeft, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.recentRepositoriesList1, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(686, 358);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // Dashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(253)))));
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Name = "Dashboard";
            this.Size = new System.Drawing.Size(686, 358);
            this.ParentChanged += new System.EventHandler(this.dashboard_ParentChanged);
            this.pnlLeft.ResumeLayout(false);
            this.flpnlContribute.ResumeLayout(false);
            this.flpnlContribute.PerformLayout();
            this.pnlLogo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flpnlStart;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.FlowLayoutPanel flpnlContribute;
        private System.Windows.Forms.Label lblContribute;
        private System.Windows.Forms.Panel pnlLogo;
        private UserRepositoriesList recentRepositoriesList1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox pbLogo;
    }
}
