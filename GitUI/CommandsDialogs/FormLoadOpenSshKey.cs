using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using GitCommands;

using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    public sealed class FormLoadOpenSshKey : GitModuleForm
    {
        /// <summary>
        /// Saved only in-memory for the case when you reopen the dialog, not exposed in settings.
        /// </summary>
        [CanBeNull]
        private static string _mruPrivateKeyPath;

        public FormLoadOpenSshKey([CanBeNull] GitUICommands aCommands, [CanBeNull] string serveruri)
            : base(aCommands)
        {
            if(aCommands == null)
                return; // Tests

            CreateView(serveruri);
            Translate();
        }

        private void AssignAll(string pathPrivateKey)
        {
            try
            {
                if(pathPrivateKey.IsNullOrWhiteSpace())
                    throw new ArgumentException("The path to the Private Key file must not be empty.");
                var fiPrivate = new FileInfo(pathPrivateKey);
                if(!fiPrivate.Exists)
                    throw new ArgumentException(string.Format("The path to the Private Key file, “{0}”, does not point to an existing file.", pathPrivateKey));
                string pathTargetFile = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.Create), ".ssh"), "id_rsa");
                if(File.Exists(pathTargetFile))
                {
                    // Compare
                    bool isAlreadyThere = false;
                    if(fiPrivate.Length == new FileInfo(pathTargetFile).Length)
                    {
                        byte[] bytesNew = File.ReadAllBytes(fiPrivate.FullName);
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
                        MessageBox.Show(this, "This OpenSSH Private Key has already been assigned to all servers.", "Private Key Already Assigned", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    if(MessageBox.Show(this, "The OpenSSH Private Key as assigned to all servers by copying to the %USERPROFILE%/.ssh/id_rsa file.\n\nA file already exists at this location.\nOverwriting this file might break authentication to other servers.\nAssign to specific server instead if not sure.\n\nOverwrite?", "Private Key Already Assigned", MessageBoxButtons.YesNo, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                        return;
                }

                // Plant the file
                EnsureDirectoryOfFile(pathTargetFile);
                fiPrivate.CopyTo(pathTargetFile, true);

                // Ack success
                MessageBox.Show(this, "The OpenSSH Private Key has been assigned\nto be used with all servers by default\n(unless overridden for specific servers).", "Private Key Assigned", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, "Could not assign the OpenSSH Private Key to all servers." + "\n\n" + ex.Message, "Failed to Assign Private Key", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AssignSpecific(string pathPrivateKey, string sServerMask)
        {
            try
            {
                if(pathPrivateKey.IsNullOrWhiteSpace())
                    throw new ArgumentException("The path to the Private Key file must not be empty.");
                var fiPrivate = new FileInfo(pathPrivateKey);
                if(!fiPrivate.Exists)
                    throw new ArgumentException(string.Format("The path to the Private Key file, “{0}”, does not point to an existing file.", pathPrivateKey));
                if(sServerMask.IsNullOrWhiteSpace())
                    throw new ArgumentException("The specific server mask must not be empty.");

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
                    if(MessageBox.Show(this, string.Format("The OpenSSH configuration file already has records that match the server you're trying to set up.\nIt is recommended that you edit the file manually at %USERPROFILE%/.ssh/config.\n\nWould you still like to add a new record for “{0}”?\n(It will have precedence over all existing records.)", sServerMask), "Record Already Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                        return;
                }

                // Prepare the insetion record
                var sb = new StringBuilder();
                sb.Append("Host ").Append(sServerMask);
                sb.AppendLine();
                sb.Append("\tIdentityFile ").Append(Path.GetFullPath(pathPrivateKey));
                sb.AppendLine();
                sb.Append("\tUser git"); // TODO: how to specify the user name? is it important for our case?

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

                MessageBox.Show(this, string.Format("A record to assign the Private Key to server “{0}” has been successfully added to the OpenSSH Config.\n\nConfig file path:\n{1}\n\nRecord:\n{2}", sServerMask, pathConfigFile, sb), "Private Key Assigned", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, string.Format("Could not assign the OpenSSH Private Key to server “{0}”.", sServerMask) + "\n\n" + ex.Message, "Failed to Assign Private Key", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateView([CanBeNull] string serveruri)
        {
            Text = "OpenSSH Keys";

            var grid = new TableLayoutPanel() {AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Fill};

            // Intro
            grid.Controls.Add(new Label() {Text = "OpenSSH uses a pair of matching keys for authentication: a Private Key and a Public Key.\n\nThe Public Key is submitted to Git servers and lets server know that it's you trying to connect.\nThe Private Key is secret, you only give it to OpenSSH to set up this end of the connection.", AutoSize = true});

            // New key pair
            grid.Controls.Add(new Label() {Text = "Don't have a key pair yet?", AutoSize = true});
            Button btnShowToMake;
            grid.Controls.Add(btnShowToMake = new Button() {Text = "Make a New Key Pair >>", AutoSize = true});
            IList<Control> controlsShowToMake = new List<Control>();
            btnShowToMake.Click += delegate
            {
                controlsShowToMake.ForEach(c => c.Visible = true);
                btnShowToMake.Enabled = false;
            };
            Label labelPuttyGenHowTo;
            grid.Controls.Add(labelPuttyGenHowTo = new Label() {Text = "This will open the PuTTY Key Generator to produce a new pair of keys.\n1) Press “Generate” to make the new key pair.\n2) Press “Save public key” to store the Public Key which you tell to the Git server, either by copypasting or uploading a file.\n3) Choose “Menu | Conversions | Export OpenSSH key” to save the Private Key to a file in a format suitable for OpenSSH. The button won't do.\n4) Done with the PuTTY Key Generator, proceed with this dialog to use the newly-generated Private Key.", AutoSize = true, Visible = false});
            controlsShowToMake.Add(labelPuttyGenHowTo);
            Button btnOpenPuttyGen;
            grid.Controls.Add(btnOpenPuttyGen = new Button() {Text = "Open PuTTY Key Generator…", AutoSize = true, Visible = false});
            btnOpenPuttyGen.Click += delegate { Module.RunExternalCmdDetached(AppSettings.Puttygen, ""); };
            controlsShowToMake.Add(btnOpenPuttyGen);

            // Browse
            grid.Controls.Add(new Label() {Text = "OpenSSH needs the Private Key which matches the Public Key you've told to the server.\nIt's generally safe to use the same key pair for more than one server, just keep the Private Key safe.", AutoSize = true});
            TextBox editPrivateKeyPath;
            grid.Controls.Add(editPrivateKeyPath = new TextBox() {Dock = DockStyle.Top, AutoSize = true, Text = _mruPrivateKeyPath ?? ""});
            editPrivateKeyPath.TextChanged += delegate { _mruPrivateKeyPath = editPrivateKeyPath.Text; };
            Button btnBrowsePrivateKey;
            grid.Controls.Add(btnBrowsePrivateKey = new Button() {Text = "Browse for Private Key…", AutoSize = true});
            btnBrowsePrivateKey.Click += delegate
            {
                using(var openpk = new OpenFileDialog())
                {
                    openpk.Title = "Browse for Private Key";
                    openpk.Filter = "All Files (*.*)|*.*";
                    if(!editPrivateKeyPath.Text.IsNullOrWhiteSpace())
                        openpk.FileName = editPrivateKeyPath.Text;
                    if(openpk.ShowDialog(this) == DialogResult.OK)
                        editPrivateKeyPath.Text = openpk.FileName;
                }
            };

            // Assign
            grid.Controls.Add(new Label() {Text = "Register this Private Key for ALL servers.\nThe private key will be copied to the id_rsa file, which OpenSSH uses by default.", AutoSize = true});
            Button btnAssignAll;
            grid.Controls.Add(btnAssignAll = new Button() {Text = "Assign to All Servers", AutoSize = true});
            btnAssignAll.Click += delegate { AssignAll(editPrivateKeyPath.Text); };
            grid.Controls.Add(new Label() {Text = "Register this Private Key for one specific server (or a group of servers if you use wildcards).\nThe private key file won't be copied, so choose a safe and persistent location for it.", AutoSize = true});
            Button btnAssignSpecific;
            grid.Controls.Add(btnAssignSpecific = new Button() {Text = "Assign to This Server", AutoSize = true});
            TextBox editServerMask;
            grid.Controls.Add(editServerMask = new TextBox() {Text = TryGetServerNameFromUri(serveruri), AutoSize = true, Dock = DockStyle.Top});
            btnAssignSpecific.Click += delegate { AssignSpecific(editPrivateKeyPath.Text, editServerMask.Text); };

            // Close
            Button btnClose;
            grid.Controls.Add(btnClose = new Button() {Text = "Close", AutoSize = true, DialogResult = DialogResult.Cancel});
            CancelButton = btnClose;

            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(grid);
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        private static void EnsureDirectoryOfFile([NotNull] string pathTargetFile)
        {
            if(pathTargetFile == null)
                throw new ArgumentNullException("pathTargetFile");

            // Ensure directory
            string dir = Path.GetDirectoryName(pathTargetFile);
            if(dir == null)
                throw new InvalidOperationException(string.Format("Unexpected: could not determine the directory of the file “{0}”.", pathTargetFile));
            if(Directory.Exists(dir))
                return;

            try
            {
                Directory.CreateDirectory(dir);
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException(string.Format("Failed to write the file “{0}” because its directory “{1}” does not exist and it could not be created. {2}", pathTargetFile, dir, ex.Message));
            }
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

            Uri uri;
            if(!Uri.TryCreate(serveruri, UriKind.Absolute, out uri))
                return "";
            if((uri.IsFile) || (uri.IsUnc))
                return "";

            string host = uri.DnsSafeHost;
            if(host.IsNullOrEmpty())
                return "";

            return host;
        }
    }
}