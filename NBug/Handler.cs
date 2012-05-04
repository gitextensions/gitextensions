// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Handler.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug
{
	using System;
	using System.Runtime.ExceptionServices;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Windows.Threading;

	using NBug.Core.Reporting;
	using NBug.Core.UI;
	using NBug.Core.Util;
	using NBug.Core.Util.Logging;

	public static class Handler
	{
		static Handler()
		{
			// Submit any queued reports on a seperate thread asynchronously, while exceptions handlers are being set);
			if (!Settings.SkipDispatching)
			{
				new Core.Submission.Dispatcher(Settings.DispatcherIsAsynchronous);
			}
		}

		// Using delegates to make sure that static constructor gets called on delegate access

		/// <summary>
		/// Used for handling general exceptions bound to the main thread.
		/// Handles the <see cref="AppDomain.UnhandledException"/> events in <see cref="System"/> namespace.
		/// </summary>
		public static UnhandledExceptionEventHandler UnhandledException
		{
			get
			{
				if (Settings.HandleProcessCorruptedStateExceptions)
				{
					return CorruptUnhandledExceptionHandler;
				}
				else
				{
					return UnhandledExceptionHandler;
				}
			}
		}

		/// <summary>
		/// Used for handling WinForms exceptions bound to the UI thread.
		/// Handles the <see cref="Application.ThreadException"/> events in <see cref="System.Windows.Forms"/> namespace.
		/// </summary>
		public static ThreadExceptionEventHandler ThreadException
		{
			get
			{
				if (Settings.HandleProcessCorruptedStateExceptions)
				{
					return CorruptThreadExceptionHandler;
				}
				else
				{
					return ThreadExceptionHandler;
				}
			}
		}

		/// <summary>
		/// Used for handling WPF exceptions bound to the UI thread.
		/// Handles the <see cref="Application.DispatcherUnhandledException"/> events in <see cref="System.Windows"/> namespace.
		/// </summary>
		public static DispatcherUnhandledExceptionEventHandler DispatcherUnhandledException
		{
			get
			{
				if (Settings.HandleProcessCorruptedStateExceptions)
				{
					return CorruptDispatcherUnhandledExceptionHandler;
				}
				else
				{
					return DispatcherUnhandledExceptionHandler;
				}
			}
		}

		/// <summary>
		/// Used for handling System.Threading.Tasks bound to a background worker thread.
		/// Handles the <see cref="UnobservedTaskException"/> event in <see cref="System.Threading.Tasks"/> namespace.
		/// </summary>
		public static EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException
		{
			get
			{
				if (Settings.HandleProcessCorruptedStateExceptions)
				{
					return CorruptUnobservedTaskExceptionHandler;
				}
				else
				{
					return UnobservedTaskExceptionHandler;
				}
			}
		}

		/// <summary>
		/// Used for handling general exceptions bound to the main thread.
		/// Handles the <see cref="AppDomain.UnhandledException"/> events in <see cref="System"/> namespace.
		/// </summary>
		/// <param name="sender">Exception sender object.</param>
		/// <param name="e">Real exception is in: ((Exception)e.ExceptionObject)</param>
		private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
		{
			if (Settings.HandleExceptions)
			{
				Logger.Trace("Starting to handle a System.AppDomain.UnhandledException.");
				var executionFlow = new BugReport().Report((Exception)e.ExceptionObject, ExceptionThread.Main);
				if (executionFlow == ExecutionFlow.BreakExecution)
				{
					Environment.Exit(0);
				} 
			}
		}

		[HandleProcessCorruptedStateExceptions]
		private static void CorruptUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
		{
			UnhandledExceptionHandler(sender, e);
		}

		/// <summary>
		/// Used for handling WinForms exceptions bound to the UI thread.
		/// Handles the <see cref="Application.ThreadException"/> events in <see cref="System.Windows.Forms"/> namespace.
		/// </summary>
		/// <param name="sender">Exception sender object.</param>
		/// <param name="e">Real exception is in: e.Exception</param>
		private static void ThreadExceptionHandler(object sender, ThreadExceptionEventArgs e)
		{
			if (Settings.HandleExceptions)
			{
				Logger.Trace("Starting to handle a System.Windows.Forms.Application.ThreadException.");

				// WinForms UI thread exceptions do not propagate to more general handlers unless: Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
				var executionFlow = new BugReport().Report(e.Exception, ExceptionThread.UI_WinForms);
				if (executionFlow == ExecutionFlow.BreakExecution)
				{
					Environment.Exit(0);
				} 
			}
		}

		[HandleProcessCorruptedStateExceptions]
		private static void CorruptThreadExceptionHandler(object sender, ThreadExceptionEventArgs e)
		{
			ThreadExceptionHandler(sender, e);
		}

		/// <summary>
		/// Used for handling WPF exceptions bound to the UI thread.
		/// Handles the <see cref="Application.DispatcherUnhandledException"/> events in <see cref="System.Windows"/> namespace.
		/// </summary>
		/// <param name="sender">Exception sender object</param>
		/// <param name="e">Real exception is in: e.Exception</param>
		private static void DispatcherUnhandledExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			if (Settings.HandleExceptions)
			{
				Logger.Trace("Starting to handle a System.Windows.Application.DispatcherUnhandledException.");
				var executionFlow = new BugReport().Report(e.Exception, ExceptionThread.UI_WPF);
				if (executionFlow == ExecutionFlow.BreakExecution)
				{
					e.Handled = true;
					Environment.Exit(0);
				}
				else if (executionFlow == ExecutionFlow.ContinueExecution)
				{
					e.Handled = true;
				} 
			}
		}

		[HandleProcessCorruptedStateExceptions]
		private static void CorruptDispatcherUnhandledExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			DispatcherUnhandledExceptionHandler(sender, e);
		}

		/// <summary>
		/// Used for handling System.Threading.Tasks bound to a background worker thread.
		/// Handles the <see cref="UnobservedTaskException"/> event in <see cref="System.Threading.Tasks"/> namespace.
		/// </summary>
		/// <param name="sender">Exception sender object.</param>
		/// <param name="e">Real exception is in: e.Exception.</param>
		private static void UnobservedTaskExceptionHandler(object sender, UnobservedTaskExceptionEventArgs e)
		{
			if (Settings.HandleExceptions)
			{
				Logger.Trace("Starting to handle a System.Threading.Tasks.UnobservedTaskException.");
				var executionFlow = new BugReport().Report(e.Exception, ExceptionThread.Task);
				if (executionFlow == ExecutionFlow.BreakExecution)
				{
					e.SetObserved();
					Environment.Exit(0);
				}
				else if (executionFlow == ExecutionFlow.ContinueExecution)
				{
					e.SetObserved();
				} 
			}
		}

		[HandleProcessCorruptedStateExceptions]
		private static void CorruptUnobservedTaskExceptionHandler(object sender, UnobservedTaskExceptionEventArgs e)
		{
			UnobservedTaskExceptionHandler(sender, e);
		}
	}
}
