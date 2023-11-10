namespace TeamCityIntegration.Settings
{
    partial class TeamCityBuildChooser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TeamCityBuildChooser));
            treeViewTeamCityProjects = new TreeView();
            buttonOK = new Button();
            buttonCancel = new Button();
            SuspendLayout();
            // 
            // treeViewTeamCityProjects
            // 
            treeViewTeamCityProjects.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeViewTeamCityProjects.Location = new Point(9, 10);
            treeViewTeamCityProjects.Margin = new Padding(2, 2, 2, 2);
            treeViewTeamCityProjects.Name = "treeViewTeamCityProjects";
            treeViewTeamCityProjects.Size = new Size(434, 323);
            treeViewTeamCityProjects.TabIndex = 14;
            treeViewTeamCityProjects.BeforeExpand += treeViewTeamCityProjects_BeforeExpand;
            treeViewTeamCityProjects.AfterSelect += treeViewTeamCityProjects_AfterSelect;
            treeViewTeamCityProjects.MouseDoubleClick += treeViewTeamCityProjects_MouseDoubleClick;
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Enabled = false;
            buttonOK.Location = new Point(274, 346);
            buttonOK.Margin = new Padding(2, 2, 2, 2);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(56, 19);
            buttonOK.TabIndex = 15;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += buttonOK_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(121, 346);
            buttonCancel.Margin = new Padding(2, 2, 2, 2);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(56, 19);
            buttonCancel.TabIndex = 15;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // TeamCityBuildChooser
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = buttonCancel;
            ClientSize = new Size(451, 379);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(treeViewTeamCityProjects);
            Margin = new Padding(2, 2, 2, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TeamCityBuildChooser";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Choose the TeamCity build...";
            Load += TeamCityBuildChooser_Load;
            ResumeLayout(false);

        }

        #endregion

        private TreeView treeViewTeamCityProjects;
        private Button buttonOK;
        private Button buttonCancel;
    }
}