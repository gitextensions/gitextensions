namespace GitExtensions.Plugins.GitlabIntegration.Settings
{
    partial class GitlabSettingsUserControl
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
            tableLayoutPanel1 = new TableLayoutPanel();
            GetProjectIdLink = new LinkLabel();
            TokenManagementLink = new LinkLabel();
            ProjectIdTextBox = new TextBox();
            label1 = new Label();
            InstanceUrlTextBox = new TextBox();
            ApiTokenTextBox = new TextBox();
            label3 = new Label();
            label2 = new Label();
            GetProjectIdStatusText = new Label();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(GetProjectIdLink, 1, 2);
            tableLayoutPanel1.Controls.Add(TokenManagementLink, 1, 5);
            tableLayoutPanel1.Controls.Add(ProjectIdTextBox, 1, 1);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(InstanceUrlTextBox, 1, 0);
            tableLayoutPanel1.Controls.Add(ApiTokenTextBox, 1, 4);
            tableLayoutPanel1.Controls.Add(label3, 0, 1);
            tableLayoutPanel1.Controls.Add(label2, 0, 4);
            tableLayoutPanel1.Controls.Add(GetProjectIdStatusText, 1, 3);
            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 6;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(834, 131);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // GetProjectIdLink
            // 
            GetProjectIdLink.AutoSize = true;
            GetProjectIdLink.Dock = DockStyle.Fill;
            GetProjectIdLink.Location = new Point(82, 54);
            GetProjectIdLink.Name = "GetProjectIdLink";
            GetProjectIdLink.Size = new Size(749, 15);
            GetProjectIdLink.TabIndex = 5;
            GetProjectIdLink.TabStop = true;
            GetProjectIdLink.Text = "Get Project ID from server";
            GetProjectIdLink.LinkClicked += GetProjectIdLink_LinkClicked;
            // 
            // TokenManagementLink
            // 
            TokenManagementLink.AutoSize = true;
            TokenManagementLink.Dock = DockStyle.Fill;
            TokenManagementLink.Location = new Point(82, 116);
            TokenManagementLink.Name = "TokenManagementLink";
            TokenManagementLink.Size = new Size(749, 15);
            TokenManagementLink.TabIndex = 9;
            TokenManagementLink.TabStop = true;
            TokenManagementLink.Text = "Go to token management page";
            TokenManagementLink.LinkClicked += TokenManagementLink_LinkClicked;
            // 
            // ProjectIdTextBox
            // 
            ProjectIdTextBox.Dock = DockStyle.Fill;
            ProjectIdTextBox.Location = new Point(81, 29);
            ProjectIdTextBox.Margin = new Padding(2);
            ProjectIdTextBox.Name = "ProjectIdTextBox";
            ProjectIdTextBox.Size = new Size(751, 23);
            ProjectIdTextBox.TabIndex = 4;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(2, 6);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(75, 15);
            label1.TabIndex = 2;
            label1.Text = "Instance URL";
            // 
            // InstanceUrlTextBox
            // 
            InstanceUrlTextBox.Dock = DockStyle.Fill;
            InstanceUrlTextBox.Location = new Point(81, 2);
            InstanceUrlTextBox.Margin = new Padding(2);
            InstanceUrlTextBox.Name = "InstanceUrlTextBox";
            InstanceUrlTextBox.Size = new Size(751, 23);
            InstanceUrlTextBox.TabIndex = 1;
            InstanceUrlTextBox.TextChanged += InstanceUrlTextBox_TextChanged;
            // 
            // ApiTokenTextBox
            // 
            ApiTokenTextBox.Dock = DockStyle.Fill;
            ApiTokenTextBox.Location = new Point(81, 91);
            ApiTokenTextBox.Margin = new Padding(2);
            ApiTokenTextBox.Name = "ApiTokenTextBox";
            ApiTokenTextBox.Size = new Size(751, 23);
            ApiTokenTextBox.TabIndex = 8;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(2, 33);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(58, 15);
            label3.TabIndex = 3;
            label3.Text = "Project ID";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(2, 95);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(59, 15);
            label2.TabIndex = 7;
            label2.Text = "Api Token";
            // 
            // GetProjectIdStatusText
            // 
            GetProjectIdStatusText.Anchor = AnchorStyles.Left;
            GetProjectIdStatusText.AutoSize = true;
            GetProjectIdStatusText.Location = new Point(81, 71);
            GetProjectIdStatusText.Margin = new Padding(2, 0, 2, 0);
            GetProjectIdStatusText.Name = "GetProjectIdStatusText";
            GetProjectIdStatusText.Size = new Size(468, 15);
            GetProjectIdStatusText.TabIndex = 6;
            GetProjectIdStatusText.Text = "Failed to obtain project from server. Try to specify valid API token or check instance URL";
            GetProjectIdStatusText.Visible = false;
            // 
            // GitlabSettingsUserControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(2);
            Name = "GitlabSettingsUserControl";
            Size = new Size(834, 133);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TextBox InstanceUrlTextBox;
        private Label label1;
        private TextBox ApiTokenTextBox;
        private TextBox ProjectIdTextBox;
        private Label label3;
        private Label label2;
        private LinkLabel TokenManagementLink;
        private LinkLabel GetProjectIdLink;
        private Label GetProjectIdStatusText;
    }
}
