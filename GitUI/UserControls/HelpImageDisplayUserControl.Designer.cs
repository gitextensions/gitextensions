namespace GitUI.Help
{
    partial class HelpImageDisplayUserControl
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.linkLabelShowHelp = new System.Windows.Forms.LinkLabel();
            this.buttonShowHelp = new System.Windows.Forms.Button();
            this.linkLabelHide = new System.Windows.Forms.LinkLabel();
            this.labelHoverText = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.labelHoverText, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(150, 150);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.linkLabelShowHelp);
            this.flowLayoutPanel1.Controls.Add(this.buttonShowHelp);
            this.flowLayoutPanel1.Controls.Add(this.linkLabelHide);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(150, 30);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // linkLabelShowHelp
            // 
            this.linkLabelShowHelp.AutoSize = true;
            this.linkLabelShowHelp.Location = new System.Drawing.Point(3, 0);
            this.linkLabelShowHelp.Name = "linkLabelShowHelp";
            this.linkLabelShowHelp.Size = new System.Drawing.Size(36, 30);
            this.linkLabelShowHelp.TabIndex = 3;
            this.linkLabelShowHelp.TabStop = true;
            this.linkLabelShowHelp.Text = "Show\r\nhelp";
            this.linkLabelShowHelp.Visible = false;
            this.linkLabelShowHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelShowHelp_LinkClicked);
            // 
            // buttonShowHelp
            // 
            this.buttonShowHelp.AutoSize = true;
            this.buttonShowHelp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonShowHelp.Image = global::GitUI.Properties.Images.Information;
            this.buttonShowHelp.Location = new System.Drawing.Point(42, 0);
            this.buttonShowHelp.Margin = new System.Windows.Forms.Padding(0);
            this.buttonShowHelp.Name = "buttonShowHelp";
            this.buttonShowHelp.Padding = new System.Windows.Forms.Padding(2);
            this.buttonShowHelp.Size = new System.Drawing.Size(26, 26);
            this.buttonShowHelp.TabIndex = 2;
            this.buttonShowHelp.UseVisualStyleBackColor = true;
            this.buttonShowHelp.Click += new System.EventHandler(this.buttonShowHelp_Click);
            // 
            // linkLabelHide
            // 
            this.linkLabelHide.AutoSize = true;
            this.linkLabelHide.Location = new System.Drawing.Point(71, 0);
            this.linkLabelHide.Name = "linkLabelHide";
            this.linkLabelHide.Size = new System.Drawing.Size(58, 15);
            this.linkLabelHide.TabIndex = 1;
            this.linkLabelHide.TabStop = true;
            this.linkLabelHide.Text = "Hide help";
            this.linkLabelHide.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHide_LinkClicked);
            // 
            // labelHoverText
            // 
            this.labelHoverText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelHoverText.AutoSize = true;
            this.labelHoverText.Location = new System.Drawing.Point(3, 30);
            this.labelHoverText.Name = "labelHoverText";
            this.labelHoverText.Size = new System.Drawing.Size(144, 20);
            this.labelHoverText.TabIndex = 1;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(3, 53);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(144, 94);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseEnter += new System.EventHandler(this.HelpImageDisplayUserControl_MouseEnter);
            this.pictureBox1.MouseLeave += new System.EventHandler(this.HelpImageDisplayUserControl_MouseLeave);
            // 
            // HelpImageDisplayUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(50, 150);
            this.Name = "HelpImageDisplayUserControl";
            this.Load += new System.EventHandler(this.HelpImageDisplayUserControl_Load);
            this.MouseEnter += new System.EventHandler(this.HelpImageDisplayUserControl_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.HelpImageDisplayUserControl_MouseLeave);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label labelHoverText;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.LinkLabel linkLabelHide;
        private System.Windows.Forms.Button buttonShowHelp;
        private System.Windows.Forms.LinkLabel linkLabelShowHelp;
    }
}
