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
            labelMaxFirstLineLength = new Label();
            _NO_TRANSLATE_numericMaxFirstLineLength = new NumericUpDown();
            labelMaxLineLength = new Label();
            _NO_TRANSLATE_numericMaxLineLength = new NumericUpDown();
            labelAutoWrap = new Label();
            checkBoxAutoWrap = new CheckBox();
            labelRegExCheck = new Label();
            _NO_TRANSLATE_textBoxCommitValidationRegex = new TextBox();
            labelUseIndent = new Label();
            checkBoxUseIndent = new CheckBox();
            labelSecondLineEmpty = new Label();
            checkBoxSecondLineEmpty = new CheckBox();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tableLayoutPanel5.SuspendLayout();
            tabPage2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_NO_TRANSLATE_numericMaxFirstLineLength).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_NO_TRANSLATE_numericMaxLineLength).BeginInit();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.AutoSize = true;
            MainPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MainPanel.Controls.Add(tabControl1);
            MainPanel.Padding = new Padding(9);
            MainPanel.Size = new Size(698, 320);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(buttonCancel);
            ControlsPanel.Controls.Add(buttonOk);
            ControlsPanel.Location = new Point(0, 102);
            ControlsPanel.Size = new Size(698, 320);
            // 
            // buttonOk
            // 
            buttonOk.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonOk.AutoSize = true;
            buttonOk.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonOk.ForeColor = SystemColors.ControlText;
            buttonOk.Location = new Point(529, 8);
            buttonOk.MinimumSize = new Size(75, 25);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new Size(75, 25);
            buttonOk.TabIndex = 20;
            buttonOk.Text = "OK";
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += buttonOk_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonCancel.AutoSize = true;
            buttonCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonCancel.ForeColor = SystemColors.ControlText;
            buttonCancel.Location = new Point(610, 8);
            buttonCancel.MinimumSize = new Size(75, 25);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 25);
            buttonCancel.TabIndex = 22;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(9, 9);
            tabControl1.Margin = new Padding(0);
            tabControl1.Name = "tabControl1";
            tabControl1.Padding = new Point(0, 0);
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(680, 302);
            tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(tableLayoutPanel5);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(672, 274);
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
            tableLayoutPanel5.Size = new Size(666, 268);
            tableLayoutPanel5.TabIndex = 1;
            // 
            // labelCommitTemplate
            // 
            labelCommitTemplate.AutoSize = true;
            labelCommitTemplate.Location = new Point(3, 64);
            labelCommitTemplate.Margin = new Padding(3, 6, 3, 0);
            labelCommitTemplate.Name = "labelCommitTemplate";
            labelCommitTemplate.Size = new Size(104, 15);
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
            _NO_TRANSLATE_comboBoxCommitTemplates.Size = new Size(660, 23);
            _NO_TRANSLATE_comboBoxCommitTemplates.TabIndex = 0;
            _NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndexChanged += comboBoxCommitTemplates_SelectedIndexChanged;
            // 
            // labelCommitTemplateName
            // 
            labelCommitTemplateName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            labelCommitTemplateName.AutoSize = true;
            labelCommitTemplateName.Location = new Point(3, 36);
            labelCommitTemplateName.Name = "labelCommitTemplateName";
            labelCommitTemplateName.Size = new Size(104, 15);
            labelCommitTemplateName.TabIndex = 7;
            labelCommitTemplateName.Text = "Name:";
            // 
            // _NO_TRANSLATE_textCommitTemplateText
            // 
            _NO_TRANSLATE_textCommitTemplateText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_textCommitTemplateText.Location = new Point(113, 61);
            _NO_TRANSLATE_textCommitTemplateText.Multiline = true;
            _NO_TRANSLATE_textCommitTemplateText.Name = "_NO_TRANSLATE_textCommitTemplateText";
            _NO_TRANSLATE_textCommitTemplateText.ScrollBars = ScrollBars.Vertical;
            _NO_TRANSLATE_textCommitTemplateText.Size = new Size(550, 204);
            _NO_TRANSLATE_textCommitTemplateText.TabIndex = 2;
            _NO_TRANSLATE_textCommitTemplateText.TextChanged += textCommitTemplateText_TextChanged;
            // 
            // _NO_TRANSLATE_textBoxCommitTemplateName
            // 
            _NO_TRANSLATE_textBoxCommitTemplateName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_textBoxCommitTemplateName.Location = new Point(113, 32);
            _NO_TRANSLATE_textBoxCommitTemplateName.Name = "_NO_TRANSLATE_textBoxCommitTemplateName";
            _NO_TRANSLATE_textBoxCommitTemplateName.Size = new Size(550, 23);
            _NO_TRANSLATE_textBoxCommitTemplateName.TabIndex = 1;
            _NO_TRANSLATE_textBoxCommitTemplateName.TextChanged += textBoxCommitTemplateName_TextChanged;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(tableLayoutPanel3);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(672, 274);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Commit validation";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.AutoSize = true;
            tableLayoutPanel3.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(labelMaxFirstLineLength, 0, 0);
            tableLayoutPanel3.Controls.Add(_NO_TRANSLATE_numericMaxFirstLineLength, 1, 0);
            tableLayoutPanel3.Controls.Add(labelMaxLineLength, 0, 1);
            tableLayoutPanel3.Controls.Add(_NO_TRANSLATE_numericMaxLineLength, 1, 1);
            tableLayoutPanel3.Controls.Add(labelAutoWrap, 0, 2);
            tableLayoutPanel3.Controls.Add(checkBoxAutoWrap, 1, 2);
            tableLayoutPanel3.Controls.Add(labelRegExCheck, 0, 3);
            tableLayoutPanel3.Controls.Add(_NO_TRANSLATE_textBoxCommitValidationRegex, 1, 3);
            tableLayoutPanel3.Controls.Add(labelUseIndent, 0, 4);
            tableLayoutPanel3.Controls.Add(checkBoxUseIndent, 1, 4);
            tableLayoutPanel3.Controls.Add(labelSecondLineEmpty, 0, 5);
            tableLayoutPanel3.Controls.Add(checkBoxSecondLineEmpty, 1, 5);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 3);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 7;
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(666, 268);
            tableLayoutPanel3.TabIndex = 0;
            // 
            // labelMaxFirstLineLength
            // 
            labelMaxFirstLineLength.Anchor = AnchorStyles.Left;
            labelMaxFirstLineLength.AutoSize = true;
            labelMaxFirstLineLength.Location = new Point(3, 7);
            labelMaxFirstLineLength.Name = "labelMaxFirstLineLength";
            labelMaxFirstLineLength.Size = new Size(368, 15);
            labelMaxFirstLineLength.TabIndex = 3;
            labelMaxFirstLineLength.Text = "Maximum number of characters in the first line (0 = check disabled):";
            // 
            // _NO_TRANSLATE_numericMaxFirstLineLength
            // 
            _NO_TRANSLATE_numericMaxFirstLineLength.Anchor = AnchorStyles.Left;
            _NO_TRANSLATE_numericMaxFirstLineLength.AutoSize = true;
            _NO_TRANSLATE_numericMaxFirstLineLength.Location = new Point(377, 3);
            _NO_TRANSLATE_numericMaxFirstLineLength.MinimumSize = new Size(60, 0);
            _NO_TRANSLATE_numericMaxFirstLineLength.Name = "_NO_TRANSLATE_numericMaxFirstLineLength";
            _NO_TRANSLATE_numericMaxFirstLineLength.Size = new Size(60, 23);
            _NO_TRANSLATE_numericMaxFirstLineLength.TabIndex = 4;
            // 
            // labelMaxLineLength
            // 
            labelMaxLineLength.Anchor = AnchorStyles.Left;
            labelMaxLineLength.AutoSize = true;
            labelMaxLineLength.Location = new Point(3, 36);
            labelMaxLineLength.Name = "labelMaxLineLength";
            labelMaxLineLength.Size = new Size(332, 15);
            labelMaxLineLength.TabIndex = 5;
            labelMaxLineLength.Text = "Maximum number of characters per line (0 = check disabled):";
            // 
            // _NO_TRANSLATE_numericMaxLineLength
            // 
            _NO_TRANSLATE_numericMaxLineLength.Anchor = AnchorStyles.Left;
            _NO_TRANSLATE_numericMaxLineLength.AutoSize = true;
            _NO_TRANSLATE_numericMaxLineLength.Location = new Point(377, 32);
            _NO_TRANSLATE_numericMaxLineLength.MinimumSize = new Size(60, 0);
            _NO_TRANSLATE_numericMaxLineLength.Name = "_NO_TRANSLATE_numericMaxLineLength";
            _NO_TRANSLATE_numericMaxLineLength.Size = new Size(60, 23);
            _NO_TRANSLATE_numericMaxLineLength.TabIndex = 6;
            // 
            // labelAutoWrap
            // 
            labelAutoWrap.Anchor = AnchorStyles.Left;
            labelAutoWrap.AutoSize = true;
            labelAutoWrap.Location = new Point(3, 64);
            labelAutoWrap.Name = "labelAutoWrap";
            labelAutoWrap.Size = new Size(267, 15);
            labelAutoWrap.TabIndex = 7;
            labelAutoWrap.Text = "Auto-wrap commit message (except subject line)";
            // 
            // checkBoxAutoWrap
            // 
            checkBoxAutoWrap.Anchor = AnchorStyles.Left;
            checkBoxAutoWrap.AutoSize = true;
            checkBoxAutoWrap.Location = new Point(377, 65);
            checkBoxAutoWrap.Margin = new Padding(3, 7, 3, 7);
            checkBoxAutoWrap.Name = "checkBoxAutoWrap";
            checkBoxAutoWrap.Size = new Size(15, 14);
            checkBoxAutoWrap.TabIndex = 12;
            checkBoxAutoWrap.UseVisualStyleBackColor = true;
            // 
            // labelRegExCheck
            // 
            labelRegExCheck.Anchor = AnchorStyles.Left;
            labelRegExCheck.AutoSize = true;
            labelRegExCheck.Location = new Point(3, 93);
            labelRegExCheck.Name = "labelRegExCheck";
            labelRegExCheck.Size = new Size(346, 15);
            labelRegExCheck.TabIndex = 13;
            labelRegExCheck.Text = "Commit must match following RegEx (Empty = check disabled):";
            // 
            // _NO_TRANSLATE_textBoxCommitValidationRegex
            // 
            _NO_TRANSLATE_textBoxCommitValidationRegex.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_textBoxCommitValidationRegex.Location = new Point(377, 89);
            _NO_TRANSLATE_textBoxCommitValidationRegex.Name = "_NO_TRANSLATE_textBoxCommitValidationRegex";
            _NO_TRANSLATE_textBoxCommitValidationRegex.Size = new Size(286, 23);
            _NO_TRANSLATE_textBoxCommitValidationRegex.TabIndex = 14;
            // 
            // labelUseIndent
            // 
            labelUseIndent.Anchor = AnchorStyles.Left;
            labelUseIndent.AutoSize = true;
            labelUseIndent.Location = new Point(3, 121);
            labelUseIndent.Name = "labelUseIndent";
            labelUseIndent.Size = new Size(163, 15);
            labelUseIndent.TabIndex = 15;
            labelUseIndent.Text = "Indent lines after the first line:";
            // 
            // checkBoxUseIndent
            // 
            checkBoxUseIndent.Anchor = AnchorStyles.Left;
            checkBoxUseIndent.AutoSize = true;
            checkBoxUseIndent.Location = new Point(377, 122);
            checkBoxUseIndent.Margin = new Padding(3, 7, 3, 7);
            checkBoxUseIndent.Name = "checkBoxUseIndent";
            checkBoxUseIndent.Size = new Size(15, 14);
            checkBoxUseIndent.TabIndex = 16;
            checkBoxUseIndent.UseVisualStyleBackColor = true;
            // 
            // labelSecondLineEmpty
            // 
            labelSecondLineEmpty.Anchor = AnchorStyles.Left;
            labelSecondLineEmpty.AutoSize = true;
            labelSecondLineEmpty.Location = new Point(3, 149);
            labelSecondLineEmpty.Name = "labelSecondLineEmpty";
            labelSecondLineEmpty.Size = new Size(154, 15);
            labelSecondLineEmpty.TabIndex = 17;
            labelSecondLineEmpty.Text = "Second line must be empty:";
            // 
            // checkBoxSecondLineEmpty
            // 
            checkBoxSecondLineEmpty.Anchor = AnchorStyles.Left;
            checkBoxSecondLineEmpty.AutoSize = true;
            checkBoxSecondLineEmpty.Location = new Point(377, 150);
            checkBoxSecondLineEmpty.Margin = new Padding(3, 7, 3, 7);
            checkBoxSecondLineEmpty.Name = "checkBoxSecondLineEmpty";
            checkBoxSecondLineEmpty.Size = new Size(15, 14);
            checkBoxSecondLineEmpty.TabIndex = 18;
            checkBoxSecondLineEmpty.UseVisualStyleBackColor = true;
            // 
            // FormCommitTemplateSettings
            // 
            AcceptButton = buttonOk;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            CancelButton = buttonCancel;
            ClientSize = new Size(698, 361);
            MinimizeBox = false;
            MinimumSize = new Size(550, 310);
            Name = "FormCommitTemplateSettings";
            Text = "Commit message settings";
            MainPanel.ResumeLayout(false);
            MainPanel.PerformLayout();
            ControlsPanel.ResumeLayout(false);
            ControlsPanel.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tableLayoutPanel5.ResumeLayout(false);
            tableLayoutPanel5.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_NO_TRANSLATE_numericMaxFirstLineLength).EndInit();
            ((System.ComponentModel.ISupportInitialize)_NO_TRANSLATE_numericMaxLineLength).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

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
