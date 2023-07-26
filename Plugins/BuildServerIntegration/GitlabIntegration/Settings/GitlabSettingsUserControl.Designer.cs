namespace GitExtensions.Plugins.GitlabIntegration.Settings
{
    partial class GitlabSettingsUserControl
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
            tableLayoutPanel1 = new TableLayoutPanel();
            ProjectIdTextBox = new TextBox();
            label3 = new Label();
            label1 = new Label();
            InstanceUrlTextBox = new TextBox();
            label2 = new Label();
            ApiTokenTextBox = new TextBox();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(ProjectIdTextBox, 1, 2);
            tableLayoutPanel1.Controls.Add(label3, 0, 2);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(InstanceUrlTextBox, 1, 0);
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            tableLayoutPanel1.Controls.Add(ApiTokenTextBox, 1, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(1185, 111);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // ProjectIdTextBox
            // 
            ProjectIdTextBox.Dock = DockStyle.Fill;
            ProjectIdTextBox.Location = new Point(122, 77);
            ProjectIdTextBox.Name = "ProjectIdTextBox";
            ProjectIdTextBox.Size = new Size(1060, 31);
            ProjectIdTextBox.TabIndex = 6;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(3, 80);
            label3.Name = "label3";
            label3.Size = new Size(89, 25);
            label3.TabIndex = 5;
            label3.Text = "Project ID";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(3, 6);
            label1.Name = "label1";
            label1.Size = new Size(113, 25);
            label1.TabIndex = 2;
            label1.Text = "Instance URL";
            // 
            // InstanceUrlTextBox
            // 
            InstanceUrlTextBox.Dock = DockStyle.Fill;
            InstanceUrlTextBox.Location = new Point(122, 3);
            InstanceUrlTextBox.Name = "InstanceUrlTextBox";
            InstanceUrlTextBox.Size = new Size(1060, 31);
            InstanceUrlTextBox.TabIndex = 1;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(3, 43);
            label2.Name = "label2";
            label2.Size = new Size(90, 25);
            label2.TabIndex = 3;
            label2.Text = "Api Token";
            // 
            // ApiTokenTextBox
            // 
            ApiTokenTextBox.Dock = DockStyle.Fill;
            ApiTokenTextBox.Location = new Point(122, 40);
            ApiTokenTextBox.Name = "ApiTokenTextBox";
            ApiTokenTextBox.Size = new Size(1060, 31);
            ApiTokenTextBox.TabIndex = 4;
            // 
            // GitlabSettingsUserControl
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Name = "GitlabSettingsUserControl";
            Size = new Size(1192, 116);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TextBox InstanceUrlTextBox;
        private Label label1;
        private Label label2;
        private TextBox ApiTokenTextBox;
        private TextBox ProjectIdTextBox;
        private Label label3;
    }
}
