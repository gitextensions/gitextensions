namespace GitUI
{
    partial class FileStatusList
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
            components = new System.ComponentModel.Container();
            FileStatusListView = new UserControls.NativeListView();
            columnHeader = new ColumnHeader();
            NoFiles = new Label();
            LoadingFiles = new Label();
            FilterComboBox = new ComboBox();
            FilterWatermarkLabel = new Label();
            FilterToolTip = new ToolTip(components);
            lblSplitter = new Label();
            DeleteFilterButton = new Label();
            cboFindInCommitFilesGitGrep = new ComboBox();
            lblFindInCommitFilesGitGrepWatermark = new Label();
            DeleteSearchButton = new Label();
            SuspendLayout();
            // 
            // FileStatusListView
            // 
            FileStatusListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            FileStatusListView.BorderStyle = BorderStyle.None;
            FileStatusListView.Columns.AddRange(new ColumnHeader[] { columnHeader });
            FileStatusListView.FullRowSelect = true;
            FileStatusListView.HeaderStyle = ColumnHeaderStyle.None;
            FileStatusListView.LabelWrap = false;
            FileStatusListView.Location = new Point(0, 46);
            FileStatusListView.Margin = new Padding(0);
            FileStatusListView.Name = "FileStatusListView";
            FileStatusListView.OwnerDraw = true;
            FileStatusListView.ShowItemToolTips = true;
            FileStatusListView.Size = new Size(682, 439);
            FileStatusListView.Sorting = SortOrder.Ascending;
            FileStatusListView.TabIndex = 9;
            FileStatusListView.UseCompatibleStateImageBehavior = false;
            FileStatusListView.View = View.Details;
            FileStatusListView.DrawSubItem += FileStatusListView_DrawSubItem;
            FileStatusListView.ClientSizeChanged += FileStatusListView_ClientSizeChanged;
            FileStatusListView.DoubleClick += FileStatusListView_DoubleClick;
            FileStatusListView.KeyDown += FileStatusListView_KeyDown;
            FileStatusListView.MouseDown += FileStatusListView_MouseDown;
            FileStatusListView.MouseMove += FileStatusListView_MouseMove;
            FileStatusListView.MouseUp += FileStatusListView_MouseUp;
            FileStatusListView.Scroll += FileStatusListView_Scroll;
            // 
            // columnHeader
            // 
            columnHeader.Text = "Files";
            columnHeader.Width = 678;
            // 
            // NoFiles
            // 
            NoFiles.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            NoFiles.AutoSize = true;
            NoFiles.BackColor = SystemColors.Window;
            NoFiles.ForeColor = SystemColors.GrayText;
            NoFiles.Location = new Point(0, 23);
            NoFiles.Name = "NoFiles";
            NoFiles.Padding = new Padding(2);
            NoFiles.Size = new Size(65, 15);
            NoFiles.TabIndex = 1;
            // 
            // LoadingFiles
            // 
            LoadingFiles.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            LoadingFiles.AutoSize = true;
            LoadingFiles.BackColor = SystemColors.Window;
            LoadingFiles.ForeColor = SystemColors.GrayText;
            LoadingFiles.Location = new Point(0, 46);
            LoadingFiles.Name = "LoadingFiles";
            LoadingFiles.Padding = new Padding(2);
            LoadingFiles.Size = new Size(65, 15);
            LoadingFiles.TabIndex = 2;
            // 
            // cboFindInCommitFilesGitGrep
            // 
            cboFindInCommitFilesGitGrep.Dock = DockStyle.Top;
            cboFindInCommitFilesGitGrep.FlatStyle = FlatStyle.Flat;
            cboFindInCommitFilesGitGrep.FormattingEnabled = true;
            cboFindInCommitFilesGitGrep.Location = new Point(0, 0);
            cboFindInCommitFilesGitGrep.Margin = new Padding(0);
            cboFindInCommitFilesGitGrep.Name = "cboFindInCommitFilesGitGrep";
            cboFindInCommitFilesGitGrep.Size = new Size(682, 23);
            cboFindInCommitFilesGitGrep.TabIndex = 0;
            cboFindInCommitFilesGitGrep.Tag = "ToolBar_group:Text search";
            cboFindInCommitFilesGitGrep.SelectedIndexChanged += cboFindInCommitFilesGitGrep_SelectedIndexChanged;
            cboFindInCommitFilesGitGrep.TextUpdate += cboFindInCommitFilesGitGrep_TextUpdate;
            cboFindInCommitFilesGitGrep.SizeChanged += cboFindInCommitFilesGitGrep_SizeChanged;
            cboFindInCommitFilesGitGrep.GotFocus += cboFindInCommitFilesGitGrep_GotFocus;
            cboFindInCommitFilesGitGrep.LostFocus += cboFindInCommitFilesGitGrep_LostFocus;
            // 
            // lblFindInCommitFilesGitGrepWatermark
            // 
            lblFindInCommitFilesGitGrepWatermark.AutoSize = true;
            lblFindInCommitFilesGitGrepWatermark.BackColor = SystemColors.Window;
            lblFindInCommitFilesGitGrepWatermark.ForeColor = SystemColors.GrayText;
            lblFindInCommitFilesGitGrepWatermark.Location = new Point(0);
            lblFindInCommitFilesGitGrepWatermark.Name = "lblFindInCommitFilesGitGrepWatermark";
            lblFindInCommitFilesGitGrepWatermark.Padding = new Padding(2);
            lblFindInCommitFilesGitGrepWatermark.Size = new Size(273, 15);
            lblFindInCommitFilesGitGrepWatermark.TabIndex = 3;
            lblFindInCommitFilesGitGrepWatermark.Text = "Find in commit files using git-grep regular expression...";
            lblFindInCommitFilesGitGrepWatermark.Click += lblFindInCommitFilesGitGrepWatermark_Click;
            // 
            // DeleteSearchButton
            // 
            DeleteSearchButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            DeleteSearchButton.BackColor = SystemColors.Window;
            DeleteSearchButton.FlatStyle = FlatStyle.Flat;
            DeleteSearchButton.Image = Properties.Resources.DeleteText;
            DeleteSearchButton.Location = new Point(646, 0);
            DeleteSearchButton.Name = "DeleteSearchButton";
            DeleteSearchButton.Padding = new Padding(0);
            DeleteSearchButton.Size = new Size(18, 23);
            DeleteSearchButton.TabIndex = 4;
            DeleteSearchButton.Click += DeleteSearchButton_Click;
            // 
            // FilterComboBox
            // 
            FilterComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            FilterComboBox.FlatStyle = FlatStyle.Flat;
            FilterComboBox.FormattingEnabled = true;
            FilterComboBox.Location = new Point(0, 23);
            FilterComboBox.Margin = new Padding(0);
            FilterComboBox.Name = "FilterComboBox";
            FilterComboBox.Size = new Size(682, 23);
            FilterComboBox.TabIndex = 5;
            FilterComboBox.SelectedIndexChanged += FilterComboBox_SelectedIndexChanged;
            FilterComboBox.TextUpdate += FilterComboBox_TextUpdate;
            FilterComboBox.SizeChanged += FilterComboBox_SizeChanged;
            FilterComboBox.GotFocus += FilterComboBox_GotFocus;
            FilterComboBox.LostFocus += FilterComboBox_LostFocus;
            FilterComboBox.MouseEnter += FilterComboBox_MouseEnter;
            // 
            // FilterWatermarkLabel
            // 
            FilterWatermarkLabel.AutoSize = true;
            FilterWatermarkLabel.BackColor = SystemColors.Window;
            FilterWatermarkLabel.ForeColor = SystemColors.GrayText;
            FilterWatermarkLabel.Location = new Point(0, 23);
            FilterWatermarkLabel.Name = "FilterWatermarkLabel";
            FilterWatermarkLabel.Padding = new Padding(2);
            FilterWatermarkLabel.Size = new Size(206, 15);
            FilterWatermarkLabel.TabIndex = 6;
            FilterWatermarkLabel.Text = "Filter files using a regular expression...";
            FilterWatermarkLabel.Click += FilterWatermarkLabel_Click;
            // 
            // FilterToolTip
            // 
            FilterToolTip.AutomaticDelay = 100;
            FilterToolTip.ShowAlways = true;
            FilterToolTip.ToolTipIcon = ToolTipIcon.Error;
            FilterToolTip.ToolTipTitle = "RegEx";
            FilterToolTip.UseAnimation = false;
            FilterToolTip.UseFading = false;
            // 
            // lblSplitter
            // 
            lblSplitter.Dock = DockStyle.Top;
            lblSplitter.Location = new Point(0, 46);
            lblSplitter.Name = "lblSplitter";
            lblSplitter.Size = new Size(682, 2);
            lblSplitter.TabIndex = 8;
            // 
            // DeleteFilterButton
            // 
            DeleteFilterButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            DeleteFilterButton.BackColor = SystemColors.Window;
            DeleteFilterButton.FlatStyle = FlatStyle.Flat;
            DeleteFilterButton.Image = Properties.Resources.DeleteText;
            DeleteFilterButton.Location = new Point(646, 23);
            DeleteFilterButton.Name = "DeleteFilterButton";
            DeleteFilterButton.Size = new Size(18, 23);
            DeleteFilterButton.TabIndex = 7;
            DeleteFilterButton.Click += DeleteFilterButton_Click;
            // 
            // FileStatusList
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(LoadingFiles);
            Controls.Add(NoFiles);
            Controls.Add(lblFindInCommitFilesGitGrepWatermark);
            Controls.Add(DeleteSearchButton);
            Controls.Add(FilterWatermarkLabel);
            Controls.Add(DeleteFilterButton);
            Controls.Add(cboFindInCommitFilesGitGrep);
            Controls.Add(FileStatusListView);
            Controls.Add(FilterComboBox);
            Controls.Add(lblSplitter);
            Margin = new Padding(3, 4, 3, 4);
            Name = "FileStatusList";
            Size = new Size(682, 485);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GitUI.UserControls.NativeListView FileStatusListView;
        private Label NoFiles;
        private Label LoadingFiles;
        private ColumnHeader columnHeader;
        private ComboBox FilterComboBox;
        private Label FilterWatermarkLabel;
        private ToolTip FilterToolTip;
        private Label lblSplitter;
        private Label DeleteFilterButton;
        private ComboBox cboFindInCommitFilesGitGrep;
        private Label lblFindInCommitFilesGitGrepWatermark;
        private Label DeleteSearchButton;
    }
}
