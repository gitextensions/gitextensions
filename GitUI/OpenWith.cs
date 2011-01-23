using System.Diagnostics;

namespace GitUI
{
    public static class OpenWith
    {
        public static void OpenAs(string file)
        {
            string path = file;
            var pi = new ProcessStartInfo("rundll32.exe")
                         {
                             UseShellExecute = false,
                             RedirectStandardOutput = true,
                             Arguments = "shell32.dll,OpenAs_RunDLL " + path
                         };
            var p = new Process { StartInfo = pi };
            p.Start();

        }
    }
}
