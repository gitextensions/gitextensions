namespace GitUI
{
    partial class BranchComboBox
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
            this.branches = new System.Windows.Forms.ComboBox();
            this.selectMultipleBranchesButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // branches
            // 
            this.branches.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.branches.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.branches.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.branches.FormattingEnabled = true;
            this.branches.Location = new System.Drawing.Point(0, 0);
            this.branches.Name = "branches";
            this.branches.Size = new System.Drawing.Size(304, 23);
            this.branches.TabIndex = 0;
            // 
            // selectMultipleBranchesButton
            // 
            this.selectMultipleBranchesButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.selectMultipleBranchesButton.Image = global::GitUI.Properties.Images.Select;
            this.selectMultipleBranchesButton.Location = new System.Drawing.Point(308, 0);
            this.selectMultipleBranchesButton.Margin = new System.Windows.Forms.Padding(0);
            this.selectMultipleBranchesButton.Name = "selectMultipleBranchesButton";
            this.selectMultipleBranchesButton.Size = new System.Drawing.Size(23, 23);
            this.selectMultipleBranchesButton.TabIndex = 1;
            this.selectMultipleBranchesButton.UseVisualStyleBackColor = true;
            this.selectMultipleBranchesButton.Click += new System.EventHandler(this.selectMultipleBranchesButton_Click);
            // 
            // BranchComboBox
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.selectMultipleBranchesButton);
            this.Controls.Add(this.branches);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "BranchComboBox";
            this.Size = new System.Drawing.Size(331, 24);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox branches;
        private System.Windows.Forms.Button selectMultipleBranchesButton;
    }
}
