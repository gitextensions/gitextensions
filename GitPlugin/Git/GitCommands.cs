using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace GitPlugin.Git
{
    public static class GitCommands
    {
        private static string GetRegistryValue(RegistryKey root, string subkey, string key)
        {
            try
            {
                RegistryKey rk;
                rk = root.OpenSubKey(subkey, false);

                string value = "";

                if (rk != null && rk.GetValue(key) is string)
                {
                    value = rk.GetValue(key).ToString();
                    rk.Flush();
                    rk.Close();
                }

                return value;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("GitExtensions has insufficient permissions to check the registry.");
            }
            return "";
        }

        private static string GetGitExRegValue(string key)
        {
            string result = GetRegistryValue(Registry.CurrentUser, "Software\\GitExtensions", key);

            if (string.IsNullOrEmpty(result))
                result = GetRegistryValue(Registry.Users, "Software\\GitExtensions", key);

            return result;
        }

        private static ProcessStartInfo CreateStartInfo(string command, string arguments, string workingDir, Encoding encoding = null)
        {
            return new ProcessStartInfo
            {
                UseShellExecute = false,
                ErrorDialog = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = encoding,
                StandardErrorEncoding = encoding,
                FileName = command,
                Arguments = arguments,
                WorkingDirectory = workingDir
            };
        }

        public static Process RunGitEx(string command, string filename)
        {
            if (!string.IsNullOrEmpty(filename))
                command += " \"" + filename + "\"";

            string path = GetGitExRegValue("InstallDir");

            ProcessStartInfo startInfo = CreateStartInfo(path + "\\GitExtensions.exe", command, new FileInfo(filename).DirectoryName, Encoding.UTF8);

            try
            {
                return Process.Start(startInfo);
            }
            catch
            {
                return null;
            }
        }

        public static string RunGitExWait(string command, string filename)
        {
            using (var process = RunGitEx(command, filename))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
        }

        private static string RunGit(string arguments, string filename, out int exitCode)
        {
            string gitcommand = GetGitExRegValue("gitcommand");

            ProcessStartInfo startInfo = CreateStartInfo(gitcommand, arguments, filename);

            using (var process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                exitCode = process.ExitCode;
                process.WaitForExit();
                return output;
            }
        }

        public static string GetCurrentBranch(string fileName)
        {
            try
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    string head;
                    string headFileName = FindGitWorkingDir(fileName) + ".git\\HEAD";
                    if (File.Exists(headFileName))
                    {
                        head = File.ReadAllText(headFileName);
                        if (!head.Contains("ref:"))
                            head = "no branch";
                    }
                    else
                    {
                        int exitCode;
                        head = RunGit("symbolic-ref HEAD", new FileInfo(fileName).DirectoryName, out exitCode);
                        if (exitCode == 1)
                            head = "no branch";
                    }
                    
                    if (!string.IsNullOrEmpty(head))
                    {
                        head = head.Replace("ref:", "").Trim().Replace("refs/heads/", string.Empty);
                        return head;
                    }
                }
            }
            catch
            {
                //ignore
            }

            return string.Empty;
        }

        private static string FindGitWorkingDir(string startDir)
        {
            if (string.IsNullOrEmpty(startDir))
                return "";

            if (!startDir.EndsWith("\\") && !startDir.EndsWith("/"))
                startDir += "\\";

            var dir = startDir;

            while (dir.LastIndexOfAny(new[] { '\\', '/' }) > 0)
            {
                dir = dir.Substring(0, dir.LastIndexOfAny(new[] { '\\', '/' }));

                if (ValidWorkingDir(dir))
                    return dir + "\\";
            }
            return startDir;
        }

        private static bool ValidWorkingDir(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return false;

            if (Directory.Exists(dir + "\\" + ".git") || File.Exists(dir + "\\" + ".git"))
                return true;

            return !dir.Contains(".git") &&
                   Directory.Exists(dir + "\\" + "info") &&
                   Directory.Exists(dir + "\\" + "objects") &&
                   Directory.Exists(dir + "\\" + "refs");
        }

        public static bool GetShowCurrentBranchSetting()
        {
            string showCurrentBranchSetting = GetGitExRegValue("ShowCurrentBranchInVS");
            return string.IsNullOrEmpty(showCurrentBranchSetting) || showCurrentBranchSetting.Equals("true", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}