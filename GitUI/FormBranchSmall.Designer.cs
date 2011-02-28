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
            this.label1 = new System.Windows.Forms.Label();
            this.Ok = new System.Windows.Forms.Button();
            this.BranchNameTextBox = new System.Windows.Forms.TextBox();
            this.CheckoutAfterCreate = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Branch name";
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Location = new System.Drawing.Point(336, 5);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(128, 25);
            this.Ok.TabIndex = 4;
            this.Ok.Text = "Create branch";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // BranchNameTextBox
            // 
            this.BranchNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.BranchNameTextBox.Location = new System.Drawing.Point(143, 6);
            this.BranchNameTextBox.Name = "BranchNameTextBox";
            this.BranchNameTextBox.Size = new System.Drawing.Size(187, 21);
            this.BranchNameTextBox.TabIndex = 3;
            this.BranchNameTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.BranchNameTextBoxKeyUp);
            // 
            // CheckoutAfterCreate
            // 
            this.CheckoutAfterCreate.AutoSize = true;
            this.CheckoutAfterCreate.Checked = true;
            this.CheckoutAfterCreate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckoutAfterCreate.Location = new System.Drawing.Point(143, 33);
            this.CheckoutAfterCreate.Name = "CheckoutAfterCreate";
            this.CheckoutAfterCreate.Size = new System.Drawing.Size(132, 17);
            this.CheckoutAfterCreate.TabIndex = 6;
            this.CheckoutAfterCreate.Text = "Checkout after create";
            this.CheckoutAfterCreate.UseVisualStyleBackColor = true;
            // 
            // FormBranchSmall
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(471, 58);
            this.Controls.Add(this.CheckoutAfterCreate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.BranchNameTextBox);
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
        private System.Windows.Forms.TextBox BranchNameTextBox;
        private System.Windows.Forms.CheckBox CheckoutAfterCreate;
    }
}