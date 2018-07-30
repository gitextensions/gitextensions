namespace GitUI.CommandsDialogs
{
    partial class FormMergeSubmodule
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
            this.btStageCurrent = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbBase = new System.Windows.Forms.TextBox();
            this.tbLocal = new System.Windows.Forms.TextBox();
            this.tbRemote = new System.Windows.Forms.TextBox();
            this.tbCurrent = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btOpenSubmodule = new System.Windows.Forms.Button();
            this.btRefresh = new System.Windows.Forms.Button();
            this.lbSubmodule = new System.Windows.Forms.Label();
            this.btCheckoutBranch = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btStageCurrent
            // 
            this.btStageCurrent.Location = new System.Drawing.Point(392, 212);
            this.btStageCurrent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btStageCurrent.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btStageCurrent.Name = "btStageCurrent";
            this.btStageCurrent.Size = new System.Drawing.Size(190, 31);
            this.btStageCurrent.TabIndex = 16;
            this.btStageCurrent.Text = "Stage Current";
            this.btStageCurrent.UseVisualStyleBackColor = true;
            this.btStageCurrent.Click += new System.EventHandler(this.btStageCurrent_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(15, 52);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Base:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(46, 11);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(252, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "There is a conflict on the submodule:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(15, 86);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "Local:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(15, 122);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 20);
            this.label4.TabIndex = 6;
            this.label4.Text = "Remote:";
            // 
            // tbBase
            // 
            this.tbBase.Location = new System.Drawing.Point(106, 49);
            this.tbBase.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbBase.Name = "tbBase";
            this.tbBase.ReadOnly = true;
            this.tbBase.Size = new System.Drawing.Size(438, 27);
            this.tbBase.TabIndex = 7;
            // 
            // tbLocal
            // 
            this.tbLocal.Location = new System.Drawing.Point(106, 82);
            this.tbLocal.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbLocal.Name = "tbLocal";
            this.tbLocal.ReadOnly = true;
            this.tbLocal.Size = new System.Drawing.Size(438, 27);
            this.tbLocal.TabIndex = 8;
            // 
            // tbRemote
            // 
            this.tbRemote.Location = new System.Drawing.Point(106, 119);
            this.tbRemote.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbRemote.Name = "tbRemote";
            this.tbRemote.ReadOnly = true;
            this.tbRemote.Size = new System.Drawing.Size(438, 27);
            this.tbRemote.TabIndex = 9;
            // 
            // tbCurrent
            // 
            this.tbCurrent.Location = new System.Drawing.Point(106, 155);
            this.tbCurrent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbCurrent.Name = "tbCurrent";
            this.tbCurrent.ReadOnly = true;
            this.tbCurrent.Size = new System.Drawing.Size(438, 27);
            this.tbCurrent.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(15, 159);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 20);
            this.label5.TabIndex = 11;
            this.label5.Text = "Current:";
            // 
            // btOpenSubmodule
            // 
            this.btOpenSubmodule.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btOpenSubmodule.Image = global::GitUI.Properties.Images.FolderSubmodule;
            this.btOpenSubmodule.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btOpenSubmodule.Location = new System.Drawing.Point(19, 212);
            this.btOpenSubmodule.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btOpenSubmodule.Name = "btOpenSubmodule";
            this.btOpenSubmodule.Size = new System.Drawing.Size(176, 31);
            this.btOpenSubmodule.TabIndex = 12;
            this.btOpenSubmodule.Text = "Open submodule";
            this.btOpenSubmodule.UseVisualStyleBackColor = true;
            this.btOpenSubmodule.Click += new System.EventHandler(this.btOpenSubmodule_Click);
            // 
            // btRefresh
            // 
            this.btRefresh.ForeColor = System.Drawing.Color.Black;
            this.btRefresh.Image = global::GitUI.Properties.Images.ReloadRevisions;
            this.btRefresh.Location = new System.Drawing.Point(552, 152);
            this.btRefresh.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btRefresh.Name = "btRefresh";
            this.btRefresh.Size = new System.Drawing.Size(34, 31);
            this.btRefresh.TabIndex = 13;
            this.btRefresh.UseVisualStyleBackColor = true;
            this.btRefresh.Click += new System.EventHandler(this.btRefresh_Click);
            // 
            // lbSubmodule
            // 
            this.lbSubmodule.AutoSize = true;
            this.lbSubmodule.ForeColor = System.Drawing.Color.Black;
            this.lbSubmodule.Location = new System.Drawing.Point(308, 11);
            this.lbSubmodule.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbSubmodule.Name = "lbSubmodule";
            this.lbSubmodule.Size = new System.Drawing.Size(85, 20);
            this.lbSubmodule.TabIndex = 14;
            this.lbSubmodule.Text = "Submodule";
            // 
            // btCheckoutBranch
            // 
            this.btCheckoutBranch.Location = new System.Drawing.Point(203, 212);
            this.btCheckoutBranch.Margin = new System.Windows.Forms.Padding(4);
            this.btCheckoutBranch.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btCheckoutBranch.Name = "btCheckoutBranch";
            this.btCheckoutBranch.Size = new System.Drawing.Size(181, 31);
            this.btCheckoutBranch.TabIndex = 17;
            this.btCheckoutBranch.Text = "Checkout Branch";
            this.btCheckoutBranch.UseVisualStyleBackColor = true;
            this.btCheckoutBranch.Click += new System.EventHandler(this.btCheckoutBranch_Click);
            // 
            // FormMergeSubmodule
            // 
            this.AcceptButton = this.btOpenSubmodule;
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(595, 254);
            this.Controls.Add(this.btCheckoutBranch);
            this.Controls.Add(this.lbSubmodule);
            this.Controls.Add(this.btRefresh);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbCurrent);
            this.Controls.Add(this.tbRemote);
            this.Controls.Add(this.tbLocal);
            this.Controls.Add(this.tbBase);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btStageCurrent);
            this.Controls.Add(this.btOpenSubmodule);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMergeSubmodule";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Submodule conflict";
            this.Load += new System.EventHandler(this.FormMergeSubmodule_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btStageCurrent;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbBase;
        private System.Windows.Forms.TextBox tbLocal;
        private System.Windows.Forms.TextBox tbRemote;
        private System.Windows.Forms.TextBox tbCurrent;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btOpenSubmodule;
        private System.Windows.Forms.Button btRefresh;
        private System.Windows.Forms.Label lbSubmodule;
        private System.Windows.Forms.Button btCheckoutBranch;
    }
}