using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using GitCommands;

using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    public class FormSparseWorkingCopyViewModel : INotifyPropertyChanged
    {
        public static readonly string RefreshWorkingCopyCommandName = "read-tree -m -u HEAD";

        public static readonly string SettingCoreSparseCheckout = "core.sparseCheckout";

        [NotNull]
        private readonly GitUICommands _gitCommands;

        private bool _isRefreshWorkingCopyOnSave = true /* on by default, otherwise index bitmap won't be updated */;

        private bool _isSparseCheckoutEnabled;

        /// <summary>
        /// Remembers what were in settings, to check <see cref="IsSparseCheckoutEnabled" /> against to tell if modified.
        /// </summary>
        private bool _isSparseCheckoutEnabledAsSaved;

        [CanBeNull]
        private string _rulesText;

        /// <summary>
        /// Remembers what were loaded from disk, to check <see cref="RulesText" /> against to tell if modified.
        /// NULL until loaded.
        /// </summary>
        [CanBeNull]
        private string _sRulesTextAsOnDisk;

        public FormSparseWorkingCopyViewModel([NotNull] GitUICommands gitCommands)
        {
            _gitCommands = gitCommands ?? throw new ArgumentNullException(nameof(gitCommands));
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
        public bool IsRulesTextChanged => (_rulesText != null) && (_rulesText != (_sRulesTextAsOnDisk ?? ""));

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
                if (value == _isSparseCheckoutEnabled)
                {
                    return;
                }

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
                return _rulesText;
            }
            set
            {
                if (_rulesText == value)
                {
                    return;
                }

                _rulesText = value;
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
            return StringComparer.OrdinalIgnoreCase.Equals(_gitCommands.Module.GetEffectiveSetting(SettingCoreSparseCheckout), bool.TrueString);
        }

        /// <summary>
        /// Path to the file with the sparse WC rules.
        /// </summary>
        [NotNull]
        public FileInfo GetPathToSparseCheckoutFile()
        {
            return new FileInfo(Path.Combine(_gitCommands.GitModule.ResolveGitInternalPath("info"), "sparse-checkout"));
        }

        /// <summary>
        /// Checks if got anything to save. Can cancel without confirmation if not dirty.
        /// </summary>
        public bool IsWithUnsavedChanges()
        {
            if (IsSparseCheckoutEnabled != _isSparseCheckoutEnabledAsSaved)
            {
                return true;
            }

            if (IsRulesTextChanged)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Re-applies the current options/rules to the WC.
        /// </summary>
        public void RefreshWorkingCopy()
        {
            // Re-apply tree to the index
            // TODO: check how it affects the uncommitted working copy changes
            using (var fromProcess = new FormRemoteProcess(_gitCommands.Module, AppSettings.GitCommand, RefreshWorkingCopyCommandName))
            {
                fromProcess.ShowDialog(Form.ActiveForm);
            }
        }

        /// <summary>
        /// Saves changes (if modified), and refresh WC.
        /// </summary>
        public void SaveChanges()
        {
            // Don't abort if !IsWithUnsavedChanges because we have to run IsRefreshWorkingCopyOnSave in either case (e.g. if edited by hand or got outdated)

            // Special case: turning off sparse for a repo — this won't just go smoothly, looks like git still reads the sparse checkout rules, so emptying or deleting them with turning off will just leave you with what you had before
            SaveChangesTurningOffSparseSpecialCase();

            // Enabled state for the repo
            if (IsSparseCheckoutEnabled != _isSparseCheckoutEnabledAsSaved)
            {
                _gitCommands.Module.SetSetting(SettingCoreSparseCheckout, IsSparseCheckoutEnabled.ToString().ToLowerInvariant());
                _isSparseCheckoutEnabledAsSaved = IsSparseCheckoutEnabled;
            }

            // Rules set
            if (IsRulesTextChanged)
            {
                string newText = RulesText ?? "";
                File.WriteAllBytes(GetPathToSparseCheckoutFile().FullName, GitModule.SystemEncoding.GetBytes(newText));
                SetRulesTextAsOnDisk(newText); // Update if OK
            }

            // Refresh WC (if chose to Save, run this regardless of the modifications)
            if (IsRefreshWorkingCopyOnSave)
            {
                RefreshWorkingCopy();
            }
        }

        /// <summary>
        /// As view loads the text in its impl of the editor, tells the exact on-disk text when it gets known.
        /// </summary>
        public void SetRulesTextAsOnDisk([NotNull] string text)
        {
            _sRulesTextAsOnDisk = text ?? throw new ArgumentNullException(nameof(text));
        }

        /// <summary>
        /// Fires for the view to show a confirmation msgbox on the matter.
        /// </summary>
        public event EventHandler<ComfirmAdjustingRulesOnDeactEventArgs> ComfirmAdjustingRulesOnDeactRequested = delegate { };

        /// <summary>
        /// Fires on any prop change. Lightweight reactive.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// Make sure WC gets unsparsed when turning off sparse.
        /// </summary>
        private void SaveChangesTurningOffSparseSpecialCase()
        {
            if (IsSparseCheckoutEnabled || !_isSparseCheckoutEnabledAsSaved)
            {
                return; // Not turning off
            }

            // Now check the rules, the well-known recommendation is to have the single "/*" rule active
            List<string> rulelines = RulesText.SplitLines().Select(l => l.Trim()).Where(l => (!l.IsNullOrEmpty()) && (l[0] != '#')).ToList(); // All nonempty and non-comment lines
            if (rulelines.All(l => l == "/*"))
            {
                return; // Rules OK for turning off
            }

            // Confirm
            var args = new ComfirmAdjustingRulesOnDeactEventArgs(!rulelines.Any());
            ComfirmAdjustingRulesOnDeactRequested(this, args);
            if (args.Cancel)
            {
                return;
            }

            // Adjust the rules
            // Comment out all existing nonempty lines, add the single “/*” line to make a total pass filter
            RulesText = new[] { "/*" }.Concat(RulesText.SplitLines().Select(l => (string.IsNullOrWhiteSpace(l) || (l[0] == '#')) ? l : "#" + l)).Join(Environment.NewLine);
        }

        /// <summary>
        /// For <see cref="ComfirmAdjustingRulesOnDeactRequested" />.
        /// </summary>
        public class ComfirmAdjustingRulesOnDeactEventArgs : CancelEventArgs
        {
            public ComfirmAdjustingRulesOnDeactEventArgs(bool isCurrentRuleSetEmpty)
            {
                IsCurrentRuleSetEmpty = isCurrentRuleSetEmpty;
            }

            /// <summary>
            /// Empty rule set vs. got some stuff there
            /// </summary>
            public bool IsCurrentRuleSetEmpty { get; }
        }
    }
}