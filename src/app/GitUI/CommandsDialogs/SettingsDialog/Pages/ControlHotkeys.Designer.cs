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
            lHotkey = new Label();
            bApply = new Button();
            bClear = new Button();
            lHotkeyableItems = new Label();
            listMappings = new UserControls.NativeListView();
            columnCommand = ((ColumnHeader)(new ColumnHeader()));
            columnKey = ((ColumnHeader)(new ColumnHeader()));
            bResetToDefaults = new Button();
            cmbSettings = new ListBox();
            txtHotkey = new TextboxHotkey();
            SuspendLayout();
            // 
            // lHotkey
            // 
            lHotkey.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lHotkey.AutoSize = true;
            lHotkey.Location = new Point(3, 471);
            lHotkey.MinimumSize = new Size(120, 0);
            lHotkey.Name = "lHotkey";
            lHotkey.Size = new Size(120, 15);
            lHotkey.TabIndex = 3;
            lHotkey.Text = "Hotkey";
            // 
            // bApply
            // 
            bApply.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            bApply.Location = new Point(580, 468);
            bApply.Name = "bApply";
            bApply.Size = new Size(100, 26);
            bApply.TabIndex = 5;
            bApply.Text = "Apply";
            bApply.UseVisualStyleBackColor = true;
            bApply.Click += bApply_Click;
            // 
            // bClear
            // 
            bClear.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            bClear.Location = new Point(686, 468);
            bClear.Name = "bClear";
            bClear.Size = new Size(100, 26);
            bClear.TabIndex = 6;
            bClear.Text = "Clear";
            bClear.UseVisualStyleBackColor = true;
            bClear.Click += bClear_Click;
            // 
            // lHotkeyableItems
            // 
            lHotkeyableItems.AutoSize = true;
            lHotkeyableItems.Location = new Point(3, 3);
            lHotkeyableItems.Name = "lHotkeyableItems";
            lHotkeyableItems.Size = new Size(99, 15);
            lHotkeyableItems.TabIndex = 0;
            lHotkeyableItems.Text = "Hotkeyable Items";
            // 
            // listMappings
            // 
            listMappings.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listMappings.Columns.AddRange(new ColumnHeader[] {
            columnCommand,
            columnKey});
            listMappings.FullRowSelect = true;
            listMappings.HideSelection = false;
            listMappings.Location = new Point(244, 27);
            listMappings.MultiSelect = false;
            listMappings.Name = "listMappings";
            listMappings.Size = new Size(542, 433);
            listMappings.TabIndex = 2;
            listMappings.UseCompatibleStateImageBehavior = false;
            listMappings.View = View.Details;
            listMappings.SelectedIndexChanged += listMappings_SelectedIndexChanged;
            // 
            // columnCommand
            // 
            columnCommand.Text = "Command";
            columnCommand.Width = 222;
            // 
            // columnKey
            // 
            columnKey.Text = "Key";
            columnKey.Width = 179;
            // 
            // bResetToDefaults
            // 
            bResetToDefaults.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            bResetToDefaults.Location = new Point(3, 496);
            bResetToDefaults.Name = "bResetToDefaults";
            bResetToDefaults.Size = new Size(209, 26);
            bResetToDefaults.TabIndex = 7;
            bResetToDefaults.Text = "Reset all Hotkeys to defaults";
            bResetToDefaults.UseVisualStyleBackColor = true;
            bResetToDefaults.Click += bResetToDefaults_Click;
            // 
            // cmbSettings
            // 
            cmbSettings.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            cmbSettings.FormattingEnabled = true;
            cmbSettings.IntegralHeight = false;
            cmbSettings.ItemHeight = 15;
            cmbSettings.Location = new Point(6, 27);
            cmbSettings.Name = "cmbSettings";
            cmbSettings.Size = new Size(232, 433);
            cmbSettings.TabIndex = 1;
            cmbSettings.SelectedIndexChanged += cmbSettings_SelectedIndexChanged;
            // 
            // txtHotkey
            // 
            txtHotkey.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtHotkey.ForeColor = Color.Red;
            txtHotkey.KeyData = Keys.None;
            txtHotkey.Location = new Point(129, 468);
            txtHotkey.Name = "txtHotkey";
            txtHotkey.Size = new Size(445, 23);
            txtHotkey.TabIndex = 4;
            txtHotkey.Text = "None";
            // 
            // ControlHotkeys
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(cmbSettings);
            Controls.Add(bResetToDefaults);
            Controls.Add(listMappings);
            Controls.Add(lHotkeyableItems);
            Controls.Add(bClear);
            Controls.Add(bApply);
            Controls.Add(lHotkey);
            Controls.Add(txtHotkey);
            Name = "ControlHotkeys";
            Size = new Size(791, 525);
            Load += ControlHotkeys_Load;
            ResumeLayout(false);
            PerformLayout();

    }

    #endregion

    private TextboxHotkey txtHotkey;
    private Label lHotkey;
    private Button bApply;
    private Button bClear;
    private Label lHotkeyableItems;
    private UserControls.NativeListView listMappings;
    private ColumnHeader columnCommand;
    private ColumnHeader columnKey;
    private Button bResetToDefaults;
    private ListBox cmbSettings;
  }
}
