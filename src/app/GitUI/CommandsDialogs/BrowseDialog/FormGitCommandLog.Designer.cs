using System.Windows.Forms;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    partial class FormGitCommandLog
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
            TabControl = new FullBleedTabControl();
            tabPageCommandLog = new TabPage();
            splitContainer2 = new SplitContainer();
            LogItems = new ListBox();
            contextMenu = new ContextMenuStrip(components);
            mnuSaveToFile = new ToolStripMenuItem();
            mnuCopyCommandLine = new ToolStripMenuItem();
            mnuClear = new ToolStripMenuItem();
            LogOutput = new RichTextBox();
            tabPageCommandCache = new TabPage();
            splitContainer1 = new SplitContainer();
            CommandCacheItems = new ListBox();
            commandCacheOutput = new RichTextBox();
            chkAlwaysOnTop = new CheckBox();
            chkWordWrap = new CheckBox();
            chkCaptureCallStacks = new CheckBox();
            TabControl.SuspendLayout();
            tabPageCommandLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            contextMenu.SuspendLayout();
            tabPageCommandCache.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // TabControl
            // 
            TabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TabControl.Controls.Add(tabPageCommandLog);
            TabControl.Controls.Add(tabPageCommandCache);
            TabControl.Location = new Point(0, 3);
            TabControl.Name = "TabControl";
            TabControl.SelectedIndex = 0;
            TabControl.Size = new Size(659, 442);
            TabControl.TabIndex = 1;
            TabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            // 
            // tabPageCommandLog
            // 
            tabPageCommandLog.BackColor = SystemColors.ControlLight;
            tabPageCommandLog.Controls.Add(splitContainer2);
            tabPageCommandLog.Location = new Point(1, 23);
            tabPageCommandLog.Margin = new Padding(0);
            tabPageCommandLog.Name = "tabPageCommandLog";
            tabPageCommandLog.Size = new Size(657, 418);
            tabPageCommandLog.TabIndex = 0;
            tabPageCommandLog.Text = "Command log";
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.FixedPanel = FixedPanel.Panel2;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Margin = new Padding(0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(LogItems);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(LogOutput);
            splitContainer2.Size = new Size(657, 418);
            splitContainer2.SplitterDistance = 279;
            splitContainer2.TabIndex = 1;
            // 
            // LogItems
            // 
            LogItems.BorderStyle = BorderStyle.None;
            LogItems.ContextMenuStrip = contextMenu;
            LogItems.Dock = DockStyle.Fill;
            LogItems.FormattingEnabled = true;
            LogItems.ItemHeight = 15;
            LogItems.Location = new Point(0, 0);
            LogItems.Margin = new Padding(0);
            LogItems.Name = "LogItems";
            LogItems.Size = new Size(657, 279);
            LogItems.TabIndex = 0;
            LogItems.SelectedIndexChanged += LogItems_SelectedIndexChanged;
            // 
            // contextMenu
            // 
            contextMenu.Items.AddRange(new ToolStripItem[] { mnuSaveToFile, mnuCopyCommandLine, mnuClear });
            contextMenu.Name = "logContextMenuStrip";
            contextMenu.Size = new Size(225, 92);
            // 
            // mnuSaveToFile
            // 
            mnuSaveToFile.Image = Properties.Images.SaveAs;
            mnuSaveToFile.Name = "mnuSaveToFile";
            mnuSaveToFile.ShortcutKeys = Keys.Control | Keys.S;
            mnuSaveToFile.Size = new Size(224, 22);
            mnuSaveToFile.Text = "&Save to file";
            mnuSaveToFile.Click += mnuSaveToFile_Click;
            // 
            // mnuCopyCommandLine
            // 
            mnuCopyCommandLine.Image = Properties.Images.CopyToClipboard;
            mnuCopyCommandLine.Name = "mnuCopyCommandLine";
            mnuCopyCommandLine.ShortcutKeys = Keys.Control | Keys.C;
            mnuCopyCommandLine.Size = new Size(224, 22);
            mnuCopyCommandLine.Text = "&Copy full command line";
            mnuCopyCommandLine.Click += mnuCopyCommandLine_Click;
            // 
            // mnuClear
            // 
            mnuClear.Image = Properties.Images.ClearLog;
            mnuClear.Name = "mnuClear";
            mnuClear.ShortcutKeys = Keys.Control | Keys.L;
            mnuClear.Size = new Size(224, 22);
            mnuClear.Text = "C&lear";
            mnuClear.Click += mnuClear_Click;
            // 
            // LogOutput
            // 
            LogOutput.BackColor = SystemColors.ControlLight;
            LogOutput.BorderStyle = BorderStyle.None;
            LogOutput.Dock = DockStyle.Fill;
            LogOutput.Location = new Point(0, 0);
            LogOutput.Margin = new Padding(0);
            LogOutput.Name = "LogOutput";
            LogOutput.Size = new Size(657, 135);
            LogOutput.TabIndex = 0;
            LogOutput.Text = "";
            // 
            // tabPageCommandCache
            // 
            tabPageCommandCache.BackColor = SystemColors.ControlLight;
            tabPageCommandCache.Controls.Add(splitContainer1);
            tabPageCommandCache.Location = new Point(1, 23);
            tabPageCommandCache.Margin = new Padding(0);
            tabPageCommandCache.Name = "tabPageCommandCache";
            tabPageCommandCache.Size = new Size(657, 418);
            tabPageCommandCache.TabIndex = 1;
            tabPageCommandCache.Text = "Command cache";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(CommandCacheItems);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(commandCacheOutput);
            splitContainer1.Size = new Size(657, 418);
            splitContainer1.SplitterDistance = 183;
            splitContainer1.TabIndex = 0;
            // 
            // CommandCacheItems
            // 
            CommandCacheItems.BorderStyle = BorderStyle.None;
            CommandCacheItems.Dock = DockStyle.Fill;
            CommandCacheItems.FormattingEnabled = true;
            CommandCacheItems.ItemHeight = 15;
            CommandCacheItems.Location = new Point(0, 0);
            CommandCacheItems.Margin = new Padding(0);
            CommandCacheItems.Name = "CommandCacheItems";
            CommandCacheItems.Size = new Size(657, 183);
            CommandCacheItems.TabIndex = 0;
            CommandCacheItems.SelectedIndexChanged += CommandCacheItems_SelectedIndexChanged;
            // 
            // commandCacheOutput
            // 
            commandCacheOutput.BackColor = SystemColors.ControlLight;
            commandCacheOutput.BorderStyle = BorderStyle.None;
            commandCacheOutput.Dock = DockStyle.Fill;
            commandCacheOutput.Location = new Point(0, 0);
            commandCacheOutput.Margin = new Padding(0);
            commandCacheOutput.Name = "commandCacheOutput";
            commandCacheOutput.Size = new Size(657, 231);
            commandCacheOutput.TabIndex = 0;
            commandCacheOutput.Text = "";
            // 
            // chkAlwaysOnTop
            // 
            chkAlwaysOnTop.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            chkAlwaysOnTop.AutoSize = true;
            chkAlwaysOnTop.Location = new Point(4, 449);
            chkAlwaysOnTop.Name = "chkAlwaysOnTop";
            chkAlwaysOnTop.Size = new Size(101, 19);
            chkAlwaysOnTop.TabIndex = 2;
            chkAlwaysOnTop.Text = "Always on top";
            chkAlwaysOnTop.UseVisualStyleBackColor = true;
            chkAlwaysOnTop.CheckedChanged += alwaysOnTopCheckBox_CheckedChanged;
            // 
            // chkWordWrap
            // 
            chkWordWrap.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            chkWordWrap.AutoSize = true;
            chkWordWrap.Checked = true;
            chkWordWrap.CheckState = CheckState.Checked;
            chkWordWrap.Location = new Point(104, 449);
            chkWordWrap.Name = "chkWordWrap";
            chkWordWrap.Size = new Size(84, 19);
            chkWordWrap.TabIndex = 3;
            chkWordWrap.Text = "Word wrap";
            chkWordWrap.UseVisualStyleBackColor = true;
            // 
            // chkCaptureCallStacks
            // 
            chkCaptureCallStacks.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            chkCaptureCallStacks.AutoSize = true;
            chkCaptureCallStacks.Location = new Point(191, 448);
            chkCaptureCallStacks.Name = "chkCaptureCallStacks";
            chkCaptureCallStacks.Size = new Size(124, 19);
            chkCaptureCallStacks.TabIndex = 4;
            chkCaptureCallStacks.Text = "Capture call stacks";
            chkCaptureCallStacks.UseVisualStyleBackColor = true;
            // 
            // FormGitCommandLog
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(659, 470);
            Controls.Add(chkCaptureCallStacks);
            Controls.Add(chkWordWrap);
            Controls.Add(chkAlwaysOnTop);
            Controls.Add(TabControl);
            Name = "FormGitCommandLog";
            Text = "Git Command Log";
            TabControl.ResumeLayout(false);
            tabPageCommandLog.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            contextMenu.ResumeLayout(false);
            tabPageCommandCache.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FullBleedTabControl TabControl;
        private TabPage tabPageCommandLog;
        private TabPage tabPageCommandCache;
        private SplitContainer splitContainer1;
        private RichTextBox commandCacheOutput;
        private ListBox CommandCacheItems;
        private SplitContainer splitContainer2;
        private ListBox LogItems;
        private RichTextBox LogOutput;
        private CheckBox chkAlwaysOnTop;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem mnuSaveToFile;
        private CheckBox chkWordWrap;
        private ToolStripMenuItem mnuClear;
        private CheckBox chkCaptureCallStacks;
        private ToolStripMenuItem mnuCopyCommandLine;
    }
}