using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using GitExtensions.Shims.WinForms;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.Editor;

namespace GitExtensions.Compat;

// parity-scaffolding: observes actual fileeditor commands launched by bundled and host Git inside the test Flatpak.
internal static class FileEditorConformanceProbe
{
    internal static void InstallIfRequested(IClassicDesktopStyleApplicationLifetime desktop)
    {
        string? report = Environment.GetEnvironmentVariable("GITEXTENSIONS_FILEEDITOR_CONFORMANCE_REPORT");
        string? text = Environment.GetEnvironmentVariable("GITEXTENSIONS_FILEEDITOR_CONFORMANCE_TEXT");
        if (!FlatpakEnvironment.IsFlatpak() || string.IsNullOrEmpty(report) || string.IsNullOrEmpty(text))
        {
            return;
        }

        IDisposable subscription = Window.WindowOpenedEvent.AddClassHandler<Window>((window, eventArgs) =>
        {
            if (window is FormEditor editor)
            {
                Dispatcher.UIThread.Post(() => _ = CompleteAsync(editor, report, text, desktop), DispatcherPriority.Background);
            }
        });
        desktop.Exit += (_, _) => subscription.Dispose();
    }

    private static async Task CompleteAsync(FormEditor editor, string report, string text, IClassicDesktopStyleApplicationLifetime desktop)
    {
        string? error = null;
        string? file = null;
        bool loaded = false;
        bool saved = false;
        try
        {
            file = editor.Text ?? throw new InvalidOperationException("The fileeditor did not supply its actual file name.");
            string expected = await File.ReadAllTextAsync(file);
            FileViewer viewer = editor.FindControl<FileViewer>("fileViewer")
                ?? throw new InvalidOperationException("The actual editor viewer is missing.");
            DateTime deadline = DateTime.UtcNow.AddSeconds(20);
            while (viewer.GetText() != expected && DateTime.UtcNow < deadline)
            {
                await Task.Delay(50);
            }

            loaded = viewer.GetText() == expected;
            if (!loaded)
            {
                throw new InvalidOperationException("The actual fileeditor did not load Git's file.");
            }

            viewer.GetVisualDescendants().OfType<TextEditor>().Single().Text = text;
            if (!editor.ProcessHotkey(Keys.Control | Keys.S))
            {
                throw new InvalidOperationException("The editor did not handle its original Save shortcut.");
            }

            saved = await File.ReadAllTextAsync(file) == text;
        }
        catch (Exception exception)
        {
            error = exception.ToString();
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(report))!);
        await File.WriteAllTextAsync(report, JsonSerializer.Serialize(new { file, loaded, saved, error, passed = loaded && saved && error is null },
            new JsonSerializerOptions { WriteIndented = true }) + "\n");
        if (loaded && saved && error is null)
        {
            editor.Close();
        }

        desktop.Shutdown(loaded && saved && error is null ? 0 : 1);
    }
}
