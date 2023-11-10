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
            FileStatusListView = new GitUI.UserControls.NativeListView();
            columnHeader = ((ColumnHeader)(new ColumnHeader()));
            NoFiles = new Label();
            LoadingFiles = new Label();
            FilterComboBox = new ComboBox();
            FilterWatermarkLabel = new Label();
            FilterToolTip = new ToolTip(components);
            lblSplitter = new Label();
            DeleteFilterButton = new Label();
            SuspendLayout();
            // 
            // FileStatusListView
            // 
            FileStatusListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            FileStatusListView.BorderStyle = BorderStyle.None;
            FileStatusListView.Columns.AddRange(new ColumnHeader[] {
            columnHeader});
            FileStatusListView.FullRowSelect = true;
            FileStatusListView.HeaderStyle = ColumnHeaderStyle.None;
            FileStatusListView.HideSelection = false;
            FileStatusListView.LabelWrap = false;
            FileStatusListView.Location = new Point(0, 21);
            FileStatusListView.Margin = new Padding(0);
            FileStatusListView.Name = "FileStatusListView";
            FileStatusListView.OwnerDraw = true;
            FileStatusListView.ShowItemToolTips = true;
            FileStatusListView.Size = new Size(682, 464);
            FileStatusListView.Sorting = SortOrder.Ascending;
            FileStatusListView.TabIndex = 4;
            FileStatusListView.UseCompatibleStateImageBehavior = false;
            FileStatusListView.View = View.Details;
            FileStatusListView.DrawSubItem += FileStatusListView_DrawSubItem;
            FileStatusListView.ClientSizeChanged += FileStatusListView_ClientSizeChanged;
            FileStatusListView.DoubleClick += FileStatusListView_DoubleClick;
            FileStatusListView.KeyDown += FileStatusListView_KeyDown;
            FileStatusListView.MouseDown += FileStatusListView_MouseDown;
            FileStatusListView.MouseUp += FileStatusListView_MouseUp;
            FileStatusListView.MouseMove += FileStatusListView_MouseMove;
            FileStatusListView.Scroll += FileStatusListView_Scroll;
            // 
            // columnHeader
            // 
            columnHeader.Text = "Files";
            columnHeader.Width = 678;
            // 
            // NoFiles
            // 
            NoFiles.AutoSize = true;
            NoFiles.BackColor = SystemColors.Window;
            NoFiles.ForeColor = SystemColors.GrayText;
            NoFiles.Location = new Point(4, 4);
            NoFiles.Margin = new Padding(0);
            NoFiles.Name = "NoFiles";
            NoFiles.Size = new Size(65, 13);
            NoFiles.TabIndex = 5;
            // 
            // LoadingFiles
            // 
            LoadingFiles.AutoSize = true;
            LoadingFiles.BackColor = SystemColors.Window;
            LoadingFiles.ForeColor = SystemColors.GrayText;
            LoadingFiles.Location = new Point(4, 21);
            LoadingFiles.Margin = new Padding(0);
            LoadingFiles.Name = "LoadingFiles";
            LoadingFiles.Size = new Size(65, 13);
            LoadingFiles.TabIndex = 5;
            // 
            // FilterComboBox
            // 
            FilterComboBox.Dock = DockStyle.Top;
            FilterComboBox.FlatStyle = FlatStyle.Flat;
            FilterComboBox.FormattingEnabled = true;
            FilterComboBox.Location = new Point(0, 0);
            FilterComboBox.Margin = new Padding(0, 0, 0, 1);
            FilterComboBox.Name = "FilterComboBox";
            FilterComboBox.Size = new Size(682, 21);
            FilterComboBox.TabIndex = 2;
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
            FilterWatermarkLabel.Location = new Point(4, 4);
            FilterWatermarkLabel.Name = "FilterWatermarkLabel";
            FilterWatermarkLabel.Size = new Size(184, 13);
            FilterWatermarkLabel.TabIndex = 0;
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
            lblSplitter.Location = new Point(0, 21);
            lblSplitter.Name = "lblSplitter";
            lblSplitter.Size = new Size(682, 2);
            lblSplitter.TabIndex = 3;
            // 
            // DeleteFilterButton
            // 
            DeleteFilterButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            DeleteFilterButton.BackColor = SystemColors.Window;
            DeleteFilterButton.FlatStyle = FlatStyle.Flat;
            DeleteFilterButton.Image = Properties.Resources.DeleteText;
            DeleteFilterButton.Location = new Point(646, 1);
            DeleteFilterButton.Name = "DeleteFilterButton";
            DeleteFilterButton.Size = new Size(18, 19);
            DeleteFilterButton.TabIndex = 1;
            DeleteFilterButton.Click += DeleteFilterButton_Click;
            // 
            // FileStatusList
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(DeleteFilterButton);
            Controls.Add(FilterWatermarkLabel);
            Controls.Add(NoFiles);
            Controls.Add(LoadingFiles);
            Controls.Add(FileStatusListView);
            Controls.Add(lblSplitter);
            Controls.Add(FilterComboBox);
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
    }
}
