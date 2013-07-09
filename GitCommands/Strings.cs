using System;
using ResourceManager.Translation;

namespace GitCommands
{
    public class Strings : Translate
    {
        // public only because of FormTranslate
        public Strings()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        private static Strings _instance;

        private static Strings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Strings();
                }
                return _instance;
            }
        }

        public static void Reinit()
        {
            if (_instance != null)
            {
                _instance = new Strings();
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

        public static string GetCurrentUnstagedChanges()
        {
            return Instance._currentUnstagedChanges.Text;
        }

        public static string GetCurrentIndex()
        {
            return Instance._currentIndex.Text;
        }

        public static string GetLoadingData()
        {
            return Instance._LoadingData.Text;
        }

        private readonly TranslationString _dateText       = new TranslationString("Date");
        private readonly TranslationString _authorText     = new TranslationString("Author");
        private readonly TranslationString _authorDateText = new TranslationString("Author date");
        private readonly TranslationString _committerText  = new TranslationString("Committer");
        private readonly TranslationString _commitDateText = new TranslationString("Commit date");
        private readonly TranslationString _commitHashText = new TranslationString("Commit hash");
        private readonly TranslationString _messageText    = new TranslationString("Message");
        private readonly TranslationString _parentsText    = new TranslationString("Parent(s)");
        private readonly TranslationString _childrenText = new TranslationString("Children");
        private readonly TranslationString _currentUnstagedChanges = new TranslationString("Current unstaged changes");
        private readonly TranslationString _currentIndex = new TranslationString("Commit index");
        private readonly TranslationString _LoadingData = new TranslationString("Loading data...");


        public static string GetNSecondsAgoText(int value)
        {
            if (Math.Abs(value) == 1)
                return string.Format(Instance._secondAgo.Text, value.ToString());
            return string.Format(Instance._secondsAgo.Text, value.ToString());
        }

        public static string GetNMinutesAgoText(int value)
        {
            if (Math.Abs(value) == 1)
                return string.Format(Instance._minuteAgo.Text, value.ToString());
            return string.Format(Instance._minutesAgo.Text, value.ToString());
        }
        public static string GetNHoursAgoText(int value)
        {
            if (Math.Abs(value) == 1)
                return string.Format(Instance._hourAgo.Text, value.ToString());
            return string.Format(Instance._hoursAgo.Text, value.ToString());
        }

        public static string GetNDaysAgoText(int value)
        {
            if (Math.Abs(value) == 1)
                return string.Format(Instance._dayAgo.Text, value.ToString());
            return string.Format(Instance._daysAgo.Text, value.ToString());
        }

        public static string GetNWeeksAgoText(int value)
        {
            if (Math.Abs(value) == 1)
                return string.Format(Instance._weekAgo.Text, value.ToString());
            return string.Format(Instance._weeksAgo.Text, value.ToString());
        }

        public static string GetNMonthsAgoText(int value)
        {
            if (Math.Abs(value) == 1)
                return string.Format(Instance._monthAgo.Text, value.ToString());
            return string.Format(Instance._monthsAgo.Text, value.ToString());
        }

        public static string GetNYearsAgoText(int value)
        {
            if (Math.Abs(value) == 1)
                return string.Format(Instance._yearAgo.Text, value.ToString());
            return string.Format(Instance._yearsAgo.Text, value.ToString());
        }
        
        private readonly TranslationString _secondAgo    = new TranslationString("{0} second ago");
        private readonly TranslationString _secondsAgo   = new TranslationString("{0} seconds ago");
        private readonly TranslationString _minuteAgo    = new TranslationString("{0} minute ago");
        private readonly TranslationString _minutesAgo   = new TranslationString("{0} minutes ago");
        private readonly TranslationString _hourAgo      = new TranslationString("{0} hour ago");
        private readonly TranslationString _hoursAgo     = new TranslationString("{0} hours ago");
        private readonly TranslationString _dayAgo       = new TranslationString("{0} day ago");
        private readonly TranslationString _daysAgo      = new TranslationString("{0} days ago");
        private readonly TranslationString _weekAgo      = new TranslationString("{0} week ago");
        private readonly TranslationString _weeksAgo     = new TranslationString("{0} weeks ago");
        private readonly TranslationString _monthAgo     = new TranslationString("{0} month ago");
        private readonly TranslationString _monthsAgo    = new TranslationString("{0} months ago");
        private readonly TranslationString _yearAgo      = new TranslationString("{0} year ago");
        private readonly TranslationString _yearsAgo     = new TranslationString("{0} years ago");
    }
}
