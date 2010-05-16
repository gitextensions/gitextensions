﻿namespace GitUI
{
    partial class FormMergeBranch
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMergeBranch));
            this.label1 = new System.Windows.Forms.Label();
            this.Currentbranch = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Branches = new System.Windows.Forms.ComboBox();
            this.Ok = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.NoFastForward = new System.Windows.Forms.CheckBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 16);
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(206, 13);
            this.label1.TabIndex = 0;
            // 
            // Currentbranch
            // 
            this.Currentbranch.AutoSize = true;
            this.Currentbranch.Location = new System.Drawing.Point(10, 42);
            resources.ApplyResources(this.Currentbranch, "Currentbranch");
            this.Currentbranch.Name = "Currentbranch";
            this.Currentbranch.Size = new System.Drawing.Size(41, 13);
            this.Currentbranch.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 68);
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 2;
            // 
            // Branches
            // 
            resources.ApplyResources(this.Branches, "Branches");
            this.Branches.FormattingEnabled = true;
            this.Branches.Location = new System.Drawing.Point(95, 65);
            this.Branches.Name = "Branches";
            this.Branches.Size = new System.Drawing.Size(327, 21);
            this.Branches.TabIndex = 3;
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(347, 208);
            resources.ApplyResources(this.Ok, "Ok");
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.TabIndex = 4;
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox1);
            // 
            // splitContainer1.Panel2
            // 
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Size = new System.Drawing.Size(514, 243);
            this.splitContainer1.SplitterDistance = 80;
            this.splitContainer1.TabIndex = 5;
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.BackgroundImage = global::GitUI.Properties.Resources.merge;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(80, 243);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.NoFastForward);
            this.groupBox1.Controls.Add(this.Ok);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.Branches);
            this.groupBox1.Controls.Add(this.Currentbranch);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(430, 243);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // NoFastForward
            // 
            this.NoFastForward.AutoSize = true;
            this.NoFastForward.Location = new System.Drawing.Point(95, 102);
            resources.ApplyResources(this.NoFastForward, "NoFastForward");
            this.NoFastForward.Name = "NoFastForward";
            this.NoFastForward.Size = new System.Drawing.Size(98, 17);
            this.NoFastForward.TabIndex = 5;
            this.NoFastForward.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.NoFastForward.UseVisualStyleBackColor = true;
            // 
            // FormMergeBranch
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 243);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            //this.Icon = global::GitUI.Properties.Resources.cow_head;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMergeBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.FormMergeBranch_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label Currentbranch;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox Branches;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox NoFastForward;
    }
}