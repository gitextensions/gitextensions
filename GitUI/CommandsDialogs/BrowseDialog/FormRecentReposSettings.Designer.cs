﻿namespace GitUI.CommandsDialogs.BrowseDialog
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.FlowLayoutPanel flpnlControls;
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
            this.Ok = new System.Windows.Forms.Button();
            this.Abort = new System.Windows.Forms.Button();
            this.comboMinWidthNote = new System.Windows.Forms.Label();
            this.maxRecentRepositories = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_maxRecentRepositories = new System.Windows.Forms.NumericUpDown();
            this.comboMinWidthEdit = new System.Windows.Forms.NumericUpDown();
            this.sortPinnedRepos = new System.Windows.Forms.CheckBox();
            this.comboMinWidthLabel = new System.Windows.Forms.Label();
            this.sortAllRecentRepos = new System.Windows.Forms.CheckBox();
            this.shorteningGB = new System.Windows.Forms.GroupBox();
            this.dontShortenRB = new System.Windows.Forms.RadioButton();
            this.middleDotRB = new System.Windows.Forms.RadioButton();
            this.mostSigDirRB = new System.Windows.Forms.RadioButton();
            this.comboPanel = new System.Windows.Forms.Panel();
            this.AllRecentLB = new System.Windows.Forms.ListView();
            this.chdrRepository1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.anchorToPinnedReposToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.anchorToAllRecentReposToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAnchorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeRecentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.PinnedLB = new System.Windows.Forms.ListView();
            this.chdrRepository = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel2 = new System.Windows.Forms.Panel();
            this.PinnedLabel = new System.Windows.Forms.Label();
            this.lblRecentRepositoriesHistorySize = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_RecentRepositoriesHistorySize = new System.Windows.Forms.NumericUpDown();
            flpnlControls = new System.Windows.Forms.FlowLayoutPanel();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            flpnlControls.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_maxRecentRepositories)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboMinWidthEdit)).BeginInit();
            this.shorteningGB.SuspendLayout();
            this.comboPanel.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_RecentRepositoriesHistorySize)).BeginInit();
            this.SuspendLayout();
            // 
            // flpnlControls
            // 
            flpnlControls.Controls.Add(this.Abort);
            flpnlControls.Controls.Add(this.Ok);
            flpnlControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            flpnlControls.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flpnlControls.Location = new System.Drawing.Point(0, 327);
            flpnlControls.Name = "flpnlControls";
            flpnlControls.Size = new System.Drawing.Size(676, 34);
            flpnlControls.TabIndex = 2;
            flpnlControls.WrapContents = false;
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.AutoSize = true;
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(517, 3);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 25);
            this.Ok.TabIndex = 0;
            this.Ok.Text = "OK";
            this.Ok.UseCompatibleTextRendering = true;
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // Abort
            // 
            this.Abort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.Abort.AutoSize = true;
            this.Abort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Abort.Location = new System.Drawing.Point(598, 3);
            this.Abort.Name = "Abort";
            this.Abort.Size = new System.Drawing.Size(75, 25);
            this.Abort.TabIndex = 1;
            this.Abort.Text = "Cancel";
            this.Abort.UseCompatibleTextRendering = true;
            this.Abort.UseVisualStyleBackColor = true;
            this.Abort.Click += new System.EventHandler(this.Abort_Click);
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_RecentRepositoriesHistorySize, 1, 0);
            tableLayoutPanel1.Controls.Add(this.lblRecentRepositoriesHistorySize, 0, 0);
            tableLayoutPanel1.Controls.Add(this.comboMinWidthNote, 0, 6);
            tableLayoutPanel1.Controls.Add(this.maxRecentRepositories, 0, 1);
            tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_maxRecentRepositories, 1, 1);
            tableLayoutPanel1.Controls.Add(this.comboMinWidthEdit, 1, 5);
            tableLayoutPanel1.Controls.Add(this.sortPinnedRepos, 0, 2);
            tableLayoutPanel1.Controls.Add(this.comboMinWidthLabel, 0, 5);
            tableLayoutPanel1.Controls.Add(this.sortAllRecentRepos, 0, 3);
            tableLayoutPanel1.Controls.Add(this.shorteningGB, 0, 4);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(8);
            tableLayoutPanel1.RowCount = 7;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new System.Drawing.Size(322, 327);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // comboMinWidthNote
            // 
            tableLayoutPanel1.SetColumnSpan(this.comboMinWidthNote, 2);
            this.comboMinWidthNote.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboMinWidthNote.Location = new System.Drawing.Point(11, 246);
            this.comboMinWidthNote.Name = "comboMinWidthNote";
            this.comboMinWidthNote.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.comboMinWidthNote.Size = new System.Drawing.Size(300, 73);
            this.comboMinWidthNote.TabIndex = 9;
            this.comboMinWidthNote.Text = "NB: The width of the columns helps to visualise how the repository name will be s" +
    "hown in the combobox.";
            // 
            // maxRecentRepositories
            // 
            this.maxRecentRepositories.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.maxRecentRepositories.AutoSize = true;
            this.maxRecentRepositories.Location = new System.Drawing.Point(11, 42);
            this.maxRecentRepositories.Name = "maxRecentRepositories";
            this.maxRecentRepositories.Size = new System.Drawing.Size(222, 13);
            this.maxRecentRepositories.TabIndex = 2;
            this.maxRecentRepositories.Text = "Maximum number of pinned recent repositories";
            // 
            // _NO_TRANSLATE_maxRecentRepositories
            // 
            this._NO_TRANSLATE_maxRecentRepositories.Location = new System.Drawing.Point(250, 38);
            this._NO_TRANSLATE_maxRecentRepositories.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this._NO_TRANSLATE_maxRecentRepositories.Name = "_NO_TRANSLATE_maxRecentRepositories";
            this._NO_TRANSLATE_maxRecentRepositories.Size = new System.Drawing.Size(61, 21);
            this._NO_TRANSLATE_maxRecentRepositories.TabIndex = 3;
            this._NO_TRANSLATE_maxRecentRepositories.ValueChanged += new System.EventHandler(this.sortPinnedRepos_CheckedChanged);
            // 
            // comboMinWidthEdit
            // 
            this.comboMinWidthEdit.Location = new System.Drawing.Point(250, 222);
            this.comboMinWidthEdit.Maximum = new decimal(new int[] {
            800,
            0,
            0,
            0});
            this.comboMinWidthEdit.Name = "comboMinWidthEdit";
            this.comboMinWidthEdit.Size = new System.Drawing.Size(61, 21);
            this.comboMinWidthEdit.TabIndex = 8;
            this.comboMinWidthEdit.ValueChanged += new System.EventHandler(this.comboMinWidthEdit_ValueChanged);
            // 
            // sortPinnedRepos
            // 
            this.sortPinnedRepos.AutoSize = true;
            this.sortPinnedRepos.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.sortPinnedRepos.Location = new System.Drawing.Point(11, 65);
            this.sortPinnedRepos.Name = "sortPinnedRepos";
            this.sortPinnedRepos.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.sortPinnedRepos.Size = new System.Drawing.Size(233, 17);
            this.sortPinnedRepos.TabIndex = 4;
            this.sortPinnedRepos.Text = "Sort pinned repositories alphabetically";
            this.sortPinnedRepos.UseVisualStyleBackColor = true;
            this.sortPinnedRepos.CheckedChanged += new System.EventHandler(this.sortPinnedRepos_CheckedChanged);
            // 
            // comboMinWidthLabel
            // 
            this.comboMinWidthLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboMinWidthLabel.AutoSize = true;
            this.comboMinWidthLabel.Location = new System.Drawing.Point(11, 226);
            this.comboMinWidthLabel.Name = "comboMinWidthLabel";
            this.comboMinWidthLabel.Size = new System.Drawing.Size(202, 13);
            this.comboMinWidthLabel.TabIndex = 7;
            this.comboMinWidthLabel.Text = "Combobox minimum width (0 = Autosize)";
            // 
            // sortAllRecentRepos
            // 
            this.sortAllRecentRepos.AutoSize = true;
            this.sortAllRecentRepos.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.sortAllRecentRepos.Location = new System.Drawing.Point(11, 88);
            this.sortAllRecentRepos.Name = "sortAllRecentRepos";
            this.sortAllRecentRepos.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.sortAllRecentRepos.Size = new System.Drawing.Size(228, 17);
            this.sortAllRecentRepos.TabIndex = 5;
            this.sortAllRecentRepos.Text = "Sort recent repositories alphabetically";
            this.sortAllRecentRepos.UseVisualStyleBackColor = true;
            this.sortAllRecentRepos.CheckedChanged += new System.EventHandler(this.sortPinnedRepos_CheckedChanged);
            // 
            // shorteningGB
            // 
            this.shorteningGB.AutoSize = true;
            this.shorteningGB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.shorteningGB.Controls.Add(this.dontShortenRB);
            this.shorteningGB.Controls.Add(this.middleDotRB);
            this.shorteningGB.Controls.Add(this.mostSigDirRB);
            this.shorteningGB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.shorteningGB.Location = new System.Drawing.Point(11, 111);
            this.shorteningGB.Name = "shorteningGB";
            this.shorteningGB.Size = new System.Drawing.Size(233, 105);
            this.shorteningGB.TabIndex = 6;
            this.shorteningGB.TabStop = false;
            this.shorteningGB.Text = "Shortening strategy";
            // 
            // dontShortenRB
            // 
            this.dontShortenRB.AutoSize = true;
            this.dontShortenRB.Location = new System.Drawing.Point(6, 22);
            this.dontShortenRB.Name = "dontShortenRB";
            this.dontShortenRB.Size = new System.Drawing.Size(103, 17);
            this.dontShortenRB.TabIndex = 0;
            this.dontShortenRB.TabStop = true;
            this.dontShortenRB.Text = "Do not shorten  ";
            this.dontShortenRB.UseVisualStyleBackColor = true;
            this.dontShortenRB.CheckedChanged += new System.EventHandler(this.sortPinnedRepos_CheckedChanged);
            // 
            // middleDotRB
            // 
            this.middleDotRB.AutoSize = true;
            this.middleDotRB.Location = new System.Drawing.Point(6, 68);
            this.middleDotRB.Name = "middleDotRB";
            this.middleDotRB.Size = new System.Drawing.Size(169, 17);
            this.middleDotRB.TabIndex = 2;
            this.middleDotRB.TabStop = true;
            this.middleDotRB.Text = "Replace middle part with dots ";
            this.middleDotRB.UseVisualStyleBackColor = true;
            this.middleDotRB.CheckedChanged += new System.EventHandler(this.sortPinnedRepos_CheckedChanged);
            // 
            // mostSigDirRB
            // 
            this.mostSigDirRB.AutoSize = true;
            this.mostSigDirRB.Location = new System.Drawing.Point(6, 45);
            this.mostSigDirRB.Name = "mostSigDirRB";
            this.mostSigDirRB.Size = new System.Drawing.Size(169, 17);
            this.mostSigDirRB.TabIndex = 1;
            this.mostSigDirRB.TabStop = true;
            this.mostSigDirRB.Text = "The most significant directory ";
            this.mostSigDirRB.UseVisualStyleBackColor = true;
            this.mostSigDirRB.CheckedChanged += new System.EventHandler(this.sortPinnedRepos_CheckedChanged);
            // 
            // comboPanel
            // 
            this.comboPanel.Controls.Add(this.AllRecentLB);
            this.comboPanel.Controls.Add(this.panel3);
            this.comboPanel.Controls.Add(this.PinnedLB);
            this.comboPanel.Controls.Add(this.panel2);
            this.comboPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboPanel.Location = new System.Drawing.Point(322, 0);
            this.comboPanel.Name = "comboPanel";
            this.comboPanel.Size = new System.Drawing.Size(354, 327);
            this.comboPanel.TabIndex = 1;
            // 
            // AllRecentLB
            // 
            this.AllRecentLB.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chdrRepository1});
            this.AllRecentLB.ContextMenuStrip = this.contextMenuStrip1;
            this.AllRecentLB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AllRecentLB.GridLines = true;
            this.AllRecentLB.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.AllRecentLB.HideSelection = false;
            this.AllRecentLB.LabelWrap = false;
            this.AllRecentLB.Location = new System.Drawing.Point(0, 162);
            this.AllRecentLB.MultiSelect = false;
            this.AllRecentLB.Name = "AllRecentLB";
            this.AllRecentLB.OwnerDraw = true;
            this.AllRecentLB.Size = new System.Drawing.Size(354, 165);
            this.AllRecentLB.TabIndex = 2;
            this.AllRecentLB.UseCompatibleStateImageBehavior = false;
            this.AllRecentLB.View = System.Windows.Forms.View.Details;
            this.AllRecentLB.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listView_DrawItem);
            // 
            // chdrRepository1
            // 
            this.chdrRepository1.Text = "Header";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.anchorToPinnedReposToolStripMenuItem,
            this.anchorToAllRecentReposToolStripMenuItem,
            this.removeAnchorToolStripMenuItem,
            this.removeRecentToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(258, 92);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // anchorToPinnedReposToolStripMenuItem
            // 
            this.anchorToPinnedReposToolStripMenuItem.Name = "anchorToPinnedReposToolStripMenuItem";
            this.anchorToPinnedReposToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.anchorToPinnedReposToolStripMenuItem.Text = "Anchor to pinned repositories";
            this.anchorToPinnedReposToolStripMenuItem.Click += new System.EventHandler(this.anchorToMostToolStripMenuItem_Click);
            // 
            // anchorToAllRecentReposToolStripMenuItem
            // 
            this.anchorToAllRecentReposToolStripMenuItem.Name = "anchorToAllRecentReposToolStripMenuItem";
            this.anchorToAllRecentReposToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.anchorToAllRecentReposToolStripMenuItem.Text = "Anchor to recent repositories";
            this.anchorToAllRecentReposToolStripMenuItem.Click += new System.EventHandler(this.anchorToLessToolStripMenuItem_Click);
            // 
            // removeAnchorToolStripMenuItem
            // 
            this.removeAnchorToolStripMenuItem.Name = "removeAnchorToolStripMenuItem";
            this.removeAnchorToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.removeAnchorToolStripMenuItem.Text = "Remove anchor";
            this.removeAnchorToolStripMenuItem.Click += new System.EventHandler(this.removeAnchorToolStripMenuItem_Click);
            // 
            // removeRecentToolStripMenuItem
            // 
            this.removeRecentToolStripMenuItem.Name = "removeRecentToolStripMenuItem";
            this.removeRecentToolStripMenuItem.Size = new System.Drawing.Size(257, 22);
            this.removeRecentToolStripMenuItem.Text = "Remove from recent repositories";
            this.removeRecentToolStripMenuItem.Click += new System.EventHandler(this.removeRecentToolStripMenuItem_Click);
            // 
            // panel3
            // 
            this.panel3.AutoSize = true;
            this.panel3.Controls.Add(this.label1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 140);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(354, 22);
            this.panel3.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Recent repositories";
            // 
            // PinnedLB
            // 
            this.PinnedLB.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chdrRepository});
            this.PinnedLB.ContextMenuStrip = this.contextMenuStrip1;
            this.PinnedLB.Dock = System.Windows.Forms.DockStyle.Top;
            this.PinnedLB.GridLines = true;
            this.PinnedLB.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.PinnedLB.HideSelection = false;
            this.PinnedLB.LabelWrap = false;
            this.PinnedLB.Location = new System.Drawing.Point(0, 19);
            this.PinnedLB.MultiSelect = false;
            this.PinnedLB.Name = "PinnedLB";
            this.PinnedLB.OwnerDraw = true;
            this.PinnedLB.Size = new System.Drawing.Size(354, 121);
            this.PinnedLB.TabIndex = 0;
            this.PinnedLB.UseCompatibleStateImageBehavior = false;
            this.PinnedLB.View = System.Windows.Forms.View.Details;
            this.PinnedLB.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listView_DrawItem);
            // 
            // chdrRepository
            // 
            this.chdrRepository.Text = "Header";
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel2.Controls.Add(this.PinnedLabel);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(354, 19);
            this.panel2.TabIndex = 0;
            // 
            // PinnedLabel
            // 
            this.PinnedLabel.AutoSize = true;
            this.PinnedLabel.Location = new System.Drawing.Point(3, 6);
            this.PinnedLabel.Name = "PinnedLabel";
            this.PinnedLabel.Size = new System.Drawing.Size(123, 13);
            this.PinnedLabel.TabIndex = 0;
            this.PinnedLabel.Text = "Pinned repositories";
            // 
            // lblRecentRepositoriesHistorySize
            // 
            this.lblRecentRepositoriesHistorySize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRecentRepositoriesHistorySize.AutoSize = true;
            this.lblRecentRepositoriesHistorySize.Location = new System.Drawing.Point(11, 15);
            this.lblRecentRepositoriesHistorySize.Name = "lblRecentRepositoriesHistorySize";
            this.lblRecentRepositoriesHistorySize.Size = new System.Drawing.Size(157, 13);
            this.lblRecentRepositoriesHistorySize.TabIndex = 0;
            this.lblRecentRepositoriesHistorySize.Text = "Recent repositories history size";
            // 
            // _NO_TRANSLATE_RecentRepositoriesHistorySize
            // 
            this._NO_TRANSLATE_RecentRepositoriesHistorySize.Location = new System.Drawing.Point(250, 11);
            this._NO_TRANSLATE_RecentRepositoriesHistorySize.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this._NO_TRANSLATE_RecentRepositoriesHistorySize.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this._NO_TRANSLATE_RecentRepositoriesHistorySize.Name = "_NO_TRANSLATE_RecentRepositoriesHistorySize";
            this._NO_TRANSLATE_RecentRepositoriesHistorySize.Size = new System.Drawing.Size(61, 21);
            this._NO_TRANSLATE_RecentRepositoriesHistorySize.TabIndex = 1;
            this._NO_TRANSLATE_RecentRepositoriesHistorySize.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // FormRecentReposSettings
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Abort;
            this.ClientSize = new System.Drawing.Size(684, 361);
            this.Controls.Add(this.comboPanel);
            this.Controls.Add(tableLayoutPanel1);
            this.Controls.Add(flpnlControls);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(700, 400);
            this.Name = "FormRecentReposSettings";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Recent repositories settings";
            flpnlControls.ResumeLayout(false);
            flpnlControls.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_maxRecentRepositories)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboMinWidthEdit)).EndInit();
            this.shorteningGB.ResumeLayout(false);
            this.shorteningGB.PerformLayout();
            this.comboPanel.ResumeLayout(false);
            this.comboPanel.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_RecentRepositoriesHistorySize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_maxRecentRepositories;
        private System.Windows.Forms.Label maxRecentRepositories;
        private System.Windows.Forms.CheckBox sortAllRecentRepos;
        private System.Windows.Forms.CheckBox sortPinnedRepos;
        private System.Windows.Forms.Panel comboPanel;
        private System.Windows.Forms.ListView AllRecentLB;
        private System.Windows.Forms.ListView PinnedLB;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label PinnedLabel;
        protected System.Windows.Forms.Button Abort;
        private System.Windows.Forms.GroupBox shorteningGB;
        private System.Windows.Forms.RadioButton mostSigDirRB;
        private System.Windows.Forms.RadioButton middleDotRB;
        private System.Windows.Forms.RadioButton dontShortenRB;
        private System.Windows.Forms.NumericUpDown comboMinWidthEdit;
        private System.Windows.Forms.Label comboMinWidthLabel;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem anchorToPinnedReposToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeAnchorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeRecentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem anchorToAllRecentReposToolStripMenuItem;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.ColumnHeader chdrRepository;
        private System.Windows.Forms.ColumnHeader chdrRepository1;
        private System.Windows.Forms.Label comboMinWidthNote;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_RecentRepositoriesHistorySize;
        private System.Windows.Forms.Label lblRecentRepositoriesHistorySize;
    }
}
