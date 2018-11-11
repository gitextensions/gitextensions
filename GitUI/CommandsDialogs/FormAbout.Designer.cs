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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
            this._NO_TRANSLATE_labelProductDescription = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_labelProductName = new System.Windows.Forms.LinkLabel();
            this.linkLabelIcons = new System.Windows.Forms.LinkLabel();
            this._NO_TRANSLATE_labelCopyright = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_ThanksTo = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.environmentInfo = new GitUI.CommandsDialogs.EnvironmentInfo();
            this.thanksTimer = new System.Windows.Forms.Timer(this.components);
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this.pictureDonate = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureDonate)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_labelProductDescription, 0, 1);
            tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_labelProductName, 0, 0);
            tableLayoutPanel1.Controls.Add(this.linkLabelIcons, 0, 5);
            tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_labelCopyright, 0, 3);
            tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_ThanksTo, 0, 4);
            tableLayoutPanel1.Controls.Add(this.label1, 0, 8);
            tableLayoutPanel1.Controls.Add(this.environmentInfo, 0, 6);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(12, 24, 12, 12);
            tableLayoutPanel1.RowCount = 9;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.Size = new System.Drawing.Size(444, 318);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // _NO_TRANSLATE_labelProductDescription
            // 
            this._NO_TRANSLATE_labelProductDescription.AutoSize = true;
            this._NO_TRANSLATE_labelProductDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_labelProductDescription.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._NO_TRANSLATE_labelProductDescription.Location = new System.Drawing.Point(12, 43);
            this._NO_TRANSLATE_labelProductDescription.Margin = new System.Windows.Forms.Padding(0);
            this._NO_TRANSLATE_labelProductDescription.Name = "_NO_TRANSLATE_labelProductDescription";
            this._NO_TRANSLATE_labelProductDescription.Size = new System.Drawing.Size(420, 13);
            this._NO_TRANSLATE_labelProductDescription.TabIndex = 1;
            this._NO_TRANSLATE_labelProductDescription.Text = "Visual Studio and Shell Explorer Extensions for Git";
            this._NO_TRANSLATE_labelProductDescription.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _NO_TRANSLATE_labelProductName
            // 
            this._NO_TRANSLATE_labelProductName.AutoSize = true;
            this._NO_TRANSLATE_labelProductName.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_labelProductName.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_labelProductName.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this._NO_TRANSLATE_labelProductName.Location = new System.Drawing.Point(12, 24);
            this._NO_TRANSLATE_labelProductName.Margin = new System.Windows.Forms.Padding(0);
            this._NO_TRANSLATE_labelProductName.Name = "_NO_TRANSLATE_labelProductName";
            this._NO_TRANSLATE_labelProductName.Size = new System.Drawing.Size(420, 19);
            this._NO_TRANSLATE_labelProductName.TabIndex = 0;
            this._NO_TRANSLATE_labelProductName.TabStop = true;
            this._NO_TRANSLATE_labelProductName.Text = "Git Extensions";
            this._NO_TRANSLATE_labelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkLabelIcons
            // 
            this.linkLabelIcons.AutoSize = true;
            this.linkLabelIcons.Cursor = System.Windows.Forms.Cursors.Hand;
            this.linkLabelIcons.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.linkLabelIcons.Location = new System.Drawing.Point(12, 92);
            this.linkLabelIcons.Margin = new System.Windows.Forms.Padding(0);
            this.linkLabelIcons.Name = "linkLabelIcons";
            this.linkLabelIcons.Size = new System.Drawing.Size(213, 13);
            this.linkLabelIcons.TabIndex = 4;
            this.linkLabelIcons.TabStop = true;
            this.linkLabelIcons.Text = "Some icons by Yusuke Kamiyamane (CCA3)";
            this.linkLabelIcons.UseMnemonic = false;
            // 
            // _NO_TRANSLATE_labelCopyright
            // 
            this._NO_TRANSLATE_labelCopyright.AutoSize = true;
            this._NO_TRANSLATE_labelCopyright.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_labelCopyright.Location = new System.Drawing.Point(12, 64);
            this._NO_TRANSLATE_labelCopyright.Margin = new System.Windows.Forms.Padding(0);
            this._NO_TRANSLATE_labelCopyright.Name = "_NO_TRANSLATE_labelCopyright";
            this._NO_TRANSLATE_labelCopyright.Size = new System.Drawing.Size(420, 15);
            this._NO_TRANSLATE_labelCopyright.TabIndex = 2;
            this._NO_TRANSLATE_labelCopyright.Text = "Proudly presented by Henk Westhuis and Team";
            this._NO_TRANSLATE_labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // thanksTo
            // 
            this._NO_TRANSLATE_ThanksTo.AutoSize = true;
            this._NO_TRANSLATE_ThanksTo.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_ThanksTo.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_ThanksTo.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this._NO_TRANSLATE_ThanksTo.Location = new System.Drawing.Point(12, 79);
            this._NO_TRANSLATE_ThanksTo.Margin = new System.Windows.Forms.Padding(0);
            this._NO_TRANSLATE_ThanksTo.Name = "_NO_TRANSLATE_ThanksTo";
            this._NO_TRANSLATE_ThanksTo.Size = new System.Drawing.Size(420, 13);
            this._NO_TRANSLATE_ThanksTo.TabIndex = 3;
            this._NO_TRANSLATE_ThanksTo.TabStop = true;
            this._NO_TRANSLATE_ThanksTo.Text = "Thanks to over {0} contributors: {1}";
            this._NO_TRANSLATE_ThanksTo.UseMnemonic = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label1.Location = new System.Drawing.Point(12, 220);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(420, 86);
            this.label1.TabIndex = 6;
            this.label1.Text = "This program is distributed in the hope that it will be useful, but WITHOUT ANY W" +
    "ARRANTY; without even the implied warranty of MERCHANTABILITY of FITNESS FOR A P" +
    "ARTICULAR PURPOSE.";
            // 
            // environmentInfo
            // 
            this.environmentInfo.AutoSize = true;
            this.environmentInfo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.environmentInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.environmentInfo.Location = new System.Drawing.Point(15, 108);
            this.environmentInfo.Name = "environmentInfo";
            this.environmentInfo.Padding = new System.Windows.Forms.Padding(0, 12, 0, 4);
            this.environmentInfo.Size = new System.Drawing.Size(414, 101);
            this.environmentInfo.TabIndex = 5;
            this.environmentInfo.ToolTip = this.toolTip;
            // 
            // thanksTimer
            // 
            this.thanksTimer.Interval = 1000;
            // 
            // logoPictureBox
            // 
            this.logoPictureBox.Image = global::GitUI.Properties.Images.GitExtensionsLogo256;
            this.logoPictureBox.Location = new System.Drawing.Point(12, 12);
            this.logoPictureBox.Margin = new System.Windows.Forms.Padding(12);
            this.logoPictureBox.Name = "logoPictureBox";
            this.logoPictureBox.Size = new System.Drawing.Size(128, 128);
            this.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.logoPictureBox.TabIndex = 12;
            this.logoPictureBox.TabStop = false;
            // 
            // pictureDonate
            // 
            this.pictureDonate.BackgroundImage = global::GitUI.Properties.Images.DonateBadge;
            this.pictureDonate.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureDonate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureDonate.Location = new System.Drawing.Point(6, 164);
            this.pictureDonate.Margin = new System.Windows.Forms.Padding(6, 12, 6, 4);
            this.pictureDonate.Name = "pictureDonate";
            this.pictureDonate.Size = new System.Drawing.Size(145, 32);
            this.pictureDonate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureDonate.TabIndex = 25;
            this.pictureDonate.TabStop = false;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(12, 156);
            this.label2.Margin = new System.Windows.Forms.Padding(12, 4, 12, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(133, 30);
            this.label2.TabIndex = 0;
            this.label2.Text = "Git Extensions is open source. Get involved!";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.logoPictureBox);
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.pictureDonate);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(157, 318);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(tableLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(157, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(444, 318);
            this.panel1.TabIndex = 1;
            // 
            // FormAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(601, 318);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAbout";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " ";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureDonate)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.LinkLabel _NO_TRANSLATE_labelProductName;
        private System.Windows.Forms.Label _NO_TRANSLATE_labelCopyright;
        private System.Windows.Forms.PictureBox pictureDonate;
        private System.Windows.Forms.Timer thanksTimer;
        private System.Windows.Forms.LinkLabel _NO_TRANSLATE_ThanksTo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel linkLabelIcons;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label _NO_TRANSLATE_labelProductDescription;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTip;
        private GitUI.CommandsDialogs.EnvironmentInfo environmentInfo;
    }
}
