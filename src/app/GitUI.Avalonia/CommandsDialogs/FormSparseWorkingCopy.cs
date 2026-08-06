using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI.Compat;
using GitUI.Editor;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// The original builds its view in code and self-translates through the nested Globalized helper
// (XLF category "Globalized"); the form itself contributes no XLF keys, so it stays untranslated.
[Untranslated]
public sealed class FormSparseWorkingCopy : GitModuleForm
{
    // parity-scaffolding: Avalonia's view inventory and designer require a parameterless constructor.
    public FormSparseWorkingCopy()
    {
        InitializeComplete();
    }

    public FormSparseWorkingCopy(IGitUICommands commands)
        : base(commands, enablePositionRestore: true)
    {
        FormSparseWorkingCopyViewModel sparse = new(commands);
        BindToViewModelGlobal(sparse);
        CreateView(sparse);
        InitializeComplete();
    }

    // A code-built Window subclass must adopt the framework Window theme; otherwise Fluent looks
    // for a theme keyed by this subclass and renders no chrome or content.
    protected override Type StyleKeyOverride => typeof(Window);

    private void BindSaveOnClose(FormSparseWorkingCopyViewModel sparse)
    {
        ArgumentNullException.ThrowIfNull(sparse);

        Closing += (sender, args) =>
        {
            try
            {
                // Save on OK — even if not dirty, to upd the rules if checkbox is ON
                if (DialogResult == WinFormsShims.DialogResult.OK)
                {
                    sparse.SaveChanges();
                    return;
                }

                // Closing/canceling, prompt to save if dirty
                if (sparse.IsWithUnsavedChanges())
                {
                    switch (MessageBoxes.Show(this, Globalized.Strings.YouHaveMadeChangesToSettingsOrRulesWouldYouLikeToSaveThem.Text, Globalized.Strings.SparseWorkingCopy.Text + " – " + Globalized.Strings.Cancel.Text, WinFormsShims.MessageBoxButtons.YesNoCancel, WinFormsShims.MessageBoxIcon.Question))
                    {
                        case WinFormsShims.DialogResult.Yes:
                            sparse.SaveChanges();
                            break;
                        case WinFormsShims.DialogResult.No:
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
                MessageBoxes.Show(this, Globalized.Strings.CouldNotSave.Text + "\n\n" + ex.Message, Globalized.Strings.SparseWorkingCopy.Text + " – " + Globalized.Strings.SaveFile.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            }
        };
    }

    private void BindToViewModelGlobal(FormSparseWorkingCopyViewModel sparse)
    {
        ArgumentNullException.ThrowIfNull(sparse);

        sparse.ComfirmAdjustingRulesOnDeactRequested += (sender, args) =>
        {
            if (!args.Cancel)
            {
                args.Cancel |= MessageBoxes.Show(this, string.Format(Globalized.Strings.ConfirmDisableGitSparse.Text, args.IsCurrentRuleSetEmpty ? Globalized.Strings.WithTheSparsePassFilterEmptyOrMissing.Text : Globalized.Strings.WithSomeRulesStillInTheSparsePassFilter.Text), Globalized.Strings.DisableGitSparse.Text, WinFormsShims.MessageBoxButtons.YesNo, WinFormsShims.MessageBoxIcon.Question) != WinFormsShims.DialogResult.Yes;
            }
        };
    }

    private void CreateView(FormSparseWorkingCopyViewModel sparse)
    {
        Text = Globalized.Strings.SparseWorkingCopy.Text;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        MinWidth = 800;
        MinHeight = 600;

        Control panelHeader = CreateViewHeader();

        Control panelFooter = CreateViewFooter(sparse, out Button btnSave, out Button btnCancel);

        Control panelOnOff = CreateViewOnOff(sparse);

        Control panelRules = CreateViewRules(sparse, this);

        sparse.FirePropertyChanged(); // Initial binding

        Grid root = new()
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,*,Auto,Auto"),
        };
        AddRow(root, 0, panelHeader);
        AddRow(root, 1, CreateViewSeparator());
        AddRow(root, 2, panelOnOff);
        AddRow(root, 3, panelRules);
        AddRow(root, 4, CreateViewSeparator());
        AddRow(root, 5, panelFooter);
        Content = root;

        AcceptButton = btnSave;
        CancelButton = btnCancel;

        // WinForms Button.DialogResult has no Avalonia twin; setting the form's DialogResult while
        // modal closes it, matching the original OK/Cancel buttons.
        btnSave.Click += delegate { DialogResult = WinFormsShims.DialogResult.OK; };
        btnCancel.Click += delegate { DialogResult = WinFormsShims.DialogResult.Cancel; };

        BindSaveOnClose(sparse);

        // Special binding: as the editor takes Enter for itself, bind Ctrl+Enter to commit
        AddHandler(KeyDownEvent, (sender, args) =>
        {
            if (args.Key == Key.Enter && args.KeyModifiers == KeyModifiers.Control)
            {
                DialogResult = WinFormsShims.DialogResult.OK;
                Close();
            }
        }, RoutingStrategies.Tunnel);
    }

    private static Control CreateViewFooter(FormSparseWorkingCopyViewModel sparse, out Button btnSave, out Button btnCancel)
    {
        Grid tableFooterButtons = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto"),
            Margin = new Thickness(10, 15, 10, 15),
        };
        SetDynamicBackground(tableFooterButtons, "GitExtensionsKnownColorControlLightLightBrush");

        CheckBox check = new() { Content = Globalized.Strings.RefreshWorkingCopyUsingTheCurrentSettingsAndRules.Text, IsChecked = sparse.IsRefreshWorkingCopyOnSave, VerticalAlignment = VerticalAlignment.Center };
        check.IsCheckedChanged += delegate { sparse.IsRefreshWorkingCopyOnSave = check.IsChecked == true; };
        ToolTip.SetTip(check, string.Format(Globalized.Strings.RefreshWorkingCopyCheckboxHint.Text, FormSparseWorkingCopyViewModel.RefreshWorkingCopyCommandName));
        Grid.SetColumn(check, 0);
        tableFooterButtons.Children.Add(check);

        btnSave = new Button { MinWidth = 75, MinHeight = 23, Content = ToAccessKey(Globalized.Strings.Save.Text), VerticalAlignment = VerticalAlignment.Bottom };
        Grid.SetColumn(btnSave, 1);
        tableFooterButtons.Children.Add(btnSave);

        Control spacer = new Panel { Width = 10 };
        Grid.SetColumn(spacer, 2);
        tableFooterButtons.Children.Add(spacer);

        btnCancel = new Button { MinWidth = 75, MinHeight = 23, Content = ToAccessKey(Globalized.Strings.Cancel.Text), VerticalAlignment = VerticalAlignment.Bottom };
        Grid.SetColumn(btnCancel, 3);
        tableFooterButtons.Children.Add(btnCancel);

        return tableFooterButtons;
    }

    private static Control CreateViewHeader()
    {
        StackPanel panelHeaderMain = new() { Orientation = Orientation.Vertical };
        SetDynamicBackground(panelHeaderMain, "GitExtensionsKnownColorControlLightLightBrush");

        TextBlock labelTitle = new() { Text = Globalized.Strings.SparseWorkingCopy.Text, Margin = new Thickness(10, 10, 10, 0), FontWeight = FontWeight.Bold };
        panelHeaderMain.Children.Add(labelTitle);

        panelHeaderMain.Children.Add(new TextBlock { Text = Globalized.Strings.HeaderDetailsText.Text, Margin = new Thickness(25, 6, 10, 10), TextWrapping = TextWrapping.Wrap });

        return panelHeaderMain;
    }

    private static Control CreateViewOnOff(FormSparseWorkingCopyViewModel sparse)
    {
        // When disabled: hint-like panel to enable
        Grid panelWhenDisabled = new() { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(10, 5, 10, 5) };
        SetDynamicBackground(panelWhenDisabled, "GitExtensionsKnownColorInfoBrush");
        TextBlock disabledText = new() { Text = Globalized.Strings.SparseWorkingCopySupportHasNotBeenEnabledForThisRepository.Text, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
        SetDynamicForeground(disabledText, "GitExtensionsKnownColorInfoTextBrush");
        Grid.SetColumn(disabledText, 0);
        panelWhenDisabled.Children.Add(disabledText);
        Button btnEnable = new() { MinWidth = 75, MinHeight = 23, Content = ToAccessKey(Globalized.Strings.Enable.Text), HorizontalAlignment = HorizontalAlignment.Right };
        btnEnable.Click += delegate { sparse.IsSparseCheckoutEnabled = true; };
        ToolTip.SetTip(btnEnable, string.Format(Globalized.Strings.SetsTheGitPropertyToTrueForTheLocalRepository.Text, FormSparseWorkingCopyViewModel.SettingCoreSparseCheckout));
        Grid.SetColumn(btnEnable, 1);
        panelWhenDisabled.Children.Add(btnEnable);
        sparse.PropertyChanged += delegate { panelWhenDisabled.IsVisible = !sparse.IsSparseCheckoutEnabled; };

        // When-disabled case should have a separator
        Control separatorWhenDisabled = CreateViewSeparator();
        sparse.PropertyChanged += delegate { separatorWhenDisabled.IsVisible = !sparse.IsSparseCheckoutEnabled; };

        // When enabled: a less bold link to disable
        StackPanel labelWhenEnabled = new() { Orientation = Orientation.Horizontal, Margin = new Thickness(10, 10, 10, 5) };
        labelWhenEnabled.Children.Add(new TextBlock { Text = Globalized.Strings.SparseWorkingCopySupportIsEnabled.Text + ' ', VerticalAlignment = VerticalAlignment.Center });
        HyperlinkButton linkDisable = new() { Content = Globalized.Strings.DisableForThisRepository.Text };
        linkDisable.Click += delegate { sparse.IsSparseCheckoutEnabled = false; };
        ToolTip.SetTip(labelWhenEnabled, string.Format(Globalized.Strings.SetsTheGitPropertyToFalseForTheLocalRepository.Text, FormSparseWorkingCopyViewModel.SettingCoreSparseCheckout));
        labelWhenEnabled.Children.Add(linkDisable);
        sparse.PropertyChanged += delegate { labelWhenEnabled.IsVisible = sparse.IsSparseCheckoutEnabled; };

        StackPanel panel = new() { Orientation = Orientation.Vertical };
        panel.Children.Add(panelWhenDisabled);
        panel.Children.Add(separatorWhenDisabled);
        panel.Children.Add(labelWhenEnabled);
        return panel;
    }

    private static Control CreateViewRules(FormSparseWorkingCopyViewModel sparse, IGitUICommandsSource commandsSource)
    {
        Grid panel = new() { RowDefinitions = new RowDefinitions("Auto,Auto,Auto,*") };

        // Label
        TextBlock label1 = new() { Text = Globalized.Strings.SpecifyTheRulesForIncludingOrExcludingFilesAndDirectories.Text, Margin = new Thickness(10, 5, 10, 0), TextWrapping = TextWrapping.Wrap };
        TextBlock label2 = new() { Text = Globalized.Strings.SpecifyTheRulesForIncludingOrExcludingFilesAndDirectoriesLine2.Text, Margin = new Thickness(25, 3, 10, 3), TextWrapping = TextWrapping.Wrap };
        SetDynamicForeground(label2, "GitExtensionsKnownColorGrayTextBrush");
        sparse.PropertyChanged += delegate { label1.IsVisible = label2.IsVisible = sparse.IsSparseCheckoutEnabled; };
        Grid.SetRow(label1, 0);
        Grid.SetRow(label2, 1);
        panel.Children.Add(label1);
        panel.Children.Add(label2);

        // Separator
        Control separator = CreateViewSeparator();
        Grid.SetRow(separator, 2);
        panel.Children.Add(separator);

        // Text editor
        FileViewer editor = new() { UICommandsSource = commandsSource, IsReadOnly = false };
        editor.TextLoaded += (sender, args) => sparse.SetRulesTextAsOnDisk(editor.GetText());
        try
        {
            FileInfo sparseFile = sparse.GetPathToSparseCheckoutFile();
            if (sparseFile.Exists)
            {
                _ = editor.ViewFileAsync(sparseFile.FullName);
            }
        }
        catch (Exception ex)
        {
            MessageBoxes.Show(null, Globalized.Strings.CannotLoadTheTextOfTheSparseFile.Text + "\n\n" + ex.Message, Globalized.Strings.SparseWorkingCopy.Text + " – " + Globalized.Strings.LoadFile.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
        }

        editor.TextChanged += (sender, args) => sparse.RulesText = editor.GetText() ?? "";
        ToolTip.SetTip(editor, Globalized.Strings.EditsTheContentsOfTheGitInfoSparseCheckoutFile.Text);
        sparse.PropertyChanged += delegate { editor.IsVisible = separator.IsVisible = sparse.IsSparseCheckoutEnabled; };
        Grid.SetRow(editor, 3);
        panel.Children.Add(editor);

        return panel;
    }

    private static Control CreateViewSeparator()
    {
        Border separator = new() { Height = 2 };
        SetDynamicBackground(separator, "GitExtensionsKnownColorControlDarkBrush");
        return separator;
    }

    private static void AddRow(Grid grid, int row, Control control)
    {
        Grid.SetRow(control, row);
        grid.Children.Add(control);
    }

    private static void SetDynamicBackground(Border control, string resourceKey)
        => control[!Border.BackgroundProperty] = new DynamicResourceExtension(resourceKey);

    private static void SetDynamicBackground(Panel control, string resourceKey)
        => control[!Panel.BackgroundProperty] = new DynamicResourceExtension(resourceKey);

    private static void SetDynamicForeground(TextBlock control, string resourceKey)
        => control[!TextBlock.ForegroundProperty] = new DynamicResourceExtension(resourceKey);

    // The WinForms mnemonic marker '&' has no meaning in Avalonia; convert it to the access-key
    // marker '_' (doubling escapes literals) so keyboard accelerators keep working.
    private static string ToAccessKey(string text)
        => text.Replace("_", "__").Replace("&&", "").Replace("&", "_").Replace("", "&");

    private sealed class Globalized : Translate
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

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormSparseWorkingCopy form)
    {
        public Grid Root => (Grid)form.Content!;
    }
}
