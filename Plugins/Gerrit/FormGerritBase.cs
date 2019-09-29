using System;
using GitUI;
using GitUIPluginInterfaces;
using RestSharp;

namespace Gerrit
{
    public class FormGerritBase : GitExtensionsForm
    {
        protected GerritSettings Settings { get; private set; }
        protected Version Version { get; private set; }
        protected readonly RestClient client = new RestClient();
        protected readonly IGitUICommands UICommands;
        protected IGitModule Module => UICommands.GitModule;

        private FormGerritBase()
            : this(null)
        {
        }

        protected FormGerritBase(IGitUICommands uiCommands)
            : base(true)
        {
            UICommands = uiCommands;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            Settings = GerritSettings.Load(Module);

            if (Settings == null)
            {
                Dispose();
                return;
            }

            SetRestClientUrl();
            Version = GetGerritVersion();

            base.OnLoad(e);
        }

        private void SetRestClientUrl()
        {
            string host = Settings.Host;

            if (!host.StartsWith("http://") || !host.StartsWith("https://"))
            {
                host = "https://" + host;
            }

            if (!host.EndsWith("/"))
            {
                host += "/";
            }

            client.BaseUrl = new Uri(host);
        }

        private Version GetGerritVersion()
        {
            RestRequest request = new RestRequest("/config/server/version");
            IRestResponse response = client.Execute(request);
            return Version.Parse(response.Content.Replace(")]}'", "").Replace("\n", "").Replace("\"", "").Split('-')[0]);
        }
    }
}
