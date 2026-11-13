// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Full.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// <copyright file="BugReportForm.cs" company="Git Extensions">
//   Copyright (c) 2019 Igor Velikorossov. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input.Platform;
using Avalonia.Media;
using BugReporter.Serialization;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Extensibility.Translations.Xliff;
using GitExtUtils.GitUI;
using GitUI;
using GitUI.Compat;
using Microsoft;
using ResourceManager;
using MessageBoxes = GitUI.MessageBoxes;
using MessageBoxDefaultButton = GitExtensions.Shims.WinForms.MessageBoxDefaultButton;
using Report = BugReporter.Info.Report;

namespace BugReporter;

public partial class BugReportForm : GitExtensionsFormBase, ITranslate
{
    private readonly TranslationString _title = new("Error Report");
    private readonly TranslationString _submitGitHubMessage = new(@"Give as much as information as possible please to help the developers solve this issue. Otherwise, your issue ticket may be closed without any follow-up from the developers.

Because of this, make sure to fill in all the fields in the report template please.

Send report?");
    private readonly TranslationString _toolTipCopy = new("Copy the issue details into clipboard");
    private readonly TranslationString _toolTipSendQuit = new("Report the issue to GitHub and quit application.\r\nA valid GitHub account is required");
    private readonly TranslationString _toolTipQuit = new("Quit application without reporting the issue");
    private readonly TranslationString _noReproStepsSuppliedErrorMessage = new("Please provide as much as information as possible to help the developers solve this issue.");

    private static readonly IErrorReportUrlBuilder ErrorReportBodyBuilder;
    private static readonly GitHubUrlBuilder UrlBuilder;
    private SerializableException? _lastException;
    private Report? _lastReport;
    private string? _exceptionInfo;
    private string? _environmentInfo;

    [GeneratedRegex(@"\s|\r|\n", RegexOptions.ExplicitCapture)]
    private static partial Regex WhitespaceRegex { get; }

    static BugReportForm()
    {
        ErrorReportBodyBuilder = new ErrorReportUrlBuilder();
        UrlBuilder = new GitHubUrlBuilder(ErrorReportBodyBuilder);
    }

    public BugReportForm()
    {
        InitializeComponent();

        btnCopy.Click += btnCopy_Click;
        sendAndQuitButton.Click += SendAndQuitButton_Click;
        quitButton.Click += QuitButton_Click;
        IgnoreButton.Click += IgnoreButton_Click;
        CancelButton = quitButton;

        // Framework constraint: native content templates add images while captions retain their original translation keys.
        btnCopy.ContentTemplate = new FuncDataTemplate<string>((text, _) => new ImageTextButtonContent { Text = text, Image = GitUI.Properties.Images.CopyToClipboard });
        sendAndQuitButton.ContentTemplate = new FuncDataTemplate<string>((text, _) => new ImageTextButtonContent { Text = text, Image = GitUI.Properties.Images.GitHub });

        // Scaling
        // Avalonia uses device-independent dimensions and the shared application icon.
        exceptionDetails.PropertyColumnWidth = DpiUtil.Scale(101);
        exceptionDetails.InformationColumnWidth = DpiUtil.Scale(350);

        warningLabel.TextWrapping = TextWrapping.Wrap;

        const int automaticDelay = 100;
        ToolTip.SetShowDelay(btnCopy, automaticDelay);
        ToolTip.SetShowDelay(sendAndQuitButton, automaticDelay);
        ToolTip.SetShowDelay(quitButton, automaticDelay);
        ToolTip.SetTip(btnCopy, _toolTipCopy.Text);
        ToolTip.SetTip(sendAndQuitButton, _toolTipSendQuit.Text);
        ToolTip.SetTip(quitButton, _toolTipQuit.Text);

        // ToDo: Displaying report contents properly requires some more work.
        mainTabs.Items.Remove(reportContentsTabPage);
        InitializeComplete();
    }

    public DialogResult ShowDialog(IWin32Window? owner, SerializableException exception, string exceptionInfo, string environmentInfo, bool canIgnore, bool showIgnore, bool focusDetails)
    {
        _lastException = exception;
        _lastReport = new Report(_lastException);
        _exceptionInfo = exceptionInfo;
        _environmentInfo = environmentInfo;

        Validates.NotNull(_lastReport.GeneralInfo);

        Text = $@"{_lastReport.GeneralInfo.HostApplication} {_title.Text}";

        // Fill in the 'General' tab
        warningPictureBox.Source = GitUI.Properties.Images.Warning;
        exceptionTextBox.Text = _lastException.Type;
        exceptionMessageTextBox.Text = _lastException.Message;
        targetSiteTextBox.Text = _lastException.TargetSite;
        applicationTextBox.Text = $@"{_lastReport.GeneralInfo.HostApplication} [{_lastReport.GeneralInfo.HostApplicationVersion}]";
        gitTextBox.Text = _lastReport.GeneralInfo.GitVersion;
        dateTimeTextBox.Text = _lastReport.GeneralInfo.DateTime;
        clrTextBox.Text = _lastReport.GeneralInfo.ClrVersion;

        // Fill in the 'Exception' tab
        exceptionDetails.Initialize(_lastException);

        if (focusDetails)
        {
            mainTabs.SelectedItem = exceptionTabPage;

            // Hold back users from reporting UserExternalOperationExceptions directly
            sendAndQuitButton.IsEnabled = false;
        }

        // Avalonia exposes close requests instead of WinForms ControlBox.
        Closing += (_, e) => e.Cancel |= !canIgnore
            && e.CloseReason == WindowCloseReason.WindowClosing && !e.IsProgrammatic;
        IgnoreButton.IsEnabled = showIgnore && canIgnore;
        IgnoreButton.IsVisible = showIgnore;

        DialogResult = DialogResult.None;

        // ToDo: Fill in the 'Report Contents' tab);
        DialogResult result = ShowDialog(owner);

        // Write back the user description (as we passed 'report' as a reference since it is a reference object anyway)
        _lastReport.GeneralInfo.UserDescription = descriptionTextBox.Text;

        return result;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // The shared Avalonia theme supplies native control styling.
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        descriptionTextBox.Focus();
    }

    private static bool CheckContainsInfo(string input)
    {
        string text = WhitespaceRegex.Replace(input, string.Empty);
        return !string.IsNullOrWhiteSpace(text);
    }

    private void QuitButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Abort;
        Close();
    }

    private void SendAndQuitButton_Click(object sender, EventArgs e)
    {
        bool hasUserText = CheckContainsInfo(descriptionTextBox.Text ?? string.Empty);
        if (!hasUserText)
        {
            MessageBoxes.ShowError(this, _noReproStepsSuppliedErrorMessage.Text, _title.Text);
            descriptionTextBox.Focus();
            return;
        }

        if (!MessageBoxes.Confirm(this, _submitGitHubMessage.Text, _title.Text,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
        {
            return;
        }

        Validates.NotNull(_lastException);

        string? url = UrlBuilder.Build("https://github.com/begota98/gitextensions/issues/new", _lastException, _exceptionInfo, _environmentInfo, descriptionTextBox.Text);

        // Use the portable shell/portal boundary for the fork issue form.
        OsShellUtil.OpenUrlInDefaultBrowser(url!);

        DialogResult = DialogResult.Abort;
        Close();
    }

    private void btnCopy_Click(object sender, EventArgs e)
    {
        Validates.NotNull(_lastException);

        string report = ErrorReportBodyBuilder.CopyText(_lastException, _exceptionInfo, _environmentInfo, descriptionTextBox.Text);
        if (string.IsNullOrWhiteSpace(report))
        {
            return;
        }

        // Avalonia's clipboard belongs to this report window, including before a main window exists.
        DispatcherPump.Wait(async () =>
        {
            var clipboard = Clipboard ?? throw new InvalidOperationException("The report window has no clipboard.");
            await clipboard.SetTextAsync(report);
            return true;
        });

        DialogResult = DialogResult.None;
    }

    private void IgnoreButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Ignore;
        Close();
    }

    #region Translation

    public override void AddTranslationItems(ITranslation translation)
    {
        AvaloniaTranslationUtils.AddTranslationItemsFromFields(nameof(BugReportForm), this, translation);
    }

    public override void TranslateItems(ITranslation translation)
    {
        AvaloniaTranslationUtils.TranslateItemsFromFields(nameof(BugReportForm), this, translation);
    }

    protected void TranslateItem(string itemName, object item)
    {
        IDictionary<string, TranslationFile> translation = Translator.GetTranslation(AppSettings.CurrentTranslation);

        if (translation.Count == 0)
        {
            return;
        }

        (string itemName, object item)[] itemsToTranslate = [(itemName, item)];

        foreach (KeyValuePair<string, TranslationFile> pair in translation)
        {
            TranslationUtils.TranslateItemsFromList(nameof(BugReportForm), pair.Value, itemsToTranslate);
        }
    }

    #endregion

    // parity-scaffolding: retains the original reporter test boundary.
    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor
    {
        private readonly BugReportForm _form;

        public TestAccessor(BugReportForm form)
        {
            _form = form;
        }

        public TextBox ExceptionTextBox => _form.exceptionTextBox;
        public TextBox ExceptionMessageTextBox => _form.exceptionMessageTextBox;
        public TextBox TargetSiteTextBox => _form.targetSiteTextBox;
        public ExceptionDetails ExceptionDetails => _form.exceptionDetails;

        public TextBox ApplicationTextBox => _form.applicationTextBox;
        public TextBox GitTextBox => _form.gitTextBox;
        public TextBox DateTimeTextBox => _form.dateTimeTextBox;
        public TextBox ClrTextBox => _form.clrTextBox;

        public static bool CheckContainsInfo(string input) => BugReportForm.CheckContainsInfo(input);
    }
}
