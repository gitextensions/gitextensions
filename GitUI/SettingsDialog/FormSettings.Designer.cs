using System.Windows.Forms;

namespace GitUI
{
    partial class FormSettings
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.directorySearcher1 = new System.DirectoryServices.DirectorySearcher();
            this.directorySearcher2 = new System.DirectoryServices.DirectorySearcher();
            this.label10 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.repositoryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpScriptsTab = new System.Windows.Forms.TabPage();
            this.tpHotkeys = new System.Windows.Forms.TabPage();
            this.controlHotkeys = new GitUI.Hotkey.ControlHotkeys();
            this.tpShellExt = new System.Windows.Forms.TabPage();
            this.lblMenuEntries = new System.Windows.Forms.Label();
            this.chlMenuEntries = new System.Windows.Forms.CheckedListBox();
            this.chkCascadedContextMenu = new System.Windows.Forms.CheckBox();
            this.scriptInfoBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.buttonOk = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.settingsTreeViewUserControl1 = new GitUI.SettingsDialog.SettingsTreeViewUserControl();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonDiscard = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labelSettingsPageTitle = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryBindingSource)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tpHotkeys.SuspendLayout();
            this.tpShellExt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scriptInfoBindingSource)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel4.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // directorySearcher1
            // 
            this.directorySearcher1.ClientTimeout = System.TimeSpan.Parse("-00:00:01");
            this.directorySearcher1.ServerPageTimeLimit = System.TimeSpan.Parse("-00:00:01");
            this.directorySearcher1.ServerTimeLimit = System.TimeSpan.Parse("-00:00:01");
            // 
            // directorySearcher2
            // 
            this.directorySearcher2.ClientTimeout = System.TimeSpan.Parse("-00:00:01");
            this.directorySearcher2.ServerPageTimeLimit = System.TimeSpan.Parse("-00:00:01");
            this.directorySearcher2.ServerTimeLimit = System.TimeSpan.Parse("-00:00:01");
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(63, 9);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(175, 39);
            this.label10.TabIndex = 19;
            this.label10.Text = "You need to set the correct path to \r\ngit.cmd before you can change\r\nany global s" +
    "etting.\r\n";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::GitUI.Properties.Resources.error;
            this.pictureBox2.Location = new System.Drawing.Point(3, 4);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(54, 50);
            this.pictureBox2.TabIndex = 18;
            this.pictureBox2.TabStop = false;
            // 
            // repositoryBindingSource
            // 
            this.repositoryBindingSource.DataSource = typeof(GitCommands.Repository.Repository);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpScriptsTab);
            this.tabControl1.Controls.Add(this.tpHotkeys);
            this.tabControl1.Controls.Add(this.tpShellExt);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(203, 43);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(800, 537);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tpScriptsTab
            // 
            this.tpScriptsTab.Location = new System.Drawing.Point(4, 24);
            this.tpScriptsTab.Name = "tpScriptsTab";
            this.tpScriptsTab.Padding = new System.Windows.Forms.Padding(3);
            this.tpScriptsTab.Size = new System.Drawing.Size(792, 509);
            this.tpScriptsTab.TabIndex = 8;
            this.tpScriptsTab.Text = "Scripts";
            this.tpScriptsTab.UseVisualStyleBackColor = true;
            // 
            // tpHotkeys
            // 
            this.tpHotkeys.Controls.Add(this.controlHotkeys);
            this.tpHotkeys.Location = new System.Drawing.Point(4, 24);
            this.tpHotkeys.Name = "tpHotkeys";
            this.tpHotkeys.Padding = new System.Windows.Forms.Padding(3);
            this.tpHotkeys.Size = new System.Drawing.Size(792, 509);
            this.tpHotkeys.TabIndex = 9;
            this.tpHotkeys.Text = "Hotkeys";
            this.tpHotkeys.UseVisualStyleBackColor = true;
            // 
            // controlHotkeys
            // 
            this.controlHotkeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlHotkeys.Location = new System.Drawing.Point(3, 3);
            this.controlHotkeys.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.controlHotkeys.Name = "controlHotkeys";
            this.controlHotkeys.Size = new System.Drawing.Size(786, 503);
            this.controlHotkeys.TabIndex = 0;
            // 
            // tpShellExt
            // 
            this.tpShellExt.Controls.Add(this.lblMenuEntries);
            this.tpShellExt.Controls.Add(this.chlMenuEntries);
            this.tpShellExt.Controls.Add(this.chkCascadedContextMenu);
            this.tpShellExt.Location = new System.Drawing.Point(4, 24);
            this.tpShellExt.Name = "tpShellExt";
            this.tpShellExt.Padding = new System.Windows.Forms.Padding(3);
            this.tpShellExt.Size = new System.Drawing.Size(792, 509);
            this.tpShellExt.TabIndex = 11;
            this.tpShellExt.Text = "Shell extension";
            this.tpShellExt.UseVisualStyleBackColor = true;
            // 
            // lblMenuEntries
            // 
            this.lblMenuEntries.AutoSize = true;
            this.lblMenuEntries.Location = new System.Drawing.Point(8, 50);
            this.lblMenuEntries.Name = "lblMenuEntries";
            this.lblMenuEntries.Size = new System.Drawing.Size(158, 15);
            this.lblMenuEntries.TabIndex = 2;
            this.lblMenuEntries.Text = "Visible context menu entries:";
            // 
            // chlMenuEntries
            // 
            this.chlMenuEntries.CheckOnClick = true;
            this.chlMenuEntries.FormattingEnabled = true;
            this.chlMenuEntries.Items.AddRange(new object[] {
            "Add files",
            "Apply patch",
            "Browse",
            "Create branch",
            "Checkout branch",
            "Checkout revision",
            "Clone",
            "Commit",
            "File history",
            "Reset file changes",
            "Pull",
            "Push",
            "Settings",
            "View diff"});
            this.chlMenuEntries.Location = new System.Drawing.Point(10, 68);
            this.chlMenuEntries.Name = "chlMenuEntries";
            this.chlMenuEntries.Size = new System.Drawing.Size(240, 256);
            this.chlMenuEntries.TabIndex = 1;
            // 
            // chkCascadedContextMenu
            // 
            this.chkCascadedContextMenu.AutoSize = true;
            this.chkCascadedContextMenu.Location = new System.Drawing.Point(8, 15);
            this.chkCascadedContextMenu.Name = "chkCascadedContextMenu";
            this.chkCascadedContextMenu.Size = new System.Drawing.Size(141, 17);
            this.chkCascadedContextMenu.TabIndex = 0;
            this.chkCascadedContextMenu.Text = "Cascaded context menu";
            this.chkCascadedContextMenu.UseVisualStyleBackColor = true;
            // 
            // scriptInfoBindingSource
            // 
            this.scriptInfoBindingSource.DataSource = typeof(GitUI.Script.ScriptInfo);
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(427, 3);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(88, 25);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.Ok_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.tabControl1, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.settingsTreeViewUserControl1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.flowLayoutPanel4, 1, 2);
            this.tableLayoutPanel3.Controls.Add(this.panel2, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1006, 620);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // settingsTreeViewUserControl1
            // 
            this.settingsTreeViewUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsTreeViewUserControl1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.settingsTreeViewUserControl1.Location = new System.Drawing.Point(3, 3);
            this.settingsTreeViewUserControl1.MinimumSize = new System.Drawing.Size(100, 220);
            this.settingsTreeViewUserControl1.Name = "settingsTreeViewUserControl1";
            this.tableLayoutPanel3.SetRowSpan(this.settingsTreeViewUserControl1, 2);
            this.settingsTreeViewUserControl1.Size = new System.Drawing.Size(194, 577);
            this.settingsTreeViewUserControl1.TabIndex = 1;
            this.settingsTreeViewUserControl1.SettingsPageSelected += new System.EventHandler<GitUI.SettingsDialog.SettingsPageSelectedEventArgs>(this.settingsTreeViewUserControl1_SettingsPageSelected);
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.AutoSize = true;
            this.flowLayoutPanel4.Controls.Add(this.buttonApply);
            this.flowLayoutPanel4.Controls.Add(this.buttonDiscard);
            this.flowLayoutPanel4.Controls.Add(this.buttonCancel);
            this.flowLayoutPanel4.Controls.Add(this.buttonOk);
            this.flowLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel4.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel4.Location = new System.Drawing.Point(203, 586);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(800, 31);
            this.flowLayoutPanel4.TabIndex = 3;
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(709, 3);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(88, 25);
            this.buttonApply.TabIndex = 3;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // buttonDiscard
            // 
            this.buttonDiscard.Location = new System.Drawing.Point(615, 3);
            this.buttonDiscard.Name = "buttonDiscard";
            this.buttonDiscard.Size = new System.Drawing.Size(88, 25);
            this.buttonDiscard.TabIndex = 2;
            this.buttonDiscard.Text = "Discard";
            this.buttonDiscard.UseVisualStyleBackColor = true;
            this.buttonDiscard.Click += new System.EventHandler(this.buttonDiscard_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(521, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(88, 25);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.labelSettingsPageTitle);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(203, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(800, 34);
            this.panel2.TabIndex = 4;
            // 
            // labelSettingsPageTitle
            // 
            this.labelSettingsPageTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.labelSettingsPageTitle.AutoSize = true;
            this.labelSettingsPageTitle.BackColor = System.Drawing.SystemColors.Window;
            this.labelSettingsPageTitle.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSettingsPageTitle.Location = new System.Drawing.Point(6, 5);
            this.labelSettingsPageTitle.Margin = new System.Windows.Forms.Padding(0);
            this.labelSettingsPageTitle.Name = "labelSettingsPageTitle";
            this.labelSettingsPageTitle.Padding = new System.Windows.Forms.Padding(20, 2, 20, 2);
            this.labelSettingsPageTitle.Size = new System.Drawing.Size(100, 24);
            this.labelSettingsPageTitle.TabIndex = 2;
            this.labelSettingsPageTitle.Text = "label11";
            this.labelSettingsPageTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1006, 620);
            this.Controls.Add(this.tableLayoutPanel3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(760, 605);
            this.Name = "FormSettings";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormSettings_FormClosing);
            this.Load += new System.EventHandler(this.FormSettings_Load);
            this.Shown += new System.EventHandler(this.FormSettings_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryBindingSource)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tpHotkeys.ResumeLayout(false);
            this.tpShellExt.ResumeLayout(false);
            this.tpShellExt.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scriptInfoBindingSource)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.flowLayoutPanel4.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.Button buttonOk;
        private System.DirectoryServices.DirectorySearcher directorySearcher1;
        private System.DirectoryServices.DirectorySearcher directorySearcher2;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.BindingSource repositoryBindingSource;
        private System.Windows.Forms.TabPage tpScriptsTab;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private System.Windows.Forms.TabPage tpHotkeys;
        private Hotkey.ControlHotkeys controlHotkeys;
        private BindingSource scriptInfoBindingSource;
        private TabPage tpShellExt;
        private Label lblMenuEntries;
        private CheckedListBox chlMenuEntries;
        private CheckBox chkCascadedContextMenu;
        private TableLayoutPanel tableLayoutPanel3;
        private SettingsDialog.SettingsTreeViewUserControl settingsTreeViewUserControl1;
        private Label labelSettingsPageTitle;
        private FlowLayoutPanel flowLayoutPanel4;
        private Button buttonApply;
        private Button buttonDiscard;
        private Button buttonCancel;
        private Panel panel2;

    }
}
