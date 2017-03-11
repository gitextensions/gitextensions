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
            label13 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            labelBuildFilter = new System.Windows.Forms.Label();
            labelProjectNameComment = new System.Windows.Forms.Label();
            labelBuildIdFilter = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(3, 41);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(137, 17);
            label13.TabIndex = 0;
            label13.Text = "TeamCity server URL";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 72);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(90, 17);
            label1.TabIndex = 2;
            label1.Text = "Project name";
            // 
            // labelBuildFilter
            // 
            labelBuildFilter.AutoSize = true;
            labelBuildFilter.Location = new System.Drawing.Point(3, 125);
            labelBuildFilter.Name = "labelBuildFilter";
            labelBuildFilter.Size = new System.Drawing.Size(84, 17);
            labelBuildFilter.TabIndex = 4;
            labelBuildFilter.Text = "Build Id Filter";
            // 
            // labelProjectNameComment
            // 
            labelProjectNameComment.AutoSize = true;
            labelProjectNameComment.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            labelProjectNameComment.Location = new System.Drawing.Point(133, 96);
            labelProjectNameComment.Name = "labelProjectNameComment";
            labelProjectNameComment.Size = new System.Drawing.Size(210, 20);
            labelProjectNameComment.TabIndex = 6;
            labelProjectNameComment.Text = "Several names splitted by | char";
            // 
            // labelBuildIdFilter
            // 
            labelBuildIdFilter.AutoSize = true;
            labelBuildIdFilter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            labelBuildIdFilter.Location = new System.Drawing.Point(133, 150);
            labelBuildIdFilter.Name = "labelBuildIdFilter";
            labelBuildIdFilter.Size = new System.Drawing.Size(55, 20);
            labelBuildIdFilter.TabIndex = 7;
            labelBuildIdFilter.Text = "Regexp";
            // 
            // TeamCityServerUrl
            // 
            this.TeamCityServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TeamCityServerUrl.Location = new System.Drawing.Point(136, 38);
            this.TeamCityServerUrl.Name = "TeamCityServerUrl";
            this.TeamCityServerUrl.Size = new System.Drawing.Size(509, 23);
            this.TeamCityServerUrl.TabIndex = 1;
            this.TeamCityServerUrl.TextChanged += new System.EventHandler(this.TeamCityServerUrl_TextChanged);
            // 
            // TeamCityProjectName
            // 
            this.TeamCityProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TeamCityProjectName.Location = new System.Drawing.Point(136, 68);
            this.TeamCityProjectName.Name = "TeamCityProjectName";
            this.TeamCityProjectName.Size = new System.Drawing.Size(478, 23);
            this.TeamCityProjectName.TabIndex = 3;
            // 
            // TeamCityBuildIdFilter
            // 
            this.TeamCityBuildIdFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TeamCityBuildIdFilter.Location = new System.Drawing.Point(136, 122);
            this.TeamCityBuildIdFilter.Name = "TeamCityBuildIdFilter";
            this.TeamCityBuildIdFilter.Size = new System.Drawing.Size(509, 23);
            this.TeamCityBuildIdFilter.TabIndex = 5;
            this.TeamCityBuildIdFilter.TextChanged += new System.EventHandler(this.TeamCityBuildIdFilter_TextChanged);
            // 
            // labelRegexError
            // 
            this.labelRegexError.AutoSize = true;
            this.labelRegexError.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.labelRegexError.ForeColor = System.Drawing.Color.Red;
            this.labelRegexError.Location = new System.Drawing.Point(188, 150);
            this.labelRegexError.Name = "labelRegexError";
            this.labelRegexError.Size = new System.Drawing.Size(457, 20);
            this.labelRegexError.TabIndex = 10;
            this.labelRegexError.Text = "The \"Build Id Filter\" regular expression is not valid and won\'t be saved!";
            this.labelRegexError.Visible = false;
            // 
            // CheckBoxLogAsGuest
            // 
            this.CheckBoxLogAsGuest.AutoSize = true;
            this.CheckBoxLogAsGuest.Location = new System.Drawing.Point(137, 177);
            this.CheckBoxLogAsGuest.Name = "CheckBoxLogAsGuest";
            this.CheckBoxLogAsGuest.Size = new System.Drawing.Size(268, 21);
            this.CheckBoxLogAsGuest.TabIndex = 11;
            this.CheckBoxLogAsGuest.Text = "Log as guest to display the build report";
            this.CheckBoxLogAsGuest.UseVisualStyleBackColor = true;
            // 
            // buttonProjectChooser
            // 
            this.buttonProjectChooser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonProjectChooser.Location = new System.Drawing.Point(617, 67);
            this.buttonProjectChooser.Name = "buttonProjectChooser";
            this.buttonProjectChooser.Size = new System.Drawing.Size(28, 25);
            this.buttonProjectChooser.TabIndex = 12;
            this.buttonProjectChooser.Text = "...";
            this.buttonProjectChooser.UseVisualStyleBackColor = true;
            this.buttonProjectChooser.Click += new System.EventHandler(this.buttonProjectChooser_Click);
            // 
            // lnkExtractDataFromBuildUrlCopiedInTheClipboard
            // 
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.AutoSize = true;
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.Location = new System.Drawing.Point(168, 11);
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.Name = "lnkExtractDataFromBuildUrlCopiedInTheClipboard";
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.Size = new System.Drawing.Size(357, 17);
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.TabIndex = 13;
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.TabStop = true;
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.Text = "Extract the data from the build url copied in the clipboard";
            this.lnkExtractDataFromBuildUrlCopiedInTheClipboard.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkExtractDataFromBuildUrlCopiedInTheClipboard_LinkClicked);
            // 
            // TeamCitySettingsUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lnkExtractDataFromBuildUrlCopiedInTheClipboard);
            this.Controls.Add(this.buttonProjectChooser);
            this.Controls.Add(this.CheckBoxLogAsGuest);
            this.Controls.Add(this.labelRegexError);
            this.Controls.Add(labelBuildIdFilter);
            this.Controls.Add(labelProjectNameComment);
            this.Controls.Add(labelBuildFilter);
            this.Controls.Add(this.TeamCityBuildIdFilter);
            this.Controls.Add(label1);
            this.Controls.Add(label13);
            this.Controls.Add(this.TeamCityProjectName);
            this.Controls.Add(this.TeamCityServerUrl);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "TeamCitySettingsUserControl";
            this.Size = new System.Drawing.Size(658, 223);
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
    }
}
