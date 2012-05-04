namespace NBug.Examples.Console
{
	using System;
	using System.IO;

	public class Program
	{
		public static void Main(string[] args)
		{
			// Check to see if test application is initialized by the configurator tool
			if (args.Length > 0)
			{
				FileStream stream = new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				NBug.Properties.SettingsOverride.LoadCustomSettings(stream);
			}

			// Sample NBug configuration for console applications
			AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
			System.Threading.Tasks.TaskScheduler.UnobservedTaskException += NBug.Handler.UnobservedTaskException;

			Console.WriteLine("NBug now auto-handles: AppDomain.CurrentDomain.UnhandledException");
			Console.WriteLine("NBug now auto-handles: Threading.Tasks.TaskScheduler.UnobservedTaskException");
			Console.WriteLine(Environment.NewLine);
			Console.Write("Generate a System.Exception (y/n): ");

			if (Console.ReadKey().Key == ConsoleKey.Y)
			{
				throw new Exception("This is an exception thrown from NBug console sample application.");
			}
		}
	}
}
