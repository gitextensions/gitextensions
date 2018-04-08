using System.Collections.Generic;
using System.Drawing;
using GitCommands;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using JetBrains.Annotations;

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

        private static readonly Dictionary<char, (string longForm, HighlightColor color)> _commandByFirstChar = new Dictionary<char, (string longForm, HighlightColor color)>
        {
            { 'p', ("pick", new HighlightColor(Color.Black, bold: true, italic: false)) },
            { 'r', ("reword", new HighlightColor(Color.Purple, bold: true, italic: false)) },
            { 'e', ("edit", new HighlightColor(Color.Black, bold: true, italic: false)) },
            { 's', ("squash", new HighlightColor(Color.DarkBlue, bold: true, italic: false)) },
            { 'f', ("fixup", new HighlightColor(Color.LightCoral, bold: true, italic: false)) },
            { 'x', ("exec", new HighlightColor(Color.Black, bold: true, italic: false)) },
            { 'd', ("drop", new HighlightColor(Color.Red, bold: true, italic: false)) }
        };

        public RebaseTodoHighlightingStrategy([NotNull] GitModule module)
            : base("GitRebaseTodo", module)
        {
        }

        protected override void MarkTokens(IDocument document, IList<LineSegment> lines)
        {
            foreach (var line in lines)
            {
                if (!TryHighlightComment(document, line) &&
                    !TryHighlightInteractiveRebaseCommand(document, line))
                {
                    line.Words = new List<TextWord>(capacity: 1)
                        { new TextWord(document, line, 0, line.Length, ColorNormal, hasDefaultColor: true) };
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

        private static bool TryHighlightInteractiveRebaseCommand(IDocument document, LineSegment line)
        {
            if (line.Length < 1)
            {
                return false;
            }

            var c = document.GetCharAt(line.Offset);

            if (!_commandByFirstChar.TryGetValue(c, out var cmd))
            {
                return false;
            }

            var state = State.Command;
            var index = 1;
            var idStartIndex = -1;

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
                            var idLength = index - idStartIndex;

                            if (idLength <= 5)
                            {
                                return false;
                            }

                            line.Words = new List<TextWord>(capacity: 3)
                            {
                                new TextWord(document, line, 0, idStartIndex, cmd.color, hasDefaultColor: false),
                                new TextWord(document, line, idStartIndex, idLength, cmd.color, hasDefaultColor: false),
                                new TextWord(document, line, index, line.Length - index, ColorNormal, hasDefaultColor: true)
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

            bool IsHexChar() => char.IsDigit(c) || (c >= 'a' && c <= 'f');
        }
    }
}