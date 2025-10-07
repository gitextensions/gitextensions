using GitUI.UserControls.Settings;

namespace GitUI.CommandsDialogs.BrowseDialog;

partial class FormUpdates
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
        UpdateLabel = new Label();
        progressBar1 = new ProgressBar();
        linkChangeLog = new LinkLabel();
        btnUpdateNow = new Button();
        linkDirectDownload = new LinkLabel();
        tlpnlContent = new TableLayoutPanel();
        linkRequiredDotNetRuntime = new SettingsLinkLabel();
        MainPanel.SuspendLayout();
        tlpnlContent.SuspendLayout();
        SuspendLayout();
        // 
        // MainPanel
        // 
        MainPanel.Controls.Add(tlpnlContent);
        MainPanel.Size = new Size(462, 103);
        MainPanel.TabIndex = 0;
        // 
        // ControlsPanel
        // 
        ControlsPanel.Controls.Add(linkDirectDownload);
        ControlsPanel.Controls.Add(btnUpdateNow);
        // 
        // UpdateLabel
        // 
        UpdateLabel.AutoSize = true;
        UpdateLabel.Dock = DockStyle.Fill;
        UpdateLabel.Location = new Point(3, 0);
        UpdateLabel.Name = "UpdateLabel";
        UpdateLabel.Size = new Size(432, 15);
        UpdateLabel.TabIndex = 0;
        UpdateLabel.Text = "Searching for updates";
        // 
        // progressBar1
        // 
        progressBar1.Dock = DockStyle.Fill;
        progressBar1.Location = new Point(3, 18);
        progressBar1.MinimumSize = new Size(0, 20);
        progressBar1.Name = "progressBar1";
        progressBar1.Size = new Size(432, 20);
        progressBar1.Style = ProgressBarStyle.Continuous;
        progressBar1.TabIndex = 1;
        // 
        // linkChangeLog
        // 
        linkChangeLog.AutoSize = true;
        linkChangeLog.Dock = DockStyle.Bottom;
        linkChangeLog.Location = new Point(3, 71);
        linkChangeLog.Name = "linkChangeLog";
        linkChangeLog.Size = new Size(432, 8);
        linkChangeLog.TabIndex = 3;
        linkChangeLog.TabStop = true;
        linkChangeLog.Text = "Show release notes and change &log";
        linkChangeLog.Visible = false;
        linkChangeLog.LinkClicked += linkChangeLog_LinkClicked;
        // 
        // btnUpdateNow
        // 
        btnUpdateNow.AutoSize = true;
        btnUpdateNow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnUpdateNow.Location = new Point(248, 8);
        btnUpdateNow.MinimumSize = new Size(100, 0);
        btnUpdateNow.Name = "btnUpdateNow";
        btnUpdateNow.Size = new Size(100, 25);
        btnUpdateNow.TabIndex = 0;
        btnUpdateNow.Text = "&Update Now";
        btnUpdateNow.UseVisualStyleBackColor = true;
        btnUpdateNow.Visible = false;
        btnUpdateNow.Click += btnUpdateNow_Click;
        // 
        // linkDirectDownload
        // 
        linkDirectDownload.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        linkDirectDownload.AutoSize = true;
        linkDirectDownload.Location = new Point(354, 5);
        linkDirectDownload.Name = "linkDirectDownload";
        linkDirectDownload.Size = new Size(95, 31);
        linkDirectDownload.TabIndex = 1;
        linkDirectDownload.TabStop = true;
        linkDirectDownload.Text = "&Direct Download";
        linkDirectDownload.TextAlign = ContentAlignment.MiddleCenter;
        linkDirectDownload.Visible = false;
        linkDirectDownload.LinkClicked += linkDirectDownload_LinkClicked;
        // 
        // tlpnlContent
        // 
        tlpnlContent.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tlpnlContent.ColumnStyles.Add(new ColumnStyle());
        tlpnlContent.Controls.Add(UpdateLabel, 0, 0);
        tlpnlContent.Controls.Add(progressBar1, 0, 1);
        tlpnlContent.Controls.Add(linkRequiredDotNetRuntime, 0, 3);
        tlpnlContent.Controls.Add(linkChangeLog, 0, 5);
        tlpnlContent.Dock = DockStyle.Fill;
        tlpnlContent.Location = new Point(12, 12);
        tlpnlContent.Margin = new Padding(0);
        tlpnlContent.Name = "tlpnlContent";
        tlpnlContent.RowCount = 6;
        tlpnlContent.RowStyles.Add(new RowStyle());
        tlpnlContent.RowStyles.Add(new RowStyle());
        tlpnlContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 8F));
        tlpnlContent.RowStyles.Add(new RowStyle());
        tlpnlContent.RowStyles.Add(new RowStyle());
        tlpnlContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpnlContent.Size = new Size(438, 79);
        tlpnlContent.TabIndex = 3;
        // 
        // linkRequiredDotNetRuntime
        // 
        linkRequiredDotNetRuntime.AutoSize = true;
        linkRequiredDotNetRuntime.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        linkRequiredDotNetRuntime.Dock = DockStyle.Fill;
        linkRequiredDotNetRuntime.LinkArea = new LinkArea(0, 0);
        linkRequiredDotNetRuntime.Location = new Point(4, 52);
        linkRequiredDotNetRuntime.ManualSectionAnchorName = null;
        linkRequiredDotNetRuntime.Margin = new Padding(4, 3, 4, 3);
        linkRequiredDotNetRuntime.Name = "linkRequiredDotNetRuntime";
        linkRequiredDotNetRuntime.Size = new Size(430, 18);
        linkRequiredDotNetRuntime.TabIndex = 2;
        linkRequiredDotNetRuntime.Text = "Required: .NET {0} Desktop Runtime {1} or later {2}.x";
        linkRequiredDotNetRuntime.ToolTipIcon = UserControls.Settings.ToolTipIcon.Information;
        linkRequiredDotNetRuntime.ToolTipText = "Download latest .NET Desktop Runtime. See docs on how to install .NET runtime without administrative privileges.";
        linkRequiredDotNetRuntime.InfoClicked += linkRequiredDotNetRuntime_InfoClicked;
        linkRequiredDotNetRuntime.LinkClicked += linkRequiredDotNetRuntime_LinkClicked;
        // 
        // FormUpdates
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(462, 135);
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "FormUpdates";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Check for update";
        MainPanel.ResumeLayout(false);
        tlpnlContent.ResumeLayout(false);
        tlpnlContent.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label UpdateLabel;
    private ProgressBar progressBar1;
    private LinkLabel linkChangeLog;
    private Button btnUpdateNow;
    private LinkLabel linkDirectDownload;
    private TableLayoutPanel tlpnlContent;
    private SettingsLinkLabel linkRequiredDotNetRuntime;
}
