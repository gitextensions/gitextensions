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
            if (disposing && (components is not null))
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
            Label label13;
            Label label3;
            Label labelBuildIdFilter;
            Label RestApiTokenLabel;
            labelRegexError = new Label();
            TfsServer = new TextBox();
            TfsBuildDefinitionNameFilter = new TextBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            label4 = new Label();
            ExtractLink = new LinkLabel();
            RestApiToken = new TextBox();
            TokenManagementLink = new LinkLabel();
            label1 = new Label();
            label13 = new Label();
            label3 = new Label();
            labelBuildIdFilter = new Label();
            RestApiTokenLabel = new Label();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // label13
            // 
            label13.Anchor = AnchorStyles.Left;
            label13.AutoSize = true;
            label13.Location = new Point(3, 18);
            label13.Name = "label13";
            label13.Size = new Size(56, 13);
            label13.TabIndex = 0;
            label13.Text = "Project Url";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(3, 114);
            label3.Name = "label3";
            label3.Size = new Size(117, 26);
            label3.TabIndex = 2;
            label3.Text = "Build definition name\r\n(all existing if left empty)";
            // 
            // labelBuildIdFilter
            // 
            labelBuildIdFilter.AutoSize = true;
            labelBuildIdFilter.Dock = DockStyle.Fill;
            labelBuildIdFilter.Location = new Point(126, 140);
            labelBuildIdFilter.Name = "labelBuildIdFilter";
            labelBuildIdFilter.Size = new Size(1100, 13);
            labelBuildIdFilter.TabIndex = 8;
            labelBuildIdFilter.Text = "If needed, use the \'*\' wildcard or enter a Regular Expression";
            // 
            // RestApiTokenLabel
            // 
            RestApiTokenLabel.Anchor = AnchorStyles.Left;
            RestApiTokenLabel.AutoSize = true;
            RestApiTokenLabel.Location = new Point(3, 201);
            RestApiTokenLabel.Name = "RestApiTokenLabel";
            RestApiTokenLabel.Size = new Size(81, 13);
            RestApiTokenLabel.TabIndex = 12;
            RestApiTokenLabel.Text = "Rest Api Token";
            // 
            // labelRegexError
            // 
            labelRegexError.AutoSize = true;
            labelRegexError.Dock = DockStyle.Fill;
            labelRegexError.ForeColor = Color.Red;
            labelRegexError.Location = new Point(126, 158);
            labelRegexError.Margin = new Padding(3, 5, 3, 5);
            labelRegexError.Name = "labelRegexError";
            labelRegexError.Size = new Size(1100, 13);
            labelRegexError.TabIndex = 9;
            labelRegexError.Text = "The \'Build definition name\' regular expression is not valid and won\'t be saved!";
            labelRegexError.Visible = false;
            // 
            // TfsServer
            // 
            TfsServer.Dock = DockStyle.Fill;
            TfsServer.Location = new Point(126, 15);
            TfsServer.Margin = new Padding(3, 2, 3, 2);
            TfsServer.Name = "TfsServer";
            TfsServer.Size = new Size(1100, 20);
            TfsServer.TabIndex = 2;
            TfsServer.TextChanged += TextBox_TextChanged;
            // 
            // TfsBuildDefinitionNameFilter
            // 
            TfsBuildDefinitionNameFilter.Dock = DockStyle.Fill;
            TfsBuildDefinitionNameFilter.Location = new Point(126, 116);
            TfsBuildDefinitionNameFilter.Margin = new Padding(3, 2, 3, 2);
            TfsBuildDefinitionNameFilter.Name = "TfsBuildDefinitionNameFilter";
            TfsBuildDefinitionNameFilter.Size = new Size(1100, 20);
            TfsBuildDefinitionNameFilter.TabIndex = 3;
            TfsBuildDefinitionNameFilter.TextChanged += TextBox_TextChanged;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(label4, 1, 10);
            tableLayoutPanel1.Controls.Add(ExtractLink, 1, 0);
            tableLayoutPanel1.Controls.Add(RestApiTokenLabel, 0, 8);
            tableLayoutPanel1.Controls.Add(RestApiToken, 1, 8);
            tableLayoutPanel1.Controls.Add(label13, 0, 1);
            tableLayoutPanel1.Controls.Add(TfsServer, 1, 1);
            tableLayoutPanel1.Controls.Add(labelBuildIdFilter, 1, 5);
            tableLayoutPanel1.Controls.Add(TfsBuildDefinitionNameFilter, 1, 4);
            tableLayoutPanel1.Controls.Add(label3, 0, 4);
            tableLayoutPanel1.Controls.Add(labelRegexError, 1, 6);
            tableLayoutPanel1.Controls.Add(TokenManagementLink, 1, 9);
            tableLayoutPanel1.Controls.Add(label1, 1, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 11;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(1229, 272);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = DockStyle.Fill;
            label4.Location = new Point(126, 233);
            label4.Name = "label4";
            label4.Size = new Size(1100, 39);
            label4.TabIndex = 14;
            label4.Text = "You need to create a token with the following scopes:\r\n - \'Build (read)\'\r\n - \'Pro" +
    "ject and team (read)\'";
            // 
            // ExtractLink
            // 
            ExtractLink.AutoSize = true;
            ExtractLink.Dock = DockStyle.Fill;
            ExtractLink.Location = new Point(126, 0);
            ExtractLink.Name = "ExtractLink";
            ExtractLink.Size = new Size(1100, 13);
            ExtractLink.TabIndex = 1;
            ExtractLink.TabStop = true;
            ExtractLink.Text = "Extract data from a build result url copied in the clipboard";
            ExtractLink.LinkClicked += ExtractLink_LinkClicked;
            // 
            // RestApiToken
            // 
            RestApiToken.Dock = DockStyle.Fill;
            RestApiToken.Location = new Point(126, 198);
            RestApiToken.Margin = new Padding(3, 2, 3, 2);
            RestApiToken.Name = "RestApiToken";
            RestApiToken.Size = new Size(1100, 20);
            RestApiToken.TabIndex = 4;
            RestApiToken.TextChanged += TextBox_TextChanged;
            // 
            // TokenManagementLink
            // 
            TokenManagementLink.AutoSize = true;
            TokenManagementLink.Dock = DockStyle.Fill;
            TokenManagementLink.Location = new Point(126, 220);
            TokenManagementLink.Name = "TokenManagementLink";
            TokenManagementLink.Size = new Size(1100, 13);
            TokenManagementLink.TabIndex = 5;
            TokenManagementLink.TabStop = true;
            TokenManagementLink.Text = "Go to token management page";
            TokenManagementLink.Click += RestApiTokenLink_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(126, 37);
            label1.Margin = new Padding(3, 0, 3, 5);
            label1.Name = "label1";
            label1.Size = new Size(1100, 52);
            label1.TabIndex = 13;
            label1.Text = "Examples:\r\n - https://dev.azure.com/yourorganization/projectname/\r\n - https://you" +
    "rhost:8080/tfs/collectionname/projectname/\r\n - https://yourorganization.visualst" +
    "udio.com/projectname/";
            // 
            // SettingsUserControl
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(0);
            Name = "SettingsUserControl";
            Size = new Size(1229, 272);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TextBox TfsServer;
        private TextBox TfsBuildDefinitionNameFilter;
        private Label labelRegexError;
        private TableLayoutPanel tableLayoutPanel1;
        private TextBox RestApiToken;
        private LinkLabel TokenManagementLink;
        private Label label4;
        private Label label1;
        private LinkLabel ExtractLink;
    }
}
