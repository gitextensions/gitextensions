using System;
using GitCommands;
using ResourceManager;

namespace GitUI
{
    internal sealed class Strings : Translate
    {
        private readonly TranslationString _viewPullRequest = new TranslationString("View pull requests");
        private readonly TranslationString _createPullRequest = new TranslationString("Create pull request");
        private readonly TranslationString _forkCloneRepo = new TranslationString("Fork or clone a repository");

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

        public static string CreatePullRequest => _instance.Value._createPullRequest.Text;
        public static string ForkCloneRepo => _instance.Value._forkCloneRepo.Text;
        public static string ViewPullRequest => _instance.Value._viewPullRequest.Text;
    }
}
