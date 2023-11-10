namespace GitUI.UserControls
{
    partial class BranchSelector
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
            tableLayoutPanel1 = new TableLayoutPanel();
            LocalBranch = new RadioButton();
            Remotebranch = new RadioButton();
            label1 = new Label();
            Branches = new ComboBox();
            lbChanges = new Label();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(LocalBranch, 0, 0);
            tableLayoutPanel1.Controls.Add(Remotebranch, 1, 0);
            tableLayoutPanel1.Controls.Add(label1, 0, 1);
            tableLayoutPanel1.Controls.Add(Branches, 1, 1);
            tableLayoutPanel1.Controls.Add(lbChanges, 2, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(325, 54);
            tableLayoutPanel1.TabIndex = 1;
            tableLayoutPanel1.TabStop = true;
            // 
            // LocalBranch
            // 
            LocalBranch.Anchor = AnchorStyles.None;
            LocalBranch.AutoSize = true;
            LocalBranch.Checked = true;
            LocalBranch.Location = new Point(0, 2);
            LocalBranch.Margin = new Padding(0);
            LocalBranch.Name = "LocalBranch";
            LocalBranch.Size = new Size(85, 17);
            LocalBranch.TabIndex = 0;
            LocalBranch.TabStop = true;
            LocalBranch.Text = "Local branch";
            LocalBranch.UseVisualStyleBackColor = true;
            LocalBranch.CheckedChanged += LocalBranch_CheckedChanged;
            // 
            // Remotebranch
            // 
            Remotebranch.Anchor = AnchorStyles.Left;
            Remotebranch.AutoSize = true;
            Remotebranch.Location = new Point(87, 2);
            Remotebranch.Margin = new Padding(2);
            Remotebranch.Name = "Remotebranch";
            Remotebranch.Padding = new Padding(6, 0, 0, 0);
            Remotebranch.Size = new Size(104, 17);
            Remotebranch.TabIndex = 0;
            Remotebranch.Text = "Remote branch";
            Remotebranch.UseVisualStyleBackColor = true;
            Remotebranch.CheckedChanged += Remotebranch_CheckedChanged;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(10, 36);
            label1.Margin = new Padding(2, 10, 3, 0);
            label1.Name = "label1";
            label1.Size = new Size(72, 13);
            label1.TabIndex = 7;
            label1.Text = "Select branch";
            // 
            // Branches
            // 
            Branches.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            Branches.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            Branches.AutoCompleteSource = AutoCompleteSource.ListItems;
            Branches.FormattingEnabled = true;
            Branches.Location = new Point(87, 31);
            Branches.Margin = new Padding(2, 10, 6, 2);
            Branches.Name = "Branches";
            Branches.Size = new Size(214, 21);
            Branches.TabIndex = 1;
            // 
            // lbChanges
            // 
            lbChanges.AutoSize = true;
            lbChanges.ForeColor = SystemColors.GrayText;
            lbChanges.Location = new Point(309, 33);
            lbChanges.Margin = new Padding(2, 12, 5, 2);
            lbChanges.Name = "lbChanges";
            lbChanges.Size = new Size(11, 13);
            lbChanges.TabIndex = 30;
            lbChanges.Text = "-";
            // 
            // BranchSelector
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(tableLayoutPanel1);
            Name = "BranchSelector";
            Size = new Size(325, 54);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private RadioButton LocalBranch;
        private RadioButton Remotebranch;
        private Label label1;
        private ComboBox Branches;
        private Label lbChanges;
    }
}
