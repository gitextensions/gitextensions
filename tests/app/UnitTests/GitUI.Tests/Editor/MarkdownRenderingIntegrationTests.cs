using AwesomeAssertions;
using GitUI.Editor;

namespace GitUITests.Editor;

/// <summary>
/// Tests that verify the HTML output from <see cref="MarkdownToHtmlConverter"/>
/// used for WebView2 rendering of markdown in CommitInfo and FileViewer.
/// </summary>
[TestFixture]
public class MarkdownRenderingIntegrationTests
{
    [Test]
    public void Convert_should_return_empty_for_null()
    {
        MarkdownToHtmlConverter.Convert(null!).Should().BeEmpty();
    }

    [Test]
    public void Convert_should_return_empty_for_empty_string()
    {
        MarkdownToHtmlConverter.Convert(string.Empty).Should().BeEmpty();
    }

    [Test]
    public void Convert_should_produce_html_document()
    {
        string html = MarkdownToHtmlConverter.Convert("Hello");
        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("<body>");
        html.Should().Contain("</body>");
    }

    [Test]
    public void Convert_two_item_list_should_have_li_for_each_item()
    {
        string html = MarkdownToHtmlConverter.Convert("""
            Hello

            - Yes
            - Please
            """);

        html.Should().Contain("<li>Yes</li>");
        html.Should().Contain("<li>Please</li>");
    }

    [Test]
    public void Convert_three_item_list_should_have_all_items()
    {
        string html = MarkdownToHtmlConverter.Convert("""
            - First
            - Second
            - Third
            """);

        html.Should().Contain("<li>First</li>");
        html.Should().Contain("<li>Second</li>");
        html.Should().Contain("<li>Third</li>");
    }

    [Test]
    public void Convert_nested_list_should_have_nested_ul()
    {
        string html = MarkdownToHtmlConverter.Convert("""
            - Parent
              - Child 1
              - Child 2
            """);

        html.Should().Contain("<li>Parent");
        html.Should().Contain("<li>Child 1</li>");
        html.Should().Contain("<li>Child 2</li>");
        html.Should().Contain("<ul>").And.Contain("</ul>");
    }

    [Test]
    public void Convert_paragraph_after_list_should_be_outside_ul()
    {
        string html = MarkdownToHtmlConverter.Convert("""
            - Item

            After the list
            """);

        html.Should().Contain("</ul>");
        int listEnd = html.IndexOf("</ul>");
        int afterStart = html.IndexOf("After the list");
        afterStart.Should().BeGreaterThan(listEnd, "paragraph should come after the list");
    }

    [Test]
    public void Convert_should_not_render_html_comments_as_list_items()
    {
        string html = MarkdownToHtmlConverter.Convert("""
            - Item one
            - Item two
            <!-- GitOpsUserAgent=something -->
            """);

        // HTML comments may be present in output but should not create extra <li> tags
        html.Split("<li>").Length.Should().Be(3, "should be 2 <li> tags, not 3");
    }

    [Test]
    public void Convert_consecutive_paragraphs_should_be_separate_p_tags()
    {
        string html = MarkdownToHtmlConverter.Convert("""
            First paragraph.

            Second paragraph.
            """);

        html.Should().Contain("<p>First paragraph.</p>");
        html.Should().Contain("<p>Second paragraph.</p>");
    }

    [Test]
    public void Convert_inline_code_should_use_code_tag()
    {
        string html = MarkdownToHtmlConverter.Convert("Use `code` here");
        html.Should().Contain("<code>code</code>");
    }

    [Test]
    public void Convert_fenced_code_block_should_use_pre_and_code()
    {
        string html = MarkdownToHtmlConverter.Convert("""
            ```
            code block
            ```
            """);

        html.Should().Contain("<pre><code>");
        html.Should().Contain("code block");
    }

    [Test]
    public void Convert_headings_should_produce_h_tags()
    {
        MarkdownToHtmlConverter.Convert("# H1").Should().Contain("<h1>");
        MarkdownToHtmlConverter.Convert("## H2").Should().Contain("<h2>");
        MarkdownToHtmlConverter.Convert("### H3").Should().Contain("<h3>");
    }

    [Test]
    public void Convert_bold_should_produce_strong()
    {
        MarkdownToHtmlConverter.Convert("**bold**").Should().Contain("<strong>bold</strong>");
    }

    [Test]
    public void Convert_italic_should_produce_em()
    {
        MarkdownToHtmlConverter.Convert("*italic*").Should().Contain("<em>italic</em>");
    }

    [Test]
    public void Convert_links_should_produce_a_tags()
    {
        string html = MarkdownToHtmlConverter.Convert("[text](https://example.com)");
        html.Should().Contain("<a href=\"https://example.com\">text</a>");
    }

    [Test]
    public void Convert_blockquote_should_produce_blockquote_tag()
    {
        string html = MarkdownToHtmlConverter.Convert("> quoted text");
        html.Should().Contain("<blockquote>");
        html.Should().Contain("quoted text");
    }

    [Test]
    public void Convert_thematic_break_should_produce_hr()
    {
        MarkdownToHtmlConverter.Convert("---").Should().Contain("<hr />");
    }

    [Test]
    public void Convert_list_continuation_should_stay_in_same_li()
    {
        string html = MarkdownToHtmlConverter.Convert("""
            - Replace ulong.TryParse/Utf8Parser with Convert.FromHexString for both char and byte
              overloads, which is SSSE3-vectorized
            - Second item
            """);

        // The continuation should be inside the first <li>, not a separate one
        int overloadsPos = html.IndexOf("overloads");
        int secondLiPos = html.IndexOf("<li>Second");

        overloadsPos.Should().BeLessThan(secondLiPos, "continuation should be before second <li>");

        // Only 2 <li> tags
        html.Split("<li>").Length.Should().Be(3, "should be 2 <li> tags (3 parts when split)");
    }

    [Test]
    public void Convert_css_should_include_theme_colors()
    {
        string html = MarkdownToHtmlConverter.Convert("test");
        html.Should().Contain("<style>");
        html.Should().Contain("rgb(");
        html.Should().Contain("font-family");
    }

    [Test]
    public void Convert_html_entities_should_be_escaped()
    {
        string html = MarkdownToHtmlConverter.Convert("1 < 2 & 3 > 0");
        html.Should().Contain("&lt;");
        html.Should().Contain("&amp;");
        html.Should().Contain("&gt;");
    }
}
