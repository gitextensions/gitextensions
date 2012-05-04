// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainForm.cs" company="NBusy Project">
//   Copyright © 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Configurator
{
	/* Dear maintainer:
	 * 
	 * Once you are done trying to 'optimize' this file, and have realized what a terrible mistake that was,
	 * please increment the following counter as a warning to the next guy:
	 * 
	 * total_hours_wasted_here = 20
	 */

	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Windows.Forms;

	using NBug.Configurator.SubmitPanels;
	using NBug.Enums;

	public partial class MainForm : Form
	{
		private FileStream settingsFile;

		public MainForm()
		{
			InitializeComponent();

			this.openFileDialog.InitialDirectory = Environment.CurrentDirectory;
			this.createFileDialog.InitialDirectory = Environment.CurrentDirectory;
		}

		/// <summary>
		/// Enables disabled controls and fills in combo boxes using related enumerations. Resets all values to defaults.
		/// </summary>
		private void InitializeControls()
		{
			this.uiModeComboBox.Items.Clear();
			foreach (UIMode value in Enum.GetValues(typeof(UIMode)))
			{
				this.uiModeComboBox.Items.Add(value);
			}

			this.uiProviderComboBox.Items.Clear();
			foreach (UIProvider value in Enum.GetValues(typeof(UIProvider)))
			{
				this.uiProviderComboBox.Items.Add(value);
			}

			this.miniDumpTypeComboBox.Items.Clear();
			foreach (MiniDumpType value in Enum.GetValues(typeof(MiniDumpType)))
			{
				this.miniDumpTypeComboBox.Items.Add(value);
			}

			this.storagePathComboBox.Items.Clear();
			foreach (StoragePath value in Enum.GetValues(typeof(StoragePath)))
			{
				this.storagePathComboBox.Items.Add(value);
			}

			this.panelLoader1.UnloadPanel();
			this.panelLoader2.UnloadPanel();
			this.panelLoader3.UnloadPanel();
			this.panelLoader4.UnloadPanel();
			this.panelLoader5.UnloadPanel();
			
			this.sleepBeforeSendNumericUpDown.Maximum = decimal.MaxValue;
			this.maxQueuedReportsNumericUpDown.Maximum = decimal.MaxValue;
			this.stopReportingAfterNumericUpDown.Maximum = decimal.MaxValue;

			this.mainTabs.Enabled = true;
			this.mainTabs.SelectedIndex = 0;
			this.runTestAppButton.Enabled = true;
			this.saveButton.Enabled = true;
		}

		/// <summary>
		/// Loads the settings file or loads defaults is settings file is empty or invalid.
		/// </summary>
		/// <param name="createNew">Force creating of new file. Overrides existing file by default.</param>
		private void LoadSettingsFile(bool createNew)
		{
			this.settingsFile.Position = 0;
			NBug.Properties.SettingsOverride.LoadCustomSettings(this.settingsFile);
			this.InitializeControls();

			this.fileTextBox.Text = createNew == false ? this.openFileDialog.FileName : this.createFileDialog.FileName;
			
			// Read application settings
			this.uiProviderComboBox.SelectedItem = Settings.UIProvider;
			this.uiModeComboBox.SelectedItem = Settings.UIMode; // Should come after uiProviderComboBox = ...
			this.miniDumpTypeComboBox.SelectedItem = Settings.MiniDumpType;
			this.sleepBeforeSendNumericUpDown.Value = Settings.SleepBeforeSend;
			this.maxQueuedReportsNumericUpDown.Value = Settings.MaxQueuedReports;
			this.stopReportingAfterNumericUpDown.Value = Settings.StopReportingAfter;
			this.writeLogToDiskCheckBox.Checked = Settings.WriteLogToDisk;
			this.exitApplicationImmediatelyCheckBox.Checked = Settings.ExitApplicationImmediately;
			this.handleProcessCorruptedStateExceptionsCheckBox.Checked = Settings.HandleProcessCorruptedStateExceptions;
			this.releaseModeCheckBox.Checked = Settings.ReleaseMode;

			if (Settings.StoragePath == StoragePath.Custom)
			{
				this.storagePathComboBox.SelectedItem = StoragePath.Custom;
				this.customStoragePathTextBox.Text = Settings.StoragePath;
			}
			else
			{
				// Make sure that we're getting the enum value
				this.storagePathComboBox.SelectedItem = (StoragePath)Settings.StoragePath;
			}

			if (Settings.Cipher != null && Settings.Cipher.Length != 0)
			{
				this.encryptConnectionStringsCheckBox.Checked = true;
			}

			if (this.settingsFile.Name.EndsWith("app.config"))
			{
				this.writeNetworkTraceToFileCheckBox.Enabled = true;
				this.networkTraceWarningLabel.Enabled = true;

				if (Settings.EnableNetworkTrace.HasValue)
				{
					this.writeNetworkTraceToFileCheckBox.Checked = Settings.EnableNetworkTrace.Value;
				}
			}

			// Read connection strings)
			if (!string.IsNullOrEmpty(Settings.Destination1))
			{
				this.panelLoader1.LoadPanel(Settings.Destination1);
			}

			if (!string.IsNullOrEmpty(Settings.Destination2))
			{
				this.panelLoader2.LoadPanel(Settings.Destination2);
			}

			if (!string.IsNullOrEmpty(Settings.Destination3))
			{
				this.panelLoader3.LoadPanel(Settings.Destination3);
			}

			if (!string.IsNullOrEmpty(Settings.Destination4))
			{
				this.panelLoader4.LoadPanel(Settings.Destination4);
			}

			if (!string.IsNullOrEmpty(Settings.Destination5))
			{
				this.panelLoader5.LoadPanel(Settings.Destination5);
			}
		}

		private void SaveButton_Click(object sender, EventArgs e)
		{
			// Validate user provide settings
			if (string.IsNullOrEmpty(this.uiProviderComboBox.Text))
			{
				MessageBox.Show(
					"The 'User Interface > UI Provider' selection should not be left blank. Please select a value for the provider or set the UI Mode to Auto.",
					"User Interface Provider is Left Blank",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				this.uiProviderComboBox.Focus();
				return;
			}

			Settings.EnableNetworkTrace = this.writeNetworkTraceToFileCheckBox.Checked; // This should come before settings app config

			// Save application settings
			Settings.UIMode = (UIMode)this.uiModeComboBox.SelectedItem;
			Settings.UIProvider = (UIProvider)this.uiProviderComboBox.SelectedItem;
			Settings.MiniDumpType = (MiniDumpType)this.miniDumpTypeComboBox.SelectedItem;
			Settings.SleepBeforeSend = Convert.ToInt32(this.sleepBeforeSendNumericUpDown.Value);
			Settings.MaxQueuedReports = Convert.ToInt32(this.maxQueuedReportsNumericUpDown.Value);
			Settings.StopReportingAfter = Convert.ToInt32(this.stopReportingAfterNumericUpDown.Value);
			Settings.WriteLogToDisk = this.writeLogToDiskCheckBox.Checked;
			Settings.HandleProcessCorruptedStateExceptions = this.handleProcessCorruptedStateExceptionsCheckBox.Checked;
			Settings.ReleaseMode = this.releaseModeCheckBox.Checked;

			if ((UIMode)this.uiModeComboBox.SelectedItem == UIMode.None)
			{
				Settings.ExitApplicationImmediately = this.exitApplicationImmediatelyCheckBox.Checked;
			}

			Settings.StoragePath = this.storagePathComboBox.Text == "Custom" ? this.customStoragePathTextBox.Text : this.storagePathComboBox.Text;

			// Save connection strings)
			if (this.panelLoader1.Controls.Count == 2)
			{
				var str = ((ISubmitPanel)this.panelLoader1.Controls[0]).ConnectionString;
				if (string.IsNullOrEmpty(str))
				{
					return;
				}
				else
				{
					Settings.Destination1 = str;
				}
			}

			if (this.panelLoader2.Controls.Count == 2)
			{
				var str = ((ISubmitPanel)this.panelLoader2.Controls[0]).ConnectionString;
				if (string.IsNullOrEmpty(str))
				{
					return;
				}
				else
				{
					Settings.Destination2 = str;
				}
			}

			if (this.panelLoader3.Controls.Count == 2)
			{
				var str = ((ISubmitPanel)this.panelLoader3.Controls[0]).ConnectionString;
				if (string.IsNullOrEmpty(str))
				{
					return;
				}
				else
				{
					Settings.Destination3 = str;
				}
			}

			if (this.panelLoader4.Controls.Count == 2)
			{
				var str = ((ISubmitPanel)this.panelLoader4.Controls[0]).ConnectionString;
				if (string.IsNullOrEmpty(str))
				{
					return;
				}
				else
				{
					Settings.Destination4 = str;
				}
			}

			if (this.panelLoader5.Controls.Count == 2)
			{
				var str = ((ISubmitPanel)this.panelLoader5.Controls[0]).ConnectionString;
				if (string.IsNullOrEmpty(str))
				{
					return;
				}
				else
				{
					Settings.Destination5 = str;
				}
			}

			this.settingsFile.Position = 0;
			NBug.Properties.SettingsOverride.SaveCustomSettings(this.settingsFile, this.encryptConnectionStringsCheckBox.Checked);
			this.status.Text = "Configuration file successfully saved. Please test your configuration.";
		}

		private void SaveChangesWarning()
		{
			if (this.settingsFile != null)
			{
				// ToDo: display some sort of a warning for user to save the changes or roll back
				this.settingsFile.Close();
			}
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.SaveChangesWarning();
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void StoragePathComboBox_SelectedValueChanged(object sender, EventArgs e)
		{
			if ((StoragePath)this.storagePathComboBox.SelectedItem == StoragePath.Custom)
			{
				this.customStoragePathTextBox.Enabled = true;
			}
			else
			{
				this.customStoragePathTextBox.Enabled = false;
				this.customStoragePathTextBox.Text = string.Empty;
			}
		}

		private void CreateButton_Click(object sender, EventArgs e)
		{
			if (this.createFileDialog.ShowDialog() == DialogResult.OK)
			{
				this.SaveChangesWarning();
				this.settingsFile = new FileStream(this.createFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
				this.LoadSettingsFile(true);
			}
		}

		private void OpenButton_Click(object sender, EventArgs e)
		{
			if (this.openFileDialog.ShowDialog() == DialogResult.OK)
			{
				this.SaveChangesWarning();
				File.SetAttributes(this.openFileDialog.FileName, FileAttributes.Normal);
				this.settingsFile = new FileStream(this.openFileDialog.FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
				this.LoadSettingsFile(false);
			}
		}

		private void RunTestAppButton_Click(object sender, EventArgs e)
		{
			this.SaveButton_Click(this, null);

			string testApp;

			if ((UIProvider)this.uiProviderComboBox.SelectedItem == UIProvider.Console)
			{
				testApp = "NBug.Examples.Console.exe";
			}
			else if ((UIProvider)this.uiProviderComboBox.SelectedItem == UIProvider.WinForms)
			{
				testApp = "NBug.Examples.WinForms.exe";
			}
			else if ((UIProvider)this.uiProviderComboBox.SelectedItem == UIProvider.WPF)
			{
				testApp = "NBug.Examples.WPF.exe";
			}
			else
			{
				testApp = "NBug.Examples.WinForms.exe";
			}

			string path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), testApp);

			if (!File.Exists(path))
			{
				path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "NBug.Examples.WinForms.exe");
			}

			if (!File.Exists(path))
			{
				MessageBox.Show("Test application cannot be found at location: " + path);
			}
			else
			{
				Process.Start(path, "\"" + this.settingsFile.Name + "\"");
			}
		}

		private void ProjectHomeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(this.projectHomeToolStripMenuItem.Tag.ToString());
		}

		private void OnlineDocumentationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(this.onlineDocumentationToolStripMenuItem.Tag.ToString());
		}

		private void DiscussionForumToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(this.discussionForumToolStripMenuItem.Tag.ToString());
		}

		private void BugTrackerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(this.bugTrackerToolStripMenuItem.Tag.ToString());
		}

		private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var about = new AboutBox())
			{
				about.ShowDialog();
			}
		}

		private void ExternalToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.createFileDialog.FileName = "NBug.config";
			this.CreateButton_Click(this, null);
		}

		private void EmbeddedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.createFileDialog.FileName = "app.config";
			this.CreateButton_Click(this, null);
		}

		private void UIModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if ((UIMode)this.uiModeComboBox.SelectedItem == UIMode.Auto)
			{
				this.uiProviderComboBox.Enabled = false;

				if (!this.uiProviderComboBox.Items.Contains(UIProvider.Auto))
				{
					this.uiProviderComboBox.Items.Add(UIProvider.Auto);
				}

				this.uiProviderComboBox.SelectedItem = UIProvider.Auto;

				// Revert back the settings for the "Handle Exceptions" check box
				this.exitApplicationImmediatelyCheckBox.Checked = Settings.ExitApplicationImmediately;
				this.exitApplicationImmediatelyCheckBox.Enabled = false;
				this.exitApplicationImmediatelyWarningLabel.Enabled = false;
			}
			else if ((UIMode)this.uiModeComboBox.SelectedItem == UIMode.None)
			{
				this.uiProviderComboBox.SelectedItem = null;
				this.previewButton.Enabled = false;

				// Enable the "Handle Exceptions" check box as it is valid for UIMode.None
				this.exitApplicationImmediatelyCheckBox.Enabled = true;
				this.exitApplicationImmediatelyWarningLabel.Enabled = true;
			}
			else
			{
				this.uiProviderComboBox.Enabled = true;
				this.uiProviderComboBox.Items.Remove(UIProvider.Auto);

				// Revert back the settings for the "Handle Exceptions" check box
				this.exitApplicationImmediatelyCheckBox.Checked = Settings.ExitApplicationImmediately;
				this.exitApplicationImmediatelyCheckBox.Enabled = false;
				this.exitApplicationImmediatelyWarningLabel.Enabled = false;
			}
		}

		private void PreviewButton_Click(object sender, EventArgs e)
		{
			using (var preview = new PreviewForm())
			{
				preview.ShowDialog((UIMode)this.uiModeComboBox.SelectedItem, (UIProvider)this.uiProviderComboBox.SelectedItem);
			}
		}

		private void UIProviderComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.uiProviderComboBox.SelectedItem == null)
			{
				this.previewButton.Enabled = false;
			}
			else if ((UIProvider)this.uiProviderComboBox.SelectedItem == UIProvider.Auto)
			{
				this.previewButton.Enabled = false;
			}
			else
			{
				this.previewButton.Enabled = true;
			}
		}
	}
}
