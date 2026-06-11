using System.Text;

namespace GitExtensions.Extensibility.Extensions;

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
        ReadOnlySpan<char> charSizes = ['i', 'a', 'Z', '%', '#', 'a', 'B', 'l', 'm', ',', '.'];
        float charWidth = g.MeasureString("I", ft).Width;

        foreach (char c in charSizes)
        {
            if (Math.Abs(g.MeasureString(c.ToString(), ft).Width - charWidth) > float.Epsilon)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// bodyOrSubject
    /// Notes:
    ///     notes
    /// </summary>
    public static string FormatBodyAndNotes(string bodyOrSubject, string? notes)
    {
        if (string.IsNullOrEmpty(notes))
        {
            return bodyOrSubject;
        }

        const string notesPrefix = "Notes:";
        const string indent = "    ";

        // trying to avoid buffer re-allocation during Append()
        StringBuilder sb = new(bodyOrSubject.Length + 4 + notesPrefix.Length + 2 + indent.Length + notes.Length + 1);
        if (!string.IsNullOrEmpty(bodyOrSubject))
        {
            sb.AppendLine(bodyOrSubject);
        }

        sb.AppendLine().AppendLine(notesPrefix);

        ReadOnlySpan<char> notesAsSpan = notes.AsSpan();
        foreach (Range range in notesAsSpan.Split('\n'))
        {
            sb.Append(indent).Append(notesAsSpan[range]).Append('\n');
        }

        --sb.Length; // removing the last artificially appended \n
        return sb.ToString();
    }
}
