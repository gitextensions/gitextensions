namespace GitUI.CommandsDialogs
{
    partial class FormInit
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
            label1 = new Label();
            _NO_TRANSLATE_Directory = new ComboBox();
            Browse = new GitUI.UserControls.FolderBrowserButton();
            groupBox1 = new GroupBox();
            Central = new RadioButton();
            Personal = new RadioButton();
            Init = new Button();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(groupBox1);
            MainPanel.Controls.Add(Browse);
            MainPanel.Controls.Add(_NO_TRANSLATE_Directory);
            MainPanel.Controls.Add(label1);
            MainPanel.Size = new Size(542, 132);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(Init);
            ControlsPanel.Location = new Point(0, 132);
            ControlsPanel.Size = new Size(542, 41);
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 17);
            label1.Name = "label1";
            label1.Size = new Size(55, 15);
            label1.TabIndex = 0;
            label1.Text = "Directory";
            // 
            // _NO_TRANSLATE_Directory
            // 
            _NO_TRANSLATE_Directory.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _NO_TRANSLATE_Directory.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            _NO_TRANSLATE_Directory.FormattingEnabled = true;
            _NO_TRANSLATE_Directory.Location = new Point(95, 14);
            _NO_TRANSLATE_Directory.Name = "_NO_TRANSLATE_Directory";
            _NO_TRANSLATE_Directory.Size = new Size(323, 23);
            _NO_TRANSLATE_Directory.TabIndex = 1;
            // 
            // Browse
            // 
            Browse.Location = new Point(424, 12);
            Browse.Name = "Browse";
            Browse.PathShowingControl = _NO_TRANSLATE_Directory;
            Browse.Size = new Size(106, 25);
            Browse.TabIndex = 2;
            Browse.Click += BrowseClick;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(Central);
            groupBox1.Controls.Add(Personal);
            groupBox1.Location = new Point(12, 41);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(518, 78);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "Repository type";
            // 
            // Central
            // 
            Central.AutoSize = true;
            Central.Location = new Point(19, 48);
            Central.Name = "Central";
            Central.Size = new Size(350, 19);
            Central.TabIndex = 1;
            Central.Text = "Central repository, no working directory  (--bare --shared=all)";
            Central.UseVisualStyleBackColor = true;
            // 
            // Personal
            // 
            Personal.AutoSize = true;
            Personal.Checked = true;
            Personal.Location = new Point(19, 25);
            Personal.Name = "Personal";
            Personal.Size = new Size(126, 19);
            Personal.TabIndex = 0;
            Personal.TabStop = true;
            Personal.Text = "Personal repository";
            Personal.UseVisualStyleBackColor = true;
            // 
            // Init
            // 
            Init.AutoSize = true;
            Init.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Init.Location = new Point(454, 8);
            Init.MinimumSize = new Size(75, 23);
            Init.Name = "Init";
            Init.Size = new Size(75, 25);
            Init.TabIndex = 4;
            Init.Text = "Create";
            Init.UseVisualStyleBackColor = true;
            Init.Click += InitClick;
            // 
            // FormInit
            // 
            AcceptButton = Init;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(542, 173);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            HelpButton = true;
            ManualSectionAnchorName = "create-new-repository";
            ManualSectionSubfolder = "getting_started";
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormInit";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Create new repository";
            MainPanel.ResumeLayout(false);
            MainPanel.PerformLayout();
            ControlsPanel.ResumeLayout(false);
            ControlsPanel.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label label1;
        private ComboBox _NO_TRANSLATE_Directory;
        private UserControls.FolderBrowserButton Browse;
        private GroupBox groupBox1;
        private RadioButton Central;
        private RadioButton Personal;
        private Button Init;
    }
}
