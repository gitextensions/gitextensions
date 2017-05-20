namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    partial class RecentRepositoryItem
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
            this.pbIcon = new System.Windows.Forms.PictureBox();
            this._NO_TRANSLATE_Path = new System.Windows.Forms.LinkLabel();
            this._NO_TRANSLATE_Category = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_BranchName = new System.Windows.Forms.Label();
            this.tlpnlContainer = new System.Windows.Forms.TableLayoutPanel();
            this.flpnlRow2 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).BeginInit();
            this.tlpnlContainer.SuspendLayout();
            this.flpnlRow2.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbIcon
            // 
            this.pbIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pbIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbIcon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbIcon.Location = new System.Drawing.Point(3, 2);
            this.pbIcon.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pbIcon.Name = "pbIcon";
            this.pbIcon.Size = new System.Drawing.Size(16, 16);
            this.pbIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbIcon.TabIndex = 0;
            this.pbIcon.TabStop = false;
            this.pbIcon.Click += new System.EventHandler(this.pbIcon_Click);
            // 
            // _NO_TRANSLATE_Path
            // 
            this._NO_TRANSLATE_Path.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._NO_TRANSLATE_Path.AutoEllipsis = true;
            this._NO_TRANSLATE_Path.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_Path.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._NO_TRANSLATE_Path.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this._NO_TRANSLATE_Path.LinkColor = System.Drawing.Color.Gainsboro;
            this._NO_TRANSLATE_Path.Location = new System.Drawing.Point(25, 0);
            this._NO_TRANSLATE_Path.Name = "_NO_TRANSLATE_Path";
            this._NO_TRANSLATE_Path.Size = new System.Drawing.Size(348, 20);
            this._NO_TRANSLATE_Path.TabIndex = 0;
            this._NO_TRANSLATE_Path.TabStop = true;
            this._NO_TRANSLATE_Path.Text = "c:\\Home\\";
            this._NO_TRANSLATE_Path.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_Category
            // 
            this._NO_TRANSLATE_Category.AutoSize = true;
            this._NO_TRANSLATE_Category.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._NO_TRANSLATE_Category.ForeColor = System.Drawing.Color.DarkGray;
            this._NO_TRANSLATE_Category.Location = new System.Drawing.Point(165, 3);
            this._NO_TRANSLATE_Category.Name = "_NO_TRANSLATE_Category";
            this._NO_TRANSLATE_Category.Size = new System.Drawing.Size(186, 25);
            this._NO_TRANSLATE_Category.TabIndex = 1;
            this._NO_TRANSLATE_Category.Text = "CustomLongCategory";
            this._NO_TRANSLATE_Category.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this._NO_TRANSLATE_Category.Visible = false;
            // 
            // _NO_TRANSLATE_BranchName
            // 
            this._NO_TRANSLATE_BranchName.AutoEllipsis = true;
            this._NO_TRANSLATE_BranchName.AutoSize = true;
            this._NO_TRANSLATE_BranchName.Cursor = System.Windows.Forms.Cursors.Hand;
            this._NO_TRANSLATE_BranchName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._NO_TRANSLATE_BranchName.ForeColor = System.Drawing.SystemColors.HotTrack;
            this._NO_TRANSLATE_BranchName.Location = new System.Drawing.Point(3, 3);
            this._NO_TRANSLATE_BranchName.Name = "_NO_TRANSLATE_BranchName";
            this._NO_TRANSLATE_BranchName.Size = new System.Drawing.Size(156, 25);
            this._NO_TRANSLATE_BranchName.TabIndex = 1;
            this._NO_TRANSLATE_BranchName.Text = "refs/heads/master";
            this._NO_TRANSLATE_BranchName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._NO_TRANSLATE_BranchName.Visible = false;
            // 
            // tlpnlContainer
            // 
            this.tlpnlContainer.ColumnCount = 2;
            this.tlpnlContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpnlContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlContainer.Controls.Add(this.flpnlRow2, 1, 1);
            this.tlpnlContainer.Controls.Add(this._NO_TRANSLATE_Path, 1, 0);
            this.tlpnlContainer.Controls.Add(this.pbIcon, 0, 0);
            this.tlpnlContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlContainer.Location = new System.Drawing.Point(8, 4);
            this.tlpnlContainer.Name = "tlpnlContainer";
            this.tlpnlContainer.RowCount = 2;
            this.tlpnlContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpnlContainer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlContainer.Size = new System.Drawing.Size(344, 45);
            this.tlpnlContainer.TabIndex = 1;
            // 
            // flpnlRow2
            // 
            this.flpnlRow2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpnlRow2.AutoSize = true;
            this.flpnlRow2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpnlRow2.Controls.Add(this._NO_TRANSLATE_BranchName);
            this.flpnlRow2.Controls.Add(this._NO_TRANSLATE_Category);
            this.flpnlRow2.Location = new System.Drawing.Point(22, 20);
            this.flpnlRow2.Margin = new System.Windows.Forms.Padding(0);
            this.flpnlRow2.Name = "flpnlRow2";
            this.flpnlRow2.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.flpnlRow2.Size = new System.Drawing.Size(354, 28);
            this.flpnlRow2.TabIndex = 2;
            this.flpnlRow2.WrapContents = false;
            // 
            // RecentRepositoryItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tlpnlContainer);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "RecentRepositoryItem";
            this.Padding = new System.Windows.Forms.Padding(8, 4, 8, 8);
            this.Size = new System.Drawing.Size(360, 57);
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).EndInit();
            this.tlpnlContainer.ResumeLayout(false);
            this.tlpnlContainer.PerformLayout();
            this.flpnlRow2.ResumeLayout(false);
            this.flpnlRow2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbIcon;
        private System.Windows.Forms.LinkLabel _NO_TRANSLATE_Path;
        private System.Windows.Forms.Label _NO_TRANSLATE_BranchName;
        private System.Windows.Forms.Label _NO_TRANSLATE_Category;
        private System.Windows.Forms.TableLayoutPanel tlpnlContainer;
        private System.Windows.Forms.FlowLayoutPanel flpnlRow2;
    }
}
