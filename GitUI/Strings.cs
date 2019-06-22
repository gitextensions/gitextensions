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
        private readonly TranslationString _branchText = new TranslationString("Branch");
        private readonly TranslationString _branchesText = new TranslationString("Branches");
        private readonly TranslationString _remotesText = new TranslationString("Remotes");
        private readonly TranslationString _tagsText = new TranslationString("Tags");
        private readonly TranslationString _submodulesText = new TranslationString("Submodules");
        private readonly TranslationString _bodyNotLoaded = new TranslationString("\n\nFull message text is not present in older commits.\nSelect this commit to populate the full message.");
        private readonly TranslationString _searchingFor = new TranslationString("Searching for: ");
        private readonly TranslationString _loadingDataText = new TranslationString("Loading data...");
        private readonly TranslationString _uninterestingDiffOmitted = new TranslationString("Uninteresting diff hunks are omitted.");

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

        public static string Branch => _instance.Value._branchText.Text;
        public static string Branches => _instance.Value._branchesText.Text;
        public static string Remotes => _instance.Value._remotesText.Text;
        public static string Tags => _instance.Value._tagsText.Text;
        public static string Submodules => _instance.Value._submodulesText.Text;

        public static string BodyNotLoaded => _instance.Value._bodyNotLoaded.Text;
        public static string SearchingFor => _instance.Value._searchingFor.Text;

        public static string LoadingData => _instance.Value._loadingDataText.Text;
        public static string UninterestingDiffOmitted => _instance.Value._uninterestingDiffOmitted.Text;
    }
}
