using System;
using System.ComponentModel;
using System.Windows.Forms;
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
            form.Load += LoadHandler;
            _translate = form;
        }

        public GitExtensionsControlInitialiser(GitExtensionsControl control)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            control.Load += LoadHandler;
            _translate = control;
        }

        private void LoadHandler(object control, EventArgs e)
        {
            if (!_initialiseCompleteCalled && !IsDesignModeActive)
            {
                throw new Exception($"{control.GetType().Name} must call {nameof(InitializeComplete)} in its constructor, ideally as the final statement.");
            }
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

            ((Control)_translate).Font = AppSettings.Font;
            Translator.Translate(_translate, AppSettings.CurrentTranslation);
        }
    }
}