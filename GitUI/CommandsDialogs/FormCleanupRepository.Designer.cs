namespace GitUI.CommandsDialogs
{
    partial class FormCleanupRepository
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
            this.Preview = new System.Windows.Forms.Button();
            this.Cleanup = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.RemoveIgnored = new System.Windows.Forms.RadioButton();
            this.RemoveNonIgnored = new System.Windows.Forms.RadioButton();
            this.RemoveAll = new System.Windows.Forms.RadioButton();
            this.RemoveDirectories = new System.Windows.Forms.CheckBox();
            this.PreviewOutput = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxPaths = new System.Windows.Forms.TextBox();
            this.checkBoxPathFilter = new System.Windows.Forms.CheckBox();
            this.labelPathHint = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            //
            // Preview
            //
            this.Preview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Preview.Image = global::GitUI.Properties.Images.Preview;
            this.Preview.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Preview.Location = new System.Drawing.Point(50, 278);
            this.Preview.Name = "Preview";
            this.Preview.Size = new System.Drawing.Size(120, 25);
            this.Preview.TabIndex = 0;
            this.Preview.Text = "Preview";
            this.Preview.UseVisualStyleBackColor = true;
            this.Preview.Click += new System.EventHandler(this.Preview_Click);
            //
            // Cleanup
            //
            this.Cleanup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Cleanup.Image = global::GitUI.Properties.Images.CleanupRepo;
            this.Cleanup.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Cleanup.Location = new System.Drawing.Point(176, 278);
            this.Cleanup.Name = "Cleanup";
            this.Cleanup.Size = new System.Drawing.Size(120, 25);
            this.Cleanup.TabIndex = 1;
            this.Cleanup.Text = "Cleanup";
            this.Cleanup.UseVisualStyleBackColor = true;
            this.Cleanup.Click += new System.EventHandler(this.Cleanup_Click);
            //
            // Cancel
            //
            this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(302, 278);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(120, 25);
            this.Cancel.TabIndex = 2;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            //
            // groupBox1
            //
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.RemoveIgnored);
            this.groupBox1.Controls.Add(this.RemoveNonIgnored);
            this.groupBox1.Controls.Add(this.RemoveAll);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(410, 100);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Remove untracked files from working directory";
            //
            // RemoveIgnored
            //
            this.RemoveIgnored.AutoSize = true;
            this.RemoveIgnored.Location = new System.Drawing.Point(7, 67);
            this.RemoveIgnored.Name = "RemoveIgnored";
            this.RemoveIgnored.Size = new System.Drawing.Size(218, 19);
            this.RemoveIgnored.TabIndex = 2;
            this.RemoveIgnored.Text = "Remove only ignored untracked files";
            this.RemoveIgnored.UseVisualStyleBackColor = true;
            //
            // RemoveNonIgnored
            //
            this.RemoveNonIgnored.AutoSize = true;
            this.RemoveNonIgnored.Location = new System.Drawing.Point(7, 43);
            this.RemoveNonIgnored.Name = "RemoveNonIgnored";
            this.RemoveNonIgnored.Size = new System.Drawing.Size(244, 19);
            this.RemoveNonIgnored.TabIndex = 1;
            this.RemoveNonIgnored.Text = "Remove only non-ignored untracked files";
            this.RemoveNonIgnored.UseVisualStyleBackColor = true;
            //
            // RemoveAll
            //
            this.RemoveAll.AutoSize = true;
            this.RemoveAll.Checked = true;
            this.RemoveAll.Location = new System.Drawing.Point(7, 20);
            this.RemoveAll.Name = "RemoveAll";
            this.RemoveAll.Size = new System.Drawing.Size(163, 19);
            this.RemoveAll.TabIndex = 0;
            this.RemoveAll.TabStop = true;
            this.RemoveAll.Text = "Remove all untracked files";
            this.RemoveAll.UseVisualStyleBackColor = true;
            //
            // RemoveDirectories
            //
            this.RemoveDirectories.AutoSize = true;
            this.RemoveDirectories.Checked = true;
            this.RemoveDirectories.CheckState = System.Windows.Forms.CheckState.Checked;
            this.RemoveDirectories.Location = new System.Drawing.Point(19, 119);
            this.RemoveDirectories.Name = "RemoveDirectories";
            this.RemoveDirectories.Size = new System.Drawing.Size(183, 19);
            this.RemoveDirectories.TabIndex = 4;
            this.RemoveDirectories.Text = "Remove untracked directories";
            this.RemoveDirectories.UseVisualStyleBackColor = true;
            //
            // PreviewOutput
            //
            this.PreviewOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PreviewOutput.Location = new System.Drawing.Point(12, 343);
            this.PreviewOutput.Multiline = true;
            this.PreviewOutput.Name = "PreviewOutput";
            this.PreviewOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.PreviewOutput.Size = new System.Drawing.Size(410, 87);
            this.PreviewOutput.TabIndex = 5;
            this.PreviewOutput.WordWrap = false;
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 325);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Log:";
            //
            // textBoxPaths
            //
            this.textBoxPaths.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPaths.Location = new System.Drawing.Point(48, 169);
            this.textBoxPaths.Multiline = true;
            this.textBoxPaths.Name = "textBoxPaths";
            this.textBoxPaths.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxPaths.Size = new System.Drawing.Size(374, 63);
            this.textBoxPaths.TabIndex = 1;
            //
            // checkBoxPathFilter
            //
            this.checkBoxPathFilter.AutoSize = true;
            this.checkBoxPathFilter.Location = new System.Drawing.Point(19, 144);
            this.checkBoxPathFilter.Name = "checkBoxPathFilter";
            this.checkBoxPathFilter.Size = new System.Drawing.Size(200, 19);
            this.checkBoxPathFilter.TabIndex = 0;
            this.checkBoxPathFilter.Text = "Affect the following path(s) only:";
            this.checkBoxPathFilter.UseVisualStyleBackColor = true;
            this.checkBoxPathFilter.CheckedChanged += new System.EventHandler(this.checkBoxPathFilter_CheckedChanged);
            //
            // labelPathHint
            //
            this.labelPathHint.AutoSize = true;
            this.labelPathHint.Location = new System.Drawing.Point(50, 236);
            this.labelPathHint.Name = "labelPathHint";
            this.labelPathHint.Size = new System.Drawing.Size(104, 15);
            this.labelPathHint.TabIndex = 7;
            this.labelPathHint.Text = "(one path per line)";
            //
            // FormCleanupRepository
            //
            this.AcceptButton = this.Preview;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(434, 442);
            this.Controls.Add(this.labelPathHint);
            this.Controls.Add(this.textBoxPaths);
            this.Controls.Add(this.checkBoxPathFilter);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PreviewOutput);
            this.Controls.Add(this.RemoveDirectories);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Cleanup);
            this.Controls.Add(this.Preview);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(450, 480);
            this.Name = "FormCleanupRepository";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Clean working directory";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Preview;
        private System.Windows.Forms.Button Cleanup;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton RemoveIgnored;
        private System.Windows.Forms.RadioButton RemoveNonIgnored;
        private System.Windows.Forms.RadioButton RemoveAll;
        private System.Windows.Forms.CheckBox RemoveDirectories;
        private System.Windows.Forms.TextBox PreviewOutput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxPaths;
        private System.Windows.Forms.CheckBox checkBoxPathFilter;
        private System.Windows.Forms.Label labelPathHint;
    }
}