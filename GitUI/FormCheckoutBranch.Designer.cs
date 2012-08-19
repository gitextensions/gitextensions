using System;

namespace GitUI
{
    partial class FormCheckoutBranch
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
            this.Branches = new System.Windows.Forms.ComboBox();
            this.Ok = new System.Windows.Forms.Button();
            this.LocalBranch = new System.Windows.Forms.RadioButton();
            this.Remotebranch = new System.Windows.Forms.RadioButton();
            this.localChangesGB = new System.Windows.Forms.GroupBox();
            this.rbStash = new System.Windows.Forms.RadioButton();
            this.rbDontChange = new System.Windows.Forms.RadioButton();
            this.rbReset = new System.Windows.Forms.RadioButton();
            this.rbMerge = new System.Windows.Forms.RadioButton();
            this.lnkSettings = new System.Windows.Forms.LinkLabel();
            this.localChangesGB.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select branch";
            // 
            // Branches
            // 
            this.Branches.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Branches.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.Branches.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(158, 42);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(318, 29);
            this.Branches.TabIndex = 1;
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(389, 72);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(87, 25);
            this.Ok.TabIndex = 2;
            this.Ok.Text = "Checkout";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // LocalBranch
            // 
            this.LocalBranch.AutoSize = true;
            this.LocalBranch.Checked = true;
            this.LocalBranch.Location = new System.Drawing.Point(13, 13);
            this.LocalBranch.Name = "LocalBranch";
            this.LocalBranch.Size = new System.Drawing.Size(127, 25);
            this.LocalBranch.TabIndex = 3;
            this.LocalBranch.TabStop = true;
            this.LocalBranch.Text = "Local branch";
            this.LocalBranch.UseVisualStyleBackColor = true;
            this.LocalBranch.CheckedChanged += new System.EventHandler(this.LocalBranchCheckedChanged);
            // 
            // Remotebranch
            // 
            this.Remotebranch.AutoSize = true;
            this.Remotebranch.Location = new System.Drawing.Point(168, 13);
            this.Remotebranch.Name = "Remotebranch";
            this.Remotebranch.Size = new System.Drawing.Size(147, 25);
            this.Remotebranch.TabIndex = 4;
            this.Remotebranch.Text = "Remote branch";
            this.Remotebranch.UseVisualStyleBackColor = true;
            this.Remotebranch.CheckedChanged += new System.EventHandler(this.RemoteBranchCheckedChanged);
            // 
            // localChangesGB
            // 
            this.localChangesGB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.localChangesGB.Controls.Add(this.rbStash);
            this.localChangesGB.Controls.Add(this.rbDontChange);
            this.localChangesGB.Controls.Add(this.rbReset);
            this.localChangesGB.Controls.Add(this.rbMerge);
            this.localChangesGB.Location = new System.Drawing.Point(12, 54);
            this.localChangesGB.Name = "localChangesGB";
            this.localChangesGB.Size = new System.Drawing.Size(369, 43);
            this.localChangesGB.TabIndex = 17;
            this.localChangesGB.TabStop = false;
            this.localChangesGB.Text = "Local changes";
            this.localChangesGB.Visible = false;
            // 
            // rbStash
            // 
            this.rbStash.AutoSize = true;
            this.rbStash.Location = new System.Drawing.Point(169, 17);
            this.rbStash.Name = "rbStash";
            this.rbStash.Size = new System.Drawing.Size(75, 24);
            this.rbStash.TabIndex = 3;
            this.rbStash.Text = "Stash";
            this.rbStash.UseVisualStyleBackColor = true;
            // 
            // rbDontChange
            // 
            this.rbDontChange.AutoSize = true;
            this.rbDontChange.Location = new System.Drawing.Point(240, 17);
            this.rbDontChange.Name = "rbDontChange";
            this.rbDontChange.Size = new System.Drawing.Size(128, 24);
            this.rbDontChange.TabIndex = 2;
            this.rbDontChange.Text = "Don\'t change";
            this.rbDontChange.UseVisualStyleBackColor = true;
            // 
            // rbReset
            // 
            this.rbReset.AutoSize = true;
            this.rbReset.Location = new System.Drawing.Point(97, 17);
            this.rbReset.Name = "rbReset";
            this.rbReset.Size = new System.Drawing.Size(76, 24);
            this.rbReset.TabIndex = 1;
            this.rbReset.Text = "Reset";
            this.rbReset.UseVisualStyleBackColor = true;
            // 
            // rbMerge
            // 
            this.rbMerge.AutoSize = true;
            this.rbMerge.Location = new System.Drawing.Point(13, 17);
            this.rbMerge.Name = "rbMerge";
            this.rbMerge.Size = new System.Drawing.Size(78, 24);
            this.rbMerge.TabIndex = 0;
            this.rbMerge.Text = "Merge";
            this.rbMerge.UseVisualStyleBackColor = true;
            // 
            // lnkSettings
            // 
            this.lnkSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkSettings.AutoSize = true;
            this.lnkSettings.Location = new System.Drawing.Point(13, 78);
            this.lnkSettings.Name = "lnkSettings";
            this.lnkSettings.Size = new System.Drawing.Size(115, 21);
            this.lnkSettings.TabIndex = 19;
            this.lnkSettings.TabStop = true;
            this.lnkSettings.Text = "Show Settings";
            this.lnkSettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkSettings_LinkClicked);
            // 
            // FormCheckoutBranch
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.ClientSize = new System.Drawing.Size(488, 109);
            this.Controls.Add(this.lnkSettings);
            this.Controls.Add(this.Remotebranch);
            this.Controls.Add(this.LocalBranch);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Branches);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.localChangesGB);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCheckoutBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Checkout branch";
            this.localChangesGB.ResumeLayout(false);
            this.localChangesGB.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox Branches;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.RadioButton LocalBranch;
        private System.Windows.Forms.RadioButton Remotebranch;
        private System.Windows.Forms.GroupBox localChangesGB;
        private System.Windows.Forms.RadioButton rbDontChange;
        private System.Windows.Forms.RadioButton rbReset;
        private System.Windows.Forms.RadioButton rbMerge;
        private System.Windows.Forms.LinkLabel lnkSettings;
        private System.Windows.Forms.RadioButton rbStash;
    }
}
