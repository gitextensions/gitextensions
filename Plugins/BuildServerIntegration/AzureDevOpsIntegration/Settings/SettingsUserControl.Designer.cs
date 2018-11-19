namespace AzureDevOpsIntegration.Settings
{
    partial class SettingsUserControl
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
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label labelBuildIdFilter;
            System.Windows.Forms.Label RestApiTokenLabel;
            this.labelRegexError = new System.Windows.Forms.Label();
            this.TfsServer = new System.Windows.Forms.TextBox();
            this.TfsBuildDefinitionNameFilter = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.ExtractLink = new System.Windows.Forms.LinkLabel();
            this.RestApiToken = new System.Windows.Forms.TextBox();
            this.TokenManagementLink = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            label13 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            labelBuildIdFilter = new System.Windows.Forms.Label();
            RestApiTokenLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label13
            // 
            label13.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(3, 18);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(56, 13);
            label13.TabIndex = 0;
            label13.Text = "Project Url";
            // 
            // label3
            // 
            label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(3, 114);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(117, 26);
            label3.TabIndex = 2;
            label3.Text = "Build definition name\r\n(all existing if left empty)";
            // 
            // labelBuildIdFilter
            // 
            labelBuildIdFilter.AutoSize = true;
            labelBuildIdFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            labelBuildIdFilter.Location = new System.Drawing.Point(126, 140);
            labelBuildIdFilter.Name = "labelBuildIdFilter";
            labelBuildIdFilter.Size = new System.Drawing.Size(1100, 13);
            labelBuildIdFilter.TabIndex = 8;
            labelBuildIdFilter.Text = "If needed, use the \'*\' wildcard or enter a Regular Expression";
            // 
            // RestApiTokenLabel
            // 
            RestApiTokenLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            RestApiTokenLabel.AutoSize = true;
            RestApiTokenLabel.Location = new System.Drawing.Point(3, 201);
            RestApiTokenLabel.Name = "RestApiTokenLabel";
            RestApiTokenLabel.Size = new System.Drawing.Size(81, 13);
            RestApiTokenLabel.TabIndex = 12;
            RestApiTokenLabel.Text = "Rest Api Token";
            // 
            // labelRegexError
            // 
            this.labelRegexError.AutoSize = true;
            this.labelRegexError.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelRegexError.ForeColor = System.Drawing.Color.Red;
            this.labelRegexError.Location = new System.Drawing.Point(126, 158);
            this.labelRegexError.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.labelRegexError.Name = "labelRegexError";
            this.labelRegexError.Size = new System.Drawing.Size(1100, 13);
            this.labelRegexError.TabIndex = 9;
            this.labelRegexError.Text = "The \'Build definition name\' regular expression is not valid and won\'t be saved!";
            this.labelRegexError.Visible = false;
            // 
            // TfsServer
            // 
            this.TfsServer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TfsServer.Location = new System.Drawing.Point(126, 15);
            this.TfsServer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TfsServer.Name = "TfsServer";
            this.TfsServer.Size = new System.Drawing.Size(1100, 20);
            this.TfsServer.TabIndex = 0;
            this.TfsServer.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // TfsBuildDefinitionNameFilter
            // 
            this.TfsBuildDefinitionNameFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TfsBuildDefinitionNameFilter.Location = new System.Drawing.Point(126, 116);
            this.TfsBuildDefinitionNameFilter.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TfsBuildDefinitionNameFilter.Name = "TfsBuildDefinitionNameFilter";
            this.TfsBuildDefinitionNameFilter.Size = new System.Drawing.Size(1100, 20);
            this.TfsBuildDefinitionNameFilter.TabIndex = 3;
            this.TfsBuildDefinitionNameFilter.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label4, 1, 10);
            this.tableLayoutPanel1.Controls.Add(this.ExtractLink, 1, 0);
            this.tableLayoutPanel1.Controls.Add(RestApiTokenLabel, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.RestApiToken, 1, 8);
            this.tableLayoutPanel1.Controls.Add(label13, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.TfsServer, 1, 1);
            this.tableLayoutPanel1.Controls.Add(labelBuildIdFilter, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.TfsBuildDefinitionNameFilter, 1, 4);
            this.tableLayoutPanel1.Controls.Add(label3, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.labelRegexError, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.TokenManagementLink, 1, 9);
            this.tableLayoutPanel1.Controls.Add(this.label1, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 11;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1229, 272);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(126, 233);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(1100, 39);
            this.label4.TabIndex = 14;
            this.label4.Text = "You need to create a token with the following scopes:\r\n - \'Build (read)\'\r\n - \'Pro" +
    "ject and team (read)\'";
            // 
            // ExtractLink
            // 
            this.ExtractLink.AutoSize = true;
            this.ExtractLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExtractLink.Location = new System.Drawing.Point(126, 0);
            this.ExtractLink.Name = "ExtractLink";
            this.ExtractLink.Size = new System.Drawing.Size(1100, 13);
            this.ExtractLink.TabIndex = 15;
            this.ExtractLink.TabStop = true;
            this.ExtractLink.Text = "Extract data from a build result url copied in the clipboard";
            this.ExtractLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ExtractLink_LinkClicked);
            // 
            // RestApiToken
            // 
            this.RestApiToken.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RestApiToken.Location = new System.Drawing.Point(126, 198);
            this.RestApiToken.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.RestApiToken.Name = "RestApiToken";
            this.RestApiToken.Size = new System.Drawing.Size(1100, 20);
            this.RestApiToken.TabIndex = 4;
            this.RestApiToken.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // TokenManagementLink
            // 
            this.TokenManagementLink.AutoSize = true;
            this.TokenManagementLink.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TokenManagementLink.Location = new System.Drawing.Point(126, 220);
            this.TokenManagementLink.Name = "TokenManagementLink";
            this.TokenManagementLink.Size = new System.Drawing.Size(1100, 13);
            this.TokenManagementLink.TabIndex = 1;
            this.TokenManagementLink.TabStop = true;
            this.TokenManagementLink.Text = "Go to token management page";
            this.TokenManagementLink.Click += new System.EventHandler(this.RestApiTokenLink_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(126, 37);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1100, 52);
            this.label1.TabIndex = 13;
            this.label1.Text = "Examples:\r\n - https://dev.azure.com/yourorganization/projectname/\r\n - https://you" +
    "rhost:8080/tfs/collectionname/projectname/\r\n - https://yourorganization.visualst" +
    "udio.com/projectname/";
            // 
            // SettingsUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "SettingsUserControl";
            this.Size = new System.Drawing.Size(1229, 272);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TfsServer;
        private System.Windows.Forms.TextBox TfsBuildDefinitionNameFilter;
        private System.Windows.Forms.Label labelRegexError;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox RestApiToken;
        private System.Windows.Forms.LinkLabel TokenManagementLink;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel ExtractLink;
    }
}
