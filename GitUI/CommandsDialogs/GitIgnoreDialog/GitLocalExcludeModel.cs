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
            new TranslationString("Edit .git/info/exclude");

        private readonly TranslationString _localExcludeOnlyInWorkingDirSupported =
            new TranslationString(".git/info/exclude is only supported when there is a working directory.");

        private readonly TranslationString _cannotAccessLocalExclude =
            new TranslationString("Failed to save .git/info/exclude." + Environment.NewLine +
                                  "Check if file is accessible.");

        private readonly TranslationString _cannotAccessLocalExcludeCaption =
            new TranslationString("Failed to save .git/info/exclude");

        private readonly TranslationString _saveFileQuestion =
            new TranslationString("Save changes to .git/info/exclude?");

        private readonly IGitModule _module;

        public GitLocalExcludeModel(IGitModule module)
        {
            _module = module;

            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        public string FormCaption
        {
            get { return _editLocalExcludeTitle.Text; }
        }

        public string ExcludeFile
        {
            get { return Path.Combine(_module.ResolveGitInternalPath("info"), "exclude"); }
        }

        public string FileOnlyInWorkingDirSupported
        {
            get { return _localExcludeOnlyInWorkingDirSupported.Text; }
        }

        public string CannotAccessFile
        {
            get { return _cannotAccessLocalExclude.Text; }
        }

        public string CannotAccessFileCaption
        {
            get { return _cannotAccessLocalExcludeCaption.Text; }
        }

        public string SaveFileQuestion
        {
            get { return _saveFileQuestion.Text; }
        }
    }
}