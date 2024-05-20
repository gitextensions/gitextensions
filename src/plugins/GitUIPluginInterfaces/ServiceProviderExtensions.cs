using System.ComponentModel.Design;

namespace GitUIPluginInterfaces;

/// <summary>
/// Extension methods for getting services from an <see cref="IServiceProvider" />.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Adds the specified service of type <typeparamref name="T"/> to the <see cref="IServiceContainer"/>.
    /// </summary>
    /// <typeparam name="T">The type of service object to add.</typeparam>
    /// <param name="container">The <see cref="IServiceContainer"/> to add the service object to.</param>
    public static void AddService<T>(this IServiceContainer container, T serviceInstance) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(serviceInstance);

        container.AddService(typeof(T), serviceInstance);
    }

    /// <summary>
    /// Get service of type <typeparamref name="T"/> from the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <param name="provider">The <see cref="IServiceProvider"/> to retrieve the service object from.</param>
    /// <returns>A service object of type <typeparamref name="T"/> or null if there is no such service.</returns>
    public static T? GetService<T>(this IServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        return (T?)provider.GetService(typeof(T));
    }

    /// <summary>
    /// Get service of type <typeparamref name="T"/> from the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <param name="provider">The <see cref="IServiceProvider"/> to retrieve the service object from.</param>
    /// <returns>A service object of type <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">There is no service of type <typeparamref name="T"/>.</exception>
    public static T GetRequiredService<T>(this IServiceProvider provider) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(provider);

        object? service = provider.GetService(typeof(T));
        if (service is null)
        {
            throw new InvalidOperationException($"No service for type '{typeof(T)}' has been registered.");
        }

        return (T)service;
    }

    /// <summary>
    /// Removes the specified service of type <typeparamref name="T"/> from the <see cref="IServiceContainer"/>.
    /// </summary>
    /// <typeparam name="T">The type of service object to add.</typeparam>
    public static void RemoveService<T>(this IServiceContainer container) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(container);

        container.RemoveService(typeof(T));
    }
}
