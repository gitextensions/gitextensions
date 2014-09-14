// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainForm.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics;

namespace NBug.Examples.WinForms
{
    using System;
    using System.Runtime.CompilerServices;
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

        private void RollStack(int index, string text, Exception e)
        {
            if (text.Length == 0)
                throw e;
            if (index == text.Length)
                return;
            char c = text[index];
            if (c == ',')
            {
                try
                {
                    RollStack(0, text.Substring(index + 1), e);
                    return;
                }
                catch (Exception ex)
                {
                    throw new Exception(text.Substring(0, index), ex);
                }
            }
            switch (((int)c - '0') % 10)
            {
                case 0: Zero(index + 1, text, e); break;
                case 1: One(index + 1, text, e); break;
                case 2: Two(index + 1, text, e); break;
                case 3: Three(index + 1, text, e); break;
                case 4: Four(index + 1, text, e); break;
                case 5: Five(index + 1, text, e); break;
                case 6: Six(index + 1, text, e); break;
                case 7: Seven(index + 1, text, e); break;
                case 8: Eight(index + 1, text, e); break;
                case 9: Nine(index + 1, text, e); break;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Zero(int index, string text, Exception e) { RollStack(index, text, e); throw e; }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void One(int index, string text, Exception e) { RollStack(index, text, e); throw e; }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Two(int index, string text, Exception e) { RollStack(index, text, e); throw e; }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Three(int index, string text, Exception e) { RollStack(index, text, e); throw e; }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Four(int index, string text, Exception e) { RollStack(index, text, e); throw e; }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Five(int index, string text, Exception e) { RollStack(index, text, e); throw e; }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Six(int index, string text, Exception e) { RollStack(index, text, e); throw e; }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Seven(int index, string text, Exception e) { RollStack(index, text, e); throw e; }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Eight(int index, string text, Exception e) { RollStack(index, text, e); throw e; }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Nine(int index, string text, Exception e) { RollStack(index, text, e); throw e; }

        private void CrashButton_Click(object sender, EventArgs e)
        {
            var text = "Selected exception: '" + this.crashTypeComboBox.Text + "' was thrown.";
            switch (this.crashTypeComboBox.Text)
            {
                case "UI Thread: System.Exception":
                    RollStack(0, StackTextBox.Text, new Exception(text));
                    break;
                case "UI Thread: System.ArgumentException":
                    RollStack(0, StackTextBox.Text, new ArgumentException(
                        text,
                        "MyInvalidParameter",
                        new Exception("Test inner exception for argument exception.")));
                    break;
                case "Background Thread (Task): System.Exception":
                    Task.Factory.StartNew(() => { RollStack(0, StackTextBox.Text, new Exception(text)); });

                    // Below code makes sure that exception is thrown as only after finalization, the aggregateexception is thrown.
                    // As a side affect, unlike the normal behavior, the application will not continue its execution but will shut
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