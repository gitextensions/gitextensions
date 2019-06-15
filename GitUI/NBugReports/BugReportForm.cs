// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Full.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// <copyright file="Full.cs" company="Git Extensions">
//   Copyright (c) 2019 Igor Velikorossov. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using GitExtUtils.GitUI;
using GitUI.NBugReports.Info;
using GitUI.NBugReports.Serialization;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.NBugReports
{
    public partial class BugReportForm : Form
    {
        private readonly TranslationString _title = new TranslationString("Error Report");
        private readonly TranslationString _submitGitHubMessage = new TranslationString(@"Give as much as information as possible please to help the developers solve this issue. Otherwise, your issue ticket may be closed without any follow-up from the developers.

Because of this, make sure to fill in all the fields in the report template please.

Send report anyway?");

        private static readonly IErrorReportMarkDownBodyBuilder ErrorReportBodyBuilder;
        private static readonly GitHubUrlBuilder UrlBuilder;
        private SerializableException _lastException;
        private Report _lastReport;
        private string _environmentInfo;

        static BugReportForm()
        {
            ErrorReportBodyBuilder = new ErrorReportMarkDownBodyBuilder();
            UrlBuilder = new GitHubUrlBuilder(ErrorReportBodyBuilder);
        }

        public BugReportForm()
        {
            InitializeComponent();
            Icon = Resources.GitExtensionsLogoIcon;

            // ToDo: Displaying report contents properly requires some more work.
            mainTabs.TabPages.Remove(mainTabs.TabPages["reportContentsTabPage"]);
        }

        public DialogResult ShowDialog(Exception exception, string environmentInfo)
        {
            _lastException = new SerializableException(exception);
            _lastReport = new Report(_lastException);
            _environmentInfo = environmentInfo;

            Text = $@"{_lastReport.GeneralInfo.HostApplication} {_title.Text}";

            // Scaling
            sendAndQuitButton.Image = DpiUtil.Scale(Images.GitHub);
            btnCopy.Image = DpiUtil.Scale(Resources.CopyToClipboard);
            exceptionTypeLabel.Image = DpiUtil.Scale(Resources.bug);
            exceptionDetails.InformationColumnWidth = DpiUtil.Scale(350);
            exceptionDetails.PropertyColumnWidth = DpiUtil.Scale(101);

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

            DialogResult = DialogResult.None;

            // ToDo: Fill in the 'Report Contents' tab);
            var result = ShowDialog();

            // Write back the user description (as we passed 'report' as a reference since it is a reference object anyway)
            _lastReport.GeneralInfo.UserDescription = descriptionTextBox.Text;

            return result;
        }

        private void QuitButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
            Close();
        }

        private void SendAndQuitButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, _submitGitHubMessage.Text, _title.Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
            {
                return;
            }

            string url = UrlBuilder.Build("https://github.com/gitextensions/gitextensions/issues/new", _lastException.OriginalException, _environmentInfo, descriptionTextBox.Text);
            Process.Start(url);

            DialogResult = DialogResult.Abort;
            Close();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            var report = ErrorReportBodyBuilder.Build(_lastException.OriginalException, _environmentInfo, descriptionTextBox.Text);
            if (string.IsNullOrWhiteSpace(report))
            {
                return;
            }

            Clipboard.SetDataObject(report, true, 5, 100);

            DialogResult = DialogResult.None;
        }
    }
}