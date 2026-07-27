using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;

namespace GitExtensions.Plugins.ReleaseNotesGenerator;

internal static class HtmlFragment
{
    private static readonly DataFormat<string> HtmlFormat = DataFormat.CreateStringPlatformFormat(
        OperatingSystem.IsWindows()
            ? "HTML Format"
            : OperatingSystem.IsMacOS()
                ? "public.html"
                : "text/html");

    internal static string To8DigitString(int value) => $"{value:00000000}";

    internal static string CreateHtmlFormatClipboardText(string htmlFragment, Uri? sourceUri = null)
    {
        System.Text.StringBuilder sb = new();
        const string header = "Version:0.9\r\n" +
                              "StartHTML:<<<<<<<1\r\n" +
                              "EndHTML:<<<<<<<2\r\n" +
                              "StartFragment:<<<<<<<3\r\n" +
                              "EndFragment:<<<<<<<4\r\n";
        sb.Append(header);

        if (sourceUri is not null)
        {
            sb.AppendFormat("SourceURL:{0}", sourceUri);
        }

        int startHtml = sb.Length;
        const string pre = "<html><body>\r\n<!--StartFragment-->";
        sb.Append(pre);
        int fragmentStart = sb.Length;

        sb.Append(htmlFragment);
        int fragmentEnd = sb.Length;

        const string post = "<!--EndFragment-->\r\n</body></html>";
        sb.Append(post);
        int endHtml = sb.Length;

        sb.Replace("<<<<<<<1", To8DigitString(startHtml));
        sb.Replace("<<<<<<<2", To8DigitString(endHtml));
        sb.Replace("<<<<<<<3", To8DigitString(fragmentStart));
        sb.Replace("<<<<<<<4", To8DigitString(fragmentEnd));
        return sb.ToString();
    }

    internal static DataTransfer CreateClipboardData(string htmlFragment, Uri? sourceUri = null)
    {
        string richHtml = OperatingSystem.IsWindows()
            ? CreateHtmlFormatClipboardText(htmlFragment, sourceUri)
            : htmlFragment;
        DataTransferItem item = new();
        item.SetText(htmlFragment);
        item.Set(HtmlFormat, richHtml);

        DataTransfer data = new();
        data.Add(item);
        return data;
    }

    public static async Task CopyToClipboardAsync(TopLevel owner, string htmlFragment, Uri? sourceUri = null)
    {
        Avalonia.Input.Platform.IClipboard? clipboard = TopLevel.GetTopLevel(owner)?.Clipboard;
        if (clipboard is null)
        {
            return;
        }

        using DataTransfer data = CreateClipboardData(htmlFragment, sourceUri);
        await clipboard.SetDataAsync(data);
    }
}
