namespace GitUIPluginInterfaces
{
    /// <summary>Provides the ability to notify the user.</summary>
    public interface INotifier
    {
        /// <summary>Notifies the user about a status update.</summary>
        /// <param name="notification">Status update to show to user.</param>
        void Notify(Notification notification);
        /// <summary>Gets a notification batch.</summary>
        INotificationBatch StartBatch();
    }
}