namespace GitUI
{
    partial class FormRecentReposSettings
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
            this._NO_TRANSLATE_maxRecentRepositories = new System.Windows.Forms.NumericUpDown();
            this.maxRecentRepositories = new System.Windows.Forms.Label();
            this.sortLessRecentRepos = new System.Windows.Forms.CheckBox();
            this.sortMostRecentRepos = new System.Windows.Forms.CheckBox();
            this.comboPanel = new System.Windows.Forms.Panel();
            this.LessRecentLB = new System.Windows.Forms.ListBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.MostRecentLB = new System.Windows.Forms.ListBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.MostRecentLabel = new System.Windows.Forms.Label();
            this.Abort = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.Ok = new System.Windows.Forms.Button();
            this.shorteningGB = new System.Windows.Forms.GroupBox();
            this.dontShortenRB = new System.Windows.Forms.RadioButton();
            this.middleDotRB = new System.Windows.Forms.RadioButton();
            this.mostSigDirRB = new System.Windows.Forms.RadioButton();
            this.comboMinWidthEdit = new System.Windows.Forms.NumericUpDown();
            this.comboMinWidthLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_maxRecentRepositories)).BeginInit();
            this.comboPanel.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.shorteningGB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboMinWidthEdit)).BeginInit();
            this.SuspendLayout();
            // 
            // _NO_TRANSLATE_maxRecentRepositories
            // 
            this._NO_TRANSLATE_maxRecentRepositories.Location = new System.Drawing.Point(251, 12);
            this._NO_TRANSLATE_maxRecentRepositories.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this._NO_TRANSLATE_maxRecentRepositories.Name = "_NO_TRANSLATE_maxRecentRepositories";
            this._NO_TRANSLATE_maxRecentRepositories.Size = new System.Drawing.Size(61, 21);
            this._NO_TRANSLATE_maxRecentRepositories.TabIndex = 58;
            this._NO_TRANSLATE_maxRecentRepositories.ValueChanged += new System.EventHandler(this.sortMostRecentRepos_CheckedChanged);
            // 
            // maxRecentRepositories
            // 
            this.maxRecentRepositories.AutoSize = true;
            this.maxRecentRepositories.Location = new System.Drawing.Point(11, 14);
            this.maxRecentRepositories.Name = "maxRecentRepositories";
            this.maxRecentRepositories.Size = new System.Drawing.Size(222, 13);
            this.maxRecentRepositories.TabIndex = 57;
            this.maxRecentRepositories.Text = "Maximum number of most recent repositories";
            // 
            // sortLessRecentRepos
            // 
            this.sortLessRecentRepos.AutoSize = true;
            this.sortLessRecentRepos.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.sortLessRecentRepos.Location = new System.Drawing.Point(12, 58);
            this.sortLessRecentRepos.Name = "sortLessRecentRepos";
            this.sortLessRecentRepos.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.sortLessRecentRepos.Size = new System.Drawing.Size(160, 17);
            this.sortLessRecentRepos.TabIndex = 56;
            this.sortLessRecentRepos.Text = "Sort less recent repositories";
            this.sortLessRecentRepos.UseVisualStyleBackColor = true;
            this.sortLessRecentRepos.CheckedChanged += new System.EventHandler(this.sortMostRecentRepos_CheckedChanged);
            // 
            // sortMostRecentRepos
            // 
            this.sortMostRecentRepos.AutoSize = true;
            this.sortMostRecentRepos.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.sortMostRecentRepos.Location = new System.Drawing.Point(12, 35);
            this.sortMostRecentRepos.Name = "sortMostRecentRepos";
            this.sortMostRecentRepos.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.sortMostRecentRepos.Size = new System.Drawing.Size(165, 17);
            this.sortMostRecentRepos.TabIndex = 55;
            this.sortMostRecentRepos.Text = "Sort most recent repositories";
            this.sortMostRecentRepos.UseVisualStyleBackColor = true;
            this.sortMostRecentRepos.CheckedChanged += new System.EventHandler(this.sortMostRecentRepos_CheckedChanged);
            // 
            // comboPanel
            // 
            this.comboPanel.Controls.Add(this.LessRecentLB);
            this.comboPanel.Controls.Add(this.panel3);
            this.comboPanel.Controls.Add(this.MostRecentLB);
            this.comboPanel.Controls.Add(this.panel2);
            this.comboPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.comboPanel.Location = new System.Drawing.Point(398, 0);
            this.comboPanel.Name = "comboPanel";
            this.comboPanel.Size = new System.Drawing.Size(233, 422);
            this.comboPanel.TabIndex = 60;
            // 
            // LessRecentLB
            // 
            this.LessRecentLB.DisplayMember = "Caption";
            this.LessRecentLB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LessRecentLB.FormattingEnabled = true;
            this.LessRecentLB.Location = new System.Drawing.Point(0, 220);
            this.LessRecentLB.Name = "LessRecentLB";
            this.LessRecentLB.Size = new System.Drawing.Size(233, 202);
            this.LessRecentLB.TabIndex = 4;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 195);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(233, 25);
            this.panel3.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Less recent repositories";
            // 
            // MostRecentLB
            // 
            this.MostRecentLB.DisplayMember = "Caption";
            this.MostRecentLB.Dock = System.Windows.Forms.DockStyle.Top;
            this.MostRecentLB.FormattingEnabled = true;
            this.MostRecentLB.Location = new System.Drawing.Point(0, 22);
            this.MostRecentLB.Name = "MostRecentLB";
            this.MostRecentLB.Size = new System.Drawing.Size(233, 173);
            this.MostRecentLB.TabIndex = 2;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.MostRecentLabel);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(233, 22);
            this.panel2.TabIndex = 1;
            // 
            // MostRecentLabel
            // 
            this.MostRecentLabel.AutoSize = true;
            this.MostRecentLabel.Location = new System.Drawing.Point(3, 6);
            this.MostRecentLabel.Name = "MostRecentLabel";
            this.MostRecentLabel.Size = new System.Drawing.Size(123, 13);
            this.MostRecentLabel.TabIndex = 0;
            this.MostRecentLabel.Text = "Most recent repositories";
            // 
            // Abort
            // 
            this.Abort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.Abort.Location = new System.Drawing.Point(318, 5);
            this.Abort.Name = "Abort";
            this.Abort.Size = new System.Drawing.Size(75, 23);
            this.Abort.TabIndex = 3;
            this.Abort.Text = "Cancel";
            this.Abort.UseCompatibleTextRendering = true;
            this.Abort.UseVisualStyleBackColor = true;
            this.Abort.Click += new System.EventHandler(this.Abort_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.Abort, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.Ok, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 422);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(631, 34);
            this.tableLayoutPanel2.TabIndex = 59;
            // 
            // Ok
            // 
            this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.Ok.Location = new System.Drawing.Point(237, 5);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.TabIndex = 5;
            this.Ok.Text = "OK";
            this.Ok.UseCompatibleTextRendering = true;
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // shorteningGB
            // 
            this.shorteningGB.Controls.Add(this.dontShortenRB);
            this.shorteningGB.Controls.Add(this.middleDotRB);
            this.shorteningGB.Controls.Add(this.mostSigDirRB);
            this.shorteningGB.Location = new System.Drawing.Point(12, 81);
            this.shorteningGB.Name = "shorteningGB";
            this.shorteningGB.Size = new System.Drawing.Size(300, 91);
            this.shorteningGB.TabIndex = 61;
            this.shorteningGB.TabStop = false;
            this.shorteningGB.Text = "Shortening strategy";
            // 
            // dontShortenRB
            // 
            this.dontShortenRB.AutoSize = true;
            this.dontShortenRB.Location = new System.Drawing.Point(6, 22);
            this.dontShortenRB.Name = "dontShortenRB";
            this.dontShortenRB.Size = new System.Drawing.Size(103, 17);
            this.dontShortenRB.TabIndex = 2;
            this.dontShortenRB.TabStop = true;
            this.dontShortenRB.Text = "Do not shorten  ";
            this.dontShortenRB.UseVisualStyleBackColor = true;
            this.dontShortenRB.CheckedChanged += new System.EventHandler(this.sortMostRecentRepos_CheckedChanged);
            // 
            // middleDotRB
            // 
            this.middleDotRB.AutoSize = true;
            this.middleDotRB.Location = new System.Drawing.Point(6, 68);
            this.middleDotRB.Name = "middleDotRB";
            this.middleDotRB.Size = new System.Drawing.Size(169, 17);
            this.middleDotRB.TabIndex = 1;
            this.middleDotRB.TabStop = true;
            this.middleDotRB.Text = "Replace middle part with dots ";
            this.middleDotRB.UseVisualStyleBackColor = true;
            this.middleDotRB.CheckedChanged += new System.EventHandler(this.sortMostRecentRepos_CheckedChanged);
            // 
            // mostSigDirRB
            // 
            this.mostSigDirRB.AutoSize = true;
            this.mostSigDirRB.Location = new System.Drawing.Point(6, 45);
            this.mostSigDirRB.Name = "mostSigDirRB";
            this.mostSigDirRB.Size = new System.Drawing.Size(169, 17);
            this.mostSigDirRB.TabIndex = 0;
            this.mostSigDirRB.TabStop = true;
            this.mostSigDirRB.Text = "The most significant directory ";
            this.mostSigDirRB.UseVisualStyleBackColor = true;
            this.mostSigDirRB.CheckedChanged += new System.EventHandler(this.sortMostRecentRepos_CheckedChanged);
            // 
            // comboMinWidthEdit
            // 
            this.comboMinWidthEdit.Location = new System.Drawing.Point(255, 180);
            this.comboMinWidthEdit.Maximum = new decimal(new int[] {
            800,
            0,
            0,
            0});
            this.comboMinWidthEdit.Name = "comboMinWidthEdit";
            this.comboMinWidthEdit.Size = new System.Drawing.Size(61, 21);
            this.comboMinWidthEdit.TabIndex = 63;
            this.comboMinWidthEdit.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // comboMinWidthLabel
            // 
            this.comboMinWidthLabel.AutoSize = true;
            this.comboMinWidthLabel.Location = new System.Drawing.Point(15, 182);
            this.comboMinWidthLabel.Name = "comboMinWidthLabel";
            this.comboMinWidthLabel.Size = new System.Drawing.Size(202, 13);
            this.comboMinWidthLabel.TabIndex = 62;
            this.comboMinWidthLabel.Text = "Combobox minimum width (0 = Autosize)";
            // 
            // FormRecentReposSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(631, 456);
            this.Controls.Add(this.comboMinWidthEdit);
            this.Controls.Add(this.comboMinWidthLabel);
            this.Controls.Add(this.shorteningGB);
            this.Controls.Add(this.comboPanel);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this._NO_TRANSLATE_maxRecentRepositories);
            this.Controls.Add(this.maxRecentRepositories);
            this.Controls.Add(this.sortLessRecentRepos);
            this.Controls.Add(this.sortMostRecentRepos);
            this.Name = "FormRecentReposSettings";
            this.Text = "Recent repositories settings";
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_maxRecentRepositories)).EndInit();
            this.comboPanel.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.shorteningGB.ResumeLayout(false);
            this.shorteningGB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboMinWidthEdit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_maxRecentRepositories;
        private System.Windows.Forms.Label maxRecentRepositories;
        private System.Windows.Forms.CheckBox sortLessRecentRepos;
        private System.Windows.Forms.CheckBox sortMostRecentRepos;
        private System.Windows.Forms.Panel comboPanel;
        private System.Windows.Forms.ListBox LessRecentLB;
        private System.Windows.Forms.ListBox MostRecentLB;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label MostRecentLabel;
        protected System.Windows.Forms.Button Abort;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        protected System.Windows.Forms.Button Ok;
        private System.Windows.Forms.GroupBox shorteningGB;
        private System.Windows.Forms.RadioButton mostSigDirRB;
        private System.Windows.Forms.RadioButton middleDotRB;
        private System.Windows.Forms.RadioButton dontShortenRB;
        private System.Windows.Forms.NumericUpDown comboMinWidthEdit;
        private System.Windows.Forms.Label comboMinWidthLabel;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label1;
    }
}