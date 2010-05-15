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
            this.CheckoutAfterCreate = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 5;
            // 
            // Ok
            // 
            resources.ApplyResources(this.Ok, "Ok");
            this.Ok.Location = new System.Drawing.Point(356, 5);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(108, 23);
            this.Ok.TabIndex = 4;
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // BName
            // 
            resources.ApplyResources(this.BName, "BName");
            this.BName.Location = new System.Drawing.Point(127, 6);
            this.BName.Name = "BName";
            this.BName.Size = new System.Drawing.Size(223, 20);
            this.BName.TabIndex = 3;
            this.BName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.BName_KeyUp);
            // 
            // CheckoutAfterCreate
            // 
            resources.ApplyResources(this.CheckoutAfterCreate, "CheckoutAfterCreate");
            this.CheckoutAfterCreate.AutoSize = true;
            this.CheckoutAfterCreate.Checked = true;
            this.CheckoutAfterCreate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckoutAfterCreate.Location = new System.Drawing.Point(127, 32);
            this.CheckoutAfterCreate.Name = "CheckoutAfterCreate";
            this.CheckoutAfterCreate.Size = new System.Drawing.Size(129, 17);
            this.CheckoutAfterCreate.TabIndex = 6;
            this.CheckoutAfterCreate.UseVisualStyleBackColor = true;
            // 
            // FormBranchSmall
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(471, 58);
            this.Controls.Add(this.CheckoutAfterCreate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.BName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            //this.Icon = global::GitUI.Properties.Resources.cow_head;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormBranchSmall";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TextBox BName;
        private System.Windows.Forms.CheckBox CheckoutAfterCreate;
    }
}