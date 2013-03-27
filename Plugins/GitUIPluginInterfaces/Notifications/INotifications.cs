using System;

namespace GitUIPluginInterfaces.Notifications
{
    /// <summary>Provides the ability to publish notifications.</summary>
    public interface INotifier
    {
        /// <summary>Publishes a notification.</summary>
        /// <param name="notification"><see cref="Notification"/> to publish.</param>
        void Notify(Notification notification);
        
        /// <summary>Gets the <see cref="INotifications"/> which contains the <see cref="INotifier"/>.</summary>
        INotifications Notifications { get; }
    }

    /// <summary>Provides access to publishing and subscribing to notifications.</summary>
    public interface INotifications
    {
        /// <summary>Gets a notifications publisher.</summary>
        INotifier Notifier { get; }
        /// <summary>Gets a notifications provider.</summary>
        IObservable<Notification> Notifications { get; }
   
        /// <summary>Gets the <see cref="IGitUICommands"/> which contains the <see cref="INotifications"/>.</summary>
        IGitUICommands UiCommands { get; }
    }
}
