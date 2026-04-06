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
        tlpnlMain = new TableLayoutPanel();
        lblApiUrl = new Label();
        txtApiUrl = new TextBox();
        lblOwner = new Label();
        txtOwner = new TextBox();
        lblRepository = new Label();
        txtRepository = new TextBox();
        lblApiToken = new Label();
        txtApiToken = new TextBox();
        lnkTokenManagement = new LinkLabel();
        tlpnlMain.SuspendLayout();
        SuspendLayout();
        //
        // tlpnlMain
        //
        tlpnlMain.AutoSize = true;
        tlpnlMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tlpnlMain.ColumnCount = 2;
        tlpnlMain.ColumnStyles.Add(new ColumnStyle());
        tlpnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tlpnlMain.Controls.Add(lblApiUrl, 0, 0);
        tlpnlMain.Controls.Add(txtApiUrl, 1, 0);
        tlpnlMain.Controls.Add(lblOwner, 0, 1);
        tlpnlMain.Controls.Add(txtOwner, 1, 1);
        tlpnlMain.Controls.Add(lblRepository, 0, 2);
        tlpnlMain.Controls.Add(txtRepository, 1, 2);
        tlpnlMain.Controls.Add(lblApiToken, 0, 3);
        tlpnlMain.Controls.Add(txtApiToken, 1, 3);
        tlpnlMain.Controls.Add(lnkTokenManagement, 1, 4);
        tlpnlMain.Dock = DockStyle.Top;
        tlpnlMain.Location = new Point(0, 0);
        tlpnlMain.Margin = new Padding(2);
        tlpnlMain.Name = "tlpnlMain";
        tlpnlMain.RowCount = 5;
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.RowStyles.Add(new RowStyle());
        tlpnlMain.Size = new Size(834, 123);
        tlpnlMain.TabIndex = 0;
        //
        // lblApiUrl
        //
        lblApiUrl.Anchor = AnchorStyles.Left;
        lblApiUrl.AutoSize = true;
        lblApiUrl.Location = new Point(2, 6);
        lblApiUrl.Margin = new Padding(2, 0, 2, 0);
        lblApiUrl.Name = "lblApiUrl";
        lblApiUrl.Size = new Size(50, 15);
        lblApiUrl.TabIndex = 0;
        lblApiUrl.Text = "&API URL";
        //
        // txtApiUrl
        //
        txtApiUrl.Dock = DockStyle.Fill;
        txtApiUrl.Location = new Point(81, 2);
        txtApiUrl.Margin = new Padding(2);
        txtApiUrl.Name = "txtApiUrl";
        txtApiUrl.Size = new Size(751, 23);
        txtApiUrl.TabIndex = 1;
        //
        // lblOwner
        //
        lblOwner.Anchor = AnchorStyles.Left;
        lblOwner.AutoSize = true;
        lblOwner.Location = new Point(2, 33);
        lblOwner.Margin = new Padding(2, 0, 2, 0);
        lblOwner.Name = "lblOwner";
        lblOwner.Size = new Size(42, 15);
        lblOwner.TabIndex = 2;
        lblOwner.Text = "&Owner";
        //
        // txtOwner
        //
        txtOwner.Dock = DockStyle.Fill;
        txtOwner.Location = new Point(81, 29);
        txtOwner.Margin = new Padding(2);
        txtOwner.Name = "txtOwner";
        txtOwner.Size = new Size(751, 23);
        txtOwner.TabIndex = 3;
        //
        // lblRepository
        //
        lblRepository.Anchor = AnchorStyles.Left;
        lblRepository.AutoSize = true;
        lblRepository.Location = new Point(2, 60);
        lblRepository.Margin = new Padding(2, 0, 2, 0);
        lblRepository.Name = "lblRepository";
        lblRepository.Size = new Size(63, 15);
        lblRepository.TabIndex = 4;
        lblRepository.Text = "&Repository";
        //
        // txtRepository
        //
        txtRepository.Dock = DockStyle.Fill;
        txtRepository.Location = new Point(81, 56);
        txtRepository.Margin = new Padding(2);
        txtRepository.Name = "txtRepository";
        txtRepository.Size = new Size(751, 23);
        txtRepository.TabIndex = 5;
        //
        // lblApiToken
        //
        lblApiToken.Anchor = AnchorStyles.Left;
        lblApiToken.AutoSize = true;
        lblApiToken.Location = new Point(2, 87);
        lblApiToken.Margin = new Padding(2, 0, 2, 0);
        lblApiToken.Name = "lblApiToken";
        lblApiToken.Size = new Size(59, 15);
        lblApiToken.TabIndex = 6;
        lblApiToken.Text = "API &Token";
        //
        // txtApiToken
        //
        txtApiToken.Dock = DockStyle.Fill;
        txtApiToken.Location = new Point(81, 83);
        txtApiToken.Margin = new Padding(2);
        txtApiToken.Name = "txtApiToken";
        txtApiToken.Size = new Size(751, 23);
        txtApiToken.TabIndex = 7;
        txtApiToken.UseSystemPasswordChar = true;
        //
        // lnkTokenManagement
        //
        lnkTokenManagement.AutoSize = true;
        lnkTokenManagement.Dock = DockStyle.Fill;
        lnkTokenManagement.Location = new Point(82, 108);
        lnkTokenManagement.Name = "lnkTokenManagement";
        lnkTokenManagement.Size = new Size(749, 15);
        lnkTokenManagement.TabIndex = 8;
        lnkTokenManagement.TabStop = true;
        lnkTokenManagement.Text = "Create a GitHub personal access token";
        lnkTokenManagement.LinkClicked += lnkTokenManagement_LinkClicked;
        //
        // GitHubActionsSettingsUserControl
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = true;
        Controls.Add(tlpnlMain);
        Margin = new Padding(2);
        Name = "GitHubActionsSettingsUserControl";
        Size = new Size(834, 125);
        tlpnlMain.ResumeLayout(false);
        tlpnlMain.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TableLayoutPanel tlpnlMain;
    private Label lblApiUrl;
    private TextBox txtApiUrl;
    private Label lblOwner;
    private TextBox txtOwner;
    private Label lblRepository;
    private TextBox txtRepository;
    private Label lblApiToken;
    private TextBox txtApiToken;
    private LinkLabel lnkTokenManagement;
}
