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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            label13 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            labelBuildIdFilter = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label13
            // 
            label13.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(3, 6);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(129, 13);
            label13.TabIndex = 0;
            label13.Text = "Tfs server (Name or URL)";
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 31);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(109, 13);
            label1.TabIndex = 2;
            label1.Text = "Team collection name";
            // 
            // label2
            // 
            label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(3, 56);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(70, 13);
            label2.TabIndex = 2;
            label2.Text = "Project name";
            // 
            // label3
            // 
            label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(3, 75);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(126, 26);
            label3.TabIndex = 2;
            label3.Text = "Build definition name\r\n(all existing if left empty)";
            // 
            // labelBuildIdFilter
            // 
            labelBuildIdFilter.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelBuildIdFilter.AutoSize = true;
            labelBuildIdFilter.Location = new System.Drawing.Point(138, 101);
            labelBuildIdFilter.Name = "labelBuildIdFilter";
            labelBuildIdFilter.Size = new System.Drawing.Size(45, 15);
            labelBuildIdFilter.TabIndex = 8;
            labelBuildIdFilter.Text = "Regexp";
            // 
            // labelRegexError
            // 
            this.labelRegexError.AutoSize = true;
            this.labelRegexError.ForeColor = System.Drawing.Color.Red;
            this.labelRegexError.Location = new System.Drawing.Point(138, 121);
            this.labelRegexError.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.labelRegexError.Name = "labelRegexError";
            this.labelRegexError.Size = new System.Drawing.Size(416, 15);
            this.labelRegexError.TabIndex = 9;
            this.labelRegexError.Text = "The \'Build definition name\' regular expression is not valid and won\'t be saved!";
            this.labelRegexError.Visible = false;
            // 
            // TfsServer
            // 
            this.TfsServer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TfsServer.Location = new System.Drawing.Point(138, 2);
            this.TfsServer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TfsServer.Name = "TfsServer";
            this.TfsServer.Size = new System.Drawing.Size(426, 21);
            this.TfsServer.TabIndex = 1;
            // 
            // TfsProjectName
            // 
            this.TfsProjectName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TfsProjectName.Location = new System.Drawing.Point(138, 52);
            this.TfsProjectName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TfsProjectName.Name = "TfsProjectName";
            this.TfsProjectName.Size = new System.Drawing.Size(426, 21);
            this.TfsProjectName.TabIndex = 3;
            // 
            // TfsTeamCollectionName
            // 
            this.TfsTeamCollectionName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TfsTeamCollectionName.Location = new System.Drawing.Point(138, 27);
            this.TfsTeamCollectionName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TfsTeamCollectionName.Name = "TfsTeamCollectionName";
            this.TfsTeamCollectionName.Size = new System.Drawing.Size(426, 21);
            this.TfsTeamCollectionName.TabIndex = 2;
            // 
            // TfsBuildDefinitionNameFilter
            // 
            this.TfsBuildDefinitionNameFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TfsBuildDefinitionNameFilter.Location = new System.Drawing.Point(138, 77);
            this.TfsBuildDefinitionNameFilter.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TfsBuildDefinitionNameFilter.Name = "TfsBuildDefinitionNameFilter";
            this.TfsBuildDefinitionNameFilter.Size = new System.Drawing.Size(426, 21);
            this.TfsBuildDefinitionNameFilter.TabIndex = 4;
            this.TfsBuildDefinitionNameFilter.TextChanged += new System.EventHandler(this.TfsBuildDefinitionNameFilter_TextChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(label13, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.TfsServer, 1, 0);
            this.tableLayoutPanel1.Controls.Add(labelBuildIdFilter, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.TfsTeamCollectionName, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.TfsBuildDefinitionNameFilter, 1, 3);
            this.tableLayoutPanel1.Controls.Add(label3, 0, 3);
            this.tableLayoutPanel1.Controls.Add(label1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.TfsProjectName, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.labelRegexError, 1, 5);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(567, 136);
            this.tableLayoutPanel1.TabIndex = 10;
            // 
            // TfsSettingsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "TfsSettingsUserControl";
            this.Size = new System.Drawing.Size(570, 139);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TfsServer;
        private System.Windows.Forms.TextBox TfsProjectName;
        private System.Windows.Forms.TextBox TfsTeamCollectionName;
        private System.Windows.Forms.TextBox TfsBuildDefinitionNameFilter;
        private System.Windows.Forms.Label labelRegexError;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
