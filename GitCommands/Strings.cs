using ResourceManager.Translation;
using ResourceManager;

namespace GitCommands
{
    public class Strings : ITranslate
    {
        public Strings()
        {
            var translator = new Translator(Settings.Translation);
            translator.TranslateControl(this);
        }

        public static string GetDateText()
        {
            return new Strings().dateText.Text;
        }

        public static string GetAuthorText()
        {
            return new Strings().authorText.Text;
        }

        public static string GetAuthorDateText()
        {
            return new Strings().authorDateText.Text;
        }

        public static string GetCommitterText()
        {
            return new Strings().committerText.Text;
        }

        public static string GetCommitterDateText()
        {
            return new Strings().committerDateText.Text;
        }

        public static string GetCommitHashText()
        {
            return new Strings().commitHashText.Text;
        }

        private readonly TranslationString dateText = new TranslationString("Date");
        private readonly TranslationString authorText = new TranslationString("Author");
        private readonly TranslationString authorDateText = new TranslationString("Author date");
        private readonly TranslationString committerText = new TranslationString("Committer");
        private readonly TranslationString committerDateText = new TranslationString("Commit date");
        private readonly TranslationString commitHashText = new TranslationString("Commit hash");
    }
}
