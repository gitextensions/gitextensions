namespace VstsAndTfsIntegration.Settings
{
    partial class VstsAndTfsSettingsUserControl
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
            this.RestApiToken = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.RestApiTokenLink = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            label13 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            labelBuildIdFilter = new System.Windows.Forms.Label();
            RestApiTokenLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label13
            // 
            label13.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(3, 5);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(56, 13);
            label13.TabIndex = 0;
            label13.Text = "Project Url";
            // 
            // label3
            // 
            label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(3, 81);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(117, 26);
            label3.TabIndex = 2;
            label3.Text = "Build definition name\r\n(all existing if left empty)";
            // 
            // labelBuildIdFilter
            // 
            labelBuildIdFilter.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelBuildIdFilter.AutoSize = true;
            labelBuildIdFilter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            labelBuildIdFilter.Location = new System.Drawing.Point(126, 107);
            labelBuildIdFilter.Name = "labelBuildIdFilter";
            labelBuildIdFilter.Size = new System.Drawing.Size(316, 15);
            labelBuildIdFilter.TabIndex = 8;
            labelBuildIdFilter.Text = "If needed, use the \'*\' wildcard or enter a Regular Expression";
            // 
            // RestApiTokenLabel
            // 
            RestApiTokenLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            RestApiTokenLabel.AutoSize = true;
            RestApiTokenLabel.Location = new System.Drawing.Point(3, 152);
            RestApiTokenLabel.Name = "RestApiTokenLabel";
            RestApiTokenLabel.Size = new System.Drawing.Size(81, 13);
            RestApiTokenLabel.TabIndex = 12;
            RestApiTokenLabel.Text = "Rest Api Token";
            // 
            // labelRegexError
            // 
            this.labelRegexError.AutoSize = true;
            this.labelRegexError.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.labelRegexError.ForeColor = System.Drawing.Color.Red;
            this.labelRegexError.Location = new System.Drawing.Point(126, 127);
            this.labelRegexError.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.labelRegexError.Name = "labelRegexError";
            this.labelRegexError.Size = new System.Drawing.Size(416, 15);
            this.labelRegexError.TabIndex = 9;
            this.labelRegexError.Text = "The \'Build definition name\' regular expression is not valid and won\'t be saved!";
            this.labelRegexError.Visible = false;
            // 
            // TfsServer
            // 
            this.TfsServer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TfsServer.Location = new System.Drawing.Point(126, 2);
            this.TfsServer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TfsServer.Name = "TfsServer";
            this.TfsServer.Size = new System.Drawing.Size(1163, 20);
            this.TfsServer.TabIndex = 0;
            this.TfsServer.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // TfsBuildDefinitionNameFilter
            // 
            this.TfsBuildDefinitionNameFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TfsBuildDefinitionNameFilter.Location = new System.Drawing.Point(126, 83);
            this.TfsBuildDefinitionNameFilter.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TfsBuildDefinitionNameFilter.Name = "TfsBuildDefinitionNameFilter";
            this.TfsBuildDefinitionNameFilter.Size = new System.Drawing.Size(1163, 20);
            this.TfsBuildDefinitionNameFilter.TabIndex = 3;
            this.TfsBuildDefinitionNameFilter.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(RestApiTokenLabel, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.RestApiToken, 1, 6);
            this.tableLayoutPanel1.Controls.Add(label13, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.TfsServer, 1, 0);
            this.tableLayoutPanel1.Controls.Add(labelBuildIdFilter, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.TfsBuildDefinitionNameFilter, 1, 3);
            this.tableLayoutPanel1.Controls.Add(label3, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.labelRegexError, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.label1, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 8;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1292, 398);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // RestApiToken
            // 
            this.RestApiToken.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RestApiToken.Location = new System.Drawing.Point(126, 149);
            this.RestApiToken.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.RestApiToken.Name = "RestApiToken";
            this.RestApiToken.Size = new System.Drawing.Size(1163, 20);
            this.RestApiToken.TabIndex = 4;
            this.RestApiToken.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.RestApiTokenLink);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(126, 174);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1163, 221);
            this.panel1.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(261, 39);
            this.label4.TabIndex = 14;
            this.label4.Text = "You need to create a token with the following scopes:\r\n - \'Build (read)\'\r\n - \'Pro" +
    "ject and team (read)\'";
            // 
            // RestApiTokenLink
            // 
            this.RestApiTokenLink.AutoSize = true;
            this.RestApiTokenLink.Location = new System.Drawing.Point(-5, 0);
            this.RestApiTokenLink.Name = "RestApiTokenLink";
            this.RestApiTokenLink.Size = new System.Drawing.Size(154, 13);
            this.RestApiTokenLink.TabIndex = 1;
            this.RestApiTokenLink.TabStop = true;
            this.RestApiTokenLink.Text = "Go to token management page";
            this.RestApiTokenLink.Click += new System.EventHandler(this.RestApiTokenLink_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(126, 24);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1163, 52);
            this.label1.TabIndex = 13;
            this.label1.Text = "Examples:\r\n - https://example.visualstudio.com/projectname/\r\n - https://dev.azure" +
    ".com/example/projectname/\r\n - https://yourhost:8080/tfs/collectionname/projectna" +
    "me/\r\n";
            // 
            // VstsAndTfsSettingsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "VstsAndTfsSettingsUserControl";
            this.Size = new System.Drawing.Size(1295, 401);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox TfsServer;
        private System.Windows.Forms.TextBox TfsBuildDefinitionNameFilter;
        private System.Windows.Forms.Label labelRegexError;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox RestApiToken;
        private System.Windows.Forms.LinkLabel RestApiTokenLink;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
    }
}
