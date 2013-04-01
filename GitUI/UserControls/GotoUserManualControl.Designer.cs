namespace GitUI.UserControls
{
    partial class GotoUserManualControl
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
            this.components = new System.ComponentModel.Container();
            this.linkLabelHelp = new System.Windows.Forms.LinkLabel();
            this.labelHelpIcon = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // linkLabelHelp
            // 
            this.linkLabelHelp.AutoSize = true;
            this.linkLabelHelp.Location = new System.Drawing.Point(22, 0);
            this.linkLabelHelp.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.linkLabelHelp.Name = "linkLabelHelp";
            this.linkLabelHelp.Size = new System.Drawing.Size(32, 15);
            this.linkLabelHelp.TabIndex = 0;
            this.linkLabelHelp.TabStop = true;
            this.linkLabelHelp.Text = "Help";
            this.linkLabelHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHelp_LinkClicked);
            // 
            // labelHelpIcon
            // 
            this.labelHelpIcon.AutoSize = true;
            this.labelHelpIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelHelpIcon.Image = global::GitUI.Properties.Resources.IconGotoManual;
            this.labelHelpIcon.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelHelpIcon.Location = new System.Drawing.Point(0, 0);
            this.labelHelpIcon.Margin = new System.Windows.Forms.Padding(0);
            this.labelHelpIcon.Name = "labelHelpIcon";
            this.labelHelpIcon.Size = new System.Drawing.Size(22, 15);
            this.labelHelpIcon.TabIndex = 1;
            this.labelHelpIcon.Text = "     ";
            this.labelHelpIcon.Click += new System.EventHandler(this.labelHelpIcon_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.labelHelpIcon);
            this.flowLayoutPanel1.Controls.Add(this.linkLabelHelp);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(60, 18);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // GotoUserManualControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "GotoUserManualControl";
            this.Size = new System.Drawing.Size(60, 18);
            this.Load += new System.EventHandler(this.GotoUserManualControl_Load);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.LinkLabel linkLabelHelp;
        private System.Windows.Forms.Label labelHelpIcon;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
