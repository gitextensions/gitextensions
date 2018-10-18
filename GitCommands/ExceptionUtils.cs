using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GitCommands
{
    public static class ExceptionUtils
    {
        /// <summary>
        /// Determines whether the <paramref name="ex"/> contains an inner exception of the desired type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Desired exception type.</typeparam>
        /// <param name="ex">The exception to inspect.</param>
        /// <returns><see langword="true"/> if one of the inner exceptions are of desired type; otherwise <see langword="false"/>.</returns>
        public static bool HasInnerOfType<T>(this Exception ex)
           where T : Exception
        {
            return ex.InnerOfType<T>() != null;
        }

        /// <summary>
        /// Determines whether the <paramref name="ex"/> contains an inner exception of the desired type <typeparamref name="T"/> and returns it, if found.
        /// </summary>
        /// <typeparam name="T">Desired exception type.</typeparam>
        /// <param name="ex">The exception to inspect.</param>
        /// <returns>The inner exceptions are of desired type; otherwise <see langword="null"/>.</returns>
        public static T InnerOfType<T>(this Exception ex)
             where T : Exception
        {
            var iex = ex.InnerException;
            while (iex != null)
            {
                if (iex is T variable)
                {
                    return variable;
                }

                iex = iex.InnerException;
            }

            return null;
        }

        public static void ShowException(Exception e, bool canIgnore = true)
        {
            ShowException(e, string.Empty, canIgnore);
        }

        public static void ShowException(Exception e, string info, bool canIgnore = true)
        {
            ShowException(null, e, info, canIgnore);
        }

        public static void ShowException(IWin32Window owner, Exception e, string info, bool canIgnore)
        {
            if (!(canIgnore && IsIgnorable(e)))
            {
                MessageBox.Show(owner, string.Join(info, Environment.NewLine + Environment.NewLine, e.ToStringWithData()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static bool IsIgnorable(Exception e)
        {
            return e is ThreadAbortException;
        }

        public static string ToStringWithData(this Exception e)
        {
            var sb = new StringBuilder();
            sb.AppendLine(e.Message);
            sb.AppendLine();
            foreach (DictionaryEntry entry in e.Data)
            {
                sb.AppendLine(entry.Key + " = " + entry.Value);
            }

            return sb.ToString();
        }
    }
}
