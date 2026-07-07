using GitUI.AI;

namespace GitUITests.AI;

[TestFixture]
public sealed class DiffNoisePromptTests
{
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

        IReadOnlyList<DiffNoiseClassification> result = DiffNoisePrompt.ParseClassifications(text, batch);

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

        IReadOnlyList<DiffNoiseClassification> result = DiffNoisePrompt.ParseClassifications(text, batch);

        result.Should().ContainSingle().Which.Category.Should().Be(DiffNoiseCategory.SyncToAsync);
    }

    [Test]
    public void ParseClassifications_maps_unknown_category_to_none()
    {
        List<DiffFileContent> batch = [new("a.cs", "diff")];
        string text = """{"files":[{"path":"a.cs","category":"Refactor"}]}""";

        IReadOnlyList<DiffNoiseClassification> result = DiffNoisePrompt.ParseClassifications(text, batch);

        result.Should().ContainSingle().Which.Category.Should().Be(DiffNoiseCategory.None);
    }

    [Test]
    public void ParseClassifications_ignores_paths_not_in_the_batch()
    {
        List<DiffFileContent> batch = [new("a.cs", "diff")];
        string text = """{"files":[{"path":"hallucinated.cs","category":"Imports"}]}""";

        IReadOnlyList<DiffNoiseClassification> result = DiffNoisePrompt.ParseClassifications(text, batch);

        result.Should().BeEmpty();
    }

    [Test]
    public void ParseClassifications_omits_files_missing_from_response()
    {
        List<DiffFileContent> batch = [new("a.cs", "diff"), new("b.cs", "diff")];
        string text = """{"files":[{"path":"a.cs","category":"Imports"}]}""";

        IReadOnlyList<DiffNoiseClassification> result = DiffNoisePrompt.ParseClassifications(text, batch);

        result.Should().ContainSingle().Which.Path.Should().Be("a.cs");
    }

    [Test]
    public void ParseClassifications_returns_empty_for_malformed_json()
    {
        List<DiffFileContent> batch = [new("a.cs", "diff")];

        DiffNoisePrompt.ParseClassifications("no json here", batch).Should().BeEmpty();
        DiffNoisePrompt.ParseClassifications("{ not valid json }", batch).Should().BeEmpty();
    }

    [Test]
    public void CreateBatches_splits_by_file_count()
    {
        List<DiffFileContent> files = [];
        for (int i = 0; i < DiffNoisePrompt.MaxFilesPerBatch + 5; i++)
        {
            files.Add(new($"f{i}.cs", "x"));
        }

        List<List<DiffFileContent>> batches = DiffNoisePrompt.CreateBatches(files, maxCharsPerFile: 1000).ToList();

        batches.Should().HaveCount(2);
        batches[0].Should().HaveCount(DiffNoisePrompt.MaxFilesPerBatch);
        batches[1].Should().HaveCount(5);
    }

    [Test]
    public void CreateBatches_splits_by_char_budget()
    {
        // Each diff counts as maxCharsPerFile (capped), so 3 files exceed a 2-file char budget.
        int perFile = (DiffNoisePrompt.MaxCharsPerBatch / 2) + 1;
        List<DiffFileContent> files =
        [
            new("a.cs", new string('a', perFile)),
            new("b.cs", new string('b', perFile)),
            new("c.cs", new string('c', perFile))
        ];

        List<List<DiffFileContent>> batches = DiffNoisePrompt.CreateBatches(files, maxCharsPerFile: perFile).ToList();

        batches.Should().HaveCount(3);
    }

    [Test]
    public void BuildUserPrompt_only_lists_enabled_categories()
    {
        List<DiffFileContent> batch = [new("a.cs", "some diff")];
        DiffNoiseFilterOptions options = new(Imports: true, CallerSiteRename: false, SyncToAsync: false, StyleOnly: false);

        string prompt = DiffNoisePrompt.BuildUserPrompt(batch, options, maxCharsPerFile: 1000);

        prompt.Should().Contain("- Imports:");
        prompt.Should().NotContain("- StyleOnly:");
        prompt.Should().NotContain("- SyncToAsync:");
        prompt.Should().Contain("a.cs");
    }
}
