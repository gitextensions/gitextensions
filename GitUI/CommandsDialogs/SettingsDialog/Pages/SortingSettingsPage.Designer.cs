namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class SortingSettingsPage
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
            tlpnlMain = new TableLayoutPanel();
            gbGeneral = new GroupBox();
            tlpnlGeneral = new TableLayoutPanel();
            _NO_TRANSLATE_cmbBranchesOrder = new ComboBox();
            _NO_TRANSLATE_cmbBranchesSortBy = new ComboBox();
            _NO_TRANSLATE_cmbRevisionsSortBy = new ComboBox();
            txtPrioBranchNames = new TextBox();
            txtPrioRemoteNames = new TextBox();
            RevisionSortOrderHelp = new PictureBox();
            PrioBranchNamesHelp = new PictureBox();
            PrioRemoteNamesHelp = new PictureBox();
            lblBranchesOrder = new Label();
            lblBranchesSortBy = new Label();
            lblRevisionsSortBy = new Label();
            lblPrioBranchNames = new Label();
            lblPrioRemoteNames = new Label();
            tlpnlMain.SuspendLayout();
            gbGeneral.SuspendLayout();
            tlpnlGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)RevisionSortOrderHelp).BeginInit();
            ((System.ComponentModel.ISupportInitialize)PrioBranchNamesHelp).BeginInit();
            ((System.ComponentModel.ISupportInitialize)PrioRemoteNamesHelp).BeginInit();
            SuspendLayout();
            // 
            // tlpnlMain
            // 
            tlpnlMain.ColumnCount = 1;
            tlpnlMain.ColumnStyles.Add(new ColumnStyle());
            tlpnlMain.Controls.Add(gbGeneral, 0, 0);
            tlpnlMain.Dock = DockStyle.Fill;
            tlpnlMain.Location = new Point(8, 8);
            tlpnlMain.Name = "tlpnlMain";
            tlpnlMain.RowCount = 3;
            tlpnlMain.RowStyles.Add(new RowStyle());
            tlpnlMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpnlMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
            tlpnlMain.Size = new Size(1511, 669);
            // 
            // gbGeneral
            // 
            gbGeneral.AutoSize = true;
            gbGeneral.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbGeneral.Controls.Add(tlpnlGeneral);
            gbGeneral.Dock = DockStyle.Fill;
            gbGeneral.Location = new Point(3, 3);
            gbGeneral.Name = "gbGeneral";
            gbGeneral.Padding = new Padding(8);
            gbGeneral.Size = new Size(1505, 197);
            gbGeneral.TabStop = false;
            gbGeneral.Text = "Sorting";
            // 
            // tlpnlGeneral
            // 
            tlpnlGeneral.AutoSize = true;
            tlpnlGeneral.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlGeneral.ColumnCount = 5;
            tlpnlGeneral.ColumnStyles.Add(new ColumnStyle());
            tlpnlGeneral.ColumnStyles.Add(new ColumnStyle());
            tlpnlGeneral.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlGeneral.Controls.Add(_NO_TRANSLATE_cmbBranchesOrder, 1, 2);
            tlpnlGeneral.Controls.Add(_NO_TRANSLATE_cmbBranchesSortBy, 1, 1);
            tlpnlGeneral.Controls.Add(_NO_TRANSLATE_cmbRevisionsSortBy, 1, 0);
            tlpnlGeneral.Controls.Add(txtPrioBranchNames, 1, 3);
            tlpnlGeneral.Controls.Add(txtPrioRemoteNames, 1, 4);
            tlpnlGeneral.Controls.Add(RevisionSortOrderHelp, 2, 0);
            tlpnlGeneral.Controls.Add(PrioBranchNamesHelp, 2, 3);
            tlpnlGeneral.Controls.Add(PrioRemoteNamesHelp, 2, 4);
            tlpnlGeneral.Controls.Add(lblBranchesOrder, 0, 2);
            tlpnlGeneral.Controls.Add(lblBranchesSortBy, 0, 1);
            tlpnlGeneral.Controls.Add(lblRevisionsSortBy, 0, 0);
            tlpnlGeneral.Controls.Add(lblPrioBranchNames, 0, 3);
            tlpnlGeneral.Controls.Add(lblPrioRemoteNames, 0, 4);
            tlpnlGeneral.Dock = DockStyle.Fill;
            tlpnlGeneral.Location = new Point(8, 24);
            tlpnlGeneral.Name = "tlpnlGeneral";
            tlpnlGeneral.RowCount = 8;
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tlpnlGeneral.Size = new Size(1489, 165);
            // 
            // _NO_TRANSLATE_cmbBranchesOrder
            // 
            _NO_TRANSLATE_cmbBranchesOrder.Dock = DockStyle.Fill;
            _NO_TRANSLATE_cmbBranchesOrder.DropDownStyle = ComboBoxStyle.DropDownList;
            _NO_TRANSLATE_cmbBranchesOrder.FormattingEnabled = true;
            _NO_TRANSLATE_cmbBranchesOrder.Items.AddRange(new object[] { "None", "Compact", "Trim start", "Filename only" });
            _NO_TRANSLATE_cmbBranchesOrder.Location = new Point(120, 61);
            _NO_TRANSLATE_cmbBranchesOrder.Name = "_NO_TRANSLATE_cmbBranchesOrder";
            _NO_TRANSLATE_cmbBranchesOrder.Size = new Size(322, 23);
            // 
            // _NO_TRANSLATE_cmbBranchesSortBy
            // 
            _NO_TRANSLATE_cmbBranchesSortBy.Dock = DockStyle.Fill;
            _NO_TRANSLATE_cmbBranchesSortBy.DropDownStyle = ComboBoxStyle.DropDownList;
            _NO_TRANSLATE_cmbBranchesSortBy.FormattingEnabled = true;
            _NO_TRANSLATE_cmbBranchesSortBy.Items.AddRange(new object[] { "None", "Compact", "Trim start", "Filename only" });
            _NO_TRANSLATE_cmbBranchesSortBy.Location = new Point(120, 32);
            _NO_TRANSLATE_cmbBranchesSortBy.Name = "_NO_TRANSLATE_cmbBranchesSortBy";
            _NO_TRANSLATE_cmbBranchesSortBy.Size = new Size(322, 23);
            // 
            // _NO_TRANSLATE_cmbRevisionsSortBy
            // 
            _NO_TRANSLATE_cmbRevisionsSortBy.Dock = DockStyle.Fill;
            _NO_TRANSLATE_cmbRevisionsSortBy.DropDownStyle = ComboBoxStyle.DropDownList;
            _NO_TRANSLATE_cmbRevisionsSortBy.FormattingEnabled = true;
            _NO_TRANSLATE_cmbRevisionsSortBy.Items.AddRange(new object[] { "Git Default", "Author Date", "Topo (Ancestor)" });
            _NO_TRANSLATE_cmbRevisionsSortBy.Location = new Point(120, 3);
            _NO_TRANSLATE_cmbRevisionsSortBy.Name = "_NO_TRANSLATE_cmbRevisionsSortBy";
            _NO_TRANSLATE_cmbRevisionsSortBy.Size = new Size(322, 23);
            // 
            // txtPrioBranchNames
            // 
            txtPrioBranchNames.Dock = DockStyle.Fill;
            txtPrioBranchNames.Location = new Point(120, 90);
            txtPrioBranchNames.Name = "txtPrioBranchNames";
            txtPrioBranchNames.Size = new Size(322, 23);
            // 
            // txtPrioRemoteNames
            // 
            txtPrioRemoteNames.Dock = DockStyle.Fill;
            txtPrioRemoteNames.Location = new Point(120, 119);
            txtPrioRemoteNames.Name = "txtPrioRemoteNames";
            txtPrioRemoteNames.Size = new Size(322, 23);
            // 
            // RevisionSortOrderHelp
            // 
            RevisionSortOrderHelp.Cursor = Cursors.Hand;
            RevisionSortOrderHelp.Image = Properties.Resources.information;
            RevisionSortOrderHelp.Location = new Point(448, 5);
            RevisionSortOrderHelp.Margin = new Padding(3, 5, 3, 3);
            RevisionSortOrderHelp.Name = "RevisionSortOrderHelp";
            RevisionSortOrderHelp.Size = new Size(16, 16);
            RevisionSortOrderHelp.SizeMode = PictureBoxSizeMode.AutoSize;
            RevisionSortOrderHelp.TabStop = false;
            // 
            // PrioBranchNamesHelp
            // 
            PrioBranchNamesHelp.Cursor = Cursors.Hand;
            PrioBranchNamesHelp.Image = Properties.Resources.information;
            PrioBranchNamesHelp.Location = new Point(448, 92);
            PrioBranchNamesHelp.Margin = new Padding(3, 5, 3, 3);
            PrioBranchNamesHelp.Name = "PrioBranchNamesHelp";
            PrioBranchNamesHelp.Size = new Size(16, 16);
            PrioBranchNamesHelp.SizeMode = PictureBoxSizeMode.AutoSize;
            PrioBranchNamesHelp.TabStop = false;
            // 
            // PrioRemoteNamesHelp
            // 
            PrioRemoteNamesHelp.Cursor = Cursors.Hand;
            PrioRemoteNamesHelp.Image = Properties.Resources.information;
            PrioRemoteNamesHelp.Location = new Point(448, 121);
            PrioRemoteNamesHelp.Margin = new Padding(3, 5, 3, 3);
            PrioRemoteNamesHelp.Name = "PrioRemoteNamesHelp";
            PrioRemoteNamesHelp.Size = new Size(16, 16);
            PrioRemoteNamesHelp.SizeMode = PictureBoxSizeMode.AutoSize;
            PrioRemoteNamesHelp.TabStop = false;
            // 
            // lblBranchesOrder
            // 
            lblBranchesOrder.AutoSize = true;
            lblBranchesOrder.Dock = DockStyle.Fill;
            lblBranchesOrder.Location = new Point(3, 58);
            lblBranchesOrder.Name = "lblBranchesOrder";
            lblBranchesOrder.Size = new Size(111, 29);
            lblBranchesOrder.Text = "Order branches";
            lblBranchesOrder.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblBranchesSortBy
            // 
            lblBranchesSortBy.AutoSize = true;
            lblBranchesSortBy.Dock = DockStyle.Fill;
            lblBranchesSortBy.Location = new Point(3, 29);
            lblBranchesSortBy.Name = "lblBranchesSortBy";
            lblBranchesSortBy.Size = new Size(111, 29);
            lblBranchesSortBy.Text = "Sort branches by";
            lblBranchesSortBy.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblRevisionsSortBy
            // 
            lblRevisionsSortBy.AutoSize = true;
            lblRevisionsSortBy.Dock = DockStyle.Fill;
            lblRevisionsSortBy.Location = new Point(3, 0);
            lblRevisionsSortBy.Name = "lblRevisionsSortBy";
            lblRevisionsSortBy.Size = new Size(111, 29);
            lblRevisionsSortBy.Text = "Sort revisions by";
            lblRevisionsSortBy.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblPrioBranchNames
            // 
            lblPrioBranchNames.AutoSize = true;
            lblPrioBranchNames.Dock = DockStyle.Fill;
            lblPrioBranchNames.Location = new Point(3, 87);
            lblPrioBranchNames.Name = "lblPrioBranchNames";
            lblPrioBranchNames.Padding = new Padding(0, 0, 0, 2);
            lblPrioBranchNames.Size = new Size(111, 29);
            lblPrioBranchNames.Text = "Prioritized branches";
            lblPrioBranchNames.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblPrioRemoteNames
            // 
            lblPrioRemoteNames.AutoSize = true;
            lblPrioRemoteNames.Dock = DockStyle.Fill;
            lblPrioRemoteNames.Location = new Point(3, 116);
            lblPrioRemoteNames.Name = "lblPrioRemoteNames";
            lblPrioRemoteNames.Padding = new Padding(0, 0, 0, 2);
            lblPrioRemoteNames.Size = new Size(111, 29);
            lblPrioRemoteNames.Text = "Prioritized remotes";
            lblPrioRemoteNames.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // SortingSettingsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(tlpnlMain);
            MinimumSize = new Size(258, 255);
            Name = "SortingSettingsPage";
            Padding = new Padding(8);
            Size = new Size(1527, 685);
            tlpnlMain.ResumeLayout(false);
            tlpnlMain.PerformLayout();
            gbGeneral.ResumeLayout(false);
            gbGeneral.PerformLayout();
            tlpnlGeneral.ResumeLayout(false);
            tlpnlGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)RevisionSortOrderHelp).EndInit();
            ((System.ComponentModel.ISupportInitialize)PrioBranchNamesHelp).EndInit();
            ((System.ComponentModel.ISupportInitialize)PrioRemoteNamesHelp).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox gbGeneral;
        private System.Windows.Forms.TableLayoutPanel tlpnlGeneral;
        private System.Windows.Forms.Label lblRevisionsSortBy;
        private System.Windows.Forms.Label lblBranchesSortBy;
        private System.Windows.Forms.Label lblBranchesOrder;
        private System.Windows.Forms.Label lblPrioBranchNames;
        private System.Windows.Forms.Label lblPrioRemoteNames;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_cmbRevisionsSortBy;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_cmbBranchesSortBy;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_cmbBranchesOrder;
        private System.Windows.Forms.TextBox txtPrioBranchNames;
        private System.Windows.Forms.TextBox txtPrioRemoteNames;
        private System.Windows.Forms.PictureBox RevisionSortOrderHelp;
        private System.Windows.Forms.PictureBox PrioBranchNamesHelp;
        private System.Windows.Forms.PictureBox PrioRemoteNamesHelp;
        private TableLayoutPanel tlpnlMain;
    }
}
