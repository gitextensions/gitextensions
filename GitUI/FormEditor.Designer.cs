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
            this.fileViewer = new GitUI.Editor.FileViewerMono();
            this.SuspendLayout();
            // 
            // fileViewer
            // 
            this.fileViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileViewer.Font = new System.Drawing.Font("Tahoma", 9.75F);
            this.fileViewer.IgnoreWhitespaceChanges = false;
            this.fileViewer.IsReadOnly = true;
            this.fileViewer.Location = new System.Drawing.Point(0, 0);
            this.fileViewer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.fileViewer.Name = "fileViewer";
            this.fileViewer.NumberOfVisibleLines = 3;
            this.fileViewer.ScrollPos = 0;
            this.fileViewer.ShowEntireFile = false;
            this.fileViewer.Size = new System.Drawing.Size(659, 543);
            this.fileViewer.TabIndex = 0;
            this.fileViewer.TreatAllFilesAsText = false;
            // 
            // FormEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(659, 543);
            this.Controls.Add(this.fileViewer);
            this.Name = "FormEditor";
            this.Text = "Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormEditor_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private GitUI.Editor.FileViewerMono fileViewer;
    }
}