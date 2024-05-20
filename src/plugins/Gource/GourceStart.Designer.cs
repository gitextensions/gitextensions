namespace GitExtensions.Plugins.Gource
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GourceStart));
            button1 = new Button();
            ArgumentsLabel = new Label();
            Arguments = new TextBox();
            label1 = new Label();
            GourcePath = new TextBox();
            label2 = new Label();
            WorkingDir = new TextBox();
            GourceBrowse = new Button();
            WorkingDirBrowse = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            linkLabel1 = new LinkLabel();
            linkLabel2 = new LinkLabel();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button1.Location = new Point(609, 125);
            button1.Margin = new Padding(4, 4, 4, 4);
            button1.Name = "button1";
            button1.Size = new Size(94, 29);
            button1.TabIndex = 0;
            button1.Text = "Start";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button1Click;
            // 
            // ArgumentsLabel
            // 
            ArgumentsLabel.AutoSize = true;
            ArgumentsLabel.Location = new Point(4, 72);
            ArgumentsLabel.Margin = new Padding(4, 6, 4, 0);
            ArgumentsLabel.Name = "ArgumentsLabel";
            ArgumentsLabel.Size = new Size(81, 20);
            ArgumentsLabel.TabIndex = 1;
            ArgumentsLabel.Text = "Arguments";
            // 
            // Arguments
            // 
            tableLayoutPanel1.SetColumnSpan(Arguments, 2);
            Arguments.Dock = DockStyle.Fill;
            Arguments.Location = new Point(129, 70);
            Arguments.Margin = new Padding(4, 4, 4, 4);
            Arguments.Name = "Arguments";
            Arguments.Size = new Size(555, 27);
            Arguments.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(4, 6);
            label1.Margin = new Padding(4, 6, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(105, 20);
            label1.TabIndex = 4;
            label1.Text = "Path to Gource";
            // 
            // GourcePath
            // 
            GourcePath.Dock = DockStyle.Fill;
            GourcePath.Location = new Point(129, 4);
            GourcePath.Margin = new Padding(4, 4, 4, 4);
            GourcePath.Name = "GourcePath";
            GourcePath.Size = new Size(430, 27);
            GourcePath.TabIndex = 5;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(4, 39);
            label2.Margin = new Padding(4, 6, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(80, 20);
            label2.TabIndex = 6;
            label2.Text = "Repository";
            // 
            // WorkingDir
            // 
            WorkingDir.Dock = DockStyle.Fill;
            WorkingDir.Location = new Point(129, 37);
            WorkingDir.Margin = new Padding(4, 4, 4, 4);
            WorkingDir.Name = "WorkingDir";
            WorkingDir.Size = new Size(430, 27);
            WorkingDir.TabIndex = 7;
            // 
            // GourceBrowse
            // 
            GourceBrowse.Dock = DockStyle.Fill;
            GourceBrowse.Location = new Point(567, 4);
            GourceBrowse.Margin = new Padding(4, 4, 4, 4);
            GourceBrowse.Name = "GourceBrowse";
            GourceBrowse.Size = new Size(117, 25);
            GourceBrowse.TabIndex = 8;
            GourceBrowse.Text = "Browse";
            GourceBrowse.UseVisualStyleBackColor = true;
            GourceBrowse.Click += GourceBrowseClick;
            // 
            // WorkingDirBrowse
            // 
            WorkingDirBrowse.Dock = DockStyle.Fill;
            WorkingDirBrowse.Location = new Point(567, 37);
            WorkingDirBrowse.Margin = new Padding(4, 4, 4, 4);
            WorkingDirBrowse.Name = "WorkingDirBrowse";
            WorkingDirBrowse.Size = new Size(117, 25);
            WorkingDirBrowse.TabIndex = 9;
            WorkingDirBrowse.Text = "Browse";
            WorkingDirBrowse.UseVisualStyleBackColor = true;
            WorkingDirBrowse.Click += WorkingDirBrowseClick;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125F));
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            tableLayoutPanel1.Controls.Add(WorkingDirBrowse, 2, 1);
            tableLayoutPanel1.Controls.Add(ArgumentsLabel, 0, 2);
            tableLayoutPanel1.Controls.Add(GourceBrowse, 2, 0);
            tableLayoutPanel1.Controls.Add(GourcePath, 1, 0);
            tableLayoutPanel1.Controls.Add(WorkingDir, 1, 1);
            tableLayoutPanel1.Controls.Add(Arguments, 1, 2);
            tableLayoutPanel1.Location = new Point(15, 15);
            tableLayoutPanel1.Margin = new Padding(4, 4, 4, 4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel1.Size = new Size(688, 99);
            tableLayoutPanel1.TabIndex = 11;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(linkLabel1);
            flowLayoutPanel1.Controls.Add(linkLabel2);
            flowLayoutPanel1.Location = new Point(15, 128);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(587, 26);
            flowLayoutPanel1.TabIndex = 12;
            // 
            // linkLabel1
            // 
            linkLabel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(4, 0);
            linkLabel1.Margin = new Padding(4, 0, 4, 0);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(107, 20);
            linkLabel1.TabIndex = 11;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "Gource project";
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // linkLabel2
            // 
            linkLabel2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkLabel2.AutoSize = true;
            linkLabel2.Location = new Point(119, 0);
            linkLabel2.Margin = new Padding(4, 0, 4, 0);
            linkLabel2.Name = "linkLabel2";
            linkLabel2.Size = new Size(155, 20);
            linkLabel2.TabIndex = 12;
            linkLabel2.TabStop = true;
            linkLabel2.Text = "Gource command line";
            linkLabel2.LinkClicked += linkLabel2_LinkClicked;
            // 
            // GourceStart
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(718, 165);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(button1);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Margin = new Padding(4, 4, 4, 4);
            MaximizeBox = false;
            Name = "GourceStart";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Gource";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Button button1;
        private Label ArgumentsLabel;
        private TextBox Arguments;
        private Label label1;
        private TextBox GourcePath;
        private Label label2;
        private TextBox WorkingDir;
        private Button GourceBrowse;
        private Button WorkingDirBrowse;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel1;
        private LinkLabel linkLabel1;
        private LinkLabel linkLabel2;
    }
}
