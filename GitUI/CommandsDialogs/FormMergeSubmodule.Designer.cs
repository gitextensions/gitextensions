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
            this.SuspendLayout();
            // 
            // btStageCurrent
            // 
            this.btStageCurrent.ForeColor = System.Drawing.Color.Black;
            this.btStageCurrent.Location = new System.Drawing.Point(263, 170);
            this.btStageCurrent.Name = "btStageCurrent";
            this.btStageCurrent.Size = new System.Drawing.Size(173, 25);
            this.btStageCurrent.TabIndex = 16;
            this.btStageCurrent.Text = "Stage Current";
            this.btStageCurrent.UseVisualStyleBackColor = true;
            this.btStageCurrent.Click += new System.EventHandler(this.btStageCurrent_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(12, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Base:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(37, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(203, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "There is a conflict on the submodule:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(12, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "Local:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(12, 98);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "Remote:";
            // 
            // tbBase
            // 
            this.tbBase.Location = new System.Drawing.Point(85, 39);
            this.tbBase.Name = "tbBase";
            this.tbBase.ReadOnly = true;
            this.tbBase.Size = new System.Drawing.Size(351, 23);
            this.tbBase.TabIndex = 7;
            // 
            // tbLocal
            // 
            this.tbLocal.Location = new System.Drawing.Point(85, 66);
            this.tbLocal.Name = "tbLocal";
            this.tbLocal.ReadOnly = true;
            this.tbLocal.Size = new System.Drawing.Size(351, 23);
            this.tbLocal.TabIndex = 8;
            // 
            // tbRemote
            // 
            this.tbRemote.Location = new System.Drawing.Point(85, 95);
            this.tbRemote.Name = "tbRemote";
            this.tbRemote.ReadOnly = true;
            this.tbRemote.Size = new System.Drawing.Size(351, 23);
            this.tbRemote.TabIndex = 9;
            // 
            // tbCurrent
            // 
            this.tbCurrent.Location = new System.Drawing.Point(85, 124);
            this.tbCurrent.Name = "tbCurrent";
            this.tbCurrent.ReadOnly = true;
            this.tbCurrent.Size = new System.Drawing.Size(351, 23);
            this.tbCurrent.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(12, 127);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 15);
            this.label5.TabIndex = 11;
            this.label5.Text = "Current:";
            // 
            // btOpenSubmodule
            // 
            this.btOpenSubmodule.ForeColor = System.Drawing.Color.Black;
            this.btOpenSubmodule.Image = global::GitUI.Properties.Resources.IconFolderSubmodule;
            this.btOpenSubmodule.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btOpenSubmodule.Location = new System.Drawing.Point(88, 170);
            this.btOpenSubmodule.Name = "btOpenSubmodule";
            this.btOpenSubmodule.Size = new System.Drawing.Size(155, 25);
            this.btOpenSubmodule.TabIndex = 12;
            this.btOpenSubmodule.Text = "Open submodule";
            this.btOpenSubmodule.UseVisualStyleBackColor = true;
            this.btOpenSubmodule.Click += new System.EventHandler(this.btOpenSubmodule_Click);
            // 
            // btRefresh
            // 
            this.btRefresh.ForeColor = System.Drawing.Color.Black;
            this.btRefresh.Image = global::GitUI.Properties.Resources.arrow_refresh;
            this.btRefresh.Location = new System.Drawing.Point(442, 122);
            this.btRefresh.Name = "btRefresh";
            this.btRefresh.Size = new System.Drawing.Size(27, 25);
            this.btRefresh.TabIndex = 13;
            this.btRefresh.UseVisualStyleBackColor = true;
            this.btRefresh.Click += new System.EventHandler(this.btRefresh_Click);
            // 
            // lbSubmodule
            // 
            this.lbSubmodule.AutoSize = true;
            this.lbSubmodule.ForeColor = System.Drawing.Color.Black;
            this.lbSubmodule.Location = new System.Drawing.Point(246, 9);
            this.lbSubmodule.Name = "lbSubmodule";
            this.lbSubmodule.Size = new System.Drawing.Size(68, 15);
            this.lbSubmodule.TabIndex = 14;
            this.lbSubmodule.Text = "Submodule";
            // 
            // FormMergeSubmodule
            // 
            this.AcceptButton = this.btOpenSubmodule;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(476, 203);
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
    }
}