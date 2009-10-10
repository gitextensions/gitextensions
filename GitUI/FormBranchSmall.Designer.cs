namespace GitUI
{
    partial class FormBranchSmall
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBranchSmall));
            this.label1 = new System.Windows.Forms.Label();
            this.Ok = new System.Windows.Forms.Button();
            this.BName = new System.Windows.Forms.TextBox();
            this.ChechoutAfterCreate = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Branch name";
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(317, 5);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(108, 23);
            this.Ok.TabIndex = 4;
            this.Ok.Text = "Create branch";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // BName
            // 
            this.BName.Location = new System.Drawing.Point(88, 6);
            this.BName.Name = "BName";
            this.BName.Size = new System.Drawing.Size(223, 20);
            this.BName.TabIndex = 3;
            this.BName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.BName_KeyUp);
            // 
            // ChechoutAfterCreate
            // 
            this.ChechoutAfterCreate.AutoSize = true;
            this.ChechoutAfterCreate.Checked = true;
            this.ChechoutAfterCreate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ChechoutAfterCreate.Location = new System.Drawing.Point(88, 32);
            this.ChechoutAfterCreate.Name = "ChechoutAfterCreate";
            this.ChechoutAfterCreate.Size = new System.Drawing.Size(129, 17);
            this.ChechoutAfterCreate.TabIndex = 6;
            this.ChechoutAfterCreate.Text = "Chechout after create";
            this.ChechoutAfterCreate.UseVisualStyleBackColor = true;
            // 
            // FormBranchSmall
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 58);
            this.Controls.Add(this.ChechoutAfterCreate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.BName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormBranchSmall";
            this.Text = "Create branch";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TextBox BName;
        private System.Windows.Forms.CheckBox ChechoutAfterCreate;
    }
}