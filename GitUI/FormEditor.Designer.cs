namespace GitUI
{
    partial class FormEditor
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
            this.fileViewer = new GitUI.Editor.FileViewer();
            this.toolStrip1 = new ToolStripEx();
            this.toolStripSaveButton = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // fileViewer
            // 
            this.fileViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileViewer.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.fileViewer.IgnoreWhitespaceChanges = false;
            this.fileViewer.IsReadOnly = true;
            this.fileViewer.Location = new System.Drawing.Point(0, 25);
            this.fileViewer.Margin = new System.Windows.Forms.Padding(0);
            this.fileViewer.Name = "fileViewer";
            this.fileViewer.NumberOfVisibleLines = 3;
            this.fileViewer.ScrollPos = 0;
            this.fileViewer.ShowEntireFile = false;
            this.fileViewer.ShowLineNumbers = true;
            this.fileViewer.Size = new System.Drawing.Size(659, 518);
            this.fileViewer.TabIndex = 0;
            this.fileViewer.TreatAllFilesAsText = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSaveButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(659, 25);
            this.toolStrip1.TabIndex = 1;
            // 
            // toolStripSaveButton
            // 
            this.toolStripSaveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSaveButton.Image = global::GitUI.Properties.Resources.Save;
            this.toolStripSaveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSaveButton.Name = "toolStripSaveButton";
            this.toolStripSaveButton.Size = new System.Drawing.Size(23, 22);
            this.toolStripSaveButton.ToolTipText = "Save";
            this.toolStripSaveButton.Click += new System.EventHandler(this.toolStripSaveButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.toolStrip1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.fileViewer, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(659, 543);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // FormEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(659, 543);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FormEditor";
            this.Text = "Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormEditor_FormClosing);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GitUI.Editor.FileViewer fileViewer;
        private ToolStripEx toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripSaveButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}