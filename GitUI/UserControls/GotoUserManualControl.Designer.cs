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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.pictureBoxHelpIcon = new System.Windows.Forms.PictureBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxHelpIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // linkLabelHelp
            // 
            this.linkLabelHelp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.linkLabelHelp.AutoSize = true;
            this.linkLabelHelp.Location = new System.Drawing.Point(22, 3);
            this.linkLabelHelp.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.linkLabelHelp.Name = "linkLabelHelp";
            this.linkLabelHelp.Size = new System.Drawing.Size(32, 15);
            this.linkLabelHelp.TabIndex = 0;
            this.linkLabelHelp.TabStop = true;
            this.linkLabelHelp.Text = "Help";
            this.linkLabelHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHelp_LinkClicked);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.pictureBoxHelpIcon);
            this.flowLayoutPanel1.Controls.Add(this.linkLabelHelp);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(70, 20);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // pictureBoxHelpIcon
            // 
            this.pictureBoxHelpIcon.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.pictureBoxHelpIcon.Image = global::GitUI.Properties.Images.GotoManual;
            this.pictureBoxHelpIcon.Location = new System.Drawing.Point(3, 3);
            this.pictureBoxHelpIcon.Name = "pictureBoxHelpIcon";
            this.pictureBoxHelpIcon.Size = new System.Drawing.Size(16, 16);
            this.pictureBoxHelpIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxHelpIcon.TabIndex = 2;
            this.pictureBoxHelpIcon.TabStop = false;
            // 
            // GotoUserManualControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.Controls.Add(this.flowLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(70, 20);
            this.Name = "GotoUserManualControl";
            this.Size = new System.Drawing.Size(70, 20);
            this.Load += new System.EventHandler(this.GotoUserManualControl_Load);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxHelpIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.LinkLabel linkLabelHelp;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.PictureBox pictureBoxHelpIcon;
    }
}
