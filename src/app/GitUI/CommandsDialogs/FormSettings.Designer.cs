using System.Windows.Forms;
using GitCommands.UserRepositoryHistory;

namespace GitUI.CommandsDialogs
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
            directorySearcher1 = new System.DirectoryServices.DirectorySearcher();
            directorySearcher2 = new System.DirectoryServices.DirectorySearcher();
            label10 = new Label();
            pictureBox2 = new PictureBox();
            repositoryBindingSource = new BindingSource(components);
            helpProvider1 = new HelpProvider();
            scriptInfoBindingSource = new BindingSource(components);
            buttonOk = new Button();
            tableLayoutPanel3 = new TableLayoutPanel();
            settingsTreeView = new GitUI.CommandsDialogs.SettingsDialog.SettingsTreeViewUserControl();
            panelCurrentSettingsPage = new Panel();
            flowLayoutPanel4 = new FlowLayoutPanel();
            buttonApply = new Button();
            buttonDiscard = new Button();
            buttonCancel = new Button();
            labelInstantSaveNotice = new Label();
            ((System.ComponentModel.ISupportInitialize)(pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(repositoryBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(scriptInfoBindingSource)).BeginInit();
            tableLayoutPanel3.SuspendLayout();
            flowLayoutPanel4.SuspendLayout();
            SuspendLayout();
            //
            // directorySearcher1
            //
            directorySearcher1.ClientTimeout = System.TimeSpan.Parse("-00:00:01");
            directorySearcher1.ServerPageTimeLimit = System.TimeSpan.Parse("-00:00:01");
            directorySearcher1.ServerTimeLimit = System.TimeSpan.Parse("-00:00:01");
            //
            // directorySearcher2
            //
            directorySearcher2.ClientTimeout = System.TimeSpan.Parse("-00:00:01");
            directorySearcher2.ServerPageTimeLimit = System.TimeSpan.Parse("-00:00:01");
            directorySearcher2.ServerTimeLimit = System.TimeSpan.Parse("-00:00:01");
            //
            // label10
            //
            label10.AutoSize = true;
            label10.Location = new Point(63, 9);
            label10.Name = "label10";
            label10.Size = new Size(175, 39);
            label10.TabIndex = 19;
            label10.Text = "You need to set the correct path to \r\ngit.cmd before you can change\r\nany global s" +
    "etting.\r\n";
            //
            // pictureBox2
            //
            pictureBox2.Image = Properties.Images.StatusBadgeError;
            pictureBox2.Location = new Point(3, 4);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(54, 50);
            pictureBox2.TabIndex = 18;
            pictureBox2.TabStop = false;
            //
            // repositoryBindingSource
            //
            repositoryBindingSource.DataSource = typeof(Repository);
            //
            // scriptInfoBindingSource
            //
            scriptInfoBindingSource.DataSource = typeof(GitUI.ScriptsEngine.ScriptInfo);
            //
            // buttonOk
            //
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonOk.Location = new Point(359, 6);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new Size(84, 25);
            buttonOk.TabIndex = 0;
            buttonOk.Text = "OK";
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += Ok_Click;
            //
            // tableLayoutPanel3
            //
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(settingsTreeView, 0, 0);
            tableLayoutPanel3.Controls.Add(panelCurrentSettingsPage, 1, 0);
            tableLayoutPanel3.Controls.Add(flowLayoutPanel4, 1, 2);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(0, 0);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.Padding = new Padding(8);
            tableLayoutPanel3.RowCount = 4;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.Size = new Size(958, 646);
            tableLayoutPanel3.TabIndex = 2;
            //
            // settingsTreeView
            //
            settingsTreeView.Dock = DockStyle.Fill;
            settingsTreeView.Location = new Point(3, 3);
            settingsTreeView.MinimumSize = new Size(100, 220);
            settingsTreeView.Name = "settingsTreeView";
            tableLayoutPanel3.SetRowSpan(settingsTreeView, 2);
            settingsTreeView.Size = new Size(194, 600);
            settingsTreeView.TabIndex = 1;
            settingsTreeView.SettingsPageSelected += new System.EventHandler<GitUI.CommandsDialogs.SettingsDialog.SettingsPageSelectedEventArgs>(OnSettingsPageSelected);
            //
            // panelCurrentSettingsPage
            //
            panelCurrentSettingsPage.Dock = DockStyle.Fill;
            panelCurrentSettingsPage.Location = new Point(203, 3);
            panelCurrentSettingsPage.Name = "panelCurrentSettingsPage";
            tableLayoutPanel3.SetRowSpan(panelCurrentSettingsPage, 2);
            panelCurrentSettingsPage.Size = new Size(732, 600);
            panelCurrentSettingsPage.TabIndex = 5;
            //
            // flowLayoutPanel4
            //
            flowLayoutPanel4.AutoSize = true;
            flowLayoutPanel4.Controls.Add(buttonApply);
            flowLayoutPanel4.Controls.Add(buttonDiscard);
            flowLayoutPanel4.Controls.Add(buttonCancel);
            flowLayoutPanel4.Controls.Add(buttonOk);
            flowLayoutPanel4.Controls.Add(labelInstantSaveNotice);
            flowLayoutPanel4.Dock = DockStyle.Right;
            flowLayoutPanel4.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel4.Location = new Point(203, 609);
            flowLayoutPanel4.Name = "flowLayoutPanel4";
            flowLayoutPanel4.Size = new Size(732, 34);
            flowLayoutPanel4.TabIndex = 3;
            flowLayoutPanel4.WrapContents = false;
            //
            // buttonApply
            //
            buttonApply.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonApply.Location = new Point(637, 6);
            buttonApply.Name = "buttonApply";
            buttonApply.Size = new Size(92, 25);
            buttonApply.TabIndex = 3;
            buttonApply.Text = "Apply";
            buttonApply.UseVisualStyleBackColor = true;
            buttonApply.Click += buttonApply_Click;
            //
            // buttonDiscard
            //
            buttonDiscard.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonDiscard.Location = new Point(543, 6);
            buttonDiscard.Name = "buttonDiscard";
            buttonDiscard.Size = new Size(88, 25);
            buttonDiscard.TabIndex = 2;
            buttonDiscard.Text = "Discard";
            buttonDiscard.UseVisualStyleBackColor = true;
            buttonDiscard.Visible = false;
            buttonDiscard.Click += buttonDiscard_Click;
            //
            // buttonCancel
            //
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonCancel.Location = new Point(449, 6);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(88, 25);
            buttonCancel.TabIndex = 1;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            //
            // labelInstantSaveNotice
            //
            labelInstantSaveNotice.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelInstantSaveNotice.AutoSize = true;
            labelInstantSaveNotice.Location = new Point(-29, 2);
            labelInstantSaveNotice.Margin = new Padding(3, 2, 3, 0);
            labelInstantSaveNotice.Name = "labelInstantSaveNotice";
            labelInstantSaveNotice.Size = new Size(382, 32);
            labelInstantSaveNotice.TabIndex = 4;
            labelInstantSaveNotice.Text = "Changes on the selected page will be saved instantly. \r\nTherefore the Cancel butt" +
    "on does NOT revert any changes made.";
            labelInstantSaveNotice.TextAlign = ContentAlignment.MiddleRight;
            //
            // FormSettings
            //
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(958, 646);
            Controls.Add(tableLayoutPanel3);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(966, 785);
            Name = "FormSettings";
            SizeGripStyle = SizeGripStyle.Show;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings";
            Shown += FormSettings_Shown;
            FormClosing += FormSettings_FormClosing;
            ((System.ComponentModel.ISupportInitialize)(pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(repositoryBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(scriptInfoBindingSource)).EndInit();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            flowLayoutPanel4.ResumeLayout(false);
            flowLayoutPanel4.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private Button buttonOk;
        private System.DirectoryServices.DirectorySearcher directorySearcher1;
        private System.DirectoryServices.DirectorySearcher directorySearcher2;
        private Label label10;
        private PictureBox pictureBox2;
        private BindingSource repositoryBindingSource;
#pragma warning disable 0414
        private HelpProvider helpProvider1;
#pragma warning restore 0414
        private BindingSource scriptInfoBindingSource;
        private TableLayoutPanel tableLayoutPanel3;
        private SettingsDialog.SettingsTreeViewUserControl settingsTreeView;
        private FlowLayoutPanel flowLayoutPanel4;
        private Button buttonApply;
        private Button buttonCancel;
        private Panel panelCurrentSettingsPage;
        private Label labelInstantSaveNotice;
        private Button buttonDiscard;

    }
}
