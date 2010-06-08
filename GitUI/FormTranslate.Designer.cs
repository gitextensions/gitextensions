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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.translateCategories = new System.Windows.Forms.ListBox();
            this.translateGrid = new System.Windows.Forms.DataGridView();
            this.hideTranslatedItems = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.translateGrid)).BeginInit();
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
            this.hideTranslatedItems});
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
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 33);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.translateCategories);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.translateGrid);
            this.splitContainer1.Size = new System.Drawing.Size(617, 379);
            this.splitContainer1.SplitterDistance = 205;
            this.splitContainer1.TabIndex = 2;
            // 
            // translateCategories
            // 
            this.translateCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.translateCategories.FormattingEnabled = true;
            this.translateCategories.Location = new System.Drawing.Point(0, 0);
            this.translateCategories.Name = "translateCategories";
            this.translateCategories.Size = new System.Drawing.Size(205, 368);
            this.translateCategories.TabIndex = 0;
            this.translateCategories.SelectedIndexChanged += new System.EventHandler(this.translateCategories_SelectedIndexChanged);
            // 
            // translateGrid
            // 
            this.translateGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.translateGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.translateGrid.Location = new System.Drawing.Point(0, 0);
            this.translateGrid.Name = "translateGrid";
            this.translateGrid.Size = new System.Drawing.Size(408, 379);
            this.translateGrid.TabIndex = 1;
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
            ((System.ComponentModel.ISupportInitialize)(this.translateGrid)).EndInit();
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

    }
}