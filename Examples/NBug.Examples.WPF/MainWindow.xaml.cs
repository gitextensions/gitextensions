// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Examples.WPF
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows;

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			this.InitializeComponent();

			this.crashTypeComboBox.SelectedIndex = 0;
		}

		private unsafe void AccessViolation()
		{
			var b = *(byte*)8762765876;
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void CrashButton_Click(object sender, RoutedEventArgs e)
		{
			switch (this.crashTypeComboBox.Text)
			{
				case "UI Thread: System.Exception":
					throw new Exception("Selected exception: '" + this.crashTypeComboBox.Text + "' was thrown.");
				case "UI Thread: System.ArgumentException":
					throw new ArgumentException("Selected exception: '" + this.crashTypeComboBox.Text + "' was thrown.", "MyInvalidParameter");
				case "Background Thread (Task): System.Exception":
					Task.Factory.StartNew(() => { throw new Exception(); });

					// Below code makes sure that exception is thrown as only after finalization, the aggregateexception is thrown.
					// As a side affect, unlike the normal behavior, the applicaiton will note continue its execution but will shut
					// down just like any main thread exceptions, even if there is no handle to UnobservedTaskException!
					// So remove below 3 lines to observe the normal continuation behavior.
					Thread.Sleep(200);
					GC.Collect();
					GC.WaitForPendingFinalizers();
					break;
				case "Process Corrupted State Exception: Access Violation":
					Settings.HandleProcessCorruptedStateExceptions = true;
					this.AccessViolation();
					break;
			}
		}
	}
}