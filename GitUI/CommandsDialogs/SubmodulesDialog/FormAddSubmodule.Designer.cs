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
            if (disposing && (components != null))
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
            this.Add = new System.Windows.Forms.Button();
            this.Browse = new System.Windows.Forms.Button();
            this.Directory = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.LocalPath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Branch = new System.Windows.Forms.ComboBox();
            this.chkForce = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // Add
            // 
            this.Add.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Add.Location = new System.Drawing.Point(378, 96);
            this.Add.Name = "Add";
            this.Add.Size = new System.Drawing.Size(102, 25);
            this.Add.TabIndex = 8;
            this.Add.Text = "Add";
            this.Add.UseVisualStyleBackColor = true;
            this.Add.Click += new System.EventHandler(this.AddClick);
            // 
            // Browse
            // 
            this.Browse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Browse.Location = new System.Drawing.Point(378, 12);
            this.Browse.Name = "Browse";
            this.Browse.Size = new System.Drawing.Size(102, 25);
            this.Browse.TabIndex = 2;
            this.Browse.Text = "Browse";
            this.Browse.UseVisualStyleBackColor = true;
            this.Browse.Click += new System.EventHandler(this.BrowseClick);
            // 
            // Directory
            // 
            this.Directory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Directory.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.Directory.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.Directory.FormattingEnabled = true;
            this.Directory.Location = new System.Drawing.Point(144, 14);
            this.Directory.Name = "Directory";
            this.Directory.Size = new System.Drawing.Size(228, 23);
            this.Directory.TabIndex = 1;
            this.Directory.SelectedIndexChanged += new System.EventHandler(this.DirectorySelectedIndexChanged);
            this.Directory.TextUpdate += new System.EventHandler(this.DirectoryTextUpdate);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Path to submodule";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Local path";
            // 
            // LocalPath
            // 
            this.LocalPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LocalPath.Location = new System.Drawing.Point(144, 41);
            this.LocalPath.Name = "LocalPath";
            this.LocalPath.Size = new System.Drawing.Size(228, 23);
            this.LocalPath.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "Branch";
            // 
            // Branch
            // 
            this.Branch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Branch.FormattingEnabled = true;
            this.Branch.Location = new System.Drawing.Point(144, 68);
            this.Branch.Name = "Branch";
            this.Branch.Size = new System.Drawing.Size(228, 23);
            this.Branch.TabIndex = 6;
            this.Branch.DropDown += new System.EventHandler(this.BranchDropDown);
            // 
            // chkForce
            // 
            this.chkForce.AutoSize = true;
            this.chkForce.Location = new System.Drawing.Point(8, 100);
            this.chkForce.Name = "chkForce";
            this.chkForce.Size = new System.Drawing.Size(55, 19);
            this.chkForce.TabIndex = 7;
            this.chkForce.Text = "Force";
            this.chkForce.UseVisualStyleBackColor = true;
            // 
            // FormAddSubmodule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(492, 131);
            this.Controls.Add(this.chkForce);
            this.Controls.Add(this.Branch);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.LocalPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Add);
            this.Controls.Add(this.Browse);
            this.Controls.Add(this.Directory);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAddSubmodule";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add submodule";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Add;
        private System.Windows.Forms.Button Browse;
        private System.Windows.Forms.ComboBox Directory;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox LocalPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox Branch;
        private System.Windows.Forms.CheckBox chkForce;
    }
}