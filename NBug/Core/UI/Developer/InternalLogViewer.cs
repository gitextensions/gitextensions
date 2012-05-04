// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InternalLogViewer.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.UI.Developer
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;

	using NBug.Enums;
	using NBug.Properties;

	internal partial class InternalLogViewer : Form
	{
		private static bool initialized;
		private static bool closed;
		private static InternalLogViewer viewer;
		private static ManualResetEvent handleCreated;

		internal InternalLogViewer()
		{
			this.InitializeComponent();
			this.Icon = Resources.NBug_icon_16;
			this.notifyIcon.Icon = Resources.NBug_icon_16;
		}

		public static void InitializeInternalLogViewer()
		{
			if (!initialized)
			{
				initialized = true;
				viewer = new InternalLogViewer();
				handleCreated = new ManualResetEvent(false);
				viewer.HandleCreated += (sender, e) => handleCreated.Set();
				Task.Factory.StartNew(() => Application.Run(viewer));
				handleCreated.WaitOne();
			}
		}

		public static void LogEntry(string message, LoggerCategory category)
		{
			InitializeInternalLogViewer();

			if (!closed)
			{
				viewer.Invoke((MethodInvoker)(() => viewer.InternalLogEntry(message, category)));
			}
		}

		internal void InternalLogEntry(string message, LoggerCategory category)
		{
			this.loggerListView.Items.Add(new ListViewItem(new[] { category.ToString().Remove(0, 4), DateTime.Now.ToString("HH:mm:ss"), message }));
		}

		private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized)
			{
				this.WindowState = FormWindowState.Normal;
			}

			this.Show();
			this.Activate();
		}

		private void LoggerListView_Click(object sender, EventArgs e)
		{
			this.detailsTextBox.Text = this.loggerListView.SelectedItems[0].SubItems[2].Text;
		}

		private void QuitButton_Click(object sender, EventArgs e)
		{
			this.notifyIcon.Visible = false;
			this.notifyIcon.Dispose();
			this.notifyIcon = null;
			closed = true;
			this.Close();
		}

		private void HideButton_Click(object sender, EventArgs e)
		{
			this.Hide();
		}

		private void InternalLogViewer_Resize(object sender, EventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized)
			{
				this.Hide();
			}
		}
	}
}