using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;



namespace GitCommandTest
{

    public struct PROCESS_INFORMATION
    {

        public IntPtr hProcess;

        public IntPtr hThread;

        public uint dwProcessId;

        public uint dwThreadId;

    }



    public struct STARTUPINFO
    {

        public uint cb;

        public string lpReserved;

        public string lpDesktop;

        public string lpTitle;

        public uint dwX;

        public uint dwY;

        public uint dwXSize;

        public uint dwYSize;

        public uint dwXCountChars;

        public uint dwYCountChars;

        public uint dwFillAttribute;

        public uint dwFlags;

        public short wShowWindow;

        public short cbReserved2;

        public IntPtr lpReserved2;

        public IntPtr hStdInput;

        public IntPtr hStdOutput;

        public IntPtr hStdError;

    }



    public struct SECURITY_ATTRIBUTES
    {

        public int length;

        public IntPtr lpSecurityDescriptor;

        public bool bInheritHandle;

    }


    /// <summary>
    /// Summary description for PushTest
    /// </summary>
    [TestClass]
    public class PushTest
    {

        [DllImport("kernel32.dll")]

        static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes,
                                        bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, 
                                        string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo,out PROCESS_INFORMATION lpProcessInformation);


        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        public StringBuilder Output { get; set; }
        public StringBuilder ErrorOutput { get; set; }

        void process_Exited(object sender, EventArgs e)
        {

        }

        void process_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Output.Append(e.Data + "\n");

        }

        void process_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Output.Append(e.Data + "\n");
        }

        System.Diagnostics.Process Process;

        [TestMethod]
        public void PushToWebTest()
        {
            STARTUPINFO si = new STARTUPINFO();
            si.dwFlags = 0x00000008;
            si.lpDesktop = "winsta0\\default";


            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

            IDictionary x = Environment.GetEnvironmentVariables();
            x["SSH_ASKPASS"] = "C:/Program Files/Git/libexec/git-core/git-gui--askpass";
            x["DISPLAY"] = "localhost:0";
            //CreateProcess("C:\\WINDOWS\\SYSTEM32\\Calc.exe", null, IntPtr.Zero, IntPtr.Zero, false, 0, IntPtr.Zero, null, ref si, out pi);
            //CreateProcess(@"C:\\WINDOWS\\notepad.exe", null, IntPtr.Zero, IntPtr.Zero, false,0x00000008 , IntPtr.Zero, null, ref si, out pi);
            CreateProcess(@"C:\Program Files\Git\bin\ssh.exe", @"""""C:\Program Files\Git\bin\ssh.exe"" git@github.com""", IntPtr.Zero, IntPtr.Zero, false, 0x00000008, IntPtr.Zero, null, ref si, out pi);
            CreateProcess(@"C:\Program Files\Git\libexec\git-core\git-push.exe", @"""C:\Program Files\Git\libexec\git-core\git-push.exe""", IntPtr.Zero, IntPtr.Zero, false, 0x00000008, IntPtr.Zero, @"f:\GitExtensions\", ref si, out pi);
            
            return;

            //process used to execute external commands
            Process = new System.Diagnostics.Process();
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.ErrorDialog = false;
            //Process.Deta
            Process.StartInfo.RedirectStandardOutput = false;
            Process.StartInfo.RedirectStandardInput = true;
            Process.StartInfo.RedirectStandardError = false;
            
            //Process.StartInfo.FileName.CreateNoWindow = false;
            //Process.StartInfo.FileName = @"git.cmd";
            //Process.StartInfo.Arguments = "push origin refs/heads/master:refs/heads/master";//"git@github.com:spdr870/gitextensions.git";
            //Process.StartInfo.EnvironmentVariables.Add("ASK_PASS", "C:/Program Files/Git/libexec/git-core/git-gui--askpass");
            //Process.StartInfo.EnvironmentVariables.Add("SSH_ASKPASS", "C:/Program Files/Git/libexec/git-core/git-gui--askpass");
            //Process.StartInfo.EnvironmentVariables.Add("ASK_PASS", "C:/Program Files/Git/libexec/git-core/git-gui--askpass");
            //Process.StartInfo.EnvironmentVariables.Add("ASKPASS", "C:/Program Files/Git/libexec/git-core/git-gui--askpass");
            Process.StartInfo.EnvironmentVariables["DISPLAY"] = "x";
            //Process.StartInfo.EnvironmentVariables["HOME"] = "%USERPROFILE%";
            Process.StartInfo.EnvironmentVariables["SSH_ASKPASS"] = "C:/Program Files/Git/libexec/git-core/git-gui--askpass";
            Process.StartInfo.EnvironmentVariables["path"] = @"C:\Program Files\Git\cmd;C:\Program Files\Git\bin;C:\Program Files\Git\mingw\bin;%PATH%";

            foreach (DictionaryEntry key in Process.StartInfo.EnvironmentVariables)
            {

            }

            Process.StartInfo.FileName = @"C:\Program Files\Git\cmd\test.cmd";
            Process.StartInfo.Arguments = "";


            Process.StartInfo.WorkingDirectory = @"F:\GitExtensions\";


            Process.StartInfo.FileName = @"perl";//@"C:\Program Files\Git\cmd\test.cmd";
            Process.StartInfo.Arguments = @"c:\temp\p git@github.com";//"git@github.com:spdr870/gitextensions.git";

            Process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            Process.StartInfo.LoadUserProfile = true;
            Process.StartInfo.FileName = @"wish.exe";//@"C:\Program Files\Git\cmd\test.cmd";
            Process.StartInfo.Arguments = "\"C:/Program Files/Git/test.tlc\"";
            Process.StartInfo.FileName = @"C:\Program Files\Git\cmd\git.cmd";//@"C:\Program Files\Git\cmd\test.cmd";
            Process.StartInfo.Arguments = "push";
            Process.StartInfo.FileName = @"ssh.exe";//@"C:\Program Files\Git\cmd\test.cmd";
            Process.StartInfo.Arguments = "git@github.com";//"git@github.com:spdr870/gitextensions.git";

            Output = new StringBuilder();
            ErrorOutput = new StringBuilder();

            Process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_OutputDataReceived);
            Process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_ErrorDataReceived);

            //Process.EnableRaisingEvents = true;
            //Process.Exited += new EventHandler(process_Exited);
            Process.Start();
            Process.StandardInput.Close();
            Process.Close();

            //Process.StandardInput.Close();
            //Process.StandardInput = null;

            //int n = 0;
            /*
            do
            {
                do{
                n = Process.StandardOutput.Read();
                Output.Append((char)n);
                } while (n > -1);
                do
                {
                n = Process.StandardError.Read();
                Output.Append((char)n);
                } while (n > -1);
                Output.Append((char)n);
            } while (n > -1);
            */
            //Process.BeginErrorReadLine();
            //Process.BeginOutputReadLine();
            //Process.WaitForExit();
        }
    }
}
