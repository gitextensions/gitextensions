namespace GitUI.CommandsDialogs.CommitDialog
{
    partial class FormCommitTemplateSettings
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
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            buttonOk = new Button();
            buttonCancel = new Button();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tableLayoutPanel5 = new TableLayoutPanel();
            labelCommitTemplate = new Label();
            _NO_TRANSLATE_comboBoxCommitTemplates = new ComboBox();
            labelCommitTemplateName = new Label();
            _NO_TRANSLATE_textCommitTemplateText = new TextBox();
            _NO_TRANSLATE_textBoxCommitTemplateName = new TextBox();
            tabPage2 = new TabPage();
            tableLayoutPanel3 = new TableLayoutPanel();
            labelUseIndent = new Label();
            labelMaxLineLength = new Label();
            labelMaxFirstLineLength = new Label();
            _NO_TRANSLATE_numericMaxLineLength = new NumericUpDown();
            _NO_TRANSLATE_numericMaxFirstLineLength = new NumericUpDown();
            labelRegExCheck = new Label();
            _NO_TRANSLATE_textBoxCommitValidationRegex = new TextBox();
            checkBoxUseIndent = new CheckBox();
            labelSecondLineEmpty = new Label();
            labelAutoWrap = new Label();
            checkBoxSecondLineEmpty = new CheckBox();
            checkBoxAutoWrap = new CheckBox();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tableLayoutPanel5.SuspendLayout();
            tabPage2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(_NO_TRANSLATE_numericMaxLineLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(_NO_TRANSLATE_numericMaxFirstLineLength)).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
            tableLayoutPanel1.Controls.Add(tabControl1, 0, 0);
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(698, 394);
            tableLayoutPanel1.TabIndex = 60;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.Controls.Add(buttonOk, 1, 0);
            tableLayoutPanel2.Controls.Add(buttonCancel, 2, 0);
            tableLayoutPanel2.Location = new Point(533, 362);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(162, 29);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // buttonOk
            // 
            buttonOk.Location = new Point(3, 3);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new Size(75, 23);
            buttonOk.TabIndex = 6;
            buttonOk.Text = "OK";
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += buttonOk_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(84, 3);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 7;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(3, 3);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(692, 353);
            tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(tableLayoutPanel5);
            tabPage1.Location = new Point(4, 22);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(684, 327);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Commit templates";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel5
            // 
            tableLayoutPanel5.ColumnCount = 2;
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.Controls.Add(labelCommitTemplate, 0, 2);
            tableLayoutPanel5.Controls.Add(_NO_TRANSLATE_comboBoxCommitTemplates, 0, 0);
            tableLayoutPanel5.Controls.Add(labelCommitTemplateName, 0, 1);
            tableLayoutPanel5.Controls.Add(_NO_TRANSLATE_textCommitTemplateText, 1, 2);
            tableLayoutPanel5.Controls.Add(_NO_TRANSLATE_textBoxCommitTemplateName, 1, 1);
            tableLayoutPanel5.Dock = DockStyle.Fill;
            tableLayoutPanel5.Location = new Point(3, 3);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 3;
            tableLayoutPanel5.RowStyles.Add(new RowStyle());
            tableLayoutPanel5.RowStyles.Add(new RowStyle());
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.Size = new Size(678, 321);
            tableLayoutPanel5.TabIndex = 1;
            // 
            // labelCommitTemplate
            // 
            labelCommitTemplate.AutoSize = true;
            labelCommitTemplate.Location = new Point(3, 59);
            labelCommitTemplate.Margin = new Padding(3, 6, 3, 0);
            labelCommitTemplate.Name = "labelCommitTemplate";
            labelCommitTemplate.Size = new Size(87, 13);
            labelCommitTemplate.TabIndex = 5;
            labelCommitTemplate.Text = "Commit template:";
            // 
            // _NO_TRANSLATE_comboBoxCommitTemplates
            // 
            _NO_TRANSLATE_comboBoxCommitTemplates.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel5.SetColumnSpan(_NO_TRANSLATE_comboBoxCommitTemplates, 2);
            _NO_TRANSLATE_comboBoxCommitTemplates.DropDownStyle = ComboBoxStyle.DropDownList;
            _NO_TRANSLATE_comboBoxCommitTemplates.FormattingEnabled = true;
            _NO_TRANSLATE_comboBoxCommitTemplates.Location = new Point(3, 3);
            _NO_TRANSLATE_comboBoxCommitTemplates.Name = "_NO_TRANSLATE_comboBoxCommitTemplates";
            _NO_TRANSLATE_comboBoxCommitTemplates.Size = new Size(672, 21);
            _NO_TRANSLATE_comboBoxCommitTemplates.TabIndex = 0;
            _NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndexChanged += comboBoxCommitTemplates_SelectedIndexChanged;
            // 
            // labelCommitTemplateName
            // 
            labelCommitTemplateName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            labelCommitTemplateName.AutoSize = true;
            labelCommitTemplateName.Location = new Point(3, 33);
            labelCommitTemplateName.Name = "labelCommitTemplateName";
            labelCommitTemplateName.Size = new Size(87, 13);
            labelCommitTemplateName.TabIndex = 7;
            labelCommitTemplateName.Text = "Name:";
            // 
            // _NO_TRANSLATE_textCommitTemplateText
            // 
            _NO_TRANSLATE_textCommitTemplateText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_textCommitTemplateText.Location = new Point(96, 56);
            _NO_TRANSLATE_textCommitTemplateText.Multiline = true;
            _NO_TRANSLATE_textCommitTemplateText.Name = "_NO_TRANSLATE_textCommitTemplateText";
            _NO_TRANSLATE_textCommitTemplateText.ScrollBars = ScrollBars.Vertical;
            _NO_TRANSLATE_textCommitTemplateText.Size = new Size(579, 262);
            _NO_TRANSLATE_textCommitTemplateText.TabIndex = 2;
            _NO_TRANSLATE_textCommitTemplateText.TextChanged += textCommitTemplateText_TextChanged;
            // 
            // _NO_TRANSLATE_textBoxCommitTemplateName
            // 
            _NO_TRANSLATE_textBoxCommitTemplateName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_textBoxCommitTemplateName.Location = new Point(96, 30);
            _NO_TRANSLATE_textBoxCommitTemplateName.Name = "_NO_TRANSLATE_textBoxCommitTemplateName";
            _NO_TRANSLATE_textBoxCommitTemplateName.Size = new Size(579, 20);
            _NO_TRANSLATE_textBoxCommitTemplateName.TabIndex = 1;
            _NO_TRANSLATE_textBoxCommitTemplateName.TextChanged += textBoxCommitTemplateName_TextChanged;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(tableLayoutPanel3);
            tabPage2.Location = new Point(4, 22);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(684, 327);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Commit validation";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel3.AutoSize = true;
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(labelUseIndent, 0, 4);
            tableLayoutPanel3.Controls.Add(labelMaxLineLength, 0, 1);
            tableLayoutPanel3.Controls.Add(labelMaxFirstLineLength, 0, 0);
            tableLayoutPanel3.Controls.Add(_NO_TRANSLATE_numericMaxLineLength, 1, 1);
            tableLayoutPanel3.Controls.Add(_NO_TRANSLATE_numericMaxFirstLineLength, 1, 0);
            tableLayoutPanel3.Controls.Add(labelRegExCheck, 0, 3);
            tableLayoutPanel3.Controls.Add(_NO_TRANSLATE_textBoxCommitValidationRegex, 1, 3);
            tableLayoutPanel3.Controls.Add(checkBoxUseIndent, 1, 4);
            tableLayoutPanel3.Controls.Add(labelSecondLineEmpty, 0, 5);
            tableLayoutPanel3.Controls.Add(labelAutoWrap, 0, 2);
            tableLayoutPanel3.Controls.Add(checkBoxSecondLineEmpty, 1, 5);
            tableLayoutPanel3.Controls.Add(checkBoxAutoWrap, 1, 2);
            tableLayoutPanel3.Location = new Point(0, 0);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 7;
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(686, 142);
            tableLayoutPanel3.TabIndex = 0;
            // 
            // labelUseIndent
            // 
            labelUseIndent.Anchor = AnchorStyles.Left;
            labelUseIndent.AutoSize = true;
            labelUseIndent.Location = new Point(3, 101);
            labelUseIndent.Name = "labelUseIndent";
            labelUseIndent.Size = new Size(126, 13);
            labelUseIndent.TabIndex = 11;
            labelUseIndent.Text = "Indent lines after first line:";
            // 
            // labelMaxLineLength
            // 
            labelMaxLineLength.Anchor = AnchorStyles.Left;
            labelMaxLineLength.AutoSize = true;
            labelMaxLineLength.Location = new Point(3, 32);
            labelMaxLineLength.Name = "labelMaxLineLength";
            labelMaxLineLength.Size = new Size(298, 13);
            labelMaxLineLength.TabIndex = 4;
            labelMaxLineLength.Text = "Maximum numbers of characters per line (0 = check disabled):";
            // 
            // labelMaxFirstLineLength
            // 
            labelMaxFirstLineLength.Anchor = AnchorStyles.Left;
            labelMaxFirstLineLength.AutoSize = true;
            labelMaxFirstLineLength.Location = new Point(3, 6);
            labelMaxFirstLineLength.Name = "labelMaxFirstLineLength";
            labelMaxFirstLineLength.Size = new Size(310, 13);
            labelMaxFirstLineLength.TabIndex = 0;
            labelMaxFirstLineLength.Text = "Maximum numbers of characters in first line (0 = check disabled):";
            // 
            // _NO_TRANSLATE_numericMaxLineLength
            // 
            _NO_TRANSLATE_numericMaxLineLength.Anchor = AnchorStyles.Left;
            _NO_TRANSLATE_numericMaxLineLength.Location = new Point(319, 29);
            _NO_TRANSLATE_numericMaxLineLength.Name = "_NO_TRANSLATE_numericMaxLineLength";
            _NO_TRANSLATE_numericMaxLineLength.Size = new Size(60, 20);
            _NO_TRANSLATE_numericMaxLineLength.TabIndex = 4;
            // 
            // _NO_TRANSLATE_numericMaxFirstLineLength
            // 
            _NO_TRANSLATE_numericMaxFirstLineLength.Anchor = AnchorStyles.Left;
            _NO_TRANSLATE_numericMaxFirstLineLength.Location = new Point(319, 3);
            _NO_TRANSLATE_numericMaxFirstLineLength.Name = "_NO_TRANSLATE_numericMaxFirstLineLength";
            _NO_TRANSLATE_numericMaxFirstLineLength.Size = new Size(60, 20);
            _NO_TRANSLATE_numericMaxFirstLineLength.TabIndex = 3;
            // 
            // labelRegExCheck
            // 
            labelRegExCheck.Anchor = AnchorStyles.Left;
            labelRegExCheck.AutoSize = true;
            labelRegExCheck.Location = new Point(3, 78);
            labelRegExCheck.Name = "labelRegExCheck";
            labelRegExCheck.Size = new Size(302, 13);
            labelRegExCheck.TabIndex = 7;
            labelRegExCheck.Text = "Commit must match following RegEx (Empty = check disabled):";
            // 
            // _NO_TRANSLATE_textBoxCommitValidationRegex
            // 
            _NO_TRANSLATE_textBoxCommitValidationRegex.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_textBoxCommitValidationRegex.Location = new Point(319, 75);
            _NO_TRANSLATE_textBoxCommitValidationRegex.Name = "_NO_TRANSLATE_textBoxCommitValidationRegex";
            _NO_TRANSLATE_textBoxCommitValidationRegex.Size = new Size(364, 20);
            _NO_TRANSLATE_textBoxCommitValidationRegex.TabIndex = 8;
            // 
            // checkBoxUseIndent
            // 
            checkBoxUseIndent.Anchor = AnchorStyles.Left;
            checkBoxUseIndent.AutoSize = true;
            checkBoxUseIndent.Location = new Point(319, 101);
            checkBoxUseIndent.Name = "checkBoxUseIndent";
            checkBoxUseIndent.Size = new Size(15, 14);
            checkBoxUseIndent.TabIndex = 10;
            checkBoxUseIndent.UseVisualStyleBackColor = true;
            // 
            // labelSecondLineEmpty
            // 
            labelSecondLineEmpty.Anchor = AnchorStyles.Left;
            labelSecondLineEmpty.AutoSize = true;
            labelSecondLineEmpty.Location = new Point(3, 121);
            labelSecondLineEmpty.Name = "labelSecondLineEmpty";
            labelSecondLineEmpty.Size = new Size(137, 13);
            labelSecondLineEmpty.TabIndex = 6;
            labelSecondLineEmpty.Text = "Second line must be empty:";
            // 
            // labelAutoWrap
            // 
            labelAutoWrap.Anchor = AnchorStyles.Left;
            labelAutoWrap.AutoSize = true;
            labelAutoWrap.Location = new Point(3, 55);
            labelAutoWrap.Name = "labelAutoWrap";
            labelAutoWrap.Size = new Size(233, 13);
            labelAutoWrap.TabIndex = 13;
            labelAutoWrap.Text = "Auto-wrap commit message (except subject line)";
            // 
            // checkBoxSecondLineEmpty
            // 
            checkBoxSecondLineEmpty.Anchor = AnchorStyles.Left;
            checkBoxSecondLineEmpty.AutoSize = true;
            checkBoxSecondLineEmpty.Location = new Point(319, 121);
            checkBoxSecondLineEmpty.Name = "checkBoxSecondLineEmpty";
            checkBoxSecondLineEmpty.Size = new Size(15, 14);
            checkBoxSecondLineEmpty.TabIndex = 5;
            checkBoxSecondLineEmpty.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoWrap
            // 
            checkBoxAutoWrap.Anchor = AnchorStyles.Left;
            checkBoxAutoWrap.AutoSize = true;
            checkBoxAutoWrap.Location = new Point(319, 55);
            checkBoxAutoWrap.Name = "checkBoxAutoWrap";
            checkBoxAutoWrap.Size = new Size(15, 14);
            checkBoxAutoWrap.TabIndex = 14;
            checkBoxAutoWrap.UseVisualStyleBackColor = true;
            // 
            // FormCommitTemplateSettings
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(698, 394);
            Controls.Add(tableLayoutPanel1);
            MinimumSize = new Size(550, 400);
            Name = "FormCommitTemplateSettings";
            Text = "Commit message settings";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tableLayoutPanel5.ResumeLayout(false);
            tableLayoutPanel5.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(_NO_TRANSLATE_numericMaxLineLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(_NO_TRANSLATE_numericMaxFirstLineLength)).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Button buttonOk;
        private Button buttonCancel;
        private TableLayoutPanel tableLayoutPanel3;
        private NumericUpDown _NO_TRANSLATE_numericMaxLineLength;
        private Label labelMaxFirstLineLength;
        private NumericUpDown _NO_TRANSLATE_numericMaxFirstLineLength;
        private Label labelMaxLineLength;
        private Label labelSecondLineEmpty;
        private CheckBox checkBoxSecondLineEmpty;
        private TableLayoutPanel tableLayoutPanel5;
        private Label labelCommitTemplate;
        private TextBox _NO_TRANSLATE_textCommitTemplateText;
        private ComboBox _NO_TRANSLATE_comboBoxCommitTemplates;
        private Label labelCommitTemplateName;
        private TextBox _NO_TRANSLATE_textBoxCommitTemplateName;
        private Label labelRegExCheck;
        private TextBox _NO_TRANSLATE_textBoxCommitValidationRegex;
        private Label labelUseIndent;
        private CheckBox checkBoxUseIndent;
        private Label labelAutoWrap;
        private CheckBox checkBoxAutoWrap;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
    }
}