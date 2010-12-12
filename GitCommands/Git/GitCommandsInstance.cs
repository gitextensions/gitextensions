using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using System.Text;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public sealed class GitCommandsInstance : IGitCommands, IDisposable
    {
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Process CmdStartProcess(string cmd, string arguments)
        {
            try
            {
                GitCommandHelpers.SetEnvironmentVariable();

                var ssh = GitCommandHelpers.UseSsh(arguments);

                Kill();

                Settings.GitLog.Log(cmd + " " + arguments);

                //process used to execute external commands
                Process process = new Process() { StartInfo = GitCommandHelpers.CreateProcessStartInfo() };
                process.StartInfo.CreateNoWindow = (!ssh && !Settings.ShowGitCommandLine);
                process.StartInfo.FileName = cmd;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = Settings.WorkingDir;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.LoadUserProfile = true;
                process.EnableRaisingEvents = true;

                if (!StreamOutput)
                {
                    process.OutputDataReceived += ProcessOutputDataReceived;
                    process.ErrorDataReceived += ProcessErrorDataReceived;
                }
                Output = new StringBuilder();
                ErrorOutput = new StringBuilder();

                process.Exited += ProcessExited;
                process.Start();
                myProcess = process;

                if (!StreamOutput)
                {
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                }

                return process;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error running command: '" + cmd + " " + arguments, ex);
            }
        }

        public void Kill()
        {
            //If there was another process running, kill it
            if (myProcess == null)
                return;
            try
            {
                if (!myProcess.HasExited)
                {
                    myProcess.Kill();
                }
                myProcess.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void Dispose()
        {
            Kill();
        }

        public string RunGit(string arguments)
        {
            return GitCommandHelpers.RunCmd(Settings.GitCommand, arguments);
        }

        public event DataReceivedEventHandler DataReceived;
        public event EventHandler Exited;

        private void ProcessExited(object sender, EventArgs e)
        {
            ExitCode = myProcess.ExitCode;
            if (Exited != null)
            {
                //The process is exited already, but this command waits also until all output is recieved.
                //Only WaitForExit when someone is conntected to the exited event. For some reason a
                //null reference is thrown sometimes when staging/unstaging in the commit dialog when
                //we wait for exit, probably a timing issue... 
                myProcess.WaitForExit();

                Exited(this, e);
            }

            myProcess = null;

        }

        private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (CollectOutput)
                Output.Append(e.Data + Environment.NewLine);
            if (DataReceived != null)
                DataReceived(this, e);
        }

        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (CollectOutput)
                Output.Append(e.Data + Environment.NewLine);
            if (DataReceived != null)
                DataReceived(this, e);
        }

        public IList<IGitSubmodule> GetSubmodules()
        {
            var submodules = GitCommandHelpers.RunCmd(Settings.GitCommand, "submodule status").Split('\n');

            IList<IGitSubmodule> submoduleList = new List<IGitSubmodule>();

            string lastLine = null;

            foreach (var submodule in submodules)
            {
                if (submodule.Length < 43)
                    continue;

                if (submodule.Equals(lastLine))
                    continue;

                lastLine = submodule;

                submoduleList.Add(GitCommandHelpers.CreateGitSubmodule(submodule));
            }

            return submoduleList;
        }

        public bool CollectOutput = true;
        public bool StreamOutput;
        public int ExitCode { get; set; }
        public bool IsRunning { get { return myProcess != null && !myProcess.HasExited; } }
        public StringBuilder Output { get; private set; }
        public StringBuilder ErrorOutput { get; private set; }

        private Process myProcess;
    }
}
