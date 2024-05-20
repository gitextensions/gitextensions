namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    partial class DetailedSettingsPage
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
            PushWindowGB = new GroupBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            chkRemotesFromServer = new CheckBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            gbRevisionGraph = new GroupBox();
            tlpnlRevisionGraph = new TableLayoutPanel();
            chkMergeGraphLanesHavingCommonParent = new CheckBox();
            chkRenderGraphWithDiagonals = new CheckBox();
            chkStraightenGraphDiagonals = new CheckBox();
            groupBoxEmailSettings = new GroupBox();
            tlpnlEmailSettings = new TableLayoutPanel();
            SmtpServer = new TextBox();
            lblSmtpServerPort = new Label();
            chkUseSSL = new CheckBox();
            SmtpServerPort = new TextBox();
            lblSmtpServerName = new Label();
            mergeWindowGroup = new GroupBox();
            tableLayoutPanel4 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            addLogMessages = new CheckBox();
            nbMessages = new TextBox();
            PushWindowGB.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            gbRevisionGraph.SuspendLayout();
            tlpnlRevisionGraph.SuspendLayout();
            groupBoxEmailSettings.SuspendLayout();
            tlpnlEmailSettings.SuspendLayout();
            mergeWindowGroup.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // PushWindowGB
            // 
            PushWindowGB.AutoSize = true;
            PushWindowGB.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            PushWindowGB.Controls.Add(tableLayoutPanel1);
            PushWindowGB.Dock = DockStyle.Fill;
            PushWindowGB.Location = new Point(3, 116);
            PushWindowGB.Name = "PushWindowGB";
            PushWindowGB.Padding = new Padding(8);
            PushWindowGB.Size = new Size(1049, 57);
            PushWindowGB.TabIndex = 1;
            PushWindowGB.TabStop = false;
            PushWindowGB.Text = "&Push window";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(chkRemotesFromServer, 0, 4);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(8, 24);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 6;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(1033, 25);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // chkRemotesFromServer
            // 
            chkRemotesFromServer.AutoSize = true;
            chkRemotesFromServer.Dock = DockStyle.Top;
            chkRemotesFromServer.Location = new Point(3, 3);
            chkRemotesFromServer.Name = "chkRemotesFromServer";
            chkRemotesFromServer.Size = new Size(1027, 19);
            chkRemotesFromServer.TabIndex = 0;
            chkRemotesFromServer.Text = "Get remote branches directly from the remote";
            chkRemotesFromServer.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.Controls.Add(gbRevisionGraph, 0, 0);
            tableLayoutPanel2.Controls.Add(groupBoxEmailSettings, 0, 3);
            tableLayoutPanel2.Controls.Add(mergeWindowGroup, 0, 2);
            tableLayoutPanel2.Controls.Add(PushWindowGB, 0, 1);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(8, 8);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 5;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(1055, 670);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // gbRevisionGraph
            // 
            gbRevisionGraph.AutoSize = true;
            gbRevisionGraph.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbRevisionGraph.Controls.Add(tlpnlRevisionGraph);
            gbRevisionGraph.Dock = DockStyle.Fill;
            gbRevisionGraph.Location = new Point(3, 3);
            gbRevisionGraph.Name = "gbRevisionGraph";
            gbRevisionGraph.Padding = new Padding(8);
            gbRevisionGraph.Size = new Size(1049, 107);
            gbRevisionGraph.TabIndex = 0;
            gbRevisionGraph.TabStop = false;
            gbRevisionGraph.Text = "&Revision graph";
            // 
            // tlpnlRevisionGraph
            // 
            tlpnlRevisionGraph.AutoSize = true;
            tlpnlRevisionGraph.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlRevisionGraph.ColumnCount = 3;
            tlpnlRevisionGraph.ColumnStyles.Add(new ColumnStyle());
            tlpnlRevisionGraph.ColumnStyles.Add(new ColumnStyle());
            tlpnlRevisionGraph.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpnlRevisionGraph.Controls.Add(chkMergeGraphLanesHavingCommonParent, 0, 0);
            tlpnlRevisionGraph.Controls.Add(chkRenderGraphWithDiagonals, 0, 1);
            tlpnlRevisionGraph.Controls.Add(chkStraightenGraphDiagonals, 0, 2);
            tlpnlRevisionGraph.Dock = DockStyle.Fill;
            tlpnlRevisionGraph.Location = new Point(8, 24);
            tlpnlRevisionGraph.Name = "tlpnlRevisionGraph";
            tlpnlRevisionGraph.RowCount = 3;
            tlpnlRevisionGraph.RowStyles.Add(new RowStyle());
            tlpnlRevisionGraph.RowStyles.Add(new RowStyle());
            tlpnlRevisionGraph.RowStyles.Add(new RowStyle());
            tlpnlRevisionGraph.Size = new Size(1033, 75);
            tlpnlRevisionGraph.TabIndex = 0;
            // 
            // chkMergeGraphLanesHavingCommonParent
            // 
            chkMergeGraphLanesHavingCommonParent.AutoSize = true;
            tlpnlRevisionGraph.SetColumnSpan(chkMergeGraphLanesHavingCommonParent, 2);
            chkMergeGraphLanesHavingCommonParent.Dock = DockStyle.Fill;
            chkMergeGraphLanesHavingCommonParent.Location = new Point(3, 3);
            chkMergeGraphLanesHavingCommonParent.Name = "chkMergeGraphLanesHavingCommonParent";
            chkMergeGraphLanesHavingCommonParent.Size = new Size(252, 19);
            chkMergeGraphLanesHavingCommonParent.TabIndex = 0;
            chkMergeGraphLanesHavingCommonParent.Text = "Merge graph lanes having common parent";
            chkMergeGraphLanesHavingCommonParent.UseVisualStyleBackColor = true;
            // 
            // chkRenderGraphWithDiagonals
            // 
            chkRenderGraphWithDiagonals.AutoSize = true;
            tlpnlRevisionGraph.SetColumnSpan(chkRenderGraphWithDiagonals, 2);
            chkRenderGraphWithDiagonals.Dock = DockStyle.Fill;
            chkRenderGraphWithDiagonals.Location = new Point(3, 28);
            chkRenderGraphWithDiagonals.Name = "chkRenderGraphWithDiagonals";
            chkRenderGraphWithDiagonals.Size = new Size(252, 19);
            chkRenderGraphWithDiagonals.TabIndex = 1;
            chkRenderGraphWithDiagonals.Text = "Render graph with diagonals";
            chkRenderGraphWithDiagonals.UseVisualStyleBackColor = true;
            // 
            // chkStraightenGraphDiagonals
            // 
            chkStraightenGraphDiagonals.AutoSize = true;
            tlpnlRevisionGraph.SetColumnSpan(chkStraightenGraphDiagonals, 2);
            chkStraightenGraphDiagonals.Dock = DockStyle.Fill;
            chkStraightenGraphDiagonals.Location = new Point(3, 53);
            chkStraightenGraphDiagonals.Name = "chkStraightenGraphDiagonals";
            chkStraightenGraphDiagonals.Size = new Size(252, 19);
            chkStraightenGraphDiagonals.TabIndex = 2;
            chkStraightenGraphDiagonals.Text = "Straighten graph diagonals";
            chkStraightenGraphDiagonals.UseVisualStyleBackColor = true;
            // 
            // groupBoxEmailSettings
            // 
            groupBoxEmailSettings.AutoSize = true;
            groupBoxEmailSettings.Controls.Add(tlpnlEmailSettings);
            groupBoxEmailSettings.Dock = DockStyle.Fill;
            groupBoxEmailSettings.Location = new Point(3, 252);
            groupBoxEmailSettings.Name = "groupBoxEmailSettings";
            groupBoxEmailSettings.Padding = new Padding(8);
            groupBoxEmailSettings.Size = new Size(1049, 115);
            groupBoxEmailSettings.TabIndex = 3;
            groupBoxEmailSettings.TabStop = false;
            groupBoxEmailSettings.Text = "&Email settings for sending patches";
            // 
            // tlpnlEmailSettings
            // 
            tlpnlEmailSettings.AutoSize = true;
            tlpnlEmailSettings.ColumnCount = 3;
            tlpnlEmailSettings.ColumnStyles.Add(new ColumnStyle());
            tlpnlEmailSettings.ColumnStyles.Add(new ColumnStyle());
            tlpnlEmailSettings.ColumnStyles.Add(new ColumnStyle());
            tlpnlEmailSettings.Controls.Add(SmtpServer, 2, 0);
            tlpnlEmailSettings.Controls.Add(lblSmtpServerPort, 0, 1);
            tlpnlEmailSettings.Controls.Add(chkUseSSL, 0, 2);
            tlpnlEmailSettings.Controls.Add(SmtpServerPort, 2, 1);
            tlpnlEmailSettings.Controls.Add(lblSmtpServerName, 0, 0);
            tlpnlEmailSettings.Dock = DockStyle.Top;
            tlpnlEmailSettings.Location = new Point(8, 24);
            tlpnlEmailSettings.Name = "tlpnlEmailSettings";
            tlpnlEmailSettings.RowCount = 3;
            tlpnlEmailSettings.RowStyles.Add(new RowStyle());
            tlpnlEmailSettings.RowStyles.Add(new RowStyle());
            tlpnlEmailSettings.RowStyles.Add(new RowStyle());
            tlpnlEmailSettings.Size = new Size(1033, 83);
            tlpnlEmailSettings.TabIndex = 0;
            // 
            // SmtpServer
            // 
            SmtpServer.Location = new Point(123, 3);
            SmtpServer.Name = "SmtpServer";
            SmtpServer.Size = new Size(179, 23);
            SmtpServer.TabIndex = 0;
            // 
            // lblSmtpServerPort
            // 
            lblSmtpServerPort.AutoSize = true;
            lblSmtpServerPort.Dock = DockStyle.Fill;
            lblSmtpServerPort.Location = new Point(3, 29);
            lblSmtpServerPort.Name = "lblSmtpServerPort";
            lblSmtpServerPort.Size = new Size(114, 29);
            lblSmtpServerPort.TabIndex = 1;
            lblSmtpServerPort.Text = "Port";
            lblSmtpServerPort.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // chkUseSSL
            // 
            chkUseSSL.AutoSize = true;
            chkUseSSL.Dock = DockStyle.Fill;
            chkUseSSL.Location = new Point(3, 61);
            chkUseSSL.Name = "chkUseSSL";
            chkUseSSL.Size = new Size(114, 19);
            chkUseSSL.TabIndex = 3;
            chkUseSSL.Text = "Use SSL/TLS";
            chkUseSSL.UseVisualStyleBackColor = true;
            chkUseSSL.CheckedChanged += chkUseSSL_CheckedChanged;
            // 
            // SmtpServerPort
            // 
            SmtpServerPort.Location = new Point(123, 32);
            SmtpServerPort.Name = "SmtpServerPort";
            SmtpServerPort.Size = new Size(49, 23);
            SmtpServerPort.TabIndex = 2;
            SmtpServerPort.Text = "587";
            // 
            // lblSmtpServerName
            // 
            lblSmtpServerName.Anchor = AnchorStyles.Left;
            lblSmtpServerName.AutoSize = true;
            lblSmtpServerName.Location = new Point(3, 7);
            lblSmtpServerName.Name = "lblSmtpServerName";
            lblSmtpServerName.Padding = new Padding(0, 0, 10, 0);
            lblSmtpServerName.Size = new Size(114, 15);
            lblSmtpServerName.TabIndex = 0;
            lblSmtpServerName.Text = "SMTP server name";
            lblSmtpServerName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // mergeWindowGroup
            // 
            mergeWindowGroup.AutoSize = true;
            mergeWindowGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            mergeWindowGroup.Controls.Add(tableLayoutPanel4);
            mergeWindowGroup.Dock = DockStyle.Top;
            mergeWindowGroup.Location = new Point(3, 179);
            mergeWindowGroup.Name = "mergeWindowGroup";
            mergeWindowGroup.Padding = new Padding(8);
            mergeWindowGroup.Size = new Size(1049, 67);
            mergeWindowGroup.TabIndex = 2;
            mergeWindowGroup.TabStop = false;
            mergeWindowGroup.Text = "&Merge window";
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.AutoSize = true;
            tableLayoutPanel4.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel4.ColumnCount = 1;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel4.Controls.Add(flowLayoutPanel1, 0, 0);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(8, 24);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.RowStyles.Add(new RowStyle());
            tableLayoutPanel4.Size = new Size(1033, 35);
            tableLayoutPanel4.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(addLogMessages);
            flowLayoutPanel1.Controls.Add(nbMessages);
            flowLayoutPanel1.Location = new Point(3, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(185, 29);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // addLogMessages
            // 
            addLogMessages.Anchor = AnchorStyles.Left;
            addLogMessages.AutoSize = true;
            addLogMessages.Location = new Point(3, 5);
            addLogMessages.Name = "addLogMessages";
            addLogMessages.Size = new Size(122, 19);
            addLogMessages.TabIndex = 0;
            addLogMessages.Text = "Add log messages";
            addLogMessages.UseVisualStyleBackColor = true;
            // 
            // nbMessages
            // 
            nbMessages.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nbMessages.Location = new Point(131, 3);
            nbMessages.Name = "nbMessages";
            nbMessages.Size = new Size(51, 23);
            nbMessages.TabIndex = 1;
            nbMessages.TextAlign = HorizontalAlignment.Center;
            // 
            // DetailedSettingsPage
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(tableLayoutPanel2);
            Name = "DetailedSettingsPage";
            Padding = new Padding(8);
            Text = "Detailed";
            Size = new Size(1071, 686);
            PushWindowGB.ResumeLayout(false);
            PushWindowGB.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            gbRevisionGraph.ResumeLayout(false);
            gbRevisionGraph.PerformLayout();
            tlpnlRevisionGraph.ResumeLayout(false);
            tlpnlRevisionGraph.PerformLayout();
            groupBoxEmailSettings.ResumeLayout(false);
            groupBoxEmailSettings.PerformLayout();
            tlpnlEmailSettings.ResumeLayout(false);
            tlpnlEmailSettings.PerformLayout();
            mergeWindowGroup.ResumeLayout(false);
            mergeWindowGroup.PerformLayout();
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox PushWindowGB;
        private TableLayoutPanel tableLayoutPanel1;
        private CheckBox chkRemotesFromServer;
        private TableLayoutPanel tableLayoutPanel2;
        private GroupBox mergeWindowGroup;
        private TableLayoutPanel tableLayoutPanel4;
        private FlowLayoutPanel flowLayoutPanel1;
        private CheckBox addLogMessages;
        private TextBox nbMessages;
        private GroupBox groupBoxEmailSettings;
        private TableLayoutPanel tlpnlEmailSettings;
        private TextBox SmtpServer;
        private Label lblSmtpServerPort;
        private CheckBox chkUseSSL;
        private TextBox SmtpServerPort;
        private Label lblSmtpServerName;
        private GroupBox gbRevisionGraph;
        private TableLayoutPanel tlpnlRevisionGraph;
        private CheckBox chkMergeGraphLanesHavingCommonParent;
        private CheckBox chkRenderGraphWithDiagonals;
        private CheckBox chkStraightenGraphDiagonals;
    }
}
