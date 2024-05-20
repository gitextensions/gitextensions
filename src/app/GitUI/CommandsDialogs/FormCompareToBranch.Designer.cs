namespace GitUI.CommandsDialogs
{
    partial class FormCompareToBranch
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
            branchSelector = new GitUI.UserControls.BranchSelector();
            btnCompare = new Button();
            SuspendLayout();
            // 
            // branchSelector
            // 
            branchSelector.AutoSize = true;
            branchSelector.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            branchSelector.Location = new Point(8, 8);
            branchSelector.Margin = new Padding(8);
            branchSelector.Name = "branchSelector";
            branchSelector.Size = new Size(325, 54);
            branchSelector.TabIndex = 0;
            // 
            // btnCompare
            // 
            btnCompare.Anchor = AnchorStyles.Right;
            btnCompare.AutoSize = true;
            btnCompare.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCompare.Location = new Point(358, 80);
            btnCompare.Name = "btnCompare";
            btnCompare.Size = new Size(60, 23);
            btnCompare.TabIndex = 1;
            btnCompare.Text = "Compare";
            btnCompare.UseVisualStyleBackColor = true;
            btnCompare.Click += btnCompare_Click;
            // 
            // FormCompareToBranch
            // 
            AcceptButton = btnCompare;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(434, 110);
            Controls.Add(btnCompare);
            Controls.Add(branchSelector);
            Name = "FormCompareToBranch";
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "Compare to branch";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private UserControls.BranchSelector branchSelector;
        private Button btnCompare;
    }
}