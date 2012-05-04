// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsUITests.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Tests.Functional
{
	using System;
	using System.IO;
	using System.Windows.Forms;

	using NBug.Core.Reporting.Info;
	using NBug.Core.UI;
	using NBug.Core.Util;
	using NBug.Core.Util.Serialization;
	using NBug.Enums;
	using NBug.Tests.Tools.Extensions;
	using NBug.Tests.Tools.Fixtures;

	using Xunit;

	public class SettingsUITests : IUseFixture<SettingsFixture>, IUseFixture<UIFixture>, IUseFixture<ReportFixture>
	{
		private Report report;

		private SerializableException serializableException;

		[Fact, UI]
		public void ExceptionThread_Task()
		{
			// By default UI is not shown for task exceptions
			Assert.Equal(
				UISelector.DisplayBugReportUI(ExceptionThread.Task, this.serializableException, this.report), 
				new UIDialogResult(ExecutionFlow.ContinueExecution, SendReport.Send));

			Assert.Equal(
				MessageBox.Show(
					"ExceptionThread: Task" + Environment.NewLine + Environment.NewLine +
					"Did you see any UI on the screen?", 
					"Assert Question", 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question), 
				DialogResult.No);
		}

		public void SetFixture(SettingsFixture settings)
		{
			// Reset settings before each run
			// settings.ReloadDefaults();
		}

		public void SetFixture(UIFixture ui)
		{
		}

		public void SetFixture(ReportFixture reportFixture)
		{
			this.serializableException = reportFixture.SerializableException;
			this.report = reportFixture.Report;
		}

		[Fact, UI]
		public void UIMode_AutoExceptionThread_Main()
		{
			// Auto mode displays minimal UI by default
			Settings.UIMode = UIMode.Auto;

			// By default UIFixture loads System.Windows.Forms so this should not behave as console but WinForms UI
			Assert.Equal(
				UISelector.DisplayBugReportUI(ExceptionThread.Main, this.serializableException, this.report), 
				new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send));

			Assert.Equal(
				MessageBox.Show(
					"UIMode: Auto" + Environment.NewLine +
					"ExceptionThread: Main" + Environment.NewLine +
					"Loaded Assembly: System.Windows.Forms" + Environment.NewLine + Environment.NewLine +
					"Did you see a Minimal WinForms message box?", 
					"Assert Question", 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question), 
				DialogResult.Yes);
		}

		[Fact, UI]
		public void UIMode_AutoExceptionThread_WPF()
		{
			// Auto mode displays minimal UI by default
			Settings.UIMode = UIMode.Auto;

			// UIMode:Auto and exception on WPF thread
			Assert.Equal(
				UISelector.DisplayBugReportUI(ExceptionThread.UI_WPF, this.serializableException, this.report), 
				new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send));

			Assert.Equal(
				MessageBox.Show(
					"UIMode: Auto" + Environment.NewLine +
					"ExceptionThread: WPF" + Environment.NewLine + Environment.NewLine +
					"Did you see a Minimal WPF message box?", 
					"Assert Question", 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question), 
				DialogResult.Yes);
		}

		[Fact, UI]
		public void UIMode_AutoExceptionThread_WinForms()
		{
			// Auto mode displays minimal UI by default
			Settings.UIMode = UIMode.Auto;

			// UIMode:Auto and exception on WinForms thread
			Assert.Equal(
				UISelector.DisplayBugReportUI(ExceptionThread.UI_WinForms, this.serializableException, this.report), 
				new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send));

			Assert.Equal(
				MessageBox.Show(
					"UIMode: Auto" + Environment.NewLine +
					"ExceptionThread: WinForms" + Environment.NewLine + Environment.NewLine +
					"Did you see a Minimal WinForms message box?", 
					"Assert Question", 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question), 
				DialogResult.Yes);
		}

		[Fact, UI]
		public void UIMode_None()
		{
			Settings.UIMode = UIMode.None;

			Assert.Equal(
				UISelector.DisplayBugReportUI(ExceptionThread.UI_WinForms, this.serializableException, this.report), 
				new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send));

			Assert.Equal(
				MessageBox.Show(
					"ExceptionThread: Task" + Environment.NewLine + Environment.NewLine +
					"Did you see any UI on the screen?", 
					"Assert Question", 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question), 
				DialogResult.No);
		}

		[Fact, UI]
		public void UIProvider_AutoUIMode_Minimal()
		{
			Settings.UIProvider = UIProvider.Auto;
			Settings.UIMode = UIMode.Minimal;

			Assert.Equal(
				UISelector.DisplayBugReportUI(ExceptionThread.UI_WinForms, this.serializableException, this.report), 
				new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send));

			// By default, Visual Studio loads some WinForms dlls so WinForms dialogs will be displayed via auto selection
			Assert.Equal(
				MessageBox.Show(
					"UIProvider: Auto" + Environment.NewLine +
					"UIMode: Minimal" + Environment.NewLine + Environment.NewLine +
					"Did you see a Minimal WinForms message box?", 
					"Assert Question", 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question), 
				DialogResult.Yes);
		}

		[Fact, UI]
		public void UIProvider_AutoUIMode_Normal()
		{
			Settings.UIProvider = UIProvider.Auto;
			Settings.UIMode = UIMode.Normal;

			MessageBox.Show("Now click 'Quit'", "User Input Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
			Assert.Equal(
				UISelector.DisplayBugReportUI(ExceptionThread.Main, this.serializableException, this.report), 
				new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send));

			MessageBox.Show("Now click 'Continue'", "User Input Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
			Assert.Equal(
				UISelector.DisplayBugReportUI(ExceptionThread.Main, this.serializableException, this.report), 
				new UIDialogResult(ExecutionFlow.ContinueExecution, SendReport.Send));

			// By default, Visual Studio loads some WinForms dlls so WinForms dialogs will be displayed via auto selection
			Assert.Equal(
				MessageBox.Show(
					"UIProvider: Auto" + Environment.NewLine +
					"UIMode: Normal" + Environment.NewLine + Environment.NewLine +
					"Did you see a Normal WinForms error dialog?", 
					"Assert Question", 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question), 
				DialogResult.Yes);
		}

		[Fact, UI]
		public void UIProvider_ConsoleUIMode_Full()
		{
			Settings.UIProvider = UIProvider.Console;
			Settings.UIMode = UIMode.Full;

			var consoleOut = new StringWriter();
			Console.SetOut(consoleOut);

			Assert.Equal(
				UISelector.DisplayBugReportUI(ExceptionThread.Main, this.serializableException, this.report), 
				new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send));

			Assert.Equal(
				MessageBox.Show(
					"UIProvider: Console" + Environment.NewLine +
					"UIMode: Full" + Environment.NewLine + Environment.NewLine +
					"Console Message: " + consoleOut + Environment.NewLine +
					"Is the message written to the console right?", 
					"Assert Question", 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question), 
				DialogResult.Yes);
		}

		[Fact, UI]
		public void UIProvider_ConsoleUIMode_Minimal()
		{
			Settings.UIProvider = UIProvider.Console;
			Settings.UIMode = UIMode.Minimal;

			var consoleOut = new StringWriter();
			Console.SetOut(consoleOut);
			
			Assert.Equal(
				UISelector.DisplayBugReportUI(ExceptionThread.Main, this.serializableException, this.report), 
				new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send));

			Assert.Equal(
				MessageBox.Show(
					"UIProvider: Console" + Environment.NewLine +
					"UIMode: Minimal" + Environment.NewLine + Environment.NewLine +
					"Console Message: " + consoleOut + Environment.NewLine +
					"Is the message written to the console right?", 
					"Assert Question", 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question), 
				DialogResult.Yes);
		}

		[Fact, UI]
		public void UIProvider_ConsoleUIMode_Normal()
		{
			Settings.UIProvider = UIProvider.Console;
			Settings.UIMode = UIMode.Normal;

			var consoleOut = new StringWriter();
			Console.SetOut(consoleOut);

			Assert.Equal(
				UISelector.DisplayBugReportUI(ExceptionThread.Main, this.serializableException, this.report), 
				new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send));

			Assert.Equal(
				MessageBox.Show(
					"UIProvider: Console" + Environment.NewLine +
					"UIMode: Normal" + Environment.NewLine + Environment.NewLine +
					"Console Message: " + consoleOut + Environment.NewLine +
					"Is the message written to the console right?", 
					"Assert Question", 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question), 
				DialogResult.Yes);
		}

		[Fact, UI]
		public void UIProvider_WinFormsUIMode_Full()
		{
			Settings.UIProvider = UIProvider.WinForms;
			Settings.UIMode = UIMode.Full;

			MessageBox.Show("Now click 'Send and Quit'", "User Input Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
			Assert.Equal(
				UISelector.DisplayBugReportUI(ExceptionThread.Main, this.serializableException, this.report), 
				new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send));

			MessageBox.Show("Now click 'Quit'", "User Input Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
			Assert.Equal(
				UISelector.DisplayBugReportUI(ExceptionThread.Main, this.serializableException, this.report), 
				new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.DoNotSend));

			Assert.Equal(
				MessageBox.Show(
					"UIProvider: WinForms" + Environment.NewLine +
					"UIMode: Full" + Environment.NewLine + Environment.NewLine +
					"Did you see a Full WinForms error dialog?", 
					"Assert Question", 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question), 
				DialogResult.Yes);
		}

		[Fact, UI]
		public void UIProvider_WinFormsUIMode_Minimal()
		{
			Settings.UIProvider = UIProvider.WinForms;
			Settings.UIMode = UIMode.Minimal;

			Assert.Equal(
				UISelector.DisplayBugReportUI(ExceptionThread.UI_WinForms, this.serializableException, this.report), 
				new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send));

			Assert.Equal(
				MessageBox.Show(
					"UIProvider: WinForms" + Environment.NewLine +
					"UIMode: Minimal" + Environment.NewLine + Environment.NewLine +
					"Did you see a Minimal WinForms message box?", 
					"Assert Question", 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question), 
				DialogResult.Yes);
		}

		[Fact, UI]
		public void UIProvider_WinFormsUIMode_Normal()
		{
			Settings.UIProvider = UIProvider.WinForms;
			Settings.UIMode = UIMode.Normal;

			MessageBox.Show("Now click 'Quit'", "User Input Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
			Assert.Equal(
				UISelector.DisplayBugReportUI(ExceptionThread.Main, this.serializableException, this.report), 
				new UIDialogResult(ExecutionFlow.BreakExecution, SendReport.Send));

			MessageBox.Show("Now click 'Continue'", "User Input Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
			Assert.Equal(
				UISelector.DisplayBugReportUI(ExceptionThread.Main, this.serializableException, this.report), 
				new UIDialogResult(ExecutionFlow.ContinueExecution, SendReport.Send));

			Assert.Equal(
				MessageBox.Show(
					"UIProvider: WinForms" + Environment.NewLine +
					"UIMode: Normal" + Environment.NewLine + Environment.NewLine +
					"Did you see a Normal WinForms error dialog?", 
					"Assert Question", 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question), 
				DialogResult.Yes);
		}
	}
}
