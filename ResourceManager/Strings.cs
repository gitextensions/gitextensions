using System;
using GitCommands;
using SmartFormat;

#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row

namespace ResourceManager
{
    /// <summary>Contains common string literals which are translated.</summary>
    public sealed class Strings : Translate
    {
        private readonly TranslationString _secondsAgo = new TranslationString("{0} {1:second|seconds} ago");
        private readonly TranslationString _minutesAgo = new TranslationString("{0} {1:minute|minutes} ago");
        private readonly TranslationString _hoursAgo = new TranslationString("{0} {1:hour|hours} ago");
        private readonly TranslationString _daysAgo = new TranslationString("{0} {1:day|days} ago");
        private readonly TranslationString _weeksAgo = new TranslationString("{0} {1:week|weeks} ago");
        private readonly TranslationString _monthsAgo = new TranslationString("{0} {1:month|months} ago");
        private readonly TranslationString _yearsAgo = new TranslationString("{0} {1:year|years} ago");
        private readonly TranslationString _dateText = new TranslationString("Date");
        private readonly TranslationString _authorText = new TranslationString("{0:Author|Authors}");

        private readonly TranslationString _telemetryPermissionCaption = new TranslationString("Allow Capture Telemetry?");
        private readonly TranslationString _telemetryPermissionMessage = new TranslationString(@"We collect information so we can make the app better.

We won't collect any personal or identifiable information.
You can change your mind at any time.

Yes, I allow telemetry!");

        private readonly TranslationString _installGitInstructions = new TranslationString("Install git...");
        private readonly TranslationString _findGitExecutable = new TranslationString("Find git...");
        private readonly TranslationString _gitExecutableNotFoundText =
            new TranslationString("The Git executable could not be located on your system.");
        private readonly TranslationString _authorDateText = new TranslationString("{0:Author date|Author dates}");
        private readonly TranslationString _committerText = new TranslationString("Committer");
        private readonly TranslationString _commitDateText = new TranslationString("{0:Commit date|Commits dates}");
        private readonly TranslationString _commitHashText = new TranslationString("{0:Commit hash|Commits hashes}");
        private readonly TranslationString _messageText = new TranslationString("{0:Message|Messages}");
        private readonly TranslationString _showAllText = new TranslationString("Show all");
        private readonly TranslationString _workspaceText = new TranslationString("Working directory");
        private readonly TranslationString _indexText = new TranslationString("Commit index");

        private readonly TranslationString _parentsText = new TranslationString("{0:Parent|Parents}");
        private readonly TranslationString _childrenText = new TranslationString("{0:Child|Children}");

        // public only because of FormTranslate
        public Strings()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        private static Lazy<Strings> _instance = new Lazy<Strings>();

        public static void Reinitialize()
        {
            if (_instance.IsValueCreated)
            {
                _instance = new Lazy<Strings>();
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

        public static string GetParents(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._parentsText.Text, value, Math.Abs(value));
        }

        public static string GetChildren(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._childrenText.Text, value, Math.Abs(value));
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
