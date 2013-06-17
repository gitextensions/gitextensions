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
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.commitSummaryUserControl1 = new GitUI.UserControls.CommitSummaryUserControl();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonArchiveRevision = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this._NO_TRANSLATE_radioButtonFormatZip = new System.Windows.Forms.RadioButton();
            this._NO_TRANSLATE_radioButtonFormatTar = new System.Windows.Forms.RadioButton();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.btnChooseRevision = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblChooseDiffRevision = new System.Windows.Forms.Label();
            this.commitSummaryUserControl2 = new GitUI.UserControls.CommitSummaryUserControl();
            this.btnDiffChooseRevision = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.checkboxRevisionFilter = new System.Windows.Forms.CheckBox();
            this.textBoxPaths = new System.Windows.Forms.TextBox();
            this.checkBoxPathFilter = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.Controls.Add(this.commitSummaryUserControl1, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.buttonArchiveRevision, 1, 3);
            this.tableLayoutPanel4.Controls.Add(this.groupBox1, 0, 3);
            this.tableLayoutPanel4.Controls.Add(this.flowLayoutPanel1, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.groupBox2, 0, 2);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 4;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(594, 652);
            this.tableLayoutPanel4.TabIndex = 6;
            // 
            // commitSummaryUserControl1
            // 
            this.commitSummaryUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commitSummaryUserControl1.AutoSize = true;
            this.commitSummaryUserControl1.Location = new System.Drawing.Point(16, 33);
            this.commitSummaryUserControl1.Margin = new System.Windows.Forms.Padding(16, 3, 3, 3);
            this.commitSummaryUserControl1.MinimumSize = new System.Drawing.Size(440, 160);
            this.commitSummaryUserControl1.Name = "commitSummaryUserControl1";
            this.commitSummaryUserControl1.Revision = null;
            this.commitSummaryUserControl1.Size = new System.Drawing.Size(458, 160);
            this.commitSummaryUserControl1.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "This revision will be archived:";
            // 
            // buttonArchiveRevision
            // 
            this.buttonArchiveRevision.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonArchiveRevision.AutoSize = true;
            this.buttonArchiveRevision.Image = global::GitUI.Properties.Resources.IconSaveAs;
            this.buttonArchiveRevision.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonArchiveRevision.Location = new System.Drawing.Point(480, 624);
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
            this.groupBox1.Location = new System.Drawing.Point(365, 594);
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
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.btnChooseRevision);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(479, 32);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(113, 162);
            this.flowLayoutPanel1.TabIndex = 5;
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
            // btnChooseRevision
            // 
            this.btnChooseRevision.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnChooseRevision.Image = global::GitUI.Properties.Resources.IconSelectRevision;
            this.btnChooseRevision.Location = new System.Drawing.Point(3, 33);
            this.btnChooseRevision.Name = "btnChooseRevision";
            this.btnChooseRevision.Size = new System.Drawing.Size(25, 24);
            this.btnChooseRevision.TabIndex = 31;
            this.btnChooseRevision.UseVisualStyleBackColor = true;
            this.btnChooseRevision.Click += new System.EventHandler(this.btnChooseRevision_Click);
            // 
            // groupBox2
            // 
            this.tableLayoutPanel4.SetColumnSpan(this.groupBox2, 2);
            this.groupBox2.Controls.Add(this.lblChooseDiffRevision);
            this.groupBox2.Controls.Add(this.commitSummaryUserControl2);
            this.groupBox2.Controls.Add(this.btnDiffChooseRevision);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.checkboxRevisionFilter);
            this.groupBox2.Controls.Add(this.textBoxPaths);
            this.groupBox2.Controls.Add(this.checkBoxPathFilter);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 216);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 20, 3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(588, 365);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Filter files";
            // 
            // lblChooseDiffRevision
            // 
            this.lblChooseDiffRevision.AutoSize = true;
            this.lblChooseDiffRevision.Location = new System.Drawing.Point(494, 198);
            this.lblChooseDiffRevision.Name = "lblChooseDiffRevision";
            this.lblChooseDiffRevision.Size = new System.Drawing.Size(91, 45);
            this.lblChooseDiffRevision.TabIndex = 0;
            this.lblChooseDiffRevision.Text = "Choose revision\r\nto compare\r\nwith first:";
            // 
            // commitSummaryUserControl2
            // 
            this.commitSummaryUserControl2.AutoSize = true;
            this.commitSummaryUserControl2.Location = new System.Drawing.Point(30, 198);
            this.commitSummaryUserControl2.Margin = new System.Windows.Forms.Padding(0);
            this.commitSummaryUserControl2.MinimumSize = new System.Drawing.Size(440, 160);
            this.commitSummaryUserControl2.Name = "commitSummaryUserControl2";
            this.commitSummaryUserControl2.Revision = null;
            this.commitSummaryUserControl2.Size = new System.Drawing.Size(455, 160);
            this.commitSummaryUserControl2.TabIndex = 1;
            // 
            // btnDiffChooseRevision
            // 
            this.btnDiffChooseRevision.Enabled = false;
            this.btnDiffChooseRevision.Image = global::GitUI.Properties.Resources.IconSelectRevision;
            this.btnDiffChooseRevision.Location = new System.Drawing.Point(494, 246);
            this.btnDiffChooseRevision.Name = "btnDiffChooseRevision";
            this.btnDiffChooseRevision.Size = new System.Drawing.Size(25, 24);
            this.btnDiffChooseRevision.TabIndex = 1;
            this.btnDiffChooseRevision.UseVisualStyleBackColor = true;
            this.btnDiffChooseRevision.Click += new System.EventHandler(this.btnDiffChooseRevision_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 130);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(194, 15);
            this.label4.TabIndex = 2;
            this.label4.Text = "separate each new path by new line";
            // 
            // checkboxRevisionFilter
            // 
            this.checkboxRevisionFilter.AutoSize = true;
            this.checkboxRevisionFilter.Location = new System.Drawing.Point(6, 167);
            this.checkboxRevisionFilter.Name = "checkboxRevisionFilter";
            this.checkboxRevisionFilter.Size = new System.Drawing.Size(483, 19);
            this.checkboxRevisionFilter.TabIndex = 0;
            this.checkboxRevisionFilter.Text = "Take the files that have changed from the first revision to the other and take on" +
    "ly those";
            this.checkboxRevisionFilter.UseVisualStyleBackColor = true;
            this.checkboxRevisionFilter.CheckedChanged += new System.EventHandler(this.checkboxRevisionFilter_CheckedChanged);
            // 
            // textBoxPaths
            // 
            this.textBoxPaths.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPaths.Location = new System.Drawing.Point(21, 52);
            this.textBoxPaths.Multiline = true;
            this.textBoxPaths.Name = "textBoxPaths";
            this.textBoxPaths.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxPaths.Size = new System.Drawing.Size(553, 75);
            this.textBoxPaths.TabIndex = 1;
            // 
            // checkBoxPathFilter
            // 
            this.checkBoxPathFilter.AutoSize = true;
            this.checkBoxPathFilter.Location = new System.Drawing.Point(6, 27);
            this.checkBoxPathFilter.Name = "checkBoxPathFilter";
            this.checkBoxPathFilter.Size = new System.Drawing.Size(167, 19);
            this.checkBoxPathFilter.TabIndex = 0;
            this.checkBoxPathFilter.Text = "Archive specific paths only";
            this.checkBoxPathFilter.UseVisualStyleBackColor = true;
            this.checkBoxPathFilter.CheckedChanged += new System.EventHandler(this.checkBoxPathFilter_CheckedChanged);
            // 
            // FormArchive
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(594, 652);
            this.Controls.Add(this.tableLayoutPanel4);
            this.MinimumSize = new System.Drawing.Size(610, 690);
            this.Name = "FormArchive";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Archive";
            this.Load += new System.EventHandler(this.FormArchive_Load);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
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
        private System.Windows.Forms.Label lblChooseDiffRevision;
        private UserControls.CommitSummaryUserControl commitSummaryUserControl2;
        private System.Windows.Forms.Button btnDiffChooseRevision;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkboxRevisionFilter;
        private System.Windows.Forms.TextBox textBoxPaths;
        private System.Windows.Forms.CheckBox checkBoxPathFilter;
    }
}