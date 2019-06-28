namespace GitUI.NBugReports
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BugReportForm));
            this.mainTabs = new System.Windows.Forms.TabControl();
            this.generalTabPage = new System.Windows.Forms.TabPage();
            this.tpnlEnvInfo = new System.Windows.Forms.TableLayoutPanel();
            this.clrTextBox = new System.Windows.Forms.TextBox();
            this._NO_TRANSLATE_ClrLabel = new System.Windows.Forms.Label();
            this._NO_TRANSLATE_GitLabel = new System.Windows.Forms.Label();
            this.gitTextBox = new System.Windows.Forms.TextBox();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.dateTimeTextBox = new System.Windows.Forms.TextBox();
            this.errorDescriptionLabel = new System.Windows.Forms.Label();
            this.applicationTextBox = new System.Windows.Forms.TextBox();
            this.targetSiteLabel = new System.Windows.Forms.Label();
            this.targetSiteTextBox = new System.Windows.Forms.TextBox();
            this.applicationLabel = new System.Windows.Forms.Label();
            this.dateTimeLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tlpnlGeneral = new System.Windows.Forms.TableLayoutPanel();
            this.pnlHeading = new System.Windows.Forms.Panel();
            this.warningLabel = new System.Windows.Forms.Label();
            this.warningPictureBox = new System.Windows.Forms.PictureBox();
            this.pnlExceptionType = new System.Windows.Forms.Panel();
            this.exceptionTextBox = new System.Windows.Forms.TextBox();
            this.exceptionTypeLabel = new System.Windows.Forms.Label();
            this.exceptionMessageTextBox = new System.Windows.Forms.TextBox();
            this.exceptionTabPage = new System.Windows.Forms.TabPage();
            this.exceptionDetails = new GitUI.NBugReports.ExceptionDetails();
            this.reportContentsTabPage = new System.Windows.Forms.TabPage();
            this.reportPreviewTextBox = new System.Windows.Forms.TextBox();
            this.previewLabel = new System.Windows.Forms.Label();
            this.contentsLabel = new System.Windows.Forms.Label();
            this.reportContentsListView = new System.Windows.Forms.ListView();
            this.nameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.descriptionColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.sizeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.sendAndQuitButton = new System.Windows.Forms.Button();
            this.quitButton = new System.Windows.Forms.LinkLabel();
            this.btnCopy = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.mainTabs.SuspendLayout();
            this.generalTabPage.SuspendLayout();
            this.tpnlEnvInfo.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tlpnlGeneral.SuspendLayout();
            this.pnlHeading.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.warningPictureBox)).BeginInit();
            this.pnlExceptionType.SuspendLayout();
            this.exceptionTabPage.SuspendLayout();
            this.reportContentsTabPage.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTabs
            // 
            this.mainTabs.Controls.Add(this.generalTabPage);
            this.mainTabs.Controls.Add(this.exceptionTabPage);
            this.mainTabs.Controls.Add(this.reportContentsTabPage);
            this.mainTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabs.Location = new System.Drawing.Point(4, 4);
            this.mainTabs.Margin = new System.Windows.Forms.Padding(0);
            this.mainTabs.Name = "mainTabs";
            this.mainTabs.SelectedIndex = 0;
            this.mainTabs.Size = new System.Drawing.Size(550, 430);
            this.mainTabs.TabIndex = 0;
            // 
            // generalTabPage
            // 
            this.generalTabPage.Controls.Add(this.tpnlEnvInfo);
            this.generalTabPage.Controls.Add(this.panel1);
            this.generalTabPage.Location = new System.Drawing.Point(4, 22);
            this.generalTabPage.Name = "generalTabPage";
            this.generalTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.generalTabPage.Size = new System.Drawing.Size(542, 404);
            this.generalTabPage.TabIndex = 0;
            this.generalTabPage.Text = "General";
            this.generalTabPage.UseVisualStyleBackColor = true;
            // 
            // tpnlEnvInfo
            // 
            this.tpnlEnvInfo.ColumnCount = 5;
            this.tpnlEnvInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18F));
            this.tpnlEnvInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
            this.tpnlEnvInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18F));
            this.tpnlEnvInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
            this.tpnlEnvInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tpnlEnvInfo.Controls.Add(this.clrTextBox, 3, 2);
            this.tpnlEnvInfo.Controls.Add(this._NO_TRANSLATE_ClrLabel, 2, 2);
            this.tpnlEnvInfo.Controls.Add(this._NO_TRANSLATE_GitLabel, 2, 1);
            this.tpnlEnvInfo.Controls.Add(this.gitTextBox, 3, 1);
            this.tpnlEnvInfo.Controls.Add(this.descriptionTextBox, 0, 5);
            this.tpnlEnvInfo.Controls.Add(this.dateTimeTextBox, 1, 2);
            this.tpnlEnvInfo.Controls.Add(this.errorDescriptionLabel, 0, 4);
            this.tpnlEnvInfo.Controls.Add(this.applicationTextBox, 1, 1);
            this.tpnlEnvInfo.Controls.Add(this.targetSiteLabel, 0, 0);
            this.tpnlEnvInfo.Controls.Add(this.targetSiteTextBox, 1, 0);
            this.tpnlEnvInfo.Controls.Add(this.applicationLabel, 0, 1);
            this.tpnlEnvInfo.Controls.Add(this.dateTimeLabel, 0, 2);
            this.tpnlEnvInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tpnlEnvInfo.Location = new System.Drawing.Point(3, 130);
            this.tpnlEnvInfo.Name = "tpnlEnvInfo";
            this.tpnlEnvInfo.Padding = new System.Windows.Forms.Padding(8);
            this.tpnlEnvInfo.RowCount = 6;
            this.tpnlEnvInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tpnlEnvInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tpnlEnvInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tpnlEnvInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tpnlEnvInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tpnlEnvInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tpnlEnvInfo.Size = new System.Drawing.Size(536, 271);
            this.tpnlEnvInfo.TabIndex = 1;
            // 
            // clrTextBox
            // 
            this.clrTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clrTextBox.Location = new System.Drawing.Point(363, 63);
            this.clrTextBox.MaxLength = 100;
            this.clrTextBox.Name = "clrTextBox";
            this.clrTextBox.ReadOnly = true;
            this.clrTextBox.Size = new System.Drawing.Size(160, 20);
            this.clrTextBox.TabIndex = 9;
            // 
            // clrLabel
            // 
            this._NO_TRANSLATE_ClrLabel.AutoSize = true;
            this._NO_TRANSLATE_ClrLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_ClrLabel.Location = new System.Drawing.Point(270, 60);
            this._NO_TRANSLATE_ClrLabel.Name = "_NO_TRANSLATE_ClrLabel";
            this._NO_TRANSLATE_ClrLabel.Size = new System.Drawing.Size(87, 26);
            this._NO_TRANSLATE_ClrLabel.TabIndex = 8;
            this._NO_TRANSLATE_ClrLabel.Text = "CLR:";
            this._NO_TRANSLATE_ClrLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gitLabel
            // 
            this._NO_TRANSLATE_GitLabel.AutoSize = true;
            this._NO_TRANSLATE_GitLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._NO_TRANSLATE_GitLabel.Location = new System.Drawing.Point(270, 34);
            this._NO_TRANSLATE_GitLabel.Name = "_NO_TRANSLATE_GitLabel";
            this._NO_TRANSLATE_GitLabel.Size = new System.Drawing.Size(87, 26);
            this._NO_TRANSLATE_GitLabel.TabIndex = 4;
            this._NO_TRANSLATE_GitLabel.Text = "Git:";
            this._NO_TRANSLATE_GitLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gitTextBox
            // 
            this.gitTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gitTextBox.Location = new System.Drawing.Point(363, 37);
            this.gitTextBox.MaxLength = 100;
            this.gitTextBox.Name = "gitTextBox";
            this.gitTextBox.ReadOnly = true;
            this.gitTextBox.Size = new System.Drawing.Size(160, 20);
            this.gitTextBox.TabIndex = 5;
            // 
            // descriptionTextBox
            // 
            this.tpnlEnvInfo.SetColumnSpan(this.descriptionTextBox, 4);
            this.descriptionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.descriptionTextBox.Location = new System.Drawing.Point(11, 110);
            this.descriptionTextBox.Multiline = true;
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(512, 150);
            this.descriptionTextBox.TabIndex = 11;
            // 
            // dateTimeTextBox
            // 
            this.dateTimeTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimeTextBox.Location = new System.Drawing.Point(104, 63);
            this.dateTimeTextBox.MaxLength = 100;
            this.dateTimeTextBox.Name = "dateTimeTextBox";
            this.dateTimeTextBox.ReadOnly = true;
            this.dateTimeTextBox.Size = new System.Drawing.Size(160, 20);
            this.dateTimeTextBox.TabIndex = 7;
            // 
            // errorDescriptionLabel
            // 
            this.errorDescriptionLabel.AutoSize = true;
            this.tpnlEnvInfo.SetColumnSpan(this.errorDescriptionLabel, 4);
            this.errorDescriptionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.errorDescriptionLabel.Location = new System.Drawing.Point(11, 94);
            this.errorDescriptionLabel.Name = "errorDescriptionLabel";
            this.errorDescriptionLabel.Size = new System.Drawing.Size(512, 13);
            this.errorDescriptionLabel.TabIndex = 10;
            this.errorDescriptionLabel.Text = "Please add a brief description (in English) of how we can reproduce the error:";
            this.errorDescriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // applicationTextBox
            // 
            this.applicationTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.applicationTextBox.Location = new System.Drawing.Point(104, 37);
            this.applicationTextBox.MaxLength = 100;
            this.applicationTextBox.Name = "applicationTextBox";
            this.applicationTextBox.ReadOnly = true;
            this.applicationTextBox.Size = new System.Drawing.Size(160, 20);
            this.applicationTextBox.TabIndex = 3;
            // 
            // targetSiteLabel
            // 
            this.targetSiteLabel.AutoSize = true;
            this.targetSiteLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.targetSiteLabel.Location = new System.Drawing.Point(11, 8);
            this.targetSiteLabel.Name = "targetSiteLabel";
            this.targetSiteLabel.Size = new System.Drawing.Size(87, 26);
            this.targetSiteLabel.TabIndex = 0;
            this.targetSiteLabel.Text = "Target Site:";
            this.targetSiteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // targetSiteTextBox
            // 
            this.tpnlEnvInfo.SetColumnSpan(this.targetSiteTextBox, 3);
            this.targetSiteTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.targetSiteTextBox.Location = new System.Drawing.Point(104, 11);
            this.targetSiteTextBox.MaxLength = 500;
            this.targetSiteTextBox.Name = "targetSiteTextBox";
            this.targetSiteTextBox.ReadOnly = true;
            this.targetSiteTextBox.Size = new System.Drawing.Size(419, 20);
            this.targetSiteTextBox.TabIndex = 1;
            // 
            // applicationLabel
            // 
            this.applicationLabel.AutoSize = true;
            this.applicationLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.applicationLabel.Location = new System.Drawing.Point(11, 34);
            this.applicationLabel.Name = "applicationLabel";
            this.applicationLabel.Size = new System.Drawing.Size(87, 26);
            this.applicationLabel.TabIndex = 2;
            this.applicationLabel.Text = "Application:";
            this.applicationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dateTimeLabel
            // 
            this.dateTimeLabel.AutoSize = true;
            this.dateTimeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimeLabel.Location = new System.Drawing.Point(11, 60);
            this.dateTimeLabel.Name = "dateTimeLabel";
            this.dateTimeLabel.Size = new System.Drawing.Size(87, 26);
            this.dateTimeLabel.TabIndex = 6;
            this.dateTimeLabel.Text = "Date/Time:";
            this.dateTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.tlpnlGeneral);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(536, 127);
            this.panel1.TabIndex = 0;
            // 
            // tlpnlGeneral
            // 
            this.tlpnlGeneral.AutoSize = true;
            this.tlpnlGeneral.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlGeneral.ColumnCount = 2;
            this.tlpnlGeneral.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpnlGeneral.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpnlGeneral.Controls.Add(this.pnlHeading, 1, 0);
            this.tlpnlGeneral.Controls.Add(this.warningPictureBox, 0, 0);
            this.tlpnlGeneral.Controls.Add(this.pnlExceptionType, 0, 1);
            this.tlpnlGeneral.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpnlGeneral.Location = new System.Drawing.Point(0, 0);
            this.tlpnlGeneral.Name = "tlpnlGeneral";
            this.tlpnlGeneral.RowCount = 2;
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpnlGeneral.Size = new System.Drawing.Size(536, 127);
            this.tlpnlGeneral.TabIndex = 0;
            // 
            // pnlHeading
            // 
            this.pnlHeading.AutoSize = true;
            this.pnlHeading.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlHeading.Controls.Add(this.warningLabel);
            this.pnlHeading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeading.Location = new System.Drawing.Point(51, 3);
            this.pnlHeading.Name = "pnlHeading";
            this.pnlHeading.Size = new System.Drawing.Size(482, 42);
            this.pnlHeading.TabIndex = 0;
            // 
            // warningLabel
            // 
            this.warningLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.warningLabel.Location = new System.Drawing.Point(0, 0);
            this.warningLabel.Name = "warningLabel";
            this.warningLabel.Size = new System.Drawing.Size(482, 42);
            this.warningLabel.TabIndex = 0;
            this.warningLabel.Text = resources.GetString("warningLabel.Text");
            // 
            // warningPictureBox
            // 
            this.warningPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.warningPictureBox.Location = new System.Drawing.Point(8, 8);
            this.warningPictureBox.Margin = new System.Windows.Forms.Padding(8);
            this.warningPictureBox.Name = "warningPictureBox";
            this.warningPictureBox.Size = new System.Drawing.Size(32, 32);
            this.warningPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.warningPictureBox.TabIndex = 1;
            this.warningPictureBox.TabStop = false;
            // 
            // pnlExceptionType
            // 
            this.pnlExceptionType.AutoSize = true;
            this.pnlExceptionType.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpnlGeneral.SetColumnSpan(this.pnlExceptionType, 2);
            this.pnlExceptionType.Controls.Add(this.exceptionTextBox);
            this.pnlExceptionType.Controls.Add(this.exceptionTypeLabel);
            this.pnlExceptionType.Controls.Add(this.exceptionMessageTextBox);
            this.pnlExceptionType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlExceptionType.Location = new System.Drawing.Point(3, 51);
            this.pnlExceptionType.Name = "pnlExceptionType";
            this.pnlExceptionType.Size = new System.Drawing.Size(530, 73);
            this.pnlExceptionType.TabIndex = 1;
            // 
            // exceptionTextBox
            // 
            this.exceptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.exceptionTextBox.Location = new System.Drawing.Point(134, 3);
            this.exceptionTextBox.Name = "exceptionTextBox";
            this.exceptionTextBox.ReadOnly = true;
            this.exceptionTextBox.Size = new System.Drawing.Size(385, 20);
            this.exceptionTextBox.TabIndex = 2;
            // 
            // exceptionTypeLabel
            // 
            this.exceptionTypeLabel.Image = global::GitUI.Properties.Resources.bug;
            this.exceptionTypeLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.exceptionTypeLabel.Location = new System.Drawing.Point(20, 3);
            this.exceptionTypeLabel.Name = "exceptionTypeLabel";
            this.exceptionTypeLabel.Size = new System.Drawing.Size(106, 16);
            this.exceptionTypeLabel.TabIndex = 1;
            this.exceptionTypeLabel.Text = "Exception Type:";
            this.exceptionTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // exceptionMessageTextBox
            // 
            this.exceptionMessageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.exceptionMessageTextBox.Location = new System.Drawing.Point(8, 31);
            this.exceptionMessageTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
            this.exceptionMessageTextBox.Multiline = true;
            this.exceptionMessageTextBox.Name = "exceptionMessageTextBox";
            this.exceptionMessageTextBox.ReadOnly = true;
            this.exceptionMessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.exceptionMessageTextBox.Size = new System.Drawing.Size(511, 34);
            this.exceptionMessageTextBox.TabIndex = 3;
            // 
            // exceptionTabPage
            // 
            this.exceptionTabPage.Controls.Add(this.exceptionDetails);
            this.exceptionTabPage.Location = new System.Drawing.Point(4, 22);
            this.exceptionTabPage.Name = "exceptionTabPage";
            this.exceptionTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.exceptionTabPage.Size = new System.Drawing.Size(542, 404);
            this.exceptionTabPage.TabIndex = 2;
            this.exceptionTabPage.Text = "Exception";
            this.exceptionTabPage.UseVisualStyleBackColor = true;
            // 
            // exceptionDetails
            // 
            this.exceptionDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exceptionDetails.InformationColumnWidth = 350;
            this.exceptionDetails.Location = new System.Drawing.Point(3, 3);
            this.exceptionDetails.Margin = new System.Windows.Forms.Padding(6);
            this.exceptionDetails.Name = "exceptionDetails";
            this.exceptionDetails.PropertyColumnWidth = 101;
            this.exceptionDetails.Size = new System.Drawing.Size(536, 398);
            this.exceptionDetails.TabIndex = 0;
            // 
            // reportContentsTabPage
            // 
            this.reportContentsTabPage.Controls.Add(this.reportPreviewTextBox);
            this.reportContentsTabPage.Controls.Add(this.previewLabel);
            this.reportContentsTabPage.Controls.Add(this.contentsLabel);
            this.reportContentsTabPage.Controls.Add(this.reportContentsListView);
            this.reportContentsTabPage.Location = new System.Drawing.Point(4, 22);
            this.reportContentsTabPage.Name = "reportContentsTabPage";
            this.reportContentsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.reportContentsTabPage.Size = new System.Drawing.Size(542, 404);
            this.reportContentsTabPage.TabIndex = 3;
            this.reportContentsTabPage.Text = "Report Contents";
            this.reportContentsTabPage.UseVisualStyleBackColor = true;
            // 
            // reportPreviewTextBox
            // 
            this.reportPreviewTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.reportPreviewTextBox.Location = new System.Drawing.Point(6, 142);
            this.reportPreviewTextBox.Multiline = true;
            this.reportPreviewTextBox.Name = "reportPreviewTextBox";
            this.reportPreviewTextBox.Size = new System.Drawing.Size(459, 175);
            this.reportPreviewTextBox.TabIndex = 5;
            // 
            // previewLabel
            // 
            this.previewLabel.AutoSize = true;
            this.previewLabel.Location = new System.Drawing.Point(6, 126);
            this.previewLabel.Name = "previewLabel";
            this.previewLabel.Size = new System.Drawing.Size(48, 13);
            this.previewLabel.TabIndex = 4;
            this.previewLabel.Text = "Preview:";
            // 
            // contentsLabel
            // 
            this.contentsLabel.AutoSize = true;
            this.contentsLabel.Location = new System.Drawing.Point(6, 6);
            this.contentsLabel.Name = "contentsLabel";
            this.contentsLabel.Size = new System.Drawing.Size(288, 13);
            this.contentsLabel.TabIndex = 3;
            this.contentsLabel.Text = "Double-click an item to open it with the associated program.";
            // 
            // reportContentsListView
            // 
            this.reportContentsListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.reportContentsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumnHeader,
            this.descriptionColumnHeader,
            this.sizeColumnHeader});
            this.reportContentsListView.Location = new System.Drawing.Point(6, 23);
            this.reportContentsListView.Name = "reportContentsListView";
            this.reportContentsListView.Size = new System.Drawing.Size(459, 94);
            this.reportContentsListView.TabIndex = 0;
            this.reportContentsListView.UseCompatibleStateImageBehavior = false;
            this.reportContentsListView.View = System.Windows.Forms.View.Details;
            // 
            // nameColumnHeader
            // 
            this.nameColumnHeader.Text = "Name";
            this.nameColumnHeader.Width = 120;
            // 
            // descriptionColumnHeader
            // 
            this.descriptionColumnHeader.Text = "Description";
            this.descriptionColumnHeader.Width = 240;
            // 
            // sizeColumnHeader
            // 
            this.sizeColumnHeader.Text = "Size";
            this.sizeColumnHeader.Width = 80;
            // 
            // sendAndQuitButton
            // 
            this.sendAndQuitButton.AutoSize = true;
            this.sendAndQuitButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.sendAndQuitButton.Image = global::GitUI.Properties.Images.GitHub;
            this.sendAndQuitButton.Location = new System.Drawing.Point(383, 3);
            this.sendAndQuitButton.MinimumSize = new System.Drawing.Size(120, 25);
            this.sendAndQuitButton.Name = "sendAndQuitButton";
            this.sendAndQuitButton.Size = new System.Drawing.Size(120, 25);
            this.sendAndQuitButton.TabIndex = 1;
            this.sendAndQuitButton.Text = "&Send and Quit";
            this.sendAndQuitButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.sendAndQuitButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.sendAndQuitButton.UseVisualStyleBackColor = true;
            this.sendAndQuitButton.Click += new System.EventHandler(this.SendAndQuitButton_Click);
            // 
            // quitButton
            // 
            this.quitButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.quitButton.AutoSize = true;
            this.quitButton.Location = new System.Drawing.Point(509, 0);
            this.quitButton.Name = "quitButton";
            this.quitButton.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.quitButton.Size = new System.Drawing.Size(34, 31);
            this.quitButton.TabIndex = 2;
            this.quitButton.TabStop = true;
            this.quitButton.Text = "&Quit";
            this.quitButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.quitButton.Click += new System.EventHandler(this.QuitButton_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.AutoSize = true;
            this.btnCopy.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCopy.Image = global::GitUI.Properties.Resources.CopyToClipboard;
            this.btnCopy.Location = new System.Drawing.Point(7, 3);
            this.btnCopy.MinimumSize = new System.Drawing.Size(75, 25);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(75, 25);
            this.btnCopy.TabIndex = 0;
            this.btnCopy.Text = "&Copy";
            this.btnCopy.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCopy.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // toolTip
            // 
            this.toolTip.AutomaticDelay = 100;
            this.toolTip.UseAnimation = false;
            this.toolTip.UseFading = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.quitButton, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.sendAndQuitButton, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnCopy, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(4, 434);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(550, 31);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // BugReportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.quitButton;
            this.ClientSize = new System.Drawing.Size(558, 469);
            this.Controls.Add(this.mainTabs);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BugReportForm";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.mainTabs.ResumeLayout(false);
            this.generalTabPage.ResumeLayout(false);
            this.generalTabPage.PerformLayout();
            this.tpnlEnvInfo.ResumeLayout(false);
            this.tpnlEnvInfo.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tlpnlGeneral.ResumeLayout(false);
            this.tlpnlGeneral.PerformLayout();
            this.pnlHeading.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.warningPictureBox)).EndInit();
            this.pnlExceptionType.ResumeLayout(false);
            this.pnlExceptionType.PerformLayout();
            this.exceptionTabPage.ResumeLayout(false);
            this.reportContentsTabPage.ResumeLayout(false);
            this.reportContentsTabPage.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl mainTabs;
        private System.Windows.Forms.TabPage generalTabPage;
        private System.Windows.Forms.LinkLabel quitButton;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.TabPage exceptionTabPage;
        private System.Windows.Forms.PictureBox warningPictureBox;
        private System.Windows.Forms.TextBox exceptionMessageTextBox;
        private System.Windows.Forms.TextBox exceptionTextBox;
        private System.Windows.Forms.TextBox targetSiteTextBox;
        private System.Windows.Forms.Label targetSiteLabel;
        private System.Windows.Forms.TextBox gitTextBox;
        private System.Windows.Forms.Label _NO_TRANSLATE_GitLabel;
        private System.Windows.Forms.TextBox applicationTextBox;
        private System.Windows.Forms.Label applicationLabel;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.Label errorDescriptionLabel;
        private System.Windows.Forms.TextBox clrTextBox;
        private System.Windows.Forms.Label _NO_TRANSLATE_ClrLabel;
        private System.Windows.Forms.TextBox dateTimeTextBox;
        private System.Windows.Forms.Label dateTimeLabel;
        private System.Windows.Forms.TabPage reportContentsTabPage;
        private System.Windows.Forms.TextBox reportPreviewTextBox;
        private System.Windows.Forms.Label previewLabel;
        private System.Windows.Forms.Label contentsLabel;
        private System.Windows.Forms.ListView reportContentsListView;
        private System.Windows.Forms.ColumnHeader nameColumnHeader;
        private System.Windows.Forms.ColumnHeader descriptionColumnHeader;
        private System.Windows.Forms.ColumnHeader sizeColumnHeader;
        private System.Windows.Forms.Label exceptionTypeLabel;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label warningLabel;
        private ExceptionDetails exceptionDetails;
        private System.Windows.Forms.Button sendAndQuitButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tpnlEnvInfo;
        private System.Windows.Forms.TableLayoutPanel tlpnlGeneral;
        private System.Windows.Forms.Panel pnlHeading;
        private System.Windows.Forms.Panel pnlExceptionType;
    }
}