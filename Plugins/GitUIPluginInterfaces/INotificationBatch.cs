namespace GitUIPluginInterfaces
{
    /// <summary>Provides the ability to notify the user about a batch process.</summary>
    public interface INotificationBatch
    {
        /// <summary>Notifies the user about the next update in the batch.</summary>
        void Next(Notification notification);
        /// <summary>Notifies the user about the last update in the batch.</summary>
        void Last(Notification notification);
    }
}
