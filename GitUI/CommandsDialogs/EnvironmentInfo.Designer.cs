namespace GitUI.CommandsDialogs
{
    partial class EnvironmentInfo
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
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
            System.Windows.Forms.Label lblSeparatorBottom;
            System.Windows.Forms.Label lblSeparatorTop;
            this.environmentIssueInfo = new System.Windows.Forms.Label();
            this.copyButton = new System.Windows.Forms.Button();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            lblSeparatorBottom = new System.Windows.Forms.Label();
            lblSeparatorTop = new System.Windows.Forms.Label();
            tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.Controls.Add(this.environmentIssueInfo, 0, 0);
            tableLayoutPanel1.Controls.Add(this.copyButton, 1, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 4);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 8, 0, 8);
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.Size = new System.Drawing.Size(165, 68);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // environmentIssueInfo
            // 
            this.environmentIssueInfo.AutoSize = true;
            this.environmentIssueInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.environmentIssueInfo.Location = new System.Drawing.Point(11, 8);
            this.environmentIssueInfo.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.environmentIssueInfo.Name = "environmentIssueInfo";
            this.environmentIssueInfo.Size = new System.Drawing.Size(112, 52);
            this.environmentIssueInfo.TabIndex = 0;
            this.environmentIssueInfo.Text = "- GitExtension version:\r\n- GIT version: \r\n- OS version: \r\n- .NET version: \r\n";
            // 
            // copyButton
            // 
            this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.copyButton.Image = global::GitUI.Properties.Images.CopyToClipboard;
            this.copyButton.Location = new System.Drawing.Point(129, 11);
            this.copyButton.Margin = new System.Windows.Forms.Padding(0);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(25, 26);
            this.copyButton.TabIndex = 1;
            this.copyButton.UseVisualStyleBackColor = true;
            this.copyButton.Click += new System.EventHandler(this.copyButton_Click);
            // 
            // lblSeparatorBottom
            // 
            lblSeparatorBottom.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            lblSeparatorBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            lblSeparatorBottom.Location = new System.Drawing.Point(0, 72);
            lblSeparatorBottom.Margin = new System.Windows.Forms.Padding(0);
            lblSeparatorBottom.Name = "lblSeparatorBottom";
            lblSeparatorBottom.Size = new System.Drawing.Size(165, 2);
            lblSeparatorBottom.TabIndex = 1;
            // 
            // lblSeparatorTop
            // 
            lblSeparatorTop.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            lblSeparatorTop.Dock = System.Windows.Forms.DockStyle.Top;
            lblSeparatorTop.Location = new System.Drawing.Point(0, 4);
            lblSeparatorTop.Margin = new System.Windows.Forms.Padding(0);
            lblSeparatorTop.Name = "lblSeparatorTop";
            lblSeparatorTop.Size = new System.Drawing.Size(165, 2);
            lblSeparatorTop.TabIndex = 0;
            // 
            // EnvironmentInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(lblSeparatorTop);
            this.Controls.Add(tableLayoutPanel1);
            this.Controls.Add(lblSeparatorBottom);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "EnvironmentInfo";
            this.Padding = new System.Windows.Forms.Padding(0);
            this.Size = new System.Drawing.Size(165, 78);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.Label environmentIssueInfo;
    }
}
