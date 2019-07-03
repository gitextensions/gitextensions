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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.labelCommitTemplate = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_comboBoxCommitTemplates = new System.Windows.Forms.ComboBox();
            this.labelCommitTemplateName = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_textCommitTemplateText = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_textBoxCommitTemplateName = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.labelUseIndent = new System.Windows.Forms.Label();
            this.labelMaxLineLength = new System.Windows.Forms.Label();
            this.labelMaxFirstLineLength = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_numericMaxLineLength = new System.Windows.Forms.NumericUpDown();
            this._NO_TRANSLATE_numericMaxFirstLineLength = new System.Windows.Forms.NumericUpDown();
            this.labelRegExCheck = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_textBoxCommitValidationRegex = new System.Windows.Forms.TextBox();
            this.checkBoxUseIndent = new System.Windows.Forms.CheckBox();
            this.labelSecondLineEmpty = new System.Windows.Forms.Label();
            this.labelAutoWrap = new System.Windows.Forms.Label();
            this.checkBoxSecondLineEmpty = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoWrap = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_numericMaxLineLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_numericMaxFirstLineLength)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(698, 394);
            this.tableLayoutPanel1.TabIndex = 60;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.buttonOk, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonCancel, 2, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(533, 362);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(162, 29);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(3, 3);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 6;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(84, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(692, 353);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel5);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(684, 327);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Commit templates";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.labelCommitTemplate, 0, 2);
            this.tableLayoutPanel5.Controls.Add(this._NO_TRANSLATE_comboBoxCommitTemplates, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.labelCommitTemplateName, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this._NO_TRANSLATE_textCommitTemplateText, 1, 2);
            this.tableLayoutPanel5.Controls.Add(this._NO_TRANSLATE_textBoxCommitTemplateName, 1, 1);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 3;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(678, 321);
            this.tableLayoutPanel5.TabIndex = 1;
            // 
            // labelCommitTemplate
            // 
            this.labelCommitTemplate.AutoSize = true;
            this.labelCommitTemplate.Location = new System.Drawing.Point(3, 59);
            this.labelCommitTemplate.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.labelCommitTemplate.Name = "labelCommitTemplate";
            this.labelCommitTemplate.Size = new System.Drawing.Size(87, 13);
            this.labelCommitTemplate.TabIndex = 5;
            this.labelCommitTemplate.Text = "Commit template:";
            // 
            // _NO_TRANSLATE_comboBoxCommitTemplates
            // 
            this._NO_TRANSLATE_comboBoxCommitTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel5.SetColumnSpan(this._NO_TRANSLATE_comboBoxCommitTemplates, 2);
            this._NO_TRANSLATE_comboBoxCommitTemplates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._NO_TRANSLATE_comboBoxCommitTemplates.FormattingEnabled = true;
            this._NO_TRANSLATE_comboBoxCommitTemplates.Location = new System.Drawing.Point(3, 3);
            this._NO_TRANSLATE_comboBoxCommitTemplates.Name = "_NO_TRANSLATE_comboBoxCommitTemplates";
            this._NO_TRANSLATE_comboBoxCommitTemplates.Size = new System.Drawing.Size(672, 21);
            this._NO_TRANSLATE_comboBoxCommitTemplates.TabIndex = 0;
            this._NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndexChanged += new System.EventHandler(this.comboBoxCommitTemplates_SelectedIndexChanged);
            // 
            // labelCommitTemplateName
            // 
            this.labelCommitTemplateName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCommitTemplateName.AutoSize = true;
            this.labelCommitTemplateName.Location = new System.Drawing.Point(3, 33);
            this.labelCommitTemplateName.Name = "labelCommitTemplateName";
            this.labelCommitTemplateName.Size = new System.Drawing.Size(87, 13);
            this.labelCommitTemplateName.TabIndex = 7;
            this.labelCommitTemplateName.Text = "Name:";
            // 
            // _NO_TRANSLATE_textCommitTemplateText
            // 
            this._NO_TRANSLATE_textCommitTemplateText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_textCommitTemplateText.Location = new System.Drawing.Point(96, 56);
            this._NO_TRANSLATE_textCommitTemplateText.Multiline = true;
            this._NO_TRANSLATE_textCommitTemplateText.Name = "_NO_TRANSLATE_textCommitTemplateText";
            this._NO_TRANSLATE_textCommitTemplateText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._NO_TRANSLATE_textCommitTemplateText.Size = new System.Drawing.Size(579, 262);
            this._NO_TRANSLATE_textCommitTemplateText.TabIndex = 2;
            this._NO_TRANSLATE_textCommitTemplateText.TextChanged += new System.EventHandler(this.textCommitTemplateText_TextChanged);
            // 
            // _NO_TRANSLATE_textBoxCommitTemplateName
            // 
            this._NO_TRANSLATE_textBoxCommitTemplateName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_textBoxCommitTemplateName.Location = new System.Drawing.Point(96, 30);
            this._NO_TRANSLATE_textBoxCommitTemplateName.Name = "_NO_TRANSLATE_textBoxCommitTemplateName";
            this._NO_TRANSLATE_textBoxCommitTemplateName.Size = new System.Drawing.Size(579, 20);
            this._NO_TRANSLATE_textBoxCommitTemplateName.TabIndex = 1;
            this._NO_TRANSLATE_textBoxCommitTemplateName.TextChanged += new System.EventHandler(this.textBoxCommitTemplateName_TextChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tableLayoutPanel3);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(684, 327);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Commit validation";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.labelUseIndent, 0, 4);
            this.tableLayoutPanel3.Controls.Add(this.labelMaxLineLength, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.labelMaxFirstLineLength, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this._NO_TRANSLATE_numericMaxLineLength, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this._NO_TRANSLATE_numericMaxFirstLineLength, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.labelRegExCheck, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this._NO_TRANSLATE_textBoxCommitValidationRegex, 1, 3);
            this.tableLayoutPanel3.Controls.Add(this.checkBoxUseIndent, 1, 4);
            this.tableLayoutPanel3.Controls.Add(this.labelSecondLineEmpty, 0, 5);
            this.tableLayoutPanel3.Controls.Add(this.labelAutoWrap, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.checkBoxSecondLineEmpty, 1, 5);
            this.tableLayoutPanel3.Controls.Add(this.checkBoxAutoWrap, 1, 2);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 7;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(686, 142);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // labelUseIndent
            // 
            this.labelUseIndent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelUseIndent.AutoSize = true;
            this.labelUseIndent.Location = new System.Drawing.Point(3, 101);
            this.labelUseIndent.Name = "labelUseIndent";
            this.labelUseIndent.Size = new System.Drawing.Size(126, 13);
            this.labelUseIndent.TabIndex = 11;
            this.labelUseIndent.Text = "Indent lines after first line:";
            // 
            // labelMaxLineLength
            // 
            this.labelMaxLineLength.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelMaxLineLength.AutoSize = true;
            this.labelMaxLineLength.Location = new System.Drawing.Point(3, 32);
            this.labelMaxLineLength.Name = "labelMaxLineLength";
            this.labelMaxLineLength.Size = new System.Drawing.Size(298, 13);
            this.labelMaxLineLength.TabIndex = 4;
            this.labelMaxLineLength.Text = "Maximum numbers of characters per line (0 = check disabled):";
            // 
            // labelMaxFirstLineLength
            // 
            this.labelMaxFirstLineLength.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelMaxFirstLineLength.AutoSize = true;
            this.labelMaxFirstLineLength.Location = new System.Drawing.Point(3, 6);
            this.labelMaxFirstLineLength.Name = "labelMaxFirstLineLength";
            this.labelMaxFirstLineLength.Size = new System.Drawing.Size(310, 13);
            this.labelMaxFirstLineLength.TabIndex = 0;
            this.labelMaxFirstLineLength.Text = "Maximum numbers of characters in first line (0 = check disabled):";
            // 
            // _NO_TRANSLATE_numericMaxLineLength
            // 
            this._NO_TRANSLATE_numericMaxLineLength.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._NO_TRANSLATE_numericMaxLineLength.Location = new System.Drawing.Point(319, 29);
            this._NO_TRANSLATE_numericMaxLineLength.Name = "_NO_TRANSLATE_numericMaxLineLength";
            this._NO_TRANSLATE_numericMaxLineLength.Size = new System.Drawing.Size(60, 20);
            this._NO_TRANSLATE_numericMaxLineLength.TabIndex = 4;
            // 
            // _NO_TRANSLATE_numericMaxFirstLineLength
            // 
            this._NO_TRANSLATE_numericMaxFirstLineLength.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._NO_TRANSLATE_numericMaxFirstLineLength.Location = new System.Drawing.Point(319, 3);
            this._NO_TRANSLATE_numericMaxFirstLineLength.Name = "_NO_TRANSLATE_numericMaxFirstLineLength";
            this._NO_TRANSLATE_numericMaxFirstLineLength.Size = new System.Drawing.Size(60, 20);
            this._NO_TRANSLATE_numericMaxFirstLineLength.TabIndex = 3;
            // 
            // labelRegExCheck
            // 
            this.labelRegExCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelRegExCheck.AutoSize = true;
            this.labelRegExCheck.Location = new System.Drawing.Point(3, 78);
            this.labelRegExCheck.Name = "labelRegExCheck";
            this.labelRegExCheck.Size = new System.Drawing.Size(302, 13);
            this.labelRegExCheck.TabIndex = 7;
            this.labelRegExCheck.Text = "Commit must match following RegEx (Empty = check disabled):";
            // 
            // _NO_TRANSLATE_textBoxCommitValidationRegex
            // 
            this._NO_TRANSLATE_textBoxCommitValidationRegex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_textBoxCommitValidationRegex.Location = new System.Drawing.Point(319, 75);
            this._NO_TRANSLATE_textBoxCommitValidationRegex.Name = "_NO_TRANSLATE_textBoxCommitValidationRegex";
            this._NO_TRANSLATE_textBoxCommitValidationRegex.Size = new System.Drawing.Size(364, 20);
            this._NO_TRANSLATE_textBoxCommitValidationRegex.TabIndex = 8;
            // 
            // checkBoxUseIndent
            // 
            this.checkBoxUseIndent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkBoxUseIndent.AutoSize = true;
            this.checkBoxUseIndent.Location = new System.Drawing.Point(319, 101);
            this.checkBoxUseIndent.Name = "checkBoxUseIndent";
            this.checkBoxUseIndent.Size = new System.Drawing.Size(15, 14);
            this.checkBoxUseIndent.TabIndex = 10;
            this.checkBoxUseIndent.UseVisualStyleBackColor = true;
            // 
            // labelSecondLineEmpty
            // 
            this.labelSecondLineEmpty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSecondLineEmpty.AutoSize = true;
            this.labelSecondLineEmpty.Location = new System.Drawing.Point(3, 121);
            this.labelSecondLineEmpty.Name = "labelSecondLineEmpty";
            this.labelSecondLineEmpty.Size = new System.Drawing.Size(137, 13);
            this.labelSecondLineEmpty.TabIndex = 6;
            this.labelSecondLineEmpty.Text = "Second line must be empty:";
            // 
            // labelAutoWrap
            // 
            this.labelAutoWrap.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelAutoWrap.AutoSize = true;
            this.labelAutoWrap.Location = new System.Drawing.Point(3, 55);
            this.labelAutoWrap.Name = "labelAutoWrap";
            this.labelAutoWrap.Size = new System.Drawing.Size(233, 13);
            this.labelAutoWrap.TabIndex = 13;
            this.labelAutoWrap.Text = "Auto-wrap commit message (except subject line)";
            // 
            // checkBoxSecondLineEmpty
            // 
            this.checkBoxSecondLineEmpty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkBoxSecondLineEmpty.AutoSize = true;
            this.checkBoxSecondLineEmpty.Location = new System.Drawing.Point(319, 121);
            this.checkBoxSecondLineEmpty.Name = "checkBoxSecondLineEmpty";
            this.checkBoxSecondLineEmpty.Size = new System.Drawing.Size(15, 14);
            this.checkBoxSecondLineEmpty.TabIndex = 5;
            this.checkBoxSecondLineEmpty.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoWrap
            // 
            this.checkBoxAutoWrap.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkBoxAutoWrap.AutoSize = true;
            this.checkBoxAutoWrap.Location = new System.Drawing.Point(319, 55);
            this.checkBoxAutoWrap.Name = "checkBoxAutoWrap";
            this.checkBoxAutoWrap.Size = new System.Drawing.Size(15, 14);
            this.checkBoxAutoWrap.TabIndex = 14;
            this.checkBoxAutoWrap.UseVisualStyleBackColor = true;
            // 
            // FormCommitTemplateSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(698, 394);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(550, 400);
            this.Name = "FormCommitTemplateSettings";
            this.Text = "Commit message settings";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_numericMaxLineLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_numericMaxFirstLineLength)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_numericMaxLineLength;
        private System.Windows.Forms.Label labelMaxFirstLineLength;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_numericMaxFirstLineLength;
        private System.Windows.Forms.Label labelMaxLineLength;
        private System.Windows.Forms.Label labelSecondLineEmpty;
        private System.Windows.Forms.CheckBox checkBoxSecondLineEmpty;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Label labelCommitTemplate;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_textCommitTemplateText;
        private System.Windows.Forms.ComboBox _NO_TRANSLATE_comboBoxCommitTemplates;
        private System.Windows.Forms.Label labelCommitTemplateName;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_textBoxCommitTemplateName;
        private System.Windows.Forms.Label labelRegExCheck;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_textBoxCommitValidationRegex;
        private System.Windows.Forms.Label labelUseIndent;
        private System.Windows.Forms.CheckBox checkBoxUseIndent;
        private System.Windows.Forms.Label labelAutoWrap;
        private System.Windows.Forms.CheckBox checkBoxAutoWrap;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
    }
}