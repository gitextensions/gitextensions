namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class RevisionLinksSettingsPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.Remove = new System.Windows.Forms.Button();
            this.Add = new System.Windows.Forms.Button();
            this._NO_TRANSLATE_Categories = new System.Windows.Forms.ListBox();
            this.CategoriesLabel = new System.Windows.Forms.Label();
            this.LinksGrid = new System.Windows.Forms.DataGridView();
            this.CaptionCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.URICol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.detailPanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this._NO_TRANSLATE_SearchPatternEdit = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_NestedPatternEdit = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.nestedPatternLab = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.MessageChx = new System.Windows.Forms.CheckBox();
            this.LocalBranchChx = new System.Windows.Forms.CheckBox();
            this.RemoteBranchChx = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this._NO_TRANSLATE_Name = new System.Windows.Forms.TextBox();
            this.EnabledChx = new System.Windows.Forms.CheckBox();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LinksGrid)).BeginInit();
            this.panel1.SuspendLayout();
            this.detailPanel.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Panel1.Controls.Add(this._NO_TRANSLATE_Categories);
            this.splitContainer1.Panel1.Controls.Add(this.CategoriesLabel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.LinksGrid);
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Panel2.Controls.Add(this.detailPanel);
            this.splitContainer1.Size = new System.Drawing.Size(1125, 548);
            this.splitContainer1.SplitterDistance = 192;
            this.splitContainer1.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.Remove, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.Add, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 519);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(192, 29);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // Remove
            // 
            this.Remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Remove.AutoSize = true;
            this.Remove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Remove.Location = new System.Drawing.Point(134, 3);
            this.Remove.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Remove.Name = "Remove";
            this.Remove.Size = new System.Drawing.Size(56, 23);
            this.Remove.TabIndex = 4;
            this.Remove.Text = "Remove";
            this.Remove.UseVisualStyleBackColor = true;
            this.Remove.Click += new System.EventHandler(this.Remove_Click);
            // 
            // Add
            // 
            this.Add.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Add.AutoSize = true;
            this.Add.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Add.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Add.Location = new System.Drawing.Point(2, 3);
            this.Add.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Add.Name = "Add";
            this.Add.Size = new System.Drawing.Size(36, 23);
            this.Add.TabIndex = 3;
            this.Add.Text = "Add";
            this.Add.UseVisualStyleBackColor = true;
            this.Add.Click += new System.EventHandler(this.Add_Click);
            // 
            // _NO_TRANSLATE_Categories
            // 
            this._NO_TRANSLATE_Categories.DisplayMember = "Name";
            this._NO_TRANSLATE_Categories.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_Categories.FormattingEnabled = true;
            this._NO_TRANSLATE_Categories.IntegralHeight = false;
            this._NO_TRANSLATE_Categories.Location = new System.Drawing.Point(0, 22);
            this._NO_TRANSLATE_Categories.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this._NO_TRANSLATE_Categories.Name = "_NO_TRANSLATE_Categories";
            this._NO_TRANSLATE_Categories.Size = new System.Drawing.Size(192, 526);
            this._NO_TRANSLATE_Categories.TabIndex = 1;
            this._NO_TRANSLATE_Categories.SelectedIndexChanged += new System.EventHandler(this._NO_TRANSLATE_Categories_SelectedIndexChanged);
            // 
            // CategoriesLabel
            // 
            this.CategoriesLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.CategoriesLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CategoriesLabel.Location = new System.Drawing.Point(0, 0);
            this.CategoriesLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.CategoriesLabel.Name = "CategoriesLabel";
            this.CategoriesLabel.Size = new System.Drawing.Size(192, 22);
            this.CategoriesLabel.TabIndex = 0;
            this.CategoriesLabel.Text = "Categories";
            // 
            // LinksGrid
            // 
            this.LinksGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.LinksGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.CaptionCol,
            this.URICol});
            this.LinksGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LinksGrid.Location = new System.Drawing.Point(0, 134);
            this.LinksGrid.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.LinksGrid.MultiSelect = false;
            this.LinksGrid.Name = "LinksGrid";
            this.LinksGrid.RowHeadersVisible = false;
            this.LinksGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.LinksGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.LinksGrid.Size = new System.Drawing.Size(929, 414);
            this.LinksGrid.TabIndex = 8;
            // 
            // CaptionCol
            // 
            this.CaptionCol.DataPropertyName = "Caption";
            this.CaptionCol.HeaderText = "Caption";
            this.CaptionCol.Name = "CaptionCol";
            this.CaptionCol.Width = 150;
            // 
            // URICol
            // 
            this.URICol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.URICol.DataPropertyName = "Format";
            this.URICol.HeaderText = "URI";
            this.URICol.Name = "URICol";
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.label6);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 119);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.panel1.Size = new System.Drawing.Size(929, 15);
            this.panel1.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(206, -1);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "Links";
            // 
            // detailPanel
            // 
            this.detailPanel.AutoSize = true;
            this.detailPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.detailPanel.Controls.Add(this.tableLayoutPanel3);
            this.detailPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.detailPanel.Location = new System.Drawing.Point(0, 0);
            this.detailPanel.Name = "detailPanel";
            this.detailPanel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 6);
            this.detailPanel.Size = new System.Drawing.Size(929, 119);
            this.detailPanel.TabIndex = 6;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this._NO_TRANSLATE_SearchPatternEdit, 1, 3);
            this.tableLayoutPanel3.Controls.Add(this._NO_TRANSLATE_NestedPatternEdit, 1, 4);
            this.tableLayoutPanel3.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel2, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.nestedPatternLab, 0, 4);
            this.tableLayoutPanel3.Controls.Add(this.flowLayoutPanel1, 1, 2);
            this.tableLayoutPanel3.Controls.Add(this.flowLayoutPanel2, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 5;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(929, 110);
            this.tableLayoutPanel3.TabIndex = 14;
            // 
            // _NO_TRANSLATE_SearchPatternEdit
            // 
            this._NO_TRANSLATE_SearchPatternEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_SearchPatternEdit.Location = new System.Drawing.Point(86, 59);
            this._NO_TRANSLATE_SearchPatternEdit.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this._NO_TRANSLATE_SearchPatternEdit.Name = "_NO_TRANSLATE_SearchPatternEdit";
            this._NO_TRANSLATE_SearchPatternEdit.Size = new System.Drawing.Size(841, 21);
            this._NO_TRANSLATE_SearchPatternEdit.TabIndex = 22;
            this._NO_TRANSLATE_SearchPatternEdit.Leave += new System.EventHandler(this._NO_TRANSLATE_SearchPatternEdit_Leave);
            // 
            // _NO_TRANSLATE_NestedPatternEdit
            // 
            this._NO_TRANSLATE_NestedPatternEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_NestedPatternEdit.Location = new System.Drawing.Point(86, 86);
            this._NO_TRANSLATE_NestedPatternEdit.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this._NO_TRANSLATE_NestedPatternEdit.Name = "_NO_TRANSLATE_NestedPatternEdit";
            this._NO_TRANSLATE_NestedPatternEdit.Size = new System.Drawing.Size(841, 21);
            this._NO_TRANSLATE_NestedPatternEdit.TabIndex = 21;
            this._NO_TRANSLATE_NestedPatternEdit.Leave += new System.EventHandler(this._NO_TRANSLATE_NestedPatternEdit_Leave);
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(2, 63);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "Search pattern";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Location = new System.Drawing.Point(87, 30);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(0, 0);
            this.tableLayoutPanel2.TabIndex = 18;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(2, 35);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Search in";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(2, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Name";
            // 
            // nestedPatternLab
            // 
            this.nestedPatternLab.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nestedPatternLab.AutoSize = true;
            this.nestedPatternLab.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.nestedPatternLab.Location = new System.Drawing.Point(2, 90);
            this.nestedPatternLab.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.nestedPatternLab.Name = "nestedPatternLab";
            this.nestedPatternLab.Size = new System.Drawing.Size(80, 13);
            this.nestedPatternLab.TabIndex = 20;
            this.nestedPatternLab.Text = "Nested pattern";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.MessageChx);
            this.flowLayoutPanel1.Controls.Add(this.LocalBranchChx);
            this.flowLayoutPanel1.Controls.Add(this.RemoteBranchChx);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(87, 30);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(271, 23);
            this.flowLayoutPanel1.TabIndex = 23;
            // 
            // MessageChx
            // 
            this.MessageChx.AutoSize = true;
            this.MessageChx.Location = new System.Drawing.Point(3, 3);
            this.MessageChx.Name = "MessageChx";
            this.MessageChx.Size = new System.Drawing.Size(68, 17);
            this.MessageChx.TabIndex = 0;
            this.MessageChx.Text = "Message";
            this.MessageChx.UseVisualStyleBackColor = true;
            this.MessageChx.CheckedChanged += new System.EventHandler(this.MessageChx_CheckedChanged);
            // 
            // LocalBranchChx
            // 
            this.LocalBranchChx.AutoSize = true;
            this.LocalBranchChx.Location = new System.Drawing.Point(77, 3);
            this.LocalBranchChx.Name = "LocalBranchChx";
            this.LocalBranchChx.Size = new System.Drawing.Size(86, 17);
            this.LocalBranchChx.TabIndex = 1;
            this.LocalBranchChx.Text = "Local branch";
            this.LocalBranchChx.UseVisualStyleBackColor = true;
            this.LocalBranchChx.CheckedChanged += new System.EventHandler(this.LocalBranchChx_CheckedChanged);
            // 
            // RemoteBranchChx
            // 
            this.RemoteBranchChx.AutoSize = true;
            this.RemoteBranchChx.Location = new System.Drawing.Point(169, 3);
            this.RemoteBranchChx.Name = "RemoteBranchChx";
            this.RemoteBranchChx.Size = new System.Drawing.Size(99, 17);
            this.RemoteBranchChx.TabIndex = 2;
            this.RemoteBranchChx.Text = "Remote branch";
            this.RemoteBranchChx.UseVisualStyleBackColor = true;
            this.RemoteBranchChx.CheckedChanged += new System.EventHandler(this.RemoteBranchChx_CheckedChanged);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this._NO_TRANSLATE_Name);
            this.flowLayoutPanel2.Controls.Add(this.EnabledChx);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(84, 0);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(346, 27);
            this.flowLayoutPanel2.TabIndex = 24;
            // 
            // _NO_TRANSLATE_Name
            // 
            this._NO_TRANSLATE_Name.Location = new System.Drawing.Point(2, 3);
            this._NO_TRANSLATE_Name.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this._NO_TRANSLATE_Name.Name = "_NO_TRANSLATE_Name";
            this._NO_TRANSLATE_Name.Size = new System.Drawing.Size(272, 21);
            this._NO_TRANSLATE_Name.TabIndex = 11;
            this._NO_TRANSLATE_Name.Leave += new System.EventHandler(this._NO_TRANSLATE_Name_Leave);
            // 
            // EnabledChx
            // 
            this.EnabledChx.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.EnabledChx.AutoSize = true;
            this.EnabledChx.Location = new System.Drawing.Point(279, 5);
            this.EnabledChx.Name = "EnabledChx";
            this.EnabledChx.Size = new System.Drawing.Size(64, 17);
            this.EnabledChx.TabIndex = 22;
            this.EnabledChx.Text = "Enabled";
            this.EnabledChx.UseVisualStyleBackColor = true;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Title";
            this.dataGridViewTextBoxColumn1.HeaderText = "Title";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.Width = 150;
            // 
            // RevisionLinksSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.splitContainer1);
            this.Name = "RevisionLinksSettingsPage";
            this.Size = new System.Drawing.Size(1125, 548);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LinksGrid)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.detailPanel.ResumeLayout(false);
            this.detailPanel.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button Remove;
        private System.Windows.Forms.Button Add;
        private System.Windows.Forms.ListBox _NO_TRANSLATE_Categories;
        private System.Windows.Forms.Label CategoriesLabel;
        private System.Windows.Forms.Panel detailPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label nestedPatternLab;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_SearchPatternEdit;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_NestedPatternEdit;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.CheckBox MessageChx;
        private System.Windows.Forms.CheckBox LocalBranchChx;
        private System.Windows.Forms.CheckBox RemoteBranchChx;
        private System.Windows.Forms.DataGridViewTextBoxColumn CaptionCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn URICol;
        private System.Windows.Forms.DataGridView LinksGrid;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.TextBox _NO_TRANSLATE_Name;
        private System.Windows.Forms.CheckBox EnabledChx;
    }
}
