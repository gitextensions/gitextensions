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
            this.localChangesGB = new System.Windows.Forms.GroupBox();
            this.rbReset = new System.Windows.Forms.RadioButton();
            this.rbMerge = new System.Windows.Forms.RadioButton();
            this.localChangesGB.SuspendLayout();
            this.SuspendLayout();
            // 
            // btOk
            // 
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOk.Location = new System.Drawing.Point(280, 127);
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
            this.btCancel.Location = new System.Drawing.Point(373, 127);
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
            this.rbResetBranch.Size = new System.Drawing.Size(187, 16);
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
            this.rbCreateBranch.Size = new System.Drawing.Size(191, 16);
            this.rbCreateBranch.TabIndex = 8;
            this.rbCreateBranch.Text = "Create local branch with the name \'{0}\'";
            this.rbCreateBranch.UseVisualStyleBackColor = true;
            // 
            // rbDontCreate
            // 
            this.rbDontCreate.AutoSize = true;
            this.rbDontCreate.Location = new System.Drawing.Point(12, 72);
            this.rbDontCreate.Name = "rbDontCreate";
            this.rbDontCreate.Size = new System.Drawing.Size(140, 16);
            this.rbDontCreate.TabIndex = 9;
            this.rbDontCreate.Text = "Do not create local branch";
            this.rbDontCreate.UseVisualStyleBackColor = true;
            // 
            // localChangesGB
            // 
            this.localChangesGB.Controls.Add(this.rbReset);
            this.localChangesGB.Controls.Add(this.rbMerge);
            this.localChangesGB.Location = new System.Drawing.Point(12, 105);
            this.localChangesGB.Name = "localChangesGB";
            this.localChangesGB.Size = new System.Drawing.Size(200, 43);
            this.localChangesGB.TabIndex = 16;
            this.localChangesGB.TabStop = false;
            this.localChangesGB.Text = "Local changes";
            // 
            // rbReset
            // 
            this.rbReset.AutoSize = true;
            this.rbReset.Location = new System.Drawing.Point(104, 17);
            this.rbReset.Name = "rbReset";
            this.rbReset.Size = new System.Drawing.Size(46, 16);
            this.rbReset.TabIndex = 1;
            this.rbReset.TabStop = true;
            this.rbReset.Text = "Reset";
            this.rbReset.UseVisualStyleBackColor = true;
            // 
            // rbMerge
            // 
            this.rbMerge.AutoSize = true;
            this.rbMerge.Location = new System.Drawing.Point(13, 17);
            this.rbMerge.Name = "rbMerge";
            this.rbMerge.Size = new System.Drawing.Size(51, 16);
            this.rbMerge.TabIndex = 0;
            this.rbMerge.TabStop = true;
            this.rbMerge.Text = "Merge";
            this.rbMerge.UseVisualStyleBackColor = true;
            // 
            // FormCheckoutRemoteBranch
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 158);
            this.Controls.Add(this.localChangesGB);
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
            this.localChangesGB.ResumeLayout(false);
            this.localChangesGB.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.RadioButton rbResetBranch;
        private System.Windows.Forms.RadioButton rbCreateBranch;
        private System.Windows.Forms.RadioButton rbDontCreate;
        private System.Windows.Forms.GroupBox localChangesGB;
        private System.Windows.Forms.RadioButton rbMerge;
        private System.Windows.Forms.RadioButton rbReset;
    }
}