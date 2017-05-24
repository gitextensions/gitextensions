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
            this.pnlLogo = new System.Windows.Forms.Panel();
            this.lblLogo = new System.Windows.Forms.Label();
            this.flpnlContribute = new System.Windows.Forms.FlowLayoutPanel();
            this.lblContribute = new System.Windows.Forms.Label();
            this.recentRepositoriesList1 = new GitUI.CommandsDialogs.BrowseDialog.DashboardControl.RecentRepositoriesList();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pnlLeft.SuspendLayout();
            this.pnlLogo.SuspendLayout();
            this.flpnlContribute.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(228)))), ((int)(((byte)(235)))));
            this.pnlLeft.Controls.Add(this.flpnlStart);
            this.pnlLeft.Controls.Add(this.pnlLogo);
            this.pnlLeft.Controls.Add(this.flpnlContribute);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeft.Location = new System.Drawing.Point(54, 0);
            this.pnlLeft.Margin = new System.Windows.Forms.Padding(0);
            this.pnlLeft.MinimumSize = new System.Drawing.Size(0, 500);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(320, 640);
            this.pnlLeft.TabIndex = 0;
            // 
            // flpnlStart
            // 
            this.flpnlStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(50)))), ((int)(((byte)(58)))));
            this.flpnlStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpnlStart.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpnlStart.Location = new System.Drawing.Point(0, 70);
            this.flpnlStart.Name = "flpnlStart";
            this.flpnlStart.Padding = new System.Windows.Forms.Padding(30);
            this.flpnlStart.Size = new System.Drawing.Size(320, 351);
            this.flpnlStart.TabIndex = 1;
            this.flpnlStart.WrapContents = false;
            // 
            // pnlLogo
            // 
            this.pnlLogo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(40)))), ((int)(((byte)(57)))));
            this.pnlLogo.Controls.Add(this.lblLogo);
            this.pnlLogo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLogo.Location = new System.Drawing.Point(0, 0);
            this.pnlLogo.Margin = new System.Windows.Forms.Padding(0);
            this.pnlLogo.Name = "pnlLogo";
            this.pnlLogo.Padding = new System.Windows.Forms.Padding(0, 0, 0, 17);
            this.pnlLogo.Size = new System.Drawing.Size(320, 70);
            this.pnlLogo.TabIndex = 0;
            // 
            // lblLogo
            // 
            this.lblLogo.AutoSize = true;
            this.lblLogo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblLogo.Font = new System.Drawing.Font("Segoe UI", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblLogo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(29)))), ((int)(((byte)(35)))));
            this.lblLogo.Image = global::GitUI.Properties.Resources.x_with_arrow_32;
            this.lblLogo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblLogo.Location = new System.Drawing.Point(0, -57);
            this.lblLogo.Name = "lblLogo";
            this.lblLogo.Size = new System.Drawing.Size(338, 55);
            this.lblLogo.TabIndex = 0;
            this.lblLogo.Text = "    git extensions";
            this.lblLogo.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lblLogo_MouseClick);
            // 
            // flpnlContribute
            // 
            this.flpnlContribute.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(69)))), ((int)(((byte)(86)))));
            this.flpnlContribute.Controls.Add(this.lblContribute);
            this.flpnlContribute.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flpnlContribute.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpnlContribute.Location = new System.Drawing.Point(0, 421);
            this.flpnlContribute.Name = "flpnlContribute";
            this.flpnlContribute.Padding = new System.Windows.Forms.Padding(30, 30, 30, 45);
            this.flpnlContribute.Size = new System.Drawing.Size(320, 219);
            this.flpnlContribute.TabIndex = 2;
            this.flpnlContribute.WrapContents = false;
            // 
            // lblContribute
            // 
            this.lblContribute.AutoSize = true;
            this.lblContribute.Font = new System.Drawing.Font("Segoe UI", 14.25F);
            this.lblContribute.ForeColor = System.Drawing.Color.Gray;
            this.lblContribute.Location = new System.Drawing.Point(33, 30);
            this.lblContribute.Margin = new System.Windows.Forms.Padding(3, 0, 3, 12);
            this.lblContribute.Name = "lblContribute";
            this.lblContribute.Size = new System.Drawing.Size(153, 40);
            this.lblContribute.TabIndex = 0;
            this.lblContribute.Text = "Contribute";
            // 
            // recentRepositoriesList1
            // 
            this.recentRepositoriesList1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.recentRepositoriesList1.AutoScroll = true;
            this.recentRepositoriesList1.AutoScrollMargin = new System.Drawing.Size(40, 200);
            this.recentRepositoriesList1.AutoScrollMinSize = new System.Drawing.Size(450, 0);
            this.recentRepositoriesList1.BranchNameColor = System.Drawing.SystemColors.HotTrack;
            this.recentRepositoriesList1.HeaderBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(183)))), ((int)(((byte)(226)))));
            this.recentRepositoriesList1.HeaderColor = System.Drawing.Color.Empty;
            this.recentRepositoriesList1.HeaderHeight = 70;
            this.recentRepositoriesList1.HoverColor = System.Drawing.Color.Empty;
            this.recentRepositoriesList1.Location = new System.Drawing.Point(374, 0);
            this.recentRepositoriesList1.MainBackColor = System.Drawing.Color.Empty;
            this.recentRepositoriesList1.Margin = new System.Windows.Forms.Padding(0);
            this.recentRepositoriesList1.Name = "recentRepositoriesList1";
            this.recentRepositoriesList1.Size = new System.Drawing.Size(657, 640);
            this.recentRepositoriesList1.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.142857F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 320F));
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1087, 640);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // Dashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.AutoScrollMinSize = new System.Drawing.Size(0, 500);
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(253)))));
            this.BackgroundImage = global::GitUI.Properties.Resources.bgblue;
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Dashboard";
            this.Size = new System.Drawing.Size(1087, 640);
            this.ParentChanged += new System.EventHandler(this.dashboard_ParentChanged);
            this.pnlLeft.ResumeLayout(false);
            this.pnlLogo.ResumeLayout(false);
            this.pnlLogo.PerformLayout();
            this.flpnlContribute.ResumeLayout(false);
            this.flpnlContribute.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.FlowLayoutPanel flpnlStart;
        private System.Windows.Forms.Label lblLogo;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.FlowLayoutPanel flpnlContribute;
        private System.Windows.Forms.Label lblContribute;
        private System.Windows.Forms.Panel pnlLogo;
        private RecentRepositoriesList recentRepositoriesList1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
