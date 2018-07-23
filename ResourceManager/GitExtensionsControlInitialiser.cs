using System;
using System.ComponentModel;
using GitCommands;
using GitUI;

namespace ResourceManager
{
    internal sealed class GitExtensionsControlInitialiser
    {
        private readonly ITranslate _translate;

        /// <summary>indicates whether the initialisation has been signalled as complete.</summary>
        private bool _initialiseCompleteCalled;

        public GitExtensionsControlInitialiser(GitExtensionsFormBase form)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            form.Font = AppSettings.Font;
            form.Load += delegate
            {
                if (!_initialiseCompleteCalled && !IsDesignModeActive)
                {
                    throw new Exception($"{form.GetType().Name} must call {nameof(InitializeComplete)} in its constructor, ideally as the final statement.");
                }
            };
            _translate = form;
        }

        public GitExtensionsControlInitialiser(GitExtensionsControl control)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            control.Font = AppSettings.Font;
            control.Load += delegate
            {
                if (!_initialiseCompleteCalled && !IsDesignModeActive)
                {
                    throw new Exception($"{control.GetType().Name} must call {nameof(InitializeComplete)} in its constructor, ideally as the final statement.");
                }
            };
            _translate = control;
        }

        /// <summary>Indicates whether code is running as part of an IDE designer, such as the WinForms designer.</summary>
        public bool IsDesignModeActive => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        public void InitializeComplete()
        {
            if (_initialiseCompleteCalled)
            {
                throw new InvalidOperationException($"{nameof(InitializeComplete)} already called.");
            }

            _initialiseCompleteCalled = true;
            Translator.Translate(_translate, AppSettings.CurrentTranslation);
        }
    }
}