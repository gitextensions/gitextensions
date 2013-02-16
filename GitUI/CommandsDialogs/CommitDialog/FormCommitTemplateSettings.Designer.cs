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
            this.groupBoxCommitValidation = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.labelUseIndent = new System.Windows.Forms.Label();
            this.labelMaxLineLength = new System.Windows.Forms.Label();
            this.labelMaxFirstLineLength = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_numericMaxLineLength = new System.Windows.Forms.NumericUpDown();
            this._NO_TRANSLATE_numericMaxFirstLineLength = new System.Windows.Forms.NumericUpDown();
            this.labelSecondLineEmpty = new System.Windows.Forms.Label();
            this.checkBoxSecondLineEmpty = new System.Windows.Forms.CheckBox();
            this.labelRegExCheck = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_textBoxCommitValidationRegex = new System.Windows.Forms.TextBox();
            this.checkBoxUseIndent = new System.Windows.Forms.CheckBox();
            this.groupBoxCommitTemplates = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.labelCommitTemplate = new System.Windows.Forms.Label();
            this.labelCommitTemplateName = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_textCommitTemplateText = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_textBoxCommitTemplateName = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_comboBoxCommitTemplates = new System.Windows.Forms.ComboBox();
            this.labelAutoWrap = new System.Windows.Forms.Label();
            this.checkBoxAutoWrap = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBoxCommitValidation.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_numericMaxLineLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_numericMaxFirstLineLength)).BeginInit();
            this.groupBoxCommitTemplates.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxCommitValidation, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxCommitTemplates, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(698, 394);
            this.tableLayoutPanel1.TabIndex = 60;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.buttonOk, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonCancel, 2, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 361);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(692, 30);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(533, 3);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 6;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(614, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBoxCommitValidation
            // 
            this.groupBoxCommitValidation.Controls.Add(this.tableLayoutPanel3);
            this.groupBoxCommitValidation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxCommitValidation.Location = new System.Drawing.Point(3, 182);
            this.groupBoxCommitValidation.Name = "groupBoxCommitValidation";
            this.groupBoxCommitValidation.Size = new System.Drawing.Size(692, 173);
            this.groupBoxCommitValidation.TabIndex = 1;
            this.groupBoxCommitValidation.TabStop = false;
            this.groupBoxCommitValidation.Text = "Commit validation";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
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
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 19);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 6;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(686, 151);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // labelUseIndent
            //
            this.labelUseIndent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelUseIndent.AutoSize = true;
            this.labelUseIndent.Location = new System.Drawing.Point(3, 109);
            this.labelUseIndent.Name = "labelUseIndent";
            this.labelUseIndent.Size = new System.Drawing.Size(143, 15);
            this.labelUseIndent.TabIndex = 11;
            this.labelUseIndent.Text = "Indent lines after first line:";
            //
            // labelMaxLineLength
            // 
            this.labelMaxLineLength.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelMaxLineLength.AutoSize = true;
            this.labelMaxLineLength.Location = new System.Drawing.Point(3, 36);
            this.labelMaxLineLength.Name = "labelMaxLineLength";
            this.labelMaxLineLength.Size = new System.Drawing.Size(336, 15);
            this.labelMaxLineLength.TabIndex = 4;
            this.labelMaxLineLength.Text = "Maximum numbers of characters per line (0 = check disabled):";
            // 
            // labelMaxFirstLineLength
            // 
            this.labelMaxFirstLineLength.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelMaxFirstLineLength.AutoSize = true;
            this.labelMaxFirstLineLength.Location = new System.Drawing.Point(3, 7);
            this.labelMaxFirstLineLength.Name = "labelMaxFirstLineLength";
            this.labelMaxFirstLineLength.Size = new System.Drawing.Size(352, 15);
            this.labelMaxFirstLineLength.TabIndex = 0;
            this.labelMaxFirstLineLength.Text = "Maximum numbers of characters in first line (0 = check disabled):";
            // 
            // _NO_TRANSLATE_numericMaxLineLength
            // 
            this._NO_TRANSLATE_numericMaxLineLength.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this._NO_TRANSLATE_numericMaxLineLength.Location = new System.Drawing.Point(623, 32);
            this._NO_TRANSLATE_numericMaxLineLength.Name = "_NO_TRANSLATE_numericMaxLineLength";
            this._NO_TRANSLATE_numericMaxLineLength.Size = new System.Drawing.Size(60, 23);
            this._NO_TRANSLATE_numericMaxLineLength.TabIndex = 4;
            // 
            // _NO_TRANSLATE_numericMaxFirstLineLength
            // 
            this._NO_TRANSLATE_numericMaxFirstLineLength.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this._NO_TRANSLATE_numericMaxFirstLineLength.Location = new System.Drawing.Point(623, 3);
            this._NO_TRANSLATE_numericMaxFirstLineLength.Name = "_NO_TRANSLATE_numericMaxFirstLineLength";
            this._NO_TRANSLATE_numericMaxFirstLineLength.Size = new System.Drawing.Size(60, 23);
            this._NO_TRANSLATE_numericMaxFirstLineLength.TabIndex = 3;
            // 
            // labelSecondLineEmpty
            // 
            this.labelSecondLineEmpty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSecondLineEmpty.AutoSize = true;
            this.labelSecondLineEmpty.Location = new System.Drawing.Point(3, 131);
            this.labelSecondLineEmpty.Name = "labelSecondLineEmpty";
            this.labelSecondLineEmpty.Size = new System.Drawing.Size(154, 15);
            this.labelSecondLineEmpty.TabIndex = 6;
            this.labelSecondLineEmpty.Text = "Second line must be empty:";
            // 
            // labelRegExCheck
            // 
            this.labelRegExCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelRegExCheck.AutoSize = true;
            this.labelRegExCheck.Location = new System.Drawing.Point(3, 85);
            this.labelRegExCheck.Name = "labelRegExCheck";
            this.labelRegExCheck.Size = new System.Drawing.Size(345, 15);
            this.labelRegExCheck.TabIndex = 7;
            this.labelRegExCheck.Text = "Commit must match following RegEx (Empty = check disabled):";
            // 
            // _NO_TRANSLATE_textBoxCommitValidationRegex
            // 
            this._NO_TRANSLATE_textBoxCommitValidationRegex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_textBoxCommitValidationRegex.Location = new System.Drawing.Point(450, 81);
            this._NO_TRANSLATE_textBoxCommitValidationRegex.Name = "_NO_TRANSLATE_textBoxCommitValidationRegex";
            this._NO_TRANSLATE_textBoxCommitValidationRegex.Size = new System.Drawing.Size(233, 23);
            this._NO_TRANSLATE_textBoxCommitValidationRegex.TabIndex = 8;
            // 
            // checkBoxUseIndent
            //
            this.checkBoxUseIndent.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.checkBoxUseIndent.AutoSize = true;
            this.checkBoxUseIndent.Location = new System.Drawing.Point(668, 110);
            this.checkBoxUseIndent.Name = "checkBoxUseIndent";
            this.checkBoxUseIndent.Size = new System.Drawing.Size(15, 14);
            this.checkBoxUseIndent.TabIndex = 10;
            this.checkBoxUseIndent.UseVisualStyleBackColor = true;
            //
            // groupBoxCommitTemplates
            // 
            this.groupBoxCommitTemplates.Controls.Add(this.tableLayoutPanel4);
            this.groupBoxCommitTemplates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxCommitTemplates.Location = new System.Drawing.Point(3, 3);
            this.groupBoxCommitTemplates.Name = "groupBoxCommitTemplates";
            this.groupBoxCommitTemplates.Size = new System.Drawing.Size(692, 173);
            this.groupBoxCommitTemplates.TabIndex = 0;
            this.groupBoxCommitTemplates.TabStop = false;
            this.groupBoxCommitTemplates.Text = "Commit templates";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel5, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this._NO_TRANSLATE_comboBoxCommitTemplates, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 19);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(686, 151);
            this.tableLayoutPanel4.TabIndex = 3;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.labelCommitTemplate, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.labelCommitTemplateName, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this._NO_TRANSLATE_textCommitTemplateText, 1, 1);
            this.tableLayoutPanel5.Controls.Add(this._NO_TRANSLATE_textBoxCommitTemplateName, 1, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(130, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(553, 145);
            this.tableLayoutPanel5.TabIndex = 1;
            // 
            // labelCommitTemplate
            // 
            this.labelCommitTemplate.AutoSize = true;
            this.labelCommitTemplate.Location = new System.Drawing.Point(3, 35);
            this.labelCommitTemplate.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.labelCommitTemplate.Name = "labelCommitTemplate";
            this.labelCommitTemplate.Size = new System.Drawing.Size(104, 15);
            this.labelCommitTemplate.TabIndex = 5;
            this.labelCommitTemplate.Text = "Commit template:";
            // 
            // labelCommitTemplateName
            // 
            this.labelCommitTemplateName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCommitTemplateName.AutoSize = true;
            this.labelCommitTemplateName.Location = new System.Drawing.Point(3, 7);
            this.labelCommitTemplateName.Name = "labelCommitTemplateName";
            this.labelCommitTemplateName.Size = new System.Drawing.Size(104, 15);
            this.labelCommitTemplateName.TabIndex = 7;
            this.labelCommitTemplateName.Text = "Name:";
            // 
            // _NO_TRANSLATE_textCommitTemplateText
            // 
            this._NO_TRANSLATE_textCommitTemplateText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_textCommitTemplateText.Location = new System.Drawing.Point(113, 32);
            this._NO_TRANSLATE_textCommitTemplateText.Multiline = true;
            this._NO_TRANSLATE_textCommitTemplateText.Name = "_NO_TRANSLATE_textCommitTemplateText";
            this._NO_TRANSLATE_textCommitTemplateText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._NO_TRANSLATE_textCommitTemplateText.Size = new System.Drawing.Size(437, 110);
            this._NO_TRANSLATE_textCommitTemplateText.TabIndex = 2;
            this._NO_TRANSLATE_textCommitTemplateText.TextChanged += new System.EventHandler(this.textCommitTemplateText_TextChanged);
            // 
            // _NO_TRANSLATE_textBoxCommitTemplateName
            // 
            this._NO_TRANSLATE_textBoxCommitTemplateName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_textBoxCommitTemplateName.Location = new System.Drawing.Point(113, 3);
            this._NO_TRANSLATE_textBoxCommitTemplateName.Name = "_NO_TRANSLATE_textBoxCommitTemplateName";
            this._NO_TRANSLATE_textBoxCommitTemplateName.Size = new System.Drawing.Size(437, 23);
            this._NO_TRANSLATE_textBoxCommitTemplateName.TabIndex = 1;
            this._NO_TRANSLATE_textBoxCommitTemplateName.TextChanged += new System.EventHandler(this.textBoxCommitTemplateName_TextChanged);
            // 
            // _NO_TRANSLATE_comboBoxCommitTemplates
            // 
            this._NO_TRANSLATE_comboBoxCommitTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_comboBoxCommitTemplates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._NO_TRANSLATE_comboBoxCommitTemplates.FormattingEnabled = true;
            this._NO_TRANSLATE_comboBoxCommitTemplates.Location = new System.Drawing.Point(3, 3);
            this._NO_TRANSLATE_comboBoxCommitTemplates.Name = "_NO_TRANSLATE_comboBoxCommitTemplates";
            this._NO_TRANSLATE_comboBoxCommitTemplates.Size = new System.Drawing.Size(121, 23);
            this._NO_TRANSLATE_comboBoxCommitTemplates.TabIndex = 0;
            this._NO_TRANSLATE_comboBoxCommitTemplates.SelectedIndexChanged += new System.EventHandler(this.comboBoxCommitTemplates_SelectedIndexChanged);
            // 
            // labelAutoWrap
            // 
            this.labelAutoWrap.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelAutoWrap.AutoSize = true;
            this.labelAutoWrap.Location = new System.Drawing.Point(3, 60);
            this.labelAutoWrap.Name = "labelAutoWrap";
            this.labelAutoWrap.Size = new System.Drawing.Size(266, 15);
            this.labelAutoWrap.TabIndex = 13;
            this.labelAutoWrap.Text = "Auto-wrap commit message (except subject line)";
            // 
            // checkBoxSecondLineEmpty
            // 
            this.checkBoxSecondLineEmpty.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.checkBoxSecondLineEmpty.AutoSize = true;
            this.checkBoxSecondLineEmpty.Location = new System.Drawing.Point(668, 132);
            this.checkBoxSecondLineEmpty.Name = "checkBoxSecondLineEmpty";
            this.checkBoxSecondLineEmpty.Size = new System.Drawing.Size(15, 14);
            this.checkBoxSecondLineEmpty.TabIndex = 5;
            this.checkBoxSecondLineEmpty.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoWrap
            // 
            this.checkBoxAutoWrap.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.checkBoxAutoWrap.AutoSize = true;
            this.checkBoxAutoWrap.Location = new System.Drawing.Point(668, 61);
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
            this.Text = "Commit template settings";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBoxCommitValidation.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_numericMaxLineLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._NO_TRANSLATE_numericMaxFirstLineLength)).EndInit();
            this.groupBoxCommitTemplates.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxCommitValidation;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_numericMaxLineLength;
        private System.Windows.Forms.Label labelMaxFirstLineLength;
        private System.Windows.Forms.NumericUpDown _NO_TRANSLATE_numericMaxFirstLineLength;
        private System.Windows.Forms.Label labelMaxLineLength;
        private System.Windows.Forms.Label labelSecondLineEmpty;
        private System.Windows.Forms.CheckBox checkBoxSecondLineEmpty;
        private System.Windows.Forms.GroupBox groupBoxCommitTemplates;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
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
    }
}