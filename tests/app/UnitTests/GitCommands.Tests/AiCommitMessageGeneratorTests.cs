using GitCommands;
using GitExtensions.Extensibility;

namespace GitCommandsTests;

public class AiCommitMessageGeneratorTests
{
    [Test]
    public void ExtractContent_returns_the_message_for_a_valid_response()
    {
        string json = """{"choices":[{"index":0,"message":{"role":"assistant","content":"feat: add the thing"}}]}""";

        AiCommitMessageGenerator.ExtractContent(json).Should().Be("feat: add the thing");
    }

    [Test]
    public void ExtractContent_trims_surrounding_whitespace()
    {
        string json = """{"choices":[{"message":{"content":"  hello  "}}]}""";

        AiCommitMessageGenerator.ExtractContent(json).Should().Be("hello");
    }

    [TestCase("""{"choices":[]}""", TestName = "empty choices")]
    [TestCase("""{"error":{"message":"invalid api key"}}""", TestName = "error response, no choices")]
    [TestCase("""{"choices":[{"message":{"content":null}}]}""", TestName = "null content")]
    [TestCase("""{"choices":[{"message":{"content":"   "}}]}""", TestName = "whitespace content")]
    [TestCase("""{"choices":[{"message":{}}]}""", TestName = "message without content")]
    public void ExtractContent_throws_when_there_is_no_usable_message(string json)
    {
        Action act = () => AiCommitMessageGenerator.ExtractContent(json);

        act.Should().Throw<UserExternalOperationException>();
    }
}
