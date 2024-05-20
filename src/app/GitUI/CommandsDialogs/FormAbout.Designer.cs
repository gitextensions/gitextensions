namespace GitUI.CommandsDialogs
{
    partial class FormAbout
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            components = new System.ComponentModel.Container();
            TableLayoutPanel tableLayoutPanel1;
            _NO_TRANSLATE_labelProductDescription = new Label();
            _NO_TRANSLATE_labelProductName = new LinkLabel();
            linkLabelIcons = new LinkLabel();
            _NO_TRANSLATE_labelCopyright = new Label();
            _NO_TRANSLATE_ThanksTo = new LinkLabel();
            label1 = new Label();
            environmentInfo = new GitUI.CommandsDialogs.EnvironmentInfo();
            thanksTimer = new System.Windows.Forms.Timer(components);
            logoPictureBox = new PictureBox();
            pictureDonate = new PictureBox();
            label2 = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            panel1 = new Panel();
            toolTip = new ToolTip(components);
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(logoPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(pictureDonate)).BeginInit();
            flowLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_labelProductDescription, 0, 1);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_labelProductName, 0, 0);
            tableLayoutPanel1.Controls.Add(linkLabelIcons, 0, 5);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_labelCopyright, 0, 3);
            tableLayoutPanel1.Controls.Add(_NO_TRANSLATE_ThanksTo, 0, 4);
            tableLayoutPanel1.Controls.Add(label1, 0, 8);
            tableLayoutPanel1.Controls.Add(environmentInfo, 0, 6);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new Padding(12, 24, 12, 12);
            tableLayoutPanel1.RowCount = 9;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 8F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 8F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(444, 318);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // _NO_TRANSLATE_labelProductDescription
            // 
            _NO_TRANSLATE_labelProductDescription.AutoSize = true;
            _NO_TRANSLATE_labelProductDescription.Dock = DockStyle.Fill;
            _NO_TRANSLATE_labelProductDescription.Font = new Font("Tahoma", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            _NO_TRANSLATE_labelProductDescription.Location = new Point(12, 43);
            _NO_TRANSLATE_labelProductDescription.Margin = new Padding(0);
            _NO_TRANSLATE_labelProductDescription.Name = "_NO_TRANSLATE_labelProductDescription";
            _NO_TRANSLATE_labelProductDescription.Size = new Size(420, 13);
            _NO_TRANSLATE_labelProductDescription.TabIndex = 1;
            _NO_TRANSLATE_labelProductDescription.Text = "Visual Studio and Shell Explorer Extensions for Git";
            _NO_TRANSLATE_labelProductDescription.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // _NO_TRANSLATE_labelProductName
            // 
            _NO_TRANSLATE_labelProductName.AutoSize = true;
            _NO_TRANSLATE_labelProductName.Cursor = Cursors.Hand;
            _NO_TRANSLATE_labelProductName.Dock = DockStyle.Fill;
            _NO_TRANSLATE_labelProductName.Font = new Font("Tahoma", 12F, FontStyle.Bold);
            _NO_TRANSLATE_labelProductName.Location = new Point(12, 24);
            _NO_TRANSLATE_labelProductName.Margin = new Padding(0);
            _NO_TRANSLATE_labelProductName.Name = "_NO_TRANSLATE_labelProductName";
            _NO_TRANSLATE_labelProductName.Size = new Size(420, 19);
            _NO_TRANSLATE_labelProductName.TabIndex = 0;
            _NO_TRANSLATE_labelProductName.TabStop = true;
            _NO_TRANSLATE_labelProductName.Text = "Git Extensions";
            _NO_TRANSLATE_labelProductName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // linkLabelIcons
            // 
            linkLabelIcons.AutoSize = true;
            linkLabelIcons.Cursor = Cursors.Hand;
            linkLabelIcons.Font = new Font("Tahoma", 8.25F);
            linkLabelIcons.Location = new Point(12, 92);
            linkLabelIcons.Margin = new Padding(0);
            linkLabelIcons.Name = "linkLabelIcons";
            linkLabelIcons.Size = new Size(213, 13);
            linkLabelIcons.TabIndex = 4;
            linkLabelIcons.TabStop = true;
            linkLabelIcons.Text = "Some icons by Yusuke Kamiyamane (CCA3)";
            linkLabelIcons.UseMnemonic = false;
            // 
            // _NO_TRANSLATE_labelCopyright
            // 
            _NO_TRANSLATE_labelCopyright.AutoSize = true;
            _NO_TRANSLATE_labelCopyright.Dock = DockStyle.Fill;
            _NO_TRANSLATE_labelCopyright.Location = new Point(12, 64);
            _NO_TRANSLATE_labelCopyright.Margin = new Padding(0);
            _NO_TRANSLATE_labelCopyright.Name = "_NO_TRANSLATE_labelCopyright";
            _NO_TRANSLATE_labelCopyright.Size = new Size(420, 15);
            _NO_TRANSLATE_labelCopyright.TabIndex = 2;
            _NO_TRANSLATE_labelCopyright.Text = "Proudly presented by Git Extensions team";
            _NO_TRANSLATE_labelCopyright.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // thanksTo
            // 
            _NO_TRANSLATE_ThanksTo.AutoSize = true;
            _NO_TRANSLATE_ThanksTo.Cursor = Cursors.Hand;
            _NO_TRANSLATE_ThanksTo.Dock = DockStyle.Fill;
            _NO_TRANSLATE_ThanksTo.Font = new Font("Tahoma", 8.25F);
            _NO_TRANSLATE_ThanksTo.Location = new Point(12, 79);
            _NO_TRANSLATE_ThanksTo.Margin = new Padding(0);
            _NO_TRANSLATE_ThanksTo.Name = "_NO_TRANSLATE_ThanksTo";
            _NO_TRANSLATE_ThanksTo.Size = new Size(420, 13);
            _NO_TRANSLATE_ThanksTo.TabIndex = 3;
            _NO_TRANSLATE_ThanksTo.TabStop = true;
            _NO_TRANSLATE_ThanksTo.Text = "Thanks to over {0} contributors: {1}";
            _NO_TRANSLATE_ThanksTo.UseMnemonic = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.ForeColor = SystemColors.GrayText;
            label1.Location = new Point(12, 220);
            label1.Margin = new Padding(0);
            label1.Name = "label1";
            label1.Size = new Size(420, 86);
            label1.TabIndex = 6;
            label1.Text = "This program is distributed in the hope that it will be useful, but WITHOUT ANY W" +
    "ARRANTY; without even the implied warranty of MERCHANTABILITY of FITNESS FOR A P" +
    "ARTICULAR PURPOSE.";
            // 
            // environmentInfo
            // 
            environmentInfo.AutoSize = true;
            environmentInfo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            environmentInfo.Dock = DockStyle.Fill;
            environmentInfo.Location = new Point(15, 108);
            environmentInfo.Name = "environmentInfo";
            environmentInfo.Padding = new Padding(0, 12, 0, 4);
            environmentInfo.Size = new Size(414, 101);
            environmentInfo.TabIndex = 5;
            environmentInfo.ToolTip = toolTip;
            // 
            // thanksTimer
            // 
            thanksTimer.Interval = 1000;
            // 
            // logoPictureBox
            // 
            logoPictureBox.Image = Properties.Images.GitExtensionsLogo256;
            logoPictureBox.Location = new Point(12, 12);
            logoPictureBox.Margin = new Padding(12);
            logoPictureBox.Name = "logoPictureBox";
            logoPictureBox.Size = new Size(128, 128);
            logoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            logoPictureBox.TabIndex = 12;
            logoPictureBox.TabStop = false;
            // 
            // pictureDonate
            // 
            pictureDonate.BackgroundImage = Properties.Images.DonateBadge;
            pictureDonate.BackgroundImageLayout = ImageLayout.Zoom;
            pictureDonate.Cursor = Cursors.Hand;
            pictureDonate.Location = new Point(6, 164);
            pictureDonate.Margin = new Padding(6, 12, 6, 4);
            pictureDonate.Name = "pictureDonate";
            pictureDonate.Size = new Size(145, 32);
            pictureDonate.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureDonate.TabIndex = 25;
            pictureDonate.TabStop = false;
            // 
            // label2
            // 
            label2.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
            | AnchorStyles.Right)));
            label2.Location = new Point(12, 156);
            label2.Margin = new Padding(12, 4, 12, 4);
            label2.Name = "label2";
            label2.Size = new Size(133, 30);
            label2.TabIndex = 0;
            label2.Text = "Git Extensions is open source. Get involved!";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            label2.AutoSize = true;
            label2.MaximumSize = new Size(133, 0);
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(logoPictureBox);
            flowLayoutPanel1.Controls.Add(label2);
            flowLayoutPanel1.Controls.Add(pictureDonate);
            flowLayoutPanel1.Dock = DockStyle.Left;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Margin = new Padding(2);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(157, 318);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(tableLayoutPanel1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(157, 0);
            panel1.Margin = new Padding(12);
            panel1.Name = "panel1";
            panel1.Size = new Size(444, 318);
            panel1.TabIndex = 1;
            // 
            // FormAbout
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(601, 318);
            Controls.Add(panel1);
            Controls.Add(flowLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormAbout";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = " ";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(logoPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(pictureDonate)).EndInit();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private PictureBox logoPictureBox;
        private LinkLabel _NO_TRANSLATE_labelProductName;
        private Label _NO_TRANSLATE_labelCopyright;
        private PictureBox pictureDonate;
        private System.Windows.Forms.Timer thanksTimer;
        private LinkLabel _NO_TRANSLATE_ThanksTo;
        private Label label2;
        private LinkLabel linkLabelIcons;
        private FlowLayoutPanel flowLayoutPanel1;
        private Panel panel1;
        private Label _NO_TRANSLATE_labelProductDescription;
        private Label label1;
        private ToolTip toolTip;
        private GitUI.CommandsDialogs.EnvironmentInfo environmentInfo;
    }
}
