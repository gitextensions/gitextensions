namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

partial class CommitDialogSettingsPage
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
        groupBoxBehaviour = new GroupBox();
        tableLayoutPanelBehaviour = new TableLayoutPanel();
        cbRememberAmendCommitState = new CheckBox();
        chkAutocomplete = new CheckBox();
        _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages =
            new NumericUpDown();
        lblCommitDialogNumberOfPreviousMessages = new Label();
        chkShowErrorsWhenStagingFiles = new CheckBox();
        chkWriteCommitMessageInCommitWindow = new CheckBox();
        grpAdditionalButtons = new GroupBox();
        flowLayoutPanel1 = new FlowLayoutPanel();
        chkShowCommitAndPush = new CheckBox();
        chkShowResetWorkTreeChanges = new CheckBox();
        chkShowResetAllChanges = new CheckBox();
        chkEnsureCommitMessageSecondLineEmpty = new CheckBox();
        groupBoxAiCommitMessage = new GroupBox();
        tableLayoutPanelAiCommitMessage = new TableLayoutPanel();
        chkAiCommitMessageEnabled = new CheckBox();
        lblAiCommitMessageProvider = new Label();
        cboAiCommitMessageProvider = new ComboBox();
        lblAiCommitMessageBaseUrl = new Label();
        txtAiCommitMessageBaseUrl = new TextBox();
        lblAiCommitMessageModel = new Label();
        txtAiCommitMessageModel = new TextBox();
        lblAiCommitMessageApiKey = new Label();
        txtAiCommitMessageApiKey = new TextBox();
        lblAiCommitMessageMaxDiff = new Label();
        numAiCommitMessageMaxDiff = new NumericUpDown();
        lblAiCommitMessageSystemPrompt = new Label();
        txtAiCommitMessageSystemPrompt = new TextBox();
        groupBoxBehaviour.SuspendLayout();
        tableLayoutPanelBehaviour.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this
            ._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages)).BeginInit();
        grpAdditionalButtons.SuspendLayout();
        flowLayoutPanel1.SuspendLayout();
        groupBoxAiCommitMessage.SuspendLayout();
        tableLayoutPanelAiCommitMessage.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(numAiCommitMessageMaxDiff)).BeginInit();
        SuspendLayout();
        // 
        // groupBoxBehaviour
        // 
        groupBoxBehaviour.AutoSize = true;
        groupBoxBehaviour.Controls.Add(tableLayoutPanelBehaviour);
        groupBoxBehaviour.Dock = DockStyle.Top;
        groupBoxBehaviour.Location = new Point(0, 0);
        groupBoxBehaviour.Name = "groupBoxBehaviour";
        groupBoxBehaviour.Size = new Size(1014, 294);
        groupBoxBehaviour.TabIndex = 56;
        groupBoxBehaviour.TabStop = false;
        groupBoxBehaviour.Text = "Behaviour";
        // 
        // tableLayoutPanelBehaviour
        // 
        tableLayoutPanelBehaviour.AutoSize = true;
        tableLayoutPanelBehaviour.ColumnCount = 2;
        tableLayoutPanelBehaviour.ColumnStyles.Add(new ColumnStyle());
        tableLayoutPanelBehaviour.ColumnStyles.Add(new ColumnStyle());
        tableLayoutPanelBehaviour.Controls.Add(cbRememberAmendCommitState, 0, 6);
        tableLayoutPanelBehaviour.Controls.Add(chkAutocomplete, 0, 0);
        tableLayoutPanelBehaviour.Controls.Add(
            _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages, 1, 4);
        tableLayoutPanelBehaviour.Controls.Add(
            lblCommitDialogNumberOfPreviousMessages, 0, 4);
        tableLayoutPanelBehaviour.Controls.Add(chkShowErrorsWhenStagingFiles, 0, 1);
        tableLayoutPanelBehaviour.Controls.Add(chkWriteCommitMessageInCommitWindow, 0,
            3);
        tableLayoutPanelBehaviour.Controls.Add(grpAdditionalButtons, 0, 7);
        tableLayoutPanelBehaviour.Controls.Add(chkEnsureCommitMessageSecondLineEmpty,
            0, 2);
        tableLayoutPanelBehaviour.Controls.Add(groupBoxAiCommitMessage, 0, 8);
        tableLayoutPanelBehaviour.Dock = DockStyle.Top;
        tableLayoutPanelBehaviour.Location = new Point(3, 19);
        tableLayoutPanelBehaviour.Name = "tableLayoutPanelBehaviour";
        tableLayoutPanelBehaviour.RowCount = 9;
        tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
        tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
        tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
        tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
        tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
        tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
        tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
        tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
        tableLayoutPanelBehaviour.RowStyles.Add(new RowStyle());
        tableLayoutPanelBehaviour.Size = new Size(1008, 272);
        tableLayoutPanelBehaviour.TabIndex = 57;
        // 
        // cbRememberAmendCommitState
        // 
        cbRememberAmendCommitState.AutoSize = true;
        cbRememberAmendCommitState.Location = new Point(3, 147);
        cbRememberAmendCommitState.Name = "cbRememberAmendCommitState";
        cbRememberAmendCommitState.Size = new Size(351, 19);
        cbRememberAmendCommitState.TabIndex = 5;
        cbRememberAmendCommitState.Text =
            "Remember \'Amend commit\' checkbox on commit form close";
        cbRememberAmendCommitState.UseVisualStyleBackColor = true;
        // 
        // chkAutocomplete
        // 
        chkAutocomplete.AutoSize = true;
        chkAutocomplete.Location = new Point(3, 3);
        chkAutocomplete.Name = "chkAutocomplete";
        chkAutocomplete.Size = new Size(253, 19);
        chkAutocomplete.TabIndex = 0;
        chkAutocomplete.Text = "Provide auto-completion in commit dialog";
        chkAutocomplete.UseVisualStyleBackColor = true;
        // 
        // _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages
        // 
        _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Location =
            new Point(360, 118);
        _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Maximum =
            new decimal(new int[] { 999, 0, 0, 0 });
        _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Minimum =
            new decimal(new int[] { 1, 0, 0, 0 });
        _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Name =
            "_NO_TRANSLATE_CommitDialogNumberOfPreviousMessages";
        _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Size =
            new Size(123, 23);
        _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.TabIndex = 4;
        _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Value =
            new decimal(new int[] { 1, 0, 0, 0 });
        // 
        // lblCommitDialogNumberOfPreviousMessages
        // 
        lblCommitDialogNumberOfPreviousMessages.Anchor =
            AnchorStyles.Left;
        lblCommitDialogNumberOfPreviousMessages.AutoSize = true;
        lblCommitDialogNumberOfPreviousMessages.Location =
            new Point(3, 122);
        lblCommitDialogNumberOfPreviousMessages.Name =
            "lblCommitDialogNumberOfPreviousMessages";
        lblCommitDialogNumberOfPreviousMessages.Size = new Size(261, 15);
        lblCommitDialogNumberOfPreviousMessages.TabIndex = 2;
        lblCommitDialogNumberOfPreviousMessages.Text =
            "Number of previous messages in commit dialog";
        // 
        // chkShowErrorsWhenStagingFiles
        // 
        chkShowErrorsWhenStagingFiles.AutoSize = true;
        chkShowErrorsWhenStagingFiles.Location = new Point(3, 28);
        chkShowErrorsWhenStagingFiles.Name = "chkShowErrorsWhenStagingFiles";
        chkShowErrorsWhenStagingFiles.Size = new Size(186, 19);
        chkShowErrorsWhenStagingFiles.TabIndex = 1;
        chkShowErrorsWhenStagingFiles.Text = "Show errors when staging files";
        chkShowErrorsWhenStagingFiles.UseVisualStyleBackColor = true;
        // 
        // chkWriteCommitMessageInCommitWindow
        // 
        chkWriteCommitMessageInCommitWindow.AutoSize = true;
        chkWriteCommitMessageInCommitWindow.Location = new Point(3, 78);
        chkWriteCommitMessageInCommitWindow.Name = "chkWriteCommitMessageInCommitWindow";
        chkWriteCommitMessageInCommitWindow.Size = new Size(329, 34);
        chkWriteCommitMessageInCommitWindow.TabIndex = 3;
        chkWriteCommitMessageInCommitWindow.Text =
            "Compose commit messages in Commit dialog\r\n(otherwise the message will be requeste" +
            "d during commit)";
        chkWriteCommitMessageInCommitWindow.UseVisualStyleBackColor = true;
        // 
        // grpAdditionalButtons
        // 
        grpAdditionalButtons.AutoSize = true;
        grpAdditionalButtons.AutoSizeMode =
            AutoSizeMode.GrowAndShrink;
        tableLayoutPanelBehaviour.SetColumnSpan(grpAdditionalButtons, 2);
        grpAdditionalButtons.Controls.Add(flowLayoutPanel1);
        grpAdditionalButtons.Dock = DockStyle.Fill;
        grpAdditionalButtons.Location = new Point(3, 172);
        grpAdditionalButtons.Name = "grpAdditionalButtons";
        grpAdditionalButtons.Size = new Size(1002, 97);
        grpAdditionalButtons.TabIndex = 6;
        grpAdditionalButtons.TabStop = false;
        grpAdditionalButtons.Text = "Show additional buttons in commit button area";
        // 
        // flowLayoutPanel1
        // 
        flowLayoutPanel1.AutoSize = true;
        flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        flowLayoutPanel1.Controls.Add(chkShowCommitAndPush);
        flowLayoutPanel1.Controls.Add(chkShowResetWorkTreeChanges);
        flowLayoutPanel1.Controls.Add(chkShowResetAllChanges);
        flowLayoutPanel1.Dock = DockStyle.Fill;
        flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
        flowLayoutPanel1.Location = new Point(3, 19);
        flowLayoutPanel1.Name = "flowLayoutPanel1";
        flowLayoutPanel1.Size = new Size(996, 75);
        flowLayoutPanel1.TabIndex = 0;
        // 
        // chkShowCommitAndPush
        // 
        chkShowCommitAndPush.AutoSize = true;
        chkShowCommitAndPush.Location = new Point(3, 3);
        chkShowCommitAndPush.Name = "chkShowCommitAndPush";
        chkShowCommitAndPush.Size = new Size(112, 19);
        chkShowCommitAndPush.TabIndex = 0;
        chkShowCommitAndPush.Text = "Commit && Push";
        chkShowCommitAndPush.UseVisualStyleBackColor = true;
        // 
        // chkShowResetWorkTreeChanges
        // 
        chkShowResetWorkTreeChanges.AutoSize = true;
        chkShowResetWorkTreeChanges.Location = new Point(3, 28);
        chkShowResetWorkTreeChanges.Name = "chkShowResetWorkTreeChanges";
        chkShowResetWorkTreeChanges.Size = new Size(156, 19);
        chkShowResetWorkTreeChanges.TabIndex = 1;
        chkShowResetWorkTreeChanges.Text = "Reset Unstaged Changes";
        chkShowResetWorkTreeChanges.UseVisualStyleBackColor = true;
        // 
        // chkShowResetAllChanges
        // 
        chkShowResetAllChanges.AutoSize = true;
        chkShowResetAllChanges.Location = new Point(3, 53);
        chkShowResetAllChanges.Name = "chkShowResetAllChanges";
        chkShowResetAllChanges.Size = new Size(120, 19);
        chkShowResetAllChanges.TabIndex = 2;
        chkShowResetAllChanges.Text = "Reset All Changes";
        chkShowResetAllChanges.UseVisualStyleBackColor = true;
        // 
        // chkEnsureCommitMessageSecondLineEmpty
        // 
        chkEnsureCommitMessageSecondLineEmpty.AutoSize = true;
        chkEnsureCommitMessageSecondLineEmpty.Location = new Point(3, 53);
        chkEnsureCommitMessageSecondLineEmpty.Name =
            "chkEnsureCommitMessageSecondLineEmpty";
        chkEnsureCommitMessageSecondLineEmpty.Size = new Size(300, 19);
        chkEnsureCommitMessageSecondLineEmpty.TabIndex = 2;
        chkEnsureCommitMessageSecondLineEmpty.Text =
            "Ensure the second line of commit message is empty";
        chkEnsureCommitMessageSecondLineEmpty.UseVisualStyleBackColor = true;
        //
        // groupBoxAiCommitMessage
        //
        groupBoxAiCommitMessage.AutoSize = true;
        groupBoxAiCommitMessage.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tableLayoutPanelBehaviour.SetColumnSpan(groupBoxAiCommitMessage, 2);
        groupBoxAiCommitMessage.Controls.Add(tableLayoutPanelAiCommitMessage);
        groupBoxAiCommitMessage.Dock = DockStyle.Fill;
        groupBoxAiCommitMessage.Name = "groupBoxAiCommitMessage";
        groupBoxAiCommitMessage.Padding = new Padding(3);
        groupBoxAiCommitMessage.TabIndex = 7;
        groupBoxAiCommitMessage.TabStop = false;
        groupBoxAiCommitMessage.Text = "AI commit message";
        //
        // tableLayoutPanelAiCommitMessage
        //
        tableLayoutPanelAiCommitMessage.AutoSize = true;
        tableLayoutPanelAiCommitMessage.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tableLayoutPanelAiCommitMessage.ColumnCount = 2;
        tableLayoutPanelAiCommitMessage.ColumnStyles.Add(new ColumnStyle());
        tableLayoutPanelAiCommitMessage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayoutPanelAiCommitMessage.Controls.Add(chkAiCommitMessageEnabled, 0, 0);
        tableLayoutPanelAiCommitMessage.Controls.Add(lblAiCommitMessageProvider, 0, 1);
        tableLayoutPanelAiCommitMessage.Controls.Add(cboAiCommitMessageProvider, 1, 1);
        tableLayoutPanelAiCommitMessage.Controls.Add(lblAiCommitMessageBaseUrl, 0, 2);
        tableLayoutPanelAiCommitMessage.Controls.Add(txtAiCommitMessageBaseUrl, 1, 2);
        tableLayoutPanelAiCommitMessage.Controls.Add(lblAiCommitMessageModel, 0, 3);
        tableLayoutPanelAiCommitMessage.Controls.Add(txtAiCommitMessageModel, 1, 3);
        tableLayoutPanelAiCommitMessage.Controls.Add(lblAiCommitMessageApiKey, 0, 4);
        tableLayoutPanelAiCommitMessage.Controls.Add(txtAiCommitMessageApiKey, 1, 4);
        tableLayoutPanelAiCommitMessage.Controls.Add(lblAiCommitMessageMaxDiff, 0, 5);
        tableLayoutPanelAiCommitMessage.Controls.Add(numAiCommitMessageMaxDiff, 1, 5);
        tableLayoutPanelAiCommitMessage.Controls.Add(lblAiCommitMessageSystemPrompt, 0, 6);
        tableLayoutPanelAiCommitMessage.Controls.Add(txtAiCommitMessageSystemPrompt, 1, 6);
        tableLayoutPanelAiCommitMessage.Dock = DockStyle.Fill;
        tableLayoutPanelAiCommitMessage.Name = "tableLayoutPanelAiCommitMessage";
        tableLayoutPanelAiCommitMessage.RowCount = 7;
        tableLayoutPanelAiCommitMessage.RowStyles.Add(new RowStyle());
        tableLayoutPanelAiCommitMessage.RowStyles.Add(new RowStyle());
        tableLayoutPanelAiCommitMessage.RowStyles.Add(new RowStyle());
        tableLayoutPanelAiCommitMessage.RowStyles.Add(new RowStyle());
        tableLayoutPanelAiCommitMessage.RowStyles.Add(new RowStyle());
        tableLayoutPanelAiCommitMessage.RowStyles.Add(new RowStyle());
        tableLayoutPanelAiCommitMessage.RowStyles.Add(new RowStyle());
        tableLayoutPanelAiCommitMessage.SetColumnSpan(chkAiCommitMessageEnabled, 2);
        tableLayoutPanelAiCommitMessage.TabIndex = 0;
        //
        // chkAiCommitMessageEnabled
        //
        chkAiCommitMessageEnabled.AutoSize = true;
        chkAiCommitMessageEnabled.Margin = new Padding(3, 3, 3, 6);
        chkAiCommitMessageEnabled.Name = "chkAiCommitMessageEnabled";
        chkAiCommitMessageEnabled.TabIndex = 0;
        chkAiCommitMessageEnabled.Text =
            "Enable AI commit message generation (adds a button to the commit dialog; the staged diff is sent to the configured endpoint only when you click it)";
        chkAiCommitMessageEnabled.UseVisualStyleBackColor = true;
        //
        // lblAiCommitMessageProvider
        //
        lblAiCommitMessageProvider.Anchor = AnchorStyles.Left;
        lblAiCommitMessageProvider.AutoSize = true;
        lblAiCommitMessageProvider.Margin = new Padding(3, 6, 3, 3);
        lblAiCommitMessageProvider.Name = "lblAiCommitMessageProvider";
        lblAiCommitMessageProvider.TabIndex = 1;
        lblAiCommitMessageProvider.Text = "Provider:";
        //
        // cboAiCommitMessageProvider
        //
        cboAiCommitMessageProvider.Anchor = AnchorStyles.Left;
        cboAiCommitMessageProvider.DropDownStyle = ComboBoxStyle.DropDownList;
        cboAiCommitMessageProvider.FormattingEnabled = true;
        cboAiCommitMessageProvider.Margin = new Padding(3);
        cboAiCommitMessageProvider.Name = "cboAiCommitMessageProvider";
        cboAiCommitMessageProvider.Size = new Size(220, 23);
        cboAiCommitMessageProvider.TabIndex = 2;
        cboAiCommitMessageProvider.SelectedIndexChanged += AiProvider_SelectedIndexChanged;
        //
        // lblAiCommitMessageBaseUrl
        //
        lblAiCommitMessageBaseUrl.Anchor = AnchorStyles.Left;
        lblAiCommitMessageBaseUrl.AutoSize = true;
        lblAiCommitMessageBaseUrl.Margin = new Padding(3, 6, 3, 3);
        lblAiCommitMessageBaseUrl.Name = "lblAiCommitMessageBaseUrl";
        lblAiCommitMessageBaseUrl.TabIndex = 3;
        lblAiCommitMessageBaseUrl.Text = "API base URL (OpenAI-compatible):";
        //
        // txtAiCommitMessageBaseUrl
        //
        txtAiCommitMessageBaseUrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtAiCommitMessageBaseUrl.Margin = new Padding(3);
        txtAiCommitMessageBaseUrl.Name = "txtAiCommitMessageBaseUrl";
        txtAiCommitMessageBaseUrl.TabIndex = 4;
        //
        // lblAiCommitMessageModel
        //
        lblAiCommitMessageModel.Anchor = AnchorStyles.Left;
        lblAiCommitMessageModel.AutoSize = true;
        lblAiCommitMessageModel.Margin = new Padding(3, 6, 3, 3);
        lblAiCommitMessageModel.Name = "lblAiCommitMessageModel";
        lblAiCommitMessageModel.TabIndex = 5;
        lblAiCommitMessageModel.Text = "Model:";
        //
        // txtAiCommitMessageModel
        //
        txtAiCommitMessageModel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtAiCommitMessageModel.Margin = new Padding(3);
        txtAiCommitMessageModel.Name = "txtAiCommitMessageModel";
        txtAiCommitMessageModel.TabIndex = 6;
        //
        // lblAiCommitMessageApiKey
        //
        lblAiCommitMessageApiKey.Anchor = AnchorStyles.Left;
        lblAiCommitMessageApiKey.AutoSize = true;
        lblAiCommitMessageApiKey.Margin = new Padding(3, 6, 3, 3);
        lblAiCommitMessageApiKey.Name = "lblAiCommitMessageApiKey";
        lblAiCommitMessageApiKey.TabIndex = 7;
        lblAiCommitMessageApiKey.Text = "API key (leave blank for local servers such as Ollama):";
        //
        // txtAiCommitMessageApiKey
        //
        txtAiCommitMessageApiKey.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtAiCommitMessageApiKey.Margin = new Padding(3);
        txtAiCommitMessageApiKey.Name = "txtAiCommitMessageApiKey";
        txtAiCommitMessageApiKey.TabIndex = 8;
        txtAiCommitMessageApiKey.UseSystemPasswordChar = true;
        //
        // lblAiCommitMessageMaxDiff
        //
        lblAiCommitMessageMaxDiff.Anchor = AnchorStyles.Left;
        lblAiCommitMessageMaxDiff.AutoSize = true;
        lblAiCommitMessageMaxDiff.Margin = new Padding(3, 6, 3, 3);
        lblAiCommitMessageMaxDiff.Name = "lblAiCommitMessageMaxDiff";
        lblAiCommitMessageMaxDiff.TabIndex = 9;
        lblAiCommitMessageMaxDiff.Text = "Max diff characters sent to the model (0 = no limit):";
        //
        // numAiCommitMessageMaxDiff
        //
        numAiCommitMessageMaxDiff.Anchor = AnchorStyles.Left;
        numAiCommitMessageMaxDiff.Increment = new decimal(new int[] { 1000, 0, 0, 0 });
        numAiCommitMessageMaxDiff.Margin = new Padding(3);
        numAiCommitMessageMaxDiff.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        numAiCommitMessageMaxDiff.Name = "numAiCommitMessageMaxDiff";
        numAiCommitMessageMaxDiff.Size = new Size(120, 23);
        numAiCommitMessageMaxDiff.TabIndex = 10;
        numAiCommitMessageMaxDiff.ThousandsSeparator = true;
        //
        // lblAiCommitMessageSystemPrompt
        //
        lblAiCommitMessageSystemPrompt.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        lblAiCommitMessageSystemPrompt.AutoSize = true;
        lblAiCommitMessageSystemPrompt.Margin = new Padding(3, 6, 3, 3);
        lblAiCommitMessageSystemPrompt.Name = "lblAiCommitMessageSystemPrompt";
        lblAiCommitMessageSystemPrompt.TabIndex = 11;
        lblAiCommitMessageSystemPrompt.Text = "System prompt:";
        //
        // txtAiCommitMessageSystemPrompt
        //
        txtAiCommitMessageSystemPrompt.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtAiCommitMessageSystemPrompt.Margin = new Padding(3);
        txtAiCommitMessageSystemPrompt.Multiline = true;
        txtAiCommitMessageSystemPrompt.Name = "txtAiCommitMessageSystemPrompt";
        txtAiCommitMessageSystemPrompt.ScrollBars = ScrollBars.Vertical;
        txtAiCommitMessageSystemPrompt.Size = new Size(400, 140);
        txtAiCommitMessageSystemPrompt.TabIndex = 12;
        txtAiCommitMessageSystemPrompt.WordWrap = true;
        //
        // CommitDialogSettingsPage
        //
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScroll = true;
        Controls.Add(groupBoxBehaviour);
        Name = "CommitDialogSettingsPage";
        Size = new Size(1014, 950);
        Text = "Commit dialog";
        groupBoxBehaviour.ResumeLayout(false);
        groupBoxBehaviour.PerformLayout();
        tableLayoutPanelBehaviour.ResumeLayout(false);
        tableLayoutPanelBehaviour.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this
            ._NO_TRANSLATE_CommitDialogNumberOfPreviousMessages)).EndInit();
        grpAdditionalButtons.ResumeLayout(false);
        grpAdditionalButtons.PerformLayout();
        flowLayoutPanel1.ResumeLayout(false);
        flowLayoutPanel1.PerformLayout();
        groupBoxAiCommitMessage.ResumeLayout(false);
        groupBoxAiCommitMessage.PerformLayout();
        tableLayoutPanelAiCommitMessage.ResumeLayout(false);
        tableLayoutPanelAiCommitMessage.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(numAiCommitMessageMaxDiff)).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private GroupBox groupBoxBehaviour;
    private CheckBox chkWriteCommitMessageInCommitWindow;
    private CheckBox chkShowErrorsWhenStagingFiles;
    private Label lblCommitDialogNumberOfPreviousMessages;
    private NumericUpDown _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages;
    private TableLayoutPanel tableLayoutPanelBehaviour;
    private GroupBox grpAdditionalButtons;
    private FlowLayoutPanel flowLayoutPanel1;
    private CheckBox chkShowCommitAndPush;
    private CheckBox chkShowResetWorkTreeChanges;
    private CheckBox chkShowResetAllChanges;
    private CheckBox chkEnsureCommitMessageSecondLineEmpty;
    private CheckBox chkAutocomplete;
    private CheckBox cbRememberAmendCommitState;
    private GroupBox groupBoxAiCommitMessage;
    private TableLayoutPanel tableLayoutPanelAiCommitMessage;
    private CheckBox chkAiCommitMessageEnabled;
    private Label lblAiCommitMessageProvider;
    private ComboBox cboAiCommitMessageProvider;
    private Label lblAiCommitMessageBaseUrl;
    private TextBox txtAiCommitMessageBaseUrl;
    private Label lblAiCommitMessageModel;
    private TextBox txtAiCommitMessageModel;
    private Label lblAiCommitMessageApiKey;
    private TextBox txtAiCommitMessageApiKey;
    private Label lblAiCommitMessageMaxDiff;
    private NumericUpDown numAiCommitMessageMaxDiff;
    private Label lblAiCommitMessageSystemPrompt;
    private TextBox txtAiCommitMessageSystemPrompt;
}
