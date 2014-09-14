namespace NBug.Core.Submission.Tracker.DoctorDump
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using NBug.Core.Reporting.Info;
    using NBug.Core.Util.Logging;
    using NBug.Core.Util.Serialization;

    public class DoctorDumpFactory : IProtocolFactory
    {
        public string SupportedType
        {
            get
            {
                return "DoctorDump";
            }
        }

        public IProtocol FromConnectionString(string connectionString)
        {
            return new DoctorDump(connectionString);
        }
    }

    public class DoctorDump : ProtocolBase
    {
        public string ApplicationGUID { get; set; }

        public string Email { get; set; }

        public bool SendAnonymousReportSilently { get { return _sendAnonymousReportSilently; } set { _sendAnonymousReportSilently = value; } }

        public bool OpenProblemSolutionPage { get { return _openProblemSolutionPage; } set { _openProblemSolutionPage = value; } }

        private bool _sendAnonymousReportSilently = true;
        private bool _openProblemSolutionPage = true;
        private DoctorDumpService _uploader = new DoctorDumpService();
        private System.Threading.Tasks.Task<NBug.DoctorDump.Response> _sendAnonymousReportResult;

        public DoctorDump(string connectionString)
            : base(connectionString)
        {
            if (SendAnonymousReportSilently)
            {
                NBug.Core.Reporting.BugReport.PreDisplayBugReportUI += OnPreDisplayBugReportUI;
                NBug.Core.Reporting.BugReport.PostDisplayBugReportUI += OnPostDisplayBugReportUI;
            }
        }

        public DoctorDump()
        {
        }

        private void OnPreDisplayBugReportUI(Exception exc, SerializableException exception, Report report)
        {
            _sendAnonymousReportResult = new System.Threading.Tasks.Task<NBug.DoctorDump.Response>(() => _uploader.SendAnonymousReport(ApplicationGUID, Email, exception, report));
            _sendAnonymousReportResult.Start();
        }

        private void OnPostDisplayBugReportUI(NBug.Core.UI.UIDialogResult uiResult, Exception exc, SerializableException exception, Report report)
        {
            try
            {
                // We need to wait till end of SendAnonymousReport or report may be dropped due to application exit
                _sendAnonymousReportResult.Wait();

                // No interaction with user if he press Cancel
                if (uiResult.Report != UI.SendReport.Send)
                    return;

                byte[] context = GetContinueContext(_sendAnonymousReportResult.Result);
                if (context == null)
                    uiResult.Report = UI.SendReport.DoNotSend;
                else
                    report.ProtocolData = context;
            }
            catch (Exception ex)
            {
                // No interaction with user if he press Cancel
                if (uiResult.Report != UI.SendReport.Send)
                    return;
                throw new Exception("Failed to send report to Doctor Dump", ex);
            }
        }

        private byte[] GetContinueContext(NBug.DoctorDump.Response response)
        {
            if (response is NBug.DoctorDump.ErrorResponse)
            {
                string error = ((NBug.DoctorDump.ErrorResponse)response).Error;
                Logger.Error(string.Format("Failed to send anonymous report, Doctor Dump: {0}", error));
                return null;
            }

            if (OpenProblemSolutionPage && !string.IsNullOrEmpty(response.UrlToProblem))
                System.Diagnostics.Process.Start(response.UrlToProblem);

            if (!(response is NBug.DoctorDump.NeedReportResponse))
            {
                // We already have enough reports with this problem
                return null;
            }

            // We need context to continue report upload in next session
            return response.Context;
        }

        public override bool Send(string fileName, System.IO.Stream file, Report report, SerializableException exception)
        {
            var context = report.ProtocolData as byte[] ?? GetContinueContext(_uploader.SendAnonymousReport(ApplicationGUID, Email, exception, report));
            if (context == null)
                return true;

            file.Position = 0;
            _uploader.SendRequest(context, ApplicationGUID, Email, file, exception, report);

            return true;
        }
    }
}
