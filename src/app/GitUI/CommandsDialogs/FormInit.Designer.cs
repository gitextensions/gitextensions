using System.Windows.Forms;

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
            Browse = new UserControls.FolderBrowserButton();
            groupBox1 = new GroupBox();
            Central = new RadioButton();
            Personal = new RadioButton();
            Init = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            tpnlMain = new TableLayoutPanel();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            groupBox1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tpnlMain.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.AutoSize = true;
            MainPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MainPanel.Controls.Add(tpnlMain);
            MainPanel.Size = new Size(542, 133);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(Init);
            ControlsPanel.Location = new Point(0, 133);
            ControlsPanel.Size = new Size(542, 41);
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(3, 8);
            label1.Name = "label1";
            label1.Size = new Size(55, 15);
            label1.TabIndex = 0;
            label1.Text = "Directory";
            // 
            // _NO_TRANSLATE_Directory
            // 
            _NO_TRANSLATE_Directory.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _NO_TRANSLATE_Directory.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _NO_TRANSLATE_Directory.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
            _NO_TRANSLATE_Directory.FormattingEnabled = true;
            _NO_TRANSLATE_Directory.Location = new Point(64, 4);
            _NO_TRANSLATE_Directory.Name = "_NO_TRANSLATE_Directory";
            _NO_TRANSLATE_Directory.Size = new Size(331, 23);
            _NO_TRANSLATE_Directory.TabIndex = 1;
            // 
            // Browse
            // 
            Browse.Anchor = AnchorStyles.Right;
            Browse.Location = new Point(409, 3);
            Browse.Name = "Browse";
            Browse.PathShowingControl = _NO_TRANSLATE_Directory;
            Browse.Size = new Size(106, 25);
            Browse.TabIndex = 2;
            Browse.Click += BrowseClick;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.AutoSize = true;
            groupBox1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            groupBox1.Controls.Add(Central);
            groupBox1.Controls.Add(Personal);
            groupBox1.Location = new Point(3, 34);
            groupBox1.MaximumSize = new Size(0, 88);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(512, 88);
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
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_Directory, 1, 0);
            tableLayoutPanel1.Controls.Add(Browse, 2, 0);
            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.Size = new Size(518, 31);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // tpnlMain
            // 
            tpnlMain.AutoSize = true;
            tpnlMain.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tpnlMain.ColumnCount = 1;
            tpnlMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tpnlMain.Controls.Add(tableLayoutPanel1, 0, 0);
            tpnlMain.Controls.Add(groupBox1, 0, 1);
            tpnlMain.Dock = DockStyle.Fill;
            tpnlMain.Location = new Point(12, 12);
            tpnlMain.Margin = new Padding(0);
            tpnlMain.Name = "tpnlMain";
            tpnlMain.RowCount = 2;
            tpnlMain.RowStyles.Add(new RowStyle());
            tpnlMain.RowStyles.Add(new RowStyle());
            tpnlMain.Size = new Size(518, 109);
            tpnlMain.TabIndex = 0;
            // 
            // FormInit
            // 
            AcceptButton = Init;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            ClientSize = new Size(542, 174);
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
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tpnlMain.ResumeLayout(false);
            tpnlMain.PerformLayout();
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
        private TableLayoutPanel tpnlMain;
        private TableLayoutPanel tableLayoutPanel1;
    }
}
