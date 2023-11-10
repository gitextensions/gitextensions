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
            tableLayoutPanel1 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            linkLabelShowHelp = new LinkLabel();
            buttonShowHelp = new Button();
            linkLabelHide = new LinkLabel();
            labelHoverText = new Label();
            pictureBox1 = new PictureBox();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 0, 0);
            tableLayoutPanel1.Controls.Add(pictureBox1, 0, 2);
            tableLayoutPanel1.Controls.Add(labelHoverText, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(150, 150);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(linkLabelShowHelp);
            flowLayoutPanel1.Controls.Add(buttonShowHelp);
            flowLayoutPanel1.Controls.Add(linkLabelHide);
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(150, 30);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // linkLabelShowHelp
            // 
            linkLabelShowHelp.AutoSize = true;
            linkLabelShowHelp.Location = new Point(3, 0);
            linkLabelShowHelp.Name = "linkLabelShowHelp";
            linkLabelShowHelp.Size = new Size(36, 30);
            linkLabelShowHelp.TabIndex = 3;
            linkLabelShowHelp.TabStop = true;
            linkLabelShowHelp.Text = "Show\r\nhelp";
            linkLabelShowHelp.Visible = false;
            linkLabelShowHelp.LinkClicked += linkLabelShowHelp_LinkClicked;
            // 
            // buttonShowHelp
            // 
            buttonShowHelp.AutoSize = true;
            buttonShowHelp.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonShowHelp.Image = Properties.Images.Information;
            buttonShowHelp.Location = new Point(42, 0);
            buttonShowHelp.Margin = new Padding(0);
            buttonShowHelp.Name = "buttonShowHelp";
            buttonShowHelp.Padding = new Padding(2);
            buttonShowHelp.Size = new Size(26, 26);
            buttonShowHelp.TabIndex = 2;
            buttonShowHelp.UseVisualStyleBackColor = true;
            buttonShowHelp.Click += buttonShowHelp_Click;
            // 
            // linkLabelHide
            // 
            linkLabelHide.AutoSize = true;
            linkLabelHide.Location = new Point(71, 0);
            linkLabelHide.Name = "linkLabelHide";
            linkLabelHide.Size = new Size(58, 15);
            linkLabelHide.TabIndex = 1;
            linkLabelHide.TabStop = true;
            linkLabelHide.Text = "Hide help";
            linkLabelHide.LinkClicked += linkLabelHide_LinkClicked;
            // 
            // labelHoverText
            // 
            labelHoverText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelHoverText.AutoSize = true;
            labelHoverText.Location = new Point(3, 30);
            labelHoverText.Name = "labelHoverText";
            labelHoverText.Size = new Size(144, 20);
            labelHoverText.TabIndex = 1;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = SystemColors.Window;
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(3, 53);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(144, 94);
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.MouseEnter += HelpImageDisplayUserControl_MouseEnter;
            pictureBox1.MouseLeave += HelpImageDisplayUserControl_MouseLeave;
            // 
            // HelpImageDisplayUserControl
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(tableLayoutPanel1);
            MinimumSize = new Size(50, 150);
            Name = "HelpImageDisplayUserControl";
            Load += HelpImageDisplayUserControl_Load;
            MouseEnter += HelpImageDisplayUserControl_MouseEnter;
            MouseLeave += HelpImageDisplayUserControl_MouseLeave;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private PictureBox pictureBox1;
        private Label labelHoverText;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel1;
        private LinkLabel linkLabelHide;
        private Button buttonShowHelp;
        private LinkLabel linkLabelShowHelp;
    }
}
