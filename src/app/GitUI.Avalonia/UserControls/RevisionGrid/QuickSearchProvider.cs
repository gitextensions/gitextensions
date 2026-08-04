using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitUIPluginInterfaces;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI;

/// <summary>
/// Provides a 'quick search' capability to <see cref="RevisionGridControl"/> whereby the user may type directly
/// into the control in order to search for the typed word.
/// </summary>
internal sealed class QuickSearchProvider
{
    private readonly TextBlock _label;
    private readonly ListBox _gridView;
    private readonly DispatcherTimer _quickSearchTimer;
    private readonly IGitRevisionTester _gitRevisionTester;
    private readonly Border _labelHost;

    private string _lastQuickSearchString = "";
    private string _quickSearchString = "";

    public QuickSearchProvider(ListBox gridView, Panel overlay, Func<string> getWorkingDir)
    {
        _gridView = gridView;

        _gitRevisionTester = new GitRevisionTester(new FullPathResolver(getWorkingDir));

        // Avalonia renders the WinForms overlay label as a native TextBlock inside a Border.
        _label = new TextBlock
        {
            Padding = new Thickness(7, 5, 5, 5),
            FontFamily = FontFamily.Default,
            FontSize = 11,
            FontWeight = FontWeight.Bold,
        };
        _labelHost = new Border
        {
            Child = _label,
            IsVisible = false,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            IsHitTestVisible = false,
            ZIndex = 100,
        };
        _labelHost.Classes.Add("revision-quick-search");

        _quickSearchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(AppSettings.RevisionGridQuickSearchTimeout) };
        _quickSearchTimer.Tick += (sender, e) =>
        {
            _quickSearchTimer.Stop();
            HideQuickSearchString();
        };

        overlay.Children.Add(_labelHost);
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        int curIndex = _gridView.SelectedIndex;
        curIndex = curIndex >= 0 ? curIndex : 0;

        if (e.Key == Key.Back)
        {
            if (_quickSearchString.Length > 1)
            {
                // backspace
                UpdateQuickSearchString(_quickSearchString[..^1]);
            }
            else
            {
                HideQuickSearchString();
                e.Handled = false;
            }
        }
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.V && WinFormsShims.Clipboard.ContainsText())
        {
            // paste
            string text = WinFormsShims.Clipboard.GetText();
            UpdateQuickSearchString(string.Concat(_quickSearchString, text));
        }
        else if (e.Key == Key.Escape)
        {
            HideQuickSearchString();
        }

        return;

        void UpdateQuickSearchString(string newValue)
        {
            RestartQuickSearchTimer();

            _quickSearchString = newValue;

            FindNextMatch(curIndex, _quickSearchString, false);
            _lastQuickSearchString = _quickSearchString;

            e.Handled = true;
            ShowQuickSearchString();
        }
    }

    public void OnTextInput(TextInputEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Text) || e.Text.Any(char.IsControl))
        {
            HideQuickSearchString();
            e.Handled = false;
            return;
        }

        int curIndex = _gridView.SelectedIndex;
        curIndex = curIndex >= 0 ? curIndex : 0;

        RestartQuickSearchTimer();
        _quickSearchString = string.Concat(_quickSearchString, e.Text.ToLower());
        FindNextMatch(curIndex, _quickSearchString, false);
        _lastQuickSearchString = _quickSearchString;
        e.Handled = true;
        ShowQuickSearchString();
    }

    public void NextResult(bool down)
    {
        int curIndex = _gridView.SelectedIndex;

        RestartQuickSearchTimer();

        bool reverse = !down;
        int nextIndex = 0;
        if (curIndex >= 0)
        {
            nextIndex = reverse ? curIndex - 1 : curIndex + 1;
        }

        _quickSearchString = _lastQuickSearchString;
        FindNextMatch(nextIndex, _quickSearchString, reverse);
        ShowQuickSearchString();
    }

    private void ShowQuickSearchString()
    {
        _labelHost.Background = FindBrush("GitExtensionsToolTipBackgroundBrush", Brushes.LightYellow);
        _labelHost.IsVisible = true;
        _label.Text = TranslatedStrings.SearchingFor + _quickSearchString;
    }

    private void HideQuickSearchString()
    {
        _quickSearchString = "";
        _labelHost.IsVisible = false;
    }

    private void RestartQuickSearchTimer()
    {
        _quickSearchTimer.Stop();
        _quickSearchTimer.Interval = TimeSpan.FromMilliseconds(AppSettings.RevisionGridQuickSearchTimeout);
        _quickSearchTimer.Start();
    }

    private void FindNextMatch(int startIndex, string searchString, bool reverse)
    {
        _label.Foreground = FindBrush("GitExtensionsToolTipForegroundBrush", Brushes.Black);
        if (_gridView.ItemCount == 0)
        {
            return;
        }

        int? matchIndex = reverse
            ? SearchBackwards()
            : SearchForward();

        if (matchIndex.HasValue)
        {
            _label.Foreground = FindBrush("GitExtensionsToolTipForegroundBrush", Brushes.Black);

            // Prevent flickering when further typing is selecting the same row
            if (_gridView.SelectedIndex != matchIndex)
            {
                _gridView.SelectedIndex = matchIndex.Value;
                if (_gridView.Items[matchIndex.Value] is object item)
                {
                    _gridView.ScrollIntoView(item);
                }
            }
        }
        else
        {
            _label.Foreground = FindBrush("GitExtensionsErrorForegroundBrush", Brushes.DarkRed);
        }

        int? SearchForward()
        {
            // Check for out of bounds roll over if required
            int index;
            if (startIndex < 0 || startIndex >= _gridView.ItemCount)
            {
                startIndex = 0;
            }

            for (index = startIndex; index < _gridView.ItemCount; ++index)
            {
                if (_gitRevisionTester.Matches(_gridView.Items[index] as GitRevision, searchString))
                {
                    return index;
                }
            }

            // We didn't find it so start searching from the top
            for (index = 0; index < startIndex; ++index)
            {
                if (_gitRevisionTester.Matches(_gridView.Items[index] as GitRevision, searchString))
                {
                    return index;
                }
            }

            return null;
        }

        int? SearchBackwards()
        {
            // Check for out of bounds roll over if required
            int index;
            if (startIndex < 0 || startIndex >= _gridView.ItemCount)
            {
                startIndex = _gridView.ItemCount - 1;
            }

            for (index = startIndex; index >= 0; --index)
            {
                if (_gitRevisionTester.Matches(_gridView.Items[index] as GitRevision, searchString))
                {
                    return index;
                }
            }

            // We didn't find it so start searching from the bottom
            for (index = _gridView.ItemCount - 1; index > startIndex; --index)
            {
                if (_gitRevisionTester.Matches(_gridView.Items[index] as GitRevision, searchString))
                {
                    return index;
                }
            }

            return null;
        }
    }

    private IBrush FindBrush(string key, IBrush fallback)
        => _gridView.TryFindResource(key, _gridView.ActualThemeVariant, out object? value) && value is IBrush brush
            ? brush
            : fallback;
}
