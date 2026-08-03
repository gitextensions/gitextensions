using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit.Document;
using GitExtensions.Extensibility.Git;
using MediaColor = Avalonia.Media.Color;

namespace GitUI.Editor;

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

    private readonly Dictionary<char, (string longForm, MediaColor color, string[] options)> _commandByFirstChar;

    public RebaseTodoHighlightingStrategy(IGitModule module)
        : base("GitRebaseTodo", module)
    {
        bool isDark = Avalonia.Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
        _commandByFirstChar = new()
        {
            { 'p', ("pick", ColorNormal, []) },
            { 'r', ("reword", isDark ? Colors.MediumPurple : Colors.Purple, []) },
            { 'e', ("edit", isDark ? Colors.LightGray : Colors.DarkGray, []) },
            { 's', ("squash", isDark ? Colors.CornflowerBlue : Colors.DarkBlue, []) },
            { 'f', ("fixup", isDark ? Colors.Coral : Colors.LightCoral, new[] { "-C", "-c" }) },
            { 'x', ("exec", Colors.Gray, []) },
            { 'd', ("drop", isDark ? Colors.IndianRed : Colors.Red, []) }
        };
    }

    protected override void MarkTokens(TextDocument document, DocumentLine line)
    {
        if (!TryHighlightComment(document, line)
            && !TryHighlightInteractiveRebaseCommand(document, line))
        {
            SetStyle(line.Offset, line.EndOffset, ColorNormal);
        }
    }

    private enum State
    {
        Command,
        SpacesAfterCommand,
        Id
    }

    private bool TryHighlightInteractiveRebaseCommand(TextDocument document, DocumentLine line)
    {
        if (line.Length < 1)
        {
            return false;
        }

        char c = document.GetCharAt(line.Offset);

        if (!_commandByFirstChar.TryGetValue(c, out (string longForm, MediaColor color, string[] options) cmd))
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

                        SetStyle(line.Offset, line.Offset + idStartIndex, cmd.color, bold: true);
                        SetStyle(line.Offset + idStartIndex, line.Offset + index, cmd.color, bold: true);
                        SetStyle(line.Offset + index, line.EndOffset, ColorNormal);
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
