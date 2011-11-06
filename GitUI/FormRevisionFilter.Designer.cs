﻿namespace GitUI
{
    partial class FormRevisionFilter
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
            this.Ok = new System.Windows.Forms.Button();
            this._NO_TRANSLATE_Limit = new System.Windows.Forms.NumericUpDown();
            this.Message = new System.Windows.Forms.TextBox();
            this.Author = new System.Windows.Forms.TextBox();
            this.Since = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Until = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.Committer = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.LimitCheck = new System.Windows.Forms.CheckBox();
            this.IgnoreCase = new System.Windows.Forms.CheckBox();
            this.MessageCheck = new System.Windows.Forms.CheckBox();
            this.CommitterCheck = new System.Windows.Forms.CheckBox();
            this.AuthorCheck = new System.Windows.Forms.CheckBox();
            this.CheckUntil = new System.Windows.Forms.CheckBox();
            this.SinceCheck = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.FileFilterCheck = new System.Windows.Forms.CheckBox();
            this.FileFilter = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.BranchFilter = new System.Windows.Forms.TextBox();
            this.CurrentBranchOnlyCheck = new System.Windows.Forms.CheckBox();
            this.BranchFilterCheck = new System.Windows.Forms.CheckBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_Limit)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Ok
            // 
            this.Ok.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Ok.Location = new System.Drawing.Point(375, 3);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 25);
            this.Ok.TabIndex = 19;
            this.Ok.Text = "OK";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // _NO_TRANSLATE_Limit
            // 
            this._NO_TRANSLATE_Limit.Location = new System.Drawing.Point(209, 153);
            this._NO_TRANSLATE_Limit.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this._NO_TRANSLATE_Limit.Name = "_NO_TRANSLATE_Limit";
            this._NO_TRANSLATE_Limit.Size = new System.Drawing.Size(116, 27);
            this._NO_TRANSLATE_Limit.TabIndex = 18;
            // 
            // Message
            // 
            this.Message.Location = new System.Drawing.Point(209, 103);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(241, 27);
            this.Message.TabIndex = 14;
            // 
            // Author
            // 
            this.Author.Location = new System.Drawing.Point(209, 53);
            this.Author.Name = "Author";
            this.Author.Size = new System.Drawing.Size(241, 27);
            this.Author.TabIndex = 9;
            // 
            // Since
            // 
            this.Since.Location = new System.Drawing.Point(209, 3);
            this.Since.Name = "Since";
            this.Since.Size = new System.Drawing.Size(200, 27);
            this.Since.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Since";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 20);
            this.label2.TabIndex = 6;
            this.label2.Text = "Until";
            // 
            // Until
            // 
            this.Until.Location = new System.Drawing.Point(209, 28);
            this.Until.Name = "Until";
            this.Until.Size = new System.Drawing.Size(200, 27);
            this.Until.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 20);
            this.label3.TabIndex = 8;
            this.label3.Text = "Author";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 75);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 20);
            this.label4.TabIndex = 11;
            this.label4.Text = "Committer";
            // 
            // Committer
            // 
            this.Committer.Location = new System.Drawing.Point(209, 78);
            this.Committer.Name = "Committer";
            this.Committer.Size = new System.Drawing.Size(241, 27);
            this.Committer.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 100);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 20);
            this.label5.TabIndex = 13;
            this.label5.Text = "Message";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 150);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(42, 20);
            this.label7.TabIndex = 17;
            this.label7.Text = "Limit";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 125);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(85, 20);
            this.label6.TabIndex = 15;
            this.label6.Text = "Ignore case";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 184F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 395F));
            this.tableLayoutPanel1.Controls.Add(this.LimitCheck, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.IgnoreCase, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.MessageCheck, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.CommitterCheck, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.AuthorCheck, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.CheckUntil, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.Committer, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.Until, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.Since, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.Author, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.Message, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_Limit, 2, 6);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.SinceCheck, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.FileFilterCheck, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.FileFilter, 2, 7);
            this.tableLayoutPanel1.Controls.Add(this.label9, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.BranchFilter, 2, 8);
            this.tableLayoutPanel1.Controls.Add(this.CurrentBranchOnlyCheck, 2, 9);
            this.tableLayoutPanel1.Controls.Add(this.BranchFilterCheck, 1, 8);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 10;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(457, 255);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // LimitCheck
            // 
            this.LimitCheck.AutoSize = true;
            this.LimitCheck.Location = new System.Drawing.Point(187, 153);
            this.LimitCheck.Name = "LimitCheck";
            this.LimitCheck.Size = new System.Drawing.Size(16, 17);
            this.LimitCheck.TabIndex = 27;
            this.LimitCheck.UseVisualStyleBackColor = true;
            this.LimitCheck.CheckedChanged += new System.EventHandler(this.SinceCheckCheckedChanged);
            // 
            // IgnoreCase
            // 
            this.IgnoreCase.AutoSize = true;
            this.IgnoreCase.Checked = true;
            this.IgnoreCase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.IgnoreCase.Location = new System.Drawing.Point(187, 128);
            this.IgnoreCase.Name = "IgnoreCase";
            this.IgnoreCase.Size = new System.Drawing.Size(16, 17);
            this.IgnoreCase.TabIndex = 26;
            this.IgnoreCase.UseVisualStyleBackColor = true;
            // 
            // MessageCheck
            // 
            this.MessageCheck.AutoSize = true;
            this.MessageCheck.Location = new System.Drawing.Point(187, 103);
            this.MessageCheck.Name = "MessageCheck";
            this.MessageCheck.Size = new System.Drawing.Size(16, 17);
            this.MessageCheck.TabIndex = 25;
            this.MessageCheck.UseVisualStyleBackColor = true;
            this.MessageCheck.CheckedChanged += new System.EventHandler(this.SinceCheckCheckedChanged);
            // 
            // CommitterCheck
            // 
            this.CommitterCheck.AutoSize = true;
            this.CommitterCheck.Location = new System.Drawing.Point(187, 78);
            this.CommitterCheck.Name = "CommitterCheck";
            this.CommitterCheck.Size = new System.Drawing.Size(16, 17);
            this.CommitterCheck.TabIndex = 24;
            this.CommitterCheck.UseVisualStyleBackColor = true;
            this.CommitterCheck.CheckedChanged += new System.EventHandler(this.SinceCheckCheckedChanged);
            // 
            // AuthorCheck
            // 
            this.AuthorCheck.AutoSize = true;
            this.AuthorCheck.Location = new System.Drawing.Point(187, 53);
            this.AuthorCheck.Name = "AuthorCheck";
            this.AuthorCheck.Size = new System.Drawing.Size(16, 17);
            this.AuthorCheck.TabIndex = 23;
            this.AuthorCheck.UseVisualStyleBackColor = true;
            this.AuthorCheck.CheckedChanged += new System.EventHandler(this.SinceCheckCheckedChanged);
            // 
            // CheckUntil
            // 
            this.CheckUntil.AutoSize = true;
            this.CheckUntil.Location = new System.Drawing.Point(187, 28);
            this.CheckUntil.Name = "CheckUntil";
            this.CheckUntil.Size = new System.Drawing.Size(16, 17);
            this.CheckUntil.TabIndex = 22;
            this.CheckUntil.UseVisualStyleBackColor = true;
            this.CheckUntil.CheckedChanged += new System.EventHandler(this.SinceCheckCheckedChanged);
            // 
            // SinceCheck
            // 
            this.SinceCheck.AutoSize = true;
            this.SinceCheck.Location = new System.Drawing.Point(187, 3);
            this.SinceCheck.Name = "SinceCheck";
            this.SinceCheck.Size = new System.Drawing.Size(16, 17);
            this.SinceCheck.TabIndex = 21;
            this.SinceCheck.UseVisualStyleBackColor = true;
            this.SinceCheck.CheckedChanged += new System.EventHandler(this.SinceCheckCheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 175);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(67, 20);
            this.label8.TabIndex = 28;
            this.label8.Text = "File filter";
            // 
            // FileFilterCheck
            // 
            this.FileFilterCheck.AutoSize = true;
            this.FileFilterCheck.Location = new System.Drawing.Point(187, 178);
            this.FileFilterCheck.Name = "FileFilterCheck";
            this.FileFilterCheck.Size = new System.Drawing.Size(16, 17);
            this.FileFilterCheck.TabIndex = 29;
            this.FileFilterCheck.UseVisualStyleBackColor = true;
            this.FileFilterCheck.CheckedChanged += new System.EventHandler(this.SinceCheckCheckedChanged);
            // 
            // FileFilter
            // 
            this.FileFilter.Location = new System.Drawing.Point(209, 178);
            this.FileFilter.Name = "FileFilter";
            this.FileFilter.Size = new System.Drawing.Size(241, 27);
            this.FileFilter.TabIndex = 30;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 200);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(68, 20);
            this.label9.TabIndex = 31;
            this.label9.Text = "Branches";
            // 
            // BranchFilter
            // 
            this.BranchFilter.Location = new System.Drawing.Point(209, 203);
            this.BranchFilter.Name = "BranchFilter";
            this.BranchFilter.Size = new System.Drawing.Size(241, 27);
            this.BranchFilter.TabIndex = 32;
            // 
            // CurrentBranchOnlyCheck
            // 
            this.CurrentBranchOnlyCheck.AutoSize = true;
            this.CurrentBranchOnlyCheck.Location = new System.Drawing.Point(209, 228);
            this.CurrentBranchOnlyCheck.Name = "CurrentBranchOnlyCheck";
            this.CurrentBranchOnlyCheck.Size = new System.Drawing.Size(198, 24);
            this.CurrentBranchOnlyCheck.TabIndex = 33;
            this.CurrentBranchOnlyCheck.Text = "Show current branch only";
            this.CurrentBranchOnlyCheck.UseVisualStyleBackColor = true;
            this.CurrentBranchOnlyCheck.CheckedChanged += new System.EventHandler(this.OnShowCurrentBranchOnlyCheckedChanged);
            // 
            // BranchFilterCheck
            // 
            this.BranchFilterCheck.AutoSize = true;
            this.BranchFilterCheck.Location = new System.Drawing.Point(187, 203);
            this.BranchFilterCheck.Name = "BranchFilterCheck";
            this.BranchFilterCheck.Size = new System.Drawing.Size(16, 17);
            this.BranchFilterCheck.TabIndex = 34;
            this.BranchFilterCheck.UseVisualStyleBackColor = true;
            this.BranchFilterCheck.CheckedChanged += new System.EventHandler(this.OnBranchFilterCheckedChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.Ok);
            this.splitContainer1.Size = new System.Drawing.Size(457, 288);
            this.splitContainer1.SplitterDistance = 255;
            this.splitContainer1.TabIndex = 21;
            // 
            // FormRevisionFilter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 288);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormRevisionFilter";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Filter";
            this.Load += new System.EventHandler(this.FormRevisionFilterLoad);
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_Limit)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_Limit;
        private System.Windows.Forms.TextBox Message;
        private System.Windows.Forms.TextBox Author;
        private System.Windows.Forms.DateTimePicker Since;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker Until;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox Committer;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox LimitCheck;
        private System.Windows.Forms.CheckBox IgnoreCase;
        private System.Windows.Forms.CheckBox MessageCheck;
        private System.Windows.Forms.CheckBox CommitterCheck;
        private System.Windows.Forms.CheckBox AuthorCheck;
        private System.Windows.Forms.CheckBox CheckUntil;
        private System.Windows.Forms.CheckBox SinceCheck;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox FileFilterCheck;
        private System.Windows.Forms.TextBox FileFilter;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox BranchFilter;
        private System.Windows.Forms.CheckBox CurrentBranchOnlyCheck;
        private System.Windows.Forms.CheckBox BranchFilterCheck;


    }
}