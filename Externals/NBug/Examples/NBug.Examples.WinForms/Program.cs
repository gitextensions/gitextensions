// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Examples.WinForms
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;

	using NBug.Properties;

	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main()
		{
			// Check to see if test application is initialized by the configurator tool
			if (Environment.GetCommandLineArgs().Count() > 1)
			{
				var stream = new FileStream(Environment.GetCommandLineArgs()[1], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				SettingsOverride.LoadCustomSettings(stream);
			}

			// For demonstrational purposes only, normally this should be left with it's default value as false!
			Settings.HandleProcessCorruptedStateExceptions = true;

			// Sample NBug configuration for WinForms applications
			AppDomain.CurrentDomain.UnhandledException += Handler.UnhandledException;
			Application.ThreadException += Handler.ThreadException;
			TaskScheduler.UnobservedTaskException += Handler.UnobservedTaskException;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}