using System.Windows.Forms;
namespace TranslationApp
{
    partial class FormTranslate
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
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip1 = new GitUI.ToolStripEx();
            this.toolStripButtonNew = new System.Windows.Forms.ToolStripButton();
            this.saveAs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTranslationLabel = new System.Windows.Forms.ToolStripLabel();
            this.translations = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripTranslationLanguageCode = new System.Windows.Forms.ToolStripLabel();
            this._NO_TRANSLATE_languageCode = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.translateProgress = new System.Windows.Forms.ToolStripLabel();
            this.hideTranslatedItems = new System.Windows.Forms.ToolStripButton();
            this._toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.translateCategories = new System.Windows.Forms.ListBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.translateGrid = new System.Windows.Forms.DataGridView();
            this.categoryDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.propertyDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.neutralValueDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.translatedValueDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.translateItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.neutralText = new System.Windows.Forms.RichTextBox();
            this.translatedText = new GitUI.SpellChecker.EditNetSpell();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.googleTranslate = new System.Windows.Forms.Button();
            this.googleAll = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.previousButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.tableLayoutPanel1.SuspendLayout();
            this.toolStrip1.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();

            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();

            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.translateGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.translateItemBindingSource)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();

            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.toolStrip1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(936, 590);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ClickThrough = true;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonNew,
            this.saveAs,
            this.toolStripSeparator2,
            this.toolStripTranslationLabel,
            this.translations,
            this.toolStripTranslationLanguageCode,
            this._NO_TRANSLATE_languageCode,
            this.toolStripSeparator1,
            this.translateProgress,
            this.hideTranslatedItems,
            this._toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(936, 38);
            this.toolStrip1.TabIndex = 1;
            // 
            // toolStripButtonNew
            // 
            this.toolStripButtonNew.Image = global::TranslationApp.Properties.Resources._21;
            this.toolStripButtonNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonNew.Name = "toolStripButtonNew";
            this.toolStripButtonNew.Size = new System.Drawing.Size(59, 35);
            this.toolStripButtonNew.Text = "New";
            this.toolStripButtonNew.ToolTipText = "Create new translation";
            this.toolStripButtonNew.Click += new System.EventHandler(this.toolStripButtonNew_Click);
            // 
            // saveAs
            // 
            this.saveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveAs.Image = global::TranslationApp.Properties.Resources.IconSaveAs;
            this.saveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveAs.Name = "saveAs";
            this.saveAs.Size = new System.Drawing.Size(23, 35);
            this.saveAs.ToolTipText = "Save translation";
            this.saveAs.Click += new System.EventHandler(this.saveAs_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripTranslationLabel
            // 
            this.toolStripTranslationLabel.Name = "toolStripTranslationLabel";
            this.toolStripTranslationLabel.Size = new System.Drawing.Size(134, 35);
            this.toolStripTranslationLabel.Text = "Current translation:";
            // 
            // translations
            // 
            this.translations.Items.AddRange(new object[] {
            "nl",
            "ja"});
            this.translations.Name = "translations";
            this.translations.Size = new System.Drawing.Size(150, 38);
            this.translations.SelectedIndexChanged += new System.EventHandler(this.translations_SelectedIndexChanged);
            // 
            // toolStripTranslationLanguageCode
            // 
            this.toolStripTranslationLanguageCode.Name = "toolStripTranslationLanguageCode";
            this.toolStripTranslationLanguageCode.Size = new System.Drawing.Size(114, 35);
            this.toolStripTranslationLanguageCode.Text = "Language code:";
            // 
            // _NO_TRANSLATE_languageCode
            // 
            this._NO_TRANSLATE_languageCode.Name = "_NO_TRANSLATE_languageCode";
            this._NO_TRANSLATE_languageCode.Size = new System.Drawing.Size(150, 38);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 38);
            // 
            // translateProgress
            // 
            this.translateProgress.Image = global::TranslationApp.Properties.Resources._53;
            this.translateProgress.Name = "translateProgress";
            this.translateProgress.Size = new System.Drawing.Size(16, 35);
            // 
            // hideTranslatedItems
            // 
            this.hideTranslatedItems.CheckOnClick = true;
            this.hideTranslatedItems.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.hideTranslatedItems.Image = global::TranslationApp.Properties.Resources.IconClean;
            this.hideTranslatedItems.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.hideTranslatedItems.Name = "hideTranslatedItems";
            this.hideTranslatedItems.Size = new System.Drawing.Size(23, 35);
            this.hideTranslatedItems.CheckedChanged += new System.EventHandler(this.hideTranslatedItems_CheckedChanged);
            // 
            // _toolStripButton1
            // 
            this._toolStripButton1.Checked = true;
            this._toolStripButton1.CheckOnClick = true;
            this._toolStripButton1.CheckState = System.Windows.Forms.CheckState.Checked;
            this._toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._toolStripButton1.Image = global::TranslationApp.Properties.Resources._26;
            this._toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripButton1.Name = "_toolStripButton1";
            this._toolStripButton1.Size = new System.Drawing.Size(23, 35);
            this._toolStripButton1.CheckedChanged += new System.EventHandler(this.toolStripButton1_CheckedChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(4, 42);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.translateCategories);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(928, 545);
            this.splitContainer1.SplitterDistance = 151;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 2;
            // 
            // translateCategories
            // 
            this.translateCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.translateCategories.FormattingEnabled = true;
            this.translateCategories.ItemHeight = 23;
            this.translateCategories.Location = new System.Drawing.Point(0, 0);
            this.translateCategories.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.translateCategories.Name = "translateCategories";
            this.translateCategories.Size = new System.Drawing.Size(151, 545);
            this.translateCategories.TabIndex = 0;
            this.translateCategories.SelectedIndexChanged += new System.EventHandler(this.translateCategories_SelectedIndexChanged);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.translateGrid);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tableLayoutPanel2);
            this.splitContainer2.Size = new System.Drawing.Size(772, 545);
            this.splitContainer2.SplitterDistance = 309;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 2;
            // 
            // translateGrid
            // 
            this.translateGrid.AllowUserToAddRows = false;
            this.translateGrid.AllowUserToDeleteRows = false;
            this.translateGrid.AutoGenerateColumns = false;
            this.translateGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.translateGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.categoryDataGridViewTextBoxColumn,
            this.nameDataGridViewTextBoxColumn,
            this.propertyDataGridViewTextBoxColumn,
            this.neutralValueDataGridViewTextBoxColumn,
            this.translatedValueDataGridViewTextBoxColumn});
            this.translateGrid.DataSource = this.translateItemBindingSource;
            this.translateGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.translateGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.translateGrid.Location = new System.Drawing.Point(0, 0);
            this.translateGrid.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.translateGrid.MultiSelect = false;
            this.translateGrid.Name = "translateGrid";
            this.translateGrid.RowHeadersVisible = false;
            this.translateGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.translateGrid.Size = new System.Drawing.Size(772, 309);
            this.translateGrid.TabIndex = 1;
            this.translateGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.translateGrid_CellEndEdit);
            this.translateGrid.SelectionChanged += new System.EventHandler(this.translateGrid_SelectionChanged);
            this.translateGrid.Click += new System.EventHandler(this.translateGrid_Click);
            // 
            // categoryDataGridViewTextBoxColumn
            // 
            this.categoryDataGridViewTextBoxColumn.HeaderText = "Category";
            this.categoryDataGridViewTextBoxColumn.Name = "categoryDataGridViewTextBoxColumn";
            this.categoryDataGridViewTextBoxColumn.ReadOnly = true;
            this.categoryDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.HeaderText = "Name";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            this.nameDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // propertyDataGridViewTextBoxColumn
            // 
            this.propertyDataGridViewTextBoxColumn.HeaderText = "Property";
            this.propertyDataGridViewTextBoxColumn.Name = "propertyDataGridViewTextBoxColumn";
            this.propertyDataGridViewTextBoxColumn.ReadOnly = true;
            this.propertyDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // neutralValueDataGridViewTextBoxColumn
            // 
            this.neutralValueDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.neutralValueDataGridViewTextBoxColumn.HeaderText = "NeutralValue";
            this.neutralValueDataGridViewTextBoxColumn.Name = "neutralValueDataGridViewTextBoxColumn";
            this.neutralValueDataGridViewTextBoxColumn.ReadOnly = true;
            this.neutralValueDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // translatedValueDataGridViewTextBoxColumn
            // 
            this.translatedValueDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.translatedValueDataGridViewTextBoxColumn.HeaderText = "TranslatedValue";
            this.translatedValueDataGridViewTextBoxColumn.Name = "translatedValueDataGridViewTextBoxColumn";
            this.translatedValueDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // translateItemBindingSource
            // 
            this.translateItemBindingSource.DataSource = typeof(TranslationApp.TranslationItemWithCategory);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.splitContainer4, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(772, 231);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(4, 4);
            this.splitContainer4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.neutralText);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.translatedText);
            this.splitContainer4.Size = new System.Drawing.Size(556, 223);
            this.splitContainer4.SplitterDistance = 101;
            this.splitContainer4.SplitterWidth = 5;
            this.splitContainer4.TabIndex = 0;
            // 
            // neutralText
            // 
            this.neutralText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.neutralText.Location = new System.Drawing.Point(0, 0);
            this.neutralText.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.neutralText.Name = "neutralText";
            this.neutralText.ReadOnly = true;
            this.neutralText.Size = new System.Drawing.Size(556, 101);
            this.neutralText.TabIndex = 0;
            this.neutralText.Text = "";
            // 
            // translatedText
            // 
            this.translatedText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.translatedText.Location = new System.Drawing.Point(0, 0);
            this.translatedText.Margin = new System.Windows.Forms.Padding(0);
            this.translatedText.Name = "translatedText";
            this.translatedText.Size = new System.Drawing.Size(556, 117);
            this.translatedText.TabIndex = 14;
            this.translatedText.TextChanged += new System.EventHandler(this.translatedText_TextChanged);
            this.translatedText.Enter += new System.EventHandler(this.translatedText_Enter);
            this.translatedText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.translatedText_KeyDown);
            this.translatedText.Leave += new System.EventHandler(this.translatedText_Leave);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.googleTranslate, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this.googleAll, 0, 4);
            this.tableLayoutPanel3.Controls.Add(this.nextButton, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.previousButton, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.progressBar, 0, 5);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(566, 2);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 6;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(204, 227);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // nextButton
            // 
            this.nextButton.AutoSize = true;
            this.nextButton.Location = new System.Drawing.Point(4, 4);
            this.nextButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(196, 34);
            this.nextButton.TabIndex = 0;
            this.nextButton.Text = "&Next";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // previousButton
            // 
            this.previousButton.AutoSize = true;
            this.previousButton.Location = new System.Drawing.Point(4, 46);
            this.previousButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.previousButton.Name = "previousButton";
            this.previousButton.Size = new System.Drawing.Size(196, 34);
            this.previousButton.TabIndex = 1;
            this.previousButton.Text = "&Previous";
            this.previousButton.UseVisualStyleBackColor = true;
            this.previousButton.Click += new System.EventHandler(this.previousButton_Click);
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(3, 201);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(198, 23);
            this.progressBar.TabIndex = 5;
            this.progressBar.Visible = false;
            // 
            // FormTranslate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(936, 590);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(846, 623);
            this.Name = "FormTranslate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Translate";
            this.Load += new System.EventHandler(this.FormTranslate_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();

            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();

            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.translateGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.translateItemBindingSource)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();

            this.splitContainer4.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private GitUI.ToolStripEx toolStrip1;
        private System.Windows.Forms.ToolStripLabel translateProgress;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox translateCategories;
        private System.Windows.Forms.DataGridView translateGrid;
        private System.Windows.Forms.ToolStripButton saveAs;
        private System.Windows.Forms.ToolStripComboBox translations;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton hideTranslatedItems;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.RichTextBox neutralText;
        private System.Windows.Forms.ToolStripButton _toolStripButton1;
        private System.Windows.Forms.Button previousButton;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.BindingSource translateItemBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn categoryDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn propertyDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn neutralValueDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn translatedValueDataGridViewTextBoxColumn;
        private System.Windows.Forms.Button googleTranslate;
        private System.Windows.Forms.ToolStripLabel toolStripTranslationLabel;
        private System.Windows.Forms.ToolStripLabel toolStripTranslationLanguageCode;
        private System.Windows.Forms.ToolStripComboBox _NO_TRANSLATE_languageCode;
        private System.Windows.Forms.Button googleAll;
        private System.Windows.Forms.ToolStripButton toolStripButtonNew;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private GitUI.SpellChecker.EditNetSpell translatedText;
        private ProgressBar progressBar;

    }
}