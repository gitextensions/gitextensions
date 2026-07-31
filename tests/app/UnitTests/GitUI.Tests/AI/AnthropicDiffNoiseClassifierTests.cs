using GitUI.AI;

namespace GitUITests.AI;

[TestFixture]
public sealed class AnthropicDiffNoiseClassifierTests
{
    private static readonly AnthropicDiffNoiseClassifier.TestAccessor _accessor = AnthropicDiffNoiseClassifier.GetTestAccessor();

    [Test]
    public void ExtractResponseText_concatenates_text_blocks()
    {
        string body = """
            {
              "content": [
                { "type": "text", "text": "Hello " },
                { "type": "text", "text": "world" }
              ]
            }
            """;

        _accessor.ExtractResponseText(body).Should().Be("Hello world");
    }

    [Test]
    public void ExtractResponseText_throws_when_no_text_content()
    {
        string body = """{ "content": [] }""";

        Action act = () => _accessor.ExtractResponseText(body);

        act.Should().Throw<DiffNoiseClassifierException>();
    }
}
