namespace GitUI.UserControls;

partial class PasswordInput
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
        TableLayoutPanel = new TableLayoutPanel();
        SendInput = new Button();
        ShowPassword = new Button();
        Password = new TextBox();
        TableLayoutPanel.SuspendLayout();
        SuspendLayout();
        // 
        // TableLayoutPanel
        // 
        TableLayoutPanel.AutoSize = true;
        TableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        TableLayoutPanel.ColumnCount = 3;
        TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
        TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
        TableLayoutPanel.Controls.Add(SendInput, 2, 0);
        TableLayoutPanel.Controls.Add(ShowPassword, 1, 0);
        TableLayoutPanel.Controls.Add(Password, 0, 0);
        TableLayoutPanel.Dock = DockStyle.Top;
        TableLayoutPanel.Location = new Point(0, 0);
        TableLayoutPanel.Name = "TableLayoutPanel";
        TableLayoutPanel.RowCount = 1;
        TableLayoutPanel.RowStyles.Add(new RowStyle());
        TableLayoutPanel.Size = new Size(349, 31);
        TableLayoutPanel.TabIndex = 1;
        // 
        // SendInput
        // 
        SendInput.AutoSize = true;
        SendInput.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        SendInput.Location = new Point(272, 3);
        SendInput.Name = "SendInput";
        SendInput.Size = new Size(74, 25);
        SendInput.TabIndex = 3;
        SendInput.Text = "Send input";
        SendInput.UseVisualStyleBackColor = true;
        SendInput.Click += SendInput_Click;
        // 
        // ShowPassword
        // 
        ShowPassword.Image = Properties.Images.EyeClosed;
        ShowPassword.Location = new Point(241, 3);
        ShowPassword.Name = "ShowPassword";
        ShowPassword.Size = new Size(25, 25);
        ShowPassword.TabIndex = 2;
        ShowPassword.UseVisualStyleBackColor = true;
        ShowPassword.Click += ShowPassword_Click;
        // 
        // Password
        // 
        Password.AllowDrop = true;
        Password.Dock = DockStyle.Fill;
        Password.Location = new Point(4, 4);
        Password.Margin = new Padding(4);
        Password.MinimumSize = new Size(100, 23);
        Password.Name = "Password";
        Password.Size = new Size(230, 23);
        Password.TabIndex = 1;
        Password.UseSystemPasswordChar = true;
        Password.DragDrop += Text_DragDrop;
        Password.DragEnter += Text_DragEnter;
        Password.DragOver += Text_DragEnter;
        Password.Enter += Text_Enter;
        Password.Leave += Text_Leave;
        // 
        // PasswordInput
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Controls.Add(TableLayoutPanel);
        Name = "PasswordInput";
        Size = new Size(349, 31);
        TableLayoutPanel.ResumeLayout(false);
        TableLayoutPanel.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TableLayoutPanel TableLayoutPanel;
    private Button SendInput;
    private Button ShowPassword;
    private TextBox Password;
}
