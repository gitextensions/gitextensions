﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using ResourceManager;
using Settings = GitCommands.AppSettings;

namespace ProxySwitcher
{
    public partial class ProxySwitcherForm : Form, ITranslate
    {
        private readonly ISettingsSource settings;
        private readonly IGitModule gitCommands;

        #region Translation
        private readonly TranslationString _pluginDescription = new TranslationString("Proxy Switcher");
        private readonly TranslationString _pleaseSetProxy = new TranslationString("There is no proxy configured. Please set the proxy host in the plugin settings.");
        #endregion

        /// <summary>
        /// Default constructor added to register all strings to be translated
        /// Use the other constructor:
        /// ProxySwitcherForm(IGitPluginSettingsContainer settings, GitUIBaseEventArgs gitUiCommands)
        /// </summary>
        public ProxySwitcherForm()
        {
            InitializeComponent();
        }

        public ProxySwitcherForm(ISettingsSource settings, GitUIBaseEventArgs gitUiCommands)
        {
            InitializeComponent();
            Translate();

            this.Text = _pluginDescription.Text;
            this.settings = settings;
            this.gitCommands = gitUiCommands.GitModule;
        }

        private void ProxySwitcherForm_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SettingsKey.HttpProxy[settings]))
            {
                MessageBox.Show(this, _pleaseSetProxy.Text, this.Text, MessageBoxButtons.OK);
                this.Close();
            }
            else
            {
                RefreshProxy();
            }
        }

        private void RefreshProxy()
        {
            LocalHttpProxy_TextBox.Text = HidePassword(gitCommands.RunGitCmd("config --get http.proxy"));
            GlobalHttpProxy_TextBox.Text = HidePassword(gitCommands.RunGitCmd("config --global --get http.proxy"));
            ApplyGlobally_CheckBox.Checked = string.Equals(LocalHttpProxy_TextBox.Text, GlobalHttpProxy_TextBox.Text);
        }

        private string HidePassword(string httpProxy)
        {
            return Regex.Replace(httpProxy, ":(.*)@", ":****@");
        }

        private string BuildHttpProxy()
        {
            var sb = new StringBuilder();
            sb.Append("\"");
            var username = SettingsKey.Username[settings];
            if (!string.IsNullOrEmpty(username))
            {
                var password = SettingsKey.Password[settings];
                sb.Append(username);
                if(!string.IsNullOrEmpty(password))
                {
                    sb.Append(":");
                    sb.Append(password);
                }
                sb.Append("@");
            }
            sb.Append(SettingsKey.HttpProxy[settings]);
            var port = SettingsKey.HttpProxyPort[settings];
            if (!string.IsNullOrEmpty(port))
            {
                sb.Append(":");
                sb.Append(port);
            }
            sb.Append("\"");
            return sb.ToString();
        }

        private void SetProxy_Button_Click(object sender, EventArgs e)
        {
            var httpproxy = BuildHttpProxy();
            if (ApplyGlobally_CheckBox.Checked)
            {
                gitCommands.RunGitCmd("config --global http.proxy " + httpproxy);
            }
            else
            {
                gitCommands.RunGitCmd("config http.proxy " + httpproxy);
            }
            RefreshProxy();
        }

        private void UnsetProxy_Button_Click(object sender, EventArgs e)
        {
            if (ApplyGlobally_CheckBox.Checked)
            {
                gitCommands.RunGitCmd("config --global --unset http.proxy");
            }
            else
            {
                gitCommands.RunGitCmd("config --unset http.proxy");
            }
            RefreshProxy();
        }

        protected void Translate()
        {
            Translator.Translate(this, Settings.CurrentTranslation);
        }

        public virtual void AddTranslationItems(ITranslation translation)
        {
            TranslationUtils.AddTranslationItemsFromFields(GetType().Name, this, translation);
        }

        public virtual void TranslateItems(ITranslation translation)
        {
            TranslationUtils.TranslateItemsFromFields(GetType().Name, this, translation);
        }
    }
}
