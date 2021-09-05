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
            this.Ok = new System.Windows.Forms.Button();
            this._NO_TRANSLATE_CommitsLimit = new System.Windows.Forms.NumericUpDown();
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
            this.SinceCheck = new System.Windows.Forms.CheckBox();
            this.CheckUntil = new System.Windows.Forms.CheckBox();
            this.AuthorCheck = new System.Windows.Forms.CheckBox();
            this.CommitterCheck = new System.Windows.Forms.CheckBox();
            this.MessageCheck = new System.Windows.Forms.CheckBox();
            this.IgnoreCase = new System.Windows.Forms.CheckBox();
            this.CommitsLimitCheck = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.PathFilterCheck = new System.Windows.Forms.CheckBox();
            this.PathFilter = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.BranchFilterCheck = new System.Windows.Forms.CheckBox();
            this.BranchFilter = new System.Windows.Forms.TextBox();
            this.CurrentBranchOnlyCheck = new System.Windows.Forms.CheckBox();
            this.SimplifyByDecorationCheck = new System.Windows.Forms.CheckBox();
            this.MainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_CommitsLimit)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.Ok);
            this.MainPanel.Controls.Add(this.tableLayoutPanel1);
            this.MainPanel.Size = new System.Drawing.Size(408, 343);
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(3, 3);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.Text = "OK";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.OkClick);
            // 
            // _NO_TRANSLATE_CommitsLimit
            // 
            this._NO_TRANSLATE_CommitsLimit.Dock = System.Windows.Forms.DockStyle.Left;
            this._NO_TRANSLATE_CommitsLimit.Increment = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this._NO_TRANSLATE_CommitsLimit.Location = new System.Drawing.Point(96, 168);
            this._NO_TRANSLATE_CommitsLimit.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this._NO_TRANSLATE_CommitsLimit.Name = "_NO_TRANSLATE_CommitsLimit";
            this._NO_TRANSLATE_CommitsLimit.Size = new System.Drawing.Size(116, 23);
            // 
            // Message
            // 
            this.Message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Message.Location = new System.Drawing.Point(96, 119);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(285, 23);
            // 
            // Author
            // 
            this.Author.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Author.Location = new System.Drawing.Point(96, 61);
            this.Author.Name = "Author";
            this.Author.Size = new System.Drawing.Size(285, 23);
            // 
            // Since
            // 
            this.Since.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Since.Location = new System.Drawing.Point(96, 3);
            this.Since.Name = "Since";
            this.Since.Size = new System.Drawing.Size(200, 23);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 29);
            this.label1.Text = "Since";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 29);
            this.label2.Text = "Until";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Until
            // 
            this.Until.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Until.Location = new System.Drawing.Point(96, 32);
            this.Until.Name = "Until";
            this.Until.Size = new System.Drawing.Size(200, 23);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 29);
            this.label3.Text = "Author";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 87);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 29);
            this.label4.Text = "Committer";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Committer
            // 
            this.Committer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Committer.Location = new System.Drawing.Point(96, 90);
            this.Committer.Name = "Committer";
            this.Committer.Size = new System.Drawing.Size(285, 23);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(3, 116);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 29);
            this.label5.Text = "Message";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Location = new System.Drawing.Point(3, 165);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(67, 29);
            this.label7.Text = "Limit";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(3, 145);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 20);
            this.label6.Text = "Ignore case";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.SinceCheck, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.Since, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.CheckUntil, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.Until, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.AuthorCheck, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.Author, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.CommitterCheck, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.Committer, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.MessageCheck, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.Message, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.IgnoreCase, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.CommitsLimitCheck, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_CommitsLimit, 2, 6);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.PathFilterCheck, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.PathFilter, 2, 7);
            this.tableLayoutPanel1.Controls.Add(this.label9, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.BranchFilterCheck, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.BranchFilter, 2, 8);
            this.tableLayoutPanel1.Controls.Add(this.CurrentBranchOnlyCheck, 2, 9);
            this.tableLayoutPanel1.Controls.Add(this.SimplifyByDecorationCheck, 2, 10);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(384, 319);
            // 
            // SinceCheck
            // 
            this.SinceCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.SinceCheck.AutoSize = true;
            this.SinceCheck.Location = new System.Drawing.Point(76, 7);
            this.SinceCheck.Name = "SinceCheck";
            this.SinceCheck.Size = new System.Drawing.Size(14, 14);
            this.SinceCheck.UseVisualStyleBackColor = true;
            this.SinceCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // CheckUntil
            // 
            this.CheckUntil.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.CheckUntil.AutoSize = true;
            this.CheckUntil.Location = new System.Drawing.Point(76, 36);
            this.CheckUntil.Name = "CheckUntil";
            this.CheckUntil.Size = new System.Drawing.Size(14, 14);
            this.CheckUntil.UseVisualStyleBackColor = true;
            this.CheckUntil.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // AuthorCheck
            // 
            this.AuthorCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.AuthorCheck.AutoSize = true;
            this.AuthorCheck.Location = new System.Drawing.Point(76, 65);
            this.AuthorCheck.Name = "AuthorCheck";
            this.AuthorCheck.Size = new System.Drawing.Size(14, 14);
            this.AuthorCheck.UseVisualStyleBackColor = true;
            this.AuthorCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // CommitterCheck
            // 
            this.CommitterCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.CommitterCheck.AutoSize = true;
            this.CommitterCheck.Location = new System.Drawing.Point(76, 94);
            this.CommitterCheck.Name = "CommitterCheck";
            this.CommitterCheck.Size = new System.Drawing.Size(14, 14);
            this.CommitterCheck.UseVisualStyleBackColor = true;
            this.CommitterCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // MessageCheck
            // 
            this.MessageCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.MessageCheck.AutoSize = true;
            this.MessageCheck.Location = new System.Drawing.Point(76, 123);
            this.MessageCheck.Name = "MessageCheck";
            this.MessageCheck.Size = new System.Drawing.Size(14, 14);
            this.MessageCheck.UseVisualStyleBackColor = true;
            this.MessageCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // IgnoreCase
            // 
            this.IgnoreCase.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.IgnoreCase.AutoSize = true;
            this.IgnoreCase.Checked = true;
            this.IgnoreCase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.IgnoreCase.Location = new System.Drawing.Point(76, 148);
            this.IgnoreCase.Name = "IgnoreCase";
            this.IgnoreCase.Size = new System.Drawing.Size(14, 14);
            this.IgnoreCase.UseVisualStyleBackColor = true;
            // 
            // CommitsLimitCheck
            // 
            this.CommitsLimitCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.CommitsLimitCheck.AutoSize = true;
            this.CommitsLimitCheck.Location = new System.Drawing.Point(76, 172);
            this.CommitsLimitCheck.Name = "CommitsLimitCheck";
            this.CommitsLimitCheck.Size = new System.Drawing.Size(14, 14);
            this.CommitsLimitCheck.UseVisualStyleBackColor = true;
            this.CommitsLimitCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Location = new System.Drawing.Point(3, 194);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(67, 29);
            this.label8.Text = "Path filter";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PathFilterCheck
            // 
            this.PathFilterCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.PathFilterCheck.AutoSize = true;
            this.PathFilterCheck.Location = new System.Drawing.Point(76, 201);
            this.PathFilterCheck.Name = "PathFilterCheck";
            this.PathFilterCheck.Size = new System.Drawing.Size(14, 14);
            this.PathFilterCheck.UseVisualStyleBackColor = true;
            this.PathFilterCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // PathFilter
            // 
            this.PathFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PathFilter.Location = new System.Drawing.Point(96, 197);
            this.PathFilter.Name = "PathFilter";
            this.PathFilter.Size = new System.Drawing.Size(285, 23);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Location = new System.Drawing.Point(3, 223);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(67, 29);
            this.label9.Text = "Branches";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BranchFilterCheck
            // 
            this.BranchFilterCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.BranchFilterCheck.AutoSize = true;
            this.BranchFilterCheck.Location = new System.Drawing.Point(76, 230);
            this.BranchFilterCheck.Name = "BranchFilterCheck";
            this.BranchFilterCheck.Size = new System.Drawing.Size(14, 14);
            this.BranchFilterCheck.UseVisualStyleBackColor = true;
            this.BranchFilterCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // BranchFilter
            // 
            this.BranchFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BranchFilter.Location = new System.Drawing.Point(96, 226);
            this.BranchFilter.Name = "BranchFilter";
            this.BranchFilter.Size = new System.Drawing.Size(285, 23);
            // 
            // CurrentBranchOnlyCheck
            // 
            this.CurrentBranchOnlyCheck.AutoSize = true;
            this.CurrentBranchOnlyCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurrentBranchOnlyCheck.Location = new System.Drawing.Point(96, 255);
            this.CurrentBranchOnlyCheck.Name = "CurrentBranchOnlyCheck";
            this.CurrentBranchOnlyCheck.Size = new System.Drawing.Size(285, 19);
            this.CurrentBranchOnlyCheck.Text = "Show current branch only";
            this.CurrentBranchOnlyCheck.UseVisualStyleBackColor = true;
            this.CurrentBranchOnlyCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // SimplifyByDecorationCheck
            // 
            this.SimplifyByDecorationCheck.AutoSize = true;
            this.SimplifyByDecorationCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SimplifyByDecorationCheck.Location = new System.Drawing.Point(96, 280);
            this.SimplifyByDecorationCheck.Name = "SimplifyByDecorationCheck";
            this.SimplifyByDecorationCheck.Size = new System.Drawing.Size(285, 19);
            this.SimplifyByDecorationCheck.Text = "Simplify by decoration";
            this.SimplifyByDecorationCheck.UseVisualStyleBackColor = true;
            this.SimplifyByDecorationCheck.CheckedChanged += new System.EventHandler(this.option_CheckedChanged);
            // 
            // FormRevisionFilter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(408, 375);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormRevisionFilter";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Filter";
            this.MainPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_CommitsLimit)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_CommitsLimit;
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
        private System.Windows.Forms.CheckBox CommitsLimitCheck;
        private System.Windows.Forms.CheckBox IgnoreCase;
        private System.Windows.Forms.CheckBox MessageCheck;
        private System.Windows.Forms.CheckBox CommitterCheck;
        private System.Windows.Forms.CheckBox AuthorCheck;
        private System.Windows.Forms.CheckBox CheckUntil;
        private System.Windows.Forms.CheckBox SinceCheck;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox PathFilterCheck;
        private System.Windows.Forms.TextBox PathFilter;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox BranchFilter;
        private System.Windows.Forms.CheckBox CurrentBranchOnlyCheck;
        private System.Windows.Forms.CheckBox BranchFilterCheck;
        private System.Windows.Forms.CheckBox SimplifyByDecorationCheck;
    }
}
