namespace GitUI
{
    partial class FormLoadPuttySSHKey
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLoadPuttySSHKey));
            this.label1 = new System.Windows.Forms.Label();
            this.PrivateKeypath = new System.Windows.Forms.ComboBox();
            this.Browse = new System.Windows.Forms.Button();
            this.LoadSSHKey = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 27;
            this.label1.Text = "Private key file";
            // 
            // PrivateKeypath
            // 
            this.PrivateKeypath.FormattingEnabled = true;
            this.PrivateKeypath.Location = new System.Drawing.Point(140, 16);
            this.PrivateKeypath.Name = "PrivateKeypath";
            this.PrivateKeypath.Size = new System.Drawing.Size(297, 21);
            this.PrivateKeypath.TabIndex = 28;
            this.PrivateKeypath.DropDown += new System.EventHandler(this.PrivateKeypath_DropDown);
            // 
            // Browse
            // 
            this.Browse.Location = new System.Drawing.Point(443, 14);
            this.Browse.Name = "Browse";
            this.Browse.Size = new System.Drawing.Size(75, 23);
            this.Browse.TabIndex = 29;
            this.Browse.Text = "Browse";
            this.Browse.UseVisualStyleBackColor = true;
            this.Browse.Click += new System.EventHandler(this.Browse_Click);
            // 
            // LoadSSHKey
            // 
            this.LoadSSHKey.Image = global::GitUI.Properties.Resources.putty;
            this.LoadSSHKey.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadSSHKey.Location = new System.Drawing.Point(443, 43);
            this.LoadSSHKey.Name = "LoadSSHKey";
            this.LoadSSHKey.Size = new System.Drawing.Size(75, 23);
            this.LoadSSHKey.TabIndex = 26;
            this.LoadSSHKey.Text = "Load";
            this.LoadSSHKey.UseVisualStyleBackColor = true;
            this.LoadSSHKey.Click += new System.EventHandler(this.LoadSSHKey_Click);
            // 
            // FormLoadPuttySSHKey
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(530, 71);
            this.Controls.Add(this.Browse);
            this.Controls.Add(this.PrivateKeypath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LoadSSHKey);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormLoadPuttySSHKey";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Load PuTTY SSH key into authentication agent";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LoadSSHKey;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox PrivateKeypath;
        private System.Windows.Forms.Button Browse;
    }
}