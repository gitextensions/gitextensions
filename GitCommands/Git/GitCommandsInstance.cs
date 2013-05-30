using System;
using System.Diagnostics;
using System.Security.Permissions;
using System.Text;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public delegate void SetupStartInfo(ProcessStartInfo startInfo);
    
    public sealed class GitCommandsInstance : IDisposable
    {
        private Process _myProcess;
        private readonly object _processLock = new object();

        public SetupStartInfo SetupStartInfoCallback { get; set; }
        public readonly string WorkingDirectory;

        public GitCommandsInstance(IGitModule module)
            : this(module.GitWorkingDir)
        {
        }

        public GitCommandsInstance(string aWorkingDirectory)
        {
            WorkingDirectory = aWorkingDirectory;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Process CmdStartProcess(string cmd, string arguments)
        {
            try
            {
                GitCommandHelpers.SetEnvironmentVariable();

                var ssh = GitCommandHelpers.UseSsh(arguments);

                Kill();

                string quotedCmd = cmd;
                if (quotedCmd.IndexOf(' ') != -1)
                    quotedCmd = quotedCmd.Quote();
                Settings.GitLog.Log(quotedCmd + " " + arguments);

                //process used to execute external commands
                var process = new Process { StartInfo = GitCommandHelpers.CreateProcessStartInfo(null) };
                process.StartInfo.CreateNoWindow = (!ssh && !Settings.ShowGitCommandLine);
                process.StartInfo.FileName = cmd;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = WorkingDirectory;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.LoadUserProfile = true;
                if (SetupStartInfoCallback != null)
                    SetupStartInfoCallback(process.StartInfo);
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
                lock (_processLock)
                {
                    _myProcess = process;
                }
                if (!StreamOutput)
                {
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                }

                return process;
            }
            catch (Exception ex)
            {
                ex.Data.Add("command", cmd);
                ex.Data.Add("arguments", arguments);
                throw;
            }
        }

        public void Kill()
        {
            lock (_processLock)
            {
                //If there was another process running, kill it
                if (_myProcess == null)
                    return;
                try
                {
                    if (!_myProcess.HasExited)
                    {
                        _myProcess.Exited -= ProcessExited;
                        _myProcess.TerminateTree();
                    }
                    if (_myProcess != null) // process is null here if filter is a slow diff
                    {
                        _myProcess.Close();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
        }

        public void Dispose()
        {
            Kill();
        }

        public event DataReceivedEventHandler DataReceived;
        public event EventHandler Exited;

        private void ProcessExited(object sender, EventArgs e)
        {
            try
            {
                if (_myProcess != null)
                {
                    ExitCode = _myProcess.ExitCode;
                    if (Exited != null)
                    {
                        //The process is exited already, but this command waits also until all output is received.
                        //Only WaitForExit when someone is connected to the exited event. For some reason a
                        //null reference is thrown sometimes when staging/unstaging in the commit dialog when
                        //we wait for exit, probably a timing issue... 
                        _myProcess.WaitForExit();

                        Exited(this, e);
                    }

                    lock (_processLock)
                    {
                        _myProcess = null;
                    }
                }
            }
            catch
            {
            }
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

        public bool CollectOutput = true;
        public bool StreamOutput;
        public int ExitCode { get; set; }
        public bool IsRunning { get { return _myProcess != null && !_myProcess.HasExited; } }
        public StringBuilder Output { get; private set; }
        public StringBuilder ErrorOutput { get; private set; }
    }
}
