// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Examples.WPF
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows;

	using NBug.Properties;

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
				var stream = new FileStream(Environment.GetCommandLineArgs()[1], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				SettingsOverride.LoadCustomSettings(stream);
			}

			// For demonstrational purposes only, normally this should be left with it's default value as false!
			Settings.HandleProcessCorruptedStateExceptions = true;

			// Sample NBug configuration for WPF applications
			AppDomain.CurrentDomain.UnhandledException += Handler.UnhandledException;
			Current.DispatcherUnhandledException += Handler.DispatcherUnhandledException;
			TaskScheduler.UnobservedTaskException += Handler.UnobservedTaskException;
		}
	}
}