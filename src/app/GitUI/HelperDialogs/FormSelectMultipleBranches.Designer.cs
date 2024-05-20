namespace GitUI.HelperDialogs
{
    partial class FormSelectMultipleBranches
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
            Branches = new CheckedListBox();
            okButton = new Button();
            selectBranchesLabel = new Label();
            SuspendLayout();
            // 
            // Branches
            // 
            Branches.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Branches.CheckOnClick = true;
            Branches.ColumnWidth = 250;
            Branches.FormattingEnabled = true;
            Branches.Location = new Point(12, 32);
            Branches.Name = "Branches";
            Branches.Size = new Size(239, 166);
            Branches.TabIndex = 2;
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            okButton.Location = new Point(176, 223);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 23);
            okButton.TabIndex = 3;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += okButton_Click;
            // 
            // selectBranchesLabel
            // 
            selectBranchesLabel.AutoSize = true;
            selectBranchesLabel.Location = new Point(12, 9);
            selectBranchesLabel.Name = "selectBranchesLabel";
            selectBranchesLabel.Size = new Size(89, 15);
            selectBranchesLabel.TabIndex = 4;
            selectBranchesLabel.Text = "Select branches";
            // 
            // FormSelectMultipleBranches
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(263, 252);
            Controls.Add(selectBranchesLabel);
            Controls.Add(okButton);
            Controls.Add(Branches);
            MinimumSize = new Size(200, 200);
            Name = "FormSelectMultipleBranches";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Select multiple branches";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private CheckedListBox Branches;
        private Button okButton;
        private Label selectBranchesLabel;
    }
}