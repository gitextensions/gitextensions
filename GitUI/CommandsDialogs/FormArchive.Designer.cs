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
            tableLayoutPanel4 = new TableLayoutPanel();
            commitSummaryUserControl1 = new GitUI.UserControls.CommitSummaryUserControl();
            label1 = new Label();
            buttonArchiveRevision = new Button();
            groupBox1 = new GroupBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            _NO_TRANSLATE_radioButtonFormatZip = new RadioButton();
            _NO_TRANSLATE_radioButtonFormatTar = new RadioButton();
            flowLayoutPanel1 = new FlowLayoutPanel();
            label2 = new Label();
            btnChooseRevision = new Button();
            groupBox2 = new GroupBox();
            gbDiffRevision = new GroupBox();
            labelAuthorCaption = new Label();
            labelDateCaption = new Label();
            labelMessage = new Label();
            labelAuthor = new Label();
            lblChooseDiffRevision = new Label();
            btnDiffChooseRevision = new Button();
            label4 = new Label();
            checkboxRevisionFilter = new CheckBox();
            textBoxPaths = new TextBox();
            checkBoxPathFilter = new CheckBox();
            tableLayoutPanel4.SuspendLayout();
            groupBox1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            groupBox2.SuspendLayout();
            gbDiffRevision.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 2;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel4.Controls.Add(commitSummaryUserControl1, 0, 1);
            tableLayoutPanel4.Controls.Add(label1, 0, 0);
            tableLayoutPanel4.Controls.Add(buttonArchiveRevision, 1, 3);
            tableLayoutPanel4.Controls.Add(groupBox1, 0, 3);
            tableLayoutPanel4.Controls.Add(flowLayoutPanel1, 1, 1);
            tableLayoutPanel4.Controls.Add(groupBox2, 0, 2);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(0, 0);
            tableLayoutPanel4.Margin = new Padding(2);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 4;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel4.Size = new Size(594, 571);
            tableLayoutPanel4.TabIndex = 6;
            // 
            // commitSummaryUserControl1
            // 
            commitSummaryUserControl1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            commitSummaryUserControl1.AutoSize = true;
            commitSummaryUserControl1.Location = new Point(16, 33);
            commitSummaryUserControl1.Margin = new Padding(16, 3, 3, 3);
            commitSummaryUserControl1.MinimumSize = new Size(440, 160);
            commitSummaryUserControl1.Name = "commitSummaryUserControl1";
            commitSummaryUserControl1.Revision = null;
            commitSummaryUserControl1.Size = new Size(458, 160);
            commitSummaryUserControl1.TabIndex = 5;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(3, 7);
            label1.Name = "label1";
            label1.Size = new Size(161, 15);
            label1.TabIndex = 0;
            label1.Text = "This revision will be archived:";
            // 
            // buttonArchiveRevision
            // 
            buttonArchiveRevision.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonArchiveRevision.AutoSize = true;
            buttonArchiveRevision.Image = Properties.Images.SaveAs;
            buttonArchiveRevision.ImageAlign = ContentAlignment.MiddleLeft;
            buttonArchiveRevision.Location = new Point(480, 545);
            buttonArchiveRevision.Name = "buttonArchiveRevision";
            buttonArchiveRevision.Size = new Size(111, 25);
            buttonArchiveRevision.TabIndex = 3;
            buttonArchiveRevision.Text = "Save as...";
            buttonArchiveRevision.UseVisualStyleBackColor = true;
            buttonArchiveRevision.Click += Save_Click;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            groupBox1.Controls.Add(tableLayoutPanel2);
            groupBox1.Location = new Point(365, 522);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(109, 48);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Archive format";
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Controls.Add(_NO_TRANSLATE_radioButtonFormatZip, 0, 0);
            tableLayoutPanel2.Controls.Add(_NO_TRANSLATE_radioButtonFormatTar, 1, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 19);
            tableLayoutPanel2.Margin = new Padding(0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new Size(103, 26);
            tableLayoutPanel2.TabIndex = 2;
            // 
            // _NO_TRANSLATE_radioButtonFormatZip
            // 
            _NO_TRANSLATE_radioButtonFormatZip.AutoSize = true;
            _NO_TRANSLATE_radioButtonFormatZip.Checked = true;
            _NO_TRANSLATE_radioButtonFormatZip.Location = new Point(3, 3);
            _NO_TRANSLATE_radioButtonFormatZip.Name = "_NO_TRANSLATE_radioButtonFormatZip";
            _NO_TRANSLATE_radioButtonFormatZip.Size = new Size(40, 19);
            _NO_TRANSLATE_radioButtonFormatZip.TabIndex = 0;
            _NO_TRANSLATE_radioButtonFormatZip.TabStop = true;
            _NO_TRANSLATE_radioButtonFormatZip.Text = "zip";
            _NO_TRANSLATE_radioButtonFormatZip.UseVisualStyleBackColor = true;
            // 
            // _NO_TRANSLATE_radioButtonFormatTar
            // 
            _NO_TRANSLATE_radioButtonFormatTar.AutoSize = true;
            _NO_TRANSLATE_radioButtonFormatTar.Location = new Point(54, 3);
            _NO_TRANSLATE_radioButtonFormatTar.Name = "_NO_TRANSLATE_radioButtonFormatTar";
            _NO_TRANSLATE_radioButtonFormatTar.Size = new Size(39, 19);
            _NO_TRANSLATE_radioButtonFormatTar.TabIndex = 1;
            _NO_TRANSLATE_radioButtonFormatTar.Text = "tar";
            _NO_TRANSLATE_radioButtonFormatTar.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(label2);
            flowLayoutPanel1.Controls.Add(btnChooseRevision);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(479, 32);
            flowLayoutPanel1.Margin = new Padding(2);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(113, 162);
            flowLayoutPanel1.TabIndex = 5;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 0);
            label2.Name = "label2";
            label2.Size = new Size(91, 30);
            label2.TabIndex = 32;
            label2.Text = "Choose another\r\nrevision:";
            // 
            // btnChooseRevision
            // 
            btnChooseRevision.Anchor = AnchorStyles.Left;
            btnChooseRevision.Image = Properties.Images.SelectRevision;
            btnChooseRevision.Location = new Point(3, 33);
            btnChooseRevision.Name = "btnChooseRevision";
            btnChooseRevision.Size = new Size(25, 24);
            btnChooseRevision.TabIndex = 31;
            btnChooseRevision.UseVisualStyleBackColor = true;
            btnChooseRevision.Click += btnChooseRevision_Click;
            // 
            // groupBox2
            // 
            tableLayoutPanel4.SetColumnSpan(groupBox2, 2);
            groupBox2.Controls.Add(gbDiffRevision);
            groupBox2.Controls.Add(lblChooseDiffRevision);
            groupBox2.Controls.Add(btnDiffChooseRevision);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(checkboxRevisionFilter);
            groupBox2.Controls.Add(textBoxPaths);
            groupBox2.Controls.Add(checkBoxPathFilter);
            groupBox2.Dock = DockStyle.Fill;
            groupBox2.Location = new Point(3, 216);
            groupBox2.Margin = new Padding(3, 20, 3, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(588, 300);
            groupBox2.TabIndex = 6;
            groupBox2.TabStop = false;
            groupBox2.Text = "Filter files";
            // 
            // gbDiffRevision
            // 
            gbDiffRevision.Controls.Add(labelAuthorCaption);
            gbDiffRevision.Controls.Add(labelDateCaption);
            gbDiffRevision.Controls.Add(labelMessage);
            gbDiffRevision.Controls.Add(labelAuthor);
            gbDiffRevision.Location = new Point(13, 192);
            gbDiffRevision.Name = "gbDiffRevision";
            gbDiffRevision.Size = new Size(458, 100);
            gbDiffRevision.TabIndex = 4;
            gbDiffRevision.TabStop = false;
            // 
            // labelAuthorCaption
            // 
            labelAuthorCaption.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelAuthorCaption.AutoSize = true;
            labelAuthorCaption.Location = new Point(5, 78);
            labelAuthorCaption.Name = "labelAuthorCaption";
            labelAuthorCaption.Size = new Size(47, 15);
            labelAuthorCaption.TabIndex = 23;
            labelAuthorCaption.Text = "Author:";
            // 
            // labelDateCaption
            // 
            labelDateCaption.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelDateCaption.AutoSize = true;
            labelDateCaption.Location = new Point(227, 78);
            labelDateCaption.Name = "labelDateCaption";
            labelDateCaption.Size = new Size(80, 15);
            labelDateCaption.TabIndex = 22;
            labelDateCaption.Text = "Commit date:";
            // 
            // labelMessage
            // 
            labelMessage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelMessage.AutoEllipsis = true;
            labelMessage.Location = new Point(5, 19);
            labelMessage.MaximumSize = new Size(422, 50);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new Size(422, 50);
            labelMessage.TabIndex = 21;
            labelMessage.Text = "...";
            labelMessage.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelAuthor
            // 
            labelAuthor.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelAuthor.AutoSize = true;
            labelAuthor.Location = new Point(98, 79);
            labelAuthor.Name = "labelAuthor";
            labelAuthor.Size = new Size(19, 13);
            labelAuthor.TabIndex = 20;
            labelAuthor.Text = "...";
            // 
            // lblChooseDiffRevision
            // 
            lblChooseDiffRevision.Location = new Point(479, 198);
            lblChooseDiffRevision.Name = "lblChooseDiffRevision";
            lblChooseDiffRevision.Size = new Size(112, 35);
            lblChooseDiffRevision.TabIndex = 0;
            lblChooseDiffRevision.Text = "Choose revision to \r\ncompare with first:";
            // 
            // btnDiffChooseRevision
            // 
            btnDiffChooseRevision.Enabled = false;
            btnDiffChooseRevision.Image = Properties.Images.SelectRevision;
            btnDiffChooseRevision.Location = new Point(479, 234);
            btnDiffChooseRevision.Name = "btnDiffChooseRevision";
            btnDiffChooseRevision.Size = new Size(25, 24);
            btnDiffChooseRevision.TabIndex = 1;
            btnDiffChooseRevision.UseVisualStyleBackColor = true;
            btnDiffChooseRevision.Click += btnDiffChooseRevision_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(24, 130);
            label4.Name = "label4";
            label4.Size = new Size(194, 15);
            label4.TabIndex = 2;
            label4.Text = "separate each new path by new line";
            // 
            // checkboxRevisionFilter
            // 
            checkboxRevisionFilter.AutoSize = true;
            checkboxRevisionFilter.Location = new Point(6, 167);
            checkboxRevisionFilter.Name = "checkboxRevisionFilter";
            checkboxRevisionFilter.Size = new Size(505, 19);
            checkboxRevisionFilter.TabIndex = 0;
            checkboxRevisionFilter.Text = "Take the files that have changed from the revision above to this one and archive " +
    "only those";
            checkboxRevisionFilter.UseVisualStyleBackColor = true;
            checkboxRevisionFilter.CheckedChanged += checkboxRevisionFilter_CheckedChanged;
            // 
            // textBoxPaths
            // 
            textBoxPaths.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxPaths.Location = new Point(21, 52);
            textBoxPaths.Multiline = true;
            textBoxPaths.Name = "textBoxPaths";
            textBoxPaths.ScrollBars = ScrollBars.Vertical;
            textBoxPaths.Size = new Size(553, 75);
            textBoxPaths.TabIndex = 1;
            // 
            // checkBoxPathFilter
            // 
            checkBoxPathFilter.AutoSize = true;
            checkBoxPathFilter.Location = new Point(6, 27);
            checkBoxPathFilter.Name = "checkBoxPathFilter";
            checkBoxPathFilter.Size = new Size(167, 19);
            checkBoxPathFilter.TabIndex = 0;
            checkBoxPathFilter.Text = "Archive specific paths only";
            checkBoxPathFilter.UseVisualStyleBackColor = true;
            checkBoxPathFilter.CheckedChanged += checkBoxPathFilter_CheckedChanged;
            // 
            // FormArchive
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(594, 571);
            Controls.Add(tableLayoutPanel4);
            MinimumSize = new Size(610, 609);
            Name = "FormArchive";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Archive";
            Load += FormArchive_Load;
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            gbDiffRevision.ResumeLayout(false);
            gbDiffRevision.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private Label label1;
        private Button buttonArchiveRevision;
        private GroupBox groupBox1;
        private TableLayoutPanel tableLayoutPanel2;
        private RadioButton _NO_TRANSLATE_radioButtonFormatZip;
        private RadioButton _NO_TRANSLATE_radioButtonFormatTar;
        private GitUI.UserControls.CommitSummaryUserControl commitSummaryUserControl1;
        private Button btnChooseRevision;
        private Label label2;
        private TableLayoutPanel tableLayoutPanel4;
        private FlowLayoutPanel flowLayoutPanel1;
        private GroupBox groupBox2;
        private Label lblChooseDiffRevision;
        private Button btnDiffChooseRevision;
        private Label label4;
        private CheckBox checkboxRevisionFilter;
        private TextBox textBoxPaths;
        private CheckBox checkBoxPathFilter;
        private GroupBox gbDiffRevision;
        private Label labelAuthorCaption;
        private Label labelDateCaption;
        private Label labelMessage;
        private Label labelAuthor;
    }
}