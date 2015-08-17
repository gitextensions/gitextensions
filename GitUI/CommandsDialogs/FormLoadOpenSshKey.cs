using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using GitCommands;

using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    public sealed class FormLoadOpenSshKey : GitModuleForm
    {
        public FormLoadOpenSshKey([CanBeNull] GitUICommands aCommands)
            : base(aCommands)
        {
            if(aCommands == null)
                return; // Tests

            CreateView();
            Translate();
        }

        private void AssignAll(string pathPrivateKey)
        {
            try
            {
                if(pathPrivateKey.IsNullOrWhiteSpace())
                    throw new ArgumentNullException("pathPrivateKey", "The path to the Private Key file must not be empty.");
                var fiPrivate = new FileInfo(pathPrivateKey);
                if(!fiPrivate.Exists)
                    throw new ArgumentOutOfRangeException("pathPrivateKey", pathPrivateKey, string.Format("The path to the Private Key file, “{0}”, does not point to an existing file.", pathPrivateKey));
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
                fiPrivate.CopyTo(pathTargetFile, true);
                MessageBox.Show(this, "The OpenSSH Private Key has been assigned to be used with all servers by default\n(unless there's another key assigned to a specific server).", "Private Key Assigned", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, "Could not assign the OpenSSH Private Key to all servers." + "\n\n" + ex.Message, "Failed to Assign Private Key", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AssignSpecific(string pathPrivateKey, string sServerMask)
        {
            MessageBox.Show("Assign Specific.");
        }

        private void CreateView()
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
            grid.Controls.Add(editPrivateKeyPath = new TextBox() {Dock = DockStyle.Top, AutoSize = true});
            Button btnBrowsePrivateKey;
            grid.Controls.Add(btnBrowsePrivateKey = new Button() {Text = "Browse for Private Key…", AutoSize = true});
            btnBrowsePrivateKey.Click += delegate
            {
                using(var openpk = new OpenFileDialog())
                {
                    openpk.Title = "Browse for Private Key";
                    openpk.Filter = "All Files (*.*)|*.*";
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
            grid.Controls.Add(editServerMask = new TextBox() {Text = "<servername>", AutoSize = true, Dock = DockStyle.Top});
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
    }
}