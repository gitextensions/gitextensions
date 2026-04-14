using System.Text;
using GitExtUtils.GitUI.Theming;
using Markdig;

namespace GitUI.Editor;

/// <summary>
///  Converts Markdown text to a full HTML document with embedded CSS,
///  suitable for rendering in a WebView2 control.
/// </summary>
internal static class MarkdownToHtmlConverter
{
    private static readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAutoLinks()
        .UseEmphasisExtras()
        .UseTaskLists()
        .Build();

    /// <summary>
    ///  Converts the specified Markdown text to a complete HTML document
    ///  with inline CSS that adapts to the current theme.
    /// </summary>
    /// <param name="markdown"> The markdown source text.</param>
    /// <param name="disableScrolling">
    ///  When <see langword="true"/>, adds <c>overflow: hidden</c> to the body and
    ///  injects JavaScript to forward wheel events to the host via
    ///  <c>window.chrome.webview.postMessage</c>. Use when the WebView2 is embedded
    ///  in a scrollable WinForms container (e.g. CommitInfo).
    /// </param>
    public static string Convert(string markdown, bool disableScrolling = false)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return string.Empty;
        }

        // Strip BOM that prevents Markdig from recognising headings at the start
        if (markdown[0] == '\uFEFF')
        {
            markdown = markdown[1..];
        }

        string bodyHtml = Markdown.ToHtml(markdown, _pipeline);
        return WrapInHtmlDocument(bodyHtml, disableScrolling);
    }

    private static string WrapInHtmlDocument(string bodyHtml, bool disableScrolling)
    {
        bool isDark = Application.IsDarkModeEnabled;

        Color background = SystemColors.Window;
        Color foreground = SystemColors.WindowText;
        Color codeBg = background.MakeBackgroundDarkerBy(isDark ? -0.08 : 0.05);
        Color borderColor = background.MakeBackgroundDarkerBy(isDark ? -0.15 : 0.15);
        Color blockquoteFg = isDark
            ? Color.FromArgb(145, 152, 161)
            : Color.FromArgb(101, 109, 118);
        Color blockquoteBorder = isDark
            ? Color.FromArgb(61, 68, 77)
            : Color.FromArgb(208, 215, 222);
        Color linkColor = isDark
            ? Color.FromArgb(68, 147, 248)
            : Color.FromArgb(3, 102, 214);
        Color ruleColor = borderColor;
        Color tableRowAlt = background.MakeBackgroundDarkerBy(isDark ? -0.04 : 0.02);
        Color h6Color = isDark
            ? Color.FromArgb(145, 152, 161)
            : Color.FromArgb(101, 109, 118);

        StringBuilder sb = new();

        // GitHub-flavoured markdown CSS, adapted from github-markdown-css (MIT license).
        // Colours are computed from SystemColors for theme integration.
        sb.Append($$"""
            <!DOCTYPE html>
            <html>
            <head>
            <meta charset="utf-8">
            <style>
            * { box-sizing: border-box; }
            body {
                font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", "Noto Sans", Helvetica, Arial, sans-serif;
                font-size: 12px;
                color: {{FormatColor(foreground)}};
                background: {{FormatColor(background)}};
                margin: 8px 16px;
                line-height: 1.5;
                word-wrap: break-word;{{(disableScrolling ? "\n                overflow: hidden;" : "")}}
            }
            body > *:first-child { margin-top: 0 !important; }
            body > *:last-child { margin-bottom: 0 !important; }
            h1, h2, h3, h4, h5, h6 {
                margin-top: 1.5rem;
                margin-bottom: 1rem;
                font-weight: 600;
                line-height: 1.25;
            }
            h1 {
                font-size: 2em;
                padding-bottom: 0.3em;
                border-bottom: 1px solid {{FormatColor(borderColor)}};
            }
            h2 {
                font-size: 1.5em;
                padding-bottom: 0.3em;
                border-bottom: 1px solid {{FormatColor(borderColor)}};
            }
            h3 { font-size: 1.25em; }
            h4 { font-size: 1em; }
            h5 { font-size: 0.875em; }
            h6 { font-size: 0.85em; color: {{FormatColor(h6Color)}}; }
            p, blockquote, ul, ol, dl, table, pre, details {
                margin-top: 0;
                margin-bottom: 1rem;
            }
            ul, ol {
                padding-left: 2em;
            }
            ul ul, ul ol, ol ol, ol ul {
                margin-top: 0;
                margin-bottom: 0;
            }
            ol ol, ul ol {
                list-style-type: lower-roman;
            }
            ul ul ol, ul ol ol, ol ul ol, ol ol ol {
                list-style-type: lower-alpha;
            }
            li + li {
                margin-top: 0.25em;
            }
            li > p {
                margin-top: 1rem;
            }
            b, strong {
                font-weight: 600;
            }
            a {
                color: {{FormatColor(linkColor)}};
                text-decoration: none;
            }
            a:hover {
                text-decoration: underline;
            }
            code, tt, samp {
                font-family: ui-monospace, SFMono-Regular, "SF Mono", Menlo, Consolas, "Liberation Mono", monospace;
                font-size: 85%;
            }
            code, tt {
                padding: 0.2em 0.4em;
                margin: 0;
                white-space: break-spaces;
                background-color: {{FormatColor(codeBg)}};
                border-radius: 6px;
            }
            pre {
                padding: 1rem;
                overflow: auto;
                font-size: 85%;
                line-height: 1.45;
                background-color: {{FormatColor(codeBg)}};
                border-radius: 6px;
                margin-top: 0;
                margin-bottom: 1rem;
            }
            pre code, pre tt {
                display: inline;
                padding: 0;
                margin: 0;
                background: transparent;
                border: 0;
                white-space: pre;
                word-wrap: normal;
                font-size: 100%;
            }
            blockquote {
                margin: 0 0 1rem 0;
                padding: 0 1em;
                color: {{FormatColor(blockquoteFg)}};
                border-left: 0.25em solid {{FormatColor(blockquoteBorder)}};
            }
            blockquote > :first-child { margin-top: 0; }
            blockquote > :last-child { margin-bottom: 0; }
            hr {
                height: 0.25em;
                padding: 0;
                margin: 1.5rem 0;
                background-color: {{FormatColor(ruleColor)}};
                border: 0;
            }
            table {
                border-spacing: 0;
                border-collapse: collapse;
                display: block;
                width: max-content;
                max-width: 100%;
                overflow: auto;
            }
            table th {
                font-weight: 600;
            }
            table th, table td {
                padding: 6px 13px;
                border: 1px solid {{FormatColor(borderColor)}};
            }
            table tr {
                background-color: {{FormatColor(background)}};
                border-top: 1px solid {{FormatColor(borderColor)}};
            }
            table tr:nth-child(2n) {
                background-color: {{FormatColor(tableRowAlt)}};
            }
            img {
                max-width: 100%;
                border-style: none;
            }
            .task-list-item {
                list-style-type: none;
            }
            .task-list-item input[type="checkbox"] {
                margin: 0 0.35em 0.25em -1.6em;
                vertical-align: middle;
            }
            dl dt {
                padding: 0;
                margin-top: 1rem;
                font-size: 1em;
                font-style: italic;
                font-weight: 600;
            }
            dl dd {
                padding: 0 1rem;
                margin-bottom: 1rem;
            }
            </style>
            """);

        if (disableScrolling)
        {
            sb.Append("""
                <script>
                document.addEventListener('wheel', function(e) {
                    e.preventDefault();
                    window.chrome.webview.postMessage(Math.round(e.deltaY));
                }, { passive: false });
                </script>
                """);
        }

        sb.Append("""
            </head>
            <body>
            """);
        sb.Append(bodyHtml);
        sb.Append("""
            </body>
            </html>
            """);

        return sb.ToString();

        static string FormatColor(Color c) => $"rgb({c.R},{c.G},{c.B})";
    }
}
