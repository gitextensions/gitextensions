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

    [Test]
    public void ParseClassifications_maps_categories_for_requested_files()
    {
        List<DiffFileContent> batch =
        [
            new("a.cs", "diff"),
            new("b.cs", "diff"),
            new("c.cs", "diff")
        ];
        string text = """
            {"files":[
              {"path":"a.cs","category":"Imports","reason":"only usings"},
              {"path":"b.cs","category":"None"},
              {"path":"c.cs","category":"StyleOnly"}
            ]}
            """;

        IReadOnlyList<DiffNoiseClassification> result = _accessor.ParseClassifications(text, batch);

        result.Should().HaveCount(3);
        result[0].Should().BeEquivalentTo(new DiffNoiseClassification("a.cs", DiffNoiseCategory.Imports, "only usings"));
        result[1].Category.Should().Be(DiffNoiseCategory.None);
        result[2].Category.Should().Be(DiffNoiseCategory.StyleOnly);
    }

    [Test]
    public void ParseClassifications_tolerates_prose_around_json()
    {
        List<DiffFileContent> batch = [new("a.cs", "diff")];
        string text = "Sure! Here is the result:\n```json\n{\"files\":[{\"path\":\"a.cs\",\"category\":\"SyncToAsync\"}]}\n```\nDone.";

        IReadOnlyList<DiffNoiseClassification> result = _accessor.ParseClassifications(text, batch);

        result.Should().ContainSingle().Which.Category.Should().Be(DiffNoiseCategory.SyncToAsync);
    }

    [Test]
    public void ParseClassifications_maps_unknown_category_to_none()
    {
        List<DiffFileContent> batch = [new("a.cs", "diff")];
        string text = """{"files":[{"path":"a.cs","category":"Refactor"}]}""";

        IReadOnlyList<DiffNoiseClassification> result = _accessor.ParseClassifications(text, batch);

        result.Should().ContainSingle().Which.Category.Should().Be(DiffNoiseCategory.None);
    }

    [Test]
    public void ParseClassifications_ignores_paths_not_in_the_batch()
    {
        List<DiffFileContent> batch = [new("a.cs", "diff")];
        string text = """{"files":[{"path":"hallucinated.cs","category":"Imports"}]}""";

        IReadOnlyList<DiffNoiseClassification> result = _accessor.ParseClassifications(text, batch);

        result.Should().BeEmpty();
    }

    [Test]
    public void ParseClassifications_omits_files_missing_from_response()
    {
        List<DiffFileContent> batch = [new("a.cs", "diff"), new("b.cs", "diff")];
        string text = """{"files":[{"path":"a.cs","category":"Imports"}]}""";

        IReadOnlyList<DiffNoiseClassification> result = _accessor.ParseClassifications(text, batch);

        result.Should().ContainSingle().Which.Path.Should().Be("a.cs");
    }

    [Test]
    public void ParseClassifications_returns_empty_for_malformed_json()
    {
        List<DiffFileContent> batch = [new("a.cs", "diff")];

        _accessor.ParseClassifications("no json here", batch).Should().BeEmpty();
        _accessor.ParseClassifications("{ not valid json }", batch).Should().BeEmpty();
    }
}
