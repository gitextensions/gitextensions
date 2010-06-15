using System;
using System.Collections.Generic;
using System.Text;
using ResourceManager.Translation;
using ResourceManager;

namespace GitCommands
{
    public class Strings : ITranslate
    {
        public Strings()
        {
            Translator translator = new Translator(Settings.Translation);
            translator.TranslateControl(this);
        }

        public static string GetDateText()
        {
            return new Strings().dateText.Text;
        }

        public static string GetAutorText()
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

        private TranslationString dateText = new TranslationString("Date");
        private TranslationString authorText = new TranslationString("Author");
        private TranslationString authorDateText = new TranslationString("Author date");
        private TranslationString committerText = new TranslationString("Committer");
        private TranslationString committerDateText = new TranslationString("Commit date");
        private TranslationString commitHashText = new TranslationString("Commit hash");
    }
}
