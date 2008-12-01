using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using GitUI;
using System.IO;
namespace FileHashShell
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            GitCommands.Settings.WorkingDir = Directory.GetCurrentDirectory();
            Application.Run(new FormBrowse());
            
        }
    }
}