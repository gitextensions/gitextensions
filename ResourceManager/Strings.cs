using System;
using GitCommands;
using SmartFormat;

#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row

namespace ResourceManager
{
    /// <summary>Contains common string literals which are translated.</summary>
    public sealed class Strings : Translate
    {
        // public only because of FormTranslate
        public Strings()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        private static Lazy<Strings> _instance = new Lazy<Strings>();

        private static Strings Instance => _instance.Value;

        public static void Reinit()
        {
            if (_instance.IsValueCreated)
            {
                _instance = new Lazy<Strings>();
            }
        }

        public static string GetDateText()
        {
            return Instance._dateText.Text;
        }

        public static string GetAuthorText()
        {
            return Instance._authorText.Text;
        }

        public static string GetAuthorDateText()
        {
            return Instance._authorDateText.Text;
        }

        public static string GetCommitterText()
        {
            return Instance._committerText.Text;
        }

        public static string GetCommitDateText()
        {
            return Instance._commitDateText.Text;
        }

        public static string GetCommitHashText()
        {
            return Instance._commitHashText.Text;
        }

        public static string GetMessageText()
        {
            return Instance._messageText.Text;
        }

        public static string GetParentsText()
        {
            return Instance._parentsText.Text;
        }

        public static string GetChildrenText()
        {
            return Instance._childrenText.Text;
        }

        public static string GetWorkspaceText()
        {
            return Instance._workspaceText.Text;
        }

        public static string GetIndexText()
        {
            return Instance._indexText.Text;
        }

        public static string GetLoadingDataText()
        {
            return Instance._loadingDataText.Text;
        }

        public static readonly TranslationString BranchesText = new TranslationString("Branches");
        public static readonly TranslationString RemotesText = new TranslationString("Remotes");
        public static readonly TranslationString TagsText = new TranslationString("Tags");

        public static string GetUninterestingDiffOmitted()
        {
            return Instance._uninterestingDiffOmitted.Text;
        }

        private readonly TranslationString _dateText       = new TranslationString("Date");
        private readonly TranslationString _authorText     = new TranslationString("Author");
        private readonly TranslationString _authorDateText = new TranslationString("Author date");
        private readonly TranslationString _committerText  = new TranslationString("Committer");
        private readonly TranslationString _commitDateText = new TranslationString("Commit date");
        private readonly TranslationString _commitHashText = new TranslationString("Commit hash");
        private readonly TranslationString _messageText    = new TranslationString("Message");
        private readonly TranslationString _parentsText    = new TranslationString("Parent(s)");
        private readonly TranslationString _childrenText   = new TranslationString("Children");
        private readonly TranslationString _workspaceText  = new TranslationString("Workspace");
        private readonly TranslationString _indexText      = new TranslationString("Index");

        private readonly TranslationString _loadingDataText
            = new TranslationString("Loading data...");

        private readonly TranslationString _uninterestingDiffOmitted
            = new TranslationString("Uninteresting diff hunks are omitted.");

        public static string GetNSecondsAgoText(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, Instance._secondsAgo.Text, value, Math.Abs(value));
        }

        public static string GetNMinutesAgoText(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, Instance._minutesAgo.Text, value, Math.Abs(value));
        }

        public static string GetNHoursAgoText(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, Instance._hoursAgo.Text, value, Math.Abs(value));
        }

        public static string GetNDaysAgoText(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, Instance._daysAgo.Text, value, Math.Abs(value));
        }

        public static string GetNWeeksAgoText(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, Instance._weeksAgo.Text, value, Math.Abs(value));
        }

        public static string GetNMonthsAgoText(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, Instance._monthsAgo.Text, value, Math.Abs(value));
        }

        public static string GetNYearsAgoText(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, Instance._yearsAgo.Text, value, Math.Abs(value));
        }

        public static string GetUnstagedCountText(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, Instance._unstagedCountText.Text, value, Math.Abs(value));
        }

        public static string GetStagedCountText(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, Instance._stagedCountText.Text, value, Math.Abs(value));
        }

        private readonly TranslationString _secondsAgo = new TranslationString("{0} {1:second|seconds} ago");
        private readonly TranslationString _minutesAgo = new TranslationString("{0} {1:minute|minutes} ago");
        private readonly TranslationString _hoursAgo   = new TranslationString("{0} {1:hour|hours} ago");
        private readonly TranslationString _daysAgo    = new TranslationString("{0} {1:day|days} ago");
        private readonly TranslationString _weeksAgo   = new TranslationString("{0} {1:week|weeks} ago");
        private readonly TranslationString _monthsAgo  = new TranslationString("{0} {1:month|months} ago");
        private readonly TranslationString _yearsAgo   = new TranslationString("{0} {1:year|years} ago");

        private readonly TranslationString _unstagedCountText = new TranslationString("{0} {1:file|files} with unstaged changes");
        private readonly TranslationString _stagedCountText   = new TranslationString("{0} {1:file|files} with staged changes");
    }
}
