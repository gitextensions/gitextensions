namespace GitUI.CommandsDialogs
{
    partial class FormArchive
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
            this.buttonArchiveRevision = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this._NO_TRANSLATE_radioButtonFormatZip = new System.Windows.Forms.RadioButton();
            this._NO_TRANSLATE_radioButtonFormatTar = new System.Windows.Forms.RadioButton();
            this.commitSummaryUserControl1 = new GitUI.UserControls.CommitSummaryUserControl();
            this.btnChooseRevision = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBoxPaths = new System.Windows.Forms.TextBox();
            this.checkBoxPathFilter = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.commitSummaryUserControl2 = new GitUI.UserControls.CommitSummaryUserControl();
            this.checkboxRevisionFilter = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.btnDiffChooseRevision = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(153, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Selected revision to archive:";
            // 
            // buttonArchiveRevision
            // 
            this.buttonArchiveRevision.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonArchiveRevision.AutoSize = true;
            this.buttonArchiveRevision.Image = global::GitUI.Properties.Resources.IconSaveAs;
            this.buttonArchiveRevision.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonArchiveRevision.Location = new System.Drawing.Point(480, 554);
            this.buttonArchiveRevision.Name = "buttonArchiveRevision";
            this.buttonArchiveRevision.Size = new System.Drawing.Size(111, 25);
            this.buttonArchiveRevision.TabIndex = 3;
            this.buttonArchiveRevision.Text = "Save as...";
            this.buttonArchiveRevision.UseVisualStyleBackColor = true;
            this.buttonArchiveRevision.Click += new System.EventHandler(this.Save_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Location = new System.Drawing.Point(365, 524);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(109, 55);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Archive format";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this._NO_TRANSLATE_radioButtonFormatZip, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this._NO_TRANSLATE_radioButtonFormatTar, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 19);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(103, 33);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // _NO_TRANSLATE_radioButtonFormatZip
            // 
            this._NO_TRANSLATE_radioButtonFormatZip.AutoSize = true;
            this._NO_TRANSLATE_radioButtonFormatZip.Checked = true;
            this._NO_TRANSLATE_radioButtonFormatZip.Location = new System.Drawing.Point(3, 3);
            this._NO_TRANSLATE_radioButtonFormatZip.Name = "_NO_TRANSLATE_radioButtonFormatZip";
            this._NO_TRANSLATE_radioButtonFormatZip.Size = new System.Drawing.Size(40, 19);
            this._NO_TRANSLATE_radioButtonFormatZip.TabIndex = 0;
            this._NO_TRANSLATE_radioButtonFormatZip.TabStop = true;
            this._NO_TRANSLATE_radioButtonFormatZip.Text = "zip";
            this._NO_TRANSLATE_radioButtonFormatZip.UseVisualStyleBackColor = true;
            // 
            // _NO_TRANSLATE_radioButtonFormatTar
            // 
            this._NO_TRANSLATE_radioButtonFormatTar.AutoSize = true;
            this._NO_TRANSLATE_radioButtonFormatTar.Location = new System.Drawing.Point(54, 3);
            this._NO_TRANSLATE_radioButtonFormatTar.Name = "_NO_TRANSLATE_radioButtonFormatTar";
            this._NO_TRANSLATE_radioButtonFormatTar.Size = new System.Drawing.Size(39, 19);
            this._NO_TRANSLATE_radioButtonFormatTar.TabIndex = 1;
            this._NO_TRANSLATE_radioButtonFormatTar.Text = "tar";
            this._NO_TRANSLATE_radioButtonFormatTar.UseVisualStyleBackColor = true;
            // 
            // commitSummaryUserControl1
            // 
            this.commitSummaryUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commitSummaryUserControl1.AutoSize = true;
            this.commitSummaryUserControl1.Location = new System.Drawing.Point(16, 18);
            this.commitSummaryUserControl1.Margin = new System.Windows.Forms.Padding(16, 3, 3, 3);
            this.commitSummaryUserControl1.MinimumSize = new System.Drawing.Size(440, 160);
            this.commitSummaryUserControl1.Name = "commitSummaryUserControl1";
            this.commitSummaryUserControl1.Revision = null;
            this.commitSummaryUserControl1.Size = new System.Drawing.Size(458, 160);
            this.commitSummaryUserControl1.TabIndex = 5;
            // 
            // btnChooseRevision
            // 
            this.btnChooseRevision.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnChooseRevision.Image = global::GitUI.Properties.Resources.IconSelectRevision;
            this.btnChooseRevision.Location = new System.Drawing.Point(69, 33);
            this.btnChooseRevision.Name = "btnChooseRevision";
            this.btnChooseRevision.Size = new System.Drawing.Size(25, 24);
            this.btnChooseRevision.TabIndex = 31;
            this.btnChooseRevision.UseVisualStyleBackColor = true;
            this.btnChooseRevision.Click += new System.EventHandler(this.btnChooseRevision_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 30);
            this.label2.TabIndex = 32;
            this.label2.Text = "Choose another\r\nrevision:";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.Controls.Add(this.commitSummaryUserControl1, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.buttonArchiveRevision, 1, 4);
            this.tableLayoutPanel4.Controls.Add(this.groupBox1, 0, 4);
            this.tableLayoutPanel4.Controls.Add(this.flowLayoutPanel1, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.groupBox2, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.groupBox3, 0, 3);
            this.tableLayoutPanel4.Controls.Add(this.flowLayoutPanel2, 1, 3);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 5;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.Size = new System.Drawing.Size(594, 582);
            this.tableLayoutPanel4.TabIndex = 6;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.btnChooseRevision);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(479, 17);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(113, 162);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.textBoxPaths);
            this.groupBox2.Controls.Add(this.checkBoxPathFilter);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 184);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(471, 115);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Filter files by path";
            // 
            // textBoxPaths
            // 
            this.textBoxPaths.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPaths.Location = new System.Drawing.Point(21, 47);
            this.textBoxPaths.Multiline = true;
            this.textBoxPaths.Name = "textBoxPaths";
            this.textBoxPaths.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxPaths.Size = new System.Drawing.Size(436, 51);
            this.textBoxPaths.TabIndex = 1;
            // 
            // checkBoxPathFilter
            // 
            this.checkBoxPathFilter.AutoSize = true;
            this.checkBoxPathFilter.Location = new System.Drawing.Point(6, 22);
            this.checkBoxPathFilter.Name = "checkBoxPathFilter";
            this.checkBoxPathFilter.Size = new System.Drawing.Size(328, 19);
            this.checkBoxPathFilter.TabIndex = 0;
            this.checkBoxPathFilter.Text = "Archive specific path(s) only (separate paths by new line):";
            this.checkBoxPathFilter.UseVisualStyleBackColor = true;
            this.checkBoxPathFilter.CheckedChanged += new System.EventHandler(this.checkBoxPathFilter_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.AutoSize = true;
            this.groupBox3.Controls.Add(this.commitSummaryUserControl2);
            this.groupBox3.Controls.Add(this.checkboxRevisionFilter);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(3, 305);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(471, 213);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Filter files by revision";
            // 
            // commitSummaryUserControl2
            // 
            this.commitSummaryUserControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commitSummaryUserControl2.AutoSize = true;
            this.commitSummaryUserControl2.Location = new System.Drawing.Point(7, 34);
            this.commitSummaryUserControl2.Margin = new System.Windows.Forms.Padding(0);
            this.commitSummaryUserControl2.MinimumSize = new System.Drawing.Size(440, 160);
            this.commitSummaryUserControl2.Name = "commitSummaryUserControl2";
            this.commitSummaryUserControl2.Revision = null;
            this.commitSummaryUserControl2.Size = new System.Drawing.Size(458, 160);
            this.commitSummaryUserControl2.TabIndex = 1;
            // 
            // checkboxRevisionFilter
            // 
            this.checkboxRevisionFilter.AutoSize = true;
            this.checkboxRevisionFilter.Location = new System.Drawing.Point(6, 19);
            this.checkboxRevisionFilter.Name = "checkboxRevisionFilter";
            this.checkboxRevisionFilter.Size = new System.Drawing.Size(228, 19);
            this.checkboxRevisionFilter.TabIndex = 0;
            this.checkboxRevisionFilter.Text = "Archive differences with other revision";
            this.checkboxRevisionFilter.UseVisualStyleBackColor = true;
            this.checkboxRevisionFilter.CheckedChanged += new System.EventHandler(this.checkboxRevisionFilter_CheckedChanged);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.label3);
            this.flowLayoutPanel2.Controls.Add(this.btnDiffChooseRevision);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(479, 304);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(113, 215);
            this.flowLayoutPanel2.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "Choose revision:";
            // 
            // btnDiffChooseRevision
            // 
            this.btnDiffChooseRevision.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnDiffChooseRevision.Enabled = false;
            this.btnDiffChooseRevision.Image = global::GitUI.Properties.Resources.IconSelectRevision;
            this.btnDiffChooseRevision.Location = new System.Drawing.Point(72, 18);
            this.btnDiffChooseRevision.Name = "btnDiffChooseRevision";
            this.btnDiffChooseRevision.Size = new System.Drawing.Size(25, 24);
            this.btnDiffChooseRevision.TabIndex = 1;
            this.btnDiffChooseRevision.UseVisualStyleBackColor = true;
            this.btnDiffChooseRevision.Click += new System.EventHandler(this.btnDiffChooseRevision_Click);
            // 
            // FormArchive
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(594, 582);
            this.Controls.Add(this.tableLayoutPanel4);
            this.MinimumSize = new System.Drawing.Size(610, 620);
            this.Name = "FormArchive";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Archive";
            this.Load += new System.EventHandler(this.FormArchive_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonArchiveRevision;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.RadioButton _NO_TRANSLATE_radioButtonFormatZip;
        private System.Windows.Forms.RadioButton _NO_TRANSLATE_radioButtonFormatTar;
        private GitUI.UserControls.CommitSummaryUserControl commitSummaryUserControl1;
        private System.Windows.Forms.Button btnChooseRevision;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBoxPaths;
        private System.Windows.Forms.CheckBox checkBoxPathFilter;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox checkboxRevisionFilter;
        private UserControls.CommitSummaryUserControl commitSummaryUserControl2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnDiffChooseRevision;
    }
}