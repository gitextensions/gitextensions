namespace GitUI.CommandsDialogs.SettingsDialog.Pages
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
            this.lHotkeyableItems = new System.Windows.Forms.Label();
            this.listMappings = new UserControls.NativeListView();
            this.columnCommand = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnKey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.bResetToDefaults = new System.Windows.Forms.Button();
            this.cmbSettings = new System.Windows.Forms.ListBox();
            this.txtHotkey = new TextboxHotkey();
            this.SuspendLayout();
            // 
            // lHotkey
            // 
            this.lHotkey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lHotkey.AutoSize = true;
            this.lHotkey.Location = new System.Drawing.Point(3, 471);
            this.lHotkey.MinimumSize = new System.Drawing.Size(120, 0);
            this.lHotkey.Name = "lHotkey";
            this.lHotkey.Size = new System.Drawing.Size(120, 15);
            this.lHotkey.TabIndex = 3;
            this.lHotkey.Text = "Hotkey";
            // 
            // bApply
            // 
            this.bApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bApply.Location = new System.Drawing.Point(580, 468);
            this.bApply.Name = "bApply";
            this.bApply.Size = new System.Drawing.Size(100, 26);
            this.bApply.TabIndex = 5;
            this.bApply.Text = "Apply";
            this.bApply.UseVisualStyleBackColor = true;
            this.bApply.Click += new System.EventHandler(this.bApply_Click);
            // 
            // bClear
            // 
            this.bClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bClear.Location = new System.Drawing.Point(686, 468);
            this.bClear.Name = "bClear";
            this.bClear.Size = new System.Drawing.Size(100, 26);
            this.bClear.TabIndex = 6;
            this.bClear.Text = "Clear";
            this.bClear.UseVisualStyleBackColor = true;
            this.bClear.Click += new System.EventHandler(this.bClear_Click);
            // 
            // lHotkeyableItems
            // 
            this.lHotkeyableItems.AutoSize = true;
            this.lHotkeyableItems.Location = new System.Drawing.Point(3, 3);
            this.lHotkeyableItems.Name = "lHotkeyableItems";
            this.lHotkeyableItems.Size = new System.Drawing.Size(99, 15);
            this.lHotkeyableItems.TabIndex = 0;
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
            this.listMappings.Location = new System.Drawing.Point(244, 27);
            this.listMappings.MultiSelect = false;
            this.listMappings.Name = "listMappings";
            this.listMappings.Size = new System.Drawing.Size(542, 433);
            this.listMappings.TabIndex = 2;
            this.listMappings.UseCompatibleStateImageBehavior = false;
            this.listMappings.View = System.Windows.Forms.View.Details;
            this.listMappings.SelectedIndexChanged += new System.EventHandler(this.listMappings_SelectedIndexChanged);
            // 
            // columnCommand
            // 
            this.columnCommand.Text = "Command";
            this.columnCommand.Width = 222;
            // 
            // columnKey
            // 
            this.columnKey.Text = "Key";
            this.columnKey.Width = 179;
            // 
            // bResetToDefaults
            // 
            this.bResetToDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bResetToDefaults.Location = new System.Drawing.Point(3, 496);
            this.bResetToDefaults.Name = "bResetToDefaults";
            this.bResetToDefaults.Size = new System.Drawing.Size(209, 26);
            this.bResetToDefaults.TabIndex = 7;
            this.bResetToDefaults.Text = "Reset all Hotkeys to defaults";
            this.bResetToDefaults.UseVisualStyleBackColor = true;
            this.bResetToDefaults.Click += new System.EventHandler(this.bResetToDefaults_Click);
            // 
            // cmbSettings
            // 
            this.cmbSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbSettings.FormattingEnabled = true;
            this.cmbSettings.IntegralHeight = false;
            this.cmbSettings.ItemHeight = 15;
            this.cmbSettings.Location = new System.Drawing.Point(6, 27);
            this.cmbSettings.Name = "cmbSettings";
            this.cmbSettings.Size = new System.Drawing.Size(232, 433);
            this.cmbSettings.TabIndex = 1;
            this.cmbSettings.SelectedIndexChanged += new System.EventHandler(this.cmbSettings_SelectedIndexChanged);
            // 
            // txtHotkey
            // 
            this.txtHotkey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHotkey.ForeColor = System.Drawing.Color.Red;
            this.txtHotkey.KeyData = System.Windows.Forms.Keys.None;
            this.txtHotkey.Location = new System.Drawing.Point(129, 468);
            this.txtHotkey.Name = "txtHotkey";
            this.txtHotkey.Size = new System.Drawing.Size(445, 23);
            this.txtHotkey.TabIndex = 4;
            this.txtHotkey.Text = "None";
            // 
            // ControlHotkeys
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.cmbSettings);
            this.Controls.Add(this.bResetToDefaults);
            this.Controls.Add(this.listMappings);
            this.Controls.Add(this.lHotkeyableItems);
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
    private System.Windows.Forms.Label lHotkeyableItems;
    private UserControls.NativeListView listMappings;
    private System.Windows.Forms.ColumnHeader columnCommand;
    private System.Windows.Forms.ColumnHeader columnKey;
    private System.Windows.Forms.Button bResetToDefaults;
    private System.Windows.Forms.ListBox cmbSettings;
  }
}
