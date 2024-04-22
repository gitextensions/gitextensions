namespace GitUI.CommandsDialogs
{
    partial class FormGitIgnore
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
            FlowLayoutPanel panel1;
            FlowLayoutPanel flowLayoutPanel2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGitIgnore));
            lnkGitIgnoreGenerate = new LinkLabel();
            lnkGitIgnorePatterns = new LinkLabel();
            AddDefault = new Button();
            AddPattern = new Button();
            Save = new Button();
            splitContainer1 = new SplitContainer();
            _NO_TRANSLATE_GitIgnoreEdit = new Editor.FileViewer();
            label1 = new TextBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            btnCancel = new Button();
            panel1 = new FlowLayoutPanel();
            flowLayoutPanel2 = new FlowLayoutPanel();
            panel1.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel1.Controls.Add(lnkGitIgnoreGenerate);
            panel1.Controls.Add(lnkGitIgnorePatterns);
            panel1.Dock = DockStyle.Bottom;
            panel1.FlowDirection = FlowDirection.RightToLeft;
            panel1.Location = new Point(0, 540);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(0, 0, 8, 4);
            panel1.Size = new Size(270, 42);
            panel1.TabIndex = 5;
            // 
            // lnkGitIgnoreGenerate
            // 
            lnkGitIgnoreGenerate.AutoSize = true;
            lnkGitIgnoreGenerate.Dock = DockStyle.Right;
            lnkGitIgnoreGenerate.Location = new Point(62, 2);
            lnkGitIgnoreGenerate.Margin = new Padding(3, 2, 3, 2);
            lnkGitIgnoreGenerate.Name = "lnkGitIgnoreGenerate";
            lnkGitIgnoreGenerate.RightToLeft = RightToLeft.Yes;
            lnkGitIgnoreGenerate.Size = new Size(197, 15);
            lnkGitIgnoreGenerate.TabIndex = 7;
            lnkGitIgnoreGenerate.TabStop = true;
            lnkGitIgnoreGenerate.Text = "Generate a custom ignore file for git";
            lnkGitIgnoreGenerate.LinkClicked += lnkGitIgnoreGenerate_LinkClicked;
            // 
            // lnkGitIgnorePatterns
            // 
            lnkGitIgnorePatterns.AutoSize = true;
            lnkGitIgnorePatterns.Dock = DockStyle.Right;
            lnkGitIgnorePatterns.Location = new Point(124, 21);
            lnkGitIgnorePatterns.Margin = new Padding(3, 2, 3, 2);
            lnkGitIgnorePatterns.Name = "lnkGitIgnorePatterns";
            lnkGitIgnorePatterns.RightToLeft = RightToLeft.Yes;
            lnkGitIgnorePatterns.Size = new Size(135, 15);
            lnkGitIgnorePatterns.TabIndex = 6;
            lnkGitIgnorePatterns.TabStop = true;
            lnkGitIgnorePatterns.Text = "Example ignore patterns";
            lnkGitIgnorePatterns.LinkClicked += lnkGitIgnorePatterns_LinkClicked;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.AutoSize = true;
            flowLayoutPanel2.Controls.Add(AddDefault);
            flowLayoutPanel2.Controls.Add(AddPattern);
            flowLayoutPanel2.Dock = DockStyle.Bottom;
            flowLayoutPanel2.Location = new Point(0, 549);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(352, 33);
            flowLayoutPanel2.TabIndex = 6;
            flowLayoutPanel2.WrapContents = false;
            // 
            // AddDefault
            // 
            AddDefault.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            AddDefault.AutoSize = true;
            AddDefault.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            AddDefault.Location = new Point(3, 3);
            AddDefault.MinimumSize = new Size(160, 27);
            AddDefault.Name = "AddDefault";
            AddDefault.Size = new Size(160, 27);
            AddDefault.TabIndex = 2;
            AddDefault.Text = "Add default ignores";
            AddDefault.UseVisualStyleBackColor = true;
            AddDefault.Click += AddDefaultClick;
            // 
            // AddPattern
            // 
            AddPattern.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            AddPattern.AutoSize = true;
            AddPattern.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            AddPattern.Location = new Point(169, 3);
            AddPattern.MinimumSize = new Size(160, 27);
            AddPattern.Name = "AddPattern";
            AddPattern.Size = new Size(160, 27);
            AddPattern.TabIndex = 3;
            AddPattern.Text = "Add pattern";
            AddPattern.UseVisualStyleBackColor = true;
            AddPattern.Click += AddPattern_Click;
            // 
            // Save
            // 
            Save.AutoSize = true;
            Save.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Save.DialogResult = DialogResult.OK;
            Save.Image = Properties.Images.Save;
            Save.ImageAlign = ContentAlignment.MiddleRight;
            Save.Location = new Point(463, 3);
            Save.MinimumSize = new Size(160, 27);
            Save.Name = "Save";
            Save.Size = new Size(160, 27);
            Save.TabIndex = 1;
            Save.Text = "Save";
            Save.TextImageRelation = TextImageRelation.ImageBeforeText;
            Save.UseVisualStyleBackColor = true;
            Save.Click += SaveClick;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel2;
            splitContainer1.Location = new Point(4, 4);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(_NO_TRANSLATE_GitIgnoreEdit);
            splitContainer1.Panel1.Controls.Add(flowLayoutPanel2);
            splitContainer1.Panel1MinSize = 250;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(label1);
            splitContainer1.Panel2.Controls.Add(panel1);
            splitContainer1.Panel2MinSize = 250;
            splitContainer1.Size = new Size(626, 582);
            splitContainer1.SplitterDistance = 352;
            splitContainer1.TabIndex = 0;
            // 
            // _NO_TRANSLATE_GitIgnoreEdit
            // 
            _NO_TRANSLATE_GitIgnoreEdit.AutoScroll = true;
            _NO_TRANSLATE_GitIgnoreEdit.BorderStyle = BorderStyle.None;
            _NO_TRANSLATE_GitIgnoreEdit.Dock = DockStyle.Fill;
            _NO_TRANSLATE_GitIgnoreEdit.IsReadOnly = false;
            _NO_TRANSLATE_GitIgnoreEdit.Location = new Point(0, 0);
            _NO_TRANSLATE_GitIgnoreEdit.Margin = new Padding(0, 0, 3, 2);
            _NO_TRANSLATE_GitIgnoreEdit.Name = "_NO_TRANSLATE_GitIgnoreEdit";
            _NO_TRANSLATE_GitIgnoreEdit.Size = new Size(352, 549);
            _NO_TRANSLATE_GitIgnoreEdit.TabIndex = 0;
            // 
            // label1
            // 
            label1.BackColor = SystemColors.Control;
            label1.BorderStyle = BorderStyle.None;
            label1.Dock = DockStyle.Fill;
            label1.ForeColor = SystemColors.ControlText;
            label1.Location = new Point(0, 0);
            label1.Margin = new Padding(3, 0, 3, 3);
            label1.Multiline = true;
            label1.Name = "label1";
            label1.ReadOnly = true;
            label1.ScrollBars = ScrollBars.Vertical;
            label1.Size = new Size(270, 540);
            label1.TabIndex = 4;
            label1.Text = resources.GetString("label1.Text");
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(Save);
            flowLayoutPanel1.Controls.Add(btnCancel);
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(4, 586);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(626, 33);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // btnCancel
            // 
            btnCancel.AutoSize = true;
            btnCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(382, 3);
            btnCancel.MinimumSize = new Size(75, 27);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 27);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // FormGitIgnore
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = btnCancel;
            ClientSize = new Size(634, 623);
            Controls.Add(splitContainer1);
            Controls.Add(flowLayoutPanel1);
            MinimumSize = new Size(650, 498);
            Name = "FormGitIgnore";
            Padding = new Padding(4);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Edit .gitignore";
            FormClosing += FormGitIgnoreFormClosing;
            Load += FormGitIgnoreLoad;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            flowLayoutPanel2.ResumeLayout(false);
            flowLayoutPanel2.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private SplitContainer splitContainer1;
        private GitUI.Editor.FileViewer _NO_TRANSLATE_GitIgnoreEdit;
        private TextBox label1;
        private Button Save;
        private Button AddDefault;
        private Button AddPattern;
        private LinkLabel lnkGitIgnorePatterns;
        private LinkLabel lnkGitIgnoreGenerate;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button btnCancel;
    }
}
