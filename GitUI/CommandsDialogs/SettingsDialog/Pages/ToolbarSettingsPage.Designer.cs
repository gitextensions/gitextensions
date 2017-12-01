namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class ToolbarSettingsPage
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
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanelForToolbar = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.cbBranchOrderingCriteria = new System.Windows.Forms.ComboBox();
            this.tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanelForToolbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelForToolbar
            // 
            this.tableLayoutPanelForToolbar.AutoSize = true;
            this.tableLayoutPanelForToolbar.ColumnCount = 2;
            this.tableLayoutPanelForToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelForToolbar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelForToolbar.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanelForToolbar.Controls.Add(this.cbBranchOrderingCriteria, 1, 0);
            this.tableLayoutPanelForToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelForToolbar.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelForToolbar.Name = "tableLayoutPanelForToolbar";
            this.tableLayoutPanelForToolbar.RowCount = 9;
            this.tableLayoutPanelForToolbar.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForToolbar.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForToolbar.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForToolbar.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForToolbar.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForToolbar.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForToolbar.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForToolbar.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForToolbar.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForToolbar.Size = new System.Drawing.Size(1333, 684);
            this.tableLayoutPanelForToolbar.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(175, 19);
            this.label2.TabIndex = 2;
            this.label2.Text = "Branch ordering criteria";
            // 
            // cbBranchOrderingCriteria
            // 
            this.cbBranchOrderingCriteria.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbBranchOrderingCriteria.FormattingEnabled = true;
            this.cbBranchOrderingCriteria.Items.AddRange(new object[] {
            "Last access date",
            "Alphabetically"});
            this.cbBranchOrderingCriteria.Location = new System.Drawing.Point(184, 3);
            this.cbBranchOrderingCriteria.Name = "cbBranchOrderingCriteria";
            this.cbBranchOrderingCriteria.Size = new System.Drawing.Size(213, 27);
            this.cbBranchOrderingCriteria.TabIndex = 3;
            // 
            // tooltip
            // 
            this.tooltip.AutoPopDelay = 30000;
            this.tooltip.InitialDelay = 500;
            this.tooltip.ReshowDelay = 100;
            // 
            // ToolbarSettingsPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.tableLayoutPanelForToolbar);
            this.Name = "ToolbarSettingsPage";
            this.Size = new System.Drawing.Size(1333, 684);
            this.tableLayoutPanelForToolbar.ResumeLayout(false);
            this.tableLayoutPanelForToolbar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelForToolbar;
        private System.Windows.Forms.ToolTip tooltip;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbBranchOrderingCriteria;
    }
}
