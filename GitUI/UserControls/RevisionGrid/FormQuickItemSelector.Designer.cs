namespace GitUI.UserControls.RevisionGrid
{
    partial class FormQuickItemSelector
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
            this.lbxRefs = new System.Windows.Forms.ListBox();
            this.btnAction = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbxRefs
            // 
            this.lbxRefs.FormattingEnabled = true;
            this.lbxRefs.Items.AddRange(new object[] {
            "local1 (ref)",
            "local2 (ref)",
            "tag1 (tag)",
            "tag2 (tag)",
            "1",
            "2",
            "3",
            "4"});
            this.lbxRefs.Location = new System.Drawing.Point(2, 2);
            this.lbxRefs.Name = "lbxRefs";
            this.lbxRefs.Size = new System.Drawing.Size(275, 108);
            this.lbxRefs.TabIndex = 0;
            this.lbxRefs.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbxRefs_MouseDoubleClick);
            // 
            // btnAction
            // 
            this.btnAction.AutoSize = true;
            this.btnAction.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAction.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAction.Location = new System.Drawing.Point(197, 3);
            this.btnAction.MinimumSize = new System.Drawing.Size(75, 0);
            this.btnAction.Name = "btnAction";
            this.btnAction.Size = new System.Drawing.Size(75, 23);
            this.btnAction.TabIndex = 1;
            this.btnAction.Text = "Action";
            this.btnAction.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.btnAction);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(2, 117);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(275, 29);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // FormQuickGitRefSelector
            // 
            this.AcceptButton = this.btnAction;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(279, 146);
            this.Controls.Add(this.lbxRefs);
            this.Controls.Add(this.flowLayoutPanel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormQuickGitRefSelector";
            this.Padding = new System.Windows.Forms.Padding(2, 2, 2, 0);
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbxRefs;
        private System.Windows.Forms.Button btnAction;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
