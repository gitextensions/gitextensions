using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces;
using GithubSharp.Core.Services;
using GithubSharp.Core.API;
using GithubSharp.Core;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

namespace Github
{
    public class GithubHostedRepo : IHostedGitRepo
    {
        private GithubPlugin _githubPlugin;

        public GithubHostedRepo(GithubPlugin githubPlugin)
        {
            _githubPlugin = githubPlugin;
        }

        public string Owner { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAFork { get; set; }
        public bool IsMine 
        { 
            get 
            {
                if (_githubPlugin.Auth == null)
                    throw new InvalidOperationException("Information not set?");
                return Owner == _githubPlugin.Auth.Username;
            }
        }

        public bool IsPrivate { get; set; }
        public int Forks { get; set; }
        public string Parent { private get; set; }
        public string Source { get; set; }

        public string ParentOwner
        {
            get
            {
                if (string.IsNullOrEmpty(Parent))
                    return null;
                string[] s = Parent.Split('/');
                if (s.Length != 2 || s[0].Length == 0 || s[1].Length == 0)
                    return null;
                return s[0];
            }
        }

        public string ParentReadOnlyUrl
        {
            get
            {
                if (string.IsNullOrEmpty(Parent))
                    return null;
                string[] s = Parent.Split('/');
                if (s.Length != 2 || s[0].Length == 0 || s[1].Length == 0)
                    return null;
                return CreateUrl(s[0], s[1], true);
            }
        }

        public static string CreateUrl(string owner, string repoName, bool readOnly)
        {
            if (readOnly)
                return string.Format("http://github.com/{0}/{1}.git", owner, repoName);
            else
                return string.Format("https://{0}@github.com/{0}/{1}.git", owner, repoName);
        }

        public string CloneReadWriteUrl 
        {
            get
            {
                return CreateUrl(Owner, Name, false);
            }
        }

        public string CloneReadOnlyUrl 
        { 
            get
            {
                return CreateUrl(Owner, Name, true);
            }
        }

        public IHostedGitRepo Fork()
        {
            if ( IsMine )
                throw new InvalidOperationException("Can not fork a repo that is already yours");

            var repoApi = _githubPlugin.GetRepositoryApi();
            var tRepo = repoApi.Fork(Owner, Name);
            if (tRepo == null || tRepo.Owner != _githubPlugin.Auth.Username)
                throw new InvalidOperationException("Some part of the fork failed.");

            _githubPlugin.InvalidateCache();

            return FromRepostiory(_githubPlugin, tRepo);
        }

        public static GithubHostedRepo FromRepostiory(GithubPlugin p, GithubSharp.Core.Models.Repository repo)
        {
            return new GithubHostedRepo(p)
                    {
                        Owner = repo.Owner,
                        Name = repo.Name,
                        Description = repo.Description,
                        IsAFork = repo.Fork,
                        Forks = repo.Forks,
                        IsPrivate = repo.Private,
                        Parent = repo.Parent
                    };
        }
    }

    public class GithubLoginInfo
    {
        public string Username;
        public string Password;
        public string ApiToken;
    }

    public class GithubPlugin : IGitHostingPlugin
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
            get { return _instance;  }
        }
        #endregion

        #region IGitPlugin
        public string Description
        {
            get { return "Github"; }
        }

        public IGitPluginSettingsContainer Settings { get; set; }

        public void Register(IGitUICommands gitUiCommands)
        {
            Settings.AddSetting("username", "");
            Settings.AddSetting("password", "");
            Settings.AddSetting("apitoken", "");
        }

        public void Execute(GitUIBaseEventArgs gitUiCommands)
        {
            new ConfigureGithub().ShowDialog();
        }
        #endregion

        #region IGitHostingPlugin
        public IList<IHostedGitRepo> SearchForRepo(string search)
        {
            var repoAPi = GetRepositoryApi();

            return (from repo in repoAPi.Search(search)
                     select new GithubHostedRepo(this)
                     {
                         Owner = repo.Username,
                         Name = repo.Name,
                         Description = repo.Description,
                         IsAFork = repo.Fork,
                         Forks = repo.Forks,
                         IsPrivate = false,
                         Parent = null
                     }).Cast<IHostedGitRepo>().ToList();
        }

        public IList<IHostedGitRepo> GetReposOfUser(string user)
        {
            var repoApi = GetRepositoryApi();
            return (from repo in repoApi.List(user) select GithubHostedRepo.FromRepostiory(this, repo)).Cast<IHostedGitRepo>().ToList();
        }

        public Repository GetRepositoryApi()
        {
            var r = new Repository(_basicCacher, _logger);
            r.Authenticate(GithubUser);
            return r;
        }

        public IList<IHostedGitRepo> GetMyRepos()
        {
            return GetReposOfUser(GithubUser.Name);
        }
        #endregion

        BasicCacher _basicCacher = new BasicCacher();
        ILogProvider _logger = new SimpleLogProvider();

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

        public void SetAuth(string username, string password)
        {
            string apitoken = GetApiTokenFromGithub(username, password);
            SetAuth(username, password, apitoken);
        }

        public void SetAuth(string user, string password, string apitoken)
        {
            if (user == null || password == null || apitoken == null)
                throw new ArgumentNullException();

            user = user.Trim();
            password = password.Trim();
            apitoken = apitoken.Trim();
            
            if (user.Length == 0 || password.Length == 0 || apitoken.Length < 30)
                throw new ArgumentOutOfRangeException("User, password or apitoken set to invalid values");

            _githubUser = null;
            _authInfo = null;
            Settings.SetSetting("username", user);
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

                    if (!string.IsNullOrEmpty(t.Username) && !string.IsNullOrEmpty(t.Password) && !string.IsNullOrEmpty(t.ApiToken))
                        _authInfo = t;
                }

                return _authInfo;
            }
        }

        internal void InvalidateCache()
        {
            _basicCacher.Clear();
        }


        //This does not work.
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
    }

    public class InMemLogger : ILogProvider
    {
        private StringBuilder _out = new StringBuilder();
        public bool DebugMode { get; set; }
       
        public bool HandleAndReturnIfToThrowError(Exception error)
        {
            return true;
        }

        public void LogMessage(string message, params object[] arguments)
        {
            _out.AppendFormat(message, arguments);
        }

        public override string ToString()
        {
            return _out.ToString();
        }
    }

    public class SimpleLogProvider : ILogProvider
    {
        #region ILogProvider Members

        public bool DebugMode
        {
            get;
            set;
        }

        public void LogMessage(string Message, params object[] Arguments)
        {
            if (DebugMode)
                WriteToLog(DateTime.Now.ToString() + " " + string.Format(Message, Arguments));
        }

        public bool HandleAndReturnIfToThrowError(Exception error)
        {
            WriteToLog(DatePrefix + " " + string.Format("{2}{0}{2}{1}{2}", error.Message, DebugMode ? error.StackTrace : "", Environment.NewLine));
            return DebugMode;
        }

        #endregion

        private void WriteToLog(string Message)
        {
            try
            {
                if (!Directory.Exists(LogDirPath))
                    Directory.CreateDirectory(LogDirPath);
                File.AppendAllText(LogFilePath, Message, Encoding.UTF8);
            }
            catch
            {
                Trace.WriteLine("WriteToLog fail");
            }
        }

        private string LogDirPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"); }
        }
        private string LogFilePath
        {
            get { return Path.Combine(LogDirPath, DatePrefix); }
        }

        private string DatePrefix
        {
            get
            {
                var now = DateTime.Now;
                return string.Format("{0}-{1}-{2}.log", now.Day, now.Month, now.Year);
            }
        }
    }

    public class BasicCacher : ICacheProvider
    {
        private static Dictionary<string, object> _cache;
        private static object _syncObj = new object();

        private const string CachePrefix = "BC";

        static BasicCacher()
        {
            lock(_syncObj)
                _cache = new Dictionary<string, object>();
        }

        public T Get<T>(string Name) where T : class
        {
            return Get<T>(Name, DefaultDuractionInMinutes);
        }

        public T Get<T>(string Name, int CacheDurationInMinutes) where T : class
        {
            lock (_syncObj)
            {
                if (!_cache.ContainsKey(CachePrefix + Name)) return null;
                var cached = _cache[CachePrefix + Name] as CachedObject<T>;
                if (cached == null) return null;

                if (cached.When.AddMinutes(CacheDurationInMinutes) < DateTime.Now)
                    return null;

                return cached.Cached;
            }
        }

        public bool IsCached<T>(string Name) where T : class
        {
            lock (_syncObj)
                return _cache.ContainsKey(CachePrefix + Name);
        }

        public void Set<T>(T ObjectToCache, string Name) where T : class
        {
            var cacheObj = new CachedObject<T>();
            cacheObj.Cached = ObjectToCache;
            cacheObj.When = DateTime.Now;

            lock (_syncObj)
                _cache[CachePrefix + Name] = cacheObj;
        }

        public void Delete(string Name)
        {
            lock (_syncObj)
                _cache.Remove(CachePrefix + Name);
        }

        public void DeleteWhereStartingWith(string Name)
        {
            var enumerator = _cache.GetEnumerator();

            lock (_syncObj)
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Key.StartsWith(CachePrefix + Name))
                        _cache.Remove(enumerator.Current.Key);
                }
            }
        }

        public void Clear()
        {
            lock (_syncObj)
                _cache.Clear();
        }

        public void DeleteAll<T>() where T : class
        {
            lock (_syncObj)
                _cache.Clear();
        }

        public int DefaultDuractionInMinutes
        {
            get { return 20; }
        }
    }
}
