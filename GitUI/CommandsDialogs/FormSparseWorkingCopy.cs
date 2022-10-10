﻿using System.ComponentModel;
using System.Runtime.InteropServices;
using GitCommands;
using GitUI.Editor;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed class FormSparseWorkingCopy : GitModuleForm
    {
        private IDisposable? _disposable1;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormSparseWorkingCopy()
        {
        }

        public FormSparseWorkingCopy(GitUICommands commands)
            : base(commands)
        {
            FormSparseWorkingCopyViewModel sparse = new(commands);
            BindToViewModelGlobal(sparse);
            CreateView(sparse);
            InitializeComplete();
        }

        private void BindSaveOnClose(FormSparseWorkingCopyViewModel sparse)
        {
            if (sparse is null)
            {
                throw new ArgumentNullException(nameof(sparse));
            }

            Closing += (sender, args) =>
            {
                try
                {
                    // Save on OK — even if not dirty, to upd the rules if checkbox is ON
                    if (DialogResult == DialogResult.OK)
                    {
                        sparse.SaveChanges();
                        return;
                    }

                    // Closing/canceling, prompt to save if dirty
                    if (sparse.IsWithUnsavedChanges())
                    {
                        switch (MessageBox.Show(this, Globalized.Strings.YouHaveMadeChangesToSettingsOrRulesWouldYouLikeToSaveThem.Text, Globalized.Strings.SparseWorkingCopy.Text + " – " + Globalized.Strings.Cancel.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                        {
                            case DialogResult.Yes:
                                sparse.SaveChanges();
                                break;
                            case DialogResult.No:
                                // Just exit
                                break;
                            default:
                                // Cancel, or error
                                args.Cancel = true;
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ActiveForm, Globalized.Strings.CouldNotSave.Text + "\n\n" + ex.Message, Globalized.Strings.SparseWorkingCopy.Text + " – " + Globalized.Strings.SaveFile.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }

        private void BindToViewModelGlobal(FormSparseWorkingCopyViewModel sparse)
        {
            if (sparse is null)
            {
                throw new ArgumentNullException(nameof(sparse));
            }

            sparse.ComfirmAdjustingRulesOnDeactRequested += (sender, args) =>
            {
                if (!args.Cancel)
                {
                    args.Cancel |= MessageBox.Show(this, string.Format(Globalized.Strings.ConfirmDisableGitSparse.Text, args.IsCurrentRuleSetEmpty ? Globalized.Strings.WithTheSparsePassFilterEmptyOrMissing.Text : Globalized.Strings.WithSomeRulesStillInTheSparsePassFilter.Text), Globalized.Strings.DisableGitSparse.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes;
                }
            };
        }

        private void CreateView(FormSparseWorkingCopyViewModel sparse)
        {
            Text = Globalized.Strings.SparseWorkingCopy.Text;
            AutoScaleMode = AutoScaleMode.Dpi;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(800, 600);

            // Tooltips support for the form
            Container componentContainer = new();
            _disposable1 = componentContainer;
            ToolTip tooltip = new(componentContainer) { AutomaticDelay = 100 };

            Panel panelHeader = CreateViewHeader();

            Panel panelFooter = CreateViewFooter(sparse, tooltip, out var btnSave, out var btnCancel);

            Control panelOnOff = CreateViewOnOff(sparse, tooltip);

            Panel panelRules = CreateViewRules(sparse, tooltip, this);

            sparse.FirePropertyChanged(); // Initial binding

            Controls.Add(new TableLayoutPanel { Dock = DockStyle.Fill, Padding = Padding.Empty, Margin = Padding.Empty, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Controls = { panelHeader, CreateViewSeparator(), panelOnOff, panelRules, CreateViewSeparator(), panelFooter }, RowStyles = { new RowStyle(), new RowStyle(), new RowStyle(), new RowStyle(SizeType.Percent, 100) } });

            AcceptButton = btnSave;
            CancelButton = btnCancel;

            BindSaveOnClose(sparse);

            // Special binding: as the editor takes Enter for itself, bind Ctrl+Enter to commit
            KeyPreview = true;
            PreviewKeyDown += (sender, args) =>
            {
                if (args.KeyData == (Keys.Enter | Keys.Control))
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            };
        }

        private static Panel CreateViewFooter(FormSparseWorkingCopyViewModel sparse, ToolTip tooltip, out Button btnSave, out Button btnCancel)
        {
            TableLayoutPanel tableFooterButtons = new() { BackColor = SystemColors.ControlLightLight, Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 4, RowCount = 1, Margin = Padding.Empty, ColumnStyles = { new ColumnStyle(SizeType.Percent, 100) }, Padding = new Padding(10, 15, 10, 15), CellBorderStyle = TableLayoutPanelCellBorderStyle.None };

            CheckBox check;
            tableFooterButtons.Controls.Add(check = new CheckBox { Text = Globalized.Strings.RefreshWorkingCopyUsingTheCurrentSettingsAndRules.Text, Checked = sparse.IsRefreshWorkingCopyOnSave, AutoSize = true, Dock = DockStyle.Fill, Margin = Padding.Empty });
            check.CheckedChanged += delegate { sparse.IsRefreshWorkingCopyOnSave = check.Checked; };
            tooltip.SetToolTip(check, string.Format(Globalized.Strings.RefreshWorkingCopyCheckboxHint.Text, FormSparseWorkingCopyViewModel.RefreshWorkingCopyCommandName));

            tableFooterButtons.Controls.Add(btnSave = new Button { Width = 75, Height = 23, Text = Globalized.Strings.Save.Text, DialogResult = DialogResult.OK, Dock = DockStyle.Bottom, UseVisualStyleBackColor = true, Margin = Padding.Empty });

            tableFooterButtons.Controls.Add(new Control { Width = 10, Dock = DockStyle.Fill });

            tableFooterButtons.Controls.Add(btnCancel = new Button { Width = 75, Height = 23, Text = Globalized.Strings.Cancel.Text, DialogResult = DialogResult.Cancel, Dock = DockStyle.Bottom, UseVisualStyleBackColor = true, Margin = Padding.Empty });

            return tableFooterButtons;
        }

        private static Panel CreateViewHeader()
        {
            TableLayoutPanel panelHeaderMain = new() { BackColor = SystemColors.ControlLightLight, Dock = DockStyle.Fill, AutoSize = true, Margin = Padding.Empty, Padding = Padding.Empty, RowCount = 2, ColumnCount = 1 };

            Label labelTitle;
            panelHeaderMain.Controls.Add(labelTitle = new Label { Text = Globalized.Strings.SparseWorkingCopy.Text, Dock = DockStyle.Bottom, AutoSize = true, Margin = new Padding(10, 10, 10, 0) });
            labelTitle.Font = new Font(labelTitle.Font, FontStyle.Bold);

            panelHeaderMain.Controls.Add(new Label { Text = Globalized.Strings.HeaderDetailsText.Text, Dock = DockStyle.Bottom, AutoSize = true, Margin = new Padding(25, 6, 10, 10) });

            return panelHeaderMain;
        }

        private static Control CreateViewOnOff(FormSparseWorkingCopyViewModel sparse, ToolTip tooltip)
        {
            // When disabled: hint-like panel to enable
            TableLayoutPanel panelWhenDisabled = new() { BackColor = SystemColors.Info, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Bottom, ColumnCount = 2, RowCount = 1, ColumnStyles = { new ColumnStyle(SizeType.Percent, 100) }, Margin = Padding.Empty, Padding = new Padding(10, 5, 10, 5) };
            panelWhenDisabled.Controls.Add(new Label { ForeColor = SystemColors.InfoText, Text = Globalized.Strings.SparseWorkingCopySupportHasNotBeenEnabledForThisRepository.Text, Dock = DockStyle.Fill, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = Padding.Empty });
            Button btnEnable;
            panelWhenDisabled.Controls.Add(btnEnable = new Button { Width = 75, Height = 23, Text = Globalized.Strings.Enable.Text, Anchor = AnchorStyles.Bottom | AnchorStyles.Right, Dock = DockStyle.Right, UseVisualStyleBackColor = true, Margin = Padding.Empty });
            btnEnable.Click += delegate { sparse.IsSparseCheckoutEnabled = true; };
            tooltip.SetToolTip(btnEnable, string.Format(Globalized.Strings.SetsTheGitPropertyToTrueForTheLocalRepository.Text, FormSparseWorkingCopyViewModel.SettingCoreSparseCheckout));
            sparse.PropertyChanged += delegate { panelWhenDisabled.Visible = !sparse.IsSparseCheckoutEnabled; };

            // When-disabled case should have a separator
            Control separatorWhenDisabled = CreateViewSeparator(DockStyle.Bottom);
            sparse.PropertyChanged += delegate { separatorWhenDisabled.Visible = !sparse.IsSparseCheckoutEnabled; };

            // When enabled: a less bold link to disable
            string labelBeforeLink = Globalized.Strings.SparseWorkingCopySupportIsEnabled.Text + ' ';
            string labelWithLink = labelBeforeLink + Globalized.Strings.DisableForThisRepository.Text;
            LinkLabel labelWhenEnabled = new() { Text = labelWithLink, Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(10, 10, 10, 5), FlatStyle = FlatStyle.System, UseCompatibleTextRendering = true };
            labelWhenEnabled.Links.Add(new LinkLabel.Link(labelBeforeLink.Length, labelWithLink.Length - labelBeforeLink.Length));
            labelWhenEnabled.LinkClicked += delegate { sparse.IsSparseCheckoutEnabled = false; };
            tooltip.SetToolTip(labelWhenEnabled, string.Format(Globalized.Strings.SetsTheGitPropertyToFalseForTheLocalRepository.Text, FormSparseWorkingCopyViewModel.SettingCoreSparseCheckout));
            sparse.PropertyChanged += delegate { labelWhenEnabled.Visible = sparse.IsSparseCheckoutEnabled; };

            return new Panel { Dock = DockStyle.Fill, Controls = { panelWhenDisabled, separatorWhenDisabled, labelWhenEnabled }, Margin = Padding.Empty, Padding = Padding.Empty, AutoSize = true };
        }

        private static Panel CreateViewRules(FormSparseWorkingCopyViewModel sparse, ToolTip tooltip, IGitUICommandsSource commandsSource)
        {
            // Label
            Label label1 = new() { AutoSize = true, Text = Globalized.Strings.SpecifyTheRulesForIncludingOrExcludingFilesAndDirectories.Text, Dock = DockStyle.Top, Padding = new Padding(10, 5, 10, 0) };
            Label label2 = new() { AutoSize = true, Text = Globalized.Strings.SpecifyTheRulesForIncludingOrExcludingFilesAndDirectoriesLine2.Text, Dock = DockStyle.Top, Padding = new Padding(25, 3, 10, 3), ForeColor = SystemColors.GrayText };
            sparse.PropertyChanged += delegate { label1.Visible = label2.Visible = sparse.IsSparseCheckoutEnabled; };

            // Text editor
            FileViewer editor = new() { Dock = DockStyle.Fill, UICommandsSource = commandsSource, IsReadOnly = false };
            editor.TextLoaded += (sender, args) => sparse.SetRulesTextAsOnDisk(editor.GetText());
            try
            {
                FileInfo sparseFile = sparse.GetPathToSparseCheckoutFile();
                if (sparseFile.Exists)
                {
                    editor.ViewFileAsync(sparseFile.FullName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ActiveForm, Globalized.Strings.CannotLoadTheTextOfTheSparseFile.Text + "\n\n" + ex.Message, Globalized.Strings.SparseWorkingCopy.Text + " – " + Globalized.Strings.LoadFile.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            editor.TextChanged += (sender, args) => sparse.RulesText = editor.GetText() ?? "";
            tooltip.SetToolTip(editor, Globalized.Strings.EditsTheContentsOfTheGitInfoSparseCheckoutFile.Text);
            Control separator = CreateViewSeparator(DockStyle.Top);
            sparse.PropertyChanged += delegate { editor.Visible = separator.Visible = sparse.IsSparseCheckoutEnabled; };

            Panel panel = new() { Margin = Padding.Empty, Padding = Padding.Empty, Controls = { editor, separator, label2, label1 }, AutoSize = true, Dock = DockStyle.Fill };

            return panel;
        }

        private static Control CreateViewSeparator([Optional] DockStyle? dock)
        {
            return new Control { Height = 2, BackColor = SystemColors.ControlDark, Dock = dock ?? DockStyle.Fill, Padding = Padding.Empty, Margin = Padding.Empty };
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _disposable1?.Dispose();
            }
        }

        private class Globalized : Translate
        {
            public static readonly Globalized Strings = new();

            private Globalized()
            {
                Translator.Translate(this, AppSettings.CurrentTranslation);
            }

            public readonly TranslationString Cancel = new("Cancel");

            public readonly TranslationString CannotLoadTheTextOfTheSparseFile = new("Cannot load the text of the sparse file.");

            public readonly TranslationString ConfirmDisableGitSparse = new("You are about to disable Git Sparse feature for this repository, {0}.\nGit won't be able to restore the working copy to its full content this way.\n\nWould you like to have the filter modified so that it allowed for the full working copy?");

            public readonly TranslationString CouldNotSave = new("Could not save the modified settings and rules.");

            public readonly TranslationString DisableForThisRepository = new("Disable for this repository");

            public readonly TranslationString DisableGitSparse = new("Disable Git Sparse");

            public readonly TranslationString EditsTheContentsOfTheGitInfoSparseCheckoutFile = new("Edits the contents of the “.git/info/sparse-checkout” file.");

            public readonly TranslationString Enable = new("&Enable");

            public readonly TranslationString HeaderDetailsText = new("Need only a small part of a large repository?\nWith sparse checkout, you can skip the rest from being extracted into your working copy.");

            public readonly TranslationString LoadFile = new("Load File");

            public readonly TranslationString RefreshWorkingCopyCheckboxHint = new("As the sparse working copy rules are changed, it might become outdated.\nRefreshes the working copy against the current set of the rules to restore any missing files and remove any extra files.\n\nnActual command line: {0}");

            public readonly TranslationString RefreshWorkingCopyUsingTheCurrentSettingsAndRules = new("Refresh working copy using the current settings and rules");

            public readonly TranslationString Save = new("&Save");

            public readonly TranslationString SaveFile = new("Save File");

            public readonly TranslationString SetsTheGitPropertyToFalseForTheLocalRepository = new("Sets the Git property “{0}” to False for the local repository.");

            public readonly TranslationString SetsTheGitPropertyToTrueForTheLocalRepository = new("Sets the Git property “{0}” to True for the local repository.");

            public readonly TranslationString SparseWorkingCopy = new("Sparse Working Copy");

            public readonly TranslationString SparseWorkingCopySupportHasNotBeenEnabledForThisRepository = new("Git Sparse feature has not been enabled for this repository.");

            public readonly TranslationString SparseWorkingCopySupportIsEnabled = new("Git Sparse feature is currently enabled.");

            public readonly TranslationString SpecifyTheRulesForIncludingOrExcludingFilesAndDirectories = new("Specify the pass-filter rules for files and directories:");

            public readonly TranslationString SpecifyTheRulesForIncludingOrExcludingFilesAndDirectoriesLine2 = new("The rules have the same format as the “.gitignore” file, matched items are included. To exclude, prefix a rule with an exclamation mark “!”.\n“#” comments a line. This is only a filter, so it cannot change the structure like pulling up a deep subfolder to the first level.");

            public readonly TranslationString WithSomeRulesStillInTheSparsePassFilter = new("with some rules still in the sparse pass-filter");

            public readonly TranslationString WithTheSparsePassFilterEmptyOrMissing = new("with the sparse pass-filter empty or missing");

            public readonly TranslationString YouHaveMadeChangesToSettingsOrRulesWouldYouLikeToSaveThem = new("You have made changes to settings or rules.\nWould you like to save them?");
        }
    }
}
