namespace GitUI.UserControls.GPGKeys
{
    partial class GPGSecretKeysCombobox
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
            this.components = new System.ComponentModel.Container();
            this.comboBoxKeys = new System.Windows.Forms.ComboBox();
            this.gpgKeyDisplayInfoBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.gpgKeyDisplayInfoBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxKeys
            // 
            this.comboBoxKeys.DataSource = this.gpgKeyDisplayInfoBindingSource;
            this.comboBoxKeys.DisplayMember = "Caption";
            this.comboBoxKeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxKeys.DropDownWidth = 500;
            this.comboBoxKeys.FormattingEnabled = true;
            this.comboBoxKeys.Location = new System.Drawing.Point(0, 0);
            this.comboBoxKeys.Name = "comboBoxKeys";
            this.comboBoxKeys.Size = new System.Drawing.Size(387, 23);
            this.comboBoxKeys.TabIndex = 0;
            this.comboBoxKeys.ValueMember = "KeyID";
            this.comboBoxKeys.SelectedIndexChanged += new System.EventHandler(this.GPGSecretKeysCombobox_SelectedIndexChanged);
            this.comboBoxKeys.MouseHover += new System.EventHandler(this.GPGSecretKeysCombobox_MouseHover);
            // 
            // gpgKeyDisplayInfoBindingSource
            // 
            this.gpgKeyDisplayInfoBindingSource.DataSource = typeof(GitUI.UserControls.GPGKeys.GpgKeyDisplayInfo);
            this.gpgKeyDisplayInfoBindingSource.Sort = "";
            // 
            // GPGSecretKeysCombobox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.comboBoxKeys);
            this.MinimumSize = new System.Drawing.Size(20, 20);
            this.Name = "GPGSecretKeysCombobox";
            this.Size = new System.Drawing.Size(387, 25);
            ((System.ComponentModel.ISupportInitialize)(this.gpgKeyDisplayInfoBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxKeys;
        private System.Windows.Forms.BindingSource gpgKeyDisplayInfoBindingSource;
    }
}
