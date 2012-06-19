using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections;

namespace GitCommands
{
    public static class ExceptionUtils
    {

        public static void ShowException(Exception e)
        {
            ShowException(e, true);      
        }

        public static void ShowException(Exception e, bool canIgnore)
        {
            ShowException(e, string.Empty, canIgnore);
        }

        public static void ShowException(Exception e, string info)
        {
            ShowException(e, info, true);
        }

        public static void ShowException(Exception e, string info, bool canIgnore)
        {
            if (!(canIgnore && IsIgnorable(e)))
                MessageBox.Show(info.Join(Environment.NewLine + Environment.NewLine, e.ToStringWithData()));            
        }

        public static bool IsIgnorable(Exception e)
        {
            return e is ThreadAbortException;
        }

        public static string ToStringWithData(this Exception e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(e.ToString());
            sb.AppendLine();
            foreach(DictionaryEntry entry in e.Data)
                sb.AppendLine(entry.Key + " = " + entry.Value);
            return sb.ToString();
        }
    }
}
