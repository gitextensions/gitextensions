// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainForm.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics;

namespace NBug.Examples.WinForms
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using NBug.Events;

    public partial class MainForm : Form
    {
        public MainForm()
        {
            this.InitializeComponent();

            //we want to display custom dialog to the user when bug occurs (optional)
            Settings.CustomUIEvent += this.Settings_CustomUIEvent;

            //we want to add custom submission processing to our application (optional)
            Settings.CustomSubmissionEvent += Settings_CustomSubmissionEvent; 

            this.crashTypeComboBox.SelectedIndex = 0;
        }

        private unsafe void AccessViolation()
        {
            var b = *(byte*)8762765876;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CrashButton_Click(object sender, EventArgs e)
        {
            switch (this.crashTypeComboBox.Text)
            {
                case "UI Thread: System.Exception":
                    throw new Exception("Selected exception: '" + this.crashTypeComboBox.Text + "' was thrown.");
                case "UI Thread: System.ArgumentException":
                    throw new ArgumentException(
                        "Selected exception: '" + this.crashTypeComboBox.Text + "' was thrown.",
                        "MyInvalidParameter",
                        new Exception("Test inner exception for argument exception."));
                case "Background Thread (Task): System.Exception":
                    Task.Factory.StartNew(() => { throw new Exception(); });

                    // Below code makes sure that exception is thrown as only after finalization, the aggregateexception is thrown.
                    // As a side affect, unlike the normal behavior, the applicaiton will note continue its execution but will shut
                    // down just like any main thread exceptions, even if there is no handle to UnobservedTaskException!
                    // So remove below 3 lines to observe the normal continuation behavior.
                    Thread.Sleep(200);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    break;
                case "Process Corrupted State Exception: Access Violation":
                    this.AccessViolation();
                    break;
            }
        }

        /// <summary>
        /// Handles CustomUIEvent to show custom dialog when bug has occured
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Settings_CustomUIEvent(object sender, CustomUIEventArgs e)
        {
            var Form = new Normal();
            e.Result = Form.ShowDialog(e.Report);
        }

        /// <summary>
        /// Handles CustomSubmissionEvent to submit bug
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Settings_CustomSubmissionEvent(object sender, CustomSubmissionEventArgs e)
        {
            Debug.WriteLine(string.Format("Custom submission for exception {0}", e.Exception.Message));
            e.Result = true;
        }
    }
}