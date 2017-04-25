namespace JenkinsIntegration.Settings
{
    partial class JenkinsSettingsUserControl
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
            System.Windows.Forms.Label label13;
            System.Windows.Forms.Label label1;
            this.JenkinsServerUrl = new System.Windows.Forms.TextBox();
            this.JenkinsProjectName = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            label13 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label13
            // 
            label13.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(3, 7);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(98, 13);
            label13.TabIndex = 0;
            label13.Text = "Jenkins server URL";
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 34);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(70, 13);
            label1.TabIndex = 2;
            label1.Text = "Project name";
            // 
            // JenkinsServerUrl
            // 
            this.JenkinsServerUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.JenkinsServerUrl.Location = new System.Drawing.Point(107, 3);
            this.JenkinsServerUrl.Name = "JenkinsServerUrl";
            this.JenkinsServerUrl.Size = new System.Drawing.Size(504, 21);
            this.JenkinsServerUrl.TabIndex = 1;
            // 
            // JenkinsProjectName
            // 
            this.JenkinsProjectName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.JenkinsProjectName.Location = new System.Drawing.Point(107, 30);
            this.JenkinsProjectName.Name = "JenkinsProjectName";
            this.JenkinsProjectName.Size = new System.Drawing.Size(504, 21);
            this.JenkinsProjectName.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(label13, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.JenkinsProjectName, 1, 1);
            this.tableLayoutPanel1.Controls.Add(label1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.JenkinsServerUrl, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(614, 54);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // JenkinsSettingsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "JenkinsSettingsUserControl";
            this.Size = new System.Drawing.Size(617, 57);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox JenkinsServerUrl;
        private System.Windows.Forms.TextBox JenkinsProjectName;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
