using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;
using GithubSharp.Core.Services;
using GithubSharp.Core.API;
using GithubSharp.Core;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace Github
{
    public class GithubLoginInfo
    {
        public string Username;
        public string Password;
        public string ApiToken;
    }

    public class GithubPlugin : IRepositoryHostPlugin, IPasswordHelper
    {
        #region cstor
        public GithubPlugin()
        {
            if (_instance != null)
                throw new InvalidOperationException("Can not create this again...");
            _instance = this;
        }

        static GithubPlugin _instance;
        public static GithubPlugin Instance
        {
            get { return _instance; }
        }
        #endregion

        #region IGitPlugin
        public string Description
        {
            get { return "Github"; }
        }

        public IGitPluginSettingsContainer Settings { get; set; }
        private IGitUICommands _gitUiCommands;

        public void Register(IGitUICommands gitUiCommands)
        {
            _gitUiCommands = gitUiCommands;
            Settings.AddSetting("username", "");
            Settings.AddSetting("password", "");
            Settings.AddSetting("apitoken", "");
            Settings.AddSetting("preferred access method", "https");
            StartConfigtest();
        }

        public void Execute(GitUIBaseEventArgs gitUiCommands)
        {
            new ConfigureGithub().ShowDialog();
        }
        #endregion

        #region IGitHostingPlugin
        public IList<IHostedRepository> SearchForRepository(string search)
        {
            var repoApi = GetRepositoryApi();
            return repoApi.Search(search).Select(r => GithubHostedRepository.Convert(this, r)).Cast<IHostedRepository>().ToList();
        }

        public IList<IHostedRepository> GetRepositoriesOfUser(string user)
        {
            var repoApi = GetRepositoryApi();
            return repoApi.List(user).Select(r => GithubHostedRepository.Convert(this, r)).Cast<IHostedRepository>().ToList();
        }

        public IHostedRepository GetRepository(string user, string repositoryName)
        {
            var repoApi = GetRepositoryApi();
            return GithubHostedRepository.Convert(this, repoApi.Get(user, repositoryName));
        }

        public bool ConfigurationOk { get { return InternalConfigurationOk; } }

        public IList<IHostedRepository> GetMyRepos()
        {
            if (GithubUser != null)
            {
                return GetRepositoriesOfUser(GithubUser.Name);
            }

            return new List<IHostedRepository>();
        }

        public List<IHostedRemote> GetHostedRemotesForCurrentWorkingDirRepo()
        {
            var repoInfos = GetCurrentWorkingDirGithubRepos();
            return (from info in repoInfos select (IHostedRemote)new GithubHostedRemote(info, this)).ToList();
        }


        public bool CurrentWorkingDirRepoIsRelevantToMe
        {
            get
            {
                return GetCurrentWorkingDirGithubRepos().Count > 0;
            }
        }
        #endregion

        #region API Getters
        public Repository GetRepositoryApi()
        {
            var r = new Repository(_basicCacher, _logger);
            r.Authenticate(GithubUser);
            return r;
        }

        public PullRequest GetPullRequestApi()
        {
            var pr = new PullRequest(_basicCacher, _logger);
            pr.Authenticate(GithubUser);
            return pr;
        }

        public Issues GetIssuesApi()
        {
            var issues = new Issues(_basicCacher, _logger);
            issues.Authenticate(GithubUser);
            return issues;
        }
        #endregion

        #region Auth stuff
        GithubSharp.Core.Models.GithubUser _githubUser;
        private GithubSharp.Core.Models.GithubUser GithubUser
        {
            get
            {
                if (_githubUser == null)
                {
                    if (Auth == null)
                        return null;

                    _githubUser = new GithubSharp.Core.Models.GithubUser { Name = Auth.Username, APIToken = Auth.ApiToken };
                }

                return _githubUser;
            }
        }

        public void SetAuth(string username, string password, string apitoken)
        {
            if (username == null || password == null || apitoken == null)
                throw new ArgumentNullException();

            username = username.Trim();
            password = password.Trim();
            apitoken = apitoken.Trim();

            if (username.Length == 0 || apitoken.Length < 30)
                throw new ArgumentOutOfRangeException("User or apitoken set to invalid values");

            _githubUser = null;
            _authInfo = null;
            _configurationOk = null;
            InvalidateCache();
            Settings.SetSetting("username", username);
            Settings.SetSetting("password", password);
            Settings.SetSetting("apitoken", apitoken);
        }

        GithubLoginInfo _authInfo;
        public GithubLoginInfo Auth
        {
            get
            {
                if (_authInfo == null)
                {
                    var t = new GithubLoginInfo()
                    {
                        Username = Settings.GetSetting("username"),
                        Password = Settings.GetSetting("password"),
                        ApiToken = Settings.GetSetting("apitoken")
                    };

                    if (!string.IsNullOrEmpty(t.Username) && !string.IsNullOrEmpty(t.ApiToken))
                        _authInfo = t;
                }

                return _authInfo;
            }
        }

        public string PreferredAccessMethod
        {
            set
            {
                if (value != "https" && value != "ssh")
                    throw new ArgumentOutOfRangeException("value");

                Settings.SetSetting("preferred access method", value);
            }
            get
            {
                return Settings.GetSetting("preferred access method");
            }
        }

        //This does not work, github does not use basic auth this way on their web pages, it turns out..
        public static string GetApiTokenFromGithub(string username, string password)
        {
            HttpWebRequest wc = WebRequest.Create(@"https://github.com/account") as HttpWebRequest;
            wc.PreAuthenticate = false;
            wc.Credentials = new NetworkCredential(username, password);
            using (var response = wc.GetResponse())
            using (var stream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                var responseData = stream.ReadToEnd();
                var m = Regex.Match(responseData, @"API token is <code>([0-9A-Za-z]+)</code>");
                if (!m.Success)
                    throw new InvalidOperationException("Could not get API token");
                return m.Groups[1].Value;
            }
        }

        //Not used as of now, but might be useful.
        public string TryGetHelperPassword(string inputUrl)
        {
            if (Auth == null || Auth.Password.Length == 0)
                return null;

            var m = Regex.Match(inputUrl, @"^https://([^/]+)@github.com");
            if (!m.Success)
                return null;

            if (Auth.Username != m.Groups[1].Value)
                return null;

            return Auth.Password;
        }
        #endregion

        #region Config test
        private bool? _configurationOk;

        private bool InternalConfigurationOk
        {
            get
            {
                Monitor.Enter(_configOkLock);
                try
                {
                    if (!_configurationOk.HasValue)
                    {
                        if (Auth == null)
                            _configurationOk = false;
                        else
                        {
                            Monitor.Exit(_configOkLock);
                            StartConfigtest();
                            Monitor.Enter(_configOkLock);
                            if (!_configurationOk.HasValue)
                                throw new ApplicationException("ConfigurationOk still does not have a value");
                            return _configurationOk.Value;
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_configOkLock);
                }

                return _configurationOk.Value;
            }
        }

        ManualResetEvent _configTestEvent = new ManualResetEvent(false);
        private object _configOkLock = new object();
        private void StartConfigtest()
        {
            _configTestEvent.Reset();
            Action a = DoConfigTest;
            a.BeginInvoke(null, null);
            _configTestEvent.WaitOne();
        }

        private void DoConfigTest()
        {
            Monitor.Enter(_configOkLock);
            try
            {
                _configTestEvent.Set();
                GetMyRepos();
                _configurationOk = true;
            }
            catch
            {
                _configurationOk = false;
            }
            finally
            {
                Monitor.Exit(_configOkLock);
            }
        }
        #endregion

        #region Misc stuff
        BasicCacher _basicCacher = new BasicCacher();
        ILogProvider _logger = new InMemLogger();
        internal void InvalidateCache()
        {
            _basicCacher.Clear();
        }

        private static IList<GithubHostedRemoteInformation> GetCurrentWorkingDirGithubRepos()
        {
            List<GithubHostedRemoteInformation> repoInfos = new List<GithubHostedRemoteInformation>();

            var remoteNames = GitCommands.GitCommandHelpers.GetRemotes();
            foreach (var remote in remoteNames.Where(r => !string.IsNullOrEmpty(r)))
            {
                var remoteUrl = GitCommands.GitCommandHelpers.GetSetting("remote." + remote + ".url");
                if (string.IsNullOrEmpty(remoteUrl))
                    continue;

                var m = Regex.Match(remoteUrl, @"git(?:@|://)github.com[:/]([^/]+)/(\w+)\.git");
                if (!m.Success)
                    m = Regex.Match(remoteUrl, @"https?://(?:[^@:]+)?(?::[^/@:]+)?@?github.com/([^/]+)/([\w_\.]+).git");
                if (m.Success)
                {
                    var t = new GithubHostedRemoteInformation() { Name = remote, Owner = m.Groups[1].Value, NameAtGithub = m.Groups[2].Value };
                    if (!repoInfos.Contains(t))
                        repoInfos.Add(t);
                }
            }

            return repoInfos;
        }

        internal string GetLoggerData()
        {
            return _logger.ToString();
        }
        #endregion
    }
}
