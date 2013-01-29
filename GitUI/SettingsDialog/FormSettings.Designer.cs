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
            this.scriptInfoBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.buttonOk = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.settingsTreeView = new GitUI.SettingsDialog.SettingsTreeViewUserControl();
            this.panelCurrentSettingsPage = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labelSettingsPageTitle = new System.Windows.Forms.Label();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonDiscard = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelInstantSaveNotice = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scriptInfoBindingSource)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.flowLayoutPanel4.SuspendLayout();
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
            // scriptInfoBindingSource
            // 
            this.scriptInfoBindingSource.DataSource = typeof(GitUI.Script.ScriptInfo);
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(414, 3);
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
            this.tableLayoutPanel3.Controls.Add(this.settingsTreeView, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.panelCurrentSettingsPage, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.panel2, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.flowLayoutPanel4, 2, 2);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(950, 602);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // settingsTreeView
            // 
            this.settingsTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsTreeView.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.settingsTreeView.Location = new System.Drawing.Point(3, 3);
            this.settingsTreeView.MinimumSize = new System.Drawing.Size(100, 220);
            this.settingsTreeView.Name = "settingsTreeView";
            this.tableLayoutPanel3.SetRowSpan(this.settingsTreeView, 2);
            this.settingsTreeView.Size = new System.Drawing.Size(194, 554);
            this.settingsTreeView.TabIndex = 1;
            this.settingsTreeView.SettingsPageSelected += new System.EventHandler<GitUI.SettingsDialog.SettingsPageSelectedEventArgs>(this.settingsTreeViewUserControl1_SettingsPageSelected);
            // 
            // panelCurrentSettingsPage
            // 
            this.panelCurrentSettingsPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCurrentSettingsPage.Location = new System.Drawing.Point(203, 43);
            this.panelCurrentSettingsPage.Name = "panelCurrentSettingsPage";
            this.panelCurrentSettingsPage.Size = new System.Drawing.Size(744, 514);
            this.panelCurrentSettingsPage.TabIndex = 5;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.labelSettingsPageTitle);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(203, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(744, 34);
            this.panel2.TabIndex = 4;
            // 
            // labelSettingsPageTitle
            // 
            this.labelSettingsPageTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.labelSettingsPageTitle.AutoSize = true;
            this.labelSettingsPageTitle.BackColor = System.Drawing.SystemColors.Control;
            this.labelSettingsPageTitle.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSettingsPageTitle.Location = new System.Drawing.Point(6, 5);
            this.labelSettingsPageTitle.Margin = new System.Windows.Forms.Padding(0);
            this.labelSettingsPageTitle.Name = "labelSettingsPageTitle";
            this.labelSettingsPageTitle.Padding = new System.Windows.Forms.Padding(20, 2, 20, 2);
            this.labelSettingsPageTitle.Size = new System.Drawing.Size(101, 24);
            this.labelSettingsPageTitle.TabIndex = 2;
            this.labelSettingsPageTitle.Text = "...title...";
            this.labelSettingsPageTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.AutoSize = true;
            this.tableLayoutPanel3.SetColumnSpan(this.flowLayoutPanel4, 2);
            this.flowLayoutPanel4.Controls.Add(this.buttonApply);
            this.flowLayoutPanel4.Controls.Add(this.buttonDiscard);
            this.flowLayoutPanel4.Controls.Add(this.buttonCancel);
            this.flowLayoutPanel4.Controls.Add(this.buttonOk);
            this.flowLayoutPanel4.Controls.Add(this.labelInstantSaveNotice);
            this.flowLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowLayoutPanel4.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel4.Location = new System.Drawing.Point(160, 563);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(787, 36);
            this.flowLayoutPanel4.TabIndex = 3;
            this.flowLayoutPanel4.WrapContents = false;
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(696, 3);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(88, 25);
            this.buttonApply.TabIndex = 3;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // buttonDiscard
            // 
            this.buttonDiscard.Location = new System.Drawing.Point(602, 3);
            this.buttonDiscard.Name = "buttonDiscard";
            this.buttonDiscard.Size = new System.Drawing.Size(88, 25);
            this.buttonDiscard.TabIndex = 2;
            this.buttonDiscard.Text = "Discard";
            this.buttonDiscard.UseVisualStyleBackColor = true;
            this.buttonDiscard.Click += new System.EventHandler(this.buttonDiscard_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(508, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(88, 25);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelInstantSaveNotice
            // 
            this.labelInstantSaveNotice.AutoSize = true;
            this.labelInstantSaveNotice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelInstantSaveNotice.Location = new System.Drawing.Point(3, 2);
            this.labelInstantSaveNotice.Margin = new System.Windows.Forms.Padding(3, 2, 3, 0);
            this.labelInstantSaveNotice.Name = "labelInstantSaveNotice";
            this.labelInstantSaveNotice.Size = new System.Drawing.Size(405, 34);
            this.labelInstantSaveNotice.TabIndex = 4;
            this.labelInstantSaveNotice.Text = "Changes made on this page will be saved instantly. \r\nThus the Cancel and Discard " +
    "button will have no effect for this page.";
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(950, 602);
            this.Controls.Add(this.tableLayoutPanel3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(966, 640);
            this.Name = "FormSettings";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormSettings_FormClosing);
            this.Load += new System.EventHandler(this.FormSettings_Load);
            this.Shown += new System.EventHandler(this.FormSettings_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.scriptInfoBindingSource)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.flowLayoutPanel4.ResumeLayout(false);
            this.flowLayoutPanel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOk;
        private System.DirectoryServices.DirectorySearcher directorySearcher1;
        private System.DirectoryServices.DirectorySearcher directorySearcher2;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.BindingSource repositoryBindingSource;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private BindingSource scriptInfoBindingSource;
        private TableLayoutPanel tableLayoutPanel3;
        private SettingsDialog.SettingsTreeViewUserControl settingsTreeView;
        private Label labelSettingsPageTitle;
        private FlowLayoutPanel flowLayoutPanel4;
        private Button buttonApply;
        private Button buttonDiscard;
        private Button buttonCancel;
        private Panel panel2;
        private Panel panelCurrentSettingsPage;
        private Label labelInstantSaveNotice;

    }
}
