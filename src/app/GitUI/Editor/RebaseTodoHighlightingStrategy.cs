using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor
{
    internal sealed class RebaseTodoHighlightingStrategy : GitHighlightingStrategyBase
    {
        /*
        Commands:
        p, pick = use commit
        r, reword = use commit, but edit the commit message
        e, edit = use commit, but stop for amending
        s, squash = use commit, but meld into previous commit
        f, fixup = like "squash", but discard this commit's log message
        x, exec = run command (the rest of the line) using shell
        d, drop = remove commit
        */

        private readonly Dictionary<char, (string longForm, HighlightColor color, string[] options)> _commandByFirstChar = new()
        {
            { 'p', ("pick", new HighlightColor(nameof(SystemColors.InfoText), bold: true, italic: false), Array.Empty<string>()) },
            { 'r', ("reword", new HighlightColor(Application.IsDarkModeEnabled ? Color.MediumPurple : Color.Purple, bold: true, italic: false, adaptable: false), Array.Empty<string>()) },
            { 'e', ("edit", new HighlightColor(Application.IsDarkModeEnabled ? Color.LightGray : Color.DarkGray, bold: true, italic: false, adaptable: false), Array.Empty<string>()) },
            { 's', ("squash", new HighlightColor(Application.IsDarkModeEnabled ? Color.CornflowerBlue : Color.DarkBlue, bold: true, italic: false, adaptable: false), Array.Empty<string>()) },
            { 'f', ("fixup", new HighlightColor(Application.IsDarkModeEnabled ? Color.Coral : Color.LightCoral, bold: true, italic: false, adaptable: false), new[] { "-C", "-c" }) },
            { 'x', ("exec", new HighlightColor(nameof(SystemColors.GrayText), bold: true, italic: false), Array.Empty<string>()) },
            { 'd', ("drop", new HighlightColor(Application.IsDarkModeEnabled ? Color.IndianRed : Color.Red, bold: true, italic: false, adaptable: false), Array.Empty<string>()) }
        };

        public RebaseTodoHighlightingStrategy(string? commentString)
            : base("GitRebaseTodo", commentString)
        {
        }

        protected override void MarkTokens(IDocument document, IList<LineSegment> lines)
        {
            foreach (LineSegment line in lines)
            {
                if (!TryHighlightComment(document, line) &&
                    !TryHighlightInteractiveRebaseCommand(document, line))
                {
                    line.Words = new List<TextWord>(capacity: 1)
                        { new(document, line, 0, line.Length, ColorNormal, hasDefaultColor: true) };
                }

                document.RequestUpdate(
                    new TextAreaUpdate(TextAreaUpdateType.SingleLine, line.LineNumber));
            }
        }

        private enum State
        {
            Command,
            SpacesAfterCommand,
            Id
        }

        private bool TryHighlightInteractiveRebaseCommand(IDocument document, LineSegment line)
        {
            if (line.Length < 1)
            {
                return false;
            }

            char c = document.GetCharAt(line.Offset);

            if (!_commandByFirstChar.TryGetValue(c, out (string longForm, HighlightColor color, string[] options) cmd))
            {
                return false;
            }

            State state = State.Command;
            int index = 1;
            int idStartIndex = -1;

            while (index < line.Length)
            {
                c = document.GetCharAt(line.Offset + index);

                if (c == '\r' || c == '\n')
                {
                    return false;
                }

                switch (state)
                {
                    case State.Command:
                    {
                        if (index == 1 && char.IsWhiteSpace(c))
                        {
                            state = State.SpacesAfterCommand;
                        }
                        else if (index == cmd.longForm.Length && char.IsWhiteSpace(c))
                        {
                            state = State.SpacesAfterCommand;
                        }
                        else if (index >= cmd.longForm.Length || c != cmd.longForm[index])
                        {
                            return false;
                        }

                        if (state == State.SpacesAfterCommand)
                        {
                            string? option = cmd.options.FirstOrDefault(o => index + 1 + o.Length < line.Length && document.GetText(line.Offset + index + 1, o.Length) == o);
                            if (option is not null)
                            {
                                index += option.Length + 1;

                                continue;
                            }
                        }

                        break;
                    }

                    case State.SpacesAfterCommand:
                    {
                        if (IsHexChar())
                        {
                            idStartIndex = index;
                            state = State.Id;
                        }
                        else if (!char.IsWhiteSpace(c))
                        {
                            return false;
                        }

                        break;
                    }

                    case State.Id:
                    {
                        if (char.IsWhiteSpace(c))
                        {
                            int idLength = index - idStartIndex;

                            if (idLength < 4)
                            {
                                return false;
                            }

                            line.Words = new List<TextWord>(capacity: 3)
                            {
                                new(document, line, 0, idStartIndex, cmd.color, hasDefaultColor: false),
                                new(document, line, idStartIndex, idLength, cmd.color, hasDefaultColor: false),
                                new(document, line, index, line.Length - index, ColorNormal, hasDefaultColor: true)
                            };

                            return true;
                        }

                        if (!IsHexChar())
                        {
                            return false;
                        }

                        break;
                    }
                }

                index++;
            }

            return false;

            bool IsHexChar() => char.IsDigit(c) || c is >= 'a' and <= 'f';
        }
    }
}
