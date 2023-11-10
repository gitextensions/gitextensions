namespace GitUI.Editor
{
    partial class FileViewerInternal
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
            if (disposing && (components is not null))
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
            TextEditor = new ICSharpCode.TextEditor.TextEditorControl();
            SuspendLayout();
            // 
            // TextEditor
            // 
            TextEditor.Dock = DockStyle.Fill;
            TextEditor.IsReadOnly = false;
            TextEditor.Location = new Point(0, 0);
            TextEditor.Margin = new Padding(0);
            TextEditor.Name = "TextEditor";
            TextEditor.Size = new Size(757, 519);
            TextEditor.TabIndex = 3;
            // 
            // FileViewerInternal
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(TextEditor);
            Margin = new Padding(0);
            Name = "FileViewerInternal";
            Size = new Size(757, 519);
            ResumeLayout(false);
        }

        #endregion

        private ICSharpCode.TextEditor.TextEditorControl TextEditor;
    }
}
