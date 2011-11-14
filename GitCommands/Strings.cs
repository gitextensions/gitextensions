using ResourceManager.Translation;
using ResourceManager;

namespace GitCommands
{
    public class Strings : ITranslate
    {
        // public only because of FormTranslate
        public Strings()
        {
            var translator = new Translator(Settings.Translation);
            translator.TranslateControl(this);
        }

        private static Strings instance;

        private static Strings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Strings();
                }
                return instance;
            }
        }

        public static void Reinit()
        {
            if (instance != null)
            {
                instance = new Strings();
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

        private readonly TranslationString _dateText       = new TranslationString("Date");
        private readonly TranslationString _authorText     = new TranslationString("Author");
        private readonly TranslationString _authorDateText = new TranslationString("Author date");
        private readonly TranslationString _committerText  = new TranslationString("Committer");
        private readonly TranslationString _commitDateText = new TranslationString("Commit date");
        private readonly TranslationString _commitHashText = new TranslationString("Commit hash");

        public static string Get1SecondAgoText()
        {
            return Instance._secondAgo.Text;
        }

        public static string GetNSecondsAgoText()
        {
            return Instance._secondsAgo.Text;
        }

        public static string Get1MinuteAgoText()
        {
            return Instance._minuteAgo.Text;
        }

        public static string GetNMinutesAgoText()
        {
            return Instance._minutesAgo.Text;
        }

        public static string Get1HourAgoText()
        {
            return Instance._hourAgo.Text;
        }

        public static string GetNHoursAgoText()
        {
            return Instance._hoursAgo.Text;
        }

        public static string Get1DayAgoText()
        {
            return Instance._dayAgo.Text;
        }

        public static string GetNDaysAgoText()
        {
            return Instance._daysAgo.Text;
        }

        public static string Get1WeekAgoText()
        {
            return Instance._weekAgo.Text;
        }

        public static string GetNWeeksAgoText()
        {
            return Instance._weeksAgo.Text;
        }

        public static string Get1MonthAgoText()
        {
            return Instance._monthAgo.Text;
        }

        public static string GetNMonthsAgoText()
        {
            return Instance._monthsAgo.Text;
        }

        public static string Get1YearAgoText()
        {
            return Instance._yearAgo.Text;
        }

        public static string GetNYearsAgoText()
        {
            return Instance._yearsAgo.Text;
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
