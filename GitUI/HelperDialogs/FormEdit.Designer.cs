using GitUI.Editor;

namespace GitUI.HelperDialogs
{
    partial class FormEdit
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Viewer = new GitUI.Editor.FileViewer();
            SuspendLayout();
            // 
            // Viewer
            // 
            Viewer.Dock = DockStyle.Fill;
            Viewer.Location = new Point(0, 0);
            Viewer.Margin = new Padding(3, 2, 3, 2);
            Viewer.Name = "Viewer";
            Viewer.Size = new Size(733, 571);
            Viewer.TabIndex = 0;
            // 
            // FormEdit
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(733, 571);
            Controls.Add(Viewer);
            Name = "FormEdit";
            StartPosition = FormStartPosition.CenterParent;
            Text = "View";
            ResumeLayout(false);

        }

        #endregion

        private FileViewer Viewer;
    }
}