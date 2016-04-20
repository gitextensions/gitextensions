using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using GitCommands;

using JetBrains.Annotations;

using ResourceManager;

namespace GitUI.CommandsDialogs
{
	/// <summary>
	/// Allows to generate or choose an existing OpenSSH key, and write it either as id_rsa or as a custom assignment for the host.
	/// </summary>
	public static class FormLoadOpenSshKey
	{
		private static readonly bool IsShowingSuccessMsbox = false; // We're not ACKing success now as we got operations preview

		/// <summary>
		/// Saved only in-memory for the case when you reopen the dialog, not exposed in settings.
		/// </summary>
		[CanBeNull]
		private static string _mruPrivateKeyPath;

		public static bool RunWizard([CanBeNull] string serveruri, [NotNull] GitUICommands aCommands)
		{
			if(aCommands == null)
				throw new ArgumentNullException("aCommands");

			////////////
			// Welcome
			// Acquires the key
			SshKey sshkey = ShowWelcomePage(aCommands);
			if(sshkey == null)
				return false;

			////////////
			// Extract public key
			if(sshkey.CanExtractPublicKey)
			{
				if(!ShowPublicKeyPage(sshkey, aCommands))
					return false;
			}

			///////////////
			// Register private key
			return ShowPrivateKeyPage(sshkey, serveruri, aCommands);
		}

		private static bool AskWritePublicKeyFile([NotNull] string sTitle, [CanBeNull] string sExistingPath, [NotNull] AnonymousGitModuleForm form, [NotNull] Action<string> FWrite) // TODO: inline?
		{
			if(sTitle == null)
				throw new ArgumentNullException("sTitle");
			if(form == null)
				throw new ArgumentNullException("form");
			if(FWrite == null)
				throw new ArgumentNullException("FWrite");

			using(var savior = new SaveFileDialog())
			{
				savior.Title = sTitle;
				savior.Filter = string.Format("{1} (*.pem)|*.pem|{0} (*.*)|*.*", Globalized.Strings.AllFiles.Text, Globalized.Strings.Pem.Text);
				savior.OverwritePrompt = true;

				if(!sExistingPath.IsNullOrWhiteSpace())
					savior.FileName = sExistingPath;
				else
				{
					string dir = null;
					if(!_mruPrivateKeyPath.IsNullOrWhiteSpace())
						dir = Path.GetDirectoryName(_mruPrivateKeyPath);
					if(dir.IsNullOrWhiteSpace())
						dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
					savior.FileName = Path.Combine(dir, "SshPublicKey.pem");
				}

				if(savior.ShowDialog(form) != DialogResult.OK)
					return false;
				FWrite(savior.FileName);
				return true;
			}
		}

		private static bool AssignAll([NotNull] SshKey sshkey, [NotNull] Form owner)
		{
			if(sshkey == null)
				throw new ArgumentNullException("sshkey");
			if(owner == null)
				throw new ArgumentNullException("owner");
			try
			{
				var streamPrivateKey = new MemoryStream();
				sshkey.WriteOutPrivateKey(streamPrivateKey);
				string pathTargetFile = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.Create), ".ssh"), "id_rsa");
				if(File.Exists(pathTargetFile))
				{
					// Compare
					bool isAlreadyThere = false;
					if(streamPrivateKey.Length == new FileInfo(pathTargetFile).Length)
					{
						byte[] bytesNew = streamPrivateKey.ToArray();
						byte[] bytesTarget = File.ReadAllBytes(pathTargetFile);
						if(bytesNew.Length == bytesTarget.Length)
						{
							isAlreadyThere = true;
							for(int a = bytesNew.Length; (a-- > 0) && (isAlreadyThere);)
							{
								if(bytesNew[a] != bytesTarget[a])
									isAlreadyThere = false;
							}
						}
					}
					if(isAlreadyThere)
					{
						MessageBox.Show(owner, Globalized.Strings.ThisOpensshPrivateKeyHasAlreadyBeenAssignedToAllServers.Text, Globalized.Strings.PrivateKeyAlreadyAssigned.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
						return true;
					}
					if(MessageBox.Show(owner, Globalized.Strings.IdRsaAlreadyExists.Text, Globalized.Strings.PrivateKeyAlreadyAssigned.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
						return false;
				}

				// Plant the file
				EnsureDirectoryOfFile(pathTargetFile);
				using(var fstream = new FileStream(pathTargetFile, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete))
				{
					streamPrivateKey.Position = 0;
					streamPrivateKey.CopyTo(fstream);
				}

				// Ack
				if(IsShowingSuccessMsbox)
					MessageBox.Show(owner, Globalized.Strings.IdRsaAssignedOk.Text, Globalized.Strings.PrivateKeyAssigned.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return true;
			}
			catch(Exception ex)
			{
				MessageBox.Show(owner, Globalized.Strings.CouldNotAssignTheOpensshPrivateKeyToAllServers.Text + "\n\n" + ex.Message, Globalized.Strings.FailedToAssignPrivateKey.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}

		private static bool AssignSpecific([NotNull] SshKey sshkey, [NotNull] string pathPrivateKey, [NotNull] string sServerMask, [NotNull] Form owner)
		{
			if(sshkey == null)
				throw new ArgumentNullException("sshkey");
			if(pathPrivateKey == null)
				throw new ArgumentNullException("pathPrivateKey");
			if(sServerMask == null)
				throw new ArgumentNullException("sServerMask");
			if(owner == null)
				throw new ArgumentNullException("owner");

			try
			{
				if(pathPrivateKey.IsNullOrWhiteSpace())
					throw new ArgumentException(Globalized.Strings.ThePathToThePrivateKeyFileMustNotBeEmpty.Text);
				var fiPrivate = new FileInfo(pathPrivateKey);
				if(sshkey.ExistingKeyPath != pathPrivateKey)
				{
					using(FileStream fstream = fiPrivate.Open(FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete))
						sshkey.WriteOutPrivateKey(fstream);
				}
				if(!fiPrivate.Exists)
					throw new ArgumentException(string.Format(Globalized.Strings.ThePathToThePrivateKeyFileDoesNotPointToAnExistingFile.Text, pathPrivateKey));
				if(sServerMask.IsNullOrWhiteSpace())
					throw new ArgumentException(Globalized.Strings.TheServerNameMustNotBeEmpty.Text);

				// Load the server assignment file
				string pathConfigFile = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.Create), ".ssh"), "config");
				string textConfig = "";
				if(File.Exists(pathConfigFile))
					textConfig = File.ReadAllText(pathConfigFile, Encoding.UTF8);

				// A bit parsing on the file, to (1) see if looks like got already a record for this host, (2) find a place to insert, before the first host but after all non-hosted stuff
				var regexHost = new Regex(@"(^|[\r\n]+)[\f\t\v\x85\p{Z}]*Host\b(?<Patterns>[^\r\n]*)($|[\r\n])", RegexOptions.Singleline); // Match all newlines before host, for correct insertion
				MatchCollection matchesHosts = regexHost.Matches(textConfig);

				// Look for our host, confirm if to proceed
				var regexPattern = new Regex("(?<P>\\S+)", RegexOptions.Singleline);
				var regexOurHost = new Regex("\\b" + Regex.Escape(sServerMask).Replace("\\*", ".+").Replace("\\?", ".") + "\\b", RegexOptions.Singleline);
				bool isMatchingOurServer = false;
				var wildcardchars = new[] {'?', '*'};
				foreach(string pattern in matchesHosts.OfType<Match>().SelectMany(m => regexPattern.Matches(m.Groups["Patterns"].Value).OfType<Match>()).Select(m => m.Groups["P"].Value))
				{
					// If our server name/pattern matches the host from the file
					if((pattern == sServerMask) || (regexOurHost.IsMatch(pattern)))
					{
						isMatchingOurServer = true;
						break;
					}
					// If the host in the file is a pattern itself, and it matches our server name
					if((pattern.IndexOfAny(wildcardchars) >= 0) && (Regex.IsMatch(sServerMask, Regex.Escape(pattern).Replace("\\*", ".+").Replace("\\?", "."), RegexOptions.Singleline)))
					{
						isMatchingOurServer = true;
						break;
					}
				}
				if(isMatchingOurServer)
				{
					if(MessageBox.Show(owner, string.Format(Globalized.Strings.SshConfigAlreadyLists.Text, sServerMask), Globalized.Strings.RecordAlreadyExists.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
						return false;
				}

				// Prepare the insetion record
				var sb = new StringBuilder();
				WriteSshConfigRecord(sServerMask, pathPrivateKey, sb);
				sb.AppendLine(); // For separating from the following records nicely

				// Choose the insertion position in the config file
				// Things to keep in mind:
				// (1) The first match wins, so we generally add our record as the first record to make it work
				// (2) A Host line applies to all lines below, so we don't want to change the meaning of any existing not-under-some-host lines in the file
				// As a result, insert before the first existing Host line
				int nInsertAt;
				if(textConfig.IsNullOrWhiteSpace())
					nInsertAt = 0;
				else if(matchesHosts.Count == 0)
					nInsertAt = textConfig.Length; // Found no host records in a non-empty file => all of its options apply to ALL hosts => add our section to the end
				else
					nInsertAt = matchesHosts[0].Index; // Before the first found Host, incl newline chars before it

				// Insert!
				// Add newlines before (after go newlines of the Host, or the end of file) — unless at the beginning of the file
				textConfig = textConfig.Insert(nInsertAt, (nInsertAt > 0 ? Environment.NewLine + Environment.NewLine : "") + sb);

				// Save
				EnsureDirectoryOfFile(pathConfigFile);
				File.WriteAllBytes(pathConfigFile, Encoding.UTF8.GetBytes(textConfig)); // WriteAllText with encoding would add a BOM which OpenSSH cannot handle, and this way it's just the useful bytes

				// Ack
				if(IsShowingSuccessMsbox)
					MessageBox.Show(owner, string.Format(Globalized.Strings.SshConfigAssignedOk.Text, sServerMask, pathConfigFile, sb), Globalized.Strings.PrivateKeyAssigned.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return true;
			}
			catch(Exception ex)
			{
				MessageBox.Show(owner, string.Format(Globalized.Strings.CouldNotAssignTheOpensshPrivateKeyToServer.Text, sServerMask) + "\n\n" + ex.Message, Globalized.Strings.FailedToAssignPrivateKey.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}

		private static void EnsureDirectoryOfFile([NotNull] string pathTargetFile)
		{
			if(pathTargetFile == null)
				throw new ArgumentNullException("pathTargetFile");

			// Ensure directory
			string dir = Path.GetDirectoryName(pathTargetFile);
			if(dir == null)
				throw new InvalidOperationException(string.Format(Globalized.Strings.UnexpectedCouldNotDetermineTheDirectoryOfTheFile.Text, pathTargetFile));
			if(Directory.Exists(dir))
				return;

			try
			{
				Directory.CreateDirectory(dir);
			}
			catch(Exception ex)
			{
				throw new InvalidOperationException(string.Format(Globalized.Strings.CantCreateDir.Text, pathTargetFile, dir, ex.Message));
			}
		}

		private static void MakeDialogWindow(out AnonymousGitModuleForm form, GitUICommands aCommands, out TableLayoutPanel grid)
		{
			form = new AnonymousGitModuleForm(aCommands);

			form.Text = Globalized.Strings.Title.Text;
			form.Padding = new Padding(20);
			form.MinimizeBox = false;
			form.MaximizeBox = false;
			form.SizeGripStyle = SizeGripStyle.Hide;
			form.FormBorderStyle = FormBorderStyle.FixedDialog;
			form.AutoSize = true;
			form.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			form.BackColor = SystemColors.Window;

			grid = new TableLayoutPanel() {AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Fill, Margin = new Padding(10)};
			form.Controls.Add(grid);
		}

		[CanBeNull]
		private static SshKey ShowBrowseForExistingKey()
		{
			try
			{
				using(var openpk = new OpenFileDialog())
				{
					openpk.Title = Globalized.Strings.BrowseForPrivateKey.Text;
					openpk.Filter = string.Format("{0} (*.*)|*.*", Globalized.Strings.AllFiles.Text);
					if(!_mruPrivateKeyPath.IsNullOrWhiteSpace())
						openpk.FileName = _mruPrivateKeyPath;
					if(openpk.ShowDialog() == DialogResult.OK)
						return SshKey.FromExistingFile(Path.GetFullPath(openpk.FileName));
					return null;
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show("Failed to browse for the existing keys." + @"\n\n" + ex.Message, Globalized.Strings.Title.Text + @" – " + Globalized.Strings.Error.Text);
				return null;
			}
		}

		private static bool ShowPrivateKeyPage([NotNull] SshKey sshkey, string serveruri, [NotNull] GitUICommands aCommands)
		{
			if(sshkey == null)
				throw new ArgumentNullException("sshkey");
			if(aCommands == null)
				throw new ArgumentNullException("aCommands");

			AnonymousGitModuleForm form;
			TableLayoutPanel grid;
			MakeDialogWindow(out form, aCommands, out grid);

			// Intro
			Label label;
			grid.Controls.Add(label = new Label() {Text = "Private Key", AutoSize = true});
			label.Font = new Font(label.Font.FontFamily, (float)(label.Font.Size * 1.25), FontStyle.Bold);
			grid.Controls.Add(new Label() {Text = "\nYou have a pair of keys: a Public Key and a Private key.\nThe local machine should be configured to use the Priate Key for connections to the Git server.\n\n", AutoSize = true});

			//////////////////////////
			// Specific Server?
			CheckBox checkSpecificServer;
			grid.Controls.Add(checkSpecificServer = new CheckBox() {Text = "Assign private key to the specific &Git server", AutoSize = true, Dock = DockStyle.Fill, Checked = true});
			checkSpecificServer.Margin += new Padding(3, 0, 0, 0);

			//////////////////////////
			// Specific Params
			TableLayoutPanel gridSpecificParams;
			grid.Controls.Add(gridSpecificParams = new TableLayoutPanel() {AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Fill, Margin = Padding.Empty, Padding = Padding.Empty, ColumnStyles = {new ColumnStyle(SizeType.AutoSize), new ColumnStyle(SizeType.Percent, 100), new ColumnStyle(SizeType.AutoSize)}, RowStyles = {new RowStyle(SizeType.AutoSize), new RowStyle(SizeType.AutoSize)}});
			gridSpecificParams.Margin += new Padding(18, 0, 0, 0);
			checkSpecificServer.CheckedChanged += delegate { gridSpecificParams.Visible = checkSpecificServer.Checked; };

			//////////////////////////
			// Server Name
			gridSpecificParams.Controls.Add(new Label() {Text = "&Server name (wildcards ok):", AutoSize = true, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft}, 0, 0);
			TextBox editServerMask;
			gridSpecificParams.Controls.Add(editServerMask = new TextBox() {Text = TryGetServerNameFromUri(serveruri), AutoSize = true, Dock = DockStyle.Fill}, 1, 0);

			//////////////////////////
			// Keys file name
			gridSpecificParams.Controls.Add(new Label() {Text = "Private &Key file path:", AutoSize = true, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft}, 0, 1);
			TextBox editPrivateKeyFilePath;
			gridSpecificParams.Controls.Add(editPrivateKeyFilePath = new TextBox() {AutoSize = true, Dock = DockStyle.Fill}, 1, 1);
			Button btnBrowse;
			gridSpecificParams.Controls.Add(btnBrowse = new Button() {Text = "…", Width = 23, Height = 23, UseVisualStyleBackColor = true, Dock = DockStyle.Right}, 2, 1);
			btnBrowse.Click += delegate { AskWritePublicKeyFile(Globalized.Strings.SavePublicKeyFile.Text, editPrivateKeyFilePath.Text, form, path => editPrivateKeyFilePath.Text = path); };
			// Initial filename
			string sLastAutoAssignedPrivateKeyPath;
			if(sshkey.ExistingKeyPath.IsNullOrWhiteSpace())
				editPrivateKeyFilePath.Text = sLastAutoAssignedPrivateKeyPath = SuggestKeyFileName(editServerMask.Text);
			else
			{
				editPrivateKeyFilePath.Text = sshkey.ExistingKeyPath;
				sLastAutoAssignedPrivateKeyPath = ""; // Don't autosuggest if we got an explicit file path
			}
			// Suggest by server name
			editServerMask.TextChanged += delegate
			{
				if(editPrivateKeyFilePath.Text == sLastAutoAssignedPrivateKeyPath)
					editPrivateKeyFilePath.Text = sLastAutoAssignedPrivateKeyPath = SuggestKeyFileName(editServerMask.Text);
			};

			/////////////////////
			// Operation preview
			Label labelOperationPreview;
			grid.Controls.Add(labelOperationPreview = new Label() {Text = "Operation Preview", AutoSize = true, Dock = DockStyle.Fill, ForeColor = SystemColors.GrayText});
			// Rendering
			Action FRenderPreview = () =>
			{
				var sb = new StringBuilder();
				sb.AppendLine();
				sb.AppendLine();
				if(checkSpecificServer.Checked)
				{
					string sServer = editServerMask.Text;
					if(sServer.IsNullOrWhiteSpace())
						sServer = "<specify server name>";
					string sPath = editPrivateKeyFilePath.Text;
					if(sPath.IsNullOrWhiteSpace())
						sPath = "<specify key file path>";
					sb.AppendLine("This key will be used only for servers matching the wildcard:");
					sb.Append('“').Append(sServer).Append('”').AppendLine();
					sb.AppendLine();
					sb.AppendLine("When you click Assign, the key will be written to the file:");
					sb.Append('“').Append(sPath).Append('”').AppendLine();
					sb.AppendLine("This file must not be deleted or moved.");
					sb.AppendLine();
					sb.AppendLine("The SSH settings file at “%USERPROFILE%\\.ssh\\config” will be updated with this text:");
					sb.AppendLine();
					try
					{
						WriteSshConfigRecord(sServer, sPath, sb);
					}
					catch
					{
						sb.AppendLine("<speicfy correct data>");
					}
				}
				else
				{
					sb.AppendLine("The key will be assigned to be used with all servers");
					sb.AppendLine("(unless the SSH settings file at “%USERPROFILE%\\.ssh\\config” overrides this).");
					sb.AppendLine();
					sb.AppendLine("When you click Assign, the key will be written to the default path:");
					sb.Append('“').Append("%USERPROFILE%/.ssh/id_rsa").Append('”').AppendLine();
				}
				sb.AppendLine();
				sb.AppendLine();

				labelOperationPreview.Text = sb.ToString();
			};
			// Initial value
			FRenderPreview();
			// Render periodically on change
			var timerRenderOperationPreview = new Timer(form.Components) {Interval = (int)TimeSpan.FromSeconds(.3).TotalMilliseconds, Enabled = false};
			timerRenderOperationPreview.Tick += delegate
			{
				FRenderPreview();
				timerRenderOperationPreview.Stop();
			};
			// Sink changes
			checkSpecificServer.CheckedChanged += delegate { timerRenderOperationPreview.Start(); };
			editServerMask.TextChanged += delegate { timerRenderOperationPreview.Start(); };
			editPrivateKeyFilePath.TextChanged += delegate { timerRenderOperationPreview.Start(); };

			//////////////////////
			// OK button
			Button btnAssign;
			grid.Controls.Add(btnAssign = new Button() {Text = "ASSIG&N", Width = 100, Height = 50, Dock = DockStyle.Right, TextAlign = ContentAlignment.MiddleCenter, UseVisualStyleBackColor = true});
			btnAssign.Click += delegate
			{
				if(checkSpecificServer.Checked)
				{
					_mruPrivateKeyPath = editPrivateKeyFilePath.Text;
					if(AssignSpecific(sshkey, editPrivateKeyFilePath.Text, editServerMask.Text, form))
						form.DialogResult = DialogResult.OK;
				}
				else
				{
					if(AssignAll(sshkey, form))
						form.DialogResult = DialogResult.OK;
				}
			};

			form.AcceptButton = btnAssign;
			return form.ShowDialog() == DialogResult.OK;
		}

		private static bool ShowPublicKeyPage([NotNull] SshKey sshkey, [NotNull] GitUICommands aCommands)
		{
			if(sshkey == null)
				throw new ArgumentNullException("sshkey");
			if(aCommands == null)
				throw new ArgumentNullException("aCommands");

			if(sshkey.RsaParameters == null)
				throw new ArgumentOutOfRangeException("sshkey", sshkey, "This SSH key is not suitable for exporting the public keys.");
			RSAParameters rsakey = sshkey.RsaParameters.Value;

			AnonymousGitModuleForm form;
			TableLayoutPanel grid;
			MakeDialogWindow(out form, aCommands, out grid);

			// Intro
			Label label;
			grid.Controls.Add(label = new Label() {Text = "Public Key", AutoSize = true});
			label.Font = new Font(label.Font.FontFamily, (float)(label.Font.Size * 1.25), FontStyle.Bold);
			grid.Controls.Add(new Label() {Text = "\nYou have just created a new pair of keys, a Public Key, and a Private key.\nThe Public Key should be uploaded to the Git server.\n\nLog on to the Git server Web, edit your profile, and add the new public key in the appropriate format:\n", AutoSize = true});

			///////////////
			// Formats

			Action<string, string, string> FAddFormat = (sHeading, sDescription, sExportedText) =>
			{
				grid.Controls.Add(label = new Label() {Text = sHeading, AutoSize = true});
				label.Font = new Font(label.Font, FontStyle.Bold);
				label.Margin += new Padding(0, 16, 0, 5);
				grid.Controls.Add(label = new Label() {Text = sDescription, AutoSize = true});
				label.Margin += new Padding(14, 0, 0, 0);

				FlowLayoutPanel stackButtons;
				grid.Controls.Add(stackButtons = new FlowLayoutPanel() {AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.LeftToRight});
				stackButtons.Margin += new Padding(14, 0, 0, 0);
				Button btn;
				stackButtons.Controls.Add(btn = new Button() {Text = "COPY", Width = 75, Height = 23, UseVisualStyleBackColor = true});
				btn.Click += delegate { Clipboard.SetText(sExportedText); };
				stackButtons.Controls.Add(btn = new Button() {Text = "SAVE…", Width = 75, Height = 23, UseVisualStyleBackColor = true});
				btn.Click += delegate { AskWritePublicKeyFile(Globalized.Strings.SavePublicKeyFile.Text, null, form, filepath => File.WriteAllText(filepath, sExportedText)); };
			};

			FAddFormat("1) SSH Public Key File format (RFC-4716)", "Default public key file format for SSH", RsaUtil.ExportPublicKeySsh(rsakey));
			FAddFormat("2) OpenSSH Public Key format (proprietary)", "GitHub.com prefers this format when pasting public key text", RsaUtil.ExportPublicKeyOpenSsh(rsakey));
			FAddFormat("3) PKCS#1 PEM DER ASN.1 (RFC-3447)", "Public-Key Cryptography Standards (PKCS) #1 format", RsaUtil.ExportPublicKeyPkcs1(rsakey));

			grid.Controls.Add(new Button() {Text = "&NEXT >", DialogResult = DialogResult.OK, Width = 100, Height = 50, Dock = DockStyle.Right, TextAlign = ContentAlignment.MiddleCenter, UseVisualStyleBackColor = true});

			return form.ShowDialog() == DialogResult.OK;
		}

		[CanBeNull]
		private static SshKey ShowWelcomePage([NotNull] GitUICommands aCommands)
		{
			if(aCommands == null)
				throw new ArgumentNullException("aCommands");

			SshKey sshkey = null;
			AnonymousGitModuleForm form;
			TableLayoutPanel grid;
			MakeDialogWindow(out form, aCommands, out grid);

			// Intro
			Label labelTitle;
			grid.Controls.Add(labelTitle = new Label() {Text = "SSH Keys", AutoSize = true});
			labelTitle.Font = new Font(labelTitle.Font.FontFamily, (float)(labelTitle.Font.Size * 1.25), FontStyle.Bold);
			grid.Controls.Add(new Label() {Text = "\nSSH uses public-key cryptography to authenticate the remote computer and allow it to authenticate the user.\nYou should set up a matching pair of keys, here on the local machine and on the git server.\n\n", AutoSize = true});

			// Task Buttons

			var gridTaskButtons = new TableLayoutPanel() {AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Fill, Margin = new Padding(0), RowCount = 3, ColumnCount = 2, RowStyles = {new RowStyle(SizeType.Percent, 50), new RowStyle(SizeType.Absolute, 10), new RowStyle(SizeType.Percent, 50)}, ColumnStyles = {new ColumnStyle(SizeType.AutoSize), new ColumnStyle(SizeType.AutoSize)}};
			grid.Controls.Add(gridTaskButtons);

			/////////////////////////
			// New

			Button btnNew;
			gridTaskButtons.Controls.Add(btnNew = new Button {Text = "CREATE &NEW\nKEY PAIR", TextAlign = ContentAlignment.MiddleLeft, Width = 100, Height = 50, UseVisualStyleBackColor = true}, 0, 0);
			btnNew.Click += delegate
			{
				// This generates a new RSA key using Windows CryptoAPI (or whatever there is in Mono instead)
				// It's a fast operation, well under a second, so no use in complicated threading (not so fun to do without Tasks here)
				Application.UseWaitCursor = true;
				try
				{
					sshkey = SshKey.CreateNew();
				}
				finally
				{
					Application.UseWaitCursor = false;
				}
				form.DialogResult = DialogResult.OK;
			};
			gridTaskButtons.Controls.Add(new Label() {Text = "Typically, you'd create a new pair of keys\nfor another local machine and for another server", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, Margin = new Padding(10)}, 1, 0);

			/////////////////////////
			// Old

			Button btnOld;
			gridTaskButtons.Controls.Add(btnOld = new Button {Text = "USE E&XISTING\nKEY PAIR", TextAlign = ContentAlignment.MiddleLeft, Width = 100, Height = 50, UseVisualStyleBackColor = true}, 0, 2);
			btnOld.Click += delegate
			{
				sshkey = ShowBrowseForExistingKey();
				if(sshkey != null) // Failed or canceled?
					form.DialogResult = DialogResult.OK;
			};
			gridTaskButtons.Controls.Add(new Label() {Text = "Registers an existing key container file", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, Margin = new Padding(10)}, 1, 2);

			form.AcceptButton = btnNew;
			form.ShowDialog();
			return sshkey;
		}

		/// <summary>
		/// Proposes where to store the generated private key.
		/// </summary>
		[NotNull]
		private static string SuggestKeyFileName([NotNull] string text)
		{
			if(text == null)
				throw new ArgumentNullException("text");

			// To ssh dir
			string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");

			// Use server name for baseline
			string idname = Regex.Replace(text.Trim(), "\\W+", "_");
			if((idname == "_") || (idname.Length == 0))
				idname = "AnyServer";

			// Disambig 
			string filename = Path.Combine(directory, "Keys." + idname + ".pem");
			if(File.Exists(filename))
				filename = Path.Combine(directory, "Keys." + idname + "." + DateTime.UtcNow.ToString("s").Replace(':', '-') + ".pem");

			return filename;
		}

		/// <summary>
		/// We use the “from” field of the Clone dialog to get the default for the remote server name.
		/// Might be either a valid remote URI, or a local abs/rel path, or an UNC path, or not yet entered at all, or some garbage. Only extract valid server names.
		/// </summary>
		[NotNull]
		private static string TryGetServerNameFromUri([CanBeNull] string serveruri)
		{
			if(serveruri.IsNullOrWhiteSpace())
				return "";

			// Parse as a valid URI
			Uri uri;
			if(Uri.TryCreate(serveruri, UriKind.Absolute, out uri))
			{
				if((uri.IsFile) || (uri.IsUnc))
					return "";

				string host = uri.DnsSafeHost;
				if(host.IsNullOrEmpty())
					return "";

				return host;
			}

			// Special SSH/SCP format of user@host:localpath
			Match match = Regex.Match(serveruri, @"^\s*(?<User>\S+?)@(?<Host>\S+?):.+$", RegexOptions.Singleline);
			if(match.Success)
				return match.Groups["Host"].Value;

			return "";
		}

		private static void WriteSshConfigRecord(string sServerMask, string pathPrivateKey, StringBuilder sb)
		{
			sb.Append("Host ").Append(sServerMask);
			sb.AppendLine();
			sb.Append("\tIdentityFile ").Append('"').Append(Path.GetFullPath(pathPrivateKey).Trim()).Append('"');
			sb.AppendLine();
		}

		private class Globalized : Translate
		{
			public static readonly Globalized Strings = new Globalized();

			private Globalized()
			{
				Translator.Translate(this, AppSettings.CurrentTranslation);
			}

			public readonly TranslationString AllFiles = new TranslationString("All Files");

			public readonly TranslationString BrowseForPrivateKey = new TranslationString("Browse for Existing Key File");

			public readonly TranslationString CantCreateDir = new TranslationString("Failed to write the file “{0}” because its directory “{1}” does not exist and it could not be created. {2}");

			public readonly TranslationString CouldNotAssignTheOpensshPrivateKeyToAllServers = new TranslationString("Could not assign the OpenSSH Private Key to all servers.");

			public readonly TranslationString CouldNotAssignTheOpensshPrivateKeyToServer = new TranslationString("Could not assign the OpenSSH Private Key to server “{0}”.");

			public readonly TranslationString Error = new TranslationString("Error");

			public readonly TranslationString FailedToAssignPrivateKey = new TranslationString("Failed to Assign Private Key");

			public readonly TranslationString IdRsaAlreadyExists = new TranslationString("The OpenSSH Private Key as assigned to all servers by copying to the %USERPROFILE%/.ssh/id_rsa file.\n\nA file already exists at this location.\nOverwriting this file might break authentication to other servers.\nAssign to specific server instead if not sure.\n\nOverwrite?");

			public readonly TranslationString IdRsaAssignedOk = new TranslationString("The OpenSSH Private Key has been assigned\nto be used with all servers by default\n(unless overridden for specific servers).");

			public readonly TranslationString Pem = new TranslationString("Privacy-Enhanced Mail");

			public readonly TranslationString PrivateKeyAlreadyAssigned = new TranslationString("Private Key Already Assigned");

			public readonly TranslationString PrivateKeyAssigned = new TranslationString("Private Key Assigned");

			public readonly TranslationString RecordAlreadyExists = new TranslationString("Record Already Exists");

			public readonly TranslationString SavePublicKeyFile = new TranslationString("Save Public Key File");

			public readonly TranslationString SshConfigAlreadyLists = new TranslationString("The OpenSSH configuration file already has records that match the server you're trying to set up.\nIt is recommended that you edit the file manually at %USERPROFILE%/.ssh/config.\n\nWould you still like to add a new record for “{0}”?\n(It will have precedence over all existing records.)");

			public readonly TranslationString SshConfigAssignedOk = new TranslationString("A record to assign the Private Key to server “{0}” has been successfully added to the OpenSSH Config.\n\nConfig file path:\n{1}\n\nRecord:\n{2}");

			public readonly TranslationString ThePathToThePrivateKeyFileDoesNotPointToAnExistingFile = new TranslationString("The path to the Private Key file, “{0}”, does not point to an existing file.");

			public readonly TranslationString ThePathToThePrivateKeyFileMustNotBeEmpty = new TranslationString("The path to the Private Key file must not be empty.");

			public readonly TranslationString TheServerNameMustNotBeEmpty = new TranslationString("The server name must not be empty.");

			public readonly TranslationString ThisOpensshPrivateKeyHasAlreadyBeenAssignedToAllServers = new TranslationString("This OpenSSH Private Key is already assigned to all servers.");

			public readonly TranslationString Title = new TranslationString("SSH Keys");

			public readonly TranslationString UnexpectedCouldNotDetermineTheDirectoryOfTheFile = new TranslationString("Unexpected: could not determine the directory of the file “{0}”.");
		}

		public class SshKey
		{
			private static readonly int RsaKeySize = 2048;

			private SshKey(RSAParameters? rsaParameters, string existingKeyPath)
			{
				RsaParameters = rsaParameters;
				ExistingKeyPath = existingKeyPath;
			}

			public bool CanExtractPublicKey
			{
				get
				{
					return RsaParameters != null;
				}
			}

			[CanBeNull]
			public readonly string ExistingKeyPath;

			/// <summary>
			/// If we have generated the key (or when we learn to read those from the existing key file).
			/// </summary>
			public readonly RSAParameters? RsaParameters;

			[NotNull]
			public static SshKey CreateNew()
			{
				return new SshKey(new RSACryptoServiceProvider(RsaKeySize).ExportParameters(true), null);
			}

			[NotNull]
			public static SshKey FromExistingFile([NotNull] string keyfile)
			{
				if(keyfile == null)
					throw new ArgumentNullException("keyfile");
				if(!File.Exists(keyfile))
					throw new ArgumentOutOfRangeException("keyfile", keyfile, "The file does not exist.");
				return new SshKey(null, keyfile);
			}

			public void WriteOutPrivateKey([NotNull] Stream output)
			{
				if(output == null)
					throw new ArgumentNullException("output");

				// From known params
				if(RsaParameters != null)
				{
					using(var sw = new StreamWriter(output, Encoding.ASCII))
						sw.Write(RsaUtil.ExportPrivateKeyRSAPrivateKey(RsaParameters.Value));
					return;
				}

				// From known existing disk file
				if(!ExistingKeyPath.IsNullOrWhiteSpace())
				{
					using(var fstream = new FileStream(ExistingKeyPath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
						fstream.CopyTo(output);
					return;
				}

				// Oops
				throw new InvalidOperationException("This SSH Key has no means for writing out a private key.");
			}
		}
	}
}