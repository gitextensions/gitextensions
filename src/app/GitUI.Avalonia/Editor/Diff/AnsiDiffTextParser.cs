using System.Text;

namespace GitUI.Editor.Diff;

internal static class AnsiDiffTextParser
{
    public static string Parse(string text, List<DiffTextMarker> markers)
    {
        StringBuilder output = new(text.Length);
        DiffMarkerKind? activeKind = null;
        int activeStart = 0;

        for (int index = 0; index < text.Length;)
        {
            if (text[index] != '\u001b' || index + 1 >= text.Length || text[index + 1] != '[')
            {
                output.Append(text[index++]);
                continue;
            }

            int commandEnd = text.IndexOf('m', index + 2);
            if (commandEnd < 0)
            {
                output.Append(text[index++]);
                continue;
            }

            AddActiveMarker();
            ReadOnlySpan<char> command = text.AsSpan(index + 2, commandEnd - index - 2);
            activeKind = GetKind(command, activeKind);
            activeStart = output.Length;
            index = commandEnd + 1;
        }

        AddActiveMarker();
        return output.ToString();

        void AddActiveMarker()
        {
            if (activeKind is not null && output.Length > activeStart)
            {
                markers.Add(new DiffTextMarker(activeStart, output.Length - activeStart, activeKind.Value));
            }
        }
    }

    private static DiffMarkerKind? GetKind(ReadOnlySpan<char> command, DiffMarkerKind? current)
    {
        if (command.IsEmpty)
        {
            return null;
        }

        DiffMarkerKind? result = current;
        foreach (Range range in command.Split(';'))
        {
            if (!int.TryParse(command[range], out int code))
            {
                continue;
            }

            result = code switch
            {
                0 or 39 or 49 => null,
                31 or 41 or 91 or 101 => DiffMarkerKind.Removed,
                32 or 42 or 92 or 102 => DiffMarkerKind.Added,
                35 or 36 or 45 or 46 or 95 or 96 or 105 or 106 => DiffMarkerKind.MovedRemoved,
                33 or 34 or 43 or 44 or 93 or 94 or 103 or 104 => DiffMarkerKind.MovedAdded,
                _ => result,
            };
        }

        return result;
    }
}
