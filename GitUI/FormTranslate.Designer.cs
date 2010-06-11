namespace GitUI
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.saveAs = new System.Windows.Forms.ToolStripButton();
            this.translations = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.translateProgress = new System.Windows.Forms.ToolStripLabel();
            this.hideTranslatedItems = new System.Windows.Forms.ToolStripButton();
            this._toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.translateCategories = new System.Windows.Forms.ListBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.translateGrid = new System.Windows.Forms.DataGridView();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.neutralTekst = new System.Windows.Forms.RichTextBox();
            this.translatedText = new System.Windows.Forms.RichTextBox();
            this.previousButton = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.translateGrid)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
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
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(623, 415);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAs,
            this.translations,
            this.toolStripSeparator1,
            this.translateProgress,
            this.hideTranslatedItems,
            this._toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(623, 30);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // saveAs
            // 
            this.saveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveAs.Image = global::GitUI.Properties.Resources._46;
            this.saveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveAs.Name = "saveAs";
            this.saveAs.Size = new System.Drawing.Size(23, 27);
            this.saveAs.Click += new System.EventHandler(this.saveAs_Click);
            // 
            // translations
            // 
            this.translations.Items.AddRange(new object[] {
            "nl",
            "ja"});
            this.translations.Name = "translations";
            this.translations.Size = new System.Drawing.Size(121, 30);
            this.translations.SelectedIndexChanged += new System.EventHandler(this.translations_SelectedIndexChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 30);
            // 
            // translateProgress
            // 
            this.translateProgress.Image = global::GitUI.Properties.Resources._53;
            this.translateProgress.Name = "translateProgress";
            this.translateProgress.Size = new System.Drawing.Size(16, 27);
            // 
            // hideTranslatedItems
            // 
            this.hideTranslatedItems.CheckOnClick = true;
            this.hideTranslatedItems.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.hideTranslatedItems.Image = global::GitUI.Properties.Resources._10;
            this.hideTranslatedItems.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.hideTranslatedItems.Name = "hideTranslatedItems";
            this.hideTranslatedItems.Size = new System.Drawing.Size(23, 27);
            this.hideTranslatedItems.CheckedChanged += new System.EventHandler(this.hideTranslatedItems_CheckedChanged);
            // 
            // _toolStripButton1
            // 
            this._toolStripButton1.Checked = true;
            this._toolStripButton1.CheckOnClick = true;
            this._toolStripButton1.CheckState = System.Windows.Forms.CheckState.Checked;
            this._toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._toolStripButton1.Image = global::GitUI.Properties.Resources._26;
            this._toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripButton1.Name = "_toolStripButton1";
            this._toolStripButton1.Size = new System.Drawing.Size(23, 27);
            this._toolStripButton1.Text = "_showDetails";
            this._toolStripButton1.CheckedChanged += new System.EventHandler(this.toolStripButton1_CheckedChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(3, 33);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.translateCategories);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(617, 379);
            this.splitContainer1.SplitterDistance = 151;
            this.splitContainer1.TabIndex = 2;
            // 
            // translateCategories
            // 
            this.translateCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.translateCategories.FormattingEnabled = true;
            this.translateCategories.Location = new System.Drawing.Point(0, 0);
            this.translateCategories.Name = "translateCategories";
            this.translateCategories.Size = new System.Drawing.Size(151, 368);
            this.translateCategories.TabIndex = 0;
            this.translateCategories.SelectedIndexChanged += new System.EventHandler(this.translateCategories_SelectedIndexChanged);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.translateGrid);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(462, 379);
            this.splitContainer2.SplitterDistance = 222;
            this.splitContainer2.TabIndex = 2;
            // 
            // translateGrid
            // 
            this.translateGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.translateGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.translateGrid.Location = new System.Drawing.Point(0, 0);
            this.translateGrid.MultiSelect = false;
            this.translateGrid.Name = "translateGrid";
            this.translateGrid.RowHeadersVisible = false;
            this.translateGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.translateGrid.Size = new System.Drawing.Size(462, 222);
            this.translateGrid.TabIndex = 1;
            this.translateGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.translateGrid_CellEndEdit);
            this.translateGrid.SelectionChanged += new System.EventHandler(this.translateGrid_SelectionChanged);
            this.translateGrid.Click += new System.EventHandler(this.translateGrid_Click);
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.splitContainer4);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.previousButton);
            this.splitContainer3.Panel2.Controls.Add(this.nextButton);
            this.splitContainer3.Size = new System.Drawing.Size(462, 153);
            this.splitContainer3.SplitterDistance = 354;
            this.splitContainer3.TabIndex = 0;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.neutralTekst);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.translatedText);
            this.splitContainer4.Size = new System.Drawing.Size(354, 153);
            this.splitContainer4.SplitterDistance = 72;
            this.splitContainer4.TabIndex = 0;
            // 
            // neutralTekst
            // 
            this.neutralTekst.Dock = System.Windows.Forms.DockStyle.Fill;
            this.neutralTekst.Location = new System.Drawing.Point(0, 0);
            this.neutralTekst.Name = "neutralTekst";
            this.neutralTekst.ReadOnly = true;
            this.neutralTekst.Size = new System.Drawing.Size(354, 72);
            this.neutralTekst.TabIndex = 0;
            this.neutralTekst.Text = "";
            // 
            // translatedText
            // 
            this.translatedText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.translatedText.Location = new System.Drawing.Point(0, 0);
            this.translatedText.Name = "translatedText";
            this.translatedText.Size = new System.Drawing.Size(354, 77);
            this.translatedText.TabIndex = 0;
            this.translatedText.Text = "";
            this.translatedText.TextChanged += new System.EventHandler(this.translatedText_TextChanged);
            // 
            // previousButton
            // 
            this.previousButton.Location = new System.Drawing.Point(3, 32);
            this.previousButton.Name = "previousButton";
            this.previousButton.Size = new System.Drawing.Size(99, 23);
            this.previousButton.TabIndex = 1;
            this.previousButton.Text = "&Previous";
            this.previousButton.UseVisualStyleBackColor = true;
            this.previousButton.Click += new System.EventHandler(this.previousButton_Click);
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(2, 3);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(99, 23);
            this.nextButton.TabIndex = 0;
            this.nextButton.Text = "&Next";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // FormTranslate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(623, 415);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FormTranslate";
            this.Text = "FormTranslate";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.translateGrid)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel translateProgress;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox translateCategories;
        private System.Windows.Forms.DataGridView translateGrid;
        private System.Windows.Forms.ToolStripButton saveAs;
        private System.Windows.Forms.ToolStripComboBox translations;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton hideTranslatedItems;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.RichTextBox neutralTekst;
        private System.Windows.Forms.RichTextBox translatedText;
        private System.Windows.Forms.ToolStripButton _toolStripButton1;
        private System.Windows.Forms.Button previousButton;
        private System.Windows.Forms.Button nextButton;

    }
}