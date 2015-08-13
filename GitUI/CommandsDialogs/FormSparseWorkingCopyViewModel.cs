using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

using GitCommands;

using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    public class FormSparseWorkingCopyViewModel : INotifyPropertyChanged
    {
        public static readonly string RefreshWorkingCopyCommandName = "read-tree -m -u HEAD";

        public static readonly string SettingCoreSparseCheckout = "core.sparseCheckout";

        private readonly GitUICommands _gitcommands;

        private bool _isRefreshWorkingCopyOnSave = true /* on by default, otherwise index bitmap won't be updated */;

        private bool _isSparseCheckoutEnabled;

        /// <summary>
        /// Remembers what were in settings, to check <see cref="IsSparseCheckoutEnabled" /> against to tell if modified.
        /// </summary>
        private bool _isSparseCheckoutEnabledAsSaved;

        [CanBeNull]
        private string _sRulesText;

        /// <summary>
        /// Remembers what were loaded from disk, to check <see cref="RulesText" /> against to tell if modified.
        /// NULL until loaded.
        /// </summary>
        [CanBeNull]
        private string _sRulesTextAsOnDisk;

        public FormSparseWorkingCopyViewModel([NotNull] GitUICommands gitcommands)
        {
            _gitcommands = gitcommands;
            if(gitcommands == null)
                throw new ArgumentNullException("gitcommands");
            _isSparseCheckoutEnabled = _isSparseCheckoutEnabledAsSaved = GetCurrentSparseEnabledState();
        }

        /// <summary>
        /// Whether to run the cmd to refresh WC when closing w/save.
        /// </summary>
        public bool IsRefreshWorkingCopyOnSave
        {
            get
            {
                return _isRefreshWorkingCopyOnSave;
            }
            set
            {
                _isRefreshWorkingCopyOnSave = value;
                FirePropertyChanged();
            }
        }

        /// <summary>
        /// Tells whether the rules have been edited in the UI against what's on disk.
        /// </summary>
        public bool IsRulesTextChanged
        {
            get
            {
                return (_sRulesText != null) && (_sRulesText != (_sRulesTextAsOnDisk ?? ""));
            }
        }

        /// <summary>
        /// Current UI state of the Git sparse option.
        /// </summary>
        public bool IsSparseCheckoutEnabled
        {
            get
            {
                return _isSparseCheckoutEnabled;
            }
            set
            {
                if(value == _isSparseCheckoutEnabled)
                    return;
                _isSparseCheckoutEnabled = value;
                FirePropertyChanged();
            }
        }

        /// <summary>
        /// Current UI state of the sparse WC rules text. NULL if n/a.
        /// </summary>
        [CanBeNull]
        public string RulesText
        {
            get
            {
                return _sRulesText;
            }
            set
            {
                if(_sRulesText == value)
                    return;
                _sRulesText = value;
                FirePropertyChanged();
            }
        }

        public void FirePropertyChanged()
        {
            PropertyChanged(this, new PropertyChangedEventArgs(""));
        }

        /// <summary>
        /// Read from settings.
        /// </summary>
        public bool GetCurrentSparseEnabledState()
        {
            return StringComparer.OrdinalIgnoreCase.Equals(_gitcommands.Module.GetEffectiveSetting(SettingCoreSparseCheckout), Boolean.TrueString);
        }

        /// <summary>
        /// Path to the file with the sparse WC rules.
        /// </summary>
        [NotNull]
        public FileInfo GetPathToSparseCheckoutFile()
        {
            return new FileInfo(Path.Combine(Path.Combine(_gitcommands.GitModule.GetGitDirectory(), "info"), "sparse-checkout"));
        }

        /// <summary>
        /// Checks if got anything to save. Can cancel without confirmation if not dirty.
        /// </summary>
        /// <returns></returns>
        public bool IsWithUnsavedChanges()
        {
            if(IsSparseCheckoutEnabled != _isSparseCheckoutEnabledAsSaved)
                return true;
            if(IsRulesTextChanged)
                return true;
            return false;
        }

        /// <summary>
        /// Re-applies the current options/rules to the WC.
        /// </summary>
        public void RefreshWorkingCopy()
        {
            // Re-apply tree to the index
            // TODO: check how it affects the uncommitted working copy changes
            using(var fromProcess = new FormRemoteProcess(_gitcommands.Module, AppSettings.GitCommand, RefreshWorkingCopyCommandName))
                fromProcess.ShowDialog(Form.ActiveForm);
        }

        /// <summary>
        /// Saves changes (if modified), and refresh WC.
        /// </summary>
        public void SaveChanges()
        {
            // Don't abort if !IsWithUnsavedChanges because we have to run IsRefreshWorkingCopyOnSave in either case (e.g. if edited by hand or got outdated)

            // Enabled state for the repo
            if(IsSparseCheckoutEnabled != _isSparseCheckoutEnabledAsSaved)
            {
                _gitcommands.Module.SetSetting(SettingCoreSparseCheckout, IsSparseCheckoutEnabled.ToString().ToLowerInvariant());
                _isSparseCheckoutEnabledAsSaved = IsSparseCheckoutEnabled;
            }

            // Rules set
            if(IsRulesTextChanged)
            {
                string sNewText = RulesText ?? "";
                File.WriteAllBytes(GetPathToSparseCheckoutFile().FullName, GitModule.SystemEncoding.GetBytes(sNewText));
                SetRulesTextAsOnDisk(sNewText); // Update if OK
            }

            // Refresh WC (if chose to Save, run this regardless of the modifications)
            if(IsRefreshWorkingCopyOnSave)
                RefreshWorkingCopy();
        }

        /// <summary>
        /// As view loads the text in its impl of the editor, tells the exact on-disk text when it gets known.
        /// </summary>
        /// <param name="text"></param>
        public void SetRulesTextAsOnDisk([NotNull] string text)
        {
            if(text == null)
                throw new ArgumentNullException("text");
            _sRulesTextAsOnDisk = text;
        }

        /// <summary>
        /// Fires on any prop change. Lightweight reactive.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}