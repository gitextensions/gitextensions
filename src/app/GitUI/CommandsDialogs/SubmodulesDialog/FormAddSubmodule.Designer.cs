namespace GitUI.CommandsDialogs.SubmodulesDialog
{
    partial class FormAddSubmodule
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
            Add = new Button();
            Browse = new Button();
            Directory = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            LocalPath = new TextBox();
            label3 = new Label();
            Branch = new ComboBox();
            chkForce = new CheckBox();
            SuspendLayout();
            // 
            // Add
            // 
            Add.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Add.Location = new Point(378, 96);
            Add.Name = "Add";
            Add.Size = new Size(102, 25);
            Add.TabIndex = 8;
            Add.Text = "Add";
            Add.UseVisualStyleBackColor = true;
            Add.Click += AddClick;
            // 
            // Browse
            // 
            Browse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Browse.Location = new Point(378, 12);
            Browse.Name = "Browse";
            Browse.Size = new Size(102, 25);
            Browse.TabIndex = 2;
            Browse.Text = "Browse";
            Browse.UseVisualStyleBackColor = true;
            Browse.Click += BrowseClick;
            // 
            // Directory
            // 
            Directory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Directory.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            Directory.AutoCompleteSource = AutoCompleteSource.ListItems;
            Directory.FormattingEnabled = true;
            Directory.Location = new Point(144, 14);
            Directory.Name = "Directory";
            Directory.Size = new Size(228, 23);
            Directory.TabIndex = 1;
            Directory.SelectedIndexChanged += DirectorySelectedIndexChanged;
            Directory.TextUpdate += DirectoryTextUpdate;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(5, 17);
            label1.Name = "label1";
            label1.Size = new Size(108, 15);
            label1.TabIndex = 0;
            label1.Text = "Path to submodule";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(5, 44);
            label2.Name = "label2";
            label2.Size = new Size(62, 15);
            label2.TabIndex = 3;
            label2.Text = "Local path";
            // 
            // LocalPath
            // 
            LocalPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            LocalPath.Location = new Point(144, 41);
            LocalPath.Name = "LocalPath";
            LocalPath.Size = new Size(228, 23);
            LocalPath.TabIndex = 4;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(5, 71);
            label3.Name = "label3";
            label3.Size = new Size(44, 15);
            label3.TabIndex = 5;
            label3.Text = "Branch";
            // 
            // Branch
            // 
            Branch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Branch.FormattingEnabled = true;
            Branch.Location = new Point(144, 68);
            Branch.Name = "Branch";
            Branch.Size = new Size(228, 23);
            Branch.TabIndex = 6;
            Branch.DropDown += BranchDropDown;
            // 
            // chkForce
            // 
            chkForce.AutoSize = true;
            chkForce.Location = new Point(8, 100);
            chkForce.Name = "chkForce";
            chkForce.Size = new Size(55, 19);
            chkForce.TabIndex = 7;
            chkForce.Text = "Force";
            chkForce.UseVisualStyleBackColor = true;
            // 
            // FormAddSubmodule
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(492, 131);
            Controls.Add(chkForce);
            Controls.Add(Branch);
            Controls.Add(label3);
            Controls.Add(LocalPath);
            Controls.Add(label2);
            Controls.Add(Add);
            Controls.Add(Browse);
            Controls.Add(Directory);
            Controls.Add(label1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormAddSubmodule";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Add submodule";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button Add;
        private Button Browse;
        private ComboBox Directory;
        private Label label1;
        private Label label2;
        private TextBox LocalPath;
        private Label label3;
        private ComboBox Branch;
        private CheckBox chkForce;
    }
}