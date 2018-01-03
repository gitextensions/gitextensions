using System.Reflection;

namespace GitCommands
{
    public interface IAppInformationalVersionProvider
    {
        /// <summary>
        /// Retrieves the app's current information version.
        /// </summary>
        /// <returns>The app's current information version.</returns>
        string Get();
    }

    public sealed class AppInformationalVersionProvider : IAppInformationalVersionProvider
    {
        /// <summary>
        /// Retrieves the app's current information version;
        /// or <see langword="null"/> if <see cref="AssemblyInformationalVersionAttribute"/> is not applied.
        /// </summary>
        /// <returns>The app's current information version.</returns>
        public string Get()
        {
            var attr = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return attr?.InformationalVersion;
        }
    }
}