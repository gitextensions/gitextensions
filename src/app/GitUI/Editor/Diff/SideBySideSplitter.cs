using System.Text.RegularExpressions;

namespace GitUI.Editor.Diff;

/// <summary>
/// Splits a unified diff patch into two column texts (old file lines, new file lines),
/// aligned so that equal line counts form matching rows - the "side by side" layout known from VS Code.
/// </summary>
public static partial class SideBySideSplitter
{
    [GeneratedRegex(@"^@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@", RegexOptions.ExplicitCapture)]
    private static partial Regex HunkHeaderRegex();

    [GeneratedRegex(@"^(diff |index |--- |\+\+\+ |@@ |new file mode|old mode|new mode|deleted file mode|similarity index|rename from|rename to|copy from|copy to)", RegexOptions.ExplicitCapture)]
    private static partial Regex MetadataLineRegex();

    [GeneratedRegex(@"\x1B\[[0-9;]*[A-Za-z]", RegexOptions.ExplicitCapture)]
    private static partial Regex AnsiEscapeRegex();

    /// <summary>
    /// A single rendered row in one pane.
    /// </summary>
    public sealed class ColumnLine
    {
        public string Text = "";
        public DiffLineType Type = DiffLineType.Context;
        public int LineNumber;
    }

    public sealed class SplitResult
    {
        public List<ColumnLine> Left = [];
        public List<ColumnLine> Right = [];
    }

    /// <summary>
    /// Converts a unified diff into two aligned column lists. Returns null when the text
    /// does not look like a unified patch (caller keeps showing the unified view).
    /// </summary>
    public static SplitResult? TrySplit(string patch)
    {
        if (string.IsNullOrEmpty(patch))
        {
            return null;
        }

        // diffs rendered with git coloring contain inline ANSI escape sequences which would
        // break the hunk-header and line-prefix parsing - strip them first
        patch = AnsiEscapeRegex().Replace(patch, "");

        string[] lines = patch.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        // drop the trailing artifact(s) created by the final newline of the patch
        int end = lines.Length;
        while (end > 0 && lines[end - 1].Length == 0)
        {
            end--;
        }

        bool sawHunk = false;
        List<(char Op, string Content, int OldLine, int NewLine)> rows = [];

        int oldLine = 0;
        int newLine = 0;
        for (int idx = 0; idx < end; idx++)
        {
            string line = lines[idx].EndsWith("\r") ? lines[idx][..^1] : lines[idx];

            Match m = HunkHeaderRegex().Match(line);
            if (m.Success)
            {
                sawHunk = true;
                oldLine = int.Parse(m.Groups[1].Value);
                newLine = int.Parse(m.Groups[3].Value);
                continue;
            }

            // git encodes blank context lines as a single space; a bare empty line is only
            // padding between metadata and hunks
            if (line.Length == 0)
            {
                continue;
            }

            if (!sawHunk)
            {
                if (MetadataLineRegex().IsMatch(line))
                {
                    continue;
                }

                return null; // free-form text, not a patch
            }

            char op = line[0];
            if (op == '\\')
            {
                // "\ No newline at end of file" - visually attached to the previous line
                continue;
            }

            switch (op)
            {
                case ' ':
                    rows.Add((' ', line[1..], oldLine, newLine));
                    oldLine++;
                    newLine++;
                    break;
                case '-':
                    rows.Add(('-', line[1..], oldLine, newLine));
                    oldLine++;
                    break;
                case '+':
                    rows.Add(('+', line[1..], oldLine, newLine));
                    newLine++;
                    break;
                default:
                    // any metadata (e.g. a second file header of a multi-file patch) after the
                    // first hunk means this is not a single-file patch - fall back
                    return null;
            }
        }

        if (!sawHunk)
        {
            return null;
        }

        // Pair removals/additions into aligned rows; context lines fill both columns.
        SplitResult result = new();
        int i = 0;
        while (i < rows.Count)
        {
            (char Op, string Content, int OldLine, int NewLine) row = rows[i];
            if (row.Op == ' ')
            {
                result.Left.Add(new ColumnLine { Text = row.Content, Type = DiffLineType.Context, LineNumber = row.OldLine });
                result.Right.Add(new ColumnLine { Text = row.Content, Type = DiffLineType.Context, LineNumber = row.NewLine });
                i++;
            }
            else if (row.Op == '-')
            {
                // gather the removal block
                int j = i;
                while (j < rows.Count && rows[j].Op == '-')
                {
                    j++;
                }

                int k = j;
                while (k < rows.Count && rows[k].Op == '+')
                {
                    k++;
                }

                int removed = j - i;
                int added = k - j;
                int paired = Math.Min(removed, added);
                for (int p = 0; p < paired; p++)
                {
                    result.Left.Add(new ColumnLine { Text = rows[i + p].Content, Type = DiffLineType.Minus, LineNumber = rows[i + p].OldLine });
                    result.Right.Add(new ColumnLine { Text = rows[j + p].Content, Type = DiffLineType.Plus, LineNumber = rows[j + p].NewLine });
                }

                for (int p = paired; p < removed; p++)
                {
                    result.Left.Add(new ColumnLine { Text = rows[i + p].Content, Type = DiffLineType.Minus, LineNumber = rows[i + p].OldLine });
                    result.Right.Add(new ColumnLine());
                }

                for (int p = paired; p < added; p++)
                {
                    result.Left.Add(new ColumnLine());
                    result.Right.Add(new ColumnLine { Text = rows[j + p].Content, Type = DiffLineType.Plus, LineNumber = rows[j + p].NewLine });
                }

                i = k;
            }
            else
            {
                // '+' without a preceding '-': addition with no removed counterpart
                result.Left.Add(new ColumnLine());
                result.Right.Add(new ColumnLine { Text = row.Content, Type = DiffLineType.Plus, LineNumber = row.NewLine });
                i++;
            }
        }

        return result;
    }
}
