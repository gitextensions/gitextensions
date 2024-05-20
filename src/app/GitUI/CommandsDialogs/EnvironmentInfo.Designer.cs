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
            TableLayoutPanel tableLayoutPanel1;
            Label lblSeparatorBottom;
            Label lblSeparatorTop;
            environmentIssueInfo = new Label();
            copyButton = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            lblSeparatorBottom = new Label();
            lblSeparatorTop = new Label();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(environmentIssueInfo, 0, 0);
            tableLayoutPanel1.Controls.Add(copyButton, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 4);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new Padding(0, 8, 0, 8);
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(165, 68);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // environmentIssueInfo
            // 
            environmentIssueInfo.AutoSize = true;
            environmentIssueInfo.Dock = DockStyle.Fill;
            environmentIssueInfo.Location = new Point(11, 8);
            environmentIssueInfo.Margin = new Padding(0, 0, 8, 0);
            environmentIssueInfo.Name = "environmentIssueInfo";
            environmentIssueInfo.Size = new Size(112, 52);
            environmentIssueInfo.TabIndex = 0;
            environmentIssueInfo.Text = "- GitExtension version:\r\n- GIT version: \r\n- OS version: \r\n- .NET version: \r\n";
            // 
            // copyButton
            // 
            copyButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            copyButton.Image = Properties.Images.CopyToClipboard;
            copyButton.Location = new Point(129, 11);
            copyButton.Margin = new Padding(0);
            copyButton.Name = "copyButton";
            copyButton.Size = new Size(25, 26);
            copyButton.TabIndex = 1;
            copyButton.UseVisualStyleBackColor = true;
            copyButton.Click += copyButton_Click;
            // 
            // lblSeparatorBottom
            // 
            lblSeparatorBottom.BorderStyle = BorderStyle.Fixed3D;
            lblSeparatorBottom.Dock = DockStyle.Bottom;
            lblSeparatorBottom.Location = new Point(0, 72);
            lblSeparatorBottom.Margin = new Padding(0);
            lblSeparatorBottom.Name = "lblSeparatorBottom";
            lblSeparatorBottom.Size = new Size(165, 2);
            lblSeparatorBottom.TabIndex = 1;
            // 
            // lblSeparatorTop
            // 
            lblSeparatorTop.BorderStyle = BorderStyle.Fixed3D;
            lblSeparatorTop.Dock = DockStyle.Top;
            lblSeparatorTop.Location = new Point(0, 4);
            lblSeparatorTop.Margin = new Padding(0);
            lblSeparatorTop.Name = "lblSeparatorTop";
            lblSeparatorTop.Size = new Size(165, 2);
            lblSeparatorTop.TabIndex = 0;
            // 
            // EnvironmentInfo
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(lblSeparatorTop);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(lblSeparatorBottom);
            Margin = new Padding(0);
            Name = "EnvironmentInfo";
            Padding = new Padding(0);
            Size = new Size(165, 78);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button copyButton;
        private Label environmentIssueInfo;
    }
}
