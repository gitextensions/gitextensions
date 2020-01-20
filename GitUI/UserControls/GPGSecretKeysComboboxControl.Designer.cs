namespace GitUI.UserControls
{
    partial class GPGSecretKeysComboboxControl
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
            this.comboBoxKeys.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxKeys.DataSource = this.gpgKeyDisplayInfoBindingSource;
            this.comboBoxKeys.DisplayMember = "Caption";
            this.comboBoxKeys.FormattingEnabled = true;
            this.comboBoxKeys.Location = new System.Drawing.Point(3, 3);
            this.comboBoxKeys.MinimumSize = new System.Drawing.Size(270, 0);
            this.comboBoxKeys.Name = "comboBoxKeys";
            this.comboBoxKeys.Size = new System.Drawing.Size(278, 21);
            this.comboBoxKeys.TabIndex = 0;
            this.comboBoxKeys.ValueMember = "KeyID";
            this.comboBoxKeys.DropDown += new System.EventHandler(this.comboBoxKeys_DropDown);
            this.comboBoxKeys.SelectedIndexChanged += new System.EventHandler(this.comboBoxKeys_SelectedIndexChanged);
            // 
            // gpgKeyDisplayInfoBindingSource
            // 
            this.gpgKeyDisplayInfoBindingSource.DataSource = typeof(GitUI.UserControls.GPGSecretKeysComboboxControl.GpgKeyDisplayInfo);
            // 
            // GPGSecretKeysComboboxControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.comboBoxKeys);
            this.MinimumSize = new System.Drawing.Size(20, 20);
            this.Name = "GPGSecretKeysComboboxControl";
            this.Size = new System.Drawing.Size(291, 29);
            this.Load += new System.EventHandler(this.GPGSecretKeysComboboxControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gpgKeyDisplayInfoBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxKeys;
        private System.Windows.Forms.BindingSource gpgKeyDisplayInfoBindingSource;
    }
}
