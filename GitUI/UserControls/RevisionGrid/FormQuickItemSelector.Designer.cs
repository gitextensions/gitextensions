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
            lbxRefs = new ListBox();
            btnAction = new Button();
            flowLayoutPanel1 = new FlowLayoutPanel();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // lbxRefs
            // 
            lbxRefs.FormattingEnabled = true;
            lbxRefs.Items.AddRange(new object[] {
            "local1 (ref)",
            "local2 (ref)",
            "tag1 (tag)",
            "tag2 (tag)",
            "1",
            "2",
            "3",
            "4"});
            lbxRefs.Location = new Point(2, 2);
            lbxRefs.Name = "lbxRefs";
            lbxRefs.Size = new Size(275, 108);
            lbxRefs.TabIndex = 0;
            lbxRefs.MouseDoubleClick += lbxRefs_MouseDoubleClick;
            // 
            // btnAction
            // 
            btnAction.AutoSize = true;
            btnAction.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnAction.DialogResult = DialogResult.OK;
            btnAction.Location = new Point(197, 3);
            btnAction.MinimumSize = new Size(75, 0);
            btnAction.Name = "btnAction";
            btnAction.Size = new Size(75, 23);
            btnAction.TabIndex = 1;
            btnAction.Text = "Action";
            btnAction.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(btnAction);
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(2, 117);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(275, 29);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // FormQuickGitRefSelector
            // 
            AcceptButton = btnAction;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(279, 146);
            Controls.Add(lbxRefs);
            Controls.Add(flowLayoutPanel1);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Name = "FormQuickGitRefSelector";
            Padding = new Padding(2, 2, 2, 0);
            StartPosition = FormStartPosition.Manual;
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private ListBox lbxRefs;
        private Button btnAction;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}
