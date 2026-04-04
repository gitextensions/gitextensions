namespace GitExtensions.Plugins.GitHubActionsIntegration.Settings;

partial class GitHubActionsSettingsUserControl
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Component Designer generated code

    private void InitializeComponent()
    {
        tableLayoutPanel1 = new TableLayoutPanel();
        labelApiUrl = new Label();
        ApiUrlTextBox = new TextBox();
        labelOwner = new Label();
        OwnerTextBox = new TextBox();
        labelRepository = new Label();
        RepositoryTextBox = new TextBox();
        labelApiToken = new Label();
        ApiTokenTextBox = new TextBox();
        TokenManagementLink = new LinkLabel();
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
        tableLayoutPanel1.Controls.Add(labelApiUrl, 0, 0);
        tableLayoutPanel1.Controls.Add(ApiUrlTextBox, 1, 0);
        tableLayoutPanel1.Controls.Add(labelOwner, 0, 1);
        tableLayoutPanel1.Controls.Add(OwnerTextBox, 1, 1);
        tableLayoutPanel1.Controls.Add(labelRepository, 0, 2);
        tableLayoutPanel1.Controls.Add(RepositoryTextBox, 1, 2);
        tableLayoutPanel1.Controls.Add(labelApiToken, 0, 3);
        tableLayoutPanel1.Controls.Add(ApiTokenTextBox, 1, 3);
        tableLayoutPanel1.Controls.Add(TokenManagementLink, 1, 4);
        tableLayoutPanel1.Dock = DockStyle.Top;
        tableLayoutPanel1.Location = new Point(0, 0);
        tableLayoutPanel1.Margin = new Padding(2);
        tableLayoutPanel1.Name = "tableLayoutPanel1";
        tableLayoutPanel1.RowCount = 5;
        tableLayoutPanel1.RowStyles.Add(new RowStyle());
        tableLayoutPanel1.RowStyles.Add(new RowStyle());
        tableLayoutPanel1.RowStyles.Add(new RowStyle());
        tableLayoutPanel1.RowStyles.Add(new RowStyle());
        tableLayoutPanel1.RowStyles.Add(new RowStyle());
        tableLayoutPanel1.Size = new Size(834, 123);
        tableLayoutPanel1.TabIndex = 0;
        //
        // labelApiUrl
        //
        labelApiUrl.Anchor = AnchorStyles.Left;
        labelApiUrl.AutoSize = true;
        labelApiUrl.Location = new Point(2, 6);
        labelApiUrl.Margin = new Padding(2, 0, 2, 0);
        labelApiUrl.Name = "labelApiUrl";
        labelApiUrl.Size = new Size(50, 15);
        labelApiUrl.TabIndex = 0;
        labelApiUrl.Text = "API URL";
        //
        // ApiUrlTextBox
        //
        ApiUrlTextBox.Dock = DockStyle.Fill;
        ApiUrlTextBox.Location = new Point(81, 2);
        ApiUrlTextBox.Margin = new Padding(2);
        ApiUrlTextBox.Name = "ApiUrlTextBox";
        ApiUrlTextBox.Size = new Size(751, 23);
        ApiUrlTextBox.TabIndex = 1;
        //
        // labelOwner
        //
        labelOwner.Anchor = AnchorStyles.Left;
        labelOwner.AutoSize = true;
        labelOwner.Location = new Point(2, 33);
        labelOwner.Margin = new Padding(2, 0, 2, 0);
        labelOwner.Name = "labelOwner";
        labelOwner.Size = new Size(42, 15);
        labelOwner.TabIndex = 2;
        labelOwner.Text = "Owner";
        //
        // OwnerTextBox
        //
        OwnerTextBox.Dock = DockStyle.Fill;
        OwnerTextBox.Location = new Point(81, 29);
        OwnerTextBox.Margin = new Padding(2);
        OwnerTextBox.Name = "OwnerTextBox";
        OwnerTextBox.Size = new Size(751, 23);
        OwnerTextBox.TabIndex = 3;
        //
        // labelRepository
        //
        labelRepository.Anchor = AnchorStyles.Left;
        labelRepository.AutoSize = true;
        labelRepository.Location = new Point(2, 60);
        labelRepository.Margin = new Padding(2, 0, 2, 0);
        labelRepository.Name = "labelRepository";
        labelRepository.Size = new Size(63, 15);
        labelRepository.TabIndex = 4;
        labelRepository.Text = "Repository";
        //
        // RepositoryTextBox
        //
        RepositoryTextBox.Dock = DockStyle.Fill;
        RepositoryTextBox.Location = new Point(81, 56);
        RepositoryTextBox.Margin = new Padding(2);
        RepositoryTextBox.Name = "RepositoryTextBox";
        RepositoryTextBox.Size = new Size(751, 23);
        RepositoryTextBox.TabIndex = 5;
        //
        // labelApiToken
        //
        labelApiToken.Anchor = AnchorStyles.Left;
        labelApiToken.AutoSize = true;
        labelApiToken.Location = new Point(2, 87);
        labelApiToken.Margin = new Padding(2, 0, 2, 0);
        labelApiToken.Name = "labelApiToken";
        labelApiToken.Size = new Size(59, 15);
        labelApiToken.TabIndex = 6;
        labelApiToken.Text = "API Token";
        //
        // ApiTokenTextBox
        //
        ApiTokenTextBox.Dock = DockStyle.Fill;
        ApiTokenTextBox.Location = new Point(81, 83);
        ApiTokenTextBox.Margin = new Padding(2);
        ApiTokenTextBox.Name = "ApiTokenTextBox";
        ApiTokenTextBox.Size = new Size(751, 23);
        ApiTokenTextBox.TabIndex = 7;
        ApiTokenTextBox.UseSystemPasswordChar = true;
        //
        // TokenManagementLink
        //
        TokenManagementLink.AutoSize = true;
        TokenManagementLink.Dock = DockStyle.Fill;
        TokenManagementLink.Location = new Point(82, 108);
        TokenManagementLink.Name = "TokenManagementLink";
        TokenManagementLink.Size = new Size(749, 15);
        TokenManagementLink.TabIndex = 8;
        TokenManagementLink.TabStop = true;
        TokenManagementLink.Text = "Create a GitHub personal access token";
        TokenManagementLink.LinkClicked += TokenManagementLink_LinkClicked;
        //
        // GitHubActionsSettingsUserControl
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = true;
        Controls.Add(tableLayoutPanel1);
        Margin = new Padding(2);
        Name = "GitHubActionsSettingsUserControl";
        Size = new Size(834, 125);
        tableLayoutPanel1.ResumeLayout(false);
        tableLayoutPanel1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TableLayoutPanel tableLayoutPanel1;
    private Label labelApiUrl;
    private TextBox ApiUrlTextBox;
    private Label labelOwner;
    private TextBox OwnerTextBox;
    private Label labelRepository;
    private TextBox RepositoryTextBox;
    private Label labelApiToken;
    private TextBox ApiTokenTextBox;
    private LinkLabel TokenManagementLink;
}
