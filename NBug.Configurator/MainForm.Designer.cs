namespace NBug.Configurator
{
	partial class MainForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.mainStatusStrip = new System.Windows.Forms.StatusStrip();
			this.status = new System.Windows.Forms.ToolStripStatusLabel();
			this.mainTabs = new System.Windows.Forms.TabControl();
			this.generalTabPage = new System.Windows.Forms.TabPage();
			this.nbugConfigurationGroupBox = new System.Windows.Forms.GroupBox();
			this.releaseModeCheckBox = new System.Windows.Forms.CheckBox();
			this.internalLoggerGroupBox = new System.Windows.Forms.GroupBox();
			this.networkTraceWarningLabel = new System.Windows.Forms.Label();
			this.writeNetworkTraceToFileCheckBox = new System.Windows.Forms.CheckBox();
			this.writeLogToDiskCheckBox = new System.Windows.Forms.CheckBox();
			this.reportSubmitterGroupBox = new System.Windows.Forms.GroupBox();
			this.encryptConnectionStringsCheckBox = new System.Windows.Forms.CheckBox();
			this.reportQueueGroupBox = new System.Windows.Forms.GroupBox();
			this.storagePathLabel = new System.Windows.Forms.Label();
			this.storagePathComboBox = new System.Windows.Forms.ComboBox();
			this.customPathLabel = new System.Windows.Forms.Label();
			this.customStoragePathTextBox = new System.Windows.Forms.TextBox();
			this.customPathTipLabel = new System.Windows.Forms.Label();
			this.reportingGroupBox = new System.Windows.Forms.GroupBox();
			this.miniDumpTypeLabel = new System.Windows.Forms.Label();
			this.miniDumpTypeComboBox = new System.Windows.Forms.ComboBox();
			this.sleepBeforeSendLabel = new System.Windows.Forms.Label();
			this.sleepBeforeSendNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.maxQueuedReportsLabel = new System.Windows.Forms.Label();
			this.maxQueuedReportsNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.sleepBeforeSendUnitLabel = new System.Windows.Forms.Label();
			this.stopReportingAfterLabel = new System.Windows.Forms.Label();
			this.stopReportingAfterUnitLabel = new System.Windows.Forms.Label();
			this.stopReportingAfterNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.userInterfaceGroupBox = new System.Windows.Forms.GroupBox();
			this.previewButton = new System.Windows.Forms.Button();
			this.uiModeLabel = new System.Windows.Forms.Label();
			this.uiModeComboBox = new System.Windows.Forms.ComboBox();
			this.uiProviderLabel = new System.Windows.Forms.Label();
			this.uiProviderComboBox = new System.Windows.Forms.ComboBox();
			this.advancedTabPage = new System.Windows.Forms.TabPage();
			this.exceptionHandlingGroupBox = new System.Windows.Forms.GroupBox();
			this.exitApplicationImmediatelyWarningLabel = new System.Windows.Forms.Label();
			this.handleProcessCorruptedStateExceptionsCheckBox = new System.Windows.Forms.CheckBox();
			this.exitApplicationImmediatelyCheckBox = new System.Windows.Forms.CheckBox();
			this.warningLabel = new System.Windows.Forms.Label();
			this.submit1TabPage = new System.Windows.Forms.TabPage();
			this.panelLoader1 = new NBug.Configurator.SubmitPanels.PanelLoader();
			this.submit2TabPage = new System.Windows.Forms.TabPage();
			this.panelLoader2 = new NBug.Configurator.SubmitPanels.PanelLoader();
			this.submit3TabPage = new System.Windows.Forms.TabPage();
			this.panelLoader3 = new NBug.Configurator.SubmitPanels.PanelLoader();
			this.submit4TabPage = new System.Windows.Forms.TabPage();
			this.panelLoader4 = new NBug.Configurator.SubmitPanels.PanelLoader();
			this.submit5TabPage = new System.Windows.Forms.TabPage();
			this.panelLoader5 = new NBug.Configurator.SubmitPanels.PanelLoader();
			this.fileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.externalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.embeddedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.testAppToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.projectHomeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.onlineDocumentationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.discussionForumToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.bugTrackerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
			this.saveButton = new System.Windows.Forms.Button();
			this.closeButton = new System.Windows.Forms.Button();
			this.mainToolTips = new System.Windows.Forms.ToolTip(this.components);
			this.mainHelpProvider = new System.Windows.Forms.HelpProvider();
			this.runTestAppButton = new System.Windows.Forms.Button();
			this.settingsFileGroupBox = new System.Windows.Forms.GroupBox();
			this.fileTextBox = new System.Windows.Forms.TextBox();
			this.pathLabel = new System.Windows.Forms.Label();
			this.createButton = new System.Windows.Forms.Button();
			this.openButton = new System.Windows.Forms.Button();
			this.createFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.mainStatusStrip.SuspendLayout();
			this.mainTabs.SuspendLayout();
			this.generalTabPage.SuspendLayout();
			this.nbugConfigurationGroupBox.SuspendLayout();
			this.internalLoggerGroupBox.SuspendLayout();
			this.reportSubmitterGroupBox.SuspendLayout();
			this.reportQueueGroupBox.SuspendLayout();
			this.reportingGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sleepBeforeSendNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.maxQueuedReportsNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.stopReportingAfterNumericUpDown)).BeginInit();
			this.userInterfaceGroupBox.SuspendLayout();
			this.advancedTabPage.SuspendLayout();
			this.exceptionHandlingGroupBox.SuspendLayout();
			this.submit1TabPage.SuspendLayout();
			this.submit2TabPage.SuspendLayout();
			this.submit3TabPage.SuspendLayout();
			this.submit4TabPage.SuspendLayout();
			this.submit5TabPage.SuspendLayout();
			this.mainMenuStrip.SuspendLayout();
			this.settingsFileGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainStatusStrip
			// 
			this.mainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.status});
			this.mainStatusStrip.Location = new System.Drawing.Point(0, 472);
			this.mainStatusStrip.Name = "mainStatusStrip";
			this.mainStatusStrip.Size = new System.Drawing.Size(703, 22);
			this.mainStatusStrip.TabIndex = 1;
			// 
			// status
			// 
			this.status.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.status.Name = "status";
			this.status.Size = new System.Drawing.Size(0, 17);
			// 
			// mainTabs
			// 
			this.mainTabs.Controls.Add(this.generalTabPage);
			this.mainTabs.Controls.Add(this.advancedTabPage);
			this.mainTabs.Controls.Add(this.submit1TabPage);
			this.mainTabs.Controls.Add(this.submit2TabPage);
			this.mainTabs.Controls.Add(this.submit3TabPage);
			this.mainTabs.Controls.Add(this.submit4TabPage);
			this.mainTabs.Controls.Add(this.submit5TabPage);
			this.mainTabs.Enabled = false;
			this.mainTabs.Location = new System.Drawing.Point(12, 105);
			this.mainTabs.Name = "mainTabs";
			this.mainTabs.SelectedIndex = 0;
			this.mainTabs.Size = new System.Drawing.Size(679, 326);
			this.mainTabs.TabIndex = 2;
			// 
			// generalTabPage
			// 
			this.generalTabPage.Controls.Add(this.nbugConfigurationGroupBox);
			this.generalTabPage.Controls.Add(this.internalLoggerGroupBox);
			this.generalTabPage.Controls.Add(this.reportSubmitterGroupBox);
			this.generalTabPage.Controls.Add(this.reportQueueGroupBox);
			this.generalTabPage.Controls.Add(this.reportingGroupBox);
			this.generalTabPage.Controls.Add(this.userInterfaceGroupBox);
			this.generalTabPage.Location = new System.Drawing.Point(4, 22);
			this.generalTabPage.Name = "generalTabPage";
			this.generalTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.generalTabPage.Size = new System.Drawing.Size(671, 300);
			this.generalTabPage.TabIndex = 0;
			this.generalTabPage.Text = "General";
			this.generalTabPage.UseVisualStyleBackColor = true;
			// 
			// nbugConfigurationGroupBox
			// 
			this.nbugConfigurationGroupBox.Controls.Add(this.releaseModeCheckBox);
			this.nbugConfigurationGroupBox.Location = new System.Drawing.Point(473, 224);
			this.nbugConfigurationGroupBox.Name = "nbugConfigurationGroupBox";
			this.nbugConfigurationGroupBox.Size = new System.Drawing.Size(140, 48);
			this.nbugConfigurationGroupBox.TabIndex = 26;
			this.nbugConfigurationGroupBox.TabStop = false;
			this.nbugConfigurationGroupBox.Text = "NBug Configuration";
			// 
			// releaseModeCheckBox
			// 
			this.releaseModeCheckBox.AutoSize = true;
			this.releaseModeCheckBox.Location = new System.Drawing.Point(12, 19);
			this.releaseModeCheckBox.Name = "releaseModeCheckBox";
			this.releaseModeCheckBox.Size = new System.Drawing.Size(95, 17);
			this.releaseModeCheckBox.TabIndex = 21;
			this.releaseModeCheckBox.Text = "Release Mode";
			this.mainToolTips.SetToolTip(this.releaseModeCheckBox, resources.GetString("releaseModeCheckBox.ToolTip"));
			this.releaseModeCheckBox.UseVisualStyleBackColor = true;
			// 
			// internalLoggerGroupBox
			// 
			this.internalLoggerGroupBox.Controls.Add(this.networkTraceWarningLabel);
			this.internalLoggerGroupBox.Controls.Add(this.writeNetworkTraceToFileCheckBox);
			this.internalLoggerGroupBox.Controls.Add(this.writeLogToDiskCheckBox);
			this.internalLoggerGroupBox.Location = new System.Drawing.Point(297, 131);
			this.internalLoggerGroupBox.Name = "internalLoggerGroupBox";
			this.internalLoggerGroupBox.Size = new System.Drawing.Size(316, 85);
			this.internalLoggerGroupBox.TabIndex = 26;
			this.internalLoggerGroupBox.TabStop = false;
			this.internalLoggerGroupBox.Text = "Internal Logger";
			// 
			// networkTraceWarningLabel
			// 
			this.networkTraceWarningLabel.AutoSize = true;
			this.networkTraceWarningLabel.Enabled = false;
			this.networkTraceWarningLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.networkTraceWarningLabel.Location = new System.Drawing.Point(30, 62);
			this.networkTraceWarningLabel.Name = "networkTraceWarningLabel";
			this.networkTraceWarningLabel.Size = new System.Drawing.Size(236, 12);
			this.networkTraceWarningLabel.TabIndex = 23;
			this.networkTraceWarningLabel.Text = "(Warning: This overrides <system.diagnostics> element)";
			// 
			// writeNetworkTraceToFileCheckBox
			// 
			this.writeNetworkTraceToFileCheckBox.AutoSize = true;
			this.writeNetworkTraceToFileCheckBox.Enabled = false;
			this.writeNetworkTraceToFileCheckBox.Location = new System.Drawing.Point(11, 42);
			this.writeNetworkTraceToFileCheckBox.Name = "writeNetworkTraceToFileCheckBox";
			this.writeNetworkTraceToFileCheckBox.Size = new System.Drawing.Size(277, 17);
			this.writeNetworkTraceToFileCheckBox.TabIndex = 22;
			this.writeNetworkTraceToFileCheckBox.Text = "Write Network Trace Info to \"NBug.Network.log\" File";
			this.mainToolTips.SetToolTip(this.writeNetworkTraceToFileCheckBox, "Indicates whether to enable network tracing and write the network trace log to \"N" +
        "Bug.Network.log\" file.\r\nThis can only be enabled if appending settings to applic" +
        "ation \"app.config\" file.");
			this.writeNetworkTraceToFileCheckBox.UseVisualStyleBackColor = true;
			// 
			// writeLogToDiskCheckBox
			// 
			this.writeLogToDiskCheckBox.AutoSize = true;
			this.writeLogToDiskCheckBox.Location = new System.Drawing.Point(11, 19);
			this.writeLogToDiskCheckBox.Name = "writeLogToDiskCheckBox";
			this.writeLogToDiskCheckBox.Size = new System.Drawing.Size(163, 17);
			this.writeLogToDiskCheckBox.TabIndex = 21;
			this.writeLogToDiskCheckBox.Text = "Write \"NBug.log\" File to Disk";
			this.mainToolTips.SetToolTip(this.writeLogToDiskCheckBox, resources.GetString("writeLogToDiskCheckBox.ToolTip"));
			this.writeLogToDiskCheckBox.UseVisualStyleBackColor = true;
			// 
			// reportSubmitterGroupBox
			// 
			this.reportSubmitterGroupBox.Controls.Add(this.encryptConnectionStringsCheckBox);
			this.reportSubmitterGroupBox.Location = new System.Drawing.Point(297, 224);
			this.reportSubmitterGroupBox.Name = "reportSubmitterGroupBox";
			this.reportSubmitterGroupBox.Size = new System.Drawing.Size(170, 48);
			this.reportSubmitterGroupBox.TabIndex = 25;
			this.reportSubmitterGroupBox.TabStop = false;
			this.reportSubmitterGroupBox.Text = "Report Submitter";
			// 
			// encryptConnectionStringsCheckBox
			// 
			this.encryptConnectionStringsCheckBox.AutoSize = true;
			this.encryptConnectionStringsCheckBox.Location = new System.Drawing.Point(11, 19);
			this.encryptConnectionStringsCheckBox.Name = "encryptConnectionStringsCheckBox";
			this.encryptConnectionStringsCheckBox.Size = new System.Drawing.Size(154, 17);
			this.encryptConnectionStringsCheckBox.TabIndex = 21;
			this.encryptConnectionStringsCheckBox.Text = "Encrypt Connection Strings";
			this.mainToolTips.SetToolTip(this.encryptConnectionStringsCheckBox, resources.GetString("encryptConnectionStringsCheckBox.ToolTip"));
			this.encryptConnectionStringsCheckBox.UseVisualStyleBackColor = true;
			// 
			// reportQueueGroupBox
			// 
			this.reportQueueGroupBox.Controls.Add(this.storagePathLabel);
			this.reportQueueGroupBox.Controls.Add(this.storagePathComboBox);
			this.reportQueueGroupBox.Controls.Add(this.customPathLabel);
			this.reportQueueGroupBox.Controls.Add(this.customStoragePathTextBox);
			this.reportQueueGroupBox.Controls.Add(this.customPathTipLabel);
			this.reportQueueGroupBox.Location = new System.Drawing.Point(297, 13);
			this.reportQueueGroupBox.Name = "reportQueueGroupBox";
			this.reportQueueGroupBox.Size = new System.Drawing.Size(316, 112);
			this.reportQueueGroupBox.TabIndex = 24;
			this.reportQueueGroupBox.TabStop = false;
			this.reportQueueGroupBox.Text = "Report Queue";
			// 
			// storagePathLabel
			// 
			this.storagePathLabel.AutoSize = true;
			this.storagePathLabel.Location = new System.Drawing.Point(6, 25);
			this.storagePathLabel.Name = "storagePathLabel";
			this.storagePathLabel.Size = new System.Drawing.Size(131, 13);
			this.storagePathLabel.TabIndex = 12;
			this.storagePathLabel.Text = "Report Files Storage Path:";
			// 
			// storagePathComboBox
			// 
			this.storagePathComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.storagePathComboBox.FormattingEnabled = true;
			this.storagePathComboBox.Location = new System.Drawing.Point(143, 22);
			this.storagePathComboBox.Name = "storagePathComboBox";
			this.storagePathComboBox.Size = new System.Drawing.Size(153, 21);
			this.storagePathComboBox.TabIndex = 13;
			this.mainToolTips.SetToolTip(this.storagePathComboBox, "Gets or sets the bug report items storage path. After and unhandled exception occ" +
        "urs, the bug reports");
			this.storagePathComboBox.SelectedValueChanged += new System.EventHandler(this.StoragePathComboBox_SelectedValueChanged);
			// 
			// customPathLabel
			// 
			this.customPathLabel.AutoSize = true;
			this.customPathLabel.Location = new System.Drawing.Point(6, 63);
			this.customPathLabel.Name = "customPathLabel";
			this.customPathLabel.Size = new System.Drawing.Size(70, 13);
			this.customPathLabel.TabIndex = 18;
			this.customPathLabel.Text = "Custom Path:";
			// 
			// customStoragePathTextBox
			// 
			this.customStoragePathTextBox.Enabled = false;
			this.customStoragePathTextBox.Location = new System.Drawing.Point(82, 60);
			this.customStoragePathTextBox.Name = "customStoragePathTextBox";
			this.customStoragePathTextBox.Size = new System.Drawing.Size(214, 20);
			this.customStoragePathTextBox.TabIndex = 19;
			this.mainToolTips.SetToolTip(this.customStoragePathTextBox, "Custom storage path. This maybe a full path or a relative one to the application\'" +
        "s CWD.");
			// 
			// customPathTipLabel
			// 
			this.customPathTipLabel.AutoSize = true;
			this.customPathTipLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.customPathTipLabel.Location = new System.Drawing.Point(95, 83);
			this.customPathTipLabel.Name = "customPathTipLabel";
			this.customPathTipLabel.Size = new System.Drawing.Size(191, 12);
			this.customPathTipLabel.TabIndex = 20;
			this.customPathTipLabel.Text = "(uses System.IO.Path.GetFullPath() method)";
			// 
			// reportingGroupBox
			// 
			this.reportingGroupBox.Controls.Add(this.miniDumpTypeLabel);
			this.reportingGroupBox.Controls.Add(this.miniDumpTypeComboBox);
			this.reportingGroupBox.Controls.Add(this.sleepBeforeSendLabel);
			this.reportingGroupBox.Controls.Add(this.sleepBeforeSendNumericUpDown);
			this.reportingGroupBox.Controls.Add(this.maxQueuedReportsLabel);
			this.reportingGroupBox.Controls.Add(this.maxQueuedReportsNumericUpDown);
			this.reportingGroupBox.Controls.Add(this.sleepBeforeSendUnitLabel);
			this.reportingGroupBox.Controls.Add(this.stopReportingAfterLabel);
			this.reportingGroupBox.Controls.Add(this.stopReportingAfterUnitLabel);
			this.reportingGroupBox.Controls.Add(this.stopReportingAfterNumericUpDown);
			this.reportingGroupBox.Location = new System.Drawing.Point(19, 108);
			this.reportingGroupBox.Name = "reportingGroupBox";
			this.reportingGroupBox.Size = new System.Drawing.Size(246, 164);
			this.reportingGroupBox.TabIndex = 23;
			this.reportingGroupBox.TabStop = false;
			this.reportingGroupBox.Text = "Reporting";
			// 
			// miniDumpTypeLabel
			// 
			this.miniDumpTypeLabel.AutoSize = true;
			this.miniDumpTypeLabel.Location = new System.Drawing.Point(6, 25);
			this.miniDumpTypeLabel.Name = "miniDumpTypeLabel";
			this.miniDumpTypeLabel.Size = new System.Drawing.Size(84, 13);
			this.miniDumpTypeLabel.TabIndex = 2;
			this.miniDumpTypeLabel.Text = "MiniDump Type:";
			// 
			// miniDumpTypeComboBox
			// 
			this.miniDumpTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.miniDumpTypeComboBox.FormattingEnabled = true;
			this.miniDumpTypeComboBox.Location = new System.Drawing.Point(123, 22);
			this.miniDumpTypeComboBox.Name = "miniDumpTypeComboBox";
			this.miniDumpTypeComboBox.Size = new System.Drawing.Size(109, 21);
			this.miniDumpTypeComboBox.TabIndex = 3;
			this.mainToolTips.SetToolTip(this.miniDumpTypeComboBox, "Defines memory dump type with a specific detail level.\r\nNone: No memory dump is g" +
        "enerated.\r\nTiny: Dump size ~200KB compressed.\r\nNormal: Dump size ~20MB compresse" +
        "d.\r\nFull: Dump size ~100MB compressed.");
			// 
			// sleepBeforeSendLabel
			// 
			this.sleepBeforeSendLabel.AutoSize = true;
			this.sleepBeforeSendLabel.Location = new System.Drawing.Point(6, 58);
			this.sleepBeforeSendLabel.Name = "sleepBeforeSendLabel";
			this.sleepBeforeSendLabel.Size = new System.Drawing.Size(99, 13);
			this.sleepBeforeSendLabel.TabIndex = 4;
			this.sleepBeforeSendLabel.Text = "Sleep Before Send:";
			// 
			// sleepBeforeSendNumericUpDown
			// 
			this.sleepBeforeSendNumericUpDown.Location = new System.Drawing.Point(123, 56);
			this.sleepBeforeSendNumericUpDown.Name = "sleepBeforeSendNumericUpDown";
			this.sleepBeforeSendNumericUpDown.Size = new System.Drawing.Size(61, 20);
			this.sleepBeforeSendNumericUpDown.TabIndex = 5;
			this.mainToolTips.SetToolTip(this.sleepBeforeSendNumericUpDown, resources.GetString("sleepBeforeSendNumericUpDown.ToolTip"));
			// 
			// maxQueuedReportsLabel
			// 
			this.maxQueuedReportsLabel.AutoSize = true;
			this.maxQueuedReportsLabel.Location = new System.Drawing.Point(6, 95);
			this.maxQueuedReportsLabel.Name = "maxQueuedReportsLabel";
			this.maxQueuedReportsLabel.Size = new System.Drawing.Size(111, 13);
			this.maxQueuedReportsLabel.TabIndex = 6;
			this.maxQueuedReportsLabel.Text = "Max Queued Reports:";
			// 
			// maxQueuedReportsNumericUpDown
			// 
			this.maxQueuedReportsNumericUpDown.Location = new System.Drawing.Point(123, 93);
			this.maxQueuedReportsNumericUpDown.Name = "maxQueuedReportsNumericUpDown";
			this.maxQueuedReportsNumericUpDown.Size = new System.Drawing.Size(61, 20);
			this.maxQueuedReportsNumericUpDown.TabIndex = 7;
			this.mainToolTips.SetToolTip(this.maxQueuedReportsNumericUpDown, "The number of bug reports that can be queued for submission.\r\nAfter an exception " +
        "occurs, a bug report is queued to be send at the next application restart.");
			// 
			// sleepBeforeSendUnitLabel
			// 
			this.sleepBeforeSendUnitLabel.AutoSize = true;
			this.sleepBeforeSendUnitLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.sleepBeforeSendUnitLabel.Location = new System.Drawing.Point(188, 59);
			this.sleepBeforeSendUnitLabel.Name = "sleepBeforeSendUnitLabel";
			this.sleepBeforeSendUnitLabel.Size = new System.Drawing.Size(46, 12);
			this.sleepBeforeSendUnitLabel.TabIndex = 8;
			this.sleepBeforeSendUnitLabel.Text = "(seconds)";
			// 
			// stopReportingAfterLabel
			// 
			this.stopReportingAfterLabel.AutoSize = true;
			this.stopReportingAfterLabel.Location = new System.Drawing.Point(6, 132);
			this.stopReportingAfterLabel.Name = "stopReportingAfterLabel";
			this.stopReportingAfterLabel.Size = new System.Drawing.Size(106, 13);
			this.stopReportingAfterLabel.TabIndex = 9;
			this.stopReportingAfterLabel.Text = "Stop Reporting After:";
			// 
			// stopReportingAfterUnitLabel
			// 
			this.stopReportingAfterUnitLabel.AutoSize = true;
			this.stopReportingAfterUnitLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.stopReportingAfterUnitLabel.Location = new System.Drawing.Point(188, 133);
			this.stopReportingAfterUnitLabel.Name = "stopReportingAfterUnitLabel";
			this.stopReportingAfterUnitLabel.Size = new System.Drawing.Size(31, 12);
			this.stopReportingAfterUnitLabel.TabIndex = 11;
			this.stopReportingAfterUnitLabel.Text = "(days)";
			// 
			// stopReportingAfterNumericUpDown
			// 
			this.stopReportingAfterNumericUpDown.Location = new System.Drawing.Point(123, 130);
			this.stopReportingAfterNumericUpDown.Name = "stopReportingAfterNumericUpDown";
			this.stopReportingAfterNumericUpDown.Size = new System.Drawing.Size(61, 20);
			this.stopReportingAfterNumericUpDown.TabIndex = 10;
			this.mainToolTips.SetToolTip(this.stopReportingAfterNumericUpDown, "The number of days that NBug will be collecting bug reports for the application.\r" +
        "\nThis prevents generating bug reports for obselete versions of applications afte" +
        "r given number of days.");
			// 
			// userInterfaceGroupBox
			// 
			this.userInterfaceGroupBox.Controls.Add(this.previewButton);
			this.userInterfaceGroupBox.Controls.Add(this.uiModeLabel);
			this.userInterfaceGroupBox.Controls.Add(this.uiModeComboBox);
			this.userInterfaceGroupBox.Controls.Add(this.uiProviderLabel);
			this.userInterfaceGroupBox.Controls.Add(this.uiProviderComboBox);
			this.userInterfaceGroupBox.Location = new System.Drawing.Point(19, 13);
			this.userInterfaceGroupBox.Name = "userInterfaceGroupBox";
			this.userInterfaceGroupBox.Size = new System.Drawing.Size(246, 89);
			this.userInterfaceGroupBox.TabIndex = 22;
			this.userInterfaceGroupBox.TabStop = false;
			this.userInterfaceGroupBox.Text = "User Interface";
			// 
			// previewButton
			// 
			this.previewButton.Enabled = false;
			this.previewButton.Location = new System.Drawing.Point(179, 54);
			this.previewButton.Name = "previewButton";
			this.previewButton.Size = new System.Drawing.Size(53, 21);
			this.previewButton.TabIndex = 4;
			this.previewButton.Text = "Preview";
			this.previewButton.UseVisualStyleBackColor = true;
			this.previewButton.Click += new System.EventHandler(this.PreviewButton_Click);
			// 
			// uiModeLabel
			// 
			this.uiModeLabel.AutoSize = true;
			this.uiModeLabel.Location = new System.Drawing.Point(6, 24);
			this.uiModeLabel.Name = "uiModeLabel";
			this.uiModeLabel.Size = new System.Drawing.Size(48, 13);
			this.uiModeLabel.TabIndex = 0;
			this.uiModeLabel.Text = "UIMode:";
			// 
			// uiModeComboBox
			// 
			this.uiModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.uiModeComboBox.FormattingEnabled = true;
			this.uiModeComboBox.Location = new System.Drawing.Point(81, 21);
			this.uiModeComboBox.Name = "uiModeComboBox";
			this.uiModeComboBox.Size = new System.Drawing.Size(151, 21);
			this.uiModeComboBox.TabIndex = 1;
			this.mainToolTips.SetToolTip(this.uiModeComboBox, resources.GetString("uiModeComboBox.ToolTip"));
			this.uiModeComboBox.SelectedIndexChanged += new System.EventHandler(this.UIModeComboBox_SelectedIndexChanged);
			// 
			// uiProviderLabel
			// 
			this.uiProviderLabel.AutoSize = true;
			this.uiProviderLabel.Location = new System.Drawing.Point(6, 57);
			this.uiProviderLabel.Name = "uiProviderLabel";
			this.uiProviderLabel.Size = new System.Drawing.Size(63, 13);
			this.uiProviderLabel.TabIndex = 2;
			this.uiProviderLabel.Text = "UI Provider:";
			// 
			// uiProviderComboBox
			// 
			this.uiProviderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.uiProviderComboBox.FormattingEnabled = true;
			this.uiProviderComboBox.Location = new System.Drawing.Point(81, 54);
			this.uiProviderComboBox.Name = "uiProviderComboBox";
			this.uiProviderComboBox.Size = new System.Drawing.Size(92, 21);
			this.uiProviderComboBox.TabIndex = 3;
			this.mainToolTips.SetToolTip(this.uiProviderComboBox, resources.GetString("uiProviderComboBox.ToolTip"));
			this.uiProviderComboBox.SelectedIndexChanged += new System.EventHandler(this.UIProviderComboBox_SelectedIndexChanged);
			// 
			// advancedTabPage
			// 
			this.advancedTabPage.Controls.Add(this.exceptionHandlingGroupBox);
			this.advancedTabPage.Controls.Add(this.warningLabel);
			this.advancedTabPage.Location = new System.Drawing.Point(4, 22);
			this.advancedTabPage.Name = "advancedTabPage";
			this.advancedTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.advancedTabPage.Size = new System.Drawing.Size(671, 300);
			this.advancedTabPage.TabIndex = 6;
			this.advancedTabPage.Text = "Advanced";
			this.advancedTabPage.UseVisualStyleBackColor = true;
			// 
			// exceptionHandlingGroupBox
			// 
			this.exceptionHandlingGroupBox.Controls.Add(this.exitApplicationImmediatelyWarningLabel);
			this.exceptionHandlingGroupBox.Controls.Add(this.handleProcessCorruptedStateExceptionsCheckBox);
			this.exceptionHandlingGroupBox.Controls.Add(this.exitApplicationImmediatelyCheckBox);
			this.exceptionHandlingGroupBox.Location = new System.Drawing.Point(19, 70);
			this.exceptionHandlingGroupBox.Name = "exceptionHandlingGroupBox";
			this.exceptionHandlingGroupBox.Size = new System.Drawing.Size(255, 90);
			this.exceptionHandlingGroupBox.TabIndex = 27;
			this.exceptionHandlingGroupBox.TabStop = false;
			this.exceptionHandlingGroupBox.Text = "Exception Handling";
			// 
			// exitApplicationImmediatelyWarningLabel
			// 
			this.exitApplicationImmediatelyWarningLabel.AutoSize = true;
			this.exitApplicationImmediatelyWarningLabel.Enabled = false;
			this.exitApplicationImmediatelyWarningLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.exitApplicationImmediatelyWarningLabel.Location = new System.Drawing.Point(29, 37);
			this.exitApplicationImmediatelyWarningLabel.Name = "exitApplicationImmediatelyWarningLabel";
			this.exitApplicationImmediatelyWarningLabel.Size = new System.Drawing.Size(171, 12);
			this.exitApplicationImmediatelyWarningLabel.TabIndex = 23;
			this.exitApplicationImmediatelyWarningLabel.Text = "(this is only valid when UIMode = None;)";
			// 
			// handleProcessCorruptedStateExceptionsCheckBox
			// 
			this.handleProcessCorruptedStateExceptionsCheckBox.AutoSize = true;
			this.handleProcessCorruptedStateExceptionsCheckBox.Location = new System.Drawing.Point(11, 59);
			this.handleProcessCorruptedStateExceptionsCheckBox.Name = "handleProcessCorruptedStateExceptionsCheckBox";
			this.handleProcessCorruptedStateExceptionsCheckBox.Size = new System.Drawing.Size(233, 17);
			this.handleProcessCorruptedStateExceptionsCheckBox.TabIndex = 22;
			this.handleProcessCorruptedStateExceptionsCheckBox.Text = "Handle Process Corrupted State Exceptions";
			this.mainToolTips.SetToolTip(this.handleProcessCorruptedStateExceptionsCheckBox, resources.GetString("handleProcessCorruptedStateExceptionsCheckBox.ToolTip"));
			this.handleProcessCorruptedStateExceptionsCheckBox.UseVisualStyleBackColor = true;
			// 
			// exitApplicationImmediatelyCheckBox
			// 
			this.exitApplicationImmediatelyCheckBox.AutoSize = true;
			this.exitApplicationImmediatelyCheckBox.Enabled = false;
			this.exitApplicationImmediatelyCheckBox.Location = new System.Drawing.Point(11, 19);
			this.exitApplicationImmediatelyCheckBox.Name = "exitApplicationImmediatelyCheckBox";
			this.exitApplicationImmediatelyCheckBox.Size = new System.Drawing.Size(156, 17);
			this.exitApplicationImmediatelyCheckBox.TabIndex = 21;
			this.exitApplicationImmediatelyCheckBox.Text = "Exit Application Immediately";
			this.mainToolTips.SetToolTip(this.exitApplicationImmediatelyCheckBox, resources.GetString("exitApplicationImmediatelyCheckBox.ToolTip"));
			this.exitApplicationImmediatelyCheckBox.UseVisualStyleBackColor = true;
			// 
			// warningLabel
			// 
			this.warningLabel.Location = new System.Drawing.Point(19, 17);
			this.warningLabel.Name = "warningLabel";
			this.warningLabel.Size = new System.Drawing.Size(630, 41);
			this.warningLabel.TabIndex = 0;
			this.warningLabel.Text = resources.GetString("warningLabel.Text");
			// 
			// submit1TabPage
			// 
			this.submit1TabPage.Controls.Add(this.panelLoader1);
			this.submit1TabPage.Location = new System.Drawing.Point(4, 22);
			this.submit1TabPage.Name = "submit1TabPage";
			this.submit1TabPage.Padding = new System.Windows.Forms.Padding(3);
			this.submit1TabPage.Size = new System.Drawing.Size(671, 300);
			this.submit1TabPage.TabIndex = 1;
			this.submit1TabPage.Text = "Submit #1";
			this.submit1TabPage.UseVisualStyleBackColor = true;
			// 
			// panelLoader1
			// 
			this.panelLoader1.AutoScroll = true;
			this.panelLoader1.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
			this.panelLoader1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelLoader1.Location = new System.Drawing.Point(3, 3);
			this.panelLoader1.Name = "panelLoader1";
			this.panelLoader1.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.panelLoader1.Size = new System.Drawing.Size(665, 294);
			this.panelLoader1.TabIndex = 0;
			// 
			// submit2TabPage
			// 
			this.submit2TabPage.Controls.Add(this.panelLoader2);
			this.submit2TabPage.Location = new System.Drawing.Point(4, 22);
			this.submit2TabPage.Name = "submit2TabPage";
			this.submit2TabPage.Padding = new System.Windows.Forms.Padding(3);
			this.submit2TabPage.Size = new System.Drawing.Size(671, 300);
			this.submit2TabPage.TabIndex = 2;
			this.submit2TabPage.Text = "Submit #2";
			this.submit2TabPage.UseVisualStyleBackColor = true;
			// 
			// panelLoader2
			// 
			this.panelLoader2.AutoScroll = true;
			this.panelLoader2.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
			this.panelLoader2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelLoader2.Location = new System.Drawing.Point(3, 3);
			this.panelLoader2.Name = "panelLoader2";
			this.panelLoader2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.panelLoader2.Size = new System.Drawing.Size(665, 294);
			this.panelLoader2.TabIndex = 1;
			// 
			// submit3TabPage
			// 
			this.submit3TabPage.Controls.Add(this.panelLoader3);
			this.submit3TabPage.Location = new System.Drawing.Point(4, 22);
			this.submit3TabPage.Name = "submit3TabPage";
			this.submit3TabPage.Padding = new System.Windows.Forms.Padding(3);
			this.submit3TabPage.Size = new System.Drawing.Size(671, 300);
			this.submit3TabPage.TabIndex = 3;
			this.submit3TabPage.Text = "Submit #3";
			this.submit3TabPage.UseVisualStyleBackColor = true;
			// 
			// panelLoader3
			// 
			this.panelLoader3.AutoScroll = true;
			this.panelLoader3.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
			this.panelLoader3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelLoader3.Location = new System.Drawing.Point(3, 3);
			this.panelLoader3.Name = "panelLoader3";
			this.panelLoader3.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.panelLoader3.Size = new System.Drawing.Size(665, 294);
			this.panelLoader3.TabIndex = 1;
			// 
			// submit4TabPage
			// 
			this.submit4TabPage.Controls.Add(this.panelLoader4);
			this.submit4TabPage.Location = new System.Drawing.Point(4, 22);
			this.submit4TabPage.Name = "submit4TabPage";
			this.submit4TabPage.Padding = new System.Windows.Forms.Padding(3);
			this.submit4TabPage.Size = new System.Drawing.Size(671, 300);
			this.submit4TabPage.TabIndex = 4;
			this.submit4TabPage.Text = "Submit #4";
			this.submit4TabPage.UseVisualStyleBackColor = true;
			// 
			// panelLoader4
			// 
			this.panelLoader4.AutoScroll = true;
			this.panelLoader4.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
			this.panelLoader4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelLoader4.Location = new System.Drawing.Point(3, 3);
			this.panelLoader4.Name = "panelLoader4";
			this.panelLoader4.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.panelLoader4.Size = new System.Drawing.Size(665, 294);
			this.panelLoader4.TabIndex = 1;
			// 
			// submit5TabPage
			// 
			this.submit5TabPage.Controls.Add(this.panelLoader5);
			this.submit5TabPage.Location = new System.Drawing.Point(4, 22);
			this.submit5TabPage.Name = "submit5TabPage";
			this.submit5TabPage.Padding = new System.Windows.Forms.Padding(3);
			this.submit5TabPage.Size = new System.Drawing.Size(671, 300);
			this.submit5TabPage.TabIndex = 5;
			this.submit5TabPage.Text = "Submit #5";
			this.submit5TabPage.UseVisualStyleBackColor = true;
			// 
			// panelLoader5
			// 
			this.panelLoader5.AutoScroll = true;
			this.panelLoader5.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
			this.panelLoader5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelLoader5.Location = new System.Drawing.Point(3, 3);
			this.panelLoader5.Name = "panelLoader5";
			this.panelLoader5.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.panelLoader5.Size = new System.Drawing.Size(665, 294);
			this.panelLoader5.TabIndex = 1;
			// 
			// fileToolStripMenuItem1
			// 
			this.fileToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.toolStripSeparator,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.importToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem1.Name = "fileToolStripMenuItem1";
			this.fileToolStripMenuItem1.Size = new System.Drawing.Size(35, 20);
			this.fileToolStripMenuItem1.Text = "&File";
			// 
			// newToolStripMenuItem
			// 
			this.newToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.externalToolStripMenuItem,
            this.embeddedToolStripMenuItem});
			this.newToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripMenuItem.Image")));
			this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.newToolStripMenuItem.Name = "newToolStripMenuItem";
			this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.newToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
			this.newToolStripMenuItem.Text = "&New";
			// 
			// externalToolStripMenuItem
			// 
			this.externalToolStripMenuItem.Name = "externalToolStripMenuItem";
			this.externalToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
			this.externalToolStripMenuItem.Text = "External (NBug.config)";
			this.externalToolStripMenuItem.Click += new System.EventHandler(this.ExternalToolStripMenuItem_Click);
			// 
			// embeddedToolStripMenuItem
			// 
			this.embeddedToolStripMenuItem.Name = "embeddedToolStripMenuItem";
			this.embeddedToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
			this.embeddedToolStripMenuItem.Text = "Embedded (app.config)";
			this.embeddedToolStripMenuItem.Click += new System.EventHandler(this.EmbeddedToolStripMenuItem_Click);
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
			this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
			this.openToolStripMenuItem.Text = "&Open";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenButton_Click);
			// 
			// toolStripSeparator
			// 
			this.toolStripSeparator.Name = "toolStripSeparator";
			this.toolStripSeparator.Size = new System.Drawing.Size(148, 6);
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem.Image")));
			this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
			this.saveToolStripMenuItem.Text = "&Save";
			this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// saveAsToolStripMenuItem
			// 
			this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
			this.saveAsToolStripMenuItem.Text = "Save &As";
			this.saveAsToolStripMenuItem.Visible = false;
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(148, 6);
			this.toolStripSeparator1.Visible = false;
			// 
			// importToolStripMenuItem
			// 
			this.importToolStripMenuItem.Name = "importToolStripMenuItem";
			this.importToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
			this.importToolStripMenuItem.Text = "&Import";
			this.importToolStripMenuItem.Visible = false;
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(148, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// toolsToolStripMenuItem
			// 
			this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testAppToolStripMenuItem});
			this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
			this.toolsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.toolsToolStripMenuItem.Text = "&Tools";
			// 
			// testAppToolStripMenuItem
			// 
			this.testAppToolStripMenuItem.Image = global::NBug.Configurator.Properties.Resources.run;
			this.testAppToolStripMenuItem.Name = "testAppToolStripMenuItem";
			this.testAppToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
			this.testAppToolStripMenuItem.Text = "Save && Run &Test App";
			this.testAppToolStripMenuItem.Click += new System.EventHandler(this.RunTestAppButton_Click);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectHomeToolStripMenuItem,
            this.onlineDocumentationToolStripMenuItem,
            this.discussionForumToolStripMenuItem,
            this.bugTrackerToolStripMenuItem,
            this.toolStripSeparator5,
            this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
			this.helpToolStripMenuItem.Text = "&Help";
			// 
			// projectHomeToolStripMenuItem
			// 
			this.projectHomeToolStripMenuItem.Name = "projectHomeToolStripMenuItem";
			this.projectHomeToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.projectHomeToolStripMenuItem.Tag = "http://www.nbusy.com/projects/nbug";
			this.projectHomeToolStripMenuItem.Text = "Project &Home";
			this.projectHomeToolStripMenuItem.Click += new System.EventHandler(this.ProjectHomeToolStripMenuItem_Click);
			// 
			// onlineDocumentationToolStripMenuItem
			// 
			this.onlineDocumentationToolStripMenuItem.Image = global::NBug.Configurator.Properties.Resources.help;
			this.onlineDocumentationToolStripMenuItem.Name = "onlineDocumentationToolStripMenuItem";
			this.onlineDocumentationToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.onlineDocumentationToolStripMenuItem.Tag = "http://www.nbusy.com/projects/nbug/documentation";
			this.onlineDocumentationToolStripMenuItem.Text = "Online &Documentation";
			this.onlineDocumentationToolStripMenuItem.Click += new System.EventHandler(this.OnlineDocumentationToolStripMenuItem_Click);
			// 
			// discussionForumToolStripMenuItem
			// 
			this.discussionForumToolStripMenuItem.Name = "discussionForumToolStripMenuItem";
			this.discussionForumToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.discussionForumToolStripMenuItem.Tag = "http://www.nbusy.com/forum/f11/";
			this.discussionForumToolStripMenuItem.Text = "Discussion &Forum";
			this.discussionForumToolStripMenuItem.Click += new System.EventHandler(this.DiscussionForumToolStripMenuItem_Click);
			// 
			// bugTrackerToolStripMenuItem
			// 
			this.bugTrackerToolStripMenuItem.Name = "bugTrackerToolStripMenuItem";
			this.bugTrackerToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.bugTrackerToolStripMenuItem.Tag = "http://www.nbusy.com/tracker/projects/nbug";
			this.bugTrackerToolStripMenuItem.Text = "&Bug Tracker";
			this.bugTrackerToolStripMenuItem.Click += new System.EventHandler(this.BugTrackerToolStripMenuItem_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(187, 6);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Image = global::NBug.Configurator.Properties.Resources.Icon_16;
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.aboutToolStripMenuItem.Text = "&About NBug";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
			// 
			// mainMenuStrip
			// 
			this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem1,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
			this.mainMenuStrip.Name = "mainMenuStrip";
			this.mainMenuStrip.Size = new System.Drawing.Size(703, 24);
			this.mainMenuStrip.TabIndex = 0;
			// 
			// saveButton
			// 
			this.saveButton.Enabled = false;
			this.saveButton.Image = global::NBug.Configurator.Properties.Resources.save;
			this.saveButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.saveButton.Location = new System.Drawing.Point(517, 439);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(89, 23);
			this.saveButton.TabIndex = 3;
			this.saveButton.Text = "&Save";
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// closeButton
			// 
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(616, 439);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(75, 23);
			this.closeButton.TabIndex = 5;
			this.closeButton.Text = "&Close";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// mainToolTips
			// 
			this.mainToolTips.AutoPopDelay = 120000;
			this.mainToolTips.InitialDelay = 500;
			this.mainToolTips.ReshowDelay = 100;
			// 
			// runTestAppButton
			// 
			this.runTestAppButton.Enabled = false;
			this.runTestAppButton.Image = global::NBug.Configurator.Properties.Resources.run;
			this.runTestAppButton.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.runTestAppButton.Location = new System.Drawing.Point(360, 439);
			this.runTestAppButton.Name = "runTestAppButton";
			this.runTestAppButton.Size = new System.Drawing.Size(136, 23);
			this.runTestAppButton.TabIndex = 6;
			this.runTestAppButton.Text = "Save && Run &Test App";
			this.runTestAppButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.runTestAppButton.UseVisualStyleBackColor = true;
			this.runTestAppButton.Click += new System.EventHandler(this.RunTestAppButton_Click);
			// 
			// settingsFileGroupBox
			// 
			this.settingsFileGroupBox.Controls.Add(this.fileTextBox);
			this.settingsFileGroupBox.Controls.Add(this.pathLabel);
			this.settingsFileGroupBox.Controls.Add(this.createButton);
			this.settingsFileGroupBox.Controls.Add(this.openButton);
			this.settingsFileGroupBox.Location = new System.Drawing.Point(12, 33);
			this.settingsFileGroupBox.Name = "settingsFileGroupBox";
			this.settingsFileGroupBox.Size = new System.Drawing.Size(679, 57);
			this.settingsFileGroupBox.TabIndex = 22;
			this.settingsFileGroupBox.TabStop = false;
			this.settingsFileGroupBox.Text = "Settings File";
			// 
			// fileTextBox
			// 
			this.fileTextBox.Location = new System.Drawing.Point(39, 21);
			this.fileTextBox.Name = "fileTextBox";
			this.fileTextBox.ReadOnly = true;
			this.fileTextBox.Size = new System.Drawing.Size(494, 20);
			this.fileTextBox.TabIndex = 3;
			// 
			// pathLabel
			// 
			this.pathLabel.AutoSize = true;
			this.pathLabel.Location = new System.Drawing.Point(6, 24);
			this.pathLabel.Name = "pathLabel";
			this.pathLabel.Size = new System.Drawing.Size(32, 13);
			this.pathLabel.TabIndex = 2;
			this.pathLabel.Text = "Path:";
			// 
			// createButton
			// 
			this.createButton.Location = new System.Drawing.Point(608, 20);
			this.createButton.Name = "createButton";
			this.createButton.Size = new System.Drawing.Size(65, 23);
			this.createButton.TabIndex = 1;
			this.createButton.Text = "Create";
			this.createButton.UseVisualStyleBackColor = true;
			this.createButton.Click += new System.EventHandler(this.CreateButton_Click);
			// 
			// openButton
			// 
			this.openButton.Location = new System.Drawing.Point(542, 20);
			this.openButton.Name = "openButton";
			this.openButton.Size = new System.Drawing.Size(60, 23);
			this.openButton.TabIndex = 0;
			this.openButton.Text = "Open";
			this.openButton.UseVisualStyleBackColor = true;
			this.openButton.Click += new System.EventHandler(this.OpenButton_Click);
			// 
			// createFileDialog
			// 
			this.createFileDialog.FileName = "NBug";
			this.createFileDialog.Filter = "Configuration File (.config)|*.config";
			this.createFileDialog.Title = "Create New Configuration File";
			// 
			// openFileDialog
			// 
			this.openFileDialog.FileName = "NBug";
			this.openFileDialog.Filter = "Configuration File (.config)|*.config";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(703, 494);
			this.Controls.Add(this.settingsFileGroupBox);
			this.Controls.Add(this.runTestAppButton);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.saveButton);
			this.Controls.Add(this.mainTabs);
			this.Controls.Add(this.mainStatusStrip);
			this.Controls.Add(this.mainMenuStrip);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "NBug - Configurator";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.mainStatusStrip.ResumeLayout(false);
			this.mainStatusStrip.PerformLayout();
			this.mainTabs.ResumeLayout(false);
			this.generalTabPage.ResumeLayout(false);
			this.nbugConfigurationGroupBox.ResumeLayout(false);
			this.nbugConfigurationGroupBox.PerformLayout();
			this.internalLoggerGroupBox.ResumeLayout(false);
			this.internalLoggerGroupBox.PerformLayout();
			this.reportSubmitterGroupBox.ResumeLayout(false);
			this.reportSubmitterGroupBox.PerformLayout();
			this.reportQueueGroupBox.ResumeLayout(false);
			this.reportQueueGroupBox.PerformLayout();
			this.reportingGroupBox.ResumeLayout(false);
			this.reportingGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.sleepBeforeSendNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.maxQueuedReportsNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.stopReportingAfterNumericUpDown)).EndInit();
			this.userInterfaceGroupBox.ResumeLayout(false);
			this.userInterfaceGroupBox.PerformLayout();
			this.advancedTabPage.ResumeLayout(false);
			this.exceptionHandlingGroupBox.ResumeLayout(false);
			this.exceptionHandlingGroupBox.PerformLayout();
			this.submit1TabPage.ResumeLayout(false);
			this.submit2TabPage.ResumeLayout(false);
			this.submit3TabPage.ResumeLayout(false);
			this.submit4TabPage.ResumeLayout(false);
			this.submit5TabPage.ResumeLayout(false);
			this.mainMenuStrip.ResumeLayout(false);
			this.mainMenuStrip.PerformLayout();
			this.settingsFileGroupBox.ResumeLayout(false);
			this.settingsFileGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.StatusStrip mainStatusStrip;
		private System.Windows.Forms.TabControl mainTabs;
		private System.Windows.Forms.TabPage generalTabPage;
		private System.Windows.Forms.TabPage submit1TabPage;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem onlineDocumentationToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.MenuStrip mainMenuStrip;
		private System.Windows.Forms.Button saveButton;
		private System.Windows.Forms.ComboBox uiModeComboBox;
		private System.Windows.Forms.Label uiModeLabel;
		private System.Windows.Forms.ComboBox uiProviderComboBox;
		private System.Windows.Forms.Label uiProviderLabel;
		private System.Windows.Forms.NumericUpDown sleepBeforeSendNumericUpDown;
		private System.Windows.Forms.Label sleepBeforeSendLabel;
		private System.Windows.Forms.NumericUpDown maxQueuedReportsNumericUpDown;
		private System.Windows.Forms.Label maxQueuedReportsLabel;
		private System.Windows.Forms.Label sleepBeforeSendUnitLabel;
		private System.Windows.Forms.NumericUpDown stopReportingAfterNumericUpDown;
		private System.Windows.Forms.Label stopReportingAfterLabel;
		private System.Windows.Forms.Label stopReportingAfterUnitLabel;
		private System.Windows.Forms.ComboBox storagePathComboBox;
		private System.Windows.Forms.Label storagePathLabel;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.TextBox customStoragePathTextBox;
		private System.Windows.Forms.Label customPathLabel;
		private System.Windows.Forms.Label customPathTipLabel;
		private System.Windows.Forms.ToolTip mainToolTips;
		private System.Windows.Forms.HelpProvider mainHelpProvider;
		private System.Windows.Forms.Button runTestAppButton;
		private System.Windows.Forms.GroupBox settingsFileGroupBox;
		private System.Windows.Forms.TextBox fileTextBox;
		private System.Windows.Forms.Label pathLabel;
		private System.Windows.Forms.Button createButton;
		private System.Windows.Forms.Button openButton;
		private System.Windows.Forms.ToolStripMenuItem externalToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem discussionForumToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem projectHomeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem bugTrackerToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem testAppToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem embeddedToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
		private System.Windows.Forms.SaveFileDialog createFileDialog;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private SubmitPanels.PanelLoader panelLoader1;
		private System.Windows.Forms.TabPage submit2TabPage;
		private System.Windows.Forms.TabPage submit3TabPage;
		private System.Windows.Forms.TabPage submit4TabPage;
		private System.Windows.Forms.TabPage submit5TabPage;
		private SubmitPanels.PanelLoader panelLoader2;
		private SubmitPanels.PanelLoader panelLoader3;
		private SubmitPanels.PanelLoader panelLoader4;
		private SubmitPanels.PanelLoader panelLoader5;
		private System.Windows.Forms.ToolStripStatusLabel status;
		private System.Windows.Forms.CheckBox encryptConnectionStringsCheckBox;
		private System.Windows.Forms.GroupBox reportingGroupBox;
		private System.Windows.Forms.Label miniDumpTypeLabel;
		private System.Windows.Forms.ComboBox miniDumpTypeComboBox;
		private System.Windows.Forms.GroupBox userInterfaceGroupBox;
		private System.Windows.Forms.GroupBox internalLoggerGroupBox;
		private System.Windows.Forms.CheckBox writeLogToDiskCheckBox;
		private System.Windows.Forms.GroupBox reportSubmitterGroupBox;
		private System.Windows.Forms.GroupBox reportQueueGroupBox;
		private System.Windows.Forms.CheckBox writeNetworkTraceToFileCheckBox;
		private System.Windows.Forms.Label networkTraceWarningLabel;
		private System.Windows.Forms.Button previewButton;
		private System.Windows.Forms.TabPage advancedTabPage;
		private System.Windows.Forms.Label warningLabel;
		private System.Windows.Forms.GroupBox exceptionHandlingGroupBox;
		private System.Windows.Forms.Label exitApplicationImmediatelyWarningLabel;
		private System.Windows.Forms.CheckBox handleProcessCorruptedStateExceptionsCheckBox;
		private System.Windows.Forms.CheckBox exitApplicationImmediatelyCheckBox;
		private System.Windows.Forms.GroupBox nbugConfigurationGroupBox;
		private System.Windows.Forms.CheckBox releaseModeCheckBox;

	}
}

