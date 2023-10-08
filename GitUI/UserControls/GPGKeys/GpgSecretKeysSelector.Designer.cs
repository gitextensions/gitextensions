namespace GitUI.UserControls.GPGKeys
{
    partial class GpgSecretKeysSelector
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
            components = new System.ComponentModel.Container();
            comboBoxKeys = new ComboBox();
            ContextMenuActions = new ContextMenuStrip(components);
            refreshToolStripMenuItem = new ToolStripMenuItem();
            gpgKeyDisplayInfoBindingSource = new BindingSource(components);
            ContextMenuActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gpgKeyDisplayInfoBindingSource).BeginInit();
            SuspendLayout();
            // 
            // comboBoxKeys
            // 
            comboBoxKeys.ContextMenuStrip = ContextMenuActions;
            comboBoxKeys.DataSource = gpgKeyDisplayInfoBindingSource;
            comboBoxKeys.DisplayMember = "Caption";
            comboBoxKeys.Dock = DockStyle.Fill;
            comboBoxKeys.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxKeys.DropDownWidth = 500;
            comboBoxKeys.FormattingEnabled = true;
            comboBoxKeys.Location = new Point(0, 0);
            comboBoxKeys.Name = "comboBoxKeys";
            comboBoxKeys.Size = new Size(387, 23);
            comboBoxKeys.TabIndex = 0;
            comboBoxKeys.ValueMember = "KeyID";
            comboBoxKeys.DropDown += GPGSecretKeysCombobox_DropDown;
            comboBoxKeys.SelectedIndexChanged += GPGSecretKeysCombobox_SelectedIndexChanged;
            comboBoxKeys.DropDownClosed += comboBoxKeys_DropDownClosed;
            comboBoxKeys.MouseHover += GPGSecretKeysCombobox_MouseHover;
            // 
            // ContextMenuActions
            // 
            ContextMenuActions.Items.AddRange(new ToolStripItem[] { refreshToolStripMenuItem });
            ContextMenuActions.Name = "ContextMenuActions";
            ContextMenuActions.ShowImageMargin = false;
            ContextMenuActions.Size = new Size(89, 26);
            ContextMenuActions.Closing += ContextMenuActions_Closing;
            ContextMenuActions.Opening += ContextMenuActions_Opening;
            ContextMenuActions.Click += ContextMenuActions_Click;
            // 
            // refreshToolStripMenuItem
            // 
            refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            refreshToolStripMenuItem.Size = new Size(88, 22);
            refreshToolStripMenuItem.Text = "&Refresh";
            refreshToolStripMenuItem.Click += refreshToolStripMenuItem_Click;
            // 
            // gpgKeyDisplayInfoBindingSource
            // 
            gpgKeyDisplayInfoBindingSource.DataSource = typeof(GpgKeyDisplayInfo);
            gpgKeyDisplayInfoBindingSource.Sort = "";
            // 
            // GpgSecretKeysSelector
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(comboBoxKeys);
            MinimumSize = new Size(20, 20);
            Name = "GpgSecretKeysSelector";
            Size = new Size(387, 25);
            ContextMenuActions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gpgKeyDisplayInfoBindingSource).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private ComboBox comboBoxKeys;
        private BindingSource gpgKeyDisplayInfoBindingSource;
        private ContextMenuStrip ContextMenuActions;
        private ToolStripMenuItem refreshToolStripMenuItem;
    }
}
