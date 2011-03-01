namespace GitUI.Hotkey
{
  partial class ControlHotkeys
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
      this.lHotkey = new System.Windows.Forms.Label();
      this.bApply = new System.Windows.Forms.Button();
      this.bClear = new System.Windows.Forms.Button();
      this.txtHotkey = new GitUI.Hotkey.TextboxHotkey();
      this.cmbItems = new System.Windows.Forms.ComboBox();
      this.lHotkeyableItems = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // lHotkey
      // 
      this.lHotkey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.lHotkey.AutoSize = true;
      this.lHotkey.Location = new System.Drawing.Point(328, 3);
      this.lHotkey.Name = "lHotkey";
      this.lHotkey.Size = new System.Drawing.Size(52, 17);
      this.lHotkey.TabIndex = 1;
      this.lHotkey.Text = "Hotkey";
      // 
      // bApply
      // 
      this.bApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bApply.Location = new System.Drawing.Point(470, 51);
      this.bApply.Name = "bApply";
      this.bApply.Size = new System.Drawing.Size(66, 26);
      this.bApply.TabIndex = 2;
      this.bApply.Text = "Apply";
      this.bApply.UseVisualStyleBackColor = true;
      // 
      // bClear
      // 
      this.bClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.bClear.Location = new System.Drawing.Point(542, 51);
      this.bClear.Name = "bClear";
      this.bClear.Size = new System.Drawing.Size(66, 26);
      this.bClear.TabIndex = 3;
      this.bClear.Text = "Clear";
      this.bClear.UseVisualStyleBackColor = true;
      // 
      // txtHotkey
      // 
      this.txtHotkey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.txtHotkey.KeyData = System.Windows.Forms.Keys.None;
      this.txtHotkey.Location = new System.Drawing.Point(327, 23);
      this.txtHotkey.Name = "txtHotkey";
      this.txtHotkey.Size = new System.Drawing.Size(281, 22);
      this.txtHotkey.TabIndex = 0;
      this.txtHotkey.Text = "None";
      // 
      // cmbItems
      // 
      this.cmbItems.FormattingEnabled = true;
      this.cmbItems.Location = new System.Drawing.Point(3, 23);
      this.cmbItems.Name = "cmbItems";
      this.cmbItems.Size = new System.Drawing.Size(177, 24);
      this.cmbItems.TabIndex = 4;
      // 
      // lHotkeyableItems
      // 
      this.lHotkeyableItems.AutoSize = true;
      this.lHotkeyableItems.Location = new System.Drawing.Point(3, 3);
      this.lHotkeyableItems.Name = "lHotkeyableItems";
      this.lHotkeyableItems.Size = new System.Drawing.Size(116, 17);
      this.lHotkeyableItems.TabIndex = 5;
      this.lHotkeyableItems.Text = "Hotkeyable Items";
      // 
      // ControlHotkeys
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.lHotkeyableItems);
      this.Controls.Add(this.cmbItems);
      this.Controls.Add(this.bClear);
      this.Controls.Add(this.bApply);
      this.Controls.Add(this.lHotkey);
      this.Controls.Add(this.txtHotkey);
      this.Name = "ControlHotkeys";
      this.Size = new System.Drawing.Size(611, 413);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private TextboxHotkey txtHotkey;
    private System.Windows.Forms.Label lHotkey;
    private System.Windows.Forms.Button bApply;
    private System.Windows.Forms.Button bClear;
    private System.Windows.Forms.ComboBox cmbItems;
    private System.Windows.Forms.Label lHotkeyableItems;
  }
}
