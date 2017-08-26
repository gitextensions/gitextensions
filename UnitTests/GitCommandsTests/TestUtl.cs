using GitCommands;
using System;
using System.IO;

namespace GitCommandsTests
{
    public class TestUtl
    {
        public static string TemporaryReposPath = BuildPath();

        public static GitModule CreateEmptyRepo()
        {
            string repoPath = CreateEmptyTestRepoStructure();
            return new GitModule(repoPath);
        }

        private static string CreateEmptyTestRepoStructure()
        {
            //create from scratch a new repository in a tmp location
            //or copy predefined repository structure to a tmp location


            string path = BuildTempPath();
            GitModule module = new GitModule(path);
            module.Init(false, false);
            return path;
        }

        public static void Touch(string parentDir, string fileName, string fileContent)
        {
            string filePath = Path.Combine(parentDir, fileName);
            string dir = Path.GetDirectoryName(filePath);

            var newFile = !File.Exists(filePath);

            Directory.CreateDirectory(dir);

            File.WriteAllText(filePath, fileContent ?? string.Empty);
        }

        protected static string BuildTempPath()
        {
            return Path.Combine(TemporaryReposPath, Path.GetRandomFileName());
        }

        public static string BuildPath()
        {
            string tempPath = null;

            const string LibGit2TestPath = "LibGit2TestPath";

            // We're running on .Net/Windows
            if (Environment.GetEnvironmentVariables().Contains(LibGit2TestPath))
            {
                //Trace.TraceInformation("{0} environment variable detected", LibGit2TestPath);
                tempPath = Environment.GetEnvironmentVariables()[LibGit2TestPath] as String;
            }

            if (String.IsNullOrWhiteSpace(tempPath) || !Directory.Exists(tempPath))
            {
                //  Trace.TraceInformation("Using default test path value");
                tempPath = Path.GetTempPath();
            }

            //workaround macOS symlinking its temp folder
            if (tempPath.StartsWith("/var"))
            {
                tempPath = "/private" + tempPath;
            }

            string testWorkingDirectory = Path.Combine(tempPath, "LibGit2Sharp-TestRepos");
            //  Trace.TraceInformation("Test working directory set to '{0}'", testWorkingDirectory);
            return testWorkingDirectory;
        }
    }
}
