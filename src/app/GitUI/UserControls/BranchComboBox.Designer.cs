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
            branches = new ComboBox();
            selectMultipleBranchesButton = new Button();
            SuspendLayout();
            // 
            // branches
            // 
            branches.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            branches.AutoCompleteMode = AutoCompleteMode.Suggest;
            branches.AutoCompleteSource = AutoCompleteSource.ListItems;
            branches.FormattingEnabled = true;
            branches.Location = new Point(0, 3);
            branches.Name = "branches";
            branches.Size = new Size(304, 23);
            branches.TabIndex = 0;
            branches.SelectedValueChanged += branches_SelectedValueChanged;
            // 
            // selectMultipleBranchesButton
            // 
            selectMultipleBranchesButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            selectMultipleBranchesButton.Image = Properties.Images.Select;
            selectMultipleBranchesButton.Location = new Point(308, 1);
            selectMultipleBranchesButton.Margin = new Padding(0);
            selectMultipleBranchesButton.Name = "selectMultipleBranchesButton";
            selectMultipleBranchesButton.TabIndex = 1;
            selectMultipleBranchesButton.Size = new Size(23, 23);
            selectMultipleBranchesButton.UseVisualStyleBackColor = true;
            selectMultipleBranchesButton.Click += selectMultipleBranchesButton_Click;
            // 
            // BranchComboBox
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(branches);
            Controls.Add(selectMultipleBranchesButton);
            Margin = new Padding(0);
            Name = "BranchComboBox";
            Size = new Size(331, 26);
            ResumeLayout(false);

        }

        #endregion

        private ComboBox branches;
        private Button selectMultipleBranchesButton;
    }
}
