using Avalonia.Controls;
using Microsoft.VisualStudio.Threading;

namespace GitUI.Compat;

/// <summary>
/// Creates an isolated task manager when a view is hosted by the Avalonia designer.
/// </summary>
public static class DesignTimeTaskManager
{
    /// <summary>
    /// Creates a task manager without requiring the application bootstrap in design mode.
    /// </summary>
    public static TaskManager Create()
    {
        // The previewer constructs views without running the entry point that initializes ThreadHelper.
        return Design.IsDesignMode
            ? new TaskManager(new JoinableTaskContext())
            : ThreadHelper.CreateTaskManager();
    }
}
