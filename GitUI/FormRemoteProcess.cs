﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using GitCommands;

namespace GitUI
{
    /// <summary>
    /// Form that handles Plink exceptions
    /// </summary>
    public partial class FormRemoteProcess : FormProcess
    {
        public bool Plink { get; set; }
        private bool restart = false;

        //constructor for VS designer
        protected FormRemoteProcess()
            : base()
        {        
        }

        public FormRemoteProcess(string process, string arguments, string input, string ASettingsName)
            : base(process, arguments, input, ASettingsName)
        {
            
        }

        public FormRemoteProcess(string process, string arguments, string ASettingsName)
            : base(process, arguments, ASettingsName)
        {
            
        }

        public FormRemoteProcess(string arguments, string ASettingsName)
            : base(arguments, ASettingsName)
        {
            
        }


        private string UrlTryingToConnect = string.Empty;
        /// <summary>
        /// When cloning a remote using putty, sometimes an error occurs that the fingerprint is not known.
        /// This is fixed by trying to connect from the command line, and choose yes when asked for storing
        /// the fingerpring. Just a dirty fix...
        /// </summary>
        public void SetUrlTryingToConnect(string url)
        {
            UrlTryingToConnect = url;
        }



        protected override void BeforeProcessStart()
        {
            restart = false;
            Plink = GitCommandHelpers.Plink();
            base.BeforeProcessStart();
        }


        protected override  bool HandleOnExit(ref bool isError)
        {
            if (restart)
            {
                Retry();
                return true;
            }


            // An error occurred!
            if (isError && Plink)
            {
                //there might be an other error, this condition is too weak
                /*
                if (OutputString.ToString().Contains("successfully authenticated"))
                {
                    isError = false;
                    return false;
                }
                */

                if (OutputString.ToString().Contains("FATAL ERROR") && OutputString.ToString().Contains("authentication"))
                {
                    var puttyError = new FormPuttyError();
                    puttyError.ShowDialog();
                    if (puttyError.RetryProcess)
                    {
                        Retry();
                        return true;
                    }
                }
                if (OutputString.ToString().ToLower().Contains("the server's host key is not cached in the registry"))
                {
                    string remoteUrl;

                    if (string.IsNullOrEmpty(UrlTryingToConnect))
                    {
                        remoteUrl = GitCommandHelpers.GetSetting("remote." + Remote + ".url");
                        if (string.IsNullOrEmpty(remoteUrl))
                            remoteUrl = Remote;
                    }
                    else
                        remoteUrl = UrlTryingToConnect;
                    if (!string.IsNullOrEmpty(remoteUrl))
                        if (MessageBox.Show("The server's host key is not cached in the registry.\n\nDo you want to trust this host key and then try again?", "SSH", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            GitCommandHelpers.RunRealCmd(
                                "cmd.exe",
                                string.Format("/k \"\"{0}\" -T \"{1}\"\"", Settings.Plink, remoteUrl));

                            Retry();
                            return true;
                        }

                }
            }
             
            return base.HandleOnExit(ref isError);
        }

        protected override void DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (Plink)
            {
                if (e.Data.StartsWith("If you trust this host, enter \"y\" to add the key to"))
                {
                    if (MessageBox.Show("The fingerprint of this host is not registered by PuTTY." + Environment.NewLine + "This causes this process to hang, and that why it is automatically stopped." + Environment.NewLine + Environment.NewLine + "When the connection is opened detached from Git and GitExtensions, the host's fingerprint can be registered." + Environment.NewLine + "You could also manually add the host's fingerprint or run Test Connection from the remotes dialog." + Environment.NewLine + Environment.NewLine + "Do you want to register the host's fingerprint and restart the process?", "Host Fingerprint not registered", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        string remoteUrl = GitCommandHelpers.GetSetting("remote." + Remote + ".url");

                        if (string.IsNullOrEmpty(remoteUrl))
                            GitCommandHelpers.RunRealCmd("cmd.exe", "/k \"\"" + Settings.Plink + "\" " + Remote + "\"");
                        else
                            GitCommandHelpers.RunRealCmd("cmd.exe", "/k \"\"" + Settings.Plink + "\" " + remoteUrl + "\"");

                        restart = true;
                    }

                    KillGitCommand();
                }
            }
            base.DataReceived(sender, e);
        }

    }
}
