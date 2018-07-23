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

        public static void Reinitialize()
        {
            if (_instance.IsValueCreated)
            {
                _instance = new Lazy<Strings>();
            }
        }

        public static string Date => _instance.Value._dateText.Text;
        public static string Author => _instance.Value._authorText.Text;
        public static string AuthorDate => _instance.Value._authorDateText.Text;
        public static string Committer => _instance.Value._committerText.Text;
        public static string CommitDate => _instance.Value._commitDateText.Text;
        public static string CommitHash => _instance.Value._commitHashText.Text;
        public static string Message => _instance.Value._messageText.Text;
        public static string Workspace => _instance.Value._workspaceText.Text;
        public static string Index => _instance.Value._indexText.Text;
        public static string LoadingData => _instance.Value._loadingDataText.Text;
        public static string UninterestingDiffOmitted => _instance.Value._uninterestingDiffOmitted.Text;

        public static string Branches => _instance.Value._branchesText.Text;
        public static string Remotes => _instance.Value._remotesText.Text;
        public static string Tags => _instance.Value._tagsText.Text;

        private readonly TranslationString _dateText       = new TranslationString("Date");
        private readonly TranslationString _authorText     = new TranslationString("Author");
        private readonly TranslationString _authorDateText = new TranslationString("Author date");
        private readonly TranslationString _committerText  = new TranslationString("Committer");
        private readonly TranslationString _commitDateText = new TranslationString("Commit date");
        private readonly TranslationString _commitHashText = new TranslationString("Commit hash");
        private readonly TranslationString _messageText    = new TranslationString("Message");
        private readonly TranslationString _workspaceText  = new TranslationString("Working directory");
        private readonly TranslationString _indexText      = new TranslationString("Commit index");
        private readonly TranslationString _loadingDataText = new TranslationString("Loading data...");
        private readonly TranslationString _uninterestingDiffOmitted = new TranslationString("Uninteresting diff hunks are omitted.");
        private readonly TranslationString _branchesText   = new TranslationString("Branches");
        private readonly TranslationString _remotesText    = new TranslationString("Remotes");
        private readonly TranslationString _tagsText       = new TranslationString("Tags");

        public static string GetParents(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._parentsText.Text, value, Math.Abs(value));
        }

        public static string GetChildren(int value)
        {
            return Smart.Format(AppSettings.CurrentCultureInfo, _instance.Value._childrenText.Text, value, Math.Abs(value));
        }

        private readonly TranslationString _parentsText    = new TranslationString("{0:Parent|Parents}");
        private readonly TranslationString _childrenText   = new TranslationString("{0:Child|Children}");

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

        private readonly TranslationString _secondsAgo = new TranslationString("{0} {1:second|seconds} ago");
        private readonly TranslationString _minutesAgo = new TranslationString("{0} {1:minute|minutes} ago");
        private readonly TranslationString _hoursAgo   = new TranslationString("{0} {1:hour|hours} ago");
        private readonly TranslationString _daysAgo    = new TranslationString("{0} {1:day|days} ago");
        private readonly TranslationString _weeksAgo   = new TranslationString("{0} {1:week|weeks} ago");
        private readonly TranslationString _monthsAgo  = new TranslationString("{0} {1:month|months} ago");
        private readonly TranslationString _yearsAgo   = new TranslationString("{0} {1:year|years} ago");
    }
}
