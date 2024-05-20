namespace GitUI.ScriptsEngine
{
    partial class FormFilePrompt
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFilePrompt));
            btnOk = new Button();
            txtFilePath = new TextBox();
            lblSelectFiles = new Label();
            btnBrowse = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // btnOk
            // 
            btnOk.AutoSize = true;
            btnOk.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnOk.Dock = DockStyle.Fill;
            btnOk.Location = new Point(415, 34);
            btnOk.MinimumSize = new Size(100, 25);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(115, 25);
            btnOk.TabIndex = 3;
            btnOk.Text = "&OK";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // txtFilePath
            // 
            txtFilePath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtFilePath.Location = new Point(75, 5);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.Size = new Size(334, 21);
            txtFilePath.TabIndex = 1;
            // 
            // lblSelectFiles
            // 
            lblSelectFiles.AutoSize = true;
            lblSelectFiles.Dock = DockStyle.Fill;
            lblSelectFiles.Location = new Point(3, 0);
            lblSelectFiles.Name = "lblSelectFiles";
            lblSelectFiles.Size = new Size(66, 31);
            lblSelectFiles.TabIndex = 0;
            lblSelectFiles.Text = "Select file(s)";
            lblSelectFiles.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnBrowse
            // 
            btnBrowse.Dock = DockStyle.Fill;
            btnBrowse.Image = Properties.Images.BrowseFileExplorer;
            btnBrowse.ImageAlign = ContentAlignment.MiddleLeft;
            btnBrowse.Location = new Point(415, 3);
            btnBrowse.MinimumSize = new Size(100, 25);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(115, 25);
            btnBrowse.TabIndex = 2;
            btnBrowse.Text = "Browse...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(lblSelectFiles, 0, 0);
            tableLayoutPanel1.Controls.Add(btnOk, 2, 1);
            tableLayoutPanel1.Controls.Add(txtFilePath, 1, 0);
            tableLayoutPanel1.Controls.Add(btnBrowse, 2, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(8, 8);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(533, 62);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // FormFilePrompt
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(549, 78);
            Controls.Add(tableLayoutPanel1);
            Icon = ((Icon)(resources.GetObject("$this.Icon")));
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormFilePrompt";
            Padding = new Padding(8);
            SizeGripStyle = SizeGripStyle.Show;
            Text = "Select script files";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button btnOk;
        private TextBox txtFilePath;
        private Label lblSelectFiles;
        private Button btnBrowse;
        private TableLayoutPanel tableLayoutPanel1;
    }
}
