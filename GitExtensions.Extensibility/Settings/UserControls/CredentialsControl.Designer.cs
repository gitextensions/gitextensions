namespace GitExtensions.Extensibility.Settings.UserControls;

public partial class CredentialsControl
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
        mainTableLayoutPanel = new TableLayoutPanel();
        userNameLabel = new Label();
        userNameTextBox = new TextBox();
        passwordTextBox = new TextBox();
        passwordLabel = new Label();
        mainTableLayoutPanel.SuspendLayout();
        SuspendLayout();
        // 
        // mainTableLayoutPanel
        // 
        mainTableLayoutPanel.ColumnCount = 4;
        mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
        mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
        mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        mainTableLayoutPanel.Controls.Add(userNameLabel, 0, 0);
        mainTableLayoutPanel.Controls.Add(userNameTextBox, 1, 0);
        mainTableLayoutPanel.Controls.Add(passwordTextBox, 3, 0);
        mainTableLayoutPanel.Controls.Add(passwordLabel, 2, 0);
        mainTableLayoutPanel.Dock = DockStyle.Fill;
        mainTableLayoutPanel.Location = new Point(0, 0);
        mainTableLayoutPanel.Name = "mainTableLayoutPanel";
        mainTableLayoutPanel.RowCount = 6;
        mainTableLayoutPanel.RowStyles.Add(new RowStyle());
        mainTableLayoutPanel.RowStyles.Add(new RowStyle());
        mainTableLayoutPanel.RowStyles.Add(new RowStyle());
        mainTableLayoutPanel.RowStyles.Add(new RowStyle());
        mainTableLayoutPanel.RowStyles.Add(new RowStyle());
        mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainTableLayoutPanel.Size = new Size(800, 21);
        mainTableLayoutPanel.TabIndex = 0;
        // 
        // userNameLabel
        // 
        userNameLabel.AutoSize = true;
        userNameLabel.Dock = DockStyle.Fill;
        userNameLabel.Location = new Point(0, 3);
        userNameLabel.Margin = new Padding(0, 3, 3, 0);
        userNameLabel.Name = "userNameLabel";
        userNameLabel.Size = new Size(58, 17);
        userNameLabel.TabIndex = 0;
        userNameLabel.Text = "User name";
        // 
        // userNameTextBox
        // 
        userNameTextBox.Dock = DockStyle.Fill;
        userNameTextBox.Location = new Point(64, 0);
        userNameTextBox.Margin = new Padding(3, 0, 3, 0);
        userNameTextBox.Name = "userNameTextBox";
        userNameTextBox.Size = new Size(334, 20);
        userNameTextBox.TabIndex = 2;
        // 
        // passwordTextBox
        // 
        passwordTextBox.Dock = DockStyle.Fill;
        passwordTextBox.Location = new Point(463, 0);
        passwordTextBox.Margin = new Padding(3, 0, 0, 0);
        passwordTextBox.Name = "passwordTextBox";
        passwordTextBox.Size = new Size(337, 20);
        passwordTextBox.TabIndex = 3;
        // 
        // passwordLabel
        // 
        passwordLabel.AutoSize = true;
        passwordLabel.Dock = DockStyle.Fill;
        passwordLabel.Location = new Point(404, 3);
        passwordLabel.Margin = new Padding(3, 3, 3, 0);
        passwordLabel.Name = "passwordLabel";
        passwordLabel.Size = new Size(53, 17);
        passwordLabel.TabIndex = 1;
        passwordLabel.Text = "API token/Password";
        // 
        // CredentialsControl
        // 
        AutoScaleDimensions = new SizeF(6F, 13F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(mainTableLayoutPanel);
        MaximumSize = new Size(1900, 20);
        MinimumSize = new Size(0, 21);
        Name = "CredentialsControl";
        Size = new Size(800, 21);
        Load += CredentialsControl_Load;
        mainTableLayoutPanel.ResumeLayout(false);
        mainTableLayoutPanel.PerformLayout();
        ResumeLayout(false);

    }

    #endregion

    private TableLayoutPanel mainTableLayoutPanel;
    private Label userNameLabel;
    private Label passwordLabel;
    private TextBox userNameTextBox;
    private TextBox passwordTextBox;
}
