﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using GitCommands;

using JetBrains.Annotations;

namespace GitUI.UserControls
{
	/// <summary>
	/// Uses an edit box and process output streams redirection.
	/// </summary>
	public sealed class EditboxBasedConsoleOutputControl : ConsoleOutputControl
	{
		private readonly RichTextBox _editbox;

		private int _exitcode;

		private Process _process;

		private ProcessOutputTimer _timer;

		public EditboxBasedConsoleOutputControl()
		{
			_timer = new ProcessOutputTimer(AppendMessage);
			_editbox = new RichTextBox {BackColor = SystemColors.Window, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, Name = "_editbox", ReadOnly = true};
			Controls.Add(_editbox);
		}

		public override int ExitCode
		{
			get
			{
				return _exitcode;
			}
		}

		public override bool IsDisplayingFullProcessOutput
		{
			get
			{
				return false;
			}
		}

		public override void AppendMessageFreeThreaded(string text)
		{
			if(_timer != null)
				_timer.Append(text);
		}

		public override void KillProcess()
		{
			if(InvokeRequired)
				throw new InvalidOperationException("This operation is to be executed on the home thread.");
			if(_process == null)
				return;
			try
			{
				_process.TerminateTree();
			}
			catch(Exception ex)
			{
				Trace.WriteLine(ex);
			}
			_process = null;
			FireProcessExited();
		}

		public override void Reset()
		{
			_timer.Clear();
			_editbox.Text = "";
			_editbox.Visible = false;
		}

		public override void StartProcess(string command, string arguments, string workdir)
		{
			try
			{
				_timer.Start();

				GitCommandHelpers.SetEnvironmentVariable();

				bool ssh = GitCommandHelpers.UseSsh(arguments);

				KillProcess();

				string quotedCmd = command;
				if(quotedCmd.IndexOf(' ') != -1)
					quotedCmd = quotedCmd.Quote();

				DateTime executionStartTimestamp = DateTime.Now;

				//process used to execute external commands
				var process = new Process();
				ProcessStartInfo startInfo = GitCommandHelpers.CreateProcessStartInfo(command, arguments, workdir, GitModule.SystemEncoding);
				startInfo.CreateNoWindow = (!ssh && !AppSettings.ShowGitCommandLine);
				process.StartInfo = startInfo;

				process.EnableRaisingEvents = true;
				process.OutputDataReceived += (sender, args) => FireDataReceived(new TextEventArgs(args.Data ?? ""));
				process.ErrorDataReceived += (sender, args) => FireDataReceived(new TextEventArgs(args.Data ?? ""));
				process.Exited += delegate
				{
					if(!IsDisposed)
					{
						Invoke(new Action(() =>
						{
							if(_process == null)
								return;
							// From GitCommandsInstance:
							//The process is exited already, but this command waits also until all output is received.
							//Only WaitForExit when someone is connected to the exited event. For some reason a
							//null reference is thrown sometimes when staging/unstaging in the commit dialog when
							//we wait for exit, probably a timing issue... 
							try
							{
								_process.WaitForExit();
							}
							catch
							{
								// NOP
							}
							_exitcode = _process.ExitCode;
							_process = null;
							_timer.Stop(true);
							FireProcessExited();
						}));
					}
				};

				process.Exited += (sender, args) =>
				{
					DateTime executionEndTimestamp = DateTime.Now;
					AppSettings.GitLog.Log(quotedCmd + " " + arguments, executionStartTimestamp, executionEndTimestamp);
				};

				process.Start();
				_process = process;
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
			}
			catch(Exception ex)
			{
				ex.Data.Add("command", command);
				ex.Data.Add("arguments", arguments);
				throw;
			}
		}

		private void AppendMessage([NotNull] string text)
		{
			if(text == null)
				throw new ArgumentNullException("text");
			if(InvokeRequired)
				throw new InvalidOperationException("This operation must be called on the GUI thread.");
			//if not disposed
			if(!IsDisposed)
			{
				_editbox.Text += text;
				_editbox.SelectionStart = _editbox.Text.Length;
				_editbox.ScrollToCaret();
				_editbox.Visible = true;
			}
		}

		protected override void Dispose(bool disposing)
		{
			KillProcess();
			if((disposing) && (_timer != null))
			{
				_timer.Dispose();
				_timer = null;
			}
			base.Dispose(disposing);
		}
	}
}