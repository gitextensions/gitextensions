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
            this.branchSelector = new GitUI.UserControls.BranchSelector();
            this.btnCompare = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // branchSelector
            // 
            this.branchSelector.AutoSize = true;
            this.branchSelector.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.branchSelector.Location = new System.Drawing.Point(8, 8);
            this.branchSelector.Margin = new System.Windows.Forms.Padding(8);
            this.branchSelector.Name = "branchSelector";
            this.branchSelector.Size = new System.Drawing.Size(325, 54);
            this.branchSelector.TabIndex = 0;
            // 
            // btnCompare
            // 
            this.btnCompare.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnCompare.AutoSize = true;
            this.btnCompare.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCompare.Location = new System.Drawing.Point(358, 80);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.Size = new System.Drawing.Size(60, 23);
            this.btnCompare.TabIndex = 1;
            this.btnCompare.Text = "Compare";
            this.btnCompare.UseVisualStyleBackColor = true;
            this.btnCompare.Click += new System.EventHandler(this.btnCompare_Click);
            // 
            // FormCompareToBranch
            // 
            this.AcceptButton = this.btnCompare;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(434, 110);
            this.Controls.Add(this.btnCompare);
            this.Controls.Add(this.branchSelector);
            this.Name = "FormCompareToBranch";
            this.Text = "Compare to branch";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private UserControls.BranchSelector branchSelector;
        private System.Windows.Forms.Button btnCompare;
    }
}