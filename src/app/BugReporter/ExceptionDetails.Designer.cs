namespace BugReporter
{
    partial class ExceptionDetails
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
            exceptionDetailsListView = new ListView();
            propertyColumnHeader = ((ColumnHeader)(new ColumnHeader()));
            informationColumnHeader = ((ColumnHeader)(new ColumnHeader()));
            exceptionDetailsLabel = new Label();
            exceptionLabel = new Label();
            exceptionTreeView = new TreeView();
            toolTip = new ToolTip(components);
            SuspendLayout();
            // 
            // exceptionDetailsListView
            // 
            exceptionDetailsListView.Columns.AddRange(new ColumnHeader[] {
            propertyColumnHeader,
            informationColumnHeader});
            exceptionDetailsListView.Dock = DockStyle.Fill;
            exceptionDetailsListView.FullRowSelect = true;
            exceptionDetailsListView.GridLines = true;
            exceptionDetailsListView.Location = new Point(0, 126);
            exceptionDetailsListView.MultiSelect = false;
            exceptionDetailsListView.Name = "exceptionDetailsListView";
            exceptionDetailsListView.Size = new Size(461, 204);
            exceptionDetailsListView.TabIndex = 3;
            exceptionDetailsListView.UseCompatibleStateImageBehavior = false;
            exceptionDetailsListView.View = View.Details;
            exceptionDetailsListView.ItemMouseHover += ExceptionDetailsListView_ItemMouseHover;
            exceptionDetailsListView.DoubleClick += ExceptionDetailsListView_DoubleClick;
            // 
            // propertyColumnHeader
            // 
            propertyColumnHeader.Text = "Property";
            propertyColumnHeader.Width = 101;
            // 
            // informationColumnHeader
            // 
            informationColumnHeader.Text = "Information";
            informationColumnHeader.Width = 350;
            // 
            // exceptionDetailsLabel
            // 
            exceptionDetailsLabel.AutoSize = true;
            exceptionDetailsLabel.Dock = DockStyle.Top;
            exceptionDetailsLabel.Location = new Point(0, 85);
            exceptionDetailsLabel.Name = "exceptionDetailsLabel";
            exceptionDetailsLabel.Padding = new Padding(0, 16, 0, 0);
            exceptionDetailsLabel.Size = new Size(540, 41);
            exceptionDetailsLabel.TabIndex = 2;
            exceptionDetailsLabel.Text = "Exception Details (double click on items to see details):";
            // 
            // exceptionLabel
            // 
            exceptionLabel.AutoSize = true;
            exceptionLabel.Dock = DockStyle.Top;
            exceptionLabel.Location = new Point(0, 0);
            exceptionLabel.Name = "exceptionLabel";
            exceptionLabel.Size = new Size(138, 25);
            exceptionLabel.TabIndex = 0;
            exceptionLabel.Text = "Exception(s):";
            // 
            // exceptionTreeView
            // 
            exceptionTreeView.Dock = DockStyle.Top;
            exceptionTreeView.Location = new Point(0, 25);
            exceptionTreeView.Name = "exceptionTreeView";
            exceptionTreeView.Size = new Size(461, 60);
            exceptionTreeView.TabIndex = 1;
            exceptionTreeView.AfterSelect += ExceptionTreeView_AfterSelect;
            // 
            // ExceptionDetails
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(exceptionDetailsListView);
            Controls.Add(exceptionDetailsLabel);
            Controls.Add(exceptionTreeView);
            Controls.Add(exceptionLabel);
            Name = "ExceptionDetails";
            Size = new Size(461, 330);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private ListView exceptionDetailsListView;
        private ColumnHeader propertyColumnHeader;
        private ColumnHeader informationColumnHeader;
        private Label exceptionDetailsLabel;
        private Label exceptionLabel;
        private TreeView exceptionTreeView;
        private ToolTip toolTip;
    }
}
