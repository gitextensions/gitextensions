using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace GitUI
{
    public static class OpenWith
    {
        public static void OpenAs(string file)
        {
            string path = file;
            Process p = new Process();
            ProcessStartInfo pi = new ProcessStartInfo("rundll32.exe");
            pi.UseShellExecute = false;
            pi.RedirectStandardOutput = true;
            pi.Arguments = "shell32.dll,OpenAs_RunDLL " + path;
            p.StartInfo = pi;
            p.Start();

        }
    }
}
