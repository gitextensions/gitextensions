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
            this.thanksTimer = new System.Windows.Forms.Timer(this.components);
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this._NO_TRANSLATE_labelCopyright = new System.Windows.Forms.Label();
            this.pictureDonate = new System.Windows.Forms.PictureBox();
            this._NO_TRANSLATE_labelProductName = new System.Windows.Forms.LinkLabel();
            this.thanksTo = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this._NO_TRANSLATE_labelProductDescription = new System.Windows.Forms.Label();
            this.lblSeparator = new System.Windows.Forms.Label();
            this.linkLabelIcons = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.osVersion = new System.Windows.Forms.Label();
            this.dotNetVersion = new System.Windows.Forms.Label();
            this.copyButton = new System.Windows.Forms.Button();
            this.gitVersion = new System.Windows.Forms.Label();
            this.gitExtensionsVersion = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureDonate)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
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
            // _NO_TRANSLATE_labelCopyright
            // 
            this._NO_TRANSLATE_labelCopyright.AutoSize = true;
            this._NO_TRANSLATE_labelCopyright.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_labelCopyright.Location = new System.Drawing.Point(12, 200);
            this._NO_TRANSLATE_labelCopyright.Margin = new System.Windows.Forms.Padding(0);
            this._NO_TRANSLATE_labelCopyright.Name = "_NO_TRANSLATE_labelCopyright";
            this._NO_TRANSLATE_labelCopyright.Size = new System.Drawing.Size(343, 15);
            this._NO_TRANSLATE_labelCopyright.TabIndex = 2;
            this._NO_TRANSLATE_labelCopyright.Text = "Proudly presented by Henk Westhuis and Team";
            this._NO_TRANSLATE_labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureDonate
            // 
            this.pictureDonate.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureDonate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureDonate.Image = global::GitUI.Properties.Images.DonateBadge;
            this.pictureDonate.Location = new System.Drawing.Point(12, 164);
            this.pictureDonate.Margin = new System.Windows.Forms.Padding(12, 12, 12, 4);
            this.pictureDonate.Name = "pictureDonate";
            this.pictureDonate.Size = new System.Drawing.Size(128, 32);
            this.pictureDonate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureDonate.TabIndex = 25;
            this.pictureDonate.TabStop = false;
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
            this._NO_TRANSLATE_labelProductName.Size = new System.Drawing.Size(343, 19);
            this._NO_TRANSLATE_labelProductName.TabIndex = 0;
            this._NO_TRANSLATE_labelProductName.TabStop = true;
            this._NO_TRANSLATE_labelProductName.Text = "Git Extensions";
            this._NO_TRANSLATE_labelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // thanksTo
            // 
            this.thanksTo.AutoSize = true;
            this.thanksTo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.thanksTo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.thanksTo.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.thanksTo.Location = new System.Drawing.Point(12, 215);
            this.thanksTo.Margin = new System.Windows.Forms.Padding(0);
            this.thanksTo.Name = "thanksTo";
            this.thanksTo.Size = new System.Drawing.Size(343, 13);
            this.thanksTo.TabIndex = 3;
            this.thanksTo.TabStop = true;
            this.thanksTo.Text = "Thanks to over {0} contributors: {1}";
            this.thanksTo.UseMnemonic = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 204);
            this.label2.Margin = new System.Windows.Forms.Padding(12, 4, 12, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(121, 30);
            this.label2.TabIndex = 0;
            this.label2.Text = "Git Extensions is open source. Get involved!";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_labelProductDescription, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblSeparator, 0, 14);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_labelProductName, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.linkLabelIcons, 0, 12);
            this.tableLayoutPanel1.Controls.Add(this._NO_TRANSLATE_labelCopyright, 0, 10);
            this.tableLayoutPanel1.Controls.Add(this.thanksTo, 0, 11);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 16);
            this.tableLayoutPanel1.Controls.Add(this.osVersion, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.dotNetVersion, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.copyButton, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.gitVersion, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.gitExtensionsVersion, 0, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(12, 24, 12, 12);
            this.tableLayoutPanel1.RowCount = 17;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(367, 345);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // _NO_TRANSLATE_labelProductDescription
            // 
            this._NO_TRANSLATE_labelProductDescription.AutoSize = true;
            this._NO_TRANSLATE_labelProductDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_labelProductDescription.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._NO_TRANSLATE_labelProductDescription.Location = new System.Drawing.Point(12, 43);
            this._NO_TRANSLATE_labelProductDescription.Margin = new System.Windows.Forms.Padding(0);
            this._NO_TRANSLATE_labelProductDescription.Name = "_NO_TRANSLATE_labelProductDescription";
            this._NO_TRANSLATE_labelProductDescription.Size = new System.Drawing.Size(343, 13);
            this._NO_TRANSLATE_labelProductDescription.TabIndex = 7;
            this._NO_TRANSLATE_labelProductDescription.Text = "Visual Studio and Shell Explorer Extensions for Git";
            this._NO_TRANSLATE_labelProductDescription.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSeparator
            // 
            this.lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSeparator.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblSeparator.Location = new System.Drawing.Point(12, 259);
            this.lblSeparator.Margin = new System.Windows.Forms.Padding(0);
            this.lblSeparator.Name = "lblSeparator";
            this.lblSeparator.Size = new System.Drawing.Size(343, 1);
            this.lblSeparator.TabIndex = 5;
            this.lblSeparator.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // linkLabelIcons
            // 
            this.linkLabelIcons.AutoSize = true;
            this.linkLabelIcons.Cursor = System.Windows.Forms.Cursors.Hand;
            this.linkLabelIcons.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.linkLabelIcons.Location = new System.Drawing.Point(12, 228);
            this.linkLabelIcons.Margin = new System.Windows.Forms.Padding(0);
            this.linkLabelIcons.Name = "linkLabelIcons";
            this.linkLabelIcons.Size = new System.Drawing.Size(213, 13);
            this.linkLabelIcons.TabIndex = 4;
            this.linkLabelIcons.TabStop = true;
            this.linkLabelIcons.Text = "Some icons by Yusuke Kamiyamane (CCA3)";
            this.linkLabelIcons.UseMnemonic = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label1.Location = new System.Drawing.Point(12, 268);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(343, 65);
            this.label1.TabIndex = 6;
            this.label1.Text = "This program is distributed in the hope that it will be useful, but WITHOUT ANY W" +
    "ARRANTY; without even the implied warranty of MERCHANTABILITY of FITNESS FOR A P" +
    "ARTICULAR PURPOSE.";
            // 
            // osVersion
            // 
            this.osVersion.AutoSize = true;
            this.osVersion.Location = new System.Drawing.Point(15, 116);
            this.osVersion.Name = "osVersion";
            this.osVersion.Size = new System.Drawing.Size(91, 15);
            this.osVersion.TabIndex = 10;
            this.osVersion.Text = "- OS version: {0}";
            // 
            // dotNetVersion
            // 
            this.dotNetVersion.AutoSize = true;
            this.dotNetVersion.Location = new System.Drawing.Point(15, 136);
            this.dotNetVersion.Name = "dotNetVersion";
            this.dotNetVersion.Size = new System.Drawing.Size(101, 15);
            this.dotNetVersion.TabIndex = 11;
            this.dotNetVersion.Text = "- .NET version: {0}";
            // 
            // copyButton
            // 
            this.copyButton.Location = new System.Drawing.Point(15, 159);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(25, 25);
            this.copyButton.TabIndex = 12;
            this.copyButton.UseVisualStyleBackColor = true;
            // 
            // gitVersion
            // 
            this.gitVersion.AutoSize = true;
            this.gitVersion.Location = new System.Drawing.Point(15, 96);
            this.gitVersion.Name = "gitVersion";
            this.gitVersion.Size = new System.Drawing.Size(94, 15);
            this.gitVersion.TabIndex = 9;
            this.gitVersion.Text = "- GIT version: {0}";
            // 
            // gitExtensionsVersion
            // 
            this.gitExtensionsVersion.AutoSize = true;
            this.gitExtensionsVersion.Location = new System.Drawing.Point(15, 76);
            this.gitExtensionsVersion.Name = "gitExtensionsVersion";
            this.gitExtensionsVersion.Size = new System.Drawing.Size(141, 15);
            this.gitExtensionsVersion.TabIndex = 8;
            this.gitExtensionsVersion.Text = "- GitExtension version: {0}";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.logoPictureBox);
            this.flowLayoutPanel1.Controls.Add(this.pictureDonate);
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(157, 345);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(157, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(367, 345);
            this.panel1.TabIndex = 1;
            // 
            // FormAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(524, 345);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAbout";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureDonate)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
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
        private System.Windows.Forms.LinkLabel thanksTo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblSeparator;
        private System.Windows.Forms.LinkLabel linkLabelIcons;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label _NO_TRANSLATE_labelProductDescription;
        private System.Windows.Forms.Label gitExtensionsVersion;
        private System.Windows.Forms.Label gitVersion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label osVersion;
        private System.Windows.Forms.Label dotNetVersion;
        private System.Windows.Forms.Button copyButton;
    }
}
