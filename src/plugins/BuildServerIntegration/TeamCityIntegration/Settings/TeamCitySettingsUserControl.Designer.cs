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
            if (disposing && (components is not null))
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
            Label label13;
            Label label1;
            Label labelBuildFilter;
            Label labelProjectNameComment;
            Label labelBuildIdFilter;
            TeamCityServerUrl = new TextBox();
            TeamCityProjectName = new TextBox();
            TeamCityBuildIdFilter = new TextBox();
            labelRegexError = new Label();
            CheckBoxLogAsGuest = new CheckBox();
            buttonProjectChooser = new Button();
            lnkExtractDataFromBuildUrlCopiedInTheClipboard = new LinkLabel();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            label13 = new Label();
            label1 = new Label();
            labelBuildFilter = new Label();
            labelProjectNameComment = new Label();
            labelBuildIdFilter = new Label();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // label13
            // 
            label13.Anchor = AnchorStyles.Left;
            label13.AutoSize = true;
            label13.Location = new Point(3, 25);
            label13.Name = "label13";
            label13.Size = new Size(108, 13);
            label13.TabIndex = 0;
            label13.Text = "TeamCity server URL";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(3, 50);
            label1.Name = "label1";
            label1.Size = new Size(70, 13);
            label1.TabIndex = 2;
            label1.Text = "Project name";
            // 
            // labelBuildFilter
            // 
            labelBuildFilter.Anchor = AnchorStyles.Left;
            labelBuildFilter.AutoSize = true;
            labelBuildFilter.Location = new Point(3, 93);
            labelBuildFilter.Name = "labelBuildFilter";
            labelBuildFilter.Size = new Size(69, 13);
            labelBuildFilter.TabIndex = 4;
            labelBuildFilter.Text = "Build Id Filter";
            // 
            // labelProjectNameComment
            // 
            labelProjectNameComment.Anchor = AnchorStyles.Left;
            labelProjectNameComment.AutoSize = true;
            labelProjectNameComment.Location = new Point(117, 72);
            labelProjectNameComment.Margin = new Padding(3, 3, 3, 0);
            labelProjectNameComment.Name = "labelProjectNameComment";
            labelProjectNameComment.Size = new Size(173, 15);
            labelProjectNameComment.TabIndex = 6;
            labelProjectNameComment.Text = "Several names splitted by | char";
            // 
            // labelBuildIdFilter
            // 
            labelBuildIdFilter.AutoSize = true;
            labelBuildIdFilter.Location = new Point(3, 0);
            labelBuildIdFilter.Name = "labelBuildIdFilter";
            labelBuildIdFilter.Size = new Size(45, 15);
            labelBuildIdFilter.TabIndex = 7;
            labelBuildIdFilter.Text = "Regexp";
            // 
            // TeamCityServerUrl
            // 
            TeamCityServerUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TeamCityServerUrl.Location = new Point(117, 21);
            TeamCityServerUrl.Margin = new Padding(3, 2, 3, 2);
            TeamCityServerUrl.Name = "TeamCityServerUrl";
            TeamCityServerUrl.Size = new Size(437, 21);
            TeamCityServerUrl.TabIndex = 2;
            TeamCityServerUrl.TextChanged += TeamCityServerUrl_TextChanged;
            // 
            // TeamCityProjectName
            // 
            TeamCityProjectName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TeamCityProjectName.Location = new Point(3, 2);
            TeamCityProjectName.Margin = new Padding(3, 2, 3, 2);
            TeamCityProjectName.Name = "TeamCityProjectName";
            TeamCityProjectName.Size = new Size(407, 21);
            TeamCityProjectName.TabIndex = 3;
            // 
            // TeamCityBuildIdFilter
            // 
            TeamCityBuildIdFilter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TeamCityBuildIdFilter.Location = new Point(117, 89);
            TeamCityBuildIdFilter.Margin = new Padding(3, 2, 3, 2);
            TeamCityBuildIdFilter.Name = "TeamCityBuildIdFilter";
            TeamCityBuildIdFilter.Size = new Size(437, 21);
            TeamCityBuildIdFilter.TabIndex = 5;
            TeamCityBuildIdFilter.TextChanged += TeamCityBuildIdFilter_TextChanged;
            // 
            // labelRegexError
            // 
            labelRegexError.AutoSize = true;
            labelRegexError.ForeColor = Color.Red;
            labelRegexError.Location = new Point(54, 0);
            labelRegexError.Name = "labelRegexError";
            labelRegexError.Size = new Size(374, 15);
            labelRegexError.TabIndex = 10;
            labelRegexError.Text = "The \"Build Id Filter\" regular expression is not valid and won\'t be saved!";
            labelRegexError.Visible = false;
            // 
            // CheckBoxLogAsGuest
            // 
            CheckBoxLogAsGuest.AutoSize = true;
            CheckBoxLogAsGuest.Location = new Point(117, 133);
            CheckBoxLogAsGuest.Margin = new Padding(3, 6, 3, 2);
            CheckBoxLogAsGuest.Name = "CheckBoxLogAsGuest";
            CheckBoxLogAsGuest.Size = new Size(213, 17);
            CheckBoxLogAsGuest.TabIndex = 6;
            CheckBoxLogAsGuest.Text = "Log as guest to display the build report";
            CheckBoxLogAsGuest.UseVisualStyleBackColor = true;
            // 
            // buttonProjectChooser
            // 
            buttonProjectChooser.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonProjectChooser.Location = new Point(416, 2);
            buttonProjectChooser.Margin = new Padding(3, 2, 3, 2);
            buttonProjectChooser.Name = "buttonProjectChooser";
            buttonProjectChooser.Size = new Size(24, 20);
            buttonProjectChooser.TabIndex = 4;
            buttonProjectChooser.Text = "...";
            buttonProjectChooser.UseVisualStyleBackColor = true;
            buttonProjectChooser.Click += buttonProjectChooser_Click;
            // 
            // lnkExtractDataFromBuildUrlCopiedInTheClipboard
            // 
            lnkExtractDataFromBuildUrlCopiedInTheClipboard.AutoSize = true;
            lnkExtractDataFromBuildUrlCopiedInTheClipboard.Location = new Point(117, 0);
            lnkExtractDataFromBuildUrlCopiedInTheClipboard.Margin = new Padding(3, 0, 3, 6);
            lnkExtractDataFromBuildUrlCopiedInTheClipboard.Name = "lnkExtractDataFromBuildUrlCopiedInTheClipboard";
            lnkExtractDataFromBuildUrlCopiedInTheClipboard.Size = new Size(280, 13);
            lnkExtractDataFromBuildUrlCopiedInTheClipboard.TabIndex = 1;
            lnkExtractDataFromBuildUrlCopiedInTheClipboard.TabStop = true;
            lnkExtractDataFromBuildUrlCopiedInTheClipboard.Text = "Extract the data from the build url copied in the clipboard";
            lnkExtractDataFromBuildUrlCopiedInTheClipboard.LinkClicked += lnkExtractDataFromBuildUrlCopiedInTheClipboard_LinkClicked;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 1, 5);
            tableLayoutPanel1.Controls.Add(CheckBoxLogAsGuest, 1, 6);
            tableLayoutPanel1.Controls.Add(lnkExtractDataFromBuildUrlCopiedInTheClipboard, 1, 0);
            tableLayoutPanel1.Controls.Add(TeamCityServerUrl, 1, 1);
            tableLayoutPanel1.Controls.Add(label13, 0, 1);
            tableLayoutPanel1.Controls.Add(label1, 0, 2);
            tableLayoutPanel1.Controls.Add(labelBuildFilter, 0, 4);
            tableLayoutPanel1.Controls.Add(labelProjectNameComment, 1, 3);
            tableLayoutPanel1.Controls.Add(TeamCityBuildIdFilter, 1, 4);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 1, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 7;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(557, 152);
            tableLayoutPanel1.TabIndex = 14;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.Controls.Add(TeamCityProjectName, 0, 0);
            tableLayoutPanel2.Controls.Add(buttonProjectChooser, 1, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(114, 44);
            tableLayoutPanel2.Margin = new Padding(0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(443, 25);
            tableLayoutPanel2.TabIndex = 15;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(labelBuildIdFilter);
            flowLayoutPanel1.Controls.Add(labelRegexError);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(114, 112);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(443, 15);
            flowLayoutPanel1.TabIndex = 15;
            // 
            // TeamCitySettingsUserControl
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(0);
            Name = "TeamCitySettingsUserControl";
            Size = new Size(557, 152);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TextBox TeamCityServerUrl;
        private TextBox TeamCityProjectName;
        private TextBox TeamCityBuildIdFilter;
        private Label labelRegexError;
        private CheckBox CheckBoxLogAsGuest;
        private Button buttonProjectChooser;
        private LinkLabel lnkExtractDataFromBuildUrlCopiedInTheClipboard;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
    }
}
