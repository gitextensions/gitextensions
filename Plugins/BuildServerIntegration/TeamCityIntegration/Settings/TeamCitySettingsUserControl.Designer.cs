namespace TeamCityIntegration.Settings
{
    partial class TeamCitySettingsUserControl
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
            System.Windows.Forms.Label labelBuildFilter;
            System.Windows.Forms.Label labelProjectNameComment;
            System.Windows.Forms.Label labelBuildIdFilter;
            this.TeamCityServerUrl = new System.Windows.Forms.TextBox();
            this.TeamCityProjectName = new System.Windows.Forms.TextBox();
            this.TeamCityBuildIdFilter = new System.Windows.Forms.TextBox();
            this.labelRegexError = new System.Windows.Forms.Label();
            this.CheckBoxLogAsGuest = new System.Windows.Forms.CheckBox();
            this.buttonProjectChooser = new System.Windows.Forms.Button();
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard = new System.Windows.Forms.LinkLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            label13 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            labelBuildFilter = new System.Windows.Forms.Label();
            labelProjectNameComment = new System.Windows.Forms.Label();
            labelBuildIdFilter = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label13
            // 
            label13.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(3, 25);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(108, 13);
            label13.TabIndex = 0;
            label13.Text = "TeamCity server URL";
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 50);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(70, 13);
            label1.TabIndex = 2;
            label1.Text = "Project name";
            // 
            // labelBuildFilter
            // 
            labelBuildFilter.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelBuildFilter.AutoSize = true;
            labelBuildFilter.Location = new System.Drawing.Point(3, 93);
            labelBuildFilter.Name = "labelBuildFilter";
            labelBuildFilter.Size = new System.Drawing.Size(69, 13);
            labelBuildFilter.TabIndex = 4;
            labelBuildFilter.Text = "Build Id Filter";
            // 
            // labelProjectNameComment
            // 
            labelProjectNameComment.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelProjectNameComment.AutoSize = true;
            labelProjectNameComment.Location = new System.Drawing.Point(117, 72);
            labelProjectNameComment.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            labelProjectNameComment.Name = "labelProjectNameComment";
            labelProjectNameComment.Size = new System.Drawing.Size(173, 15);
            labelProjectNameComment.TabIndex = 6;
            labelProjectNameComment.Text = "Several names splitted by | char";
            // 
            // labelBuildIdFilter
            // 
            labelBuildIdFilter.AutoSize = true;
            labelBuildIdFilter.Location = new System.Drawing.Point(3, 0);
            labelBuildIdFilter.Name = "labelBuildIdFilter";
            labelBuildIdFilter.Size = new System.Drawing.Size(45, 15);
            labelBuildIdFilter.TabIndex = 7;
            labelBuildIdFilter.Text = "Regexp";
            // 
            // TeamCityServerUrl
            // 
            this.TeamCityServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TeamCityServerUrl.Location = new System.Drawing.Point(117, 21);
            this.TeamCityServerUrl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TeamCityServerUrl.Name = "TeamCityServerUrl";
            this.TeamCityServerUrl.Size = new System.Drawing.Size(437, 21);
            this.TeamCityServerUrl.TabIndex = 1;
            this.TeamCityServerUrl.TextChanged += new System.EventHandler(this.TeamCityServerUrl_TextChanged);
            // 
            // TeamCityProjectName
            // 
            this.TeamCityProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TeamCityProjectName.Location = new System.Drawing.Point(3, 2);
            this.TeamCityProjectName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TeamCityProjectName.Name = "TeamCityProjectName";
            this.TeamCityProjectName.Size = new System.Drawing.Size(407, 21);
            this.TeamCityProjectName.TabIndex = 3;
            // 
            // TeamCityBuildIdFilter
            // 
            this.TeamCityBuildIdFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TeamCityBuildIdFilter.Location = new System.Drawing.Point(117, 89);
            this.TeamCityBuildIdFilter.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TeamCityBuildIdFilter.Name = "TeamCityBuildIdFilter";
            this.TeamCityBuildIdFilter.Size = new System.Drawing.Size(437, 21);
            this.TeamCityBuildIdFilter.TabIndex = 5;
            this.TeamCityBuildIdFilter.TextChanged += new System.EventHandler(this.TeamCityBuildIdFilter_TextChanged);
            // 
            // labelRegexError
            // 
            this.labelRegexError.AutoSize = true;
            this.labelRegexError.ForeColor = System.Drawing.Color.Red;
            this.labelRegexError.Location = new System.Drawing.Point(54, 0);
            this.labelRegexError.Name = "labelRegexError";
            this.labelRegexError.Size = new System.Drawing.Size(374, 15);
            this.labelRegexError.TabIndex = 10;
            this.labelRegexError.Text = "The \"Build Id Filter\" regular expression is not valid and won\'t be saved!";
            this.labelRegexError.Visible = false;
            // 
            // CheckBoxLogAsGuest
            // 
            this.CheckBoxLogAsGuest.AutoSize = true;
            this.CheckBoxLogAsGuest.Location = new System.Drawing.Point(117, 133);
            this.CheckBoxLogAsGuest.Margin = new System.Windows.Forms.Padding(3, 6, 3, 2);
            this.CheckBoxLogAsGuest.Name = "CheckBoxLogAsGuest";
            this.CheckBoxLogAsGuest.Size = new System.Drawing.Size(213, 17);
            this.CheckBoxLogAsGuest.TabIndex = 11;
            this.CheckBoxLogAsGuest.Text = "Log as guest to display the build report";
            this.CheckBoxLogAsGuest.UseVisualStyleBackColor = true;
            // 
            // buttonProjectChooser
            // 
            this.buttonProjectChooser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonProjectChooser.Location = new System.Drawing.Point(416, 2);
            this.buttonProjectChooser.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonProjectChooser.Name = "buttonProjectChooser";
            this.buttonProjectChooser.Size = new System.Drawing.Size(24, 20);
            this.buttonProjectChooser.TabIndex = 12;
            this.buttonProjectChooser.Text = "...";
            this.buttonProjectChooser.UseVisualStyleBackColor = true;
            this.buttonProjectChooser.Click += new System.EventHandler(this.buttonProjectChooser_Click);
            // 
            // lnkExtractDataFromBuildUrlCopiedInTheClipboard
            // 
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.AutoSize = true;
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.Location = new System.Drawing.Point(117, 0);
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.Name = "lnkExtractDataFromBuildUrlCopiedInTheClipboard";
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.Size = new System.Drawing.Size(280, 13);
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.TabIndex = 13;
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.TabStop = true;
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.Text = "Extract the data from the build url copied in the clipboard";
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkExtractDataFromBuildUrlCopiedInTheClipboard_LinkClicked);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.CheckBoxLogAsGuest, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.lnkExtractDataFromBuildUrlCopiedInTheClipboard, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.TeamCityServerUrl, 1, 1);
            this.tableLayoutPanel1.Controls.Add(label13, 0, 1);
            this.tableLayoutPanel1.Controls.Add(label1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(labelBuildFilter, 0, 4);
            this.tableLayoutPanel1.Controls.Add(labelProjectNameComment, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.TeamCityBuildIdFilter, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(557, 152);
            this.tableLayoutPanel1.TabIndex = 14;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.TeamCityProjectName, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonProjectChooser, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(114, 44);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(443, 25);
            this.tableLayoutPanel2.TabIndex = 15;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(labelBuildIdFilter);
            this.flowLayoutPanel1.Controls.Add(this.labelRegexError);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(114, 112);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(443, 15);
            this.flowLayoutPanel1.TabIndex = 15;
            // 
            // TeamCitySettingsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "TeamCitySettingsUserControl";
            this.Size = new System.Drawing.Size(557, 152);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TeamCityServerUrl;
        private System.Windows.Forms.TextBox TeamCityProjectName;
        private System.Windows.Forms.TextBox TeamCityBuildIdFilter;
        private System.Windows.Forms.Label labelRegexError;
        private System.Windows.Forms.CheckBox CheckBoxLogAsGuest;
        private System.Windows.Forms.Button buttonProjectChooser;
        private System.Windows.Forms.LinkLabel lnkExtractDataFromBuildUrlCopiedInTheClipboard;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}
