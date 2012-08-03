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
            this.lnkSettings = new System.Windows.Forms.LinkLabel();
            this.rbDontCreate = new System.Windows.Forms.RadioButton();
            this.rbCreateBranch = new System.Windows.Forms.RadioButton();
            this.rbResetBranch = new System.Windows.Forms.RadioButton();
            this.localChangesGB = new System.Windows.Forms.GroupBox();
            this.rbDontChange = new System.Windows.Forms.RadioButton();
            this.rbReset = new System.Windows.Forms.RadioButton();
            this.rbMerge = new System.Windows.Forms.RadioButton();
            this.localChangesGB.SuspendLayout();
            this.SuspendLayout();
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOk.Location = new System.Drawing.Point(330, 92);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(87, 25);
            this.btOk.TabIndex = 2;
            this.btOk.Text = "Checkout";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.OkClick);
            // 
            // lnkSettings
            // 
            this.lnkSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkSettings.AutoSize = true;
            this.lnkSettings.Location = new System.Drawing.Point(12, 99);
            this.lnkSettings.Name = "lnkSettings";
            this.lnkSettings.Size = new System.Drawing.Size(81, 15);
            this.lnkSettings.TabIndex = 20;
            this.lnkSettings.TabStop = true;
            this.lnkSettings.Text = "Show Settings";
            this.lnkSettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkSettings_LinkClicked);
            // 
            // rbDontCreate
            // 
            this.rbDontCreate.AutoSize = true;
            this.rbDontCreate.Location = new System.Drawing.Point(12, 63);
            this.rbDontCreate.Name = "rbDontCreate";
            this.rbDontCreate.Size = new System.Drawing.Size(164, 19);
            this.rbDontCreate.TabIndex = 9;
            this.rbDontCreate.Text = "Do not create local branch";
            this.rbDontCreate.UseVisualStyleBackColor = true;
            // 
            // rbCreateBranch
            // 
            this.rbCreateBranch.AutoSize = true;
            this.rbCreateBranch.Location = new System.Drawing.Point(12, 37);
            this.rbCreateBranch.Name = "rbCreateBranch";
            this.rbCreateBranch.Size = new System.Drawing.Size(229, 19);
            this.rbCreateBranch.TabIndex = 8;
            this.rbCreateBranch.Text = "Create local branch with the name \'{0}\'";
            this.rbCreateBranch.UseVisualStyleBackColor = true;
            // 
            // rbResetBranch
            // 
            this.rbResetBranch.AutoSize = true;
            this.rbResetBranch.Checked = true;
            this.rbResetBranch.Location = new System.Drawing.Point(12, 12);
            this.rbResetBranch.Name = "rbResetBranch";
            this.rbResetBranch.Size = new System.Drawing.Size(223, 19);
            this.rbResetBranch.TabIndex = 7;
            this.rbResetBranch.TabStop = true;
            this.rbResetBranch.Text = "Reset local branch with the name \'{0}\'";
            this.rbResetBranch.UseVisualStyleBackColor = true;
            // 
            // localChangesGB
            // 
            this.localChangesGB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.localChangesGB.Controls.Add(this.rbDontChange);
            this.localChangesGB.Controls.Add(this.rbReset);
            this.localChangesGB.Controls.Add(this.rbMerge);
            this.localChangesGB.Location = new System.Drawing.Point(12, 74);
            this.localChangesGB.Name = "localChangesGB";
            this.localChangesGB.Size = new System.Drawing.Size(300, 43);
            this.localChangesGB.TabIndex = 16;
            this.localChangesGB.TabStop = false;
            this.localChangesGB.Text = "Local changes";
            this.localChangesGB.Visible = false;
            // 
            // rbDontChange
            // 
            this.rbDontChange.AutoSize = true;
            this.rbDontChange.Location = new System.Drawing.Point(176, 17);
            this.rbDontChange.Name = "rbDontChange";
            this.rbDontChange.Size = new System.Drawing.Size(96, 19);
            this.rbDontChange.TabIndex = 2;
            this.rbDontChange.TabStop = true;
            this.rbDontChange.Text = "Don\'t change";
            this.rbDontChange.UseVisualStyleBackColor = true;
            // 
            // rbReset
            // 
            this.rbReset.AutoSize = true;
            this.rbReset.Location = new System.Drawing.Point(97, 17);
            this.rbReset.Name = "rbReset";
            this.rbReset.Size = new System.Drawing.Size(53, 19);
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
            this.rbMerge.Size = new System.Drawing.Size(59, 19);
            this.rbMerge.TabIndex = 0;
            this.rbMerge.TabStop = true;
            this.rbMerge.Text = "Merge";
            this.rbMerge.UseVisualStyleBackColor = true;
            // 
            // FormCheckoutRemoteBranch
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(429, 127);
            this.Controls.Add(this.lnkSettings);
            this.Controls.Add(this.rbDontCreate);
            this.Controls.Add(this.rbCreateBranch);
            this.Controls.Add(this.rbResetBranch);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.localChangesGB);
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
        private System.Windows.Forms.RadioButton rbResetBranch;
        private System.Windows.Forms.RadioButton rbCreateBranch;
        private System.Windows.Forms.RadioButton rbDontCreate;
        private System.Windows.Forms.GroupBox localChangesGB;
        private System.Windows.Forms.RadioButton rbMerge;
        private System.Windows.Forms.RadioButton rbReset;
        private System.Windows.Forms.RadioButton rbDontChange;
        private System.Windows.Forms.LinkLabel lnkSettings;
    }
}