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
            components = new System.ComponentModel.Container();
            linkLabelHelp = new LinkLabel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            pictureBoxHelpIcon = new PictureBox();
            toolTip1 = new ToolTip(components);
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(pictureBoxHelpIcon)).BeginInit();
            SuspendLayout();
            // 
            // linkLabelHelp
            // 
            linkLabelHelp.Anchor = AnchorStyles.Left;
            linkLabelHelp.AutoSize = true;
            linkLabelHelp.Location = new Point(22, 3);
            linkLabelHelp.Margin = new Padding(0, 0, 3, 0);
            linkLabelHelp.Name = "linkLabelHelp";
            linkLabelHelp.Size = new Size(32, 15);
            linkLabelHelp.TabIndex = 0;
            linkLabelHelp.TabStop = true;
            linkLabelHelp.Text = "Help";
            linkLabelHelp.LinkClicked += linkLabelHelp_LinkClicked;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(pictureBoxHelpIcon);
            flowLayoutPanel1.Controls.Add(linkLabelHelp);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(70, 20);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // pictureBoxHelpIcon
            // 
            pictureBoxHelpIcon.Anchor = AnchorStyles.Left;
            pictureBoxHelpIcon.Image = Properties.Images.GotoManual;
            pictureBoxHelpIcon.Location = new Point(3, 3);
            pictureBoxHelpIcon.Name = "pictureBoxHelpIcon";
            pictureBoxHelpIcon.Size = new Size(16, 16);
            pictureBoxHelpIcon.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBoxHelpIcon.TabIndex = 2;
            pictureBoxHelpIcon.TabStop = false;
            // 
            // GotoUserManualControl
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            AutoSize = true;
            Controls.Add(flowLayoutPanel1);
            MinimumSize = new Size(70, 20);
            Name = "GotoUserManualControl";
            Size = new Size(70, 20);
            Load += GotoUserManualControl_Load;
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(pictureBoxHelpIcon)).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private LinkLabel linkLabelHelp;
        private FlowLayoutPanel flowLayoutPanel1;
        private ToolTip toolTip1;
        private PictureBox pictureBoxHelpIcon;
    }
}
