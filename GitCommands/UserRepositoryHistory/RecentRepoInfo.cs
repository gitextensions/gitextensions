using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace GitCommands.UserRepositoryHistory
{
    public class RecentRepoInfo
    {
        public Repository Repo { get; set; }
        [CanBeNull] public string Caption { get; set; }
        public bool MostRecent { get; set; }
        [CanBeNull] public DirectoryInfo DirInfo { get; set; }
        [CanBeNull] public string ShortName { get; set; }
        public string DirName { get; set; }

        public RecentRepoInfo(Repository repo, bool mostRecent)
        {
            Repo = repo;
            MostRecent = mostRecent;
            try
            {
                DirInfo = new DirectoryInfo(Repo.Path);
            }
            catch (SystemException)
            {
                DirInfo = null;
                Caption = Repo.Path;
            }

            if (DirInfo != null)
            {
                ShortName = DirInfo.Name;
                DirInfo = DirInfo.Parent;
            }

            DirName = DirInfo?.FullName ?? "";
        }

        public bool FullPath => DirInfo == null;

        public override string ToString() => Repo.ToString();
    }

    public class RecentRepoSplitter
    {
        public int MaxRecentRepositories { get; set; }
        public ShorteningRecentRepoPathStrategy ShorteningStrategy { get; set; }
        public bool SortMostRecentRepos { get; set; }
        public bool SortLessRecentRepos { get; set; }
        public int RecentReposComboMinWidth { get; set; }

        // need to be set before shortening using middleDots strategy
        public Graphics Graphics { get; set; }
        public Font MeasureFont { get; set; }

        public RecentRepoSplitter()
        {
            MaxRecentRepositories = AppSettings.MaxMostRecentRepositories;
            ShorteningStrategy = AppSettings.ShorteningRecentRepoPathStrategy;
            SortMostRecentRepos = AppSettings.SortMostRecentRepos;
            SortLessRecentRepos = AppSettings.SortLessRecentRepos;
            RecentReposComboMinWidth = AppSettings.RecentReposComboMinWidth;
        }

        public void SplitRecentRepos(IList<Repository> recentRepositories, List<RecentRepoInfo> mostRecentRepoList, List<RecentRepoInfo> lessRecentRepoList)
        {
            var orderedRepos = new SortedList<string, List<RecentRepoInfo>>();
            var mostRecentRepos = new List<RecentRepoInfo>();
            var lessRecentRepos = new List<RecentRepoInfo>();

            var middleDot = ShorteningStrategy == ShorteningRecentRepoPathStrategy.MiddleDots;
            var signDir = ShorteningStrategy == ShorteningRecentRepoPathStrategy.MostSignDir;

            int n = Math.Min(MaxRecentRepositories, recentRepositories.Count);

            // the maxRecentRepositories repositories will be added at beginning
            // rest will be added in alphabetical order
            foreach (Repository repository in recentRepositories)
            {
                bool mostRecent = (mostRecentRepos.Count < n && repository.Anchor == Repository.RepositoryAnchor.None) ||
                    repository.Anchor == Repository.RepositoryAnchor.MostRecent;
                var ri = new RecentRepoInfo(repository, mostRecent);
                if (ri.MostRecent)
                {
                    mostRecentRepos.Add(ri);
                }
                else
                {
                    lessRecentRepos.Add(ri);
                }

                if (middleDot)
                {
                    AddToOrderedMiddleDots(orderedRepos, ri);
                }
                else
                {
                    AddToOrderedSignDir(orderedRepos, ri, signDir);
                }

                if (ri.Caption != null)
                {
                    ri.Caption = PathUtil.GetDisplayPath(ri.Caption);
                }
            }

            int r = mostRecentRepos.Count - 1;

            // remove not anchored repos if there is more than maxRecentRepositories repos
            while (mostRecentRepos.Count > n && r >= 0)
            {
                var repo = mostRecentRepos[r];
                if (repo.Repo.Anchor == Repository.RepositoryAnchor.MostRecent)
                {
                    r--;
                }
                else
                {
                    repo.MostRecent = false;
                    mostRecentRepos.RemoveAt(r);
                }
            }

            void AddSortedRepos(bool mostRecent, List<RecentRepoInfo> addToList)
            {
                addToList.AddRange(
                    from caption in orderedRepos.Keys
                    from repo in orderedRepos[caption]
                    where repo.MostRecent == mostRecent
                    select repo);
            }

            void AddNotSortedRepos(List<RecentRepoInfo> list, List<RecentRepoInfo> addToList)
            {
                addToList.AddRange(list);
            }

            if (SortMostRecentRepos)
            {
                AddSortedRepos(true, mostRecentRepoList);
            }
            else
            {
                AddNotSortedRepos(mostRecentRepos, mostRecentRepoList);
            }

            if (SortLessRecentRepos)
            {
                AddSortedRepos(false, lessRecentRepoList);
            }
            else
            {
                AddNotSortedRepos(lessRecentRepos, lessRecentRepoList);
            }
        }

        private static void AddToOrderedSignDir(SortedList<string, List<RecentRepoInfo>> orderedRepos, RecentRepoInfo repoInfo, bool shortenPath)
        {
            // if there is no short name for a repo, then try to find unique caption extending short directory path
            if (shortenPath && repoInfo.DirInfo != null)
            {
                string s = repoInfo.DirName.Substring(repoInfo.DirInfo.FullName.Length);
                if (!s.IsNullOrEmpty())
                {
                    s = s.Trim(Path.DirectorySeparatorChar);
                }

                // candidate for short name
                repoInfo.Caption = repoInfo.ShortName;
                if (!s.IsNullOrEmpty())
                {
                    repoInfo.Caption += " (" + s + ")";
                }

                repoInfo.DirInfo = repoInfo.DirInfo.Parent;
            }
            else
            {
                repoInfo.Caption = repoInfo.Repo.Path;
            }

            var existsShortName = orderedRepos.TryGetValue(repoInfo.Caption, out var list);
            if (!existsShortName)
            {
                list = new List<RecentRepoInfo>();
                orderedRepos.Add(repoInfo.Caption, list);
            }

            var tmpList = new List<RecentRepoInfo>();
            if (existsShortName)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    RecentRepoInfo r = list[i];
                    if (!r.FullPath)
                    {
                        tmpList.Add(r);
                        list.RemoveAt(i);
                    }
                }
            }

            if (repoInfo.FullPath || !existsShortName)
            {
                list.Add(repoInfo);
            }
            else
            {
                tmpList.Add(repoInfo);
            }

            // find unique caption for repos with no title
            foreach (RecentRepoInfo r in tmpList)
            {
                AddToOrderedSignDir(orderedRepos, r, shortenPath);
            }
        }

        private static string MakePath(string l, string r)
        {
            if (l == null)
            {
                return r;
            }

            return Path.Combine(l, r);
        }

        private void AddToOrderedMiddleDots(SortedList<string, List<RecentRepoInfo>> orderedRepos, RecentRepoInfo repoInfo)
        {
            DirectoryInfo dirInfo;
            try
            {
                dirInfo = new DirectoryInfo(repoInfo.Repo.Path);
                if (!string.Equals(dirInfo.FullName, repoInfo.Repo.Path, StringComparison.OrdinalIgnoreCase))
                {
                    // this is likely to happen when attempting to interpret windows paths on linux
                    // e.g. dirInfo = DirectoryInfo("c:\\temp") -> dirInfo => /usr/home/temp
                    dirInfo = null;
                }
            }
            catch
            {
                dirInfo = null;
            }

            if (dirInfo == null)
            {
                repoInfo.Caption = repoInfo.Repo.Path;
            }
            else
            {
                string root = null;
                string company = null;
                string repository = null;
                string workingDir = dirInfo.Name;
                dirInfo = dirInfo.Parent;
                if (dirInfo != null)
                {
                    repository = dirInfo.Name;
                    dirInfo = dirInfo.Parent;
                }

                bool addDots = false;

                if (dirInfo != null)
                {
                    while (dirInfo.Parent?.Parent != null)
                    {
                        dirInfo = dirInfo.Parent;
                        addDots = true;
                    }

                    company = dirInfo.Name;
                    if (dirInfo.Parent != null)
                    {
                        root = dirInfo.Parent.Name;
                    }
                }

                void ShortenPathWithCompany(int skipCount)
                {
                    string c = null;
                    string r = null;

                    if (company?.Length > skipCount)
                    {
                        c = company.Substring(0, company.Length - skipCount);
                    }

                    if (repository?.Length > skipCount)
                    {
                        r = repository.Substring(skipCount, repository.Length - skipCount);
                    }

                    repoInfo.Caption = MakePath(root, c);

                    if (addDots)
                    {
                        repoInfo.Caption = MakePath(repoInfo.Caption, "..");
                    }

                    repoInfo.Caption = MakePath(repoInfo.Caption, r);
                    repoInfo.Caption = MakePath(repoInfo.Caption, workingDir);
                }

                bool ShortenPath(int skipCount)
                {
                    string path = repoInfo.Repo.Path;
                    string fistDir = (root ?? company) ?? repository;
                    string lastDir = workingDir;
                    if (fistDir != null && path.Length - lastDir.Length - fistDir.Length - skipCount > 0)
                    {
                        int middle = ((path.Length - lastDir.Length) / 2) + ((path.Length - lastDir.Length) % 2);
                        int leftEnd = middle - (skipCount / 2);
                        int rightStart = middle + (skipCount / 2) + (skipCount % 2);

                        if (leftEnd == rightStart)
                        {
                            repoInfo.Caption = path;
                        }
                        else
                        {
                            repoInfo.Caption = path.Substring(0, leftEnd) + ".." + path.Substring(rightStart, path.Length - rightStart);
                        }

                        return true;
                    }

                    return false;
                }

                // if fixed width is not set then short as in pull request vccp's example
                // full "E:\CompanyName\Projects\git\ProductName\Sources\RepositoryName\WorkingDirName"
                // short "E:\CompanyName\...\RepositoryName\WorkingDirName"
                if (RecentReposComboMinWidth == 0)
                {
                    ShortenPathWithCompany(0);
                }

                // else skip symbols beginning from the middle to both sides,
                // so we'll see "E:\Compa...toryName\WorkingDirName" and "E:\...\WorkingDirName" at the end.
                else
                {
                    SizeF captionSize;
                    bool canShorten;
                    int skipCount = 0;
                    do
                    {
                        canShorten = ShortenPath(skipCount);
                        skipCount++;
                        captionSize = Graphics.MeasureString(repoInfo.Caption, MeasureFont);
                    }
                    while (captionSize.Width > RecentReposComboMinWidth - 10 && canShorten);
                }
            }

            if (!orderedRepos.TryGetValue(repoInfo.Caption, out var list))
            {
                list = new List<RecentRepoInfo>();
                orderedRepos.Add(repoInfo.Caption, list);
            }

            list.Add(repoInfo);
        }
    }
}
