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
            if (disposing && (components is not null))
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
            pnlLeft = new Panel();
            flpnlStart = new FlowLayoutPanel();
            flpnlContribute = new FlowLayoutPanel();
            lblContribute = new Label();
            pnlLogo = new Panel();
            pbLogo = new PictureBox();
            userRepositoriesList = new GitUI.CommandsDialogs.BrowseDialog.DashboardControl.UserRepositoriesList();
            tableLayoutPanel1 = new TableLayoutPanel();
            pnlLeft.SuspendLayout();
            flpnlContribute.SuspendLayout();
            pnlLogo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(pbLogo)).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // pnlLeft
            // 
            pnlLeft.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlLeft.BackColor = Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(228)))), ((int)(((byte)(235)))));
            pnlLeft.Controls.Add(flpnlStart);
            pnlLeft.Controls.Add(flpnlContribute);
            pnlLeft.Controls.Add(pnlLogo);
            pnlLeft.Location = new Point(33, 0);
            pnlLeft.Margin = new Padding(0);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(213, 358);
            pnlLeft.TabIndex = 0;
            // 
            // flpnlStart
            // 
            flpnlStart.BackColor = Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(50)))), ((int)(((byte)(58)))));
            flpnlStart.Dock = DockStyle.Fill;
            flpnlStart.FlowDirection = FlowDirection.TopDown;
            flpnlStart.Location = new Point(0, 68);
            flpnlStart.Margin = new Padding(2);
            flpnlStart.Name = "flpnlStart";
            flpnlStart.Padding = new Padding(20);
            flpnlStart.Size = new Size(213, 135);
            flpnlStart.TabIndex = 1;
            flpnlStart.WrapContents = false;
            // 
            // flpnlContribute
            // 
            flpnlContribute.BackColor = Color.Transparent;
            flpnlContribute.Controls.Add(lblContribute);
            flpnlContribute.Dock = DockStyle.Bottom;
            flpnlContribute.FlowDirection = FlowDirection.TopDown;
            flpnlContribute.Location = new Point(0, 203);
            flpnlContribute.Margin = new Padding(2);
            flpnlContribute.Name = "flpnlContribute";
            flpnlContribute.Padding = new Padding(20, 20, 20, 30);
            flpnlContribute.Size = new Size(213, 155);
            flpnlContribute.TabIndex = 2;
            flpnlContribute.WrapContents = false;
            // 
            // lblContribute
            // 
            lblContribute.AutoSize = true;
            lblContribute.Font = new Font("Segoe UI", 14.25F);
            lblContribute.ForeColor = SystemColors.GrayText;
            lblContribute.Location = new Point(22, 20);
            lblContribute.Margin = new Padding(2, 0, 2, 8);
            lblContribute.Name = "lblContribute";
            lblContribute.Size = new Size(102, 25);
            lblContribute.TabIndex = 0;
            lblContribute.Text = "Contribute";
            // 
            // pnlLogo
            // 
            pnlLogo.BackColor = Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(40)))), ((int)(((byte)(57)))));
            pnlLogo.Controls.Add(pbLogo);
            pnlLogo.Dock = DockStyle.Top;
            pnlLogo.Location = new Point(0, 0);
            pnlLogo.Margin = new Padding(8);
            pnlLogo.Name = "pnlLogo";
            pnlLogo.Padding = new Padding(20, 0, 20, 14);
            pnlLogo.Size = new Size(213, 68);
            pnlLogo.TabIndex = 0;
            // 
            // pbLogo
            // 
            pbLogo.Image = Properties.Images.GitExtensionsLogoWide;
            pbLogo.Location = new Point(14, 15);
            pbLogo.Name = "pbLogo";
            pbLogo.Size = new Size(185, 44);
            pbLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pbLogo.TabIndex = 0;
            pbLogo.TabStop = false;
            // 
            // userRepositoriesList
            // 
            userRepositoriesList.BranchNameColor = SystemColors.HotTrack;
            userRepositoriesList.Dock = DockStyle.Fill;
            userRepositoriesList.HeaderBackColor = Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(183)))), ((int)(((byte)(226)))));
            userRepositoriesList.HeaderColor = Color.Empty;
            userRepositoriesList.HeaderHeight = 70;
            userRepositoriesList.HoverColor = Color.Empty;
            userRepositoriesList.Location = new Point(246, 0);
            userRepositoriesList.MainBackColor = Color.Empty;
            userRepositoriesList.Margin = new Padding(0);
            userRepositoriesList.Name = "userRepositoriesList";
            userRepositoriesList.Size = new Size(405, 358);
            userRepositoriesList.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.BackColor = Color.Transparent;
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142857F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 213F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 85.71428F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142857F));
            tableLayoutPanel1.Controls.Add(pnlLeft, 1, 0);
            tableLayoutPanel1.Controls.Add(userRepositoriesList, 2, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(686, 358);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // Dashboard
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;
            BackColor = Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(253)))));
            Controls.Add(tableLayoutPanel1);
            DoubleBuffered = true;
            Margin = new Padding(2, 1, 2, 1);
            Name = "Dashboard";
            Size = new Size(686, 358);
            ParentChanged += dashboard_ParentChanged;
            pnlLeft.ResumeLayout(false);
            flpnlContribute.ResumeLayout(false);
            flpnlContribute.PerformLayout();
            pnlLogo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(pbLogo)).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private FlowLayoutPanel flpnlStart;
        private Panel pnlLeft;
        private FlowLayoutPanel flpnlContribute;
        private Label lblContribute;
        private Panel pnlLogo;
        private UserRepositoriesList userRepositoriesList;
        private TableLayoutPanel tableLayoutPanel1;
        private PictureBox pbLogo;
    }
}
