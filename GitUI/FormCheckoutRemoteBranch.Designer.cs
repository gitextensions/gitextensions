using System;

namespace GitUI
{
    partial class FormCheckoutRemoteBranch
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
            this.btOk = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.rbResetBranch = new System.Windows.Forms.RadioButton();
            this.rbCreateBranch = new System.Windows.Forms.RadioButton();
            this.rbDontCreate = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // btOk
            // 
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOk.Location = new System.Drawing.Point(280, 104);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(87, 25);
            this.btOk.TabIndex = 2;
            this.btOk.Text = "Checkout";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.OkClick);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(373, 104);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(87, 25);
            this.btCancel.TabIndex = 6;
            this.btCancel.Text = "Cancel";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // rbResetBranch
            // 
            this.rbResetBranch.AutoSize = true;
            this.rbResetBranch.Checked = true;
            this.rbResetBranch.Location = new System.Drawing.Point(12, 12);
            this.rbResetBranch.Name = "rbResetBranch";
            this.rbResetBranch.Size = new System.Drawing.Size(295, 24);
            this.rbResetBranch.TabIndex = 7;
            this.rbResetBranch.TabStop = true;
            this.rbResetBranch.Text = "Reset local branch with the name \'{0}\'";
            this.rbResetBranch.UseVisualStyleBackColor = true;
            // 
            // rbCreateBranch
            // 
            this.rbCreateBranch.AutoSize = true;
            this.rbCreateBranch.Location = new System.Drawing.Point(12, 42);
            this.rbCreateBranch.Name = "rbCreateBranch";
            this.rbCreateBranch.Size = new System.Drawing.Size(284, 24);
            this.rbCreateBranch.TabIndex = 8;
            this.rbCreateBranch.Text = "Create local branch with the name \'{0}\'";
            this.rbCreateBranch.UseVisualStyleBackColor = true;
            // 
            // rbDontCreate
            // 
            this.rbDontCreate.AutoSize = true;
            this.rbDontCreate.Location = new System.Drawing.Point(12, 72);
            this.rbDontCreate.Name = "rbDontCreate";
            this.rbDontCreate.Size = new System.Drawing.Size(206, 24);
            this.rbDontCreate.TabIndex = 9;
            this.rbDontCreate.Text = "Do not create local branch";
            this.rbDontCreate.UseVisualStyleBackColor = true;
            // 
            // FormCheckoutRemoteBranch
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 139);
            this.Controls.Add(this.rbDontCreate);
            this.Controls.Add(this.rbCreateBranch);
            this.Controls.Add(this.rbResetBranch);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCheckoutRemoteBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Checkout remote branch \'{0}\'";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.RadioButton rbResetBranch;
        private System.Windows.Forms.RadioButton rbCreateBranch;
        private System.Windows.Forms.RadioButton rbDontCreate;
    }
}