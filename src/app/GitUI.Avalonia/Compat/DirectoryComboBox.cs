using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace GitUI.Compat;

internal sealed class DirectoryComboBox : ComboBox
{
    private readonly ListBox _directoryItems = new()
    {
        Focusable = false,
        MaxHeight = 240,
    };

    private readonly Popup _directoryPopup;
    private TextBox? _editor;
    private CancellationTokenSource? _suggestionsCancellation;
    private string? _typedPrefix;
    private bool _updatingEditor;

    public DirectoryComboBox()
    {
        // Keep the recent-repository ComboBox list separate from filesystem SuggestAppend.
        _directoryPopup = new Popup
        {
            Child = _directoryItems,
            Placement = PlacementMode.Bottom,
            IsLightDismissEnabled = true,
            OverlayDismissEventPassThrough = true,
        };
        LogicalChildren.Add(_directoryPopup);
        _directoryItems.AddHandler(PointerReleasedEvent, DirectoryItems_PointerReleased, RoutingStrategies.Bubble, handledEventsToo: true);
        DropDownOpened += (_, _) => CloseDirectorySuggestions();
        AddHandler(KeyDownEvent, DirectoryKeyDown, RoutingStrategies.Tunnel);
    }

    protected override Type StyleKeyOverride => typeof(ComboBox);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        CloseDirectorySuggestions();
        if (_editor is not null)
        {
            _editor.TextChanging -= Editor_TextChanging;
        }

        base.OnApplyTemplate(e);

        _editor = e.NameScope.Find<TextBox>("PART_EditableTextBox");
        if (_editor is not null)
        {
            _editor.TextChanging += Editor_TextChanging;
            _directoryPopup.PlacementTarget = _editor;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CloseDirectorySuggestions();
        base.OnDetachedFromVisualTree(e);
    }

    internal void SuggestDirectories()
    {
        if (_editor is { } editor)
        {
            editor.Focus();
            editor.CaretIndex = editor.Text?.Length ?? 0;
            RequestDirectorySuggestions(editor.Text ?? string.Empty, append: false);
        }
    }

    private void Editor_TextChanging(object? sender, TextChangingEventArgs e)
    {
        if (!_updatingEditor && _editor is { IsFocused: true } editor && !IsDropDownOpen)
        {
            string text = editor.Text ?? string.Empty;
            bool append = _typedPrefix?.StartsWith(text, StringComparison.Ordinal) != true;
            RequestDirectorySuggestions(text, append);
        }
    }

    private void RequestDirectorySuggestions(string text, bool append)
    {
        _suggestionsCancellation?.Cancel();
        _directoryPopup.IsOpen = false;
        _typedPrefix = text;
        if (string.IsNullOrWhiteSpace(text))
        {
            CloseDirectorySuggestions();
            return;
        }

        CancellationTokenSource request = new();
        _suggestionsCancellation = request;
        _ = PopulateDirectorySuggestionsAsync(text, append, request);
    }

    private async Task PopulateDirectorySuggestionsAsync(string text, bool append, CancellationTokenSource request)
    {
        try
        {
            CancellationToken token = request.Token;
            await Task.Delay(75, token);
            IReadOnlyList<string> suggestions = await Task.Run(() => GetDirectories(text, token), token);
            if (token.IsCancellationRequested || _editor is not { IsFocused: true } editor
                || IsDropDownOpen || editor.Text != text || !this.IsAttachedToVisualTree())
            {
                return;
            }

            if (suggestions.Count == 0)
            {
                _directoryPopup.IsOpen = false;
                return;
            }

            _directoryItems.ItemsSource = suggestions;
            _directoryItems.SelectedIndex = -1;
            if (append && editor.CaretIndex == text.Length)
            {
                string first = suggestions[0];
                StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                if (first.StartsWith(text, comparison) && first.Length > text.Length)
                {
                    SetEditorText(text + first[text.Length..], text.Length);
                    _directoryItems.SelectedIndex = 0;
                }
            }

            _directoryPopup.MinWidth = editor.Bounds.Width;
            _directoryPopup.IsOpen = true;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (ReferenceEquals(_suggestionsCancellation, request))
            {
                _suggestionsCancellation = null;
            }

            request.Dispose();
        }
    }

    private static IReadOnlyList<string> GetDirectories(string text, CancellationToken cancellationToken)
    {
        try
        {
            string leaf = Path.GetFileName(text);
            string? directory = leaf.Length == 0 ? text : Path.GetDirectoryName(text);
            string prefix = text[..^leaf.Length];
            StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            StringComparer order = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            List<string> suggestions = [];
            foreach (string entry in Directory.EnumerateDirectories(string.IsNullOrEmpty(directory) ? "." : directory))
            {
                cancellationToken.ThrowIfCancellationRequested();
                string name = Path.GetFileName(entry);
                if (name.StartsWith(leaf, comparison))
                {
                    suggestions.Add(prefix + name);
                }
            }

            suggestions.Sort(order);
            return suggestions;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return [];
        }
    }

    private void DirectoryKeyDown(object? sender, KeyEventArgs e)
    {
        if (!_directoryPopup.IsOpen || (e.KeyModifiers != KeyModifiers.None && e.Key != Key.Tab))
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Down:
                _directoryItems.SelectedIndex = Math.Min(_directoryItems.SelectedIndex + 1, _directoryItems.ItemCount - 1);
                _directoryItems.ScrollIntoView(_directoryItems.SelectedIndex);
                e.Handled = true;
                break;
            case Key.Up:
                _directoryItems.SelectedIndex = Math.Max(0, _directoryItems.SelectedIndex - 1);
                _directoryItems.ScrollIntoView(_directoryItems.SelectedIndex);
                e.Handled = true;
                break;
            case Key.Enter when _directoryItems.SelectedItem is string directory:
                SelectDirectory(directory);
                e.Handled = true;
                break;
            case Key.Escape:
                if (_typedPrefix is { } typedPrefix)
                {
                    SetEditorText(typedPrefix, typedPrefix.Length);
                }

                CloseDirectorySuggestions();
                e.Handled = true;
                break;
            case Key.Tab when _directoryItems.SelectedItem is string directory:
                SelectDirectory(directory);
                break;
            case Key.Tab:
                CloseDirectorySuggestions();
                break;
        }
    }

    private void DirectoryItems_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left
            && (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>(includeSelf: true) is { Content: string directory })
        {
            SelectDirectory(directory);
        }
    }

    private void SelectDirectory(string directory)
    {
        SetEditorText(directory, directory.Length);
        CloseDirectorySuggestions();
        _editor?.Focus();
    }

    private void SetEditorText(string text, int selectionStart)
    {
        if (_editor is not { } editor)
        {
            return;
        }

        _updatingEditor = true;
        try
        {
            SetCurrentValue(TextProperty, text);
            editor.CaretIndex = text.Length;
            editor.SelectionStart = selectionStart;
            editor.SelectionEnd = text.Length;
        }
        finally
        {
            _updatingEditor = false;
        }
    }

    private void CloseDirectorySuggestions()
    {
        _suggestionsCancellation?.Cancel();
        _typedPrefix = null;
        _directoryPopup.IsOpen = false;
    }
}
