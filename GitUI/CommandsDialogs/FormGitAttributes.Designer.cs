namespace GitUI.CommandsDialogs
{
    partial class FormGitAttributes
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGitAttributes));
            splitContainer1 = new SplitContainer();
            _NO_TRANSLATE_GitAttributesText = new GitUI.Editor.FileViewer();
            label1 = new Label();
            Save = new Button();

            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();

            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(_NO_TRANSLATE_GitAttributesText);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(label1);
            splitContainer1.Panel2.Controls.Add(Save);
            splitContainer1.Size = new Size(634, 474);
            splitContainer1.SplitterDistance = 381;
            splitContainer1.TabIndex = 0;
            // 
            // _NO_TRANSLATE_GitAttributesText
            // 
            _NO_TRANSLATE_GitAttributesText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_GitAttributesText.IsReadOnly = false;
            _NO_TRANSLATE_GitAttributesText.Location = new Point(0, 0);
            _NO_TRANSLATE_GitAttributesText.Margin = new Padding(3, 2, 3, 2);
            _NO_TRANSLATE_GitAttributesText.Name = "_NO_TRANSLATE_GitAttributesText";
            _NO_TRANSLATE_GitAttributesText.Size = new Size(381, 474);
            _NO_TRANSLATE_GitAttributesText.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 9);
            label1.Name = "label1";
            label1.Size = new Size(209, 285);
            label1.TabIndex = 1;
            label1.Text = resources.GetString("label1.Text");
            // 
            // Save
            // 
            Save.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Save.Location = new Point(162, 439);
            Save.Name = "Save";
            Save.Size = new Size(75, 25);
            Save.TabIndex = 0;
            Save.Text = "Save";
            Save.UseVisualStyleBackColor = true;
            Save.Click += SaveClick;
            // 
            // FormGitAttributes
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(634, 474);
            Controls.Add(splitContainer1);
            Name = "FormGitAttributes";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Edit .gitattributes";
            FormClosing += FormGitAttributesClosing;
            Load += FormGitAttributesLoad;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();

            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();

            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private SplitContainer splitContainer1;
        private GitUI.Editor.FileViewer _NO_TRANSLATE_GitAttributesText;
        private Button Save;
        private Label label1;
    }
}