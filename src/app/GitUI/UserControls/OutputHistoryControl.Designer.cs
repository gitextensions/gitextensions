namespace GitUI.UserControls;

partial class OutputHistoryControl
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
        components = new System.ComponentModel.Container();
        TextBox = new RichTextBox();
        TextBoxContextMenuStrip = new ContextMenuStrip(components);
        tsmiCopy = new ToolStripMenuItem();
        tsmiClear = new ToolStripMenuItem();
        TextBoxContextMenuStrip.SuspendLayout();
        SuspendLayout();
        // 
        // TextBox
        // 
        TextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        TextBox.ContextMenuStrip = TextBoxContextMenuStrip;
        TextBox.Location = new Point(0, 0);
        TextBox.Margin = new Padding(0);
        TextBox.Name = "TextBox";
        TextBox.ReadOnly = true;
        TextBox.Size = new Size(200, 100);
        TextBox.TabIndex = 0;
        TextBox.Text = "";
        TextBox.WordWrap = false;
        // 
        // TextBoxContextMenuStrip
        // 
        TextBoxContextMenuStrip.Items.AddRange(new ToolStripItem[] { tsmiCopy, tsmiClear });
        TextBoxContextMenuStrip.Name = "TextBoxContextMenuStrip";
        TextBoxContextMenuStrip.Size = new Size(181, 70);
        // 
        // tsmiCopy
        // 
        tsmiCopy.Name = "tsmiCopy";
        tsmiCopy.ShortcutKeys = Keys.Control | Keys.C;
        tsmiCopy.Size = new Size(180, 22);
        tsmiCopy.Text = "&Copy";
        // 
        // tsmiClear
        // 
        tsmiClear.Name = "tsmiClear";
        tsmiClear.Size = new Size(180, 22);
        tsmiClear.Text = "C&lear";
        // 
        // OutputHistoryControl
        // 
        AutoScaleMode = AutoScaleMode.Inherit;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Controls.Add(TextBox);
        Margin = new Padding(0);
        Name = "OutputHistoryControl";
        Size = new Size(200, 100);
        TextBoxContextMenuStrip.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    internal RichTextBox TextBox;
    private ContextMenuStrip TextBoxContextMenuStrip;
    internal ToolStripMenuItem tsmiCopy;
    internal ToolStripMenuItem tsmiClear;
}
