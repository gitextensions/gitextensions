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
            if (disposing && (components is not null))
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
            btStageCurrent = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            tbBase = new TextBox();
            tbLocal = new TextBox();
            tbRemote = new TextBox();
            tbCurrent = new TextBox();
            label5 = new Label();
            btOpenSubmodule = new Button();
            btRefresh = new Button();
            lbSubmodule = new Label();
            btCheckoutBranch = new Button();
            SuspendLayout();
            // 
            // btStageCurrent
            // 
            btStageCurrent.Location = new Point(392, 212);
            btStageCurrent.Margin = new Padding(4, 4, 4, 4);
            btStageCurrent.ForeColor = SystemColors.ControlText;
            btStageCurrent.Name = "btStageCurrent";
            btStageCurrent.Size = new Size(190, 31);
            btStageCurrent.TabIndex = 16;
            btStageCurrent.Text = "Stage Current";
            btStageCurrent.UseVisualStyleBackColor = true;
            btStageCurrent.Click += btStageCurrent_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = SystemColors.ControlText;
            label1.Location = new Point(15, 52);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(43, 20);
            label1.TabIndex = 1;
            label1.Text = "Base:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = SystemColors.ControlText;
            label2.Location = new Point(46, 11);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(252, 20);
            label2.TabIndex = 0;
            label2.Text = "There is a conflict on the submodule:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.ForeColor = SystemColors.ControlText;
            label3.Location = new Point(15, 86);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(47, 20);
            label3.TabIndex = 5;
            label3.Text = "Local:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.ForeColor = SystemColors.ControlText;
            label4.Location = new Point(15, 122);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(64, 20);
            label4.TabIndex = 6;
            label4.Text = "Remote:";
            // 
            // tbBase
            // 
            tbBase.Location = new Point(106, 49);
            tbBase.Margin = new Padding(4, 4, 4, 4);
            tbBase.Name = "tbBase";
            tbBase.ReadOnly = true;
            tbBase.Size = new Size(438, 27);
            tbBase.TabIndex = 7;
            // 
            // tbLocal
            // 
            tbLocal.Location = new Point(106, 82);
            tbLocal.Margin = new Padding(4, 4, 4, 4);
            tbLocal.Name = "tbLocal";
            tbLocal.ReadOnly = true;
            tbLocal.Size = new Size(438, 27);
            tbLocal.TabIndex = 8;
            // 
            // tbRemote
            // 
            tbRemote.Location = new Point(106, 119);
            tbRemote.Margin = new Padding(4, 4, 4, 4);
            tbRemote.Name = "tbRemote";
            tbRemote.ReadOnly = true;
            tbRemote.Size = new Size(438, 27);
            tbRemote.TabIndex = 9;
            // 
            // tbCurrent
            // 
            tbCurrent.Location = new Point(106, 155);
            tbCurrent.Margin = new Padding(4, 4, 4, 4);
            tbCurrent.Name = "tbCurrent";
            tbCurrent.ReadOnly = true;
            tbCurrent.Size = new Size(438, 27);
            tbCurrent.TabIndex = 10;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.ForeColor = SystemColors.ControlText;
            label5.Location = new Point(15, 159);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(60, 20);
            label5.TabIndex = 11;
            label5.Text = "Current:";
            // 
            // btOpenSubmodule
            // 
            btOpenSubmodule.ForeColor = SystemColors.ControlText;
            btOpenSubmodule.Image = Properties.Images.FolderSubmodule;
            btOpenSubmodule.ImageAlign = ContentAlignment.MiddleLeft;
            btOpenSubmodule.Location = new Point(19, 212);
            btOpenSubmodule.Margin = new Padding(4, 4, 4, 4);
            btOpenSubmodule.Name = "btOpenSubmodule";
            btOpenSubmodule.Size = new Size(176, 31);
            btOpenSubmodule.TabIndex = 12;
            btOpenSubmodule.Text = "Open submodule";
            btOpenSubmodule.UseVisualStyleBackColor = true;
            btOpenSubmodule.Click += btOpenSubmodule_Click;
            // 
            // btRefresh
            // 
            btRefresh.ForeColor = SystemColors.ControlText;
            btRefresh.Image = Properties.Images.ReloadRevisions;
            btRefresh.Location = new Point(552, 152);
            btRefresh.Margin = new Padding(4, 4, 4, 4);
            btRefresh.Name = "btRefresh";
            btRefresh.Size = new Size(34, 31);
            btRefresh.TabIndex = 13;
            btRefresh.UseVisualStyleBackColor = true;
            btRefresh.Click += btRefresh_Click;
            // 
            // lbSubmodule
            // 
            lbSubmodule.AutoSize = true;
            lbSubmodule.ForeColor = SystemColors.ControlText;
            lbSubmodule.Location = new Point(308, 11);
            lbSubmodule.Margin = new Padding(4, 0, 4, 0);
            lbSubmodule.Name = "lbSubmodule";
            lbSubmodule.Size = new Size(85, 20);
            lbSubmodule.TabIndex = 14;
            lbSubmodule.Text = "Submodule";
            // 
            // btCheckoutBranch
            // 
            btCheckoutBranch.Location = new Point(203, 212);
            btCheckoutBranch.Margin = new Padding(4);
            btCheckoutBranch.ForeColor = SystemColors.ControlText;
            btCheckoutBranch.Name = "btCheckoutBranch";
            btCheckoutBranch.Size = new Size(181, 31);
            btCheckoutBranch.TabIndex = 17;
            btCheckoutBranch.Text = "Checkout Branch";
            btCheckoutBranch.UseVisualStyleBackColor = true;
            btCheckoutBranch.Click += btCheckoutBranch_Click;
            // 
            // FormMergeSubmodule
            // 
            AcceptButton = btOpenSubmodule;
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(595, 254);
            Controls.Add(btCheckoutBranch);
            Controls.Add(lbSubmodule);
            Controls.Add(btRefresh);
            Controls.Add(label5);
            Controls.Add(tbCurrent);
            Controls.Add(tbRemote);
            Controls.Add(tbLocal);
            Controls.Add(tbBase);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(btStageCurrent);
            Controls.Add(btOpenSubmodule);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4, 4, 4, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormMergeSubmodule";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Submodule conflict";
            Load += FormMergeSubmodule_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button btStageCurrent;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private TextBox tbBase;
        private TextBox tbLocal;
        private TextBox tbRemote;
        private TextBox tbCurrent;
        private Label label5;
        private Button btOpenSubmodule;
        private Button btRefresh;
        private Label lbSubmodule;
        private Button btCheckoutBranch;
    }
}