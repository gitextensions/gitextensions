using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GitCommands;

namespace GitUI.CommandsDialogs.CommitDialog
{
    /// <summary>
    /// Represents the commit message after having applied the formatting rules specified in AppSettings
    /// </summary>
    public class FormattedCommitMessage
    {
        /// <summary>
        /// Represents a line that should be colored to indicate it is too long.
        /// This can happen if AutoWrap is not enabled, if it is the commit title,
        /// or if lacks appropriate breaking point (no space).
        /// </summary>
        public class ColoredLine
        {
            public int LineNumber { get; }
            public int From { get; }
            public int Length { get; }

            public ColoredLine(int lineNumber, int from, int length)
            {
                LineNumber = lineNumber;
                From = from;
                Length = length;
            }
        }

        /// <summary>
        /// Represents the modifications from the lines as displayed in the form's Message TextBox
        /// </summary>
        public class MessageUpdate
        {
            /// <summary>
            /// Indicates whether the content should be completely replaced with the FormattedCommitMessage data
            /// </summary>
            public bool NeedFullUpdate { get; }
            /// <summary>
            /// Then new lines that need to be colored.
            /// Only meaningful when no need for a full update.
            /// </summary>
            public IEnumerable<ColoredLine> NewColoredLines { get; }
            /// <summary>
            /// The line where the cursor should be after a full update
            /// </summary>
            public int CursorLine { get; }
            /// <summary>
            /// The column where the cursor should be after a full update
            /// </summary>
            public int CursorColumn { get; }

            private MessageUpdate(bool needFullUpdate, ColoredLine[] newColoredLines, int cursorLine, int cursorColumn)
            {
                NeedFullUpdate = needFullUpdate;
                NewColoredLines = newColoredLines;
                CursorLine = cursorLine;
                CursorColumn = cursorColumn;
            }

            public static MessageUpdate CreateFullUpdate(int cursorLine, int cursorColumn)
            {
                return new MessageUpdate(true, new ColoredLine[0], cursorLine, cursorColumn);
            }

            public static MessageUpdate CreateColorUpdate(IEnumerable<ColoredLine> newColoredLines)
            {
                return new MessageUpdate(false, newColoredLines?.ToArray() ?? new ColoredLine[0], 0, 0);
            }

            public static readonly MessageUpdate NoChange = new MessageUpdate(false, new ColoredLine[0], 0, 0);
        }

        private string _commentChar;

        /// <summary>
        /// The commit message, formatted according to the AppSettings rules
        /// </summary>
        public IEnumerable<string> FormattedLines { get; private set; } = new string[0];

        private ColoredLine[] _coloredLines = new ColoredLine[0];
        /// <summary>
        /// The lines that need additional coloring to indicate they are too long
        /// </summary>
        public IEnumerable<ColoredLine> ColoredLines { get { return _coloredLines; } }

        public FormattedCommitMessage(string commentChar)
        {
            _commentChar = commentChar;
        }

        public MessageUpdate UpdateLines(string[] enteredLines, int cursorLine, int cursorColumn)
        {
            var formattedLines = new List<string>();
            var coloredLines = new List<ColoredLine>();

            FormatLines(new List<string>(enteredLines), formattedLines, coloredLines, ref cursorLine, ref cursorColumn);

            bool needFullUpdate = formattedLines.Count != enteredLines.Length;
            if (!needFullUpdate)
            {
                for (int i = 0; i < enteredLines.Length; i++)
                    if (enteredLines[i] != formattedLines[i])
                    {
                        needFullUpdate = true;
                        break;
                    }
            }
            List<ColoredLine> updatedColoredLines = null;
            if (!needFullUpdate)
            {
                updatedColoredLines = new List<ColoredLine>();
                for (int i = 0, j = 0; i < coloredLines.Count; i++)
                {
                    if (j >= _coloredLines.Length)
                    {
                        updatedColoredLines.Add(coloredLines[i]);
                        continue;
                    }
                    if (coloredLines[i].LineNumber == _coloredLines[j].LineNumber)
                    {
                        if (coloredLines[i].From != _coloredLines[j].From || coloredLines[i].Length != _coloredLines[j].Length)
                            updatedColoredLines.Add(coloredLines[i]);
                        j++;
                        continue;
                    }
                    if (coloredLines[i].LineNumber > _coloredLines[j].LineNumber)
                    {
                        // unlikely for a non-changed line to not need coloring anymore, but possible if settings changed
                        needFullUpdate = true;
                        break;
                    }
                    updatedColoredLines.Add(coloredLines[i]);
                }
            }

            FormattedLines = formattedLines.ToArray();
            _coloredLines = coloredLines.ToArray();
            return needFullUpdate ? MessageUpdate.CreateFullUpdate(cursorLine, cursorColumn) : (updatedColoredLines.Count > 0 ? MessageUpdate.CreateColorUpdate(updatedColoredLines.ToArray()) : MessageUpdate.NoChange);
        }

        private void FormatLines(List<string> lines, List<string> formattedLines, List<ColoredLine> coloredLines, ref int cursorLine, ref int cursorColumn)
        {
            int titleLimit = AppSettings.CommitValidationMaxCntCharsFirstLine;
            int lineLimit = AppSettings.CommitValidationMaxCntCharsPerLine;
            int nonCommentLine = 0;
            for (int line = 0; line < lines.Count; line++)
            {
                if (lines[line].StartsWith(_commentChar))
                {
                    formattedLines.Add(lines[line]);
                    continue;
                }
                if (nonCommentLine == 0)
                {
                    if (titleLimit > 0 && lines[line].Length > titleLimit)
                    {
                        // can never wrap title : just color it
                        coloredLines.Add(new ColoredLine(line, titleLimit, lines[line].Length - titleLimit));
                    }
                    formattedLines.Add(lines[line]);
                    nonCommentLine++;
                    continue;
                }
                if (nonCommentLine == 1 && lines[line] != string.Empty && AppSettings.CommitValidationSecondLineMustBeEmpty)
                {
                    if (AppSettings.CommitValidationIndentAfterFirstLine)
                    {
                        lines[line] = " - " + lines[line];
                        if (cursorLine == line)
                            cursorColumn += 3;
                    }
                    formattedLines.Add(string.Empty);
                    if (line <= cursorLine)
                        cursorLine++;
                }
                if (AppSettings.CommitValidationAutoWrap && lineLimit > 0)
                {
                    ProcessLine(lines, line, lineLimit, ref cursorLine, ref cursorColumn);
                }
                if (lineLimit > 0 && lines[line].Length > lineLimit)
                {
                    // either we don't wrap lines, or the current line has only one word, in either case :
                    coloredLines.Add(new ColoredLine(line, lineLimit, lines[line].Length - lineLimit));
                }
                formattedLines.Add(lines[line]);
                nonCommentLine++;
            }
        }

        private void ProcessLine(List<string> lines, int lineIndex, int lineLimit, ref int cursorLine, ref int cursorColumn)
        {
            string line = lines[lineIndex];
            string indent = FindIndent(line);
            // always trim, except at end of current line when not wrapping
            if (lineIndex == cursorLine && cursorColumn == line.Length)
            {
                // on the current line, we may be "building" the indent, so no trim
                indent = FindIndent(line + "dummy");
                // special case when just adding a space makes line too long
                if (cursorColumn == lineLimit + 1 && line[cursorColumn - 1] == ' ')
                {
                    bool nextLineAvailable;
                    if (lineIndex == lines.Count - 1)
                    {
                        nextLineAvailable = false;
                    }
                    else
                    {
                        string nextLine = lines[lineIndex + 1];
                        string nextLineIndent = FindIndent(nextLine);
                        nextLineAvailable = nextLine.StartsWith(nextLineIndent) && nextLineIndent == indent;
                    }
                    if (!nextLineAvailable)
                    {
                        lines.Insert(lineIndex + 1, indent);
                    }
                    line = line.TrimEnd();
                    lines[lineIndex] = line;
                    cursorLine++;
                    cursorColumn = indent.Length;
                }
                else if (cursorColumn > lineLimit)
                {
                    // will wrap anyway, no need to keep space
                    line = line.TrimEnd();
                    lines[lineIndex] = line;
                }
            }
            else
            {
                line = line.TrimEnd();
                lines[lineIndex] = line;
            }
            if (line.Length > lineLimit)
                WrapLine(lines, lineIndex, lineLimit, indent, ref cursorLine, ref cursorColumn);
            else if (line.Length > 0)
                JoinLine(lines, lineIndex, lineLimit, indent, ref cursorLine, ref cursorColumn);
        }

        private static Regex _indentRegex = new Regex(@"^ *((\+|\-|\*|(\d+\.?)) *)?", RegexOptions.Compiled);
        /// <summary>
        /// We want to keep indentation : blocks that all start with exactly the same number of spaces,
        /// except maybe the first line where "bullets" (including numbering) are part of the indent
        /// </summary>
        private string FindIndent(string line)
        {
            var match = _indentRegex.Match(line.TrimEnd());
            return new string(' ', match.Length);
        }

        private void WrapLine(List<string> lines, int lineIndex, int lineLimit, string indent, ref int cursorLine, ref int cursorColumn)
        {
            int indentSize = indent.Length;
            string line = lines[lineIndex];
            int breakPoint = 0;
            for (int i = indentSize + 1; i < line.Length; i++)
            {
                if (line[i] == ' ' && line[i - 1] != ' ')
                {
                    if (i <= lineLimit || breakPoint == 0)
                        breakPoint = i;
                    if (i > lineLimit)
                        break;
                }
            }
            if (breakPoint == 0)
                // only one already too long word in this line : nothing to be done
                return;
            bool moveCursor = lineIndex == cursorLine && cursorColumn > breakPoint;
            int newCursorColumn = moveCursor ? cursorColumn - breakPoint : 0;
            lines[lineIndex] = line.Substring(0, breakPoint);
            while (line[breakPoint] == ' ')
            {
                breakPoint++;
                if (moveCursor && newCursorColumn > 0)
                    newCursorColumn--;
            }
            var remaining = line.Substring(breakPoint);
            bool canPrependNextLine = lineIndex + 1 < lines.Count && !lines[lineIndex + 1].StartsWith(_commentChar) &&
                lines[lineIndex + 1].Length > indentSize && lines[lineIndex + 1].StartsWith(indent) && lines[lineIndex + 1][indentSize] != ' ';
            if (canPrependNextLine)
            {
                lines[lineIndex + 1] = indent + remaining + " " + lines[lineIndex + 1].Substring(indentSize);
                if (cursorLine == lineIndex + 1 && cursorColumn >= indentSize)
                    cursorColumn += remaining.Length + 1;
            }
            else
            {
                lines.Insert(lineIndex + 1, indent + remaining);
                if (cursorLine > lineIndex)
                    cursorLine++;
            }
            if (moveCursor)
            {
                cursorLine++;
                cursorColumn = newCursorColumn + indentSize;
            }
        }

        private void JoinLine(List<string> lines, int lineIndex, int lineLimit, string indent, ref int cursorLine, ref int cursorColumn)
        {
            string line = lines[lineIndex];
            int indentSize = indent.Length;
            string joiningSpace = line.EndsWith(" ") ? "" : " ";
            int available = lineLimit - line.Length - joiningSpace.Length;
            while (available > 0)
            {
                // cannot join last line
                if (lineIndex == lines.Count - 1)
                    return;

                string nextLine = lines[lineIndex + 1].TrimEnd();
                string nextLineIndent = FindIndent(nextLine);
                bool canJoinNextLine = nextLine.Length > 0 && !nextLine.StartsWith(_commentChar) &&
                    nextLine.StartsWith(indent) &&  // only join for plain spaces, not "bullet" indent
                    nextLineIndent == indent;
                if (!canJoinNextLine)
                    return;

                // can we join it all ?
                if (nextLine.Length - indentSize <= available)
                {
                    if (cursorLine == lineIndex + 1)
                        cursorColumn += line.Length + joiningSpace.Length - indentSize;
                    if (cursorLine > lineIndex)
                        cursorLine--;
                    line += joiningSpace + nextLine.Substring(indentSize);
                    lines[lineIndex] = line;
                    lines.RemoveAt(lineIndex + 1);
                    joiningSpace = line.EndsWith(" ") ? "" : " ";
                    available = lineLimit - line.Length - joiningSpace.Length;
                    continue;
                }
                // else take as much as possible
                int breakPoint = 0;
                for (int i = indentSize + 1; i < nextLine.Length && i - indentSize <= available; i++)
                {
                    if (nextLine[i] == ' ' && nextLine[i - 1] != ' ')
                        breakPoint = i; 
                }
                if (breakPoint > 0)
                {
                    if (cursorLine == lineIndex + 1)
                    {
                        if (cursorColumn <= indentSize)
                        {
                            cursorLine = lineIndex;
                            cursorColumn = line.Length + joiningSpace.Length;
                        }
                        else if (cursorColumn <= breakPoint)
                        {
                            cursorLine = lineIndex;
                            cursorColumn += line.Length + joiningSpace.Length - indentSize;
                        }
                    }
                    line += joiningSpace + nextLine.Substring(indentSize, breakPoint - indentSize);
                    lines[lineIndex] = line;
                    int breakPointEnd = breakPoint;
                    while (nextLine[breakPointEnd] == ' ')
                        breakPointEnd++;
                    if (cursorLine == lineIndex + 1)
                    {
                        if (cursorColumn > breakPointEnd)
                            cursorColumn = indentSize + (cursorColumn - breakPointEnd);
                        else if (cursorColumn > breakPoint)
                            cursorColumn = indentSize;
                    }
                    lines[lineIndex + 1] = indent + nextLine.Substring(breakPointEnd);
                }
                // don't iterate now that we couldn't take a full line
                available = 0;
            }
        }
    }
}
