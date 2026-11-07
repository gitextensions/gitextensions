using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using BugReporter;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.Compat;
using GitUI.NBugReports;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class BugReportFormTests
{
    [AvaloniaTest]
    public void AddTranslationItems_should_preserve_the_original_image_button_keys()
    {
        using AppSettingsTestScope settings = new();
        using TranslationTestScope translation = new();
        using BugReportForm form = new();
        ITranslation collector = Substitute.For<ITranslation>();

        try
        {
            form.Show();
            form.UpdateLayout();
            form.AddTranslationItems(collector);

            collector.Received(1).AddTranslationItem(nameof(BugReportForm), "btnCopy", "Text", "&Copy");
            collector.Received(1).AddTranslationItem(nameof(BugReportForm), "sendAndQuitButton", "Text", "&Send and Quit");
            foreach (string name in new[] { "btnCopy", "sendAndQuitButton" })
            {
                form.FindControl<Button>(name)!.GetVisualDescendants().OfType<Image>().Single().Source.Should().NotBeNull();
            }
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [TestCase("send", false)]
    [TestCase("quit", false)]
    [TestCase("ignore", false)]
    [TestCase("quit", true)]
    [TestCase("programmatic", true)]
    [TestCase("quit-external", true)]
    [TestCase("details", false)]
    public void Report_should_supply_the_original_form_and_actions(string action, bool terminating)
    {
        using AppSettingsTestScope settings = new();
        using TranslationTestScope translation = new();
        FieldInfo shellField = typeof(WinFormsShims.ShimHost).GetField("_osShell", BindingFlags.Static | BindingFlags.NonPublic)!;
        FieldInfo messageField = typeof(WinFormsShims.ShimHost).GetField("_messageBoxHost", BindingFlags.Static | BindingFlags.NonPublic)!;
        object? oldShell = shellField.GetValue(null);
        object? oldMessages = messageField.GetValue(null);
        var oldPresenter = UIReporter.TestAccessor.TaskDialogPresenter;
        var oldEnvironment = UIReporter.TestAccessor.EnvironmentInformationProvider;
        var oldExit = UIReporter.TestAccessor.ExitApplication;
        WinFormsShims.IOsShell shell = Substitute.For<WinFormsShims.IOsShell>();
        IClipboard? reportClipboard = null;
        string? oldClipboardText = null;
        BugReportForm? activeReport = null;
        int detailCount = 0;
        WinFormsShims.IMessageBoxHost messages = Substitute.For<WinFormsShims.IMessageBoxHost>();
        shell.TryLaunch(Arg.Any<string>(), WinFormsShims.OsShellLaunchKind.OpenUri).Returns(true);
        messages.Show(Arg.Any<WinFormsShims.IWin32Window?>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<WinFormsShims.MessageBoxButtons>(), Arg.Any<WinFormsShims.MessageBoxIcon>(),
            Arg.Any<WinFormsShims.MessageBoxDefaultButton>()).Returns(WinFormsShims.DialogResult.Yes);
        List<int> exits = [];
        Exception? failure = null;
        DispatcherTimer deadline = new() { Interval = TimeSpan.FromSeconds(20) };
        deadline.Tick += (_, _) =>
        {
            deadline.Stop();
            failure = new AssertionException($"The report action '{action}' did not complete.");
            if (activeReport is not null)
            {
                activeReport.DialogResult = WinFormsShims.DialogResult.Abort;
                activeReport.Close();
            }
        };
        int formCount = 0;
        Exception reported = action switch
        {
            "ignore" or "quit-external" => new IOException("failure details"),
            "details" => new UserExternalOperationException(new InvalidOperationException("failure details")),
            _ => new InvalidOperationException("failure details"),
        };
        using IDisposable observer = Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    if (window is BugReportForm form)
                    {
                        formCount++;
                        activeReport = form;
                        form.GetTestAccessor().ExceptionTextBox.Text.Should().Be(reported.GetType().FullName);
                        form.GetTestAccessor().ExceptionMessageTextBox.Text.Should().Be(reported.Message);
                        form.GetTestAccessor().ApplicationTextBox.Text.Should().Contain("Git Extensions");
                        form.GetTestAccessor().ClrTextBox.Text.Should().Contain(".NET");
                        form.GetTestAccessor().ExceptionDetails.GetTestAccessor().ExceptionDetailsListView.Items
                            .OfType<KeyValuePair<string, string>>().Should().Contain(pair => pair.Key == "Message" && pair.Value == reported.Message);
                        foreach (string name in new[] { "btnCopy", "sendAndQuitButton", "quitButton" })
                        {
                            ToolTip.GetShowDelay(form.FindControl<Button>(name)!).Should().Be(100);
                        }

                        Button ignore = form.FindControl<Button>("IgnoreButton")!;
                        bool external = action is "ignore" or "quit-external" or "details";
                        ignore.IsVisible.Should().Be(external);
                        ignore.IsEnabled.Should().Be(external && !terminating);
                        form.FindControl<Button>("sendAndQuitButton")!.IsEnabled.Should().Be(action != "details");
                        form.FindControl<TabControl>("mainTabs")!.SelectedItem.Should().BeSameAs(
                            form.FindControl<TabItem>(action == "details" ? "exceptionTabPage" : "generalTabPage"));
                        if (terminating)
                        {
                            RequestNativeClose(form).Should().BeTrue();
                            form.IsVisible.Should().BeTrue("fatal reports cannot be dismissed through the title bar");
                            if (action == "programmatic")
                            {
                                form.Close();
                                form.IsVisible.Should().BeFalse("the original ControlBox setting does not veto host shutdown");
                                return;
                            }
                        }

                        TextBox description = form.FindControl<TextBox>("descriptionTextBox")!;
                        if (action == "send")
                        {
                            Click(form, form.FindControl<Button>("sendAndQuitButton")!);
                            shell.DidNotReceive().TryLaunch(Arg.Any<string>(), Arg.Any<WinFormsShims.OsShellLaunchKind>());
                            messages.Received(1).Show(form, Arg.Is<string>(text => text.Contains("Please provide")), Arg.Any<string>(),
                                WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error, Arg.Any<WinFormsShims.MessageBoxDefaultButton>());
                            form.IsVisible.Should().BeTrue();
                            description.Text = "Open the repository, then click Refresh.";
                            reportClipboard = form.Clipboard;
                            reportClipboard.Should().NotBeNull();
                            oldClipboardText = DispatcherPump.Wait(() => reportClipboard!.TryGetTextAsync());
                            Click(form, form.FindControl<Button>("btnCopy")!);
                            string? copied = DispatcherPump.Wait(() => reportClipboard!.TryGetTextAsync());
                            copied.Should().Contain("failure details").And.Contain(description.Text).And.Contain("OS: test");
                            form.IsVisible.Should().BeTrue();
                            messages.Show(Arg.Any<WinFormsShims.IWin32Window?>(), Arg.Any<string>(), Arg.Any<string>(),
                                WinFormsShims.MessageBoxButtons.YesNo, Arg.Any<WinFormsShims.MessageBoxIcon>(),
                                Arg.Any<WinFormsShims.MessageBoxDefaultButton>()).Returns(WinFormsShims.DialogResult.No, WinFormsShims.DialogResult.Yes);
                            Click(form, form.FindControl<Button>("sendAndQuitButton")!);
                            shell.DidNotReceive().TryLaunch(Arg.Any<string>(), Arg.Any<WinFormsShims.OsShellLaunchKind>());
                            form.IsVisible.Should().BeTrue();
                            Click(form, form.FindControl<Button>("sendAndQuitButton")!);
                        }
                        else
                        {
                            if (action == "details")
                            {
                                ListBox details = form.GetTestAccessor().ExceptionDetails.GetTestAccessor().ExceptionDetailsListView;
                                form.UpdateLayout();
                                Control row = details.ContainerFromIndex(1)!;
                                Point point = row.TranslatePoint(new Point(row.Bounds.Width / 2, row.Bounds.Height / 2), form)!.Value;
                                form.MouseDown(point, Avalonia.Input.MouseButton.Left);
                                form.MouseUp(point, Avalonia.Input.MouseButton.Left);
                                form.MouseDown(point, Avalonia.Input.MouseButton.Left);
                                form.MouseUp(point, Avalonia.Input.MouseButton.Left);
                            }

                            Click(form, form.FindControl<Button>(action == "ignore" ? "IgnoreButton" : "quitButton")!);
                        }
                    }
                    else if (window is ExceptionDetailView)
                    {
                        detailCount++;
                        window.Owner.Should().BeSameAs(activeReport);
                        window.FindControl<TextBox>("propertyTextBox")!.Text.Should().Be("Message");
                        window.FindControl<TextBox>("propertyInformationTextBox")!.Text.Should().Be(reported.Message);
                        window.Close();
                    }
                    else if (window.Classes.Contains("gitextensions-task-dialog"))
                    {
                        string caption = action switch
                        {
                            "ignore" or "quit-external" => GitUI.TranslatedStrings.ReportIssue,
                            "details" => GitUI.TranslatedStrings.ButtonViewDetails,
                            _ => GitUI.TranslatedStrings.ButtonReportBug,
                        };
                        Click(window, window.GetVisualDescendants().OfType<Button>().Single(button =>
                            button.Classes.Contains("gitextensions-command-link")
                            && button.GetVisualDescendants().OfType<AccessText>().Any(text =>
                                text.Text == AvaloniaTranslationUtils.ToAvaloniaMnemonics(caption))));
                    }
                }
                catch (Exception exception)
                {
                    failure = exception;
                    if (window is BugReportForm reportForm)
                    {
                        reportForm.DialogResult = WinFormsShims.DialogResult.Abort;
                    }

                    window.Close();
                }
            });
        });
        try
        {
            WinFormsShims.ShimHost.OsShell = shell;
            WinFormsShims.ShimHost.MessageBoxHost = messages;
            UIReporter.TestAccessor.TaskDialogPresenter = TaskDialog.ShowDialog;
            UIReporter.TestAccessor.EnvironmentInformationProvider = () => "OS: test";
            UIReporter.TestAccessor.ExitApplication = exits.Add;

            deadline.Start();
            BugReportInvoker.Report(reported, terminating);

            failure.Should().BeNull();
            formCount.Should().Be(1);
            detailCount.Should().Be(action == "details" ? 1 : 0);
            exits.Should().HaveCount(action == "ignore" ? 0 : 1);
            if (action == "send")
            {
                shell.Received(1).TryLaunch(Arg.Is<string>(url =>
                    url.StartsWith("https://github.com/begota98/gitextensions/issues/new?")
                    && Uri.UnescapeDataString(url).Contains("Open the repository, then click Refresh.")), WinFormsShims.OsShellLaunchKind.OpenUri);
            }
        }
        finally
        {
            deadline.Stop();
            observer.Dispose();
            UIReporter.TestAccessor.TaskDialogPresenter = oldPresenter;
            UIReporter.TestAccessor.EnvironmentInformationProvider = oldEnvironment;
            UIReporter.TestAccessor.ExitApplication = oldExit;
            shellField.SetValue(null, oldShell);
            messageField.SetValue(null, oldMessages);
            if (reportClipboard is not null)
            {
                DispatcherPump.Wait(async () =>
                {
                    if (oldClipboardText is null)
                    {
                        await reportClipboard.ClearAsync();
                    }
                    else
                    {
                        await reportClipboard.SetTextAsync(oldClipboardText);
                    }

                    return true;
                });
            }
        }
    }

    private static bool RequestNativeClose(Window window)
    {
        PropertyInfo closing = typeof(Avalonia.Platform.IWindowImpl).GetProperty("Closing", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("The platform close callback could not be found.");
        Func<WindowCloseReason, bool> requestClose = (Func<WindowCloseReason, bool>?)closing.GetValue(window.PlatformImpl)
            ?? throw new InvalidOperationException("The window has no platform close callback.");
        return requestClose(WindowCloseReason.WindowClosing);
    }

    private static void Click(Window window, Button button)
    {
        window.UpdateLayout();
        Point point = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;
        window.MouseMove(point);
        window.MouseDown(point, Avalonia.Input.MouseButton.Left);
        window.MouseUp(point, Avalonia.Input.MouseButton.Left);
    }
}
