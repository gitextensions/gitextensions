using System;
using System.IO;
using GitCommands;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.GitIgnoreDialog
{
    public class GitLocalExcludeModel : Translate, IGitIgnoreDialogModel
    {
        private readonly TranslationString _editLocalExcludeTitle =
            new("Edit .git/info/exclude");

        private readonly TranslationString _localExcludeOnlyInWorkingDirSupported =
            new(".git/info/exclude is only supported when there is a working directory.");

        private readonly TranslationString _cannotAccessLocalExclude =
            new("Failed to save .git/info/exclude." + Environment.NewLine +
                                  "Check if file is accessible.");

        private readonly TranslationString _cannotAccessLocalExcludeCaption =
            new("Failed to save .git/info/exclude");

        private readonly TranslationString _saveFileQuestion =
            new("Save changes to .git/info/exclude?");

        private readonly IGitModule _module;

        public GitLocalExcludeModel(IGitModule module)
        {
            _module = module;

            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        public string FormCaption => _editLocalExcludeTitle.Text;

        public string ExcludeFile => Path.Combine(_module.ResolveGitInternalPath("info"), "exclude");

        public string FileOnlyInWorkingDirSupported => _localExcludeOnlyInWorkingDirSupported.Text;

        public string CannotAccessFile => _cannotAccessLocalExclude.Text;

        public string CannotAccessFileCaption => _cannotAccessLocalExcludeCaption.Text;

        public string SaveFileQuestion => _saveFileQuestion.Text;
    }
}