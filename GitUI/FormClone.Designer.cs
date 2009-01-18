namespace GitUI
{
    partial class FormClone
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormClone));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.CentralRepository = new System.Windows.Forms.RadioButton();
            this.PersonalRepository = new System.Windows.Forms.RadioButton();
            this.From = new System.Windows.Forms.ComboBox();
            this.To = new System.Windows.Forms.ComboBox();
            this.ToBrowse = new System.Windows.Forms.Button();
            this.FromBrowse = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Ok = new System.Windows.Forms.Button();
            this.Central = new System.Windows.Forms.RadioButton();
            this.Personal = new System.Windows.Forms.RadioButton();
            this.LoadSSHKey = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel1.Controls.Add(this.From);
            this.splitContainer1.Panel1.Controls.Add(this.To);
            this.splitContainer1.Panel1.Controls.Add(this.ToBrowse);
            this.splitContainer1.Panel1.Controls.Add(this.FromBrowse);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel1_Paint);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.LoadSSHKey);
            this.splitContainer1.Panel2.Controls.Add(this.Ok);
            this.splitContainer1.Size = new System.Drawing.Size(460, 176);
            this.splitContainer1.SplitterDistance = 147;
            this.splitContainer1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.CentralRepository);
            this.groupBox1.Controls.Add(this.PersonalRepository);
            this.groupBox1.Location = new System.Drawing.Point(3, 73);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(352, 68);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Repository type";
            // 
            // CentralRepository
            // 
            this.CentralRepository.AutoSize = true;
            this.CentralRepository.Location = new System.Drawing.Point(6, 42);
            this.CentralRepository.Name = "CentralRepository";
            this.CentralRepository.Size = new System.Drawing.Size(274, 17);
            this.CentralRepository.TabIndex = 1;
            this.CentralRepository.Text = "Central repository, no working dir  (--bare --shared=all)";
            this.CentralRepository.UseVisualStyleBackColor = true;
            // 
            // PersonalRepository
            // 
            this.PersonalRepository.AutoSize = true;
            this.PersonalRepository.Checked = true;
            this.PersonalRepository.Location = new System.Drawing.Point(6, 19);
            this.PersonalRepository.Name = "PersonalRepository";
            this.PersonalRepository.Size = new System.Drawing.Size(114, 17);
            this.PersonalRepository.TabIndex = 0;
            this.PersonalRepository.TabStop = true;
            this.PersonalRepository.Text = "Personal repository";
            this.PersonalRepository.UseVisualStyleBackColor = true;
            // 
            // From
            // 
            this.From.FormattingEnabled = true;
            this.From.Location = new System.Drawing.Point(66, 17);
            this.From.Name = "From";
            this.From.Size = new System.Drawing.Size(289, 21);
            this.From.TabIndex = 8;
            this.From.DropDown += new System.EventHandler(this.From_DropDown);
            // 
            // To
            // 
            this.To.FormattingEnabled = true;
            this.To.Location = new System.Drawing.Point(66, 45);
            this.To.Name = "To";
            this.To.Size = new System.Drawing.Size(289, 21);
            this.To.TabIndex = 7;
            this.To.DropDown += new System.EventHandler(this.To_DropDown);
            // 
            // ToBrowse
            // 
            this.ToBrowse.Location = new System.Drawing.Point(373, 45);
            this.ToBrowse.Name = "ToBrowse";
            this.ToBrowse.Size = new System.Drawing.Size(75, 23);
            this.ToBrowse.TabIndex = 6;
            this.ToBrowse.Text = "Browse";
            this.ToBrowse.UseVisualStyleBackColor = true;
            this.ToBrowse.Click += new System.EventHandler(this.ToBrowse_Click);
            // 
            // FromBrowse
            // 
            this.FromBrowse.Location = new System.Drawing.Point(373, 17);
            this.FromBrowse.Name = "FromBrowse";
            this.FromBrowse.Size = new System.Drawing.Size(75, 23);
            this.FromBrowse.TabIndex = 5;
            this.FromBrowse.Text = "Browse";
            this.FromBrowse.UseVisualStyleBackColor = true;
            this.FromBrowse.Click += new System.EventHandler(this.FromBrowse_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "To";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "From";
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(373, 2);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.TabIndex = 0;
            this.Ok.Text = "Clone";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // Central
            // 
            this.Central.AutoSize = true;
            this.Central.Location = new System.Drawing.Point(6, 42);
            this.Central.Name = "Central";
            this.Central.Size = new System.Drawing.Size(274, 17);
            this.Central.TabIndex = 1;
            this.Central.Text = "Central repository, no working dir  (--bare --shared=all)";
            this.Central.UseVisualStyleBackColor = true;
            // 
            // Personal
            // 
            this.Personal.AutoSize = true;
            this.Personal.Checked = true;
            this.Personal.Location = new System.Drawing.Point(6, 19);
            this.Personal.Name = "Personal";
            this.Personal.Size = new System.Drawing.Size(114, 17);
            this.Personal.TabIndex = 0;
            this.Personal.TabStop = true;
            this.Personal.Text = "Personal repository";
            this.Personal.UseVisualStyleBackColor = true;
            // 
            // LoadSSHKey
            // 
            this.LoadSSHKey.Image = global::GitUI.Properties.Resources.putty;
            this.LoadSSHKey.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LoadSSHKey.Location = new System.Drawing.Point(0, 2);
            this.LoadSSHKey.Name = "LoadSSHKey";
            this.LoadSSHKey.Size = new System.Drawing.Size(123, 23);
            this.LoadSSHKey.TabIndex = 25;
            this.LoadSSHKey.Text = "Load SSH key";
            this.LoadSSHKey.UseVisualStyleBackColor = true;
            this.LoadSSHKey.Click += new System.EventHandler(this.LoadSSHKey_Click);
            // 
            // FormClone
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(460, 176);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormClone";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Clone";
            this.Load += new System.EventHandler(this.FormClone_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button ToBrowse;
        private System.Windows.Forms.Button FromBrowse;
        private System.Windows.Forms.ComboBox From;
        private System.Windows.Forms.ComboBox To;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton CentralRepository;
        private System.Windows.Forms.RadioButton PersonalRepository;
        private System.Windows.Forms.RadioButton Central;
        private System.Windows.Forms.RadioButton Personal;
        private System.Windows.Forms.Button LoadSSHKey;
    }
}