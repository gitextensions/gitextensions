using GitCommands;
using GitCommands.Config;
using GitExtUtils;
using GitUI.Infrastructure;
using GitUI.UserControls;
using ResourceManager;

namespace GitUI.HelperDialogs
{
    /// <summary>
    /// Form that handles Plink exceptions.
    /// </summary>
    public partial class FormRemoteProcess : FormProcess
    {
        #region Translation
        private readonly TranslationString _fingerprintNotRegistredText =
            new(@"The fingerprint of this host is not registered by PuTTY.
This causes this process to hang, and that why it is automatically stopped.

When the connection is opened detached from Git and Git Extensions, the host's fingerprint can be registered.
You could also manually add the host's fingerprint or run Test Connection from the remotes dialog.

Do you want to register the host's fingerprint and restart the process?");
        private readonly TranslationString _fingerprintNotRegistredTextCaption =
            new("Host Fingerprint not registered");
        #endregion

        private bool _restart;
        private string _urlTryingToConnect = string.Empty;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormRemoteProcess()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            : base()
        {
            InitializeComponent();
        }

        public FormRemoteProcess(GitUICommands commands, ArgumentString arguments)
            : base(commands, arguments, commands.Module.WorkingDir, input: null, useDialogSettings: true)
        {
            Commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public static bool ShowDialog(IWin32Window? owner, GitUICommands commands, ArgumentString arguments)
        {
            using FormRemoteProcess formRemoteProcess = new(commands, arguments);
            formRemoteProcess.ShowDialog(owner);
            return !formRemoteProcess.ErrorOccurred();
        }

        public bool Plink { get; set; }

        private GitUICommands Commands { get; }

        /// <summary>
        /// When cloning a remote using putty, sometimes an error occurs that the fingerprint is not known.
        /// This is fixed by trying to connect from the command line, and choose yes when asked for storing
        /// the fingerprint. Just a dirty fix...
        /// </summary>
        public void SetUrlTryingToConnect(string url)
        {
            _urlTryingToConnect = url;
        }

        protected override void BeforeProcessStart()
        {
            _restart = false;
            Plink = GitSshHelpers.IsPlink;
            base.BeforeProcessStart();
        }

        protected override bool HandleOnExit(ref bool isError)
        {
            if (_restart)
            {
                Retry();
                return true;
            }

            // An error occurred!
            if (isError && Plink)
            {
                var output = GetOutputString();

                // there might be another error, this condition is too weak
                /*
                if (output.Contains("successfully authenticated"))
                {
                    isError = false;
                    return false;
                }
                */

                // If the authentication failed because of a missing key, ask the user to supply one.
                if (output.Contains("FATAL ERROR") && output.Contains("authentication"))
                {
                    if (FormPuttyError.AskForKey(this, out var loadedKey))
                    {
                        // To prevent future authentication errors, save this key for this remote.
                        if (!string.IsNullOrEmpty(loadedKey) && !string.IsNullOrEmpty(Remote) &&
                            string.IsNullOrEmpty(Commands.Module.GetSetting("remote.{0}.puttykeyfile")))
                        {
                            Commands.Module.SetPathSetting(string.Format("remote.{0}.puttykeyfile", Remote), loadedKey);
                        }

                        // Retry the command.
                        Retry();
                        return true;
                    }
                }

                if (output.Contains("the server's host key is not cached in the registry", StringComparison.OrdinalIgnoreCase))
                {
                    string remoteUrl;

                    if (string.IsNullOrEmpty(_urlTryingToConnect))
                    {
                        remoteUrl = Commands.Module.GetSetting(string.Format(SettingKeyString.RemoteUrl, Remote));
                        if (string.IsNullOrEmpty(remoteUrl))
                        {
                            remoteUrl = Remote;
                        }
                    }
                    else
                    {
                        remoteUrl = _urlTryingToConnect;
                    }

                    if (AskForCacheHostkey(this, remoteUrl))
                    {
                        Retry();
                        return true;
                    }
                }
            }

            return base.HandleOnExit(ref isError);
        }

        public static bool AskForCacheHostkey(IWin32Window owner, string remoteUrl)
        {
            if (!string.IsNullOrEmpty(remoteUrl) && MessageBoxes.CacheHostkey(owner))
            {
                return new Plink().Connect(remoteUrl);
            }

            return false;
        }

        protected override void DataReceived(object sender, TextEventArgs e)
        {
            if (Plink && e.Text.Contains("If you trust this host, enter \"y\" to add the key to"))
            {
                if (MessageBox.Show(this, _fingerprintNotRegistredText.Text, _fingerprintNotRegistredTextCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    string remoteUrl;
                    if (string.IsNullOrEmpty(_urlTryingToConnect))
                    {
                        remoteUrl = Commands.Module.GetSetting(string.Format(SettingKeyString.RemoteUrl, Remote));
                        remoteUrl = string.IsNullOrEmpty(remoteUrl) ? Remote : remoteUrl;
                    }
                    else
                    {
                        remoteUrl = _urlTryingToConnect;
                    }

                    new Plink().Connect(remoteUrl);

                    _restart = true;
                    Reset();
                }
                else
                {
                    KillProcess();
                }
            }

            base.DataReceived(sender, e);
        }
    }
}
