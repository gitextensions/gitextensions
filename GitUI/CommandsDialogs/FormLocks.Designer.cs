using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;
using GitUI.Editor;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Patches;
using GitCommands.Utils;
using GitUI.Properties;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    partial class FormLocks
    {
        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLocks));
            this._currentFilesList = new GitUI.FileStatusList();
            this.Pull = new System.Windows.Forms.Button();
            this._stageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._unstagedFileContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._unstagedFileContext.SuspendLayout();
            this.SuspendLayout();
            // 
            // _currentFilesList
            // 
            this._currentFilesList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._currentFilesList.FilterVisible = true;
            this._currentFilesList.GroupByRevision = false;
            this._currentFilesList.Location = new System.Drawing.Point(0, 0);
            this._currentFilesList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this._currentFilesList.Name = "_currentFilesList";
            this._currentFilesList.SelectFirstItemOnSetItems = false;
            this._currentFilesList.Size = new System.Drawing.Size(832, 436);
            this._currentFilesList.TabIndex = 0;
            this._currentFilesList.SelectedIndexChanged += new System.EventHandler(this.SelectionChanged);
            // 
            // Pull
            // 
            this.Pull.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Pull.Location = new System.Drawing.Point(668, 0);
            this.Pull.Name = "Pull";
            this.Pull.Size = new System.Drawing.Size(101, 25);
            this.Pull.TabIndex = 3;
            this.Pull.Text = "Refresh";
            this.Pull.UseVisualStyleBackColor = true;
            this.Pull.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // _stageToolStripMenuItem
            // 
            this._stageToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this._stageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_stageToolStripMenuItem.Image")));
            this._stageToolStripMenuItem.Name = "_stageToolStripMenuItem";
            this._stageToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._stageToolStripMenuItem.Text = "Unlock";
            this._stageToolStripMenuItem.Click += new System.EventHandler(this.stageToolStripMenuItem_Click);
            // 
            // _unstagedFileContext
            // 
            this._unstagedFileContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._stageToolStripMenuItem,
            this.refreshToolStripMenuItem});
            this._unstagedFileContext.Name = "_unstagedFileContext";
            this._unstagedFileContext.Size = new System.Drawing.Size(181, 70);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // FormLocks
            // 
            this.ClientSize = new System.Drawing.Size(832, 436);
            this.Controls.Add(this.Pull);
            this.Controls.Add(this._currentFilesList);
            this.Name = "FormLocks";
            this._unstagedFileContext.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private FileStatusList _currentFilesList;

        #endregion

        private System.ComponentModel.IContainer components;
        private Button Pull;
        private ToolStripMenuItem _stageToolStripMenuItem;
        private ContextMenuStrip _unstagedFileContext;
        private ToolStripMenuItem refreshToolStripMenuItem;
    }
}
