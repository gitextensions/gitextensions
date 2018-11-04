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
            this.avatarControl = new GitUI.AvatarControl();
            this.rtbRevisionHeader = new System.Windows.Forms.RichTextBox();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // avatarControl
            // 
            this.avatarControl.AutoSize = true;
            this.avatarControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.avatarControl.Location = new System.Drawing.Point(0, 0);
            this.avatarControl.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.avatarControl.Name = "avatarControl";
            this.avatarControl.Size = new System.Drawing.Size(96, 96);
            this.avatarControl.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.Controls.Add(this.avatarControl, 0, 0);
            tableLayoutPanel1.Controls.Add(this.rtbRevisionHeader, 1, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.Size = new System.Drawing.Size(260, 96);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // rtbRevisionHeader
            // 
            this.rtbRevisionHeader.BackColor = System.Drawing.SystemColors.Window;
            this.rtbRevisionHeader.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbRevisionHeader.Location = new System.Drawing.Point(104, 0);
            this.rtbRevisionHeader.Margin = new System.Windows.Forms.Padding(0);
            this.rtbRevisionHeader.Name = "rtbRevisionHeader";
            this.rtbRevisionHeader.ReadOnly = true;
            this.rtbRevisionHeader.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.rtbRevisionHeader.Size = new System.Drawing.Size(156, 96);
            this.rtbRevisionHeader.TabIndex = 0;
            this.rtbRevisionHeader.Text = "";
            this.rtbRevisionHeader.WordWrap = false;
            this.rtbRevisionHeader.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtbRevisionHeader_LinkClicked);
            this.rtbRevisionHeader.KeyDown += new System.Windows.Forms.KeyEventHandler(this.rtbRevisionHeader_KeyDown);
            this.rtbRevisionHeader.MouseDown += new System.Windows.Forms.MouseEventHandler(this.rtbRevisionHeader_MouseDown);
            // 
            // CommitInfoHeader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "CommitInfoHeader";
            this.Size = new System.Drawing.Size(260, 96);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbRevisionHeader;
        private AvatarControl avatarControl;
    }
}
