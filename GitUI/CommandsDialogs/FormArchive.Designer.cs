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
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(221, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Selected revision to archive:";
            // 
            // buttonArchiveRevision
            // 
            this.buttonArchiveRevision.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonArchiveRevision.AutoSize = true;
            this.buttonArchiveRevision.Image = global::GitUI.Properties.Resources.IconSaveAs;
            this.buttonArchiveRevision.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonArchiveRevision.Location = new System.Drawing.Point(599, 276);
            this.buttonArchiveRevision.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonArchiveRevision.Name = "buttonArchiveRevision";
            this.buttonArchiveRevision.Size = new System.Drawing.Size(139, 33);
            this.buttonArchiveRevision.TabIndex = 3;
            this.buttonArchiveRevision.Text = "Save as...";
            this.buttonArchiveRevision.UseVisualStyleBackColor = true;
            this.buttonArchiveRevision.Click += new System.EventHandler(this.Save_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Location = new System.Drawing.Point(455, 240);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(136, 69);
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
            this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 27);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(128, 38);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // _NO_TRANSLATE_radioButtonFormatZip
            // 
            this._NO_TRANSLATE_radioButtonFormatZip.AutoSize = true;
            this._NO_TRANSLATE_radioButtonFormatZip.Checked = true;
            this._NO_TRANSLATE_radioButtonFormatZip.Location = new System.Drawing.Point(4, 4);
            this._NO_TRANSLATE_radioButtonFormatZip.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._NO_TRANSLATE_radioButtonFormatZip.Name = "_NO_TRANSLATE_radioButtonFormatZip";
            this._NO_TRANSLATE_radioButtonFormatZip.Size = new System.Drawing.Size(53, 27);
            this._NO_TRANSLATE_radioButtonFormatZip.TabIndex = 0;
            this._NO_TRANSLATE_radioButtonFormatZip.TabStop = true;
            this._NO_TRANSLATE_radioButtonFormatZip.Text = "zip";
            this._NO_TRANSLATE_radioButtonFormatZip.UseVisualStyleBackColor = true;
            // 
            // _NO_TRANSLATE_radioButtonFormatTar
            // 
            this._NO_TRANSLATE_radioButtonFormatTar.AutoSize = true;
            this._NO_TRANSLATE_radioButtonFormatTar.Location = new System.Drawing.Point(68, 4);
            this._NO_TRANSLATE_radioButtonFormatTar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._NO_TRANSLATE_radioButtonFormatTar.Name = "_NO_TRANSLATE_radioButtonFormatTar";
            this._NO_TRANSLATE_radioButtonFormatTar.Size = new System.Drawing.Size(52, 27);
            this._NO_TRANSLATE_radioButtonFormatTar.TabIndex = 1;
            this._NO_TRANSLATE_radioButtonFormatTar.Text = "tar";
            this._NO_TRANSLATE_radioButtonFormatTar.UseVisualStyleBackColor = true;
            // 
            // commitSummaryUserControl1
            // 
            this.commitSummaryUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commitSummaryUserControl1.AutoSize = true;
            this.commitSummaryUserControl1.Location = new System.Drawing.Point(20, 27);
            this.commitSummaryUserControl1.Margin = new System.Windows.Forms.Padding(20, 4, 4, 4);
            this.commitSummaryUserControl1.MinimumSize = new System.Drawing.Size(550, 200);
            this.commitSummaryUserControl1.Name = "commitSummaryUserControl1";
            this.commitSummaryUserControl1.Revision = null;
            this.commitSummaryUserControl1.Size = new System.Drawing.Size(571, 200);
            this.commitSummaryUserControl1.TabIndex = 5;
            // 
            // btnChooseRevision
            // 
            this.btnChooseRevision.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnChooseRevision.Image = global::GitUI.Properties.Resources.IconSelectRevision;
            this.btnChooseRevision.Location = new System.Drawing.Point(105, 50);
            this.btnChooseRevision.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnChooseRevision.Name = "btnChooseRevision";
            this.btnChooseRevision.Size = new System.Drawing.Size(31, 30);
            this.btnChooseRevision.TabIndex = 31;
            this.btnChooseRevision.UseVisualStyleBackColor = true;
            this.btnChooseRevision.Click += new System.EventHandler(this.btnChooseRevision_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 46);
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
            this.tableLayoutPanel4.Controls.Add(this.buttonArchiveRevision, 1, 2);
            this.tableLayoutPanel4.Controls.Add(this.groupBox1, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.flowLayoutPanel1, 1, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(742, 313);
            this.tableLayoutPanel4.TabIndex = 6;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.btnChooseRevision);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(598, 26);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(141, 202);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // FormArchive
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(742, 313);
            this.Controls.Add(this.tableLayoutPanel4);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(760, 360);
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
    }
}