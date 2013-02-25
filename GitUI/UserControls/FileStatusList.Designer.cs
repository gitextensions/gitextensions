using GitUI.UserControls;
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
            this.FileStatusListView = new GitUI.UserControls.ExListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NoFiles = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // FileStatusListView
            // 
            this.FileStatusListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.FileStatusListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileStatusListView.FullRowSelect = true;
            this.FileStatusListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.FileStatusListView.HideSelection = false;
            this.FileStatusListView.LabelWrap = false;
            this.FileStatusListView.Location = new System.Drawing.Point(0, 0);
            this.FileStatusListView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.FileStatusListView.Name = "FileStatusListView";
            this.FileStatusListView.OwnerDraw = true;
            this.FileStatusListView.ShowItemToolTips = true;
            this.FileStatusListView.Size = new System.Drawing.Size(682, 485);
            this.FileStatusListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.FileStatusListView.TabIndex = 0;
            this.FileStatusListView.UseCompatibleStateImageBehavior = false;
            this.FileStatusListView.View = System.Windows.Forms.View.Details;
            this.FileStatusListView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.FileStatusListView_DrawItem);
            this.FileStatusListView.SelectedIndexChanged += new System.EventHandler(this.FileStatusListView_SelectedIndexChanged);
            this.FileStatusListView.SizeChanged += new System.EventHandler(this.FileStatusListView_SizeChanged);
            this.FileStatusListView.DoubleClick += new System.EventHandler(this.FileStatusListView_DoubleClick);
            this.FileStatusListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FileStatusListView_KeyDown);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Files";
            this.columnHeader1.Width = 678;
            // 
            // NoFiles
            // 
            this.NoFiles.BackColor = System.Drawing.SystemColors.Window;
            this.NoFiles.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.NoFiles.Location = new System.Drawing.Point(6, 6);
            this.NoFiles.Margin = new System.Windows.Forms.Padding(0);
            this.NoFiles.Name = "NoFiles";
            this.NoFiles.Size = new System.Drawing.Size(201, 56);
            this.NoFiles.TabIndex = 1;
            this.NoFiles.Text = "No changes";
            // 
            // FileStatusList
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.NoFiles);
            this.Controls.Add(this.FileStatusListView);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FileStatusList";
            this.Size = new System.Drawing.Size(682, 485);
            this.ResumeLayout(false);

        }

        #endregion

        private ExListView FileStatusListView;
        //This property cannot be private because this will break compilation in monodevelop
        private System.Windows.Forms.Label NoFiles;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}
