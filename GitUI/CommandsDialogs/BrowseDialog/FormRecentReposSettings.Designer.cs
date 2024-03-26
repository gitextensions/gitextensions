namespace GitUI.CommandsDialogs.BrowseDialog
{
    partial class FormRecentReposSettings
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            FlowLayoutPanel flpnlControls;
            TableLayoutPanel tableLayoutPanel1;
            Abort = new Button();
            Ok = new Button();
            _NO_TRANSLATE_RecentRepositoriesHistorySize = new NumericUpDown();
            lblRecentRepositoriesHistorySize = new Label();
            comboMinWidthNote = new Label();
            maxRecentRepositories = new Label();
            _NO_TRANSLATE_maxRecentRepositories = new NumericUpDown();
            comboMinWidthEdit = new NumericUpDown();
            sortPinnedRepos = new CheckBox();
            comboMinWidthLabel = new Label();
            sortAllRecentRepos = new CheckBox();
            shorteningGB = new GroupBox();
            dontShortenRB = new RadioButton();
            middleDotRB = new RadioButton();
            mostSigDirRB = new RadioButton();
            comboPanel = new Panel();
            AllRecentLB = new ListView();
            chdrRepository1 = new ColumnHeader();
            contextMenuStrip1 = new ContextMenuStrip(components);
            anchorToPinnedReposToolStripMenuItem = new ToolStripMenuItem();
            anchorToAllRecentReposToolStripMenuItem = new ToolStripMenuItem();
            removeAnchorToolStripMenuItem = new ToolStripMenuItem();
            removeRecentToolStripMenuItem = new ToolStripMenuItem();
            panel3 = new Panel();
            label1 = new Label();
            PinnedLB = new ListView();
            chdrRepository = new ColumnHeader();
            panel2 = new Panel();
            PinnedLabel = new Label();
            flpnlControls = new FlowLayoutPanel();
            tableLayoutPanel1 = new TableLayoutPanel();
            flpnlControls.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_NO_TRANSLATE_RecentRepositoriesHistorySize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_NO_TRANSLATE_maxRecentRepositories).BeginInit();
            ((System.ComponentModel.ISupportInitialize)comboMinWidthEdit).BeginInit();
            shorteningGB.SuspendLayout();
            comboPanel.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            panel3.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // flpnlControls
            // 
            flpnlControls.Controls.Add(Abort);
            flpnlControls.Controls.Add(Ok);
            flpnlControls.Dock = DockStyle.Bottom;
            flpnlControls.FlowDirection = FlowDirection.RightToLeft;
            flpnlControls.Location = new Point(0, 327);
            flpnlControls.Name = "flpnlControls";
            flpnlControls.Size = new Size(676, 34);
            flpnlControls.TabIndex = 2;
            flpnlControls.WrapContents = false;
            // 
            // Abort
            // 
            Abort.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            Abort.AutoSize = true;
            Abort.DialogResult = DialogResult.Cancel;
            Abort.Location = new Point(598, 3);
            Abort.Name = "Abort";
            Abort.Size = new Size(75, 28);
            Abort.TabIndex = 1;
            Abort.Text = "Cancel";
            Abort.UseCompatibleTextRendering = true;
            Abort.UseVisualStyleBackColor = true;
            Abort.Click += Abort_Click;
            // 
            // Ok
            // 
            Ok.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            Ok.AutoSize = true;
            Ok.DialogResult = DialogResult.OK;
            Ok.Location = new Point(517, 3);
            Ok.Name = "Ok";
            Ok.Size = new Size(75, 28);
            Ok.TabIndex = 0;
            Ok.Text = "OK";
            Ok.UseCompatibleTextRendering = true;
            Ok.UseVisualStyleBackColor = true;
            Ok.Click += Ok_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_RecentRepositoriesHistorySize, 1, 0);
            tableLayoutPanel1.Controls.Add(lblRecentRepositoriesHistorySize, 0, 0);
            tableLayoutPanel1.Controls.Add(comboMinWidthNote, 0, 6);
            tableLayoutPanel1.Controls.Add(maxRecentRepositories, 0, 1);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_maxRecentRepositories, 1, 1);
            tableLayoutPanel1.Controls.Add(comboMinWidthEdit, 1, 5);
            tableLayoutPanel1.Controls.Add(sortPinnedRepos, 0, 2);
            tableLayoutPanel1.Controls.Add(comboMinWidthLabel, 0, 5);
            tableLayoutPanel1.Controls.Add(sortAllRecentRepos, 0, 3);
            tableLayoutPanel1.Controls.Add(shorteningGB, 0, 4);
            tableLayoutPanel1.Dock = DockStyle.Left;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new Padding(8);
            tableLayoutPanel1.RowCount = 7;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(350, 327);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // _NO_TRANSLATE_RecentRepositoriesHistorySize
            // 
            _NO_TRANSLATE_RecentRepositoriesHistorySize.Location = new Point(278, 11);
            _NO_TRANSLATE_RecentRepositoriesHistorySize.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            _NO_TRANSLATE_RecentRepositoriesHistorySize.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            _NO_TRANSLATE_RecentRepositoriesHistorySize.Name = "_NO_TRANSLATE_RecentRepositoriesHistorySize";
            _NO_TRANSLATE_RecentRepositoriesHistorySize.Size = new Size(61, 23);
            _NO_TRANSLATE_RecentRepositoriesHistorySize.TabIndex = 1;
            _NO_TRANSLATE_RecentRepositoriesHistorySize.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // lblRecentRepositoriesHistorySize
            // 
            lblRecentRepositoriesHistorySize.Anchor = AnchorStyles.Left;
            lblRecentRepositoriesHistorySize.AutoSize = true;
            lblRecentRepositoriesHistorySize.Location = new Point(11, 15);
            lblRecentRepositoriesHistorySize.Name = "lblRecentRepositoriesHistorySize";
            lblRecentRepositoriesHistorySize.Size = new Size(168, 15);
            lblRecentRepositoriesHistorySize.TabIndex = 0;
            lblRecentRepositoriesHistorySize.Text = "Recent repositories history size";
            // 
            // comboMinWidthNote
            // 
            tableLayoutPanel1.SetColumnSpan(comboMinWidthNote, 2);
            comboMinWidthNote.Dock = DockStyle.Fill;
            comboMinWidthNote.Location = new Point(11, 260);
            comboMinWidthNote.Name = "comboMinWidthNote";
            comboMinWidthNote.Padding = new Padding(12, 0, 0, 0);
            comboMinWidthNote.Size = new Size(328, 59);
            comboMinWidthNote.TabIndex = 9;
            comboMinWidthNote.Text = "NB: The width of the columns helps to visualise how the repository name will be shown in the combobox.";
            // 
            // maxRecentRepositories
            // 
            maxRecentRepositories.Anchor = AnchorStyles.Left;
            maxRecentRepositories.AutoSize = true;
            maxRecentRepositories.Location = new Point(11, 44);
            maxRecentRepositories.Name = "maxRecentRepositories";
            maxRecentRepositories.Size = new Size(261, 15);
            maxRecentRepositories.TabIndex = 2;
            maxRecentRepositories.Text = "Maximum number of pinned recent repositories";
            // 
            // _NO_TRANSLATE_maxRecentRepositories
            // 
            _NO_TRANSLATE_maxRecentRepositories.Location = new Point(278, 40);
            _NO_TRANSLATE_maxRecentRepositories.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            _NO_TRANSLATE_maxRecentRepositories.Name = "_NO_TRANSLATE_maxRecentRepositories";
            _NO_TRANSLATE_maxRecentRepositories.Size = new Size(61, 23);
            _NO_TRANSLATE_maxRecentRepositories.TabIndex = 3;
            _NO_TRANSLATE_maxRecentRepositories.ValueChanged += sortPinnedRepos_CheckedChanged;
            // 
            // comboMinWidthEdit
            // 
            comboMinWidthEdit.Location = new Point(278, 234);
            comboMinWidthEdit.Maximum = new decimal(new int[] { 800, 0, 0, 0 });
            comboMinWidthEdit.Name = "comboMinWidthEdit";
            comboMinWidthEdit.Size = new Size(61, 23);
            comboMinWidthEdit.TabIndex = 8;
            comboMinWidthEdit.ValueChanged += comboMinWidthEdit_ValueChanged;
            // 
            // sortPinnedRepos
            // 
            sortPinnedRepos.AutoSize = true;
            sortPinnedRepos.CheckAlign = ContentAlignment.MiddleRight;
            tableLayoutPanel1.SetColumnSpan(sortPinnedRepos, 2);
            sortPinnedRepos.Location = new Point(11, 69);
            sortPinnedRepos.Name = "sortPinnedRepos";
            sortPinnedRepos.RightToLeft = RightToLeft.Yes;
            sortPinnedRepos.Size = new Size(227, 19);
            sortPinnedRepos.TabIndex = 4;
            sortPinnedRepos.Text = "Sort pinned repositories alphabetically";
            sortPinnedRepos.UseVisualStyleBackColor = true;
            sortPinnedRepos.CheckedChanged += sortPinnedRepos_CheckedChanged;
            // 
            // comboMinWidthLabel
            // 
            comboMinWidthLabel.Anchor = AnchorStyles.Left;
            comboMinWidthLabel.AutoSize = true;
            comboMinWidthLabel.Location = new Point(11, 238);
            comboMinWidthLabel.Name = "comboMinWidthLabel";
            comboMinWidthLabel.Size = new Size(232, 15);
            comboMinWidthLabel.TabIndex = 7;
            comboMinWidthLabel.Text = "Combobox minimum width (0 = Autosize)";
            // 
            // sortAllRecentRepos
            // 
            sortAllRecentRepos.AutoSize = true;
            sortAllRecentRepos.CheckAlign = ContentAlignment.MiddleRight;
            tableLayoutPanel1.SetColumnSpan(sortAllRecentRepos, 2);
            sortAllRecentRepos.Location = new Point(11, 94);
            sortAllRecentRepos.Name = "sortAllRecentRepos";
            sortAllRecentRepos.RightToLeft = RightToLeft.Yes;
            sortAllRecentRepos.Size = new Size(223, 19);
            sortAllRecentRepos.TabIndex = 5;
            sortAllRecentRepos.Text = "Sort recent repositories alphabetically";
            sortAllRecentRepos.UseVisualStyleBackColor = true;
            sortAllRecentRepos.CheckedChanged += sortPinnedRepos_CheckedChanged;
            // 
            // shorteningGB
            // 
            shorteningGB.AutoSize = true;
            shorteningGB.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.SetColumnSpan(shorteningGB, 2);
            shorteningGB.Controls.Add(dontShortenRB);
            shorteningGB.Controls.Add(middleDotRB);
            shorteningGB.Controls.Add(mostSigDirRB);
            shorteningGB.Dock = DockStyle.Fill;
            shorteningGB.Location = new Point(11, 119);
            shorteningGB.Name = "shorteningGB";
            shorteningGB.Size = new Size(328, 109);
            shorteningGB.TabIndex = 6;
            shorteningGB.TabStop = false;
            shorteningGB.Text = "Shortening strategy";
            // 
            // dontShortenRB
            // 
            dontShortenRB.AutoSize = true;
            dontShortenRB.Location = new Point(6, 22);
            dontShortenRB.Name = "dontShortenRB";
            dontShortenRB.Size = new Size(110, 19);
            dontShortenRB.TabIndex = 0;
            dontShortenRB.TabStop = true;
            dontShortenRB.Text = "Do not shorten  ";
            dontShortenRB.UseVisualStyleBackColor = true;
            dontShortenRB.CheckedChanged += sortPinnedRepos_CheckedChanged;
            // 
            // middleDotRB
            // 
            middleDotRB.AutoSize = true;
            middleDotRB.Location = new Point(6, 45);
            middleDotRB.Name = "middleDotRB";
            middleDotRB.Size = new Size(185, 19);
            middleDotRB.TabIndex = 1;
            middleDotRB.TabStop = true;
            middleDotRB.Text = "Replace middle part with dots ";
            middleDotRB.UseVisualStyleBackColor = true;
            middleDotRB.CheckedChanged += sortPinnedRepos_CheckedChanged;
            // 
            // mostSigDirRB
            // 
            mostSigDirRB.AutoSize = true;
            mostSigDirRB.Location = new Point(6, 68);
            mostSigDirRB.Name = "mostSigDirRB";
            mostSigDirRB.Size = new Size(185, 19);
            mostSigDirRB.TabIndex = 2;
            mostSigDirRB.TabStop = true;
            mostSigDirRB.Text = "The most significant directory ";
            mostSigDirRB.UseVisualStyleBackColor = true;
            mostSigDirRB.CheckedChanged += sortPinnedRepos_CheckedChanged;
            // 
            // comboPanel
            // 
            comboPanel.Controls.Add(AllRecentLB);
            comboPanel.Controls.Add(panel3);
            comboPanel.Controls.Add(PinnedLB);
            comboPanel.Controls.Add(panel2);
            comboPanel.Dock = DockStyle.Fill;
            comboPanel.Location = new Point(350, 0);
            comboPanel.Name = "comboPanel";
            comboPanel.Size = new Size(326, 327);
            comboPanel.TabIndex = 1;
            // 
            // AllRecentLB
            // 
            AllRecentLB.Columns.AddRange(new ColumnHeader[] { chdrRepository1 });
            AllRecentLB.ContextMenuStrip = contextMenuStrip1;
            AllRecentLB.Dock = DockStyle.Fill;
            AllRecentLB.FullRowSelect = true;
            AllRecentLB.GridLines = true;
            AllRecentLB.HeaderStyle = ColumnHeaderStyle.None;
            AllRecentLB.LabelWrap = false;
            AllRecentLB.Location = new Point(0, 166);
            AllRecentLB.Name = "AllRecentLB";
            AllRecentLB.ShowItemToolTips = true;
            AllRecentLB.Size = new Size(326, 161);
            AllRecentLB.TabIndex = 2;
            AllRecentLB.UseCompatibleStateImageBehavior = false;
            AllRecentLB.View = View.Details;
            AllRecentLB.DrawItem += listView_DrawItem;
            AllRecentLB.DoubleClick += AllRecentLB_DoubleClick;
            // 
            // chdrRepository1
            // 
            chdrRepository1.Text = "Header";
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { anchorToPinnedReposToolStripMenuItem, anchorToAllRecentReposToolStripMenuItem, removeAnchorToolStripMenuItem, removeRecentToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(247, 92);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // anchorToPinnedReposToolStripMenuItem
            // 
            anchorToPinnedReposToolStripMenuItem.Name = "anchorToPinnedReposToolStripMenuItem";
            anchorToPinnedReposToolStripMenuItem.Size = new Size(246, 22);
            anchorToPinnedReposToolStripMenuItem.Text = "Anchor to pinned repositories";
            anchorToPinnedReposToolStripMenuItem.Click += anchorToMostToolStripMenuItem_Click;
            // 
            // anchorToAllRecentReposToolStripMenuItem
            // 
            anchorToAllRecentReposToolStripMenuItem.Name = "anchorToAllRecentReposToolStripMenuItem";
            anchorToAllRecentReposToolStripMenuItem.Size = new Size(246, 22);
            anchorToAllRecentReposToolStripMenuItem.Text = "Anchor to recent repositories";
            anchorToAllRecentReposToolStripMenuItem.Click += anchorToLessToolStripMenuItem_Click;
            // 
            // removeAnchorToolStripMenuItem
            // 
            removeAnchorToolStripMenuItem.Name = "removeAnchorToolStripMenuItem";
            removeAnchorToolStripMenuItem.Size = new Size(246, 22);
            removeAnchorToolStripMenuItem.Text = "Remove anchor";
            removeAnchorToolStripMenuItem.Click += removeAnchorToolStripMenuItem_Click;
            // 
            // removeRecentToolStripMenuItem
            // 
            removeRecentToolStripMenuItem.Name = "removeRecentToolStripMenuItem";
            removeRecentToolStripMenuItem.Size = new Size(246, 22);
            removeRecentToolStripMenuItem.Text = "Remove from recent repositories";
            removeRecentToolStripMenuItem.Click += removeRecentToolStripMenuItem_Click;
            // 
            // panel3
            // 
            panel3.AutoSize = true;
            panel3.Controls.Add(label1);
            panel3.Dock = DockStyle.Top;
            panel3.Location = new Point(0, 142);
            panel3.Name = "panel3";
            panel3.Size = new Size(326, 24);
            panel3.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 9);
            label1.Name = "label1";
            label1.Size = new Size(107, 15);
            label1.TabIndex = 0;
            label1.Text = "Recent repositories";
            // 
            // PinnedLB
            // 
            PinnedLB.Columns.AddRange(new ColumnHeader[] { chdrRepository });
            PinnedLB.ContextMenuStrip = contextMenuStrip1;
            PinnedLB.Dock = DockStyle.Top;
            PinnedLB.FullRowSelect = true;
            PinnedLB.GridLines = true;
            PinnedLB.HeaderStyle = ColumnHeaderStyle.None;
            PinnedLB.LabelWrap = false;
            PinnedLB.Location = new Point(0, 21);
            PinnedLB.Name = "PinnedLB";
            PinnedLB.ShowItemToolTips = true;
            PinnedLB.Size = new Size(326, 121);
            PinnedLB.TabIndex = 0;
            PinnedLB.UseCompatibleStateImageBehavior = false;
            PinnedLB.View = View.Details;
            PinnedLB.DrawItem += listView_DrawItem;
            PinnedLB.DoubleClick += PinnedLB_DoubleClick;
            // 
            // chdrRepository
            // 
            chdrRepository.Text = "Header";
            // 
            // panel2
            // 
            panel2.AutoSize = true;
            panel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel2.Controls.Add(PinnedLabel);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(326, 21);
            panel2.TabIndex = 0;
            // 
            // PinnedLabel
            // 
            PinnedLabel.AutoSize = true;
            PinnedLabel.Location = new Point(3, 6);
            PinnedLabel.Name = "PinnedLabel";
            PinnedLabel.Size = new Size(108, 15);
            PinnedLabel.TabIndex = 0;
            PinnedLabel.Text = "Pinned repositories";
            // 
            // FormRecentReposSettings
            // 
            AcceptButton = Ok;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = Abort;
            ClientSize = new Size(684, 361);
            Controls.Add(comboPanel);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(flpnlControls);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(700, 400);
            Name = "FormRecentReposSettings";
            Padding = new Padding(0, 0, 8, 0);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Recent repositories settings";
            flpnlControls.ResumeLayout(false);
            flpnlControls.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_NO_TRANSLATE_RecentRepositoriesHistorySize).EndInit();
            ((System.ComponentModel.ISupportInitialize)_NO_TRANSLATE_maxRecentRepositories).EndInit();
            ((System.ComponentModel.ISupportInitialize)comboMinWidthEdit).EndInit();
            shorteningGB.ResumeLayout(false);
            shorteningGB.PerformLayout();
            comboPanel.ResumeLayout(false);
            comboPanel.PerformLayout();
            contextMenuStrip1.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NumericUpDown _NO_TRANSLATE_maxRecentRepositories;
        private Label maxRecentRepositories;
        private CheckBox sortAllRecentRepos;
        private CheckBox sortPinnedRepos;
        private Panel comboPanel;
        private ListView AllRecentLB;
        private ListView PinnedLB;
        private Panel panel2;
        private Label PinnedLabel;
        protected Button Abort;
        private GroupBox shorteningGB;
        private RadioButton mostSigDirRB;
        private RadioButton middleDotRB;
        private RadioButton dontShortenRB;
        private NumericUpDown comboMinWidthEdit;
        private Label comboMinWidthLabel;
        private Panel panel3;
        private Label label1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem anchorToPinnedReposToolStripMenuItem;
        private ToolStripMenuItem removeAnchorToolStripMenuItem;
        private ToolStripMenuItem removeRecentToolStripMenuItem;
        private ToolStripMenuItem anchorToAllRecentReposToolStripMenuItem;
        private Button Ok;
        private ColumnHeader chdrRepository;
        private ColumnHeader chdrRepository1;
        private Label comboMinWidthNote;
        private NumericUpDown _NO_TRANSLATE_RecentRepositoriesHistorySize;
        private Label lblRecentRepositoriesHistorySize;
    }
}
