﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Full.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// <copyright file="BugReportForm.cs" company="Git Extensions">
//   Copyright (c) 2019 Igor Velikorossov. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Text.RegularExpressions;
using BugReporter.Properties;
using BugReporter.Serialization;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI;
using Microsoft;
using ResourceManager;
using Report = BugReporter.Info.Report;

namespace BugReporter
{
    public partial class BugReportForm : Form, ITranslate
    {
        private readonly TranslationString _title = new("Error Report");
        private readonly TranslationString _submitGitHubMessage = new(@"Give as much as information as possible please to help the developers solve this issue. Otherwise, your issue ticket may be closed without any follow-up from the developers.

Because of this, make sure to fill in all the fields in the report template please.

Send report anyway?");
        private readonly TranslationString _toolTipCopy = new("Copy the issue details into clipboard");
        private readonly TranslationString _toolTipSendQuit = new("Report the issue to GitHub and quit application.\r\nA valid GitHub account is required");
        private readonly TranslationString _toolTipQuit = new("Quit application without reporting the issue");
        private readonly TranslationString _noReproStepsSuppliedErrorMessage = new(@"Please provide as much as information as possible to help the developers solve this issue.");

        private static readonly IErrorReportUrlBuilder ErrorReportBodyBuilder;
        private static readonly GitHubUrlBuilder UrlBuilder;
        private SerializableException? _lastException;
        private Report? _lastReport;
        private string _exceptionInfo;
        private string? _environmentInfo;

        static BugReportForm()
        {
            ErrorReportBodyBuilder = new ErrorReportUrlBuilder();
            UrlBuilder = new GitHubUrlBuilder(ErrorReportBodyBuilder);
        }

        public BugReportForm()
        {
            InitializeComponent();

            Icon = Resources.GitExtensionsLogoIcon;

            // Scaling
            exceptionTypeLabel.Image = DpiUtil.Scale(exceptionTypeLabel.Image);
            exceptionDetails.PropertyColumnWidth = DpiUtil.Scale(101);
            exceptionDetails.PropertyColumnWidth = DpiUtil.Scale(101);
            DpiUtil.Scale(sendAndQuitButton.MinimumSize);
            DpiUtil.Scale(btnCopy.MinimumSize);

            warningLabel.MaximumSize = new Size(warningLabel.Width, 0);
            warningLabel.AutoSize = true;

            AutoScaleMode = AppSettings.EnableAutoScale
                ? AutoScaleMode.Dpi
                : AutoScaleMode.None;

            this.AdjustForDpiScaling();
            this.EnableRemoveWordHotkey();

            toolTip.SetToolTip(btnCopy, _toolTipCopy.Text);
            toolTip.SetToolTip(sendAndQuitButton, _toolTipSendQuit.Text);
            toolTip.SetToolTip(quitButton, _toolTipQuit.Text);

            // ToDo: Displaying report contents properly requires some more work.
            mainTabs.TabPages.Remove(mainTabs.TabPages["reportContentsTabPage"]);
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
            warningPictureBox.Image = SystemIcons.Warning.ToBitmap();
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
                mainTabs.SelectedTab = exceptionTabPage;
            }

            ControlBox = canIgnore;
            showIgnore &= canIgnore;
            IgnoreButton.Visible = showIgnore;
            IgnoreButton.Visible = showIgnore;

            DialogResult = DialogResult.None;

            // ToDo: Fill in the 'Report Contents' tab);
            var result = ShowDialog(owner);

            // Write back the user description (as we passed 'report' as a reference since it is a reference object anyway)
            _lastReport.GeneralInfo.UserDescription = descriptionTextBox.Text;

            return result;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            descriptionTextBox.Focus();
        }

        private static bool CheckContainsInfo(string input)
        {
            var text = Regex.Replace(input, @"\s*|\r|\n", string.Empty);
            return !string.IsNullOrWhiteSpace(text);
        }

        private void QuitButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
            Close();
        }

        private void SendAndQuitButton_Click(object sender, EventArgs e)
        {
            var hasUserText = CheckContainsInfo(descriptionTextBox.Text);
            if (!hasUserText)
            {
                MessageBox.Show(this, _noReproStepsSuppliedErrorMessage.Text, _title.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                descriptionTextBox.Focus();
                return;
            }

            if (MessageBox.Show(this, _submitGitHubMessage.Text, _title.Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
            {
                return;
            }

            Validates.NotNull(_lastException);

            string? url = UrlBuilder.Build("https://github.com/gitextensions/gitextensions/issues/new", _lastException, _exceptionInfo, _environmentInfo, descriptionTextBox.Text);
            new Executable(url!).Start(useShellExecute: true, throwOnErrorExit: false);

            DialogResult = DialogResult.Abort;
            Close();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Validates.NotNull(_lastException);

            var report = ErrorReportBodyBuilder.CopyText(_lastException, _exceptionInfo, _environmentInfo, descriptionTextBox.Text);
            if (string.IsNullOrWhiteSpace(report))
            {
                return;
            }

            Clipboard.SetDataObject(report, true, 5, 100);

            DialogResult = DialogResult.None;
        }

        private void IgnoreButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Ignore;
            Close();
        }

        #region Translation

        public virtual void AddTranslationItems(ITranslation translation)
        {
            TranslationUtils.AddTranslationItemsFromFields(Name, this, translation);
        }

        public virtual void TranslateItems(ITranslation translation)
        {
            TranslationUtils.TranslateItemsFromFields(Name, this, translation);
        }

        protected void TranslateItem(string itemName, object item)
        {
            var translation = Translator.GetTranslation(AppSettings.CurrentTranslation);

            if (translation.Count == 0)
            {
                return;
            }

            var itemsToTranslate = new[] { (itemName, item) };

            foreach (var pair in translation)
            {
                TranslationUtils.TranslateItemsFromList(Name, pair.Value, itemsToTranslate);
            }
        }

        #endregion

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
}
