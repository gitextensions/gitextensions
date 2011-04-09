using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace GitPlugin.Git
{
    public static class GitCommands
    {
        public static void RunGitEx(string command, string filename)
        {
            if (!string.IsNullOrEmpty(filename))
                command += " \"" + filename + "\"";


            string path = GetRegistryValue(Registry.CurrentUser, "Software\\GitExtensions\\GitExtensions\\1.0.0.0", "InstallDir");

            if (string.IsNullOrEmpty(path))
                path = GetRegistryValue(Registry.Users, "Software\\GitExtensions\\GitExtensions\\1.0.0.0", "InstallDir");

            Run(path + "\\GitExtensions.exe", command);
        }

        public static string RunGit(string arguments, string filename, out int exitCode)
        {
            string gitcommand = GetRegistryValue(Registry.CurrentUser, "Software\\GitExtensions\\GitExtensions\\1.0.0.0", "gitcommand");

            if (string.IsNullOrEmpty(gitcommand))
                gitcommand = GetRegistryValue(Registry.Users, "Software\\GitExtensions\\GitExtensions\\1.0.0.0", "gitcommand");


            ProcessStartInfo startInfo = new ProcessStartInfo
                       {
                           UseShellExecute = false,
                           ErrorDialog = false,
                           RedirectStandardOutput = true,
                           RedirectStandardInput = true,
                           RedirectStandardError = true
                       };
            startInfo.CreateNoWindow = true;
            startInfo.FileName = gitcommand;
            startInfo.Arguments = arguments;
            startInfo.WorkingDirectory = filename;
            startInfo.LoadUserProfile = true;

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
            if (!string.IsNullOrEmpty(fileName))
            {
                string head;
                string headFileName = GitCommands.FindGitWorkingDir(fileName) + ".git\\HEAD";
                if (File.Exists(headFileName))
                {
                    head = File.ReadAllText(headFileName);
                    if (!head.Contains("ref:"))
                        head = "no branch";
                }
                else
                {
                    int exitCode;
                    head = GitCommands.RunGit("symbolic-ref HEAD", new FileInfo(fileName).DirectoryName, out exitCode);
                    if (exitCode == 1)
                        head = "no branch";
                }

                if (!string.IsNullOrEmpty(head))
                {
                    head = head.Replace("ref:", "").Trim().Replace("refs/heads/", string.Empty);
                    return string.Concat(" (", head, ")");
                }

            }

            return string.Empty;
        }

        public static string FindGitWorkingDir(string startDir)
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

        public static bool ValidWorkingDir(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return false;

            if (Directory.Exists(dir + "\\" + ".git"))
                return true;

            return !dir.Contains(".git") &&
                   Directory.Exists(dir + "\\" + "info") &&
                   Directory.Exists(dir + "\\" + "objects") &&
                   Directory.Exists(dir + "\\" + "refs");
        }

        public static string GetRegistryValue(RegistryKey root, string subkey, string key)
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
                MessageBox.Show("GitExtensions has insufficient permisions to check the registry.");
            }
            return "";
        }
        public static void Run(string cmd, string arguments)
        {
            try
            {
                //process used to execute external commands
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = cmd;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                process.StartInfo.LoadUserProfile = true;

                process.Start();
                //process.WaitForExit();
            }
            catch
            {
            }
        }

        public static bool GetShowCurrentBranchSetting()
        {
            string showCurrentBranchSetting = GetRegistryValue(Registry.CurrentUser, "Software\\GitExtensions\\GitExtensions\\1.0.0.0", "showcurrentbranch");

            if (string.IsNullOrEmpty(showCurrentBranchSetting))
                showCurrentBranchSetting = GetRegistryValue(Registry.Users, "Software\\GitExtensions\\GitExtensions\\1.0.0.0", "showcurrentbranch");

            return showCurrentBranchSetting != null && showCurrentBranchSetting.Equals("True", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
