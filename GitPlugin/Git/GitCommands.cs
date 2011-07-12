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
        public static void RunGitEx(string command, string filename)
        {
            if (!string.IsNullOrEmpty(filename))
                command += " \"" + filename + "\"";

            string path = GetGitExRegValue("InstallDir");
            Run(path + "\\GitExtensions.exe", command);
        }

        private static string RunGit(string arguments, string filename, out int exitCode)
        {
            string gitcommand = GetGitExRegValue("gitcommand");

            ProcessStartInfo startInfo = new ProcessStartInfo();
            SetupProcessStartInfo(startInfo, gitcommand, arguments, filename, false, false);

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

            if (Directory.Exists(dir + "\\" + ".git"))
                return true;

            return !dir.Contains(".git") &&
                   Directory.Exists(dir + "\\" + "info") &&
                   Directory.Exists(dir + "\\" + "objects") &&
                   Directory.Exists(dir + "\\" + "refs");
        }

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
                MessageBox.Show("GitExtensions has insufficient permisions to check the registry.");
            }
            return "";
        }

        private static string GetGitExRegValue(string key)
        {
            string result = GetRegistryValue(Registry.CurrentUser, "Software\\GitExtensions\\GitExtensions\\1.0.0.0", key);

            if (string.IsNullOrEmpty(result))
                result = GetRegistryValue(Registry.Users, "Software\\GitExtensions\\GitExtensions\\1.0.0.0", key);

            return result;
        }

        private static void SetupProcessStartInfo(ProcessStartInfo startInfo, string command, string arguments, string workingDir, bool useUTF8, bool show)
        {
            startInfo.UseShellExecute = false;
            startInfo.ErrorDialog = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            if (useUTF8)
            {
                startInfo.StandardOutputEncoding = Encoding.UTF8;
                startInfo.StandardErrorEncoding = Encoding.UTF8;
            }
            startInfo.CreateNoWindow = true;
            startInfo.LoadUserProfile = true;

            startInfo.FileName = command;
            startInfo.Arguments = arguments;
            startInfo.WorkingDirectory = workingDir;
            if (show)
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
        }

        private static void Run(string cmd, string arguments)
        {
            try
            {
                //process used to execute external commands
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                SetupProcessStartInfo(process.StartInfo, cmd, arguments, "", true, true);

                process.Start();
                //process.WaitForExit();
            }
            catch
            {
            }
        }

        public static bool GetShowCurrentBranchSetting()
        {
            string showCurrentBranchSetting = GetGitExRegValue("showcurrentbranchinvisualstudio");
            return showCurrentBranchSetting != null && showCurrentBranchSetting.Equals("True", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}