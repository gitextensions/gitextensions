using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands.Git;
using GitUI.Properties;
using GitUIPluginInterfaces.Notifications;

namespace GitUI.Notifications
{
    /// <summary>Provides helpful members for notifications.</summary>
    internal static class NotifierHelpers
    {
        /// <summary>blank icon</summary>
        internal static string blank = Guid.NewGuid().ToString();
        /// <summary>info icon</summary>
        internal static string info = Guid.NewGuid().ToString();
        /// <summary>success icon</summary>
        internal static string success = Guid.NewGuid().ToString();
        /// <summary>warning icon</summary>
        internal static string warn = Guid.NewGuid().ToString();
        /// <summary>failure icon</summary>
        internal static string fail = Guid.NewGuid().ToString();

        static readonly ImageList images;

        static NotifierHelpers()
        {
            images = new ImageList();
            images.Images.Add(blank, Resources.BlankIcon);
            images.Images.Add(success, Resources.NotifySuccess);
            images.Images.Add(warn, Resources.NotifyWarn); // add when needed
            images.Images.Add(fail, Resources.NotifyError);
            images.Images.Add(info, Resources.NotifyInfo);
        }

        /// <summary>Gets the image key for a <see cref="Notification"/>.</summary>
        public static string GetImageKey(this Notification notification)
        {
            switch (notification.Severity)
            {
                case StatusSeverity.Info: return info;
                case StatusSeverity.Success: return success;
                case StatusSeverity.Warn: return warn;
                case StatusSeverity.Fail: return fail;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>Resets a <see cref="Timer"/>.</summary>
        public static void Reset(this Timer timer)
        {
            timer.Stop();
            timer.Start();
        }

        /// <summary>Sets the <see cref="ImageList"/> for the specified <see cref="ToolStrip"/>.</summary>
        public static void SetImageList(this ToolStrip toolStrip)
        {
            if (toolStrip == null) { return; }

            if (toolStrip.ImageList == null)
            {
                toolStrip.ImageList = images;
            }
        }

        static Dictionary<ToolStripDropDownItem, Queue<ScheduledAction>> scheduledActions
            = new Dictionary<ToolStripDropDownItem, Queue<ScheduledAction>>();

        class ScheduledAction
        {
            public Action<ToolStripItemCollection, ToolStripItem> action;
            public ToolStripItem item;
        }

        /// <summary>Removes a <see cref="ToolStripItem"/> from the drop-down when it's closed.</summary>
        public static void Removal(this ToolStripDropDownItem dropDown, ToolStripItem item)
        {
            dropDown.Schedule(item, (items, child) =>
            {
                items.Remove(child);
                child.Dispose();
            });
        }

        /// <summary>Inserts a <see cref="ToolStripItem"/> into the drop-down when it's closed.</summary>
        public static void Insertion(this ToolStripDropDownItem dropDown, ToolStripItem item)
        {
            dropDown.Schedule(item, (items, child) => items.Insert(0, child));
        }

        /// <summary>Schedules execution of the specified action, when the drop-down is closed.</summary>
        public static void Schedule(this ToolStripDropDownItem dropDown, ToolStripItem item, Action<ToolStripItemCollection, ToolStripItem> action)
        {
            if (dropDown.DropDown.Visible)
            {// visible
                if (scheduledActions.ContainsKey(dropDown) == false)
                {
                    scheduledActions[dropDown] = new Queue<ScheduledAction>();
                }
                scheduledActions[dropDown].Enqueue(new ScheduledAction { action = action, item = item });
                dropDown.DropDownClosed += OnDropDownClosed;
            }
            else
            {
                dropDown.DropDown.Enabled = false;
                action(dropDown.DropDownItems, item);
                dropDown.DropDown.Enabled = true;
            }
        }

        static void OnDropDownClosed(object sender, EventArgs e)
        {
            ToolStripDropDownItem dropDown = (ToolStripDropDownItem)sender;
            Queue<ScheduledAction> actions = scheduledActions[dropDown];
            while (actions.Any())
            {
                ScheduledAction scheduledAction = actions.Dequeue();
                scheduledAction.action(dropDown.DropDownItems, scheduledAction.item);
            }
        }

        /// <summary>Executes an action on the specified <see cref="Control"/>'s thread.</summary>
        public static void Invoke(this Control control, Action action)
        {
            control.Invoke(action);
        }

        /// <summary>Depending on a git command's result, publishes a notification.</summary>
        /// <param name="notifier">Notifier to publish to.</param>
        /// <param name="result">Result of the git command.</param>
        /// <param name="successNotification">Notification to publish if successful.</param>
        /// <param name="failNotification">Notification to publish if failed.</param>
        public static void NotifyIf(this INotifier notifier,
            GitCommandResult result,
            Func<Notification> successNotification,
            Func<Notification> failNotification)
        {
            if (result.WasSuccessful && successNotification != null)
            {// successful AND success notification -> notify
                notifier.Notify(successNotification());
            }
            else if (result.WasSuccessful == false && failNotification != null)
            {// failed AND fail notification -> notify
                notifier.Notify(failNotification());
            }
        }
    }
}