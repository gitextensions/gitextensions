namespace GitUI.CommitInfo
{
    partial class CommitInfoHeader
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
            avatarControl = new GitUI.AvatarControl();
            rtbRevisionHeader = new RichTextBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // avatarControl
            // 
            avatarControl.AutoSize = true;
            avatarControl.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            avatarControl.Location = new Point(0, 0);
            avatarControl.Margin = new Padding(0, 0, 8, 0);
            avatarControl.Name = "avatarControl";
            avatarControl.Size = new Size(96, 96);
            avatarControl.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(avatarControl, 0, 0);
            tableLayoutPanel1.Controls.Add(rtbRevisionHeader, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(260, 96);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // rtbRevisionHeader
            // 
            rtbRevisionHeader.BackColor = SystemColors.Window;
            rtbRevisionHeader.BorderStyle = BorderStyle.None;
            rtbRevisionHeader.Location = new Point(104, 0);
            rtbRevisionHeader.Margin = new Padding(0);
            rtbRevisionHeader.Name = "rtbRevisionHeader";
            rtbRevisionHeader.ReadOnly = true;
            rtbRevisionHeader.ScrollBars = RichTextBoxScrollBars.None;
            rtbRevisionHeader.Size = new Size(156, 96);
            rtbRevisionHeader.TabIndex = 0;
            rtbRevisionHeader.Text = "";
            rtbRevisionHeader.WordWrap = false;
            rtbRevisionHeader.LinkClicked += rtbRevisionHeader_LinkClicked;
            rtbRevisionHeader.KeyDown += rtbRevisionHeader_KeyDown;
            rtbRevisionHeader.MouseDown += rtbRevisionHeader_MouseDown;
            // 
            // CommitInfoHeader
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(tableLayoutPanel1);
            DoubleBuffered = true;
            Margin = new Padding(0);
            Name = "CommitInfoHeader";
            Size = new Size(260, 96);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private RichTextBox rtbRevisionHeader;
        private AvatarControl avatarControl;
    }
}
