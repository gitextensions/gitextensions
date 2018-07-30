using System;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace GitUI
{
    public static class TaskbarProgress
    {
        private static void Try(Action<TaskbarManager> action)
        {
            if (GitCommands.Utils.EnvUtils.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
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
                taskbar.SetProgressState(TaskbarProgressBarState.Normal);
                taskbar.SetProgressValue(progressValue, maximumValue);
            });
        }

        public static void SetIndeterminate()
        {
            Try(taskbar => taskbar.SetProgressState(TaskbarProgressBarState.Indeterminate));
        }
    }
}