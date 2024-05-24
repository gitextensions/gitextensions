using GitCommands.Utils;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace GitUI
{
    public static class TaskbarProgress
    {
        private static void Try(Action<TaskbarManager> action)
        {
            if (EnvUtils.RunningOnWindowsWithMainWindow() && TaskbarManager.IsPlatformSupported)
            {
                try
                {
                    action(TaskbarManager.Instance);
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        public static void Clear()
        {
            Try(taskbar => taskbar.SetProgressState(TaskbarProgressBarState.NoProgress));
        }

        public static void SetProgress(TaskbarProgressBarState state, int progressValue, int maximumValue)
        {
            Try(taskbar =>
            {
                taskbar.SetProgressState(state);
                taskbar.SetProgressValue(progressValue, maximumValue);
            });
        }

        public static void SetState(TaskbarProgressBarState state)
        {
            Try(taskbar => taskbar.SetProgressState(state));
        }
    }
}
