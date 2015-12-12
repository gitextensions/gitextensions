using System;
using System.Drawing;
using System.Windows.Forms;

using JetBrains.Annotations;

namespace GitUI.UserControls
{
	/// <summary>
	/// Uses an edit box and process output streams redirection.
	/// </summary>
	public sealed class EditboxBasedConsoleOutputControl : ConsoleOutputControl
	{
		private readonly RichTextBox _editbox;

		private ProcessOutputTimer _timer;

		public EditboxBasedConsoleOutputControl()
		{
			_timer = new ProcessOutputTimer(AppendMessage);
			_editbox = new RichTextBox {BackColor = SystemColors.Window, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill, Name = "_editbox", ReadOnly = true};
			Controls.Add(_editbox);
		}

		public override void AppendMessageFreeThreaded(string text)
		{
			if(_timer != null)
				_timer.Append(text);
		}

		public override void Done()
		{
			if(_timer != null)
				_timer.Stop(true);
		}

		public override void Reset()
		{
			_timer.Clear();
			_editbox.Text = "";
			_editbox.Visible = false;
		}

		public override void Start()
		{
			_timer.Start();
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
			if((disposing) && (_timer != null))
			{
				_timer.Dispose();
				_timer = null;
			}
			base.Dispose(disposing);
		}
	}
}