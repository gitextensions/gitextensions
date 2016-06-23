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
            // TfsServer
            // 
            this.TfsServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TfsServer.Location = new System.Drawing.Point(162, 8);
            this.TfsServer.Name = "TfsServer";
            this.TfsServer.Size = new System.Drawing.Size(380, 23);
            this.TfsServer.TabIndex = 1;
            // 
            // TfsProjectName
            // 
            this.TfsProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TfsProjectName.Location = new System.Drawing.Point(162, 76);
            this.TfsProjectName.Name = "TfsProjectName";
            this.TfsProjectName.Size = new System.Drawing.Size(380, 23);
            this.TfsProjectName.TabIndex = 3;
            // 
            // TfsTeamCollectionName
            // 
            this.TfsTeamCollectionName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TfsTeamCollectionName.Location = new System.Drawing.Point(162, 42);
            this.TfsTeamCollectionName.Name = "TfsTeamCollectionName";
            this.TfsTeamCollectionName.Size = new System.Drawing.Size(380, 23);
            this.TfsTeamCollectionName.TabIndex = 2;
            // 
            // TfsBuildDefinitionNameFilter
            // 
            this.TfsBuildDefinitionNameFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TfsBuildDefinitionNameFilter.Location = new System.Drawing.Point(162, 110);
            this.TfsBuildDefinitionNameFilter.Name = "TfsBuildDefinitionNameFilter";
            this.TfsBuildDefinitionNameFilter.Size = new System.Drawing.Size(380, 23);
            this.TfsBuildDefinitionNameFilter.TabIndex = 4;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(3, 11);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(138, 15);
            label13.TabIndex = 0;
            label13.Text = "Tfs server (Name or URL)";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 45);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(125, 15);
            label1.TabIndex = 2;
            label1.Text = "Team collection name";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(3, 79);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(77, 15);
            label2.TabIndex = 2;
            label2.Text = "Project name";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(3, 104);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(137, 30);
            label3.TabIndex = 2;
            label3.Text = "Build definition name\r\n(all existing if left empty)";
            // 
            // labelBuildIdFilter
            // 
            labelBuildIdFilter.AutoSize = true;
            labelBuildIdFilter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            labelBuildIdFilter.Location = new System.Drawing.Point(159, 136);
            labelBuildIdFilter.Name = "labelBuildIdFilter";
            labelBuildIdFilter.Size = new System.Drawing.Size(45, 15);
            labelBuildIdFilter.TabIndex = 8;
            labelBuildIdFilter.Text = "Regexp";
            // 
            // TfsSettingsUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
            this.Size = new System.Drawing.Size(550, 157);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TfsServer;
        private System.Windows.Forms.TextBox TfsProjectName;
        private System.Windows.Forms.TextBox TfsTeamCollectionName;
        private System.Windows.Forms.TextBox TfsBuildDefinitionNameFilter;
    }
}
