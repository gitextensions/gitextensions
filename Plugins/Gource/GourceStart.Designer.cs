namespace Gource
{
    partial class GourceStart
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GourceStart));
            this.button1 = new System.Windows.Forms.Button();
            this.ArgumentsLabel = new System.Windows.Forms.Label();
            this.Arguments = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.GourcePath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.WorkingDir = new System.Windows.Forms.TextBox();
            this.GourceBrowse = new System.Windows.Forms.Button();
            this.WorkingDirBrowse = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(609, 125);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(94, 29);
            this.button1.TabIndex = 0;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1Click);
            // 
            // ArgumentsLabel
            // 
            this.ArgumentsLabel.AutoSize = true;
            this.ArgumentsLabel.Location = new System.Drawing.Point(4, 72);
            this.ArgumentsLabel.Margin = new System.Windows.Forms.Padding(4, 6, 4, 0);
            this.ArgumentsLabel.Name = "ArgumentsLabel";
            this.ArgumentsLabel.Size = new System.Drawing.Size(81, 20);
            this.ArgumentsLabel.TabIndex = 1;
            this.ArgumentsLabel.Text = "Arguments";
            // 
            // Arguments
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.Arguments, 2);
            this.Arguments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Arguments.Location = new System.Drawing.Point(129, 70);
            this.Arguments.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Arguments.Name = "Arguments";
            this.Arguments.Size = new System.Drawing.Size(555, 27);
            this.Arguments.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "Path to Gource";
            // 
            // GourcePath
            // 
            this.GourcePath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GourcePath.Location = new System.Drawing.Point(129, 4);
            this.GourcePath.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.GourcePath.Name = "GourcePath";
            this.GourcePath.Size = new System.Drawing.Size(430, 27);
            this.GourcePath.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 39);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 6, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 20);
            this.label2.TabIndex = 6;
            this.label2.Text = "Repository";
            // 
            // WorkingDir
            // 
            this.WorkingDir.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WorkingDir.Location = new System.Drawing.Point(129, 37);
            this.WorkingDir.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.WorkingDir.Name = "WorkingDir";
            this.WorkingDir.Size = new System.Drawing.Size(430, 27);
            this.WorkingDir.TabIndex = 7;
            // 
            // GourceBrowse
            // 
            this.GourceBrowse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GourceBrowse.Location = new System.Drawing.Point(567, 4);
            this.GourceBrowse.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.GourceBrowse.Name = "GourceBrowse";
            this.GourceBrowse.Size = new System.Drawing.Size(117, 25);
            this.GourceBrowse.TabIndex = 8;
            this.GourceBrowse.Text = "Browse";
            this.GourceBrowse.UseVisualStyleBackColor = true;
            this.GourceBrowse.Click += new System.EventHandler(this.GourceBrowseClick);
            // 
            // WorkingDirBrowse
            // 
            this.WorkingDirBrowse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WorkingDirBrowse.Location = new System.Drawing.Point(567, 37);
            this.WorkingDirBrowse.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.WorkingDirBrowse.Name = "WorkingDirBrowse";
            this.WorkingDirBrowse.Size = new System.Drawing.Size(117, 25);
            this.WorkingDirBrowse.TabIndex = 9;
            this.WorkingDirBrowse.Text = "Browse";
            this.WorkingDirBrowse.UseVisualStyleBackColor = true;
            this.WorkingDirBrowse.Click += new System.EventHandler(this.WorkingDirBrowseClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 125F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 125F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.WorkingDirBrowse, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.ArgumentsLabel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.GourceBrowse, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.GourcePath, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.WorkingDir, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.Arguments, 1, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(15, 15);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(688, 99);
            this.tableLayoutPanel1.TabIndex = 11;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.linkLabel1);
            this.flowLayoutPanel1.Controls.Add(this.linkLabel2);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(15, 128);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(587, 26);
            this.flowLayoutPanel1.TabIndex = 12;
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(4, 0);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(107, 20);
            this.linkLabel1.TabIndex = 11;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Gource project";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // linkLabel2
            // 
            this.linkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(119, 0);
            this.linkLabel2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(155, 20);
            this.linkLabel2.TabIndex = 12;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "Gource command line";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // GourceStart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(718, 165);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "GourceStart";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gource";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label ArgumentsLabel;
        private System.Windows.Forms.TextBox Arguments;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox GourcePath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox WorkingDir;
        private System.Windows.Forms.Button GourceBrowse;
        private System.Windows.Forms.Button WorkingDirBrowse;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.LinkLabel linkLabel2;
    }
}