using System.Threading;
using System.Windows.Forms;

namespace GitUI.UserControls
{
  /// <summary>
  /// <para>Base control for executing a console process, as used by the <see cref="FormProcess"/>.</para>
  /// <para>Switches between the basic impl which redirects stdout and integration of a real interactive terminal window into the form, if available.</para>
  /// </summary>
  public abstract class ConsoleProcessControl : Control
  {
  }

  /// <summary>
  /// Uses an edit box and process output streams redirection.
  /// </summary>
  public sealed class EditboxBasedConsoleProcessControl : ConsoleProcessControl
  {
        private ProcessOutputTimer outpuTimer;

    public EditboxBasedConsoleProcessControl()
    {
            outpuTimer = new ProcessOutputTimer(AppendMessageCrossThread);
            syncContext = SynchronizationContext.Current;
    }
        protected readonly SynchronizationContext syncContext;


        public void AppendMessageCrossThread(string text)
        {
            if (syncContext == SynchronizationContext.Current)
                AppendMessage(text);
            else
                syncContext.Post(o => AppendMessage(text), this);
        }

        private void AddMessageToTimer(string text)
        {
            if (outpuTimer != null)
                outpuTimer.Append(text);
        }

        private void AppendMessage(string text)
        {
            //if not disposed
            if (outpuTimer != null)
            {
                MessageTextBox.Text += text;
                MessageTextBox.SelectionStart = MessageTextBox.Text.Length;
                MessageTextBox.ScrollToCaret();
                MessageTextBox.Visible = true;
            }
        }

  }
}