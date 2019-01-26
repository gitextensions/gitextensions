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
            this.components = new System.ComponentModel.Container();
            this.FileStatusListView = new GitUI.UserControls.ExListView();
            this.columnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NoFiles = new System.Windows.Forms.Label();
            this.FilterComboBox = new System.Windows.Forms.ComboBox();
            this.FilterWatermarkLabel = new System.Windows.Forms.Label();
            this.FilterToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.lblSplitter = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // FileStatusListView
            // 
            this.FileStatusListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FileStatusListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.FileStatusListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader});
            this.FileStatusListView.FullRowSelect = true;
            this.FileStatusListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.FileStatusListView.HideSelection = false;
            this.FileStatusListView.LabelWrap = false;
            this.FileStatusListView.Location = new System.Drawing.Point(0, 21);
            this.FileStatusListView.Margin = new System.Windows.Forms.Padding(0);
            this.FileStatusListView.Name = "FileStatusListView";
            this.FileStatusListView.OwnerDraw = true;
            this.FileStatusListView.ShowItemToolTips = true;
            this.FileStatusListView.Size = new System.Drawing.Size(682, 464);
            this.FileStatusListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.FileStatusListView.TabIndex = 2;
            this.FileStatusListView.UseCompatibleStateImageBehavior = false;
            this.FileStatusListView.View = System.Windows.Forms.View.Details;
            this.FileStatusListView.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.FileStatusListView_DrawSubItem);
            this.FileStatusListView.DoubleClick += new System.EventHandler(this.FileStatusListView_DoubleClick);
            this.FileStatusListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FileStatusListView_KeyDown);
            this.FileStatusListView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FileStatusListView_MouseDown);
            this.FileStatusListView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FileStatusListView_MouseMove);
            this.FileStatusListView.GroupMouseDown += new System.EventHandler<GitUI.UserControls.ListViewGroupMouseEventArgs>(this.FileStatusListView_GroupMouseDown);
            this.FileStatusListView.ClientSizeChanged += new System.EventHandler(this.FileStatusListView_ClientSizeChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader.Text = "Files";
            this.columnHeader.Width = 678;
            // 
            // NoFiles
            // 
            this.NoFiles.AutoSize = true;
            this.NoFiles.BackColor = System.Drawing.SystemColors.Window;
            this.NoFiles.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.NoFiles.Location = new System.Drawing.Point(4, 4);
            this.NoFiles.Margin = new System.Windows.Forms.Padding(0);
            this.NoFiles.Name = "NoFiles";
            this.NoFiles.Size = new System.Drawing.Size(63, 13);
            this.NoFiles.TabIndex = 1;
            this.NoFiles.Text = "No changes";
            // 
            // FilterComboBox
            // 
            this.FilterComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.FilterComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FilterComboBox.FormattingEnabled = true;
            this.FilterComboBox.Location = new System.Drawing.Point(0, 0);
            this.FilterComboBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.FilterComboBox.Name = "FilterComboBox";
            this.FilterComboBox.Size = new System.Drawing.Size(682, 21);
            this.FilterComboBox.TabIndex = 0;
            this.FilterComboBox.SelectedIndexChanged += new System.EventHandler(this.FilterComboBox_SelectedIndexChanged);
            this.FilterComboBox.TextUpdate += new System.EventHandler(this.FilterComboBox_TextUpdate);
            this.FilterComboBox.GotFocus += new System.EventHandler(this.FilterComboBox_GotFocus);
            this.FilterComboBox.LostFocus += new System.EventHandler(this.FilterComboBox_LostFocus);
            this.FilterComboBox.MouseEnter += new System.EventHandler(this.FilterComboBox_MouseEnter);
            this.FilterComboBox.SizeChanged += new System.EventHandler(this.FilterComboBox_SizeChanged);
            // 
            // FilterWatermarkLabel
            // 
            this.FilterWatermarkLabel.AutoSize = true;
            this.FilterWatermarkLabel.BackColor = System.Drawing.Color.White;
            this.FilterWatermarkLabel.ForeColor = System.Drawing.Color.LightGray;
            this.FilterWatermarkLabel.Location = new System.Drawing.Point(4, 4);
            this.FilterWatermarkLabel.Name = "FilterWatermarkLabel";
            this.FilterWatermarkLabel.Size = new System.Drawing.Size(65, 13);
            this.FilterWatermarkLabel.TabIndex = 0;
            this.FilterWatermarkLabel.Text = "Filter files...";
            this.FilterWatermarkLabel.Click += new System.EventHandler(this.FilterWatermarkLabel_Click);
            // 
            // FilterToolTip
            // 
            this.FilterToolTip.AutomaticDelay = 100;
            this.FilterToolTip.ShowAlways = true;
            this.FilterToolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Error;
            this.FilterToolTip.ToolTipTitle = "RegEx";
            this.FilterToolTip.UseAnimation = false;
            this.FilterToolTip.UseFading = false;
            // 
            // lblSplitter
            // 
            this.lblSplitter.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSplitter.Location = new System.Drawing.Point(0, 22);
            this.lblSplitter.Name = "lblSplitter";
            this.lblSplitter.Size = new System.Drawing.Size(682, 2);
            this.lblSplitter.TabIndex = 1;
            // 
            // FileStatusList
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.FilterWatermarkLabel);
            this.Controls.Add(this.NoFiles);
            this.Controls.Add(this.FileStatusListView);
            this.Controls.Add(this.lblSplitter);
            this.Controls.Add(this.FilterComboBox);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FileStatusList";
            this.Size = new System.Drawing.Size(682, 485);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GitUI.UserControls.ExListView FileStatusListView;
        private System.Windows.Forms.Label NoFiles;
        private System.Windows.Forms.ColumnHeader columnHeader;
        private System.Windows.Forms.ComboBox FilterComboBox;
        private System.Windows.Forms.Label FilterWatermarkLabel;
        private System.Windows.Forms.ToolTip FilterToolTip;
        private System.Windows.Forms.Label lblSplitter;
    }
}
