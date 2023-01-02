using System.Diagnostics;
using System.Runtime.InteropServices;
using GitCommands;
using GitExtUtils;
using GitUI.NBugReports;
using GitUIPluginInterfaces;

namespace GitUI.Infrastructure
{
    public static class PuttyHelpers
    {
        /// <summary>
        ///  Starts PuTTY agent for the specified working directory.
        /// </summary>
        /// <param name="workingDirectory">The working directory.</param>
        /// <exception cref="UserExternalOperationException">PuTTY executable cannot be found.</exception>
        /// <exception cref="COMException">The method was not called on the UI thread.</exception>
        public static void StartPageant(string workingDirectory)
        {
            // Exceptions must be thrown from the UI thread, otherwise we'll crash the app.
            ThreadHelper.ThrowIfNotOnUIThread();

            ThrowIfFileNotFound(AppSettings.Pageant, $"'{AppSettings.Pageant}'\r\n\r\n{TranslatedStrings.ErrorSshPuTTYInstalled}");

            new Executable(AppSettings.Pageant, workingDirectory).Start();
        }

        /// <summary>
        ///  Starts PuTTY key generator for the specified working directory.
        /// </summary>
        /// <param name="workingDirectory">The working directory.</param>
        /// <exception cref="UserExternalOperationException">PuTTY executable cannot be found.</exception>
        /// <exception cref="COMException">The method was not called on the UI thread.</exception>
        public static void StartPuttygen(string workingDirectory)
        {
            // Exceptions must be thrown from the UI thread, otherwise we'll crash the app.
            ThreadHelper.ThrowIfNotOnUIThread();

            ThrowIfFileNotFound(AppSettings.Puttygen, $"'{AppSettings.Puttygen}'\r\n\r\n{TranslatedStrings.ErrorSshPuTTYInstalled}");

            new Executable(AppSettings.Puttygen, workingDirectory).Start();
        }

        /// <summary>
        ///  Starts PuTTY agent with the specified SSH key.
        /// </summary>
        /// <param name="sshKeyFileLoader">The delegate that provides the key to load.</param>
        /// <returns><see langword="true"/> if PuTTY agent was started successfully; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="UserExternalOperationException">
        ///  <para>PuTTY isn't configured is preferred SSH client.</para>
        ///  <para>- or -</para>
        ///  <para>PuTTY executable cannot be found.</para>
        ///  <para>- or -</para>
        ///  <para>The SSH key cannot be found.</para>
        /// </exception>
        /// <exception cref="COMException">The method was not called on the UI thread.</exception>
        public static bool StartPageantIfConfigured(Func<string?> sshKeyFileLoader)
        {
            // Exceptions must be thrown from the UI thread, otherwise we'll crash the app.
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!GitSshHelpers.IsPlink)
            {
                throw new UserExternalOperationException(TranslatedStrings.ErrorSshPuTTYWhereConfigure,
                    new ExternalOperationException(innerException: new Exception(TranslatedStrings.ErrorSshPuTTYNotConfigured)));
            }

            ThrowIfFileNotFound(AppSettings.Pageant, $"'{AppSettings.Pageant}'\r\n\r\n{TranslatedStrings.ErrorSshPuTTYInstalled}");

            Executable pageantExecutable = new(AppSettings.Pageant);

            // ensure pageant is loaded, so we can wait for loading a key in the next command
            // otherwise we'll stuck there waiting until pageant exits
            if (!IsPageantRunning())
            {
                // NOTE we leave the process to dangle here
                IProcess process = pageantExecutable.Start();
                process.WaitForInputIdle();
            }

            string? sshKeyFile = sshKeyFileLoader();
            if (string.IsNullOrWhiteSpace(sshKeyFile))
            {
                return false;
            }

            ThrowIfFileNotFound(sshKeyFile, $"'{sshKeyFile}'", TranslatedStrings.ErrorSshKeyNotFound);

            return pageantExecutable.RunCommand(sshKeyFile.Quote());

            static bool IsPageantRunning()
            {
                var pageantProcName = Path.GetFileNameWithoutExtension(AppSettings.Pageant);
                return Process.GetProcessesByName(pageantProcName).Length != 0;
            }
        }

        private static void ThrowIfFileNotFound(string filePath, string errorMessage, string? heading = null)
        {
            if (!File.Exists(filePath))
            {
                throw new UserExternalOperationException(errorMessage,
                    new ExternalOperationException(innerException: new FileNotFoundException(heading ?? TranslatedStrings.ErrorFileNotFound)));
            }
        }
    }
}
