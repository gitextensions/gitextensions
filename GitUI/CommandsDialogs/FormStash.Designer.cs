using System.Windows.Forms;
using GitCommands.Git;
using GitUI.Editor;

namespace GitUI.CommandsDialogs
{
    partial class FormStash
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
            _asyncLoader.Dispose();
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
            this.components = new System.ComponentModel.Container();
            this.gitStashBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.StashSelectedFiles = new System.Windows.Forms.Button();
            this.Stash = new System.Windows.Forms.Button();
            this.chkIncludeUntrackedFiles = new System.Windows.Forms.CheckBox();
            this.StashKeepIndex = new System.Windows.Forms.CheckBox();
            this.Apply = new System.Windows.Forms.Button();
            this.Clear = new System.Windows.Forms.Button();
            this.StashMessage = new System.Windows.Forms.RichTextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Loading = new System.Windows.Forms.PictureBox();
            this.Stashed = new GitUI.FileStatusList();
            this.toolStrip1 = new GitUI.ToolStripEx();
            this.showToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            this.Stashes = new System.Windows.Forms.ToolStripComboBox();
            this.refreshToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_customMessage = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.View = new GitUI.Editor.FileViewer();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.gitStashBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gitStashBindingSource
            // 
            this.gitStashBindingSource.DataSource = typeof(GitStash);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel2);
            this.splitContainer1.Panel1MinSize = 170;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.View);
            this.splitContainer1.Size = new System.Drawing.Size(708, 520);
            this.splitContainer1.SplitterDistance = 280;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel1, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.StashMessage, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.toolStrip1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(280, 520);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.StashSelectedFiles, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.Stash, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkIncludeUntrackedFiles, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.StashKeepIndex, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.Apply, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.Clear, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 370);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(274, 147);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // StashSelectedFiles
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.StashSelectedFiles, 2);
            this.StashSelectedFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StashSelectedFiles.Location = new System.Drawing.Point(3, 57);
            this.StashSelectedFiles.Name = "StashSelectedFiles";
            this.StashSelectedFiles.Size = new System.Drawing.Size(268, 25);
            this.StashSelectedFiles.TabIndex = 18;
            this.StashSelectedFiles.Text = "Stash selected changes";
            this.toolTip.SetToolTip(this.StashSelectedFiles, "Stash changes for the selected files, then revert them to the original state");
            this.StashSelectedFiles.UseVisualStyleBackColor = true;
            this.StashSelectedFiles.Click += new System.EventHandler(this.StashSelectedFiles_Click);
            // 
            // Stash
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.Stash, 2);
            this.Stash.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Stash.Location = new System.Drawing.Point(3, 26);
            this.Stash.Name = "Stash";
            this.Stash.Size = new System.Drawing.Size(268, 25);
            this.Stash.TabIndex = 15;
            this.Stash.Text = "Stash all changes";
            this.toolTip.SetToolTip(this.Stash, "Save local changes to a new stash, then revert local changes");
            this.Stash.UseVisualStyleBackColor = true;
            this.Stash.Click += new System.EventHandler(this.StashClick);
            // 
            // chkIncludeUntrackedFiles
            // 
            this.chkIncludeUntrackedFiles.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.chkIncludeUntrackedFiles.AutoSize = true;
            this.chkIncludeUntrackedFiles.Location = new System.Drawing.Point(140, 3);
            this.chkIncludeUntrackedFiles.Name = "chkIncludeUntrackedFiles";
            this.chkIncludeUntrackedFiles.Size = new System.Drawing.Size(131, 17);
            this.chkIncludeUntrackedFiles.TabIndex = 14;
            this.chkIncludeUntrackedFiles.Text = "Include untracked files";
            this.toolTip.SetToolTip(this.chkIncludeUntrackedFiles, "All untracked files are also stashed and then cleaned");
            this.chkIncludeUntrackedFiles.UseVisualStyleBackColor = true;
            // 
            // StashKeepIndex
            // 
            this.StashKeepIndex.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.StashKeepIndex.AutoSize = true;
            this.StashKeepIndex.Location = new System.Drawing.Point(3, 3);
            this.StashKeepIndex.Name = "StashKeepIndex";
            this.StashKeepIndex.Size = new System.Drawing.Size(79, 17);
            this.StashKeepIndex.TabIndex = 13;
            this.StashKeepIndex.Text = "Keep index";
            this.toolTip.SetToolTip(this.StashKeepIndex, "All changes already added to the index are left intact");
            this.StashKeepIndex.UseVisualStyleBackColor = true;
            // 
            // Apply
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.Apply, 2);
            this.Apply.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Apply.Location = new System.Drawing.Point(3, 119);
            this.Apply.Name = "Apply";
            this.Apply.Size = new System.Drawing.Size(268, 25);
            this.Apply.TabIndex = 17;
            this.Apply.Text = "Apply Selected Stash";
            this.toolTip.SetToolTip(this.Apply, "Apply the selected stash on top of the current working directory state");
            this.Apply.UseVisualStyleBackColor = true;
            this.Apply.Click += new System.EventHandler(this.ApplyClick);
            // 
            // Clear
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.Clear, 2);
            this.Clear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Clear.Location = new System.Drawing.Point(3, 88);
            this.Clear.Name = "Clear";
            this.Clear.Size = new System.Drawing.Size(268, 25);
            this.Clear.TabIndex = 16;
            this.Clear.Text = "Drop Selected Stash";
            this.toolTip.SetToolTip(this.Clear, "Remove the selected stash from the list");
            this.Clear.UseVisualStyleBackColor = true;
            this.Clear.Click += new System.EventHandler(this.ClearClick);
            // 
            // StashMessage
            // 
            this.StashMessage.BackColor = System.Drawing.SystemColors.Info;
            this.StashMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.StashMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StashMessage.Location = new System.Drawing.Point(3, 289);
            this.StashMessage.Name = "StashMessage";
            this.StashMessage.ReadOnly = true;
            this.StashMessage.Size = new System.Drawing.Size(274, 75);
            this.StashMessage.TabIndex = 3;
            this.StashMessage.Text = "";
            this.StashMessage.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.StashMessage_MouseDoubleClick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.Loading);
            this.panel1.Controls.Add(this.Stashed);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 28);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(274, 255);
            this.panel1.TabIndex = 4;
            // 
            // Loading
            // 
            this.Loading.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.Loading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Loading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Loading.Location = new System.Drawing.Point(0, 0);
            this.Loading.Name = "Loading";
            this.Loading.Size = new System.Drawing.Size(274, 255);
            this.Loading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Loading.TabIndex = 12;
            this.Loading.TabStop = false;
            this.Loading.Visible = false;
            // 
            // Stashed
            // 
            this.Stashed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Stashed.Location = new System.Drawing.Point(0, 0);
            this.Stashed.Margin = new System.Windows.Forms.Padding(4);
            this.Stashed.Name = "Stashed";
            this.Stashed.Size = new System.Drawing.Size(274, 255);
            this.Stashed.TabIndex = 2;
            this.Stashed.SelectedIndexChanged += new System.EventHandler(this.StashedSelectedIndexChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.toolStrip1.ClickThrough = true;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showToolStripLabel,
            this.Stashes,
            this.refreshToolStripButton,
            this.toolStripButton_customMessage,
            this.toolStripSeparator1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(280, 25);
            this.toolStrip1.TabIndex = 1;
            // 
            // refreshToolStripButton
            // 
            this.refreshToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.refreshToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.refreshToolStripButton.Image = global::GitUI.Properties.Resources.arrow_refresh;
            this.refreshToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.refreshToolStripButton.Name = "refreshToolStripButton";
            this.refreshToolStripButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.refreshToolStripButton.Size = new System.Drawing.Size(23, 25);
            this.refreshToolStripButton.Text = "Refresh";
            this.refreshToolStripButton.Click += new System.EventHandler(this.RefreshClick);
            // 
            // showToolStripLabel
            // 
            this.showToolStripLabel.Name = "showToolStripLabel";
            this.showToolStripLabel.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.showToolStripLabel.Size = new System.Drawing.Size(39, 22);
            this.showToolStripLabel.Text = "Show:";
            // 
            // Stashes
            // 
            this.Stashes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Stashes.DropDownWidth = 200;
            this.Stashes.MaxDropDownItems = 30;
            this.Stashes.Name = "Stashes";
            this.Stashes.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.Stashes.Size = new System.Drawing.Size(141, 25);
            this.Stashes.ToolTipText = "Select a stash";
            this.Stashes.SelectedIndexChanged += new System.EventHandler(this.StashesSelectedIndexChanged);
            this.Stashes.DropDown += Stashes_DropDown;
            // 
            // toolStripButton_customMessage
            // 
            this.toolStripButton_customMessage.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_customMessage.CheckOnClick = true;
            this.toolStripButton_customMessage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_customMessage.Enabled = false;
            this.toolStripButton_customMessage.Image = global::GitUI.Properties.Resources.Modified;
            this.toolStripButton_customMessage.ImageTransparentColor = System.Drawing.Color.White;
            this.toolStripButton_customMessage.Name = "toolStripButton_customMessage";
            this.toolStripButton_customMessage.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.toolStripButton_customMessage.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_customMessage.Text = "Custom stash message";
            this.toolStripButton_customMessage.ToolTipText = "Write custom stash message";
            this.toolStripButton_customMessage.Click += new System.EventHandler(this.toolStripButton_customMessage_Click);
            this.toolStripButton_customMessage.EnabledChanged += new System.EventHandler(this.toolStripButton_customMessage_EnabledChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // View
            // 
            this.View.Dock = System.Windows.Forms.DockStyle.Fill;
            this.View.Location = new System.Drawing.Point(0, 0);
            this.View.Margin = new System.Windows.Forms.Padding(4);
            this.View.Name = "View";
            this.View.Size = new System.Drawing.Size(423, 520);
            this.View.TabIndex = 0;
            this.View.KeyUp += new System.Windows.Forms.KeyEventHandler(this.View_KeyUp);
            // 
            // FormStash
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = null;
            this.ClientSize = new System.Drawing.Size(708, 520);
            this.Controls.Add(this.splitContainer1);
            this.MinimumSize = new System.Drawing.Size(640, 478);
            this.Name = "FormStash";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Stash";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormStashFormClosing);
            this.Load += new System.EventHandler(this.FormStashLoad);
            this.Shown += new System.EventHandler(this.FormStashShown);
            this.Resize += new System.EventHandler(this.FormStash_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.gitStashBindingSource)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Clear;
        private System.Windows.Forms.Button Stash;
        private System.Windows.Forms.Button Apply;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private FileStatusList Stashed;
        private System.Windows.Forms.BindingSource gitStashBindingSource;
        private System.Windows.Forms.RichTextBox StashMessage;
        private FileViewer View;
        private ToolStripEx toolStrip1;
        private System.Windows.Forms.ToolStripButton refreshToolStripButton;
        private System.Windows.Forms.ToolStripLabel showToolStripLabel;
        private System.Windows.Forms.ToolStripComboBox Stashes;
        private PictureBox Loading;
        private CheckBox StashKeepIndex;
        private ToolStripButton toolStripButton_customMessage;
        private ToolStripSeparator toolStripSeparator1;
        private CheckBox chkIncludeUntrackedFiles;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private ToolTip toolTip;
        private Button StashSelectedFiles;
    }
}