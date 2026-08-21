using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using GitCommands;
using GitCommands.Logging;
using GitExtUtils;
using GitUI.Compat;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.BrowseDialog;

public sealed partial class FormGitCommandLog : GitExtensionsForm
{
    // parity-scaffolding: Avalonia's view inventory and designer require a public parameterless constructor.
    public FormGitCommandLog()
        : base(enablePositionRestore: true)
    {
        // Avalonia's generated x:Name field cannot share the enclosing class name; set the original Form.Name in code.
        Name = nameof(FormGitCommandLog);
        InitializeComponent();
        InitializeComplete();
        ActiveControl = LogItems;

        LogItems.ItemTemplate = new FuncDataTemplate<CommandLogEntry>(
            (entry, _) => new TextBlock { Text = entry?.ColumnLine ?? string.Empty }, supportsRecycling: true);
        CommandCacheItems.ItemTemplate = new FuncDataTemplate<CacheItem>(
            (item, _) => new TextBlock { Text = item?.DisplayString ?? string.Empty }, supportsRecycling: true);

        if (!Design.IsDesignMode)
        {
            chkCaptureCallStacks.IsChecked = AppSettings.LogCaptureCallStacks;
        }

        chkCaptureCallStacks.IsCheckedChanged += delegate { AppSettings.LogCaptureCallStacks = chkCaptureCallStacks.IsChecked == true; };

        chkWordWrap.IsCheckedChanged += delegate
        {
            TextWrapping wrapping = chkWordWrap.IsChecked == true ? TextWrapping.Wrap : TextWrapping.NoWrap;
            LogOutput.TextWrapping = wrapping;
            commandCacheOutput.TextWrapping = wrapping;
        };

        LogItems.SelectionChanged += LogItems_SelectedIndexChanged;
        CommandCacheItems.SelectionChanged += CommandCacheItems_SelectedIndexChanged;
        TabControl.SelectionChanged += TabControl_SelectedIndexChanged;
        chkAlwaysOnTop.IsCheckedChanged += alwaysOnTopCheckBox_CheckedChanged;
        mnuSaveToFile.Click += mnuSaveToFile_Click;
        mnuCopyCommandLine.Click += mnuCopyCommandLine_Click;
        mnuClear.Click += mnuClear_Click;
        tsmiClearCache.Click += tsmiClearCache_Click;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        CommandLog.CommandsChanged += OnGitCommandLogChanged;
        GitModule.GitCommandCache.Changed += OnCachedCommandsLogChanged;

        RefreshLogItems();
        RefreshCommandCacheItems();
    }

    protected override void OnClosed(EventArgs e)
    {
        CommandLog.CommandsChanged -= OnGitCommandLogChanged;
        GitModule.GitCommandCache.Changed -= OnCachedCommandsLogChanged;
        instance = null;
        base.OnClosed(e);
    }

    private void OnGitCommandLogChanged()
    {
        this.InvokeAndForget(RefreshLogItems);
    }

    private void OnCachedCommandsLogChanged(object? sender, EventArgs e)
    {
        this.InvokeAndForget(RefreshCommandCacheItems);
    }

    private void RefreshLogItems()
    {
        if (ReferenceEquals(TabControl.SelectedItem, tabPageCommandLog))
        {
            RefreshListBox(LogItems, CommandLog.Commands.ToArray());
        }
    }

    private void RefreshCommandCacheItems()
    {
        if (ReferenceEquals(TabControl.SelectedItem, tabPageCommandCache))
        {
            RefreshListBox(CommandCacheItems, GitModule.GitCommandCache.GetCachedCommands().Select(key => new CacheItem(key)).ToArray());
        }
    }

    private static void RefreshListBox(ListBox log, IReadOnlyList<object> dataSource)
    {
        int itemCount = log.ItemCount;
        bool isLastIndexSelected = itemCount == 0 || log.SelectedIndex == itemCount - 1;
        int lastIndex = -1;
        if (!isLastIndexSelected)
        {
            lastIndex = log.SelectedIndex;
        }

        log.ItemsSource = dataSource;

        if (dataSource.Count < 1)
        {
            return;
        }

        // select the very last item first, then select the previously selected item, if any
        log.SelectedIndex = dataSource.Count - 1;
        if (isLastIndexSelected)
        {
            log.SelectedIndex = dataSource.Count - 1;
        }
        else if (lastIndex >= 0)
        {
            log.SelectedIndex = lastIndex;
        }
    }

    private void CommandCacheItems_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (CommandCacheItems.SelectedItem is not CacheItem cacheItem)
        {
            commandCacheOutput.Text = string.Empty;
            return;
        }

        string command = cacheItem.Key;

        if (GitModule.GitCommandCache.TryGet(command, out string? cmdOut, out string? cmdErr))
        {
            commandCacheOutput.Text =
                command +
                "\n-------------------------------------\n\n" +
                PrintableChars(cmdOut) +
                "\n-------------------------------------\n\n" +
                PrintableChars(cmdErr);
        }
        else
        {
            commandCacheOutput.Text = string.Empty;
        }

        return;

        static string? PrintableChars(string? str)
        {
            if (str is null)
            {
                return str;
            }

            return str.Replace("\0", @"\0").Replace("\r", @"\r").Replace("\n", "\\n\n").Replace("\t", "\u00bb").Replace(" ", "\u00b7").Replace("\u001b", @"\x1b");
        }
    }

    private void LogItems_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (LogItems.SelectedItem is CommandLogEntry entry)
        {
            LogOutput.Text = entry.Detail;
        }
    }

    private void TabControl_SelectedIndexChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Avalonia selection events bubble from descendant lists; WinForms raised this handler only for the TabControl.
        if (!ReferenceEquals(e.Source, TabControl))
        {
            return;
        }

        RefreshLogItems();
        RefreshCommandCacheItems();
    }

    private void alwaysOnTopCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        Topmost = !Topmost;
        chkAlwaysOnTop.IsChecked = Topmost;
    }

    // Avalonia has no synchronous SaveFileDialog; route the save through the portal-backed
    // StorageProvider like the other Avalonia dialogs and keep the same output shape.
    private void mnuSaveToFile_Click(object? sender, EventArgs e)
    {
        ThreadHelper.FileAndForget(SaveToFileAsync);
    }

    private async Task SaveToFileAsync()
    {
        if (!await PortalPickerGuard.IsAvailableAsync())
        {
            return;
        }

        IStorageFile? file = await PortalPickerGuard.SaveFilePickerAsync(StorageProvider, new FilePickerSaveOptions
        {
            Title = Name,
            DefaultExtension = "txt",
            FileTypeChoices =
            [
                new FilePickerFileType("Text files (*.txt)") { Patterns = ["*.txt"] },
                new FilePickerFileType("CSV files (*.csv)") { Patterns = ["*.csv"] },
                new FilePickerFileType("All files *.*") { Patterns = ["*.*"] },
            ],
        });

        string? targetPath = file?.TryGetLocalPath();
        if (string.IsNullOrEmpty(targetPath))
        {
            return;
        }

        string separator = targetPath.EndsWith("csv") ?
            System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator :
            "\t";
        File.WriteAllLines(
            targetPath,
            CommandLog.Commands.Select(cle => cle.FullLine(separator)));
    }

    private void mnuClear_Click(object? sender, EventArgs e)
    {
        CommandLog.Clear();
    }

    private void mnuCopyCommandLine_Click(object? sender, EventArgs e)
    {
        if (LogItems.SelectedItem is CommandLogEntry commandLogEntry)
        {
            ClipboardUtil.TrySetText(commandLogEntry.CommandLine);
        }
    }

    private void tsmiClearCache_Click(object? sender, EventArgs e)
    {
        GitModule.GitCommandCache.Clear();
        RefreshCommandCacheItems();
    }

    // A closed Avalonia ContextMenu does not receive its items' shortcut gestures, so the
    // menu accelerators are matched here (Ctrl+C copy stays with the native text control).
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.S
            && ReferenceEquals(TabControl.SelectedItem, tabPageCommandLog))
        {
            mnuSaveToFile_Click(this, EventArgs.Empty);
            e.Handled = true;
        }
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.L)
        {
            if (ReferenceEquals(TabControl.SelectedItem, tabPageCommandCache))
            {
                tsmiClearCache_Click(this, EventArgs.Empty);
            }
            else
            {
                mnuClear_Click(this, EventArgs.Empty);
            }

            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    #region Single instance static members

    private static FormGitCommandLog? instance;

    public static void ShowOrActivate(WinFormsShims.IWin32Window owner)
    {
        if (instance is null)
        {
            instance = new FormGitCommandLog();
            if (owner is Window ownerWindow && ownerWindow.IsVisible)
            {
                instance.Show(ownerWindow);
            }
            else
            {
                instance.Show();
            }
        }
        else if (instance.WindowState == WindowState.Minimized)
        {
            instance.WindowState = WindowState.Normal;
        }
        else
        {
            instance.Activate();
        }
    }

    #endregion

    private sealed class CacheItem(string key)
    {
        public string Key { get; } = key;
        public string DisplayString { get; } = CommandLogEntry.GetGitArgumentsWithoutConfiguration(key);
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormGitCommandLog form)
    {
        public TabControl TabControl => form.TabControl;
        public ListBox LogItems => form.LogItems;
        public ListBox CommandCacheItems => form.CommandCacheItems;
        public TextBox LogOutput => form.LogOutput;
        public TextBox CommandCacheOutput => form.commandCacheOutput;
        public CheckBox AlwaysOnTop => form.chkAlwaysOnTop;
        public CheckBox WordWrap => form.chkWordWrap;
        public CheckBox CaptureCallStacks => form.chkCaptureCallStacks;
        public MenuItem SaveToFile => form.mnuSaveToFile;
        public MenuItem CopyCommandLine => form.mnuCopyCommandLine;
        public MenuItem ClearLog => form.mnuClear;
        public MenuItem ClearCache => form.tsmiClearCache;

        public static FormGitCommandLog? OpenInstance => instance;

        public static void Refresh(ListBox log, IReadOnlyList<object> dataSource) => RefreshListBox(log, dataSource);
    }
}
