namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class DetailedSettingsPage
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PushWindowGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.chkRemotesFromServer = new System.Windows.Forms.CheckBox();
            this.BrowseRepoGB = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.chkChowConsoleTab = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.PushWindowGB.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.BrowseRepoGB.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // PushWindowGB
            // 
            this.PushWindowGB.AutoSize = true;
            this.PushWindowGB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PushWindowGB.Controls.Add(this.tableLayoutPanel1);
            this.PushWindowGB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PushWindowGB.Location = new System.Drawing.Point(3, 62);
            this.PushWindowGB.Name = "PushWindowGB";
            this.PushWindowGB.Padding = new System.Windows.Forms.Padding(8);
            this.PushWindowGB.Size = new System.Drawing.Size(968, 53);
            this.PushWindowGB.TabIndex = 1;
            this.PushWindowGB.TabStop = false;
            this.PushWindowGB.Text = "Push window";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.chkRemotesFromServer, 0, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(8, 22);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(952, 23);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // chkRemotesFromServer
            // 
            this.chkRemotesFromServer.AutoSize = true;
            this.chkRemotesFromServer.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.chkRemotesFromServer.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkRemotesFromServer.Location = new System.Drawing.Point(3, 3);
            this.chkRemotesFromServer.Name = "chkRemotesFromServer";
            this.chkRemotesFromServer.Size = new System.Drawing.Size(946, 17);
            this.chkRemotesFromServer.TabIndex = 4;
            this.chkRemotesFromServer.Text = "Get remote branches directly from the remote";
            this.chkRemotesFromServer.UseVisualStyleBackColor = true;
            // 
            // BrowseRepoGB
            // 
            this.BrowseRepoGB.AutoSize = true;
            this.BrowseRepoGB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BrowseRepoGB.Controls.Add(this.tableLayoutPanel3);
            this.BrowseRepoGB.Dock = System.Windows.Forms.DockStyle.Top;
            this.BrowseRepoGB.Location = new System.Drawing.Point(3, 3);
            this.BrowseRepoGB.Name = "BrowseRepoGB";
            this.BrowseRepoGB.Padding = new System.Windows.Forms.Padding(8);
            this.BrowseRepoGB.Size = new System.Drawing.Size(968, 53);
            this.BrowseRepoGB.TabIndex = 0;
            this.BrowseRepoGB.TabStop = false;
            this.BrowseRepoGB.Text = "Browse repository window";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.chkChowConsoleTab, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(8, 22);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(952, 23);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // chkChowConsoleTab
            // 
            this.chkChowConsoleTab.AutoSize = true;
            this.chkChowConsoleTab.Location = new System.Drawing.Point(3, 3);
            this.chkChowConsoleTab.Name = "chkChowConsoleTab";
            this.chkChowConsoleTab.Size = new System.Drawing.Size(131, 17);
            this.chkChowConsoleTab.TabIndex = 0;
            this.chkChowConsoleTab.Text = "Show the Console tab";
            this.chkChowConsoleTab.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.BrowseRepoGB, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.PushWindowGB, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(974, 452);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // DetailedSettingsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "DetailedSettingsPage";
            this.Size = new System.Drawing.Size(974, 452);
            this.PushWindowGB.ResumeLayout(false);
            this.PushWindowGB.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.BrowseRepoGB.ResumeLayout(false);
            this.BrowseRepoGB.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox PushWindowGB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox chkRemotesFromServer;
        private System.Windows.Forms.GroupBox BrowseRepoGB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.CheckBox chkChowConsoleTab;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}