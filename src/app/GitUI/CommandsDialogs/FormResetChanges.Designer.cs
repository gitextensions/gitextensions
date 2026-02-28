namespace GitUI.CommandsDialogs;

partial class FormResetChanges
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        txtMessage = new TextBox();
        btnReset = new Button();
        btnCancel = new Button();
        cbDeleteNewFilesAndDirectories = new CheckBox();
        lblDeleteHint = new Label();
        flowLayoutPanel1 = new FlowLayoutPanel();
        flowLayoutPanel1.SuspendLayout();
        SuspendLayout();
        // 
        // txtMessage
        // 
        txtMessage.BorderStyle = BorderStyle.None;
        txtMessage.Dock = DockStyle.Fill;
        txtMessage.Location = new Point(8, 8);
        txtMessage.Multiline = true;
        txtMessage.Name = "txtMessage";
        txtMessage.ReadOnly = true;
        txtMessage.Size = new Size(452, 16);
        txtMessage.TabIndex = 0;
        txtMessage.TabStop = false;
        txtMessage.Text = "Are you sure you want to reset your changes?";
        // 
        // btnReset
        // 
        btnReset.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnReset.Location = new Point(253, 3);
        btnReset.Name = "btnReset";
        btnReset.Size = new Size(95, 25);
        btnReset.TabIndex = 2;
        btnReset.Text = "R&eset";
        btnReset.UseVisualStyleBackColor = true;
        btnReset.Click += btnReset_Click;
        // 
        // btnCancel
        // 
        btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Location = new Point(354, 3);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(95, 25);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "&Cancel";
        btnCancel.UseVisualStyleBackColor = true;
        btnCancel.Click += btnCancel_Click;
        // 
        // cbDeleteNewFilesAndDirectories
        // 
        cbDeleteNewFilesAndDirectories.AutoSize = true;
        cbDeleteNewFilesAndDirectories.Dock = DockStyle.Bottom;
        cbDeleteNewFilesAndDirectories.Location = new Point(8, 55);
        cbDeleteNewFilesAndDirectories.Name = "cbDeleteNewFilesAndDirectories";
        cbDeleteNewFilesAndDirectories.Padding = new Padding(3, 8, 0, 8);
        cbDeleteNewFilesAndDirectories.Size = new Size(452, 35);
        cbDeleteNewFilesAndDirectories.TabIndex = 3;
        cbDeleteNewFilesAndDirectories.Text = "Also delete &new files and/or directories";
        cbDeleteNewFilesAndDirectories.UseVisualStyleBackColor = true;
        // 
        // lblDeleteHint
        // 
        lblDeleteHint.AutoSize = true;
        lblDeleteHint.Dock = DockStyle.Bottom;
        lblDeleteHint.Location = new Point(8, 24);
        lblDeleteHint.Name = "lblDeleteHint";
        lblDeleteHint.Padding = new Padding(0, 8, 0, 8);
        lblDeleteHint.Size = new Size(214, 31);
        lblDeleteHint.TabIndex = 4;
        lblDeleteHint.Text = "This will delete any uncommitted work.";
        // 
        // flowLayoutPanel1
        // 
        flowLayoutPanel1.AutoSize = true;
        flowLayoutPanel1.Controls.Add(btnCancel);
        flowLayoutPanel1.Controls.Add(btnReset);
        flowLayoutPanel1.Dock = DockStyle.Bottom;
        flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
        flowLayoutPanel1.Location = new Point(8, 90);
        flowLayoutPanel1.Margin = new Padding(2);
        flowLayoutPanel1.Name = "flowLayoutPanel1";
        flowLayoutPanel1.Size = new Size(452, 31);
        flowLayoutPanel1.TabIndex = 5;
        // 
        // FormResetChanges
        // 
        AcceptButton = btnCancel;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSize = true;
        CancelButton = btnCancel;
        ClientSize = new Size(460, 121);
        Controls.Add(txtMessage);
        Controls.Add(lblDeleteHint);
        Controls.Add(cbDeleteNewFilesAndDirectories);
        Controls.Add(flowLayoutPanel1);
        MinimizeBox = false;
        Name = "FormResetChanges";
        Padding = new Padding(8, 8, 0, 0);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Reset changes";
        flowLayoutPanel1.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();

    }

    #endregion

    private TextBox txtMessage;
    private Button btnReset;
    private Button btnCancel;
    private CheckBox cbDeleteNewFilesAndDirectories;
    private Label lblDeleteHint;
    private FlowLayoutPanel flowLayoutPanel1;
}
