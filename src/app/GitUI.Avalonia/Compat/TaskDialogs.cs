using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

// Avalonia stand-ins for the WinForms task-dialog object model, limited to the members
// the ported forms use, so upstream TaskDialog code ports mechanically: a page with
// heading/text, regular buttons (including an explicit default), command links,
// a verification checkbox, and a footnote.
// The one intentional call-site change: WinForms identifies the owner by handle
// (TaskDialog.ShowDialog(Handle, page)); here the owner is passed directly
// (TaskDialog.ShowDialog(this, page)).

public class TaskDialogButton
{
    public static TaskDialogButton OK { get; } = new("OK");
    public static TaskDialogButton Yes { get; } = new("Yes");
    public static TaskDialogButton No { get; } = new("No");
    public static TaskDialogButton Cancel { get; } = new("Cancel");

    public TaskDialogButton(string? text = null)
    {
        Text = text;
    }

    public string? Text { get; set; }

    public event EventHandler? Click;

    internal void PerformClick() => Click?.Invoke(this, EventArgs.Empty);
}

public sealed class TaskDialogCommandLinkButton : TaskDialogButton
{
    public TaskDialogCommandLinkButton(string? text = null)
        : base(text)
    {
    }
}

public sealed class TaskDialogVerificationCheckBox
{
    public string? Text { get; set; }

    public bool Checked { get; set; }
}

public enum TaskDialogIcon
{
    None = 0,
    Information,
    Warning,
    Error
}

public sealed class TaskDialogPage
{
    public string? Text { get; set; }

    public string? Heading { get; set; }

    public string? Caption { get; set; }

    public TaskDialogIcon Icon { get; set; }

    public List<TaskDialogButton> Buttons { get; } = [];

    public TaskDialogButton? DefaultButton { get; set; }

    public TaskDialogVerificationCheckBox? Verification { get; set; }

    public string? Footnote { get; set; }

    public bool AllowCancel { get; set; }

    public bool SizeToContent { get; set; }
}

public static class TaskDialog
{
    /// <summary>
    ///  Shows the page modally and returns the button that closed it (like the WinForms
    ///  <c>TaskDialog.ShowDialog</c>; closing the window counts as Cancel).
    /// </summary>
    public static TaskDialogButton ShowDialog(WinFormsShims.IWin32Window? owner, TaskDialogPage page)
    {
        return DispatcherPump.Wait(ShowDialogAsync);

        async Task<TaskDialogButton> ShowDialogAsync()
        {
            Window? ownerWindow = owner as Window
                ?? (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

            Window dialog = new()
            {
                Title = page.Caption ?? string.Empty,
                Icon = Properties.Images.ApplicationIcon,
                SizeToContent = Avalonia.Controls.SizeToContent.WidthAndHeight,
                CanResize = false,
                WindowStartupLocation = ownerWindow is null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
                MaxWidth = 700,
            };

            TaskDialogButton result = TaskDialogButton.Cancel;

            StackPanel content = new()
            {
                Spacing = 12,
            };

            if (!string.IsNullOrEmpty(page.Heading))
            {
                content.Children.Add(new SelectableTextBlock
                {
                    Text = page.Heading,
                    FontWeight = FontWeight.Bold,
                    FontSize = 16,
                    TextWrapping = TextWrapping.Wrap,
                });
            }

            if (!string.IsNullOrEmpty(page.Text))
            {
                content.Children.Add(new SelectableTextBlock
                {
                    Text = page.Text,
                    TextWrapping = TextWrapping.Wrap,
                });
            }

            // WinForms renders command links as a stack of wide buttons above the
            // regular button row.
            foreach (TaskDialogButton pageButton in page.Buttons.Where(button => button is TaskDialogCommandLinkButton))
            {
                Button commandLink = CreateButton(pageButton, minWidth: 320, HorizontalAlignment.Stretch);
                commandLink.HorizontalContentAlignment = HorizontalAlignment.Left;
                content.Children.Add(commandLink);
            }

            CheckBox? verification = null;
            if (page.Verification is not null)
            {
                verification = new CheckBox
                {
                    Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(page.Verification.Text ?? string.Empty),
                    IsChecked = page.Verification.Checked,
                };
                content.Children.Add(verification);
            }

            if (!string.IsNullOrEmpty(page.Footnote))
            {
                content.Children.Add(new SelectableTextBlock
                {
                    Text = page.Footnote,
                    Opacity = 0.7,
                    TextWrapping = TextWrapping.Wrap,
                });
            }

            StackPanel buttonPanel = new()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
            };

            bool isFirst = true;
            foreach (TaskDialogButton pageButton in page.Buttons.Where(button => button is not TaskDialogCommandLinkButton))
            {
                Button button = CreateButton(pageButton, minWidth: 80, HorizontalAlignment.Right);
                button.IsDefault = page.DefaultButton is null
                    ? isFirst
                    : ReferenceEquals(pageButton, page.DefaultButton);
                button.IsCancel = pageButton == TaskDialogButton.Cancel || pageButton == TaskDialogButton.No;
                isFirst = false;
                buttonPanel.Children.Add(button);
            }

            if (buttonPanel.Children.Count > 0)
            {
                content.Children.Add(buttonPanel);
            }

            StackPanel root = new()
            {
                Margin = new Avalonia.Thickness(20),
                Orientation = Orientation.Horizontal,
                Spacing = 16,
            };
            Control? pageIcon = DialogIconFactory.Create(page.Icon);
            if (pageIcon is not null)
            {
                root.Children.Add(pageIcon);
            }

            root.Children.Add(content);
            dialog.Content = root;

            if (ownerWindow is not null && ownerWindow != dialog && ownerWindow.IsVisible)
            {
                await dialog.ShowDialog(ownerWindow);
            }
            else
            {
                // No owner window yet: emulate the modal loop by waiting for the close.
                TaskCompletionSource closed = new();
                dialog.Closed += (_, _) => closed.TrySetResult();
                dialog.Show();
                await closed.Task;
            }

            if (page.Verification is not null && verification is not null)
            {
                page.Verification.Checked = verification.IsChecked == true;
            }

            return result;

            Button CreateButton(TaskDialogButton pageButton, double minWidth, HorizontalAlignment alignment)
            {
                Button button = new()
                {
                    Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(pageButton.Text ?? string.Empty),
                    MinWidth = minWidth,
                    HorizontalAlignment = alignment,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                };
                button.Click += (_, _) =>
                {
                    result = pageButton;
                    pageButton.PerformClick();
                    dialog.Close();
                };
                return button;
            }
        }
    }
}
