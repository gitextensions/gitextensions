using GitExtensions.Plugins.GitlabIntegration.ApiClient.Models;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitlabIntegrationTests
{
    internal class GitlabAdapterTestData
    {
        static GitlabAdapterTestData()
        {
            Builds = new Dictionary<int, Tuple<BuildInfo, GitlabPipeline>>();

            Builds.Add(1,
                new Tuple<BuildInfo, GitlabPipeline>(
                    new BuildInfo
                    {
                        Id = "1",
                        CommitHashList = new[] { ObjectId.Parse("ea6e12b9fbf6da0a558f62e1dc6eaf9d52c22f74") },
                        Duration = 0
                    },
                    new GitlabPipeline
                    {
                        id = 1,
                        sha = "ea6e12b9fbf6da0a558f62e1dc6eaf9d52c22f74"
                    }));

            Builds.Add(2,
                new Tuple<BuildInfo, GitlabPipeline>(
                    new BuildInfo
                    {
                        Id = "2",
                        CommitHashList = new[] { ObjectId.Parse("0ecc29cbb3ceaa4d12411ceadae9c500e93c9255") },
                        Duration = 0
                    },
                    new GitlabPipeline
                    {
                        id = 2,
                        sha = "0ecc29cbb3ceaa4d12411ceadae9c500e93c9255"
                    }));

            Builds.Add(3,
                new Tuple<BuildInfo, GitlabPipeline>(
                    new BuildInfo
                    {
                        Id = "3",
                        CommitHashList = new[] { ObjectId.Parse("4ed8079785c070fdaf273771511901e4746c7164") },
                        Duration = 0
                    },
                    new GitlabPipeline
                    {
                        id = 3,
                        sha = "4ed8079785c070fdaf273771511901e4746c7164"
                    }));

            Builds.Add(4,
                new Tuple<BuildInfo, GitlabPipeline>(
                    new BuildInfo
                    {
                        Id = "4",
                        CommitHashList = new[] { ObjectId.Parse("afe7fd31ae5156f1aff74874693c0fec7b8cd50a") },
                        Duration = 0
                    },
                    new GitlabPipeline
                    {
                        id = 4,
                        sha = "afe7fd31ae5156f1aff74874693c0fec7b8cd50a"
                    }));

            Builds.Add(5,
                new Tuple<BuildInfo, GitlabPipeline>(
                    new BuildInfo
                    {
                        Id = "5",
                        CommitHashList = new[] { ObjectId.Parse("df0b59d03b11ee5d70c0db11bfcf99fc862df56f") },
                        Duration = 0
                    },
                    new GitlabPipeline
                    {
                        id = 5,
                        sha = "df0b59d03b11ee5d70c0db11bfcf99fc862df56f"
                    }));

            Builds.Add(6,
                new Tuple<BuildInfo, GitlabPipeline>(
                    new BuildInfo
                    {
                        Id = "6",
                        CommitHashList = new[] { ObjectId.Parse("2b2fb529ba9f24646e5a3a6daf1c42a27940ac26") },
                        Duration = 0
                    },
                    new GitlabPipeline
                    {
                        id = 6,
                        sha = "2b2fb529ba9f24646e5a3a6daf1c42a27940ac26"
                    }));

            Builds.Add(7,
                new Tuple<BuildInfo, GitlabPipeline>(
                    new BuildInfo
                    {
                        Id = "7",
                        CommitHashList = new[] { ObjectId.Parse("cd4090d807d75f165a3cf0000a1773422fdf56ca") },
                        Duration = 0,
                        Status = BuildInfo.BuildStatus.InProgress
                    },
                    new GitlabPipeline
                    {
                        id = 7,
                        sha = "cd4090d807d75f165a3cf0000a1773422fdf56ca",
                        status = "running"
                    }));
        }

        public static Dictionary<int, Tuple<BuildInfo, GitlabPipeline>> Builds;
    }
}
