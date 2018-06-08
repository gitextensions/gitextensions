namespace GitUI.UserControls.RevisionGrid
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
            this.SimplifyByDecorationCheck = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_Limit)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Location = new System.Drawing.Point(379, 288);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 25);
            this.Ok.TabIndex = 19;
            this.Ok.Text = "OK";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // _NO_TRANSLATE_Limit
            // 
            this._NO_TRANSLATE_Limit.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._NO_TRANSLATE_Limit.Location = new System.Drawing.Point(213, 168);
            this._NO_TRANSLATE_Limit.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this._NO_TRANSLATE_Limit.Name = "_NO_TRANSLATE_Limit";
            this._NO_TRANSLATE_Limit.Size = new System.Drawing.Size(116, 23);
            this._NO_TRANSLATE_Limit.TabIndex = 18;
            // 
            // Message
            // 
            this.Message.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Message.Location = new System.Drawing.Point(213, 119);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(241, 23);
            this.Message.TabIndex = 14;
            // 
            // Author
            // 
            this.Author.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Author.Location = new System.Drawing.Point(213, 61);
            this.Author.Name = "Author";
            this.Author.Size = new System.Drawing.Size(241, 23);
            this.Author.TabIndex = 9;
            // 
            // Since
            // 
            this.Since.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Since.Location = new System.Drawing.Point(213, 3);
            this.Since.Name = "Since";
            this.Since.Size = new System.Drawing.Size(200, 23);
            this.Since.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Since";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Until";
            // 
            // Until
            // 
            this.Until.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Until.Location = new System.Drawing.Point(213, 32);
            this.Until.Name = "Until";
            this.Until.Size = new System.Drawing.Size(200, 23);
            this.Until.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "Author";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 15);
            this.label4.TabIndex = 11;
            this.label4.Text = "Committer";
            // 
            // Committer
            // 
            this.Committer.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Committer.Location = new System.Drawing.Point(213, 90);
            this.Committer.Name = "Committer";
            this.Committer.Size = new System.Drawing.Size(241, 23);
            this.Committer.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 123);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 15);
            this.label5.TabIndex = 13;
            this.label5.Text = "Message";
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 172);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(34, 15);
            this.label7.TabIndex = 17;
            this.label7.Text = "Limit";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 147);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 15);
            this.label6.TabIndex = 15;
            this.label6.Text = "Ignore case";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.Ok, 2, 11);
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
            this.tableLayoutPanel1.Controls.Add(this.SimplifyByDecorationCheck, 2, 10);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 12;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(457, 316);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // LimitCheck
            // 
            this.LimitCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LimitCheck.AutoSize = true;
            this.LimitCheck.Location = new System.Drawing.Point(193, 172);
            this.LimitCheck.Name = "LimitCheck";
            this.LimitCheck.Size = new System.Drawing.Size(14, 14);
            this.LimitCheck.TabIndex = 27;
            this.LimitCheck.UseVisualStyleBackColor = true;
            this.LimitCheck.CheckedChanged += new System.EventHandler(this.SinceCheckCheckedChanged);
            // 
            // IgnoreCase
            // 
            this.IgnoreCase.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.IgnoreCase.AutoSize = true;
            this.IgnoreCase.Checked = true;
            this.IgnoreCase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.IgnoreCase.Location = new System.Drawing.Point(193, 148);
            this.IgnoreCase.Name = "IgnoreCase";
            this.IgnoreCase.Size = new System.Drawing.Size(14, 14);
            this.IgnoreCase.TabIndex = 26;
            this.IgnoreCase.UseVisualStyleBackColor = true;
            // 
            // MessageCheck
            // 
            this.MessageCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.MessageCheck.AutoSize = true;
            this.MessageCheck.Location = new System.Drawing.Point(193, 123);
            this.MessageCheck.Name = "MessageCheck";
            this.MessageCheck.Size = new System.Drawing.Size(14, 14);
            this.MessageCheck.TabIndex = 25;
            this.MessageCheck.UseVisualStyleBackColor = true;
            this.MessageCheck.CheckedChanged += new System.EventHandler(this.SinceCheckCheckedChanged);
            // 
            // CommitterCheck
            // 
            this.CommitterCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.CommitterCheck.AutoSize = true;
            this.CommitterCheck.Location = new System.Drawing.Point(193, 94);
            this.CommitterCheck.Name = "CommitterCheck";
            this.CommitterCheck.Size = new System.Drawing.Size(14, 14);
            this.CommitterCheck.TabIndex = 24;
            this.CommitterCheck.UseVisualStyleBackColor = true;
            this.CommitterCheck.CheckedChanged += new System.EventHandler(this.SinceCheckCheckedChanged);
            // 
            // AuthorCheck
            // 
            this.AuthorCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.AuthorCheck.AutoSize = true;
            this.AuthorCheck.Location = new System.Drawing.Point(193, 65);
            this.AuthorCheck.Name = "AuthorCheck";
            this.AuthorCheck.Size = new System.Drawing.Size(14, 14);
            this.AuthorCheck.TabIndex = 23;
            this.AuthorCheck.UseVisualStyleBackColor = true;
            this.AuthorCheck.CheckedChanged += new System.EventHandler(this.SinceCheckCheckedChanged);
            // 
            // CheckUntil
            // 
            this.CheckUntil.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.CheckUntil.AutoSize = true;
            this.CheckUntil.Location = new System.Drawing.Point(193, 36);
            this.CheckUntil.Name = "CheckUntil";
            this.CheckUntil.Size = new System.Drawing.Size(14, 14);
            this.CheckUntil.TabIndex = 22;
            this.CheckUntil.UseVisualStyleBackColor = true;
            this.CheckUntil.CheckedChanged += new System.EventHandler(this.SinceCheckCheckedChanged);
            // 
            // SinceCheck
            // 
            this.SinceCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.SinceCheck.AutoSize = true;
            this.SinceCheck.Location = new System.Drawing.Point(193, 7);
            this.SinceCheck.Name = "SinceCheck";
            this.SinceCheck.Size = new System.Drawing.Size(14, 14);
            this.SinceCheck.TabIndex = 21;
            this.SinceCheck.UseVisualStyleBackColor = true;
            this.SinceCheck.CheckedChanged += new System.EventHandler(this.SinceCheckCheckedChanged);
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 201);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(52, 15);
            this.label8.TabIndex = 28;
            this.label8.Text = "File filter";
            // 
            // FileFilterCheck
            // 
            this.FileFilterCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.FileFilterCheck.AutoSize = true;
            this.FileFilterCheck.Location = new System.Drawing.Point(193, 201);
            this.FileFilterCheck.Name = "FileFilterCheck";
            this.FileFilterCheck.Size = new System.Drawing.Size(14, 14);
            this.FileFilterCheck.TabIndex = 29;
            this.FileFilterCheck.UseVisualStyleBackColor = true;
            this.FileFilterCheck.CheckedChanged += new System.EventHandler(this.SinceCheckCheckedChanged);
            // 
            // FileFilter
            // 
            this.FileFilter.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.FileFilter.Location = new System.Drawing.Point(213, 197);
            this.FileFilter.Name = "FileFilter";
            this.FileFilter.Size = new System.Drawing.Size(241, 23);
            this.FileFilter.TabIndex = 30;
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 230);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(55, 15);
            this.label9.TabIndex = 31;
            this.label9.Text = "Branches";
            // 
            // BranchFilter
            // 
            this.BranchFilter.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.BranchFilter.Location = new System.Drawing.Point(213, 226);
            this.BranchFilter.Name = "BranchFilter";
            this.BranchFilter.Size = new System.Drawing.Size(241, 23);
            this.BranchFilter.TabIndex = 32;
            // 
            // CurrentBranchOnlyCheck
            // 
            this.CurrentBranchOnlyCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.CurrentBranchOnlyCheck.AutoSize = true;
            this.CurrentBranchOnlyCheck.Location = new System.Drawing.Point(213, 255);
            this.CurrentBranchOnlyCheck.Name = "CurrentBranchOnlyCheck";
            this.CurrentBranchOnlyCheck.Size = new System.Drawing.Size(162, 19);
            this.CurrentBranchOnlyCheck.TabIndex = 33;
            this.CurrentBranchOnlyCheck.Text = "Show current branch only";
            this.CurrentBranchOnlyCheck.UseVisualStyleBackColor = true;
            this.CurrentBranchOnlyCheck.CheckedChanged += new System.EventHandler(this.OnShowCurrentBranchOnlyCheckedChanged);
            // 
            // BranchFilterCheck
            // 
            this.BranchFilterCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.BranchFilterCheck.AutoSize = true;
            this.BranchFilterCheck.Location = new System.Drawing.Point(193, 230);
            this.BranchFilterCheck.Name = "BranchFilterCheck";
            this.BranchFilterCheck.Size = new System.Drawing.Size(14, 14);
            this.BranchFilterCheck.TabIndex = 34;
            this.BranchFilterCheck.UseVisualStyleBackColor = true;
            this.BranchFilterCheck.CheckedChanged += new System.EventHandler(this.OnBranchFilterCheckedChanged);
            // 
            // SimplifyByDecorationCheck
            // 
            this.SimplifyByDecorationCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.SimplifyByDecorationCheck.AutoSize = true;
            this.SimplifyByDecorationCheck.Location = new System.Drawing.Point(213, 262);
            this.SimplifyByDecorationCheck.Name = "SimplifyByDecorationCheck";
            this.SimplifyByDecorationCheck.Size = new System.Drawing.Size(131, 17);
            this.SimplifyByDecorationCheck.TabIndex = 35;
            this.SimplifyByDecorationCheck.Text = "Simplify by decoration";
            this.SimplifyByDecorationCheck.UseVisualStyleBackColor = true;
            this.SimplifyByDecorationCheck.CheckedChanged += new System.EventHandler(this.OnSimplifyByDecorationCheckedChanged);
            // 
            // FormRevisionFilter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(457, 344);
            this.Controls.Add(this.tableLayoutPanel1);
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
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox FileFilterCheck;
        private System.Windows.Forms.TextBox FileFilter;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox BranchFilter;
        private System.Windows.Forms.CheckBox CurrentBranchOnlyCheck;
        private System.Windows.Forms.CheckBox BranchFilterCheck;
        private System.Windows.Forms.CheckBox SimplifyByDecorationCheck;
    }
}