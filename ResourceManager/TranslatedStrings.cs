using GitCommands;
using SmartFormat;
using SmartFormat.Core.Settings;

namespace ResourceManager
{
    /// <summary>Contains common string literals which are translated.</summary>
    public sealed class TranslatedStrings : Translate
    {
        private readonly TranslationString _secondsAgo = new("{0} {1:second|seconds} ago");
        private readonly TranslationString _minutesAgo = new("{0} {1:minute|minutes} ago");
        private readonly TranslationString _hoursAgo = new("{0} {1:hour|hours} ago");
        private readonly TranslationString _daysAgo = new("{0} {1:day|days} ago");
        private readonly TranslationString _weeksAgo = new("{0} {1:week|weeks} ago");
        private readonly TranslationString _monthsAgo = new("{0} {1:month|months} ago");
        private readonly TranslationString _yearsAgo = new("{0} {1:year|years} ago");
        private readonly TranslationString _dateText = new("Date");
        private readonly TranslationString _authorText = new("{0:Author|Authors}");

        private readonly TranslationString _telemetryPermissionCaption = new("Allow Capture Telemetry?");
        private readonly TranslationString _telemetryPermissionMessage = new(@"We collect information so we can make the app better.

We won't collect any personal or identifiable information.
You can change your mind at any time.

Yes, I allow telemetry!");

        private readonly TranslationString _installGitInstructions = new("Install git...");
        private readonly TranslationString _findGitExecutable = new("Find git...");
        private readonly TranslationString _gitExecutableNotFoundText = new("The Git executable could not be located on your system.");
        private readonly TranslationString _authorDateText = new("{0:Author date|Author dates}");
        private readonly TranslationString _committerText = new("Committer");
        private readonly TranslationString _commitDateText = new("{0:Commit date|Commit dates}");
        private readonly TranslationString _commitHashText = new("{0:Commit hash|Commit hashes}");
        private readonly TranslationString _messageText = new("{0:Message|Messages}");
        private readonly TranslationString _showAllText = new("Show all");
        private readonly TranslationString _workspaceText = new("Working directory");
        private readonly TranslationString _indexText = new("Commit index");

        private readonly TranslationString _parentsText = new("{0:Parent|Parents}");
        private readonly TranslationString _childrenText = new("{0:Child|Children}");

        private readonly TranslationString _deleteFile = new("{0:Delete file|Delete files}");

        private readonly TranslationString _generalGitConfigExceptionMessage = new("Failed to read \"{0}\" due to the following error:{1}{1}{2}{1}{1}Due to the nature of this problem, the behavior of the application cannot be guaranteed and it must be closed.{1}{1}Please correct this issue and re-open Git Extensions.");
        private readonly TranslationString _generalGitConfigExceptionCaption = new("Repository Configuration Error");

        private readonly TranslationString _disableMenuItem = new("Disable this dropdown");

        // public only because of FormTranslate
        public TranslatedStrings()
        {
            // Our original implementations were created against SmartFormat.NET pre-dating v2.0.0.
            // Since v2.5.0 the default error action was changed to ThrowError. See https://github.com/axuno/SmartFormat/issues/192.
            // This applies for the Formatter AND the Parser
            Smart.Default.Settings.Formatter.ErrorAction = FormatErrorAction.Ignore;
            Smart.Default.Settings.Parser.ErrorAction = ParseErrorAction.Ignore;

            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        private static Lazy<TranslatedStrings> _instance = new();

        public static void Reinitialize()
        {
            if (_instance.IsValueCreated)
            {
                _instance = new();
            }
        }

        public static string FindGitExecutable => _instance.Value._findGitExecutable.Text;
        public static string InstallGitInstructions => _instance.Value._installGitInstructions.Text;
        public static string GitExecutableNotFound => _instance.Value._gitExecutableNotFoundText.Text;

        public static string TelemetryPermissionCaption => _instance.Value._telemetryPermissionCaption.Text;
        public static string TelemetryPermissionMessage => _instance.Value._telemetryPermissionMessage.Text;

        public static string Date => _instance.Value._dateText.Text;
        public static string Author => GetAuthor(1);
        public static string AuthorDate => GetAuthorDate(1);
        public static string Committer => _instance.Value._committerText.Text;
        public static string CommitDate => GetCommitDate(1);
        public static string CommitHash => GetCommitHash(1);
        public static string ShowAll => _instance.Value._showAllText.Text;
        public static string Workspace => _instance.Value._workspaceText.Text;
        public static string Index => _instance.Value._indexText.Text;

        public static string GeneralGitConfigExceptionMessage => _instance.Value._generalGitConfigExceptionMessage.Text;
        public static string GeneralGitConfigExceptionCaption => _instance.Value._generalGitConfigExceptionCaption.Text;
        public static string DisableMenuItem => _instance.Value._disableMenuItem.Text;

        public static string GetParents(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._parentsText.Text, value, Math.Abs(value));
        }

        public static string GetChildren(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._childrenText.Text, value, Math.Abs(value));
        }

        public static string GetDeleteFile(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._deleteFile.Text, value, Math.Abs(value));
        }

        public static string GetCommitDate(int value)
        {
            string v = Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._commitDateText.Text, value, Math.Abs(value));
            return v;
        }

        public static string GetCommitHash(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._commitHashText.Text, value, Math.Abs(value));
        }

        public static string GetMessage(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._messageText.Text, value, Math.Abs(value));
        }

        public static string GetAuthor(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._authorText.Text, value, Math.Abs(value));
        }

        public static string GetAuthorDate(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._authorDateText.Text, value, Math.Abs(value));
        }

        public static string GetNSecondsAgoText(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._secondsAgo.Text, value, Math.Abs(value));
        }

        public static string GetNMinutesAgoText(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._minutesAgo.Text, value, Math.Abs(value));
        }

        public static string GetNHoursAgoText(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._hoursAgo.Text, value, Math.Abs(value));
        }

        public static string GetNDaysAgoText(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._daysAgo.Text, value, Math.Abs(value));
        }

        public static string GetNWeeksAgoText(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._weeksAgo.Text, value, Math.Abs(value));
        }

        public static string GetNMonthsAgoText(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._monthsAgo.Text, value, Math.Abs(value));
        }

        public static string GetNYearsAgoText(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._yearsAgo.Text, value, Math.Abs(value));
        }
    }
}
