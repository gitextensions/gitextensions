using System;

namespace GitUIPluginInterfaces.Notifications
{
    /// <summary>Represents a contextual action which is related to a specific <see cref="Notification"/>.</summary>
    public class ContextAction
    {
        /// <summary>Creates a new <see cref="ContextAction"/>.</summary>
        /// <param name="text">The text of the context action.</param>
        /// <param name="action">The action to execute.</param>
        /// <param name="help">The (ToolTip) text which may provide additional insight into the action.</param>
        public ContextAction(string text, Action action, string help)
        {
            Help = help;
            Action = action;
            Text = text;
        }

        /// <summary>Gets the text of the context action.</summary>
        public string Text { get; private set; }
        /// <summary>Gets the action to execute.</summary>
        public Action Action { get; private set; }
        /// <summary>Gets the (ToolTip) text which may provide additional insight into the action.</summary>
        public string Help { get; private set; }
    }
}
