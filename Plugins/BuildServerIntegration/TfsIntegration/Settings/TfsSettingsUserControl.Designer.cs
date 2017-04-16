namespace TfsIntegration.Settings
{
    partial class TfsSettingsUserControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label label13;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label labelBuildIdFilter;
            this.labelRegexError = new System.Windows.Forms.Label();
            this.TfsServer = new System.Windows.Forms.TextBox();
            this.TfsProjectName = new System.Windows.Forms.TextBox();
            this.TfsTeamCollectionName = new System.Windows.Forms.TextBox();
            this.TfsBuildDefinitionNameFilter = new System.Windows.Forms.TextBox();
            label13 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            labelBuildIdFilter = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(3, 10);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(129, 13);
            label13.TabIndex = 0;
            label13.Text = "Tfs server (Name or URL)";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 39);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(109, 13);
            label1.TabIndex = 2;
            label1.Text = "Team collection name";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(3, 68);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(70, 13);
            label2.TabIndex = 2;
            label2.Text = "Project name";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(3, 90);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(126, 26);
            label3.TabIndex = 2;
            label3.Text = "Build definition name\r\n(all existing if left empty)";
            // 
            // labelBuildIdFilter
            // 
            labelBuildIdFilter.AutoSize = true;
            labelBuildIdFilter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            labelBuildIdFilter.Location = new System.Drawing.Point(136, 118);
            labelBuildIdFilter.Name = "labelBuildIdFilter";
            labelBuildIdFilter.Size = new System.Drawing.Size(45, 15);
            labelBuildIdFilter.TabIndex = 8;
            labelBuildIdFilter.Text = "Regexp";
            // 
            // labelRegexError
            // 
            this.labelRegexError.AutoSize = true;
            this.labelRegexError.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.labelRegexError.ForeColor = System.Drawing.Color.Red;
            this.labelRegexError.Location = new System.Drawing.Point(136, 143);
            this.labelRegexError.Name = "labelRegexError";
            this.labelRegexError.Size = new System.Drawing.Size(416, 15);
            this.labelRegexError.TabIndex = 9;
            this.labelRegexError.Text = "The \'Build definition name\' regular expression is not valid and won\'t be saved!";
            this.labelRegexError.Visible = false;
            // 
            // TfsServer
            // 
            this.TfsServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TfsServer.Location = new System.Drawing.Point(139, 7);
            this.TfsServer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TfsServer.Name = "TfsServer";
            this.TfsServer.Size = new System.Drawing.Size(426, 21);
            this.TfsServer.TabIndex = 1;
            // 
            // TfsProjectName
            // 
            this.TfsProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TfsProjectName.Location = new System.Drawing.Point(139, 66);
            this.TfsProjectName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TfsProjectName.Name = "TfsProjectName";
            this.TfsProjectName.Size = new System.Drawing.Size(426, 21);
            this.TfsProjectName.TabIndex = 3;
            // 
            // TfsTeamCollectionName
            // 
            this.TfsTeamCollectionName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TfsTeamCollectionName.Location = new System.Drawing.Point(139, 37);
            this.TfsTeamCollectionName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TfsTeamCollectionName.Name = "TfsTeamCollectionName";
            this.TfsTeamCollectionName.Size = new System.Drawing.Size(426, 21);
            this.TfsTeamCollectionName.TabIndex = 2;
            // 
            // TfsBuildDefinitionNameFilter
            // 
            this.TfsBuildDefinitionNameFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TfsBuildDefinitionNameFilter.Location = new System.Drawing.Point(139, 95);
            this.TfsBuildDefinitionNameFilter.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TfsBuildDefinitionNameFilter.Name = "TfsBuildDefinitionNameFilter";
            this.TfsBuildDefinitionNameFilter.Size = new System.Drawing.Size(426, 21);
            this.TfsBuildDefinitionNameFilter.TabIndex = 4;
            this.TfsBuildDefinitionNameFilter.TextChanged += new System.EventHandler(this.TfsBuildDefinitionNameFilter_TextChanged);
            // 
            // TfsSettingsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.labelRegexError);
            this.Controls.Add(labelBuildIdFilter);
            this.Controls.Add(label3);
            this.Controls.Add(label2);
            this.Controls.Add(label1);
            this.Controls.Add(label13);
            this.Controls.Add(this.TfsBuildDefinitionNameFilter);
            this.Controls.Add(this.TfsTeamCollectionName);
            this.Controls.Add(this.TfsProjectName);
            this.Controls.Add(this.TfsServer);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "TfsSettingsUserControl";
            this.Size = new System.Drawing.Size(571, 176);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TfsServer;
        private System.Windows.Forms.TextBox TfsProjectName;
        private System.Windows.Forms.TextBox TfsTeamCollectionName;
        private System.Windows.Forms.TextBox TfsBuildDefinitionNameFilter;
        private System.Windows.Forms.Label labelRegexError;
    }
}
