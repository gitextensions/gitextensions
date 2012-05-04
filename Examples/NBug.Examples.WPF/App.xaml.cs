namespace NBug.Examples.WPF
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Windows;

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			// Check to see if test application is initialized by the configurator tool
			if (Environment.GetCommandLineArgs().Count() > 1)
			{
				FileStream stream = new FileStream(Environment.GetCommandLineArgs()[1], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				NBug.Properties.SettingsOverride.LoadCustomSettings(stream);
			}

			// For demonstrational purposes only, normally this should be left with it's default value as false!
			NBug.Settings.HandleProcessCorruptedStateExceptions = true;

			// Sample NBug configuration for WPF applications
			AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
			Application.Current.DispatcherUnhandledException += NBug.Handler.DispatcherUnhandledException;
			System.Threading.Tasks.TaskScheduler.UnobservedTaskException += NBug.Handler.UnobservedTaskException;
		}
	}
}
