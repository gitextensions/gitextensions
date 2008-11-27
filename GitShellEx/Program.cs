using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using GitUI;
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
            Application.Run(new FormBrowse());
            
        }
    }
}