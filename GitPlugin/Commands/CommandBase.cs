// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using System;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using Microsoft.Win32;

namespace GitPlugin.Commands
{
	public abstract class CommandBase
    {
        abstract public void OnCommand(DTE2 application, OutputWindowPane pane);
        abstract public bool IsEnabled(DTE2 application);

        public void RunGitEx(string command, string filename)
        {
            command += " \"" + filename + "\"";


            string path = GetRegistryValue(Registry.CurrentUser, "Software\\GitExtensions\\GitExtensions\\1.0.0.0", "InstallDir");

            if (string.IsNullOrEmpty(path))
                path = GetRegistryValue(Registry.Users, "Software\\GitExtensions\\GitExtensions\\1.0.0.0", "InstallDir");


            Run(path + "\\GitExtensions.exe", command);
        }

        protected string GetRegistryValue(RegistryKey root, string subkey, string key)
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
            catch (UnauthorizedAccessException ex)
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

    }
}
