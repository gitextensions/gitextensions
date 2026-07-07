namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

partial class AiFilterSettingsPage
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
        mainPanel = new FlowLayoutPanel();
        grpBackend = new GroupBox();
        tableBackend = new TableLayoutPanel();
        lblBackend = new Label();
        cboBackend = new ComboBox();
        lblClaudeCodeExecutable = new Label();
        txtClaudeCodeExecutable = new TextBox();
        grpConnection = new GroupBox();
        tableConnection = new TableLayoutPanel();
        lblEndpoint = new Label();
        txtEndpoint = new TextBox();
        lblModel = new Label();
        txtModel = new TextBox();
        lblApiKey = new Label();
        txtApiKey = new TextBox();
        lblApiKeyHint = new Label();
        lblAnthropicVersion = new Label();
        txtAnthropicVersion = new TextBox();
        lblMaxDiffChars = new Label();
        numMaxDiffChars = new NumericUpDown();
        grpCategories = new GroupBox();
        flowCategories = new FlowLayoutPanel();
        chkImports = new CheckBox();
        chkCallerRenames = new CheckBox();
        chkSyncToAsync = new CheckBox();
        chkStyleOnly = new CheckBox();
        lblPrivacyHint = new Label();
        mainPanel.SuspendLayout();
        grpBackend.SuspendLayout();
        tableBackend.SuspendLayout();
        grpConnection.SuspendLayout();
        tableConnection.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numMaxDiffChars).BeginInit();
        grpCategories.SuspendLayout();
        flowCategories.SuspendLayout();
        SuspendLayout();
        //
        // mainPanel
        //
        mainPanel.AutoSize = true;
        mainPanel.Controls.Add(grpBackend);
        mainPanel.Controls.Add(grpConnection);
        mainPanel.Controls.Add(grpCategories);
        mainPanel.Controls.Add(lblPrivacyHint);
        mainPanel.Dock = DockStyle.Top;
        mainPanel.FlowDirection = FlowDirection.TopDown;
        mainPanel.Location = new Point(0, 0);
        mainPanel.Name = "mainPanel";
        mainPanel.Size = new Size(1014, 400);
        mainPanel.TabIndex = 0;
        mainPanel.WrapContents = false;
        //
        // grpBackend
        //
        grpBackend.AutoSize = true;
        grpBackend.Controls.Add(tableBackend);
        grpBackend.Location = new Point(3, 3);
        grpBackend.Name = "grpBackend";
        grpBackend.Size = new Size(660, 90);
        grpBackend.TabIndex = 0;
        grpBackend.TabStop = false;
        grpBackend.Text = "Backend";
        //
        // tableBackend
        //
        tableBackend.AutoSize = true;
        tableBackend.ColumnCount = 2;
        tableBackend.ColumnStyles.Add(new ColumnStyle());
        tableBackend.ColumnStyles.Add(new ColumnStyle());
        tableBackend.Controls.Add(lblBackend, 0, 0);
        tableBackend.Controls.Add(cboBackend, 1, 0);
        tableBackend.Controls.Add(lblClaudeCodeExecutable, 0, 1);
        tableBackend.Controls.Add(txtClaudeCodeExecutable, 1, 1);
        tableBackend.Dock = DockStyle.Top;
        tableBackend.Location = new Point(3, 19);
        tableBackend.Name = "tableBackend";
        tableBackend.RowCount = 2;
        tableBackend.RowStyles.Add(new RowStyle());
        tableBackend.RowStyles.Add(new RowStyle());
        tableBackend.Size = new Size(654, 62);
        tableBackend.TabIndex = 0;
        //
        // lblBackend
        //
        lblBackend.Anchor = AnchorStyles.Left;
        lblBackend.AutoSize = true;
        lblBackend.Location = new Point(3, 8);
        lblBackend.Name = "lblBackend";
        lblBackend.Size = new Size(52, 15);
        lblBackend.TabIndex = 0;
        lblBackend.Text = "Provider";
        //
        // cboBackend
        //
        cboBackend.DropDownStyle = ComboBoxStyle.DropDownList;
        cboBackend.FormattingEnabled = true;
        cboBackend.Items.AddRange(new object[] { "Anthropic API (requires API key)", "Claude Code (uses your Claude subscription)" });
        cboBackend.Location = new Point(150, 3);
        cboBackend.Name = "cboBackend";
        cboBackend.Size = new Size(390, 23);
        cboBackend.TabIndex = 1;
        //
        // lblClaudeCodeExecutable
        //
        lblClaudeCodeExecutable.Anchor = AnchorStyles.Left;
        lblClaudeCodeExecutable.AutoSize = true;
        lblClaudeCodeExecutable.Location = new Point(3, 37);
        lblClaudeCodeExecutable.Name = "lblClaudeCodeExecutable";
        lblClaudeCodeExecutable.Size = new Size(141, 15);
        lblClaudeCodeExecutable.TabIndex = 2;
        lblClaudeCodeExecutable.Text = "Claude Code command";
        //
        // txtClaudeCodeExecutable
        //
        txtClaudeCodeExecutable.Location = new Point(150, 33);
        txtClaudeCodeExecutable.Name = "txtClaudeCodeExecutable";
        txtClaudeCodeExecutable.Size = new Size(390, 23);
        txtClaudeCodeExecutable.TabIndex = 3;
        //
        // grpConnection
        //
        grpConnection.AutoSize = true;
        grpConnection.Controls.Add(tableConnection);
        grpConnection.Location = new Point(3, 3);
        grpConnection.Name = "grpConnection";
        grpConnection.Size = new Size(660, 200);
        grpConnection.TabIndex = 0;
        grpConnection.TabStop = false;
        grpConnection.Text = "Anthropic connection";
        //
        // tableConnection
        //
        tableConnection.AutoSize = true;
        tableConnection.ColumnCount = 2;
        tableConnection.ColumnStyles.Add(new ColumnStyle());
        tableConnection.ColumnStyles.Add(new ColumnStyle());
        tableConnection.Controls.Add(lblEndpoint, 0, 0);
        tableConnection.Controls.Add(txtEndpoint, 1, 0);
        tableConnection.Controls.Add(lblModel, 0, 1);
        tableConnection.Controls.Add(txtModel, 1, 1);
        tableConnection.Controls.Add(lblApiKey, 0, 2);
        tableConnection.Controls.Add(txtApiKey, 1, 2);
        tableConnection.Controls.Add(lblApiKeyHint, 1, 3);
        tableConnection.Controls.Add(lblAnthropicVersion, 0, 4);
        tableConnection.Controls.Add(txtAnthropicVersion, 1, 4);
        tableConnection.Controls.Add(lblMaxDiffChars, 0, 5);
        tableConnection.Controls.Add(numMaxDiffChars, 1, 5);
        tableConnection.Dock = DockStyle.Top;
        tableConnection.Location = new Point(3, 19);
        tableConnection.Name = "tableConnection";
        tableConnection.RowCount = 6;
        tableConnection.RowStyles.Add(new RowStyle());
        tableConnection.RowStyles.Add(new RowStyle());
        tableConnection.RowStyles.Add(new RowStyle());
        tableConnection.RowStyles.Add(new RowStyle());
        tableConnection.RowStyles.Add(new RowStyle());
        tableConnection.RowStyles.Add(new RowStyle());
        tableConnection.Size = new Size(654, 178);
        tableConnection.TabIndex = 0;
        //
        // lblEndpoint
        //
        lblEndpoint.Anchor = AnchorStyles.Left;
        lblEndpoint.AutoSize = true;
        lblEndpoint.Location = new Point(3, 7);
        lblEndpoint.Name = "lblEndpoint";
        lblEndpoint.Size = new Size(55, 15);
        lblEndpoint.TabIndex = 0;
        lblEndpoint.Text = "Endpoint";
        //
        // txtEndpoint
        //
        txtEndpoint.Location = new Point(120, 3);
        txtEndpoint.Name = "txtEndpoint";
        txtEndpoint.Size = new Size(420, 23);
        txtEndpoint.TabIndex = 1;
        //
        // lblModel
        //
        lblModel.Anchor = AnchorStyles.Left;
        lblModel.AutoSize = true;
        lblModel.Location = new Point(3, 36);
        lblModel.Name = "lblModel";
        lblModel.Size = new Size(40, 15);
        lblModel.TabIndex = 2;
        lblModel.Text = "Model";
        //
        // txtModel
        //
        txtModel.Location = new Point(120, 32);
        txtModel.Name = "txtModel";
        txtModel.Size = new Size(420, 23);
        txtModel.TabIndex = 3;
        //
        // lblApiKey
        //
        lblApiKey.Anchor = AnchorStyles.Left;
        lblApiKey.AutoSize = true;
        lblApiKey.Location = new Point(3, 65);
        lblApiKey.Name = "lblApiKey";
        lblApiKey.Size = new Size(47, 15);
        lblApiKey.TabIndex = 4;
        lblApiKey.Text = "API key";
        //
        // txtApiKey
        //
        txtApiKey.Location = new Point(120, 61);
        txtApiKey.Name = "txtApiKey";
        txtApiKey.Size = new Size(420, 23);
        txtApiKey.TabIndex = 5;
        txtApiKey.UseSystemPasswordChar = true;
        //
        // lblApiKeyHint
        //
        lblApiKeyHint.AutoSize = true;
        lblApiKeyHint.ForeColor = SystemColors.GrayText;
        lblApiKeyHint.Location = new Point(120, 87);
        lblApiKeyHint.Margin = new Padding(3, 0, 3, 6);
        lblApiKeyHint.Name = "lblApiKeyHint";
        lblApiKeyHint.Size = new Size(400, 15);
        lblApiKeyHint.TabIndex = 6;
        lblApiKeyHint.Text = "Leave empty to use the ANTHROPIC_API_KEY environment variable.";
        //
        // lblAnthropicVersion
        //
        lblAnthropicVersion.Anchor = AnchorStyles.Left;
        lblAnthropicVersion.AutoSize = true;
        lblAnthropicVersion.Location = new Point(3, 115);
        lblAnthropicVersion.Name = "lblAnthropicVersion";
        lblAnthropicVersion.Size = new Size(103, 15);
        lblAnthropicVersion.TabIndex = 7;
        lblAnthropicVersion.Text = "anthropic-version";
        //
        // txtAnthropicVersion
        //
        txtAnthropicVersion.Location = new Point(120, 111);
        txtAnthropicVersion.Name = "txtAnthropicVersion";
        txtAnthropicVersion.Size = new Size(200, 23);
        txtAnthropicVersion.TabIndex = 8;
        //
        // lblMaxDiffChars
        //
        lblMaxDiffChars.Anchor = AnchorStyles.Left;
        lblMaxDiffChars.AutoSize = true;
        lblMaxDiffChars.Location = new Point(3, 146);
        lblMaxDiffChars.Name = "lblMaxDiffChars";
        lblMaxDiffChars.Size = new Size(111, 15);
        lblMaxDiffChars.TabIndex = 9;
        lblMaxDiffChars.Text = "Max diff chars/file";
        //
        // numMaxDiffChars
        //
        numMaxDiffChars.Location = new Point(120, 142);
        numMaxDiffChars.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        numMaxDiffChars.Minimum = new decimal(new int[] { 1000, 0, 0, 0 });
        numMaxDiffChars.Name = "numMaxDiffChars";
        numMaxDiffChars.Size = new Size(120, 23);
        numMaxDiffChars.TabIndex = 10;
        numMaxDiffChars.ThousandsSeparator = true;
        numMaxDiffChars.Value = new decimal(new int[] { 20000, 0, 0, 0 });
        //
        // grpCategories
        //
        grpCategories.AutoSize = true;
        grpCategories.Controls.Add(flowCategories);
        grpCategories.Location = new Point(3, 209);
        grpCategories.Name = "grpCategories";
        grpCategories.Size = new Size(660, 130);
        grpCategories.TabIndex = 1;
        grpCategories.TabStop = false;
        grpCategories.Text = "Categories to filter away";
        //
        // flowCategories
        //
        flowCategories.AutoSize = true;
        flowCategories.Controls.Add(chkImports);
        flowCategories.Controls.Add(chkCallerRenames);
        flowCategories.Controls.Add(chkSyncToAsync);
        flowCategories.Controls.Add(chkStyleOnly);
        flowCategories.Dock = DockStyle.Top;
        flowCategories.FlowDirection = FlowDirection.TopDown;
        flowCategories.Location = new Point(3, 19);
        flowCategories.Name = "flowCategories";
        flowCategories.Size = new Size(654, 108);
        flowCategories.TabIndex = 0;
        flowCategories.WrapContents = false;
        //
        // chkImports
        //
        chkImports.AutoSize = true;
        chkImports.Location = new Point(3, 3);
        chkImports.Name = "chkImports";
        chkImports.Size = new Size(300, 19);
        chkImports.TabIndex = 0;
        chkImports.Text = "Import-only changes (e.g. C# using directives)";
        chkImports.UseVisualStyleBackColor = true;
        //
        // chkCallerRenames
        //
        chkCallerRenames.AutoSize = true;
        chkCallerRenames.Location = new Point(3, 28);
        chkCallerRenames.Name = "chkCallerRenames";
        chkCallerRenames.Size = new Size(300, 19);
        chkCallerRenames.TabIndex = 1;
        chkCallerRenames.Text = "Renames applied at caller sites (not the declaration)";
        chkCallerRenames.UseVisualStyleBackColor = true;
        //
        // chkSyncToAsync
        //
        chkSyncToAsync.AutoSize = true;
        chkSyncToAsync.Location = new Point(3, 53);
        chkSyncToAsync.Name = "chkSyncToAsync";
        chkSyncToAsync.Size = new Size(300, 19);
        chkSyncToAsync.TabIndex = 2;
        chkSyncToAsync.Text = "Sync → async conversions (.NET)";
        chkSyncToAsync.UseVisualStyleBackColor = true;
        //
        // chkStyleOnly
        //
        chkStyleOnly.AutoSize = true;
        chkStyleOnly.Location = new Point(3, 78);
        chkStyleOnly.Name = "chkStyleOnly";
        chkStyleOnly.Size = new Size(300, 19);
        chkStyleOnly.Text = "Style-only changes (e.g. whitespace, .NET)";
        chkStyleOnly.TabIndex = 3;
        chkStyleOnly.UseVisualStyleBackColor = true;
        //
        // lblPrivacyHint
        //
        lblPrivacyHint.AutoSize = true;
        lblPrivacyHint.ForeColor = SystemColors.GrayText;
        lblPrivacyHint.Location = new Point(3, 342);
        lblPrivacyHint.Margin = new Padding(3, 6, 3, 3);
        lblPrivacyHint.Name = "lblPrivacyHint";
        lblPrivacyHint.Size = new Size(600, 30);
        lblPrivacyHint.TabIndex = 2;
        lblPrivacyHint.Text = "Note: enabling the AI filter sends the diffs of the listed files to Claude (via the selected backend).\r\nOnly use it with repositories whose contents you are allowed to share with the service.";
        //
        // AiFilterSettingsPage
        //
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScroll = true;
        Controls.Add(mainPanel);
        Name = "AiFilterSettingsPage";
        Size = new Size(1014, 950);
        Text = "Diff AI filter";
        mainPanel.ResumeLayout(false);
        mainPanel.PerformLayout();
        grpBackend.ResumeLayout(false);
        grpBackend.PerformLayout();
        tableBackend.ResumeLayout(false);
        tableBackend.PerformLayout();
        grpConnection.ResumeLayout(false);
        grpConnection.PerformLayout();
        tableConnection.ResumeLayout(false);
        tableConnection.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numMaxDiffChars).EndInit();
        grpCategories.ResumeLayout(false);
        grpCategories.PerformLayout();
        flowCategories.ResumeLayout(false);
        flowCategories.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private FlowLayoutPanel mainPanel;
    private GroupBox grpBackend;
    private TableLayoutPanel tableBackend;
    private Label lblBackend;
    private ComboBox cboBackend;
    private Label lblClaudeCodeExecutable;
    private TextBox txtClaudeCodeExecutable;
    private GroupBox grpConnection;
    private TableLayoutPanel tableConnection;
    private Label lblEndpoint;
    private TextBox txtEndpoint;
    private Label lblModel;
    private TextBox txtModel;
    private Label lblApiKey;
    private TextBox txtApiKey;
    private Label lblApiKeyHint;
    private Label lblAnthropicVersion;
    private TextBox txtAnthropicVersion;
    private Label lblMaxDiffChars;
    private NumericUpDown numMaxDiffChars;
    private GroupBox grpCategories;
    private FlowLayoutPanel flowCategories;
    private CheckBox chkImports;
    private CheckBox chkCallerRenames;
    private CheckBox chkSyncToAsync;
    private CheckBox chkStyleOnly;
    private Label lblPrivacyHint;
}
