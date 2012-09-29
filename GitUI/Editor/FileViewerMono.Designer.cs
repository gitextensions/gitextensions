namespace GitUI.Editor
{
    partial class FileViewerMono
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
            this.TextEditor = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // TextEditor
            // 
            this.TextEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextEditor.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextEditor.Location = new System.Drawing.Point(0, 0);
            this.TextEditor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.TextEditor.Name = "TextEditor";
            this.TextEditor.Size = new System.Drawing.Size(660, 480);
            this.TextEditor.TabIndex = 0;
            this.TextEditor.Text = "";
            // 
            // FileViewerMono
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.TextEditor);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FileViewerMono";
            this.Size = new System.Drawing.Size(660, 480);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox TextEditor;
    }
}
