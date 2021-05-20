using System;
using GitCommands;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.GitIgnoreDialog
{
    public class GitIgnoreModel : Translate, IGitIgnoreDialogModel
    {
        private readonly TranslationString _editGitignoreTitle =
            new("Edit .gitignore");

        private readonly TranslationString _gitignoreOnlyInWorkingDirSupported =
            new(".gitignore is only supported when there is a working directory.");

        private readonly TranslationString _cannotAccessGitignore =
            new("Failed to save .gitignore." + Environment.NewLine + "Check if file is accessible.");

        private readonly TranslationString _cannotAccessGitignoreCaption =
            new("Failed to save .gitignore");

        private readonly TranslationString _saveFileQuestion =
            new("Save changes to .gitignore?");

        private readonly IFullPathResolver _fullPathResolver;

        public GitIgnoreModel(IGitModule module)
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
            _fullPathResolver = new FullPathResolver(() => module.WorkingDir);
        }

        public string FormCaption => _editGitignoreTitle.Text;

        public string? ExcludeFile => _fullPathResolver.Resolve(".gitignore");

        public string FileOnlyInWorkingDirSupported => _gitignoreOnlyInWorkingDirSupported.Text;

        public string CannotAccessFile => _cannotAccessGitignore.Text;

        public string CannotAccessFileCaption => _cannotAccessGitignoreCaption.Text;

        public string SaveFileQuestion => _saveFileQuestion.Text;
    }
}
