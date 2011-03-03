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
      this.cmbSettings = new System.Windows.Forms.ComboBox();
      this.lHotkeyableItems = new System.Windows.Forms.Label();
      this.listMappings = new System.Windows.Forms.ListView();
      this.columnCommand = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnKey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.bResetToDefaults = new System.Windows.Forms.Button();
      this.bSaveSettings = new System.Windows.Forms.Button();
      this.txtHotkey = new GitUI.Hotkey.TextboxHotkey();
      this.SuspendLayout();
      // 
      // lHotkey
      // 
      this.lHotkey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lHotkey.AutoSize = true;
      this.lHotkey.Location = new System.Drawing.Point(3, 471);
      this.lHotkey.Name = "lHotkey";
      this.lHotkey.Size = new System.Drawing.Size(52, 17);
      this.lHotkey.TabIndex = 1;
      this.lHotkey.Text = "Hotkey";
      // 
      // bApply
      // 
      this.bApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bApply.Location = new System.Drawing.Point(648, 496);
      this.bApply.Name = "bApply";
      this.bApply.Size = new System.Drawing.Size(66, 26);
      this.bApply.TabIndex = 4;
      this.bApply.Text = "Apply";
      this.bApply.UseVisualStyleBackColor = true;
      this.bApply.Click += new System.EventHandler(this.bApply_Click);
      // 
      // bClear
      // 
      this.bClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bClear.Location = new System.Drawing.Point(720, 496);
      this.bClear.Name = "bClear";
      this.bClear.Size = new System.Drawing.Size(66, 26);
      this.bClear.TabIndex = 5;
      this.bClear.Text = "Clear";
      this.bClear.UseVisualStyleBackColor = true;
      this.bClear.Click += new System.EventHandler(this.bClear_Click);
      // 
      // cmbSettings
      // 
      this.cmbSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cmbSettings.DisplayMember = "Name";
      this.cmbSettings.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbSettings.FormattingEnabled = true;
      this.cmbSettings.Location = new System.Drawing.Point(3, 23);
      this.cmbSettings.Name = "cmbSettings";
      this.cmbSettings.Size = new System.Drawing.Size(783, 24);
      this.cmbSettings.TabIndex = 0;
      this.cmbSettings.SelectedIndexChanged += new System.EventHandler(this.cmbSettings_SelectedIndexChanged);
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
      // listMappings
      // 
      this.listMappings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.listMappings.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnCommand,
            this.columnKey});
      this.listMappings.FullRowSelect = true;
      this.listMappings.HideSelection = false;
      this.listMappings.Location = new System.Drawing.Point(6, 53);
      this.listMappings.MultiSelect = false;
      this.listMappings.Name = "listMappings";
      this.listMappings.Size = new System.Drawing.Size(780, 409);
      this.listMappings.TabIndex = 1;
      this.listMappings.UseCompatibleStateImageBehavior = false;
      this.listMappings.View = System.Windows.Forms.View.Details;
      this.listMappings.SelectedIndexChanged += new System.EventHandler(this.listMappings_SelectedIndexChanged);
      // 
      // columnCommand
      // 
      this.columnCommand.Text = "Command";
      this.columnCommand.Width = 193;
      // 
      // columnKey
      // 
      this.columnKey.Text = "Key";
      this.columnKey.Width = 120;
      // 
      // bResetToDefaults
      // 
      this.bResetToDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.bResetToDefaults.Location = new System.Drawing.Point(3, 496);
      this.bResetToDefaults.Name = "bResetToDefaults";
      this.bResetToDefaults.Size = new System.Drawing.Size(209, 26);
      this.bResetToDefaults.TabIndex = 6;
      this.bResetToDefaults.Text = "Reset all Hotkeys to defaults";
      this.bResetToDefaults.UseVisualStyleBackColor = true;
      this.bResetToDefaults.Click += new System.EventHandler(this.bResetToDefaults_Click);
      // 
      // bSaveSettings
      // 
      this.bSaveSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bSaveSettings.Location = new System.Drawing.Point(478, 496);
      this.bSaveSettings.Name = "bSaveSettings";
      this.bSaveSettings.Size = new System.Drawing.Size(164, 26);
      this.bSaveSettings.TabIndex = 7;
      this.bSaveSettings.Text = "Save Hotkey Settings";
      this.bSaveSettings.UseVisualStyleBackColor = true;
      this.bSaveSettings.Click += new System.EventHandler(this.bSaveSettings_Click);
      // 
      // txtHotkey
      // 
      this.txtHotkey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtHotkey.KeyData = System.Windows.Forms.Keys.None;
      this.txtHotkey.Location = new System.Drawing.Point(56, 468);
      this.txtHotkey.Name = "txtHotkey";
      this.txtHotkey.Size = new System.Drawing.Size(730, 22);
      this.txtHotkey.TabIndex = 3;
      this.txtHotkey.Text = "None";
      // 
      // ControlHotkeys
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.Controls.Add(this.bSaveSettings);
      this.Controls.Add(this.bResetToDefaults);
      this.Controls.Add(this.listMappings);
      this.Controls.Add(this.lHotkeyableItems);
      this.Controls.Add(this.cmbSettings);
      this.Controls.Add(this.bClear);
      this.Controls.Add(this.bApply);
      this.Controls.Add(this.lHotkey);
      this.Controls.Add(this.txtHotkey);
      this.Name = "ControlHotkeys";
      this.Size = new System.Drawing.Size(791, 525);
      this.Load += new System.EventHandler(this.ControlHotkeys_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private TextboxHotkey txtHotkey;
    private System.Windows.Forms.Label lHotkey;
    private System.Windows.Forms.Button bApply;
    private System.Windows.Forms.Button bClear;
    private System.Windows.Forms.ComboBox cmbSettings;
    private System.Windows.Forms.Label lHotkeyableItems;
    private System.Windows.Forms.ListView listMappings;
    private System.Windows.Forms.ColumnHeader columnCommand;
    private System.Windows.Forms.ColumnHeader columnKey;
    private System.Windows.Forms.Button bResetToDefaults;
    private System.Windows.Forms.Button bSaveSettings;
  }
}
