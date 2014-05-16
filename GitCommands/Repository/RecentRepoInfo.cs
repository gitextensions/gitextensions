using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GitCommands.Repository
{
    public class RecentRepoInfo
    {

        public Repository Repo { get; set; }
        public string Caption { get; set; }
        public bool MostRecent { get; set; }
        public DirectoryInfo DirInfo { get; set; }
        public string ShortName { get; set; }
        public string DirName { get; set; }

        public RecentRepoInfo(Repository aRepo, bool aMostRecent)
        {
            Repo = aRepo;
            MostRecent = aMostRecent;
            try
            {
                DirInfo = new DirectoryInfo(Repo.Path);
            }
            catch (SystemException)
            {
                DirInfo = null;
                Caption = Repo.Path;
            }

            if (Repo.Title != null)
                ShortName = Repo.Title;
            else if (DirInfo != null)
                ShortName = DirInfo.Name;
            

            if (DirInfo != null)
                DirInfo = DirInfo.Parent;

            DirName = DirInfo == null ? "" : DirInfo.FullName;
        }

        public bool FullPath
        {
            get { return DirInfo == null; }
        }


        public override string ToString()
        {
            return Repo.ToString();
        }

    }

    public class RecentRepoSplitter
    {
        public static readonly string ShorteningStrategy_None = "";
        public static readonly string ShorteningStrategy_MostSignDir = "MostSignDir";
        public static readonly string ShorteningStrategy_MiddleDots = "MiddleDots";


        public int MaxRecentRepositories { get; set; }
        public string ShorteningStrategy { get; set; }
        public bool SortMostRecentRepos { get; set; }
        public bool SortLessRecentRepos { get; set; }
        public int RecentReposComboMinWidth { get; set; }
        //need to be set before shortening using middleDots strategy
        public Graphics graphics { get; set; }
        public Font measureFont { get; set; }


        
        public RecentRepoSplitter()
        {
            MaxRecentRepositories = AppSettings.MaxMostRecentRepositories;
            ShorteningStrategy = AppSettings.ShorteningRecentRepoPathStrategy;
            SortMostRecentRepos = AppSettings.SortMostRecentRepos;
            SortLessRecentRepos = AppSettings.SortLessRecentRepos;
            RecentReposComboMinWidth = AppSettings.RecentReposComboMinWidth;
        
        }



        public void SplitRecentRepos(ICollection<Repository> recentRepositories, List<RecentRepoInfo> mostRecentRepoList, List<RecentRepoInfo> lessRecentRepoList)
        {
            SortedList<string, List<RecentRepoInfo>> orderedRepos = new SortedList<string, List<RecentRepoInfo>>();
            List<RecentRepoInfo> mostRecentRepos = new List<RecentRepoInfo>();
            List<RecentRepoInfo> lessRecentRepos = new List<RecentRepoInfo>();

            bool middleDot = ShorteningStrategy_MiddleDots.Equals(ShorteningStrategy);
            bool signDir = ShorteningStrategy_MostSignDir.Equals(ShorteningStrategy);

            int n = Math.Min(MaxRecentRepositories, recentRepositories.Count);
            //the maxRecentRepositories repositories will be added at begining
            //rest will be added in alphabetical order
            foreach (Repository repository in recentRepositories)
            {
                bool mostRecent = mostRecentRepos.Count < n && repository.Anchor == Repository.RepositoryAnchor.None ||
                    repository.Anchor == Repository.RepositoryAnchor.MostRecent;
                RecentRepoInfo ri = new RecentRepoInfo(repository, mostRecent);
                if (ri.MostRecent)
                    mostRecentRepos.Add(ri);
                else
                    lessRecentRepos.Add(ri);
                if (middleDot)
                    AddToOrderedMiddleDots(orderedRepos, ri);
                else
                    AddToOrderedSignDir(orderedRepos, ri, signDir);
            }
            int r = mostRecentRepos.Count - 1;
            //remove not anchored repos if there is more than maxRecentRepositories repos
            while (mostRecentRepos.Count > n && r >= 0 )
            {
                var repo = mostRecentRepos[r];
                if (repo.Repo.Anchor == Repository.RepositoryAnchor.MostRecent)
                    r--;
                else
                {
                    repo.MostRecent = false;
                    mostRecentRepos.RemoveAt(r);
                }
            }


            Action<bool, List<RecentRepoInfo>> addSortedRepos = delegate(bool mostRecent, List<RecentRepoInfo> addToList)
            {
                foreach (string caption in orderedRepos.Keys)
                {
                    List<RecentRepoInfo> list = orderedRepos[caption];
                    foreach (RecentRepoInfo repo in list)
                        if (repo.MostRecent == mostRecent)
                            addToList.Add(repo);
                }
            };

            Action<List<RecentRepoInfo>, List<RecentRepoInfo>> addNotSortedRepos = delegate(List<RecentRepoInfo> list, List<RecentRepoInfo> addToList)
            {
                foreach (RecentRepoInfo repo in list)
                    addToList.Add(repo);
            };

            if (SortMostRecentRepos)
                addSortedRepos(true, mostRecentRepoList);
            else
                addNotSortedRepos(mostRecentRepos, mostRecentRepoList);

            if (SortLessRecentRepos)
                addSortedRepos(false, lessRecentRepoList);
            else
                addNotSortedRepos(lessRecentRepos, lessRecentRepoList);
        }

        private void AddToOrderedSignDir(SortedList<string, List<RecentRepoInfo>> orderedRepos, RecentRepoInfo repoInfo, bool shortenPath)
        {
            List<RecentRepoInfo> list = null;
            bool existsShortName;
            //if there is no short name for a repo, then try to find unique caption extendig short directory path
            if (shortenPath && repoInfo.DirInfo != null)
            {
                string s = repoInfo.DirName.Substring(repoInfo.DirInfo.FullName.Length);
                if (!s.IsNullOrEmpty())
                    s = Path.GetDirectoryName(s);
                //candidate for short name
                repoInfo.Caption = repoInfo.ShortName;
                if (!s.IsNullOrEmpty())
                    repoInfo.Caption += " (" + s + ")";
                repoInfo.DirInfo = repoInfo.DirInfo.Parent;
            }
            else
                repoInfo.Caption = repoInfo.Repo.Path;

            existsShortName = orderedRepos.TryGetValue(repoInfo.Caption, out list);
            if (!existsShortName)
            {
                list = new List<RecentRepoInfo>();
                orderedRepos.Add(repoInfo.Caption, list);
            }

            List<RecentRepoInfo> tmpList = new List<RecentRepoInfo>();
            if (existsShortName)
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    RecentRepoInfo r = list[i];
                    if (!r.FullPath)
                    {
                        tmpList.Add(r);
                        list.RemoveAt(i);
                    }
                }

            if (repoInfo.FullPath || !existsShortName)
                list.Add(repoInfo);
            else
                tmpList.Add(repoInfo);

            //find unique caption for repos with no title
            foreach (RecentRepoInfo r in tmpList)
                AddToOrderedSignDir(orderedRepos, r, shortenPath);
        }

        private string MakePath(string l, string r)
        {
            if (l == null)
                return r;
            return Path.Combine(l, r);
        }

        private void AddToOrderedMiddleDots(SortedList<string, List<RecentRepoInfo>> orderedRepos, RecentRepoInfo repoInfo)
        {
            DirectoryInfo dirInfo;

            try
            {
                dirInfo = new DirectoryInfo(repoInfo.Repo.Path);
            }
            catch (Exception)
            {
                dirInfo = null;
            }

            if (dirInfo != null)
            {

                string root = null;
                string company = null;
                string repository = null;
                string workingDir = null;


                workingDir = dirInfo.Name;
                dirInfo = dirInfo.Parent;
                if (dirInfo != null)
                {
                    repository = dirInfo.Name;
                    dirInfo = dirInfo.Parent;
                }
                bool addDots = false;

                if (dirInfo != null)
                {
                    while (dirInfo.Parent != null && dirInfo.Parent.Parent != null)
                    {
                        dirInfo = dirInfo.Parent;
                        addDots = true;
                    }
                    company = dirInfo.Name;
                    if (dirInfo.Parent != null)
                        root = dirInfo.Parent.Name;
                }

                Func<int, bool> shortenPathWithCompany = delegate(int skipCount)
                {
                    bool result = false;
                    string c = null;
                    string r = null;
                    if (company != null)
                    {
                        if (company.Length > skipCount)
                        {
                            c = company.Substring(0, company.Length - skipCount);
                            result = true;
                        }
                    }

                    if (repository != null)
                    {
                        if (repository.Length > skipCount)
                        {
                            r = repository.Substring(skipCount, repository.Length - skipCount);
                            result = true;
                        }
                    }

                    repoInfo.Caption = MakePath(root, c);
                    if (addDots)
                        repoInfo.Caption = MakePath(repoInfo.Caption, "..");

                    repoInfo.Caption = MakePath(repoInfo.Caption, r);
                    repoInfo.Caption = MakePath(repoInfo.Caption, workingDir);

                    return result && addDots;
                };


                Func<int, bool> shortenPath = delegate(int skipCount)
                {
                    string path = repoInfo.Repo.Path;
                    string fistDir = (root ?? company) ?? repository;
                    string lastDir = workingDir;
                    if (fistDir != null && path.Length - lastDir.Length - fistDir.Length - skipCount > 0)
                    {

                        int middle = (path.Length - lastDir.Length) / 2 + (path.Length - lastDir.Length) % 2;
                        int leftEnd = middle - skipCount / 2;
                        int rightStart = middle + skipCount / 2 + skipCount % 2;

                        if (leftEnd == rightStart)
                            repoInfo.Caption = path;
                        else
                            repoInfo.Caption = path.Substring(0, leftEnd) + ".." + path.Substring(rightStart, path.Length - rightStart);
                        return true;
                    }

                    return false;
                };

                //if fixed width is not set then short as in pull request vccp's example
                //full "E:\CompanyName\Projects\git\ProductName\Sources\RepositoryName\WorkingDirName"
                //short "E:\CompanyName\...\RepositoryName\WorkingDirName"
                if (this.RecentReposComboMinWidth == 0)
                {
                    shortenPathWithCompany(0);
                }
                //else skip symbols beginning from the middle to both sides, 
                //so we'll see "E:\Compa...toryName\WorkingDirName" and "E:\...\WorkingDirName" at the end.
                else
                {
                    SizeF captionSize;
                    bool canShorten;
                    int skipCount = 0;
                    do
                    {
                        canShorten = shortenPath(skipCount);
                        skipCount++;
                        captionSize = graphics.MeasureString(repoInfo.Caption, measureFont);
                    }
                    while (captionSize.Width > RecentReposComboMinWidth - 10 && canShorten);
                }
            }

            List<RecentRepoInfo> list = null;

            if (!orderedRepos.TryGetValue(repoInfo.Caption, out list))
            {
                list = new List<RecentRepoInfo>();
                orderedRepos.Add(repoInfo.Caption, list);
            }
            list.Add(repoInfo);

        }
    
    }
}
