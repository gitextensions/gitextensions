namespace BugReporter
{
    partial class BugReportForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BugReportForm));
            mainTabs = new TabControl();
            generalTabPage = new TabPage();
            tpnlEnvInfo = new TableLayoutPanel();
            clrTextBox = new TextBox();
            _NO_TRANSLATE_ClrLabel = new Label();
            _NO_TRANSLATE_GitLabel = new Label();
            gitTextBox = new TextBox();
            descriptionTextBox = new TextBox();
            dateTimeTextBox = new TextBox();
            errorDescriptionLabel = new Label();
            applicationTextBox = new TextBox();
            targetSiteLabel = new Label();
            targetSiteTextBox = new TextBox();
            applicationLabel = new Label();
            dateTimeLabel = new Label();
            panel1 = new Panel();
            tlpnlGeneral = new TableLayoutPanel();
            pnlHeading = new Panel();
            warningLabel = new Label();
            warningPictureBox = new PictureBox();
            pnlExceptionType = new Panel();
            exceptionTextBox = new TextBox();
            exceptionTypeLabel = new Label();
            exceptionMessageTextBox = new TextBox();
            exceptionTabPage = new TabPage();
            exceptionDetails = new BugReporter.ExceptionDetails();
            reportContentsTabPage = new TabPage();
            reportPreviewTextBox = new TextBox();
            previewLabel = new Label();
            contentsLabel = new Label();
            reportContentsListView = new ListView();
            nameColumnHeader = ((ColumnHeader)(new ColumnHeader()));
            descriptionColumnHeader = ((ColumnHeader)(new ColumnHeader()));
            sizeColumnHeader = ((ColumnHeader)(new ColumnHeader()));
            sendAndQuitButton = new Button();
            quitButton = new LinkLabel();
            btnCopy = new Button();
            toolTip = new ToolTip(components);
            tableLayoutPanel1 = new TableLayoutPanel();
            IgnoreButton = new Button();
            mainTabs.SuspendLayout();
            generalTabPage.SuspendLayout();
            tpnlEnvInfo.SuspendLayout();
            panel1.SuspendLayout();
            tlpnlGeneral.SuspendLayout();
            pnlHeading.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(warningPictureBox)).BeginInit();
            pnlExceptionType.SuspendLayout();
            exceptionTabPage.SuspendLayout();
            reportContentsTabPage.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // mainTabs
            // 
            mainTabs.Controls.Add(generalTabPage);
            mainTabs.Controls.Add(exceptionTabPage);
            mainTabs.Controls.Add(reportContentsTabPage);
            mainTabs.Dock = DockStyle.Fill;
            mainTabs.Location = new Point(4, 4);
            mainTabs.Margin = new Padding(0);
            mainTabs.Name = "mainTabs";
            mainTabs.SelectedIndex = 0;
            mainTabs.Size = new Size(550, 430);
            mainTabs.TabIndex = 0;
            // 
            // generalTabPage
            // 
            generalTabPage.Controls.Add(tpnlEnvInfo);
            generalTabPage.Controls.Add(panel1);
            generalTabPage.Location = new Point(4, 22);
            generalTabPage.Name = "generalTabPage";
            generalTabPage.Padding = new Padding(3);
            generalTabPage.Size = new Size(542, 404);
            generalTabPage.TabIndex = 0;
            generalTabPage.Text = "General";
            generalTabPage.UseVisualStyleBackColor = true;
            // 
            // tpnlEnvInfo
            // 
            tpnlEnvInfo.ColumnCount = 5;
            tpnlEnvInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            tpnlEnvInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            tpnlEnvInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            tpnlEnvInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            tpnlEnvInfo.ColumnStyles.Add(new ColumnStyle());
            tpnlEnvInfo.Controls.Add(clrTextBox, 3, 2);
            tpnlEnvInfo.Controls.Add(_NO_TRANSLATE_ClrLabel, 2, 2);
            tpnlEnvInfo.Controls.Add(_NO_TRANSLATE_GitLabel, 2, 1);
            tpnlEnvInfo.Controls.Add(gitTextBox, 3, 1);
            tpnlEnvInfo.Controls.Add(descriptionTextBox, 0, 5);
            tpnlEnvInfo.Controls.Add(dateTimeTextBox, 1, 2);
            tpnlEnvInfo.Controls.Add(errorDescriptionLabel, 0, 4);
            tpnlEnvInfo.Controls.Add(applicationTextBox, 1, 1);
            tpnlEnvInfo.Controls.Add(targetSiteLabel, 0, 0);
            tpnlEnvInfo.Controls.Add(targetSiteTextBox, 1, 0);
            tpnlEnvInfo.Controls.Add(applicationLabel, 0, 1);
            tpnlEnvInfo.Controls.Add(dateTimeLabel, 0, 2);
            tpnlEnvInfo.Dock = DockStyle.Fill;
            tpnlEnvInfo.Location = new Point(3, 130);
            tpnlEnvInfo.Name = "tpnlEnvInfo";
            tpnlEnvInfo.Padding = new Padding(8);
            tpnlEnvInfo.RowCount = 6;
            tpnlEnvInfo.RowStyles.Add(new RowStyle());
            tpnlEnvInfo.RowStyles.Add(new RowStyle());
            tpnlEnvInfo.RowStyles.Add(new RowStyle());
            tpnlEnvInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 8F));
            tpnlEnvInfo.RowStyles.Add(new RowStyle());
            tpnlEnvInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tpnlEnvInfo.Size = new Size(536, 271);
            tpnlEnvInfo.TabIndex = 1;
            // 
            // clrTextBox
            // 
            clrTextBox.Dock = DockStyle.Fill;
            clrTextBox.Location = new Point(363, 63);
            clrTextBox.MaxLength = 100;
            clrTextBox.Name = "clrTextBox";
            clrTextBox.ReadOnly = true;
            clrTextBox.Size = new Size(160, 20);
            clrTextBox.TabIndex = 9;
            // 
            // _NO_TRANSLATE_ClrLabel
            // 
            _NO_TRANSLATE_ClrLabel.AutoSize = true;
            _NO_TRANSLATE_ClrLabel.Dock = DockStyle.Fill;
            _NO_TRANSLATE_ClrLabel.Location = new Point(270, 60);
            _NO_TRANSLATE_ClrLabel.Name = "_NO_TRANSLATE_ClrLabel";
            _NO_TRANSLATE_ClrLabel.Size = new Size(87, 26);
            _NO_TRANSLATE_ClrLabel.TabIndex = 8;
            _NO_TRANSLATE_ClrLabel.Text = "CLR:";
            _NO_TRANSLATE_ClrLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _NO_TRANSLATE_GitLabel
            // 
            _NO_TRANSLATE_GitLabel.AutoSize = true;
            _NO_TRANSLATE_GitLabel.Dock = DockStyle.Fill;
            _NO_TRANSLATE_GitLabel.Location = new Point(270, 34);
            _NO_TRANSLATE_GitLabel.Name = "_NO_TRANSLATE_GitLabel";
            _NO_TRANSLATE_GitLabel.Size = new Size(87, 26);
            _NO_TRANSLATE_GitLabel.TabIndex = 4;
            _NO_TRANSLATE_GitLabel.Text = "Git:";
            _NO_TRANSLATE_GitLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // gitTextBox
            // 
            gitTextBox.Dock = DockStyle.Fill;
            gitTextBox.Location = new Point(363, 37);
            gitTextBox.MaxLength = 100;
            gitTextBox.Name = "gitTextBox";
            gitTextBox.ReadOnly = true;
            gitTextBox.Size = new Size(160, 20);
            gitTextBox.TabIndex = 5;
            // 
            // descriptionTextBox
            // 
            tpnlEnvInfo.SetColumnSpan(descriptionTextBox, 4);
            descriptionTextBox.Dock = DockStyle.Fill;
            descriptionTextBox.Location = new Point(11, 110);
            descriptionTextBox.Multiline = true;
            descriptionTextBox.Name = "descriptionTextBox";
            descriptionTextBox.Size = new Size(512, 150);
            descriptionTextBox.TabIndex = 11;
            // 
            // dateTimeTextBox
            // 
            dateTimeTextBox.Dock = DockStyle.Fill;
            dateTimeTextBox.Location = new Point(104, 63);
            dateTimeTextBox.MaxLength = 100;
            dateTimeTextBox.Name = "dateTimeTextBox";
            dateTimeTextBox.ReadOnly = true;
            dateTimeTextBox.Size = new Size(160, 20);
            dateTimeTextBox.TabIndex = 7;
            // 
            // errorDescriptionLabel
            // 
            errorDescriptionLabel.AutoSize = true;
            tpnlEnvInfo.SetColumnSpan(errorDescriptionLabel, 4);
            errorDescriptionLabel.Dock = DockStyle.Fill;
            errorDescriptionLabel.Location = new Point(11, 94);
            errorDescriptionLabel.Name = "errorDescriptionLabel";
            errorDescriptionLabel.Size = new Size(512, 13);
            errorDescriptionLabel.TabIndex = 10;
            errorDescriptionLabel.Text = "Please add a brief description (in English) of how we can reproduce the error:";
            errorDescriptionLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // applicationTextBox
            // 
            applicationTextBox.Dock = DockStyle.Fill;
            applicationTextBox.Location = new Point(104, 37);
            applicationTextBox.MaxLength = 100;
            applicationTextBox.Name = "applicationTextBox";
            applicationTextBox.ReadOnly = true;
            applicationTextBox.Size = new Size(160, 20);
            applicationTextBox.TabIndex = 3;
            // 
            // targetSiteLabel
            // 
            targetSiteLabel.AutoSize = true;
            targetSiteLabel.Dock = DockStyle.Fill;
            targetSiteLabel.Location = new Point(11, 8);
            targetSiteLabel.Name = "targetSiteLabel";
            targetSiteLabel.Size = new Size(87, 26);
            targetSiteLabel.TabIndex = 0;
            targetSiteLabel.Text = "Target Site:";
            targetSiteLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // targetSiteTextBox
            // 
            tpnlEnvInfo.SetColumnSpan(targetSiteTextBox, 3);
            targetSiteTextBox.Dock = DockStyle.Fill;
            targetSiteTextBox.Location = new Point(104, 11);
            targetSiteTextBox.MaxLength = 500;
            targetSiteTextBox.Name = "targetSiteTextBox";
            targetSiteTextBox.ReadOnly = true;
            targetSiteTextBox.Size = new Size(419, 20);
            targetSiteTextBox.TabIndex = 1;
            // 
            // applicationLabel
            // 
            applicationLabel.AutoSize = true;
            applicationLabel.Dock = DockStyle.Fill;
            applicationLabel.Location = new Point(11, 34);
            applicationLabel.Name = "applicationLabel";
            applicationLabel.Size = new Size(87, 26);
            applicationLabel.TabIndex = 2;
            applicationLabel.Text = "Application:";
            applicationLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // dateTimeLabel
            // 
            dateTimeLabel.AutoSize = true;
            dateTimeLabel.Dock = DockStyle.Fill;
            dateTimeLabel.Location = new Point(11, 60);
            dateTimeLabel.Name = "dateTimeLabel";
            dateTimeLabel.Size = new Size(87, 26);
            dateTimeLabel.TabIndex = 6;
            dateTimeLabel.Text = "Date/Time:";
            dateTimeLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel1.Controls.Add(tlpnlGeneral);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(536, 127);
            panel1.TabIndex = 0;
            // 
            // tlpnlGeneral
            // 
            tlpnlGeneral.AutoSize = true;
            tlpnlGeneral.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlGeneral.ColumnCount = 2;
            tlpnlGeneral.ColumnStyles.Add(new ColumnStyle());
            tlpnlGeneral.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlGeneral.Controls.Add(pnlHeading, 1, 0);
            tlpnlGeneral.Controls.Add(warningPictureBox, 0, 0);
            tlpnlGeneral.Controls.Add(pnlExceptionType, 0, 1);
            tlpnlGeneral.Dock = DockStyle.Fill;
            tlpnlGeneral.Location = new Point(0, 0);
            tlpnlGeneral.Name = "tlpnlGeneral";
            tlpnlGeneral.RowCount = 2;
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.RowStyles.Add(new RowStyle());
            tlpnlGeneral.Size = new Size(536, 127);
            tlpnlGeneral.TabIndex = 0;
            // 
            // pnlHeading
            // 
            pnlHeading.AutoSize = true;
            pnlHeading.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlHeading.Controls.Add(warningLabel);
            pnlHeading.Dock = DockStyle.Fill;
            pnlHeading.Location = new Point(51, 3);
            pnlHeading.Name = "pnlHeading";
            pnlHeading.Size = new Size(482, 42);
            pnlHeading.TabIndex = 0;
            // 
            // warningLabel
            // 
            warningLabel.Dock = DockStyle.Fill;
            warningLabel.Location = new Point(0, 0);
            warningLabel.Name = "warningLabel";
            warningLabel.Size = new Size(482, 42);
            warningLabel.TabIndex = 0;
            warningLabel.Text = "The application has crashed and it will now be terminated. If you click Quit, the application will close immediately. If you click Send and Quit, the application will close and a bug report will be sent.";
            // 
            // warningPictureBox
            // 
            warningPictureBox.Dock = DockStyle.Fill;
            warningPictureBox.Location = new Point(8, 8);
            warningPictureBox.Margin = new Padding(8);
            warningPictureBox.Name = "warningPictureBox";
            warningPictureBox.Size = new Size(32, 32);
            warningPictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            warningPictureBox.TabIndex = 1;
            warningPictureBox.TabStop = false;
            // 
            // pnlExceptionType
            // 
            pnlExceptionType.AutoSize = true;
            pnlExceptionType.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlGeneral.SetColumnSpan(pnlExceptionType, 2);
            pnlExceptionType.Controls.Add(exceptionTextBox);
            pnlExceptionType.Controls.Add(exceptionTypeLabel);
            pnlExceptionType.Controls.Add(exceptionMessageTextBox);
            pnlExceptionType.Dock = DockStyle.Fill;
            pnlExceptionType.Location = new Point(3, 51);
            pnlExceptionType.Name = "pnlExceptionType";
            pnlExceptionType.Size = new Size(530, 73);
            pnlExceptionType.TabIndex = 1;
            // 
            // exceptionTextBox
            // 
            exceptionTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            exceptionTextBox.Location = new Point(134, 3);
            exceptionTextBox.Name = "exceptionTextBox";
            exceptionTextBox.ReadOnly = true;
            exceptionTextBox.Size = new Size(385, 20);
            exceptionTextBox.TabIndex = 2;
            // 
            // exceptionTypeLabel
            // 
            exceptionTypeLabel.Image = global::BugReporter.Properties.Resources.bug;
            exceptionTypeLabel.ImageAlign = ContentAlignment.MiddleLeft;
            exceptionTypeLabel.Location = new Point(20, 3);
            exceptionTypeLabel.Name = "exceptionTypeLabel";
            exceptionTypeLabel.Size = new Size(106, 16);
            exceptionTypeLabel.TabIndex = 1;
            exceptionTypeLabel.Text = "Exception Type:";
            exceptionTypeLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // exceptionMessageTextBox
            // 
            exceptionMessageTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            exceptionMessageTextBox.Location = new Point(8, 31);
            exceptionMessageTextBox.Margin = new Padding(3, 3, 3, 8);
            exceptionMessageTextBox.Multiline = true;
            exceptionMessageTextBox.Name = "exceptionMessageTextBox";
            exceptionMessageTextBox.ReadOnly = true;
            exceptionMessageTextBox.ScrollBars = ScrollBars.Vertical;
            exceptionMessageTextBox.Size = new Size(511, 34);
            exceptionMessageTextBox.TabIndex = 3;
            // 
            // exceptionTabPage
            // 
            exceptionTabPage.Controls.Add(exceptionDetails);
            exceptionTabPage.Location = new Point(4, 22);
            exceptionTabPage.Name = "exceptionTabPage";
            exceptionTabPage.Padding = new Padding(3);
            exceptionTabPage.Size = new Size(542, 404);
            exceptionTabPage.TabIndex = 2;
            exceptionTabPage.Text = "Exception";
            exceptionTabPage.UseVisualStyleBackColor = true;
            // 
            // exceptionDetails
            // 
            exceptionDetails.Dock = DockStyle.Fill;
            exceptionDetails.InformationColumnWidth = 350;
            exceptionDetails.Location = new Point(3, 3);
            exceptionDetails.Margin = new Padding(6);
            exceptionDetails.Name = "exceptionDetails";
            exceptionDetails.PropertyColumnWidth = 101;
            exceptionDetails.Size = new Size(536, 398);
            exceptionDetails.TabIndex = 0;
            // 
            // reportContentsTabPage
            // 
            reportContentsTabPage.Controls.Add(reportPreviewTextBox);
            reportContentsTabPage.Controls.Add(previewLabel);
            reportContentsTabPage.Controls.Add(contentsLabel);
            reportContentsTabPage.Controls.Add(reportContentsListView);
            reportContentsTabPage.Location = new Point(4, 22);
            reportContentsTabPage.Name = "reportContentsTabPage";
            reportContentsTabPage.Padding = new Padding(3);
            reportContentsTabPage.Size = new Size(542, 404);
            reportContentsTabPage.TabIndex = 3;
            reportContentsTabPage.Text = "Report Contents";
            reportContentsTabPage.UseVisualStyleBackColor = true;
            // 
            // reportPreviewTextBox
            // 
            reportPreviewTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            reportPreviewTextBox.Location = new Point(6, 142);
            reportPreviewTextBox.Multiline = true;
            reportPreviewTextBox.Name = "reportPreviewTextBox";
            reportPreviewTextBox.Size = new Size(459, 175);
            reportPreviewTextBox.TabIndex = 5;
            // 
            // previewLabel
            // 
            previewLabel.AutoSize = true;
            previewLabel.Location = new Point(6, 126);
            previewLabel.Name = "previewLabel";
            previewLabel.Size = new Size(48, 13);
            previewLabel.TabIndex = 4;
            previewLabel.Text = "Preview:";
            // 
            // contentsLabel
            // 
            contentsLabel.AutoSize = true;
            contentsLabel.Location = new Point(6, 6);
            contentsLabel.Name = "contentsLabel";
            contentsLabel.Size = new Size(288, 13);
            contentsLabel.TabIndex = 3;
            contentsLabel.Text = "Double-click an item to open it with the associated program.";
            // 
            // reportContentsListView
            // 
            reportContentsListView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            reportContentsListView.Columns.AddRange(new ColumnHeader[] {
            nameColumnHeader,
            descriptionColumnHeader,
            sizeColumnHeader});
            reportContentsListView.Location = new Point(6, 23);
            reportContentsListView.Name = "reportContentsListView";
            reportContentsListView.Size = new Size(459, 94);
            reportContentsListView.TabIndex = 0;
            reportContentsListView.UseCompatibleStateImageBehavior = false;
            reportContentsListView.View = View.Details;
            // 
            // nameColumnHeader
            // 
            nameColumnHeader.Text = "Name";
            nameColumnHeader.Width = 120;
            // 
            // descriptionColumnHeader
            // 
            descriptionColumnHeader.Text = "Description";
            descriptionColumnHeader.Width = 240;
            // 
            // sizeColumnHeader
            // 
            sizeColumnHeader.Text = "Size";
            sizeColumnHeader.Width = 80;
            // 
            // sendAndQuitButton
            // 
            sendAndQuitButton.AutoSize = true;
            sendAndQuitButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            sendAndQuitButton.Image = global::BugReporter.Properties.Resources.GitHub;
            sendAndQuitButton.Location = new Point(383, 3);
            sendAndQuitButton.MinimumSize = new Size(120, 25);
            sendAndQuitButton.Name = "sendAndQuitButton";
            sendAndQuitButton.Size = new Size(120, 25);
            sendAndQuitButton.TabIndex = 2;
            sendAndQuitButton.Text = "&Send and Quit";
            sendAndQuitButton.TextAlign = ContentAlignment.MiddleRight;
            sendAndQuitButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            sendAndQuitButton.UseVisualStyleBackColor = true;
            sendAndQuitButton.Click += SendAndQuitButton_Click;
            // 
            // quitButton
            // 
            quitButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            quitButton.AutoSize = true;
            quitButton.Location = new Point(509, 0);
            quitButton.Name = "quitButton";
            quitButton.Padding = new Padding(4, 0, 4, 0);
            quitButton.Size = new Size(34, 31);
            quitButton.TabIndex = 3;
            quitButton.TabStop = true;
            quitButton.Text = "&Quit";
            quitButton.TextAlign = ContentAlignment.MiddleCenter;
            quitButton.Click += QuitButton_Click;
            // 
            // btnCopy
            // 
            btnCopy.AutoSize = true;
            btnCopy.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCopy.Image = global::BugReporter.Properties.Resources.CopyToClipboard;
            btnCopy.Location = new Point(7, 3);
            btnCopy.MinimumSize = new Size(75, 25);
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new Size(75, 25);
            btnCopy.TabIndex = 0;
            btnCopy.Text = "&Copy";
            btnCopy.TextAlign = ContentAlignment.MiddleRight;
            btnCopy.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnCopy.UseVisualStyleBackColor = true;
            btnCopy.Click += btnCopy_Click;
            // 
            // toolTip
            // 
            toolTip.AutomaticDelay = 100;
            toolTip.UseAnimation = false;
            toolTip.UseFading = false;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(quitButton, 3, 0);
            tableLayoutPanel1.Controls.Add(sendAndQuitButton, 2, 0);
            tableLayoutPanel1.Controls.Add(IgnoreButton, 1, 0);
            tableLayoutPanel1.Controls.Add(btnCopy, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Bottom;
            tableLayoutPanel1.Location = new Point(4, 434);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new Padding(4, 0, 4, 0);
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(550, 31);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // IgnoreButton
            // 
            IgnoreButton.AutoSize = true;
            IgnoreButton.Location = new Point(302, 3);
            IgnoreButton.Name = "IgnoreButton";
            IgnoreButton.Size = new Size(75, 25);
            IgnoreButton.TabIndex = 1;
            IgnoreButton.Text = "&Ignore";
            IgnoreButton.UseVisualStyleBackColor = true;
            IgnoreButton.Click += IgnoreButton_Click;
            // 
            // BugReportForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = quitButton;
            ClientSize = new Size(558, 469);
            Controls.Add(mainTabs);
            Controls.Add(tableLayoutPanel1);
            MinimizeBox = false;
            Name = "BugReportForm";
            Padding = new Padding(4);
            SizeGripStyle = SizeGripStyle.Show;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;
            mainTabs.ResumeLayout(false);
            generalTabPage.ResumeLayout(false);
            generalTabPage.PerformLayout();
            tpnlEnvInfo.ResumeLayout(false);
            tpnlEnvInfo.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            tlpnlGeneral.ResumeLayout(false);
            tlpnlGeneral.PerformLayout();
            pnlHeading.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(warningPictureBox)).EndInit();
            pnlExceptionType.ResumeLayout(false);
            pnlExceptionType.PerformLayout();
            exceptionTabPage.ResumeLayout(false);
            reportContentsTabPage.ResumeLayout(false);
            reportContentsTabPage.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TabControl mainTabs;
        private TabPage generalTabPage;
        private LinkLabel quitButton;
        private Button btnCopy;
        private TabPage exceptionTabPage;
        private PictureBox warningPictureBox;
        private TextBox exceptionMessageTextBox;
        private TextBox exceptionTextBox;
        private TextBox targetSiteTextBox;
        private Label targetSiteLabel;
        private TextBox gitTextBox;
        private Label _NO_TRANSLATE_GitLabel;
        private TextBox applicationTextBox;
        private Label applicationLabel;
        private TextBox descriptionTextBox;
        private Label errorDescriptionLabel;
        private TextBox clrTextBox;
        private Label _NO_TRANSLATE_ClrLabel;
        private TextBox dateTimeTextBox;
        private Label dateTimeLabel;
        private TabPage reportContentsTabPage;
        private TextBox reportPreviewTextBox;
        private Label previewLabel;
        private Label contentsLabel;
        private ListView reportContentsListView;
        private ColumnHeader nameColumnHeader;
        private ColumnHeader descriptionColumnHeader;
        private ColumnHeader sizeColumnHeader;
        private Label exceptionTypeLabel;
        private ToolTip toolTip;
        private Label warningLabel;
        private ExceptionDetails exceptionDetails;
        private Button sendAndQuitButton;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private TableLayoutPanel tpnlEnvInfo;
        private TableLayoutPanel tlpnlGeneral;
        private Panel pnlHeading;
        private Panel pnlExceptionType;
        private Button IgnoreButton;
    }
}
