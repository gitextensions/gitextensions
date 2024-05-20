using System.Windows.Forms;
using GitUI.Editor;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs
{
    partial class FormStash
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
            components = new System.ComponentModel.Container();
            gitStashBindingSource = new BindingSource(components);
            splitContainer1 = new SplitContainer();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel1 = new TableLayoutPanel();
            StashSelectedFiles = new Button();
            Stash = new Button();
            chkIncludeUntrackedFiles = new CheckBox();
            StashKeepIndex = new CheckBox();
            Apply = new Button();
            Clear = new Button();
            messageLabel = new Label();
            StashMessage = new RichTextBox();
            panel1 = new Panel();
            Loading = new LoadingControl();
            Stashed = new GitUI.FileStatusList();
            contextMenuStripStashedFiles = new ContextMenuStrip(components);
            cherryPickFileChangesToolStripMenuItem = new ToolStripMenuItem();
            toolStrip1 = new GitUI.ToolStripEx();
            showToolStripLabel = new ToolStripLabel();
            Stashes = new ToolStripComboBox();
            refreshToolStripButton = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            View = new GitUI.Editor.FileViewer();
            toolTip = new ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)(gitStashBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            toolStrip1.SuspendLayout();
            contextMenuStripStashedFiles.SuspendLayout();
            SuspendLayout();
            // 
            // gitStashBindingSource
            // 
            gitStashBindingSource.DataSource = typeof(GitStash);
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel2);
            splitContainer1.Panel1MinSize = 170;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(View);
            splitContainer1.Size = new Size(1416, 1040);
            splitContainer1.SplitterDistance = 280;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            splitContainer1.SplitterMoved += splitContainer1_SplitterMoved;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(tableLayoutPanel1, 0, 3);
            tableLayoutPanel2.Controls.Add(StashMessage, 0, 2);
            tableLayoutPanel2.Controls.Add(messageLabel, 0, 2);
            tableLayoutPanel2.Controls.Add(panel1, 0, 1);
            tableLayoutPanel2.Controls.Add(toolStrip1, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 0);
            tableLayoutPanel2.Margin = new Padding(0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 5;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(280, 1040);
            tableLayoutPanel2.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(StashSelectedFiles, 0, 2);
            tableLayoutPanel1.Controls.Add(Stash, 0, 1);
            tableLayoutPanel1.Controls.Add(chkIncludeUntrackedFiles, 1, 0);
            tableLayoutPanel1.Controls.Add(StashKeepIndex, 0, 0);
            tableLayoutPanel1.Controls.Add(Apply, 0, 4);
            tableLayoutPanel1.Controls.Add(Clear, 0, 3);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 751);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(280, 289);
            tableLayoutPanel1.TabIndex = 5;
            // 
            // StashSelectedFiles
            // 
            tableLayoutPanel1.SetColumnSpan(StashSelectedFiles, 2);
            StashSelectedFiles.Dock = DockStyle.Fill;
            StashSelectedFiles.Location = new Point(6, 109);
            StashSelectedFiles.Margin = new Padding(6, 6, 0, 6);
            StashSelectedFiles.Name = "StashSelectedFiles";
            StashSelectedFiles.Size = new Size(274, 50);
            StashSelectedFiles.TabIndex = 18;
            StashSelectedFiles.Text = "Stash &selected changes";
            toolTip.SetToolTip(StashSelectedFiles, "Stash changes for the selected files, then revert them to the original state");
            StashSelectedFiles.UseVisualStyleBackColor = true;
            StashSelectedFiles.Click += StashSelectedFiles_Click;
            // 
            // Stash
            // 
            tableLayoutPanel1.SetColumnSpan(Stash, 2);
            Stash.Dock = DockStyle.Fill;
            Stash.Location = new Point(6, 47);
            Stash.Margin = new Padding(6, 6, 0, 6);
            Stash.Name = "Stash";
            Stash.Size = new Size(274, 50);
            Stash.TabIndex = 15;
            Stash.Text = "S&tash all changes";
            toolTip.SetToolTip(Stash, "Save local changes to a new stash, then revert local changes");
            Stash.UseVisualStyleBackColor = true;
            Stash.Click += StashClick;
            // 
            // chkIncludeUntrackedFiles
            // 
            chkIncludeUntrackedFiles.Anchor = AnchorStyles.Right;
            chkIncludeUntrackedFiles.AutoSize = true;
            chkIncludeUntrackedFiles.Location = new Point(146, 6);
            chkIncludeUntrackedFiles.Margin = new Padding(6, 6, 6, 6);
            chkIncludeUntrackedFiles.Name = "chkIncludeUntrackedFiles";
            chkIncludeUntrackedFiles.Size = new Size(128, 29);
            chkIncludeUntrackedFiles.TabIndex = 14;
            chkIncludeUntrackedFiles.Text = "&Include untracked files";
            toolTip.SetToolTip(chkIncludeUntrackedFiles, "All untracked files are also stashed and then cleaned");
            chkIncludeUntrackedFiles.UseVisualStyleBackColor = true;
            // 
            // StashKeepIndex
            // 
            StashKeepIndex.Anchor = AnchorStyles.Left;
            StashKeepIndex.AutoSize = true;
            StashKeepIndex.Location = new Point(6, 6);
            StashKeepIndex.Margin = new Padding(6, 6, 6, 6);
            StashKeepIndex.Name = "StashKeepIndex";
            StashKeepIndex.Size = new Size(128, 29);
            StashKeepIndex.TabIndex = 13;
            StashKeepIndex.Text = "&Keep index";
            toolTip.SetToolTip(StashKeepIndex, "All changes already added to the index are left intact");
            StashKeepIndex.UseVisualStyleBackColor = true;
            // 
            // Apply
            // 
            tableLayoutPanel1.SetColumnSpan(Apply, 2);
            Apply.Dock = DockStyle.Fill;
            Apply.Location = new Point(6, 233);
            Apply.Margin = new Padding(6, 6, 0, 6);
            Apply.Name = "Apply";
            Apply.Size = new Size(274, 50);
            Apply.TabIndex = 17;
            Apply.Text = "&Apply Selected Stash";
            toolTip.SetToolTip(Apply, "Apply the selected stash on top of the current working directory state");
            Apply.UseVisualStyleBackColor = true;
            Apply.Click += ApplyClick;
            // 
            // Clear
            // 
            tableLayoutPanel1.SetColumnSpan(Clear, 2);
            Clear.Dock = DockStyle.Fill;
            Clear.Location = new Point(6, 171);
            Clear.Margin = new Padding(6, 6, 0, 6);
            Clear.Name = "Clear";
            Clear.Size = new Size(274, 50);
            Clear.TabIndex = 16;
            Clear.Text = "&Drop Selected Stash";
            toolTip.SetToolTip(Clear, "Remove the selected stash from the list");
            Clear.UseVisualStyleBackColor = true;
            Clear.Click += ClearClick;
            // 
            // messageLabel
            // 
            messageLabel.Name = "messageLabel";
            messageLabel.Size = new Size(280, 37);
            messageLabel.Text = "&Message:";
            // 
            // StashMessage
            // 
            StashMessage.BackColor = SystemColors.Info;
            StashMessage.BorderStyle = BorderStyle.None;
            StashMessage.Dock = DockStyle.Fill;
            StashMessage.Location = new Point(0, 601);
            StashMessage.Margin = new Padding(0);
            StashMessage.Name = "StashMessage";
            StashMessage.ReadOnly = true;
            StashMessage.Size = new Size(280, 150);
            StashMessage.TabIndex = 3;
            StashMessage.Text = "";
            // 
            // panel1
            // 
            panel1.Controls.Add(Loading);
            panel1.Controls.Add(Stashed);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 48);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(280, 553);
            panel1.TabIndex = 4;
            // 
            // Loading
            // 
            Loading.BackColor = SystemColors.Control;
            Loading.BackgroundImageLayout = ImageLayout.None;
            Loading.Dock = DockStyle.Fill;
            Loading.IsAnimating = true;
            Loading.Location = new Point(0, 0);
            Loading.Margin = new Padding(0);
            Loading.Name = "Loading";
            Loading.Size = new Size(280, 553);
            Loading.TabIndex = 12;
            Loading.TabStop = false;
            Loading.Visible = false;
            // 
            // Stashed
            // 
            Stashed.ContextMenuStrip = contextMenuStripStashedFiles;
            Stashed.Dock = DockStyle.Fill;
            Stashed.GroupByRevision = true;
            Stashed.Location = new Point(0, 0);
            Stashed.Margin = new Padding(0);
            Stashed.Name = "Stashed";
            Stashed.Size = new Size(280, 553);
            Stashed.TabIndex = 2;
            Stashed.SelectedIndexChanged += StashedSelectedIndexChanged;
            // 
            // contextMenuStripStashedFiles
            // 
            contextMenuStripStashedFiles.Items.AddRange(new ToolStripItem[] {
            cherryPickFileChangesToolStripMenuItem});
            contextMenuStripStashedFiles.Name = "contextMenuStripStashedFiles";
            contextMenuStripStashedFiles.Size = new Size(209, 76);
            contextMenuStripStashedFiles.Opening += ContextMenuStripStashedFiles_Opening;
            // 
            // cherryPickFileChangesToolStripMenuItem
            // 
            cherryPickFileChangesToolStripMenuItem.Image = Properties.Resources.CherryPick;
            cherryPickFileChangesToolStripMenuItem.Name = "cherryPickFileChangesToolStripMenuItem";
            cherryPickFileChangesToolStripMenuItem.Size = new Size(208, 22);
            cherryPickFileChangesToolStripMenuItem.Text = "&Cherry pick file changes";
            cherryPickFileChangesToolStripMenuItem.Click += CherryPickFileChangesToolStripMenuItem_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.BackColor = SystemColors.Control;
            toolStrip1.ClickThrough = true;
            toolStrip1.Dock = DockStyle.Fill;
            toolStrip1.DrawBorder = false;
            toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new ToolStripItem[] {
            showToolStripLabel,
            Stashes,
            refreshToolStripButton,
            toolStripSeparator1});
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new Padding(4);
            toolStrip1.RenderMode = ToolStripRenderMode.System;
            toolStrip1.Size = new Size(280, 48);
            toolStrip1.TabIndex = 1;
            // 
            // showToolStripLabel
            // 
            showToolStripLabel.Name = "showToolStripLabel";
            showToolStripLabel.Overflow = ToolStripItemOverflow.Never;
            showToolStripLabel.Size = new Size(78, 37);
            showToolStripLabel.Text = "S&how:";
            // 
            // Stashes
            // 
            Stashes.DropDownStyle = ComboBoxStyle.DropDownList;
            Stashes.DropDownWidth = 200;
            Stashes.MaxDropDownItems = 30;
            Stashes.Name = "Stashes";
            Stashes.Overflow = ToolStripItemOverflow.Never;
            Stashes.Size = new Size(278, 40);
            Stashes.ToolTipText = "Select a stash";
            Stashes.SelectedIndexChanged += StashesSelectedIndexChanged;
            Stashes.DropDown += Stashes_DropDown;
            // 
            // refreshToolStripButton
            // 
            refreshToolStripButton.Alignment = ToolStripItemAlignment.Right;
            refreshToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            refreshToolStripButton.Image = Properties.Images.ReloadRevisions;
            refreshToolStripButton.ImageTransparentColor = Color.Magenta;
            refreshToolStripButton.Name = "refreshToolStripButton";
            refreshToolStripButton.Overflow = ToolStripItemOverflow.Never;
            refreshToolStripButton.Size = new Size(36, 37);
            refreshToolStripButton.Text = "Refresh";
            refreshToolStripButton.Click += RefreshClick;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Overflow = ToolStripItemOverflow.Never;
            toolStripSeparator1.Size = new Size(6, 40);
            // 
            // View
            // 
            View.Dock = DockStyle.Fill;
            View.Location = new Point(0, 0);
            View.Margin = new Padding(8, 8, 8, 8);
            View.Name = "View";
            View.Size = new Size(1126, 1040);
            View.TabIndex = 0;
            View.KeyUp += View_KeyUp;
            // 
            // FormStash
            // 
            AutoScaleDimensions = new SizeF(192F, 192F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = null;
            ClientSize = new Size(1416, 1040);
            Controls.Add(splitContainer1);
            Margin = new Padding(6, 6, 6, 6);
            MinimumSize = new Size(1254, 885);
            Name = "FormStash";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Stash";
            FormClosing += FormStashFormClosing;
            Load += FormStashLoad;
            Shown += FormStashShown;
            Resize += FormStash_Resize;
            ((System.ComponentModel.ISupportInitialize)(gitStashBindingSource)).EndInit();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();
            splitContainer1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            panel1.ResumeLayout(false);
            contextMenuStripStashedFiles.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private Button Clear;
        private Button Stash;
        private Button Apply;
        private SplitContainer splitContainer1;
        private FileStatusList Stashed;
        private BindingSource gitStashBindingSource;
        private Label messageLabel;
        private RichTextBox StashMessage;
        private FileViewer View;
        private ToolStripEx toolStrip1;
        private ToolStripButton refreshToolStripButton;
        private ToolStripLabel showToolStripLabel;
        private ToolStripComboBox Stashes;
        private GitUI.UserControls.RevisionGrid.LoadingControl Loading;
        private CheckBox StashKeepIndex;
        private ToolStripSeparator toolStripSeparator1;
        private CheckBox chkIncludeUntrackedFiles;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private ToolTip toolTip;
        private Button StashSelectedFiles;
        private ContextMenuStrip contextMenuStripStashedFiles;
        private ToolStripMenuItem cherryPickFileChangesToolStripMenuItem;
    }
}
