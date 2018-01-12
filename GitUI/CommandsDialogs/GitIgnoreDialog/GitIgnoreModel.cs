using System;
using System.IO;
using GitCommands;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.GitIgnoreDialog
{
    public class GitIgnoreModel : Translate, IGitIgnoreDialogModel
    {
        private readonly TranslationString _editGitignoreTitle =
            new TranslationString("Edit .gitignore");

        private readonly TranslationString _gitignoreOnlyInWorkingDirSupported =
            new TranslationString(".gitignore is only supported when there is a working directory.");

        private readonly TranslationString _cannotAccessGitignore =
            new TranslationString("Failed to save .gitignore." + Environment.NewLine + "Check if file is accessible.");

        private readonly TranslationString _cannotAccessGitignoreCaption =
            new TranslationString("Failed to save .gitignore");

        private readonly TranslationString _saveFileQuestion =
            new TranslationString("Save changes to .gitignore?");

        private readonly IGitModule _module;
        private readonly IFullPathResolver _fullPathResolver;

        public GitIgnoreModel(IGitModule module)
        {
            _module = module;

            Translator.Translate(this, AppSettings.CurrentTranslation);
            _fullPathResolver = new FullPathResolver(() => _module.WorkingDir);
        }

        public string FormCaption
        {
            get { return _editGitignoreTitle.Text; }
        }

        public string ExcludeFile
        {
            get { return _fullPathResolver.Resolve(".gitignore"); }
        }

        public string FileOnlyInWorkingDirSupported
        {
            get { return _gitignoreOnlyInWorkingDirSupported.Text; }
        }

        public string CannotAccessFile
        {
            get { return _cannotAccessGitignore.Text; }
        }

        public string CannotAccessFileCaption
        {
            get { return _cannotAccessGitignoreCaption.Text; }
        }

        public string SaveFileQuestion
        {
            get { return _saveFileQuestion.Text; }
        }
    }
}