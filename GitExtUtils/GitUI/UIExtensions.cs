using System.Text;

namespace GitUI
{
    public static class UIExtensions
    {
        public static bool? GetNullableChecked(this CheckBox chx)
        {
            if (chx.CheckState == CheckState.Indeterminate)
            {
                return null;
            }
            else
            {
                return chx.Checked;
            }
        }

        public static void SetNullableChecked(this CheckBox chx, bool? @checked)
        {
            if (@checked.HasValue)
            {
                chx.CheckState = @checked.Value ? CheckState.Checked : CheckState.Unchecked;
            }
            else
            {
                chx.CheckState = CheckState.Indeterminate;
            }
        }

        public static bool IsFixedWidth(this Font ft, Graphics g)
        {
            char[] charSizes = { 'i', 'a', 'Z', '%', '#', 'a', 'B', 'l', 'm', ',', '.' };
            float charWidth = g.MeasureString("I", ft).Width;

            bool fixedWidth = true;

            foreach (char c in charSizes)
            {
                if (Math.Abs(g.MeasureString(c.ToString(), ft).Width - charWidth) > float.Epsilon)
                {
                    fixedWidth = false;
                }
            }

            return fixedWidth;
        }

        /// <summary>
        /// bodyOrSubject
        /// Notes:
        ///     notes
        /// </summary>
        public static string FormatBodyAndNotes(string bodyOrSubject, string? notes)
        {
            if (string.IsNullOrWhiteSpace(notes))
            {
                return bodyOrSubject;
            }

            const string notesPrefix = "Notes:";
            const string indent = "    ";

            // trying to avoid buffer re-allocation during Append()
            StringBuilder? sb = new StringBuilder((bodyOrSubject?.Length ?? 0) + 2 + notesPrefix.Length + 2 + indent.Length + notes.Length + 1)
                .AppendLine(bodyOrSubject)
                .AppendLine(notesPrefix);

            foreach (string line in notes.Split('\n'))
            {
                sb.Append(indent).Append(line).Append('\n');
            }

            --sb.Length; // removing the last artificially appended \n
            return sb.ToString();
        }
    }
}
